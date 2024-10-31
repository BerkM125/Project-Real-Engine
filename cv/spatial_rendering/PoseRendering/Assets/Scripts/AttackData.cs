/*
 * AttackData.cs - Berkan Mertan
 * Serializable C# class used to store attack data, matching the JSON schema used to store attack info on DB.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace AttackData
{
    [System.Serializable]
    public class Attack
    {
        [JsonProperty("type")]
        public string name { get; set; }

        [JsonProperty("location")]
        public string location { get; set; }
        [JsonProperty("direction")]

        public string direction { get; set; }
        [JsonProperty("processed")]
        public string processed { get; set; }
    }
}
