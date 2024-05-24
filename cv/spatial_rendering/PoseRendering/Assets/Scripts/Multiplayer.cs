using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using System;

public class Multiplayer : MonoBehaviour
{
    public SocketIOUnity socket;
    public bool emitted = false;
    // Start is called before the first frame update
    void Start()
    {
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
        TestClass testClass = new TestClass(new string[] { "foo", "bar", "baz", "qux" });
        TestClass2 testClass2 = new TestClass2("lorem ipsum");
        socket.Emit("class", testClass2);
    }

    // our test class
    [System.Serializable]
    class TestClass
    {
        public string[] arr;

        public TestClass(string[] arr)
        {
            this.arr = arr;
        }
    }

    [System.Serializable]
    class TestClass2
    {
        public string text;

        public TestClass2(string text)
        {
            this.text = text;
        }
    }
    // Update is called once per frame
    void Update()
    {


    }
}
