using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Lobby : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGameStart(TMP_InputField userField)
    {
        PlayerPrefs.SetString("username", userField.text);
        SceneManager.LoadScene("MainScene");
    }
}
