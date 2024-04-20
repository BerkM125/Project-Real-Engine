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

    // Start is called before the first frame update
    void Start()
    {
        boneMap["right-shoulder"] = "Bone.029";
        boneMap["right-elbow"] = "Bone.030";
        boneMap["left-shoulder"] = "Bone.006";
        boneMap["left-elbow"] = "Bone.007";
        boneMap["left-wrist"] = "Bone.031";
        boneMap["right-wrist"] = "Bone.008";

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

        foreach (KeyValuePair<string, string> entry in boneMap)
        {
            initialBoneLocations[entry.Key] = getBone(entry.Key).transform.position;
        }
        partsMapped = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!partsMapped) return;
        Vector3 data;
        string part = "SHIT";
        data = parseDataForVector(out part);
        if (part != "SHIT")
            placeBone(part, data);

        //shiftBone("left-shoulder", new Vector3(-0.005f, 0.005f, 0f));
    }

    // Parse incoming network stream for three vector values
    private Vector3 parseDataForVector (out string partString)
    {
        if (networkData == "")
        {
            //Debug.Log("network empty rn");
            partString = "SHIT";
            return new Vector3(0.0f, 0.0f, 0.0f);
        }
        // Splitting based on colon to separate name and values
        string[] parts = networkData.Split(':');

        // Extracting name
        string name = parts[0].Trim();
        partString = name;
        // Splitting the values based on comma
        string[] valuesStr = parts[1].Split(',');

        // Parsing the values into doubles
        double[] values = new double[3];
        for (int i = 0; i < valuesStr.Length; i++)
        {
            values[i] = double.Parse(valuesStr[i].Trim());
        }

        Vector3 res = new Vector3((float)values[0], (float)values[1], (float)values[2]) + initialBoneLocations[name];
        return res;
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
