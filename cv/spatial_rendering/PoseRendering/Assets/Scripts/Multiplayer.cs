using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json;
using System;
using UserData;

public class Multiplayer : MonoBehaviour
{
    public SocketIOUnity socket;
    public bool emitted = false;
    public GameObject [] gamePlayers;
    // Start is called before the first frame update
    void Start()
    {
        gamePlayers = GameObject.FindGameObjectsWithTag("Player");
        

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
            Debug.Log("socket.OnConnected");

            // Join the default room, VONK, for now
            socket.Emit("join-room", "VONK");
            
            // Retrieve game data just for funzies
            socket.Emit("retrieve-game-data", "VONK");
        };

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            socket.DisconnectAsync();
        };

        socket.On("send-game-data", (response) =>
        {
            Debug.Log("Game data was sent: " + response);
            string wR = response.ToString();

            {
                Debug.Log("Starting unpacking...");
                string jsonString = wR[1..^1];

                Debug.Log("Main character name: " + jsonString);

                gamePlayers[0].GetComponent<PackUserData>().player = JsonConvert.DeserializeObject<User>(jsonString);
                Debug.Log("Player currently: " + gamePlayers[0].GetComponent<PackUserData>().player.kinematics.hand.leftthumbFirst);
                gamePlayers[0].GetComponent<PackUserData>().unpackInfoFromSerializable();

                Debug.Log("Unpacking and serializing should be finished");
            }
        });

        socket.On("send-general-signal", (response) =>
        {
            Debug.Log("SIGNAL: " + response);
        });
        ////

        Debug.Log("Connecting...");
        socket.Connect();
    }

    public void EmitClass()
    {
        socket.Emit("pingKinematicInfo", GameObject.Find("MainCharacter").GetComponent<PackUserData>().player);
    }
    // Update is called once per frame
    void Update()
    {
    }
}
