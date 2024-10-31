/*
 * PackUserData.cs - Berkan Mertan
 * Script in charge of packing and unpacking the user's in-game position and action data into/from 
 * serializable objects storing user data for communication with the multiplayer server.
 */

using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using PropertyToolkit;
using UserData;
using AttackData;
using DrawAction;
using System.Security.Cryptography.X509Certificates;

public class PackUserData : MonoBehaviour
{
    public User player;
    public Attack attacks;

    public bool unpacked = false;
    public bool attacksUnpacked = false;

    public bool readyForPackaging = false;
    // Load user's hand and body kinematic structures
    GameObject [] handBuffer = new GameObject[32];
    GameObject [] bodyBuffer = new GameObject[6];

    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    private void Awake()
    {
        init();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void init()
    {
        Transform handParent = gameObject.transform.GetChild(0);
        Transform bodyParent = gameObject.transform.GetChild(1);

        for (int c = 0; c < handParent.childCount; c++)
        {
            handBuffer[c] = handParent.GetChild(c).gameObject;
        }

        for (int c = 0; c < bodyParent.childCount; c++)
        {
            bodyBuffer[c] = bodyParent.GetChild(c).gameObject;
        }
        player.kinematics = new User.Kinematics();
        player.kinematics.hand = new User.Hand();
        player.kinematics.body = new User.Body();
        readyForPackaging = true;
    }

    public string packVectorIntoString (Vector3 vec)
    {
        return vec.x + ", " + vec.y + ", " + vec.z; 
    }

    public Vector3 unpackStringIntoVector (string strVec)
    {
        string [] parts = strVec.Split(',');
        Vector3 unpackedVec = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        //Debug.Log("Unpacked vector: " + unpackedVec);
        return unpackedVec;
    }

    // Unpack all the data from this class' USERDATA variable, and update the player's data in game.
    public void unpackInfoFromSerializable ()
    {
        //Debug.Log("Unpacking now...");
        // Go through buffers, pack data into player's 3D structure
        for (int i = 0; i < handBuffer.Length; i++)
        {
            // Get hand feature's position
            Vector3 handFeaturePosition = handBuffer[i].transform.position;
            string property = handBuffer[i].name;

            // Use our custom namespace for serializable object modification through string fields
            //Debug.Log("Player hand: " + Tools.GetPropertyValue(player.kinematics.hand, property) + " Property: " + property);
            string sVec = Tools.GetPropertyValue(player.kinematics.hand, property).ToString();
            handFeaturePosition = unpackStringIntoVector(sVec);

            handBuffer[i].transform.position = handFeaturePosition;
        }

        for (int i = 0; i < bodyBuffer.Length; i++)
        {
            // Get hand feature's position
            Vector3 bodyFeaturePosition = bodyBuffer[i].transform.position;
            string property = bodyBuffer[i].name;

            // Use our custom namespace for serializable object modification through string fields
            string sVec = Tools.GetPropertyValue(player.kinematics.body, property).ToString();
            bodyFeaturePosition = unpackStringIntoVector(sVec);
            bodyBuffer[i].transform.position = bodyFeaturePosition;
        }

        gameObject.name = player.username;
        unpacked = true;
    }

    public void unpackAttacksFromSerializable ()
    {
        if (attacks.name == "beam")
        {
            StartCoroutine(DrawAttack.Beam(unpackStringIntoVector(attacks.location),
                                            unpackStringIntoVector(attacks.direction), "enemy"));
        }

        if (attacks.name == "red")
        {
            StartCoroutine(DrawAttack.Red(unpackStringIntoVector(attacks.location),
                                            unpackStringIntoVector(attacks.direction), "enemy"));
        }
        attacksUnpacked = true;
    }
    
    // Pack all the data from the user's game data into the user data object.
    public void packagePlayerIntoSerializable ()
    {
        player.username = gameObject.name;
        player.id = gameObject.name;
        // Go through buffers, pack data into player's 3D structure
        for (int i = 0; i < handBuffer.Length; i++)
        {
            // Get hand feature's position
            Vector3 handFeaturePosition = handBuffer[i].transform.position;
            string property = handBuffer[i].name;
            string stringifiedVector = packVectorIntoString(handFeaturePosition);

            // Use our custom namespace for serializable object modification through string fields
            Tools.SetPropertyValue(player.kinematics.hand, property, stringifiedVector);
        }

        for (int i = 0; i < bodyBuffer.Length; i++)
        {
            // Get hand feature's position
            Vector3 bodyFeaturePosition = bodyBuffer[i].transform.position;
            string property = bodyBuffer[i].name;
            string stringifiedVector = packVectorIntoString(bodyFeaturePosition);

            // Use our custom namespace for serializable object modification through string fields
            Tools.SetPropertyValue(player.kinematics.body, property, stringifiedVector);
        }
    }

    public void packageAttacksIntoSerializable (string playerName, string name, Vector3 direction, Vector3 location)
    {
        attacks.processed = name + "-" + playerName + "-" + Random.Range(0, 100000f);
        attacks.name = name;
        attacks.direction = packVectorIntoString (direction);
        attacks.location = packVectorIntoString (location);
    }
}
