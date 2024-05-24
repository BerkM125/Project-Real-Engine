using System.Collections;
using UnityEngine;

[System.Serializable]
public class UserData {
    public string username;
    public string id;
    public int xp;
    public int hp;
    public Kinematics kinematics;

    [System.Serializable]
    public class Kinematics
    {
        public Hand hand;
        public Body body;
    }

    [System.Serializable]
    public class Hand
    {
        public Vector3 wrist;
        public Vector3 thumbFirst;
        public Vector3 thumbSecond;
        public Vector3 thumbThird;
        public Vector3 indexFirst;
        public Vector3 indexSecond;
        public Vector3 indexThird;
        public Vector3 middleFirst;
        public Vector3 middleSecond;
        public Vector3 middleThird;
        public Vector3 ringFirst;
        public Vector3 ringSecond;
        public Vector3 ringThird;
        public Vector3 pinkieFirst;
        public Vector3 pinkieSecond;
        public Vector3 pinkieThird;
    }

    [System.Serializable]
    public class Body
    {
        public Vector3 rightShoulder;
        public Vector3 leftShoulder;
        public Vector3 rightElbow;
        public Vector3 leftElbow;
        public Vector3 rightHip;
        public Vector3 leftHip;
    }
}
