using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.Events;


namespace PhotonLobby
{ /// <summary>
///   the script that hold all logic for Room's lobby  subscribe  to it's event to change visuals
/// </summary>
/// <remarks>
/// if you want this Object instance use (RoomLobbyHandler)PlayerNumbering.instance 
/// </remarks>
    public class RoomLobbyHandler : PlayerNumbering
    { 
        #region References Variables
        [Header("A GameObject representing a player in this room(visualy) ")]
        public GameObject PlayerItemGo;
        [Header("The Button that will change to gameplay scene")]
        public Button ChangeSceneButton;
        #endregion
        [SerializeField] private List<RoomPlayer> RoomPlayers = new List<RoomPlayer>();
        public Dictionary<Player, GameObject> PlayersGo = new Dictionary<Player, GameObject>();
        [Header("Game Logic (next scene)")]
        public bool CheckWith4;
        /// <summary>
        /// Gets updated using room properiets at start 
        /// </summary>
        public RoomOptions roomOptions;
      
        #region Events
        /// <summary>
        ///  This event is invoked whenever A Player item GO is Instantiated
        /// </summary>
        /// <remarks>
        /// use this method to change that GameObject visual to your liking
        /// </remarks>
        public UnityEvent<GameObject,Player> GoMade;
        /// <summary>
        ///  This event is invoked when setup is called bool represents if the player is MasterClient
        /// </summary>
        public UnityEvent<bool> onstart;
        /// <summary>
        ///  This event is invoked when onEnable Button to change text for ChangeSceneButton 
        /// </summary>
        /// <remarks>
        /// first bool if the game can start ,second for need all check,third  if all players are ready
        /// </remarks>
        public UnityEvent<bool,bool,bool> onPlayersReady;
        /// <summary>
        ///  This event is invoked when a player change his ready state
        /// </summary>
        public UnityEvent<RoomPlayer,bool> readyToggle;
        /// <summary>
        ///  This event is invoked when a player leaves or enter the room
        /// </summary>
        public UnityEvent<int,int> onPlayerNumbersChange;
        #endregion
        public override void OnDisable()
        {
            base.OnDisable();
            onPlayerNumbersChange.RemoveAllListeners();
            readyToggle.RemoveAllListeners();
            onPlayersReady.RemoveAllListeners();
            onstart.RemoveAllListeners();
            GoMade.RemoveAllListeners();
        }
        void Start()
        { 
            UpdateRoomProperiets();
            Setup();

        }
        public void Setup()
        {
           StartWithoutTeam();
        }
        #region Creating GameObject and RoomPlayer for Players in the room
        #region Public Methods
        public virtual void StartWithoutTeam()
        {

            onstart?.Invoke(PhotonNetwork.IsMasterClient);

            foreach (Player player in SortedPlayers)
            {
                CreatePlayer(player).playershowcase = CreatePlayerGo(player);
            }
            onPlayerNumbersChange?.Invoke(SortedPlayers.Length, PhotonNetwork.CurrentRoom.MaxPlayers);
        }
     
       
        #endregion
        #region internal methods
        GameObject CreatePlayerGo(Player player)
        {
            GameObject _player = Instantiate(PlayerItemGo);
            PlayersGo.Add(player, _player);
            if (roomOptions.UseReady)
                _player.GetComponentInChildren<Button>().onClick.AddListener(delegate { ChangeReady(); });
            GoMade?.Invoke(_player, player);
            return _player;

        }


        RoomPlayer CreatePlayer(Player newPlayer)
        {
            RoomPlayer _roomplayer = new RoomPlayer(newPlayer.ActorNumber, newPlayer);
            RoomPlayers.Add(_roomplayer);
            return _roomplayer;

        }
        #endregion
        #endregion
        #region ReadyState Changes
        void ChangeReady()
        {
            RoomPlayer _roomPlayer = GetRoomPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
            _roomPlayer.SetReadyState();
            object[] para = { PhotonNetwork.LocalPlayer.ActorNumber, _roomPlayer.ReadyState };
            EnableButton();
            photonView.RPC("StateChanged", RpcTarget.Others, para);
            readyToggle?.Invoke(_roomPlayer, _roomPlayer.ReadyState);
        }
        /// <summary>
        /// Called to update new player for Ready State of older players
        /// </summary>
        /// <remarks>
        /// PunRpc Method
        /// </remarks>
        [PunRPC]
        void UpdateNewbie(object[] para)
        {
            try
            {
                UpdateRoomProperiets();
                if (para.Length % 2 != 0)
                {
                    Debug.LogError("Parameters for Update Newbie was incorrect");
                }

                int i = -1;
                while (i < para.Length - 1)
                {
                    i++;
                    if (CheckObject.IsNumber(para[i]))
                    {
                        i++;
                        if (CheckObject.IsBool(para[i]))
                        {
                            RoomPlayer _roomplayer = GetRoomPlayer((int)para[i - 1]);
                            _roomplayer.ReadyState = (bool)para[i];
                            readyToggle?.Invoke(_roomplayer, _roomplayer.ReadyState);
                        }
                        else
                        {
                            Debug.LogError("There was not bool inserted after a Player Index! Stopping! at i= " + i);
                            return;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        [PunRPC]
        void StateChanged(int actorindex, bool ready, PhotonMessageInfo info)
        {
            Debug.Log("changing state Rpc");
            RoomPlayer _player = GetRoomPlayerWithPlayer(info.Sender);
            _player.ReadyState = ready;
            try
            {

                readyToggle?.Invoke(_player, ready);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
            EnableButton();
        }
        void EnableButton()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                bool allReady;
                if (roomOptions.UseReady)
                    allReady = RoomPlayers.All(PlayerState => PlayerState.ReadyState);
                else allReady = true;

                bool CanStart;
                if (roomOptions.NeedAll)
                    CanStart = (PhotonNetwork.CurrentRoom.MaxPlayers == RoomPlayers.Count) && allReady;
                else CanStart = allReady;
                onPlayersReady?.Invoke(CanStart, roomOptions.NeedAll, allReady);

                ChangeSceneButton.interactable = CanStart;
            }

        }
        #endregion
        #region Others Method

        void UpdateRoomProperiets()
        {
            roomOptions = new RoomOptions();
            foreach (var item in PhotonNetwork.CurrentRoom.CustomProperties)
            {
                
                Debug.Log("key in properiets= " + item.Key.ToString() + " value " + item.Value.ToString());
                if ((string)item.Key == "NA")
                {
                    roomOptions.NeedAll = (bool)item.Value;
                }
                else if ((string)item.Key == "UR")
                {
                    roomOptions.UseReady = (bool)item.Value;
                }

            }
        }
        void RefreshNumbers()
        {
            foreach (RoomPlayer item in RoomPlayers)
            {
                item.RefreshNumber();
            }
        }
        RoomPlayer GetRoomPlayerWithPlayer(Player player)
        {
            foreach (RoomPlayer item in RoomPlayers)
            {
                if (item._player == player)
                    return item;
            }
            Debug.LogError("Didn't Find a Player!!!");
            return null;
        }
        /// <summary>
        /// Search for RoomPlayer with Actor number in Photon Player
        /// </summary>
        /// <returns>
        /// Returns RoomPlayer with the given Actor number , null if not found
        /// </returns>
        /// <param name="Playerindex">Actor Number </param>
        public RoomPlayer GetRoomPlayer(int Playerindex)
        {
            foreach (RoomPlayer item in RoomPlayers)
            {
                if (item.PlayerIndex == Playerindex)
                    return item;
            }
            Debug.LogError("Found no player ");
            return null;
        }
        /// <summary>
        /// Search for for RoomPlayer associated with the local player
        /// </summary>
        public RoomPlayer GetMyPlayer()
        {
            foreach (RoomPlayer _player in RoomPlayers)
            {

                if (_player._player == PhotonNetwork.LocalPlayer)
                    return _player;
            }
            Debug.LogError("i GOT NO PLAYER !!!");
            return null;
        }
        #endregion
        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            SceneManager.LoadScene(0);
        }
        #region Photon call back changed to virtual methods
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            CreatePlayerGo(newPlayer);
            CreatePlayer(newPlayer);
            onPlayerNumbersChange?.Invoke(SortedPlayers.Length, PhotonNetwork.CurrentRoom.MaxPlayers);
            if (roomOptions.UseReady)
                if (PhotonNetwork.IsMasterClient)
                {
                    object[] parameters = new object[10];
                    int i = 0;
                    foreach (RoomPlayer item in RoomPlayers)
                    {
                        parameters[i] = item.PlayerIndex;
                        i++;
                        parameters[i] = item.ReadyState;
                        i++;
                    }
                    photonView.RPC("UpdateNewbie", newPlayer, parameters);
                }
            EnableButton();
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            base.OnPlayerLeftRoom(otherPlayer);
            RoomPlayer _PLAYER = GetRoomPlayer(otherPlayer.ActorNumber);
            RoomPlayers.Remove(_PLAYER);
            Destroy(PlayersGo[_PLAYER._player]);
            RefreshNumbers();
            onPlayerNumbersChange?.Invoke(SortedPlayers.Length, PhotonNetwork.CurrentRoom.MaxPlayers);

        }
        #endregion
        public void ToGamePlayScene()
        {
            Hashtable tempprop = PhotonNetwork.CurrentRoom.CustomProperties;
            tempprop.Add("C", CheckWith4);
            PhotonNetwork.CurrentRoom.SetCustomProperties(tempprop);
            PhotonNetwork.LoadLevel("FoolV2Online");
        }
        /// <summary>
        /// local representasion of the players inside the room to check thier ready status
        /// </summary>
        /// <remarks>
        /// </remarks>
        [System.Serializable]
        public class RoomPlayer
        {
            public Photon.Realtime.Player _player;
            public GameObject playershowcase;
            // Details about player
            public int PlayerIndex;
            public bool ReadyState;
            public string name;

            public void SetReadyState()
            {
                if (ReadyState) ReadyState = false;
                else ReadyState = true;
            }
            public RoomPlayer(int index, Player playyer)
            {
                this.PlayerIndex = index;
                this._player = playyer;
                name = playyer.NickName;
                ReadyState = false;
            }
            public RoomPlayer(int index, Player playyer, GameObject Go)
            {
                this.PlayerIndex = index;
                this._player = playyer;
                playershowcase = Go;
                name = playyer.NickName;
                ReadyState = false;
            }
            public void RefreshNumber() => PlayerIndex = _player.ActorNumber;

        }
      
        [System.Serializable]
        public struct RoomOptions
        {
            [Header("Does Room needs to be at max players to Start Game")]
            public bool NeedAll;
            public bool UseReady;
           
        }
      
    }
    public static class CheckObject
    {
        public static bool IsNumber(this object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }
        public static bool IsBool(this object value)
        {
            return value is bool;
        }
    }
}