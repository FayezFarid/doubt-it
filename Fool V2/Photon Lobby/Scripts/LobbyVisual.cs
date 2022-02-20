using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace PhotonLobby.Example
{
    public class LobbyVisual : MonoBehaviour
    {
        #region Scene Variables
        public GameObject AdvancedMenu;
        public GameObject JoinOptions;
        public GameObject HostOptions;
        public GameObject ErrorText;
        public GameObject Connecting;
        public GameObject RoomMenu;
        public Transform content;
        public Transform Canvas;
        public GameObject Loading;
        #endregion
        private void OnEnable()
        {
            NetWorkConnecter.Instance.OnRoomInfoCreated.AddListener(RoomInfoCreated);
            NetWorkConnecter.Instance.FailedToJoinRandomRoom.AddListener(FailedToJoinRandomRoom);
            NetWorkConnecter.Instance.OnConnectedToLobby.AddListener(ConnectedToLobby);
            NetWorkConnecter.Instance.OnCreatedRoomFailed.AddListener(FailedToCreateOrJoinRoom);
            NetWorkConnecter.Instance.OnJoinRoomFailure.AddListener(FailedToCreateOrJoinRoom);
            
        }
        #region Methods assigned to NetWorkConnecter events
        void RoomCreated()
        {
            Loading.SetActive(true);
        }
        void FailedToCreateOrJoinRoom(short code, string message)
        {
            Loading.SetActive(false);
        }
        void ConnectedToLobby()
        {
            try
            {
                Debug.Log("Connected to lobby");
                RoomMenu.SetActive(true);
                Connecting.SetActive(false);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        void FailedToJoinRandomRoom(int code, string message)
        {
            GameObject _error = Instantiate(ErrorText);
            Connecting.SetActive(false);
            _error.transform.SetParent(Canvas);
            _error.transform.localPosition = new Vector2(0, 0);
            _error.GetComponentInChildren<TextMeshProUGUI>().text = "Code= " + code + " " + message;
            Destroy(_error, 5f);

        }
        public void RoomInfoCreated(GameObject _item, Photon.Realtime.RoomInfo item)
        {
            _item.transform.SetParent(content, false);
            _item.GetComponentInChildren<TextMeshProUGUI>().text = item.Name;
        }
        #endregion
        #region Methods Assigned to Buttons in the scene
        public void OpenAdvanced() => AdvancedMenu.SetActive(true);
        public void CloseAdvanced() =>AdvancedMenu.GetComponent<Animator>().SetTrigger("Return"); 
        public void OpenJoinOptions() => JoinOptions.SetActive(true);
        public void CloseJoinOptions() => JoinOptions.SetActive(false);
        public void OpenHostOptions() => HostOptions.SetActive(true);
        public void CloseHostOptions() => HostOptions.SetActive(false);
        public void GetRoomList() => NetWorkConnecter.Instance.GetRoomList();
        public void JoinRandomRoom() => NetWorkConnecter.Instance.JoinRandomRoom();
        public void HostRoom() => NetWorkConnecter.Instance.CreateRoom();
        #endregion
 
    }
}