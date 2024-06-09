using Newtonsoft.Json;
using UserData;
using System.Collections.Generic;

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
    }
}
