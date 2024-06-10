using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace AttackData
{
    [System.Serializable]
    public class Attack
    {
        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("location")]
        public string location { get; set; }
    }
}
