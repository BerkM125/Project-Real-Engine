using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SkeletalMover : MonoBehaviour
{
    // Bone mapping for avatar
    private Dictionary<string, string> boneMap = new Dictionary<string, string>();
    private Dictionary<string, Vector3> initialBoneLocations = new Dictionary<string, Vector3>();
    private Dictionary<string, List<string>> boneConnections = new Dictionary<string, List<string>>();
    private string[] handDigits =
    {
        "thumb",
        "index",
        "middle",
        "ring",
        "pinkie"
    };
    

    // If parts not mapped yet, do not proceed with avatar part transform yet.
    private bool partsMapped = false;
    private uint bIndex = 0;
    private uint pIndex = 0;
    public GameObject cam;

    // Network received data
    public string networkData = "";
    private string part = "EMPTY_PART";
    private const float JOINT_SCALING_FACTOR = 40.0f;
    private const float JOINT_TRANSLATION_FACTOR = 60.0f;
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
            "left-thumb-third",

            "right-shoulder",
            "left-shoulder",
            "right-elbow",
            "left-elbow",
            "right-hip",
            "left-hip"

        };

    GameObject[] givenCylinders = new GameObject[40];
    GameObject[] givenBodyCylinders = new GameObject[20];

    // Callback template function to a BodyPartHandler.
    private delegate bool BodyPartHandler(string bodyPart, Vector3 socketDataVector);

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
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        // All of this is bone connections between bones / the joints
        {
            // For right hand connections
            connectDigits("right");
            connectDigits("left");
            connectBodyParts();
        }
        // Create a bunch just make sure as many cylinders as you need are available, doesnt really matter
        int cN = 0;
        for(cN = 0; cN < 40; cN++)
        {
            givenCylinders[cN] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        }

        for (cN = 0; cN < 20; cN++)
        {
            givenBodyCylinders[cN] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        }
        partsMapped = true;
    }

    // Update is called once per frame
    void Update()
    {
        parseNetworkData(SimpleBodyRenderer, SimpleHandRenderer);
    }
    
    // Links body parts, joints to one another
    private void linkJointsByKeyValue (string key, string value)
    {
        // If no key exists make sure to instantiate a List and avoid 
        // key errors
        if (!boneConnections.ContainsKey(key))  
            boneConnections[key] = new List<string> { value }; 
        else 
            boneConnections[key].Add(value);
    }

    // Map digit connections 
    private void connectDigits (string handedness)
    {
        for (int n = 0; n < 5; n++)
        {
            string fingerType = handDigits[n];
            linkJointsByKeyValue(handedness + "-wrist", handedness + "-" + fingerType + "-first");
            linkJointsByKeyValue(handedness + "-" + fingerType + "-first", handedness + "-" + fingerType + "-second");
            linkJointsByKeyValue(handedness + "-" + fingerType + "-second", handedness + "-" + fingerType + "-first");
            linkJointsByKeyValue(handedness + "-" + fingerType + "-third", handedness + "-" + fingerType + "-second");
        }
    }

    // Map large body connections
    private void connectBodyParts ()
    {
        linkJointsByKeyValue("right-shoulder", "right-elbow");
        linkJointsByKeyValue("right-shoulder", "left-shoulder");
        linkJointsByKeyValue("right-elbow", "left-wrist");
        linkJointsByKeyValue("left-elbow", "right-wrist");
        linkJointsByKeyValue("left-shoulder", "left-elbow");
        linkJointsByKeyValue("right-hip", "right-shoulder");
        linkJointsByKeyValue("left-hip", "left-shoulder");
        linkJointsByKeyValue("right-hip", "left-hip");
    }

    // Hand rendering function, upon network data reception this function should be invoked by data parser
    private bool SimpleHandRenderer(string bodyPart, Vector3 socketDataVector)
    {
        float x = socketDataVector.x;
        float y = socketDataVector.y;
        float z = socketDataVector.z;

        if (nonAvatarPartSupport.Contains(bodyPart))
        {
            GameObject currJoint = GameObject.Find(bodyPart);
            int cjIndex = 0;
            while (cjIndex < boneConnections[bodyPart].Count)
            {
                if (bIndex >= 40)
                {
                    bIndex = 0;
                }
                GameObject connectedJoint = GameObject.Find(boneConnections[bodyPart][cjIndex]);

                if (currJoint != null)
                {
                    currJoint.transform.position = new Vector3(x * JOINT_SCALING_FACTOR,
                                (-y * JOINT_SCALING_FACTOR) + JOINT_TRANSLATION_FACTOR,
                                z * -JOINT_SCALING_FACTOR);
                    GameObject bone = givenCylinders[bIndex];
                    UpdateCylinderPosition(bone, currJoint.transform.position, connectedJoint.transform.position);
   
                }
                cjIndex++;
                bIndex++;
            }
            
        }
        else
        {
            return false;
        }
        return true;
    }

    // BODY PART rendering
    private bool SimpleBodyRenderer(string bodyPart, Vector3 socketDataVector)
    {
        float x = socketDataVector.x;
        float y = socketDataVector.y;
        float z = socketDataVector.z;

        if (nonAvatarPartSupport.Contains(bodyPart))
        {
            GameObject currJoint = GameObject.Find(bodyPart);

            int cjIndex = 0;
            while (cjIndex < boneConnections[bodyPart].Count)
            {
                if (pIndex == 10)
                {
                    pIndex = 0;
                }
                GameObject connectedJoint = GameObject.Find(boneConnections[bodyPart][cjIndex]);

                if (currJoint != null)
                {
                    float cZ = (bodyPart.Contains("elbow")) ? socketDataVector.z : currJoint.transform.position.z;
                    currJoint.transform.position = new Vector3(x * JOINT_SCALING_FACTOR,
                                (-y * JOINT_SCALING_FACTOR) + JOINT_TRANSLATION_FACTOR,
                                cZ);

                    GameObject bone = givenBodyCylinders[pIndex];

                    UpdateCylinderPosition(bone, currJoint.transform.position, connectedJoint.transform.position);

                    if (bodyPart == "right-shoulder" && boneConnections[bodyPart][cjIndex] == "left-shoulder")
                    {
                        cam.transform.position = bone.transform.position;
                    }
                    
                }
                pIndex++;
                cjIndex++;

            }
        }
        else
        {
            return false;
        }
        return true;
    }

    // AVATAR body rendering function
    private bool AvatarBodyRenderer(string bodyPart, Vector3 socketDataVector)
    {
        float x = socketDataVector.x;
        float y = socketDataVector.y;
        float z = socketDataVector.z;

        SimpleBodyRenderer(bodyPart, socketDataVector);
        return true;
    }

    // AVATAR hand rendering function, upon network data reception this function should be invoked by data parser
    private bool AvatarHandRenderer(string bodyPart, Vector3 socketDataVector)
    {
        SimpleHandRenderer(bodyPart, socketDataVector);
        return true;
    }
    
    // Simple hand skeleton rendering function
    private bool SimpleSkeletalRenderer()
    {
        return true;
    }
    // Parse the network data stored inside this script's network data string
    private void parseNetworkData (BodyPartHandler partHandlerCallback, BodyPartHandler handCallback)
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

            if ( !(bodyPart.Contains("shoulder") || bodyPart.Contains("elbow") || bodyPart.Contains("hip")))
            {
                if (!handCallback(bodyPart, new Vector3(x, y, z)))
                    Debug.Log("Something went wrong with the Part Handler Callback, returning... ");
            }
            else
            {
                if (!partHandlerCallback(bodyPart, new Vector3(x, y, z)))
                    Debug.Log("Something went wrong with the Part Handler Callback, returning... ");
            }
        }
    }

    // Update position of cylinder and render it between two points so as to connect them
    private void UpdateCylinderPosition(GameObject cylinder, Vector3 beginPoint, Vector3 endPoint)
    {
        Vector3 pos = Vector3.Lerp(beginPoint, endPoint, 0.5f);

        cylinder.transform.localScale = new Vector3(0.77f,
                                            Vector3.Distance(beginPoint, endPoint) * 0.5f,
                                            0.77f);
        cylinder.transform.position = pos;
        cylinder.transform.up = endPoint - beginPoint;
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
