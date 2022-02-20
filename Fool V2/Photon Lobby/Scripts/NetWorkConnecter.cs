using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using Photon.Pun.UtilityScripts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
namespace PhotonLobby
{
    public class NetWorkConnecter : MonoBehaviourPunCallbacks
    {
        [Header("GameObject Representing a room")]
        public GameObject RoomInfoGo;
        private static NetWorkConnecter instance;
        public static NetWorkConnecter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<NetWorkConnecter>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = "NetworkManager";
                        instance = obj.AddComponent<NetWorkConnecter>();
                    }

                }

                return instance;
            }
        }
        [Header("Automatically Sync Scene")]
        public bool autoSync=true;
        private bool AutoSync
        {
            get { return autoSync; }
            set 
            {
                autoSync = value;
                PhotonNetwork.AutomaticallySyncScene = AutoSync; 
            }
        }
        private Dictionary<RoomInfo, GameObject> Rooms = new Dictionary<RoomInfo, GameObject>();
        private List<RoomInfo> FoundRoom = new List<RoomInfo>();
        [Header("Information about the room to be created")]
        public Roominfo roominfo= new Roominfo(true, true, 4, "ww",new RoomLobbyHandler.RoomOptions() { NeedAll = true, UseReady = false });
        [Header("Default number of players")]
        public int DefaultPlayerNumber;
        private bool GetRooms = false;
        #region Events
       [HideInInspector] public UnityEvent<GameObject, RoomInfo> OnRoomInfoCreated;
        public UnityEvent<int, string> FailedToJoinRandomRoom;
        public UnityEvent OnConnectedToLobby;
        public UnityEvent<short, string> OnCreatedRoomFailed;
        public UnityEvent<short, string> OnJoinRoomFailure;
        /// <summary>
        /// this event is invoked when room is created and room lobby scene is loading
        /// </summary>
        public UnityEvent OnRoomCreated;
        #endregion
  
        public override void OnDisable()
        {
            base.OnDisable();
            OnDisableVirtual();
        }
        public virtual void OnDisableVirtual()
        {
            Debug.Log("OnDisable");
            GetRooms = false;
            #region Removing Listeners
            OnRoomInfoCreated.RemoveAllListeners();
            FailedToJoinRandomRoom.RemoveAllListeners();
            OnRoomCreated.RemoveAllListeners();
            OnConnectedToLobby.RemoveAllListeners();
            OnCreatedRoomFailed.RemoveAllListeners();
            OnJoinRoomFailure.RemoveAllListeners();
            #endregion
        }
        void Start()
        {
            roominfo = new Roominfo(true, true, 4, RandomNameGenerator.GenerateRandomString(5), new RoomLobbyHandler.RoomOptions() { NeedAll = true, UseReady = false });
          
        }
        public override  void OnConnectedToMaster() => JoinLobby();
        #region Lobby
        public virtual void Setup()
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.AutomaticallySyncScene = AutoSync;
        }
       
        public void JoinLobby()=>  PhotonNetwork.JoinLobby();       
        public virtual void GetRoomList()
        {
            ResetList();
            GetRooms = true;
            MakeRoomsGO();

        }
        void MakeRoomsGO()
        {
            foreach (RoomInfo item in FoundRoom)
            {
                if (!Rooms.ContainsKey(item))
                {
                    GameObject _item = Instantiate(RoomInfoGo);
                    OnRoomInfoCreated?.Invoke(_item, item);
                    _item.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(item.Name); });
                    Rooms.Add(item, _item);
                }
            }

        }
        void ResetList()
        {
            foreach (KeyValuePair<RoomInfo, GameObject> entry in Rooms)
            {

                if (entry.Value != null)
                {
                    Destroy(entry.Value);
                }

            }
            Rooms = new Dictionary<RoomInfo, GameObject>();
        }
        public virtual void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

        #endregion
        #region room
        RoomOptions SetARoomOptions()
        {
            #region Checking for empty values
            if (roominfo.MaxPlayers == 0)
                roominfo.MaxPlayers = DefaultPlayerNumber;
            if (roominfo.RoomName == null || roominfo.RoomName == string.Empty)
                roominfo.RoomName = RandomNameGenerator.GenerateRandomString(5);
            #endregion
            ExitGames.Client.Photon.Hashtable RoomEntries = new ExitGames.Client.Photon.Hashtable();
            RoomEntries.Add("NA", roominfo.roomOptions.NeedAll);
            RoomEntries.Add("UR", roominfo.roomOptions.UseReady);

            RoomOptions roomOptions = new RoomOptions()
            { IsVisible = roominfo.IsVisible, IsOpen = roominfo.IsOpen, MaxPlayers = (byte)roominfo.MaxPlayers, CustomRoomProperties = RoomEntries };
            return roomOptions;
        }
        public virtual void CreateRoom()
        {
            PhotonNetwork.CreateRoom(roominfo.RoomName, SetARoomOptions());
            OnRoomCreated?.Invoke();
        }
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError("Failed to create room");
            OnCreatedRoomFailed?.Invoke(returnCode, message);
        }
        public override void OnJoinRoomFailed(short returnCode, string message) { OnJoinRoomFailure?.Invoke(returnCode, message); }
        public override void OnJoinedRoom() => OnJoinedRoomVirtual();
        public  void JoinRoom(string roomname) => PhotonNetwork.JoinRoom(roomname);
        public override void OnJoinedLobby() => OnConnectedToLobby?.Invoke();
        #endregion
        /// <summary>
        /// It has been changed to virtual because NetWorkConnecter need the callback and for you to expand upon them
        /// </summary>
        #region PhotonCallback changed to virtual
        #region Lobby
        public virtual void OnJoinedRoomVirtual()
        {
            PhotonNetwork.LoadLevel("RoomLobby");
        }
 
       
        #endregion
        #region Room
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            Debug.Log("Room list update");
            FoundRoom = new List<RoomInfo>();
            foreach (RoomInfo item in roomList)
            {
                if (!Rooms.ContainsKey(item))
                {
                    Debug.Log(item.Name);
                    if (GetRooms)
                    {
                        GameObject _item = Instantiate(RoomInfoGo);
                        OnRoomInfoCreated?.Invoke(_item, item);
                        _item.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(item.Name); });
                        Rooms.Add(item, _item);
                    }
                    if (!FoundRoom.Contains(item))
                        FoundRoom.Add(item);

                }
            }
        }
        public override void OnJoinRandomFailed(short returnCode, string message)=>FailedToJoinRandomRoom?.Invoke(returnCode, message);

        #endregion
        #endregion
        #region Assign to button 
        public void ChangeRoomName(string chooseName) => roominfo.RoomName = chooseName;

        public  void SetNeedAll(bool needall) => roominfo.roomOptions.NeedAll = needall;
        public  void SetUseReady(bool UseReady) => roominfo.roomOptions.UseReady = UseReady;
        public  void SetOpen(bool isopen) => roominfo.IsOpen = isopen;
        #endregion
        [System.Serializable]
        public struct Roominfo
        {
            public bool IsVisible;
            public bool IsOpen;
            public int MaxPlayers;
            public string RoomName;
            /// <summary>
            /// This options aren't the basic room options 
            /// </summary>
            /// <remarks>
            /// i have reserved NA,CT,UR string for those options
            /// NA=NEED all,CT=roomcontainteams,UR=useready
            /// </remarks>
            public RoomLobbyHandler.RoomOptions roomOptions;
            public Roominfo(bool visibile, bool isopen, int maxplay, string roomname, RoomLobbyHandler.RoomOptions entries)
            {
                IsVisible = visibile;
                IsOpen = isopen;
                MaxPlayers = maxplay;
                RoomName = roomname;
                roomOptions = entries;
            }
        }

    }
    public static class RandomNameGenerator
    {   //simple random string generation 
        public static string GenerateRandomString(int size)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            char[] chars = new char[size];

            for (int i = 0; i < size; i++)
            {
                chars[i] = allowedChars[Random.Range(0, allowedChars.Length)];
            }

            return new string(chars);
        }
        public static byte GenerateRandomByte(int size, int max, int min = 0)
        {
            byte ToReturn = new byte();
            for (int i = 0; i < size; i++)
            {
                ToReturn = (byte)Random.Range(min, max);
            }
            return ToReturn;

        }
    }
}