/*
 * Multiplayer.cs - Berkan Mertan
 * This script handles all of the communication between the clientside gameplay and the
 * multiplayer server. Data flow of multiplayer info is described below:
 * 
    // Check if all player data from the server has been unpacked first, and then package back into a serializable 
    // for recommunication with the server if so.

    // Custom network pipeline is:
    // 1) Load data from server into EVERY client's game data
    // 2) Allow for gameplay, updating of client-side gameobjects
    // 3) Package each client-side player into socket-deliverable JSON data
    // 4) Ping server with JSON data, restart loop

 */

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
    public PoseTrackingBridge bridgeHandler;

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
        MAINCHARACTER.name = PlayerPrefs.GetString("username");

        MAINCHARACTER.AddComponent<SkeletalMover>();
        bridgeHandler.avatar = MAINCHARACTER.GetComponent<SkeletalMover>();

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
                    StartCoroutine(UpdateDBKinematicsRepeatedly(0.01f, socket));
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

    // Load all room data from the DB into game
    private void RefreshDataFromDB(SocketIOResponse response)
    {
        string wR = response.ToString();
        wR = wR[1..^1];

        // Queue up the room and user deserialization on the Unity client end 
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
