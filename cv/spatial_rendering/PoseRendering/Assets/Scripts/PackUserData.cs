using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackUserData : MonoBehaviour
{
    UserData player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void packagePlayerIntoSerializable ()
    {
        GameObject [] handBuffer = gameObject.transform.GetChild(0).GetComponentsInChildren<GameObject>();
        GameObject[] bodyBuffer = gameObject.transform.GetChild(1).GetComponentsInChildren<GameObject>();
        for (int i = 0; i < handBuffer.Length; i++)
        {
            
        }
    }
}
