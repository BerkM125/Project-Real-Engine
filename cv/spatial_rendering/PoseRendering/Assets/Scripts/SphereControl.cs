using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereControl : MonoBehaviour
{
    public string networkData = "";
    private Vector3 initialPosition;

    // Want the listener to approve the consideration of a new vector
    public bool considerVector = false;
    // Start is called before the first frame update
    void Start()
    {
        initialPosition = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (considerVector)
        {
            //gameObject.transform.position = parseDataForVector();
        }
    }

    private Vector3 parseDataForVector()
    {
        if (networkData == "" || !networkData.Contains("right-wrist"))
        {
            //Debug.Log("network empty rn");
            considerVector = false;
            return gameObject.transform.position;
        }
        // Splitting based on colon to separate name and values
        string[] parts = networkData.Split(':');
        string[] valuesStr = parts[1].Split(',');

        // Parsing the values into doubles
        double[] values = new double[3];
        for (int i = 0; i < valuesStr.Length; i++)
        {
            values[i] = double.Parse(valuesStr[i].Trim());
        }

        Vector3 res = new Vector3((float)values[0] * 50f, (float)values[1] * 100f, (float)values[2] * 50f) + initialPosition;
        return res;
    }
}
