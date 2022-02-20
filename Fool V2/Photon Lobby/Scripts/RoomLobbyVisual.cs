using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using TMPro;
namespace PhotonLobby.Example
{   
    public class RoomLobbyVisual : MonoBehaviour
    {
        [Header("Static strings")]
        public string ButtonReadyTrigger = "Ready";
        public string ButtonUnReadyTrigger = "UnReady";
        public string isMasterText = "You are the room owner";
        public string NotMasterText = "You not the room owner Wait for the owner to start";
        public string StartGameText = "Start Game";
        public string NeedMorePlayersText = "Need More Players to Start";
        public string StaticPlayerNumber = "Number of Players :";
        [Header("References")]
        public RoomLobbyHandler roomlobbyLogic;
        public GameObject StartGameButton;
        public GameObject RoomSettingMenu;
        public TextMeshProUGUI PlayersNumberText;
        public GameObject NoTeam;
        public Transform NoTeamcontent;

     
      
        public void OnEnable()
        {
            roomlobbyLogic.readyToggle.AddListener(ButtonStateChanged);
            roomlobbyLogic.GoMade.AddListener(GameObjectMade);
            roomlobbyLogic.onstart.AddListener(onstart);
            roomlobbyLogic.onPlayersReady.AddListener(onAllReady);
            roomlobbyLogic.onPlayerNumbersChange.AddListener(PlayerNumberChange);
           
        }
       
        #region Subcricbed Events Methods
        void PlayerNumberChange(int PlayersNumber, int MaxPlayers)
        {
            string TextToWrite;
            if (PlayersNumber == MaxPlayers)
                TextToWrite = " <color=green>" + PlayersNumber + "/" + MaxPlayers + "</color>";
            else TextToWrite = " <color=red>" + PlayersNumber + "/" + MaxPlayers + "</color>";
            PlayersNumberText.text = StaticPlayerNumber + TextToWrite;

        }
        void onAllReady(bool CanStart, bool NeedAll, bool AllReady)
        {
            Debug.Log("All Ready =" + AllReady + " Can Start = " + CanStart + " Need all= " + NeedAll);
            if (!roomlobbyLogic.roomOptions.UseReady)
                return;
            if (AllReady)
            {
                if (CanStart)
                {
                    StartGameButton.GetComponentInChildren<TextMeshProUGUI>().text = StartGameText;

                }
                else if (NeedAll)
                {
                    StartGameButton.GetComponentInChildren<TextMeshProUGUI>().text = NeedMorePlayersText;
                }
            }
            else
            {
                onstart(PhotonNetwork.IsMasterClient);
            }
        }
        void onstart(bool IsMaster)
        {
            if (IsMaster)
                StartGameButton.GetComponentInChildren<TextMeshProUGUI>().text = isMasterText;
            else StartGameButton.GetComponentInChildren<TextMeshProUGUI>().text = NotMasterText;
        }
        void ButtonStateChanged(RoomLobbyHandler.RoomPlayer roomPlayer, bool ready)
        {
            try
            {
                Button _button;
                if (roomPlayer.playershowcase == null)
                    _button = roomlobbyLogic.PlayersGo[roomPlayer._player].GetComponentInChildren<Button>();
                else
                    _button = roomPlayer.playershowcase.GetComponentInChildren<Button>();
               
                if (roomPlayer._player == PhotonNetwork.LocalPlayer)
                {
                    //   Debug.LogWarning("It's my player State changed visual");
                    if (ready)
                        _button.GetComponent<Animator>().SetTrigger(ButtonReadyTrigger);
                    else _button.GetComponent<Animator>().SetTrigger(ButtonUnReadyTrigger);
                }
                else
                {
                    //   Debug.LogWarning("It's my NOTTT player State changed visual");
                    if (ready)
                        _button.GetComponent<Image>().color = Color.green;
                    else _button.GetComponent<Image>().color = Color.red;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        void GameObjectMade(GameObject _player, Player player)
        {
            try
            {
                _player.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = player.ActorNumber + ": ";
                _player.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = player.NickName;
                Debug.Log("use ready mil visual " + roomlobbyLogic.roomOptions.UseReady);
                if (roomlobbyLogic.roomOptions.UseReady)
                {
                    if (player != PhotonNetwork.LocalPlayer)
                    {
                        Button _button = _player.GetComponentInChildren<Button>();
                        _button.enabled = false;
                        _button.GetComponent<Animator>().enabled = false;
                        _button.GetComponent<Image>().color = Color.red;
                    }
                }
                else _player.GetComponentInChildren<Button>().gameObject.SetActive(false);
                //if (!roomlobbyLogic.roomOptions.RoomContainTeams)
                _player.transform.SetParent(NoTeamcontent);
                //PhotonTeam team = PhotonTeamExtensions.GetPhotonTeam(player);
                //if (team == null)
                //    return;
                //JoinedTeam(player, team);

            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
      
        #endregion
        #region UI Actions(assigned to Buttons)
        public void LeaveRoom() => PhotonNetwork.LeaveRoom();
        public void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
                return;
            }
            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel("offline");
        }
        public void ToggleSettings()
        {
            if (!RoomSettingMenu.activeInHierarchy)
                RoomSettingMenu.SetActive(true);
            else RoomSettingMenu.SetActive(false);
        }
        public void CloseSettings() => RoomSettingMenu.SetActive(false);
        public void ChangeToBlueTeam() => PhotonTeamExtensions.SwitchTeam(PhotonNetwork.LocalPlayer, 1);
        public void ChangeToRedTeam() => PhotonTeamExtensions.SwitchTeam(PhotonNetwork.LocalPlayer, 0);

        public void CheckWith4(bool To) => roomlobbyLogic.CheckWith4 = To;
        #endregion

    }
}