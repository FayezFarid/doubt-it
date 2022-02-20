using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace PhotonLobby { 
public class StartMenu : MonoBehaviour
{
    public GameObject RoomMenu;
    public GameObject Connecting;
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            RoomMenu.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
       if(Input.anyKeyDown)
        {
            Connect();
        }
    }
    public void Connect() { Connecting.SetActive(true) ; NetWorkConnecter.Instance.Setup(); gameObject.SetActive(false); }
}
}
