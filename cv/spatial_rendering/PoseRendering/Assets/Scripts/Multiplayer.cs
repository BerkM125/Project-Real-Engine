using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json;
using System;
using UserData;
using ClientRoom;
using System.Collections;

public class Multiplayer : MonoBehaviour
{
    // FILL THIS FIELD IN UNITY EDITOR!!! WITH THE PLAYER PREFAB
    public GameObject playerPrefab;
    public GameObject MAINCHARACTER;
    public string NEWNAME;
    // Private vars
    Room room;
    public SocketIOUnity socket;
    HashSet<string> presentUsers = new HashSet<string>();
    Queue<Action> UNITYTHREADQUEUE = new Queue<Action>();

    // Start is called before the first frame update
    void Start()
    {
        Debug.developerConsoleEnabled = true;
        Debug.developerConsoleVisible = true;
        // Initialize the MAIN CHARACTER
        MAINCHARACTER = GameObject.Find(NEWNAME);
        MAINCHARACTER.name = PlayerPrefs.GetString("username");
        NEWNAME = MAINCHARACTER.name;

        // Initialize socket system
        var uri = new Uri("http://127.0.0.1:3000");
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                {"token", "UNITY" }
            }
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        ///// reserved socketio events
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Socket has connected with multiplayer server.");
            
            // Join the default room, VONK, for now
            socket.Emit("join-room", "VONK");

            lock (UNITYTHREADQUEUE)
            {
                UNITYTHREADQUEUE.Enqueue(() =>
                {
                    StartCoroutine(UpdateDBKinematicsRepeatedly(2.0f, socket));
                    CopyNewUserToDB(socket);
                    
                });
            }
        };

        socket.OnDisconnected += (sender, e) =>
        {
            socket.Emit("delete-user", "VONK", MAINCHARACTER.name);
        };

        socket.On("refresh-data-from-db", (response) =>
        {
            RefreshDataFromDB(response);
        });

        Debug.Log("Connecting...");
        socket.Connect();
        

    }

    private IEnumerator UpdateDBKinematicsRepeatedly(float interval, SocketIOUnity socket)
    {
        while (true)
        {
            lock (UNITYTHREADQUEUE)
            {
                UNITYTHREADQUEUE.Enqueue(() =>
                {
                    SetKinematicsToDB(socket);
                });
            }
            yield return new WaitForSeconds(interval);
        }
    }
    // 
    private void RefreshDataFromDB(SocketIOResponse response)
    {
        string wR = response.ToString();
        wR = wR[1..^1];

        lock (UNITYTHREADQUEUE)
        {
            UNITYTHREADQUEUE.Enqueue(() =>
            {
                room = JsonConvert.DeserializeObject<Room>(wR);
                foreach (var userEntry in room.users)
                {
                    if (userEntry.Key == MAINCHARACTER.name) continue;

                    GameObject specifiedPlayer = GameObject.Find(userEntry.Key);

                    if (!presentUsers.Contains(userEntry.Key))
                    {
                        GameObject newPlayer = Instantiate(playerPrefab);
                        specifiedPlayer = newPlayer;
                    }
                    presentUsers.Add(userEntry.Key);

                    PackUserData newUserPacking = specifiedPlayer.GetComponent<PackUserData>();
                    newUserPacking.player = userEntry.Value;

                    newUserPacking.unpackInfoFromSerializable();
                    newUserPacking.unpacked = true;
                }
            });
        }
    }

    // Check if all player data from the server has been unpacked first, and then package back into a serializable 
    // for recommunication with the server if so.

    // Pipeline is:
    // 1) Load data from server into EVERY client's game data
    // 2) Allow for gameplay, updating of client-side gameobjects
    // 3) Package each client-side player into socket-deliverable JSON data
    // 4) Ping server with JSON data, restart loop

    // Modify kinematic data from DB
    public void SetKinematicsToDB(SocketIOUnity socket)
    {
        PackUserData userPacker = MAINCHARACTER.GetComponent<PackUserData>();

        if (!userPacker.readyForPackaging) return;

        userPacker.packagePlayerIntoSerializable();
        // Serialize player's content
        var serializedContent = JsonConvert.SerializeObject(userPacker.player);
        // Ping 
        socket.Emit("ping-kinematic-info", "VONK", MAINCHARACTER.name, serializedContent);
    }

    // Add new user through socket
    public void CopyNewUserToDB(SocketIOUnity socket)
    {
        PackUserData userPacker = MAINCHARACTER.GetComponent<PackUserData>();

        if (!userPacker.readyForPackaging) return;

        userPacker.packagePlayerIntoSerializable();
        // Serialize player's content
        var serializedContent = JsonConvert.SerializeObject(userPacker.player);
        socket.Emit("add-new-user", "VONK", MAINCHARACTER.name, serializedContent);
    }

    public void EraseUserFromDB(SocketIOUnity socket)
    {
        socket.Emit("delete-user", "VONK", MAINCHARACTER.name);
    }

    // This includes the clientside main character's kinematics, AND the objects they own
    // Each action object will have a originUser field where the username of the action's creator is listed
    // Each character themselves will hav ea username.
    private void EmitPlayerData()
    {

    }

    // Update is called once per frame
    void Update()
    {
        lock (UNITYTHREADQUEUE)
        {
            while (UNITYTHREADQUEUE.Count > 0)
            {
                UNITYTHREADQUEUE.Dequeue()();
            }
        }
        //PreparePlayerDataForPipeline();
    }

    private void OnApplicationQuit()
    {
        Debug.Log("App was QUIT");
        socket.Emit("delete-user", "VONK", MAINCHARACTER.name);
        socket.Dispose();
    }

    private void OnApplicationPause(bool pause)
    {
        //socket.Dispose();
    }
}
