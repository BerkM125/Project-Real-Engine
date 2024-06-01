using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using PropertyToolkit;
using UserData;

public class PackUserData : MonoBehaviour
{
    public User player;
    // Load user's hand and body kinematic structures
    GameObject [] handBuffer = new GameObject[32];
    GameObject [] bodyBuffer = new GameObject[12];

    // Start is called before the first frame update
    void Start()
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
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Perform a simple get request on a coroutine
    public IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:

                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;

                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;

                case UnityWebRequest.Result.Success:

                    string wR = webRequest.downloadHandler.text;

                    if (wR.Contains("kinematics"))
                    {
                        string jsonString = webRequest.downloadHandler.text;
                        player = JsonConvert.DeserializeObject<User>(jsonString);

                        unpackInfoFromSerializable();
                    }
                    break;
            }
        }
    }

    public string packVectorIntoString (Vector3 vec)
    {
        return vec.x + ", " + vec.y + ", " + vec.z; 
    }

    public Vector3 unpackStringIntoVector (string strVec)
    {
        string [] parts = strVec.Split(',');
        Vector3 unpackedVec = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        Debug.Log("Unpacked vector: " + unpackedVec);
        return unpackedVec;
    }

    // Unpack all the data from this class' USERDATA variable, and update the player's data in game.
    public void unpackInfoFromSerializable ()
    {
        Debug.Log("Unpacking now...");
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
        }

        for (int i = 0; i < bodyBuffer.Length; i++)
        {
            // Get hand feature's position
            Vector3 bodyFeaturePosition = bodyBuffer[i].transform.position;
            string property = bodyBuffer[i].name;

            // Use our custom namespace for serializable object modification through string fields
            string sVec = Tools.GetPropertyValue(player.kinematics.body, property).ToString();
            bodyFeaturePosition = unpackStringIntoVector(sVec);
        }
    }
    
    // Pack all the data from the user's game data into the user data object.
    public void packagePlayerIntoSerializable ()
    {
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
}
