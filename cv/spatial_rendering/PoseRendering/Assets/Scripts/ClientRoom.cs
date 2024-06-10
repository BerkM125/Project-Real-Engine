using System.Collections.Generic;
using Newtonsoft.Json;
using UserData;
using AttackData;

namespace ClientRoom {
    [System.Serializable]
    public class Room
    {
        [JsonProperty("_id")]
        public string _id { get; set; }

        [JsonProperty("roomId")]
        public string roomID {  get; set; }

        [JsonProperty("users")]
        public Dictionary<string, User> users { get; set; }

        [JsonProperty("attacks")]
        public Dictionary<string, Attack> attacks { get; set; }
    }
}
