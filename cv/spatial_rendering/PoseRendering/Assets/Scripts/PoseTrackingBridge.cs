/*
 * PoseTrackingBridge.cs - Berkan Mertan
 * Script in charge of listening to the client-side pose estimation server for data, and relaying it
 * to network data variables in SkeletalMover instances.
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

public class PoseTrackingBridge : MonoBehaviour
{
    public string globalDataRecieved = "";
    public SkeletalMover avatar;

    SocketIOUnity socket;

    void Start()
    {
        var uri = new Uri("http://127.0.0.1:8000");
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

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Socket has connected with tracking server.");
        };

        socket.On("post-tracking-update", (response) =>
        {
            string dataReceived = response.ToString();
            dataReceived = dataReceived[2..^2];
            globalDataRecieved = dataReceived;
            avatar.networkData = globalDataRecieved;
        });

        socket.Connect();
    }


    void Update()
    {

    }

    private void OnApplicationQuit()
    {
        socket.Dispose();
    }
}
