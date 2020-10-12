using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;

    public GameObject start_Menu;
    public InputField username_Field;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Another instance exists");
            Destroy(this);
        }
    }

    public void Connect_To_Server()
    {
        start_Menu.SetActive(false);
        username_Field.interactable = false;
        Client.instance.Connect_To_Server();
    }
}
