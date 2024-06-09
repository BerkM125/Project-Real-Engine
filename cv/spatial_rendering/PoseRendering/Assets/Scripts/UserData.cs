// USER DATA NAMESPACE AND CLASS: this class is used ENTIRELY for the web interface between
// multiplayer server and client side gameplay. This is where multiplayer server data
// is loaded and then unpacked into appropriate GameObjects, and in-game data is packed
// back into to update server data.

// This file is also a sort of "schema" for the format by which user data is stored

using Newtonsoft.Json;
using System.Collections.Generic;

namespace UserData
{
    [System.Serializable]
    public class User
    {
        [JsonProperty("username")]
        public string username { get; set; }
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("xp")]
        public int xp { get; set; }
        [JsonProperty("hp")]
        public int hp { get; set; }
        [JsonProperty("kinematics")]
        public Kinematics kinematics { get; set; }

        [System.Serializable]
        public class Kinematics
        {
            [JsonProperty("hand")]
            public Hand hand { get; set; }
            [JsonProperty("body")]
            public Body body { get; set; }
        }

        [System.Serializable]
        public class Hand
        {
            /// <summary>
            ///  LEFT HAND FEATURES
            /// </summary>

            [JsonProperty("left-wrist")]
            public string leftwrist { get; set; }

            [JsonProperty("left-thumb-first")]
            public string leftthumbFirst { get; set; }

            [JsonProperty("left-thumb-second")]
            public string leftthumbSecond { get; set; }

            [JsonProperty("left-thumb-third")]
            public string leftthumbThird { get; set; }

            [JsonProperty("left-index-first")]
            public string leftindexFirst { get; set; }

            [JsonProperty("left-index-second")]
            public string leftindexSecond { get; set; }

            [JsonProperty("left-index-third")]
            public string leftindexThird { get; set; }

            [JsonProperty("left-middle-first")]
            public string leftmiddleFirst { get; set; }

            [JsonProperty("left-middle-second")]
            public string leftmiddleSecond { get; set; }

            [JsonProperty("left-middle-third")]
            public string leftmiddleThird { get; set; }

            [JsonProperty("left-ring-first")]
            public string leftringFirst { get; set; }

            [JsonProperty("left-ring-second")]
            public string leftringSecond { get; set; }

            [JsonProperty("left-ring-third")]
            public string leftringThird { get; set; }

            [JsonProperty("left-pinkie-first")]
            public string leftpinkieFirst { get; set; }

            [JsonProperty("left-pinkie-second")]
            public string leftpinkieSecond { get; set; }

            [JsonProperty("left-pinkie-third")]
            public string leftpinkieThird { get; set; }


            /// <summary>
            ///  RIGHT HAND FEATURES
            /// </summary>

            [JsonProperty("right-wrist")]
            public string rightwrist { get; set; }

            [JsonProperty("right-thumb-first")]
            public string rightthumbFirst { get; set; }

            [JsonProperty("right-thumb-second")]
            public string rightthumbSecond { get; set; }

            [JsonProperty("right-thumb-third")]
            public string rightthumbThird { get; set; }

            [JsonProperty("right-index-first")]
            public string rightindexFirst { get; set; }

            [JsonProperty("right-index-second")]
            public string rightindexSecond { get; set; }

            [JsonProperty("right-index-third")]
            public string rightindexThird { get; set; }

            [JsonProperty("right-middle-first")]
            public string rightmiddleFirst { get; set; }

            [JsonProperty("right-middle-second")]
            public string rightmiddleSecond { get; set; }

            [JsonProperty("right-middle-third")]
            public string rightmiddleThird { get; set; }

            [JsonProperty("right-ring-first")]
            public string rightringFirst { get; set; }

            [JsonProperty("right-ring-second")]
            public string rightringSecond { get; set; }

            [JsonProperty("right-ring-third")]
            public string rightringThird { get; set; }

            [JsonProperty("right-pinkie-first")]
            public string rightpinkieFirst { get; set; }

            [JsonProperty("right-pinkie-second")]
            public string rightpinkieSecond { get; set; }

            [JsonProperty("right-pinkie-third")]
            public string rightpinkieThird { get; set; }
        }

        [System.Serializable]
        public class Body
        {
            [JsonProperty("right-shoulder")]
            public string rightShoulder { get; set; }

            [JsonProperty("left-shoulder")]
            public string leftShoulder { get; set; }

            [JsonProperty("right-elbow")]
            public string rightElbow { get; set; }

            [JsonProperty("left-elbow")]
            public string leftElbow { get; set; }

            [JsonProperty("right-hip")]
            public string rightHip { get; set; }

            [JsonProperty("left-hip")]
            public string leftHip { get; set; }
        }
    }
}