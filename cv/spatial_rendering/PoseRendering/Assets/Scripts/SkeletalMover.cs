using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SkeletalMover : MonoBehaviour
{
    private Dictionary<string, string> boneMap = new Dictionary<string, string>();
    private Dictionary<string, Vector3> initialBoneLocations = new Dictionary<string, Vector3>();
    private bool partsMapped = false;

    // Network received data
    public string networkData = "";
    private string part = "EMPTY_PART";
    private HashSet<string> nonAvatarPartSupport = new HashSet<string>
        {
            "right-wrist",
            "right-index-first",
            "right-middle-first",
            "right-ring-first",
            "right-pinkie-first",
            "right-thumb-first",
            "left-wrist",
            "left-index-first",
            "left-middle-first",
            "left-ring-first",
            "left-pinkie-first",
            "left-thumb-first",

            "right-index-second",
            "right-middle-second",
            "right-ring-second",
            "right-pinkie-second",
            "right-thumb-second",
            "left-index-second",
            "left-middle-second",
            "left-ring-second",
            "left-pinkie-second",
            "left-thumb-second",

            "right-index-third",
            "right-middle-third",
            "right-ring-third",
            "right-pinkie-third",
            "right-thumb-third",
            "left-index-third",
            "left-middle-third",
            "left-ring-third",
            "left-pinkie-third",
            "left-thumb-third"
        };

    // Start is called before the first frame update
    void Start()
    {
        // All of this is simply mapping to one avatar model
        {
            boneMap["right-shoulder"] = "Bone.029";
            boneMap["right-elbow"] = "Bone.030";
            boneMap["left-shoulder"] = "Bone.006";
            boneMap["left-elbow"] = "Bone.007";
            boneMap["right-wrist"] = "Bone.031";
            boneMap["left-wrist"] = "Bone.008";

            // HANDS
            // Left
            boneMap["left-middle-first"] = "Bone.009";
            boneMap["left-middle-second"] = "Bone.010";
            boneMap["left-middle-third"] = "Bone.011";

            boneMap["left-index-first"] = "Bone.017";
            boneMap["left-index-second"] = "Bone.018";
            boneMap["left-index-third"] = "Bone.019";

            boneMap["left-thumb-first"] = "Bone.013";
            boneMap["left-thumb-second"] = "Bone.014";
            boneMap["left-thumb-third"] = "Bone.015";

            boneMap["left-ring-first"] = "Bone.021";
            boneMap["left-ring-second"] = "Bone.022";
            boneMap["left-ring-third"] = "Bone.023";

            boneMap["left-pinkie-first"] = "Bone.025";
            boneMap["left-pinkie-second"] = "Bone.026";
            boneMap["left-pinkie-third"] = "Bone.027";

            // Right
            boneMap["right-middle-first"] = "Bone.032";
            boneMap["right-middle-second"] = "Bone.033";
            boneMap["right-middle-third"] = "Bone.034";

            boneMap["right-index-first"] = "Bone.040";
            boneMap["right-index-second"] = "Bone.041";
            boneMap["right-index-third"] = "Bone.042";

            boneMap["right-thumb-first"] = "Bone.036";
            boneMap["right-thumb-second"] = "Bone.037";
            boneMap["right-thumb-third"] = "Bone.038";

            boneMap["right-ring-first"] = "Bone.044";
            boneMap["right-ring-second"] = "Bone.045";
            boneMap["right-ring-third"] = "Bone.046";

            boneMap["right-pinkie-first"] = "Bone.048";
            boneMap["right-pinkie-second"] = "Bone.049";
            boneMap["right-pinkie-third"] = "Bone.050";
        }
        foreach (KeyValuePair<string, string> entry in boneMap)
        {
            initialBoneLocations[entry.Key] = getBone(entry.Key).transform.position;
        }
        partsMapped = true;
    }

    // Update is called once per frame
    void Update()
    {
        parseNetworkData();
    }

    private void parseNetworkData ()
    {
        if (networkData == "")
            return;

        // Split the input string by '|' character
        string[] parcels = networkData.Split('|');

        foreach (string parcel in parcels)
        {
            if (!parcel.Contains(":")) break;

            // Split each parcel by ':' character to separate body part and coordinates
            string[] parts = parcel.Split(':');

            // Trim any leading or trailing white spaces
            string bodyPart = parts[0].Trim();
            string coordinates = parts[1].Trim();

            string[] xyz = coordinates.Split(',');

            // Extract X, Y, and Z coordinates
            float x = float.Parse(xyz[0].Trim());
            float y = float.Parse(xyz[1].Trim());
            float z = float.Parse(xyz[2].Trim());

            if (nonAvatarPartSupport.Contains(bodyPart))
            {
                GameObject currJoint = GameObject.Find(bodyPart);
                if (currJoint != null)
                {
                    currJoint.transform.position = new Vector3(x * 20, (-y * 20) + 10, z * 20);
                }
            }

        }
    }
    // Get bone from name of bone
    private GameObject getBone(string part)
    {
        if (!boneMap.ContainsKey(part))
        {
            Debug.Log("Body part does not exist or does not have a mapping.");
            return null;
        }

        GameObject bodyPart = GameObject.Find(boneMap[part]);
        return bodyPart;
    }
    // Part for word of the body part, Vector3 representing increment in each direction
    private void shiftBone(string part, Vector3 increment)
    {
        GameObject bp = getBone(part);
        bp.transform.position = new Vector3(bp.transform.position.x + increment.x,
                                                    bp.transform.position.y + increment.y,
                                                    bp.transform.position.z + increment.z);
    }

    // Part for word of the body part, Vector3 representing increment in each direction
    private void placeBone(string part, Vector3 newPos)
    {
        GameObject bp = getBone(part);
        bp.transform.position = newPos;
    }
}
