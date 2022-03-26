using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using Fusion;

public class GameHandlerv2 : NetworkBehaviour
{
    public static readonly string[] AllowedChar = { "K", "L", "S", "P" };
    public float TickInterval;
    public NetworkPrefabRef PlayerObject;
    public NetworkPrefabRef EmptyCard;
    [SerializeField] private float CurrentTickStamp;
    #region references
    public GameObject P1area;
    public GameObject P2area;
    public GameObject P3area;
    public GameObject P4area;
    public GameObject card;
    public GameObject DontMindHim;
    public GameObject StartPanelPreFab;
    #endregion
    #region Lists
   
    public ModNetworkArray all = new ModNetworkArray();
    [Networked, Capacity(100)]
    public NetworkArray<ModNetworkObject> _Array => default;
    public List<GameObject> allcards = new List<GameObject>();
    public List<GameObject> allcardsInGame = new List<GameObject>();
    public List<GameObject> CardsOnField = new List<GameObject>();
    [HideInInspector] public List<GameObject> allButton = new List<GameObject>();
    public List<GameObject> TempoSelectedCards = new List<GameObject>();
    [HideInInspector] public List<Transform> Areas = new List<Transform>();
    public List<OnlinePlayer> onlinePlayers = new List<OnlinePlayer>();
    #endregion
    #region Bools 
    [HideInInspector] [Networked] public NetworkBool isCardsOnField { get; set; }
    public bool Flipped = false;
    [HideInInspector] [Networked] public NetworkBool CheckWith4 { get; set; }
    #endregion
    #region int Checkers
    [Header("Int for checking")]
    public int EachPlayerNB = 13;
    public int WaitTime;
    [HideInInspector] [Networked] public int SelectedCardNumber { get; set; }
    [HideInInspector] [Networked] public int LastPlayedCards { get; set; }
    [HideInInspector] [Networked] public int DebateOngoingTurns { get; set; }
    [HideInInspector] [Networked] public int TurnInt { get; set; }
    public GameObject PrevSelectedButton;
    #endregion
    private static GameHandlerv2 instance;
    public static GameHandlerv2 Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameHandlerv2>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = "GameHandler";
                    instance = obj.AddComponent<GameHandlerv2>();
                }

            }

            return instance;
        }
    }
    public string PlayersWonOrder;
    public Lang lang;
    public OnlineGameState gameState;
    public CardsContainer cardContainer;
    public List<int> CheckList = new List<int>();
    public NetworkObject networkObject;
    public NetworkArray<float> sync => default;
    [System.Serializable]
    public struct CardFalse
    {
        public GameObject Carde;
        public NetworkBool Correct;
        public CardFalse(GameObject cc, NetworkBool ccorr)
        {
            Carde = cc;
            Correct = ccorr;
        }
    }
    //public struct CardFalse :INetworkStruct
    //{
    //    public NetworkObject Carde;
    //    public NetworkBool Correct;
    //    public CardFalse(NetworkObject cc, NetworkBool ccorr)
    //    {
    //        Carde = cc;
    //        Correct = ccorr;
    //    }
    //}
    [HideInInspector] public Stringcontainer stringcontainer;
    #region  Events
    [HideInInspector] public UnityEvent<GameObject, Transform> SetParent;
    [HideInInspector] public UnityEvent<GameObject, Transform> OnConfirm;
    [HideInInspector] public UnityEvent<GameObject> ResetAreaPar;
    [HideInInspector] public UnityEvent<string> OnCardDestroy;
    [HideInInspector] public UnityEvent PopSmoke;
    [HideInInspector] public UnityEvent PreGameStart;
    [HideInInspector] public UnityEvent OnChoiceConfirm;
    [HideInInspector] public UnityEvent OnLostDebate;
    [HideInInspector] public UnityEvent OnWinDebate;
    [HideInInspector] public UnityEvent<bool> OnOthersDebate;
    [HideInInspector] public UnityEvent<GameObject> Deselected;
    [HideInInspector] public UnityEvent<GameObject> Selected;
    [HideInInspector] public UnityEvent OnTurnChanged;
    [HideInInspector] public UnityEvent CleanUpButton;
    [HideInInspector] public UnityEvent OnShowOthersStart;
    [HideInInspector] public Action<OnlinePlayer> OnPlayerCreated;
    #endregion

    public void IncTurnInt() => TurnInt = Extension.IncInt(TurnInt, onlinePlayers);
    private void Update()
    {
        if (CurrentTickStamp >= TickInterval)
        {

            RemoveNulls();
            checkCardsAreInPlace();
            CurrentTickStamp = 0;

        }
        else CurrentTickStamp += Time.deltaTime;
    }
    private void Start()
    {
        SetParent.AddListener(SetPARENT);
    }
    public override void Spawned()
    {
        #region setting up Variables
        Flipped = true;
        TurnInt = 0;
        SelectedCardNumber = -1;
        isCardsOnField = false;
        CheckWith4 = true;
        networkObject = GetComponent<NetworkObject>();
        Runner = FindObjectOfType<NetworkRunner>();
       // all.Set(1, networkObject);
        #endregion
    }
    public void StartGame()
    {
        if (networkObject.HasStateAuthority)
            StartGamePre(Runner.ActivePlayers.ToList());
        GetMyPlayer().Area = Areas[0];
        
    }
    public void SortPlayerCards()
    {
        foreach (OnlinePlayer item in onlinePlayers)
            item.SortCards();
    }
    void checkCardsAreInPlace()
    {
        int Checker;
        for (int i = 0; i < CheckList.Count; i++)
        {
            Checker = 0;
            for (int j = 0; j < allcards.Count(); j++)
            {

                if (int.Parse(allcards[j].name) == CheckList[i])
                {
                    Checker++;
                }
                if (Checker == 4)
                {

                    break;

                }
            }
            if (Checker <= 3)
                Debug.LogError("Missing card Number= " + allcards[i]);

        }
    }
    void RemoveNulls()
    {
        try
        {
            foreach (OnlinePlayer item in onlinePlayers)
            {
                if (item == null)
                {
                    onlinePlayers.Remove(item);
                    break;
                }
                item.checkFornull();
            }
            for (int i = allcards.Count() - 1; i >= 0; i--)
            {
                if (allcards[i] == null)
                    allcards.Remove(allcards[i]);
            }

        }
        catch (Exception e)
        {
            Debug.Log("Error in remove null " + e.Message);
            RemoveNulls();
        }
    }
    #region GameStarter
    public void StartGamePre(List<PlayerRef> SortedPlayers)
    {
        CheckWith4 = true;
        Flipped = false;
        PreGameStart?.Invoke();
        if (networkObject.HasStateAuthority)
        {

            foreach (PlayerRef item in SortedPlayers)
            {
                NetworkObject obj = Runner.Spawn(PlayerObject, new Vector3(0, 0, 0), Quaternion.identity);
                obj.GetComponent<OnlinePlayer>().SetupPlayer(item);
                //obj.GetComponent<NetworkObject>().State(item);
                onlinePlayers.Add(obj.GetComponent<OnlinePlayer>());
                //Rpc Target OTher
                RPCAddPlayer(obj);
                OnPlayerCreated?.Invoke(obj.GetComponent<OnlinePlayer>());
            }
            StartGameV2();
            gameState = new OnlineTurnState(onlinePlayers[0],this);
            gameState.ManageState();

        }
        //RpcTarget Other
        RPCToWaitState();

    }
    public void StartGameV2()
    {

        Debug.Log("Start Game v2");
        AddCardsV3();
        ///important
        for (int i = 0; i < allcards.Count; i++)
        {
            swap(ref allcards, Random.Range(0, allcards.Count), Random.Range(0, allcards.Count));
        }
        int PlayerIndex = 0;
        foreach (GameObject item in allcards)
        { //RPC target All
            RPCAddCard(item.GetComponent<NetworkObject>(), PlayerIndex);
            PlayerIndex = Extension.IncInt(PlayerIndex, onlinePlayers.Count);
        }
        ///End of important
        //RpcTarget All
        RPCSetParent();
        DestroyDuplicatesAll();
        Debug.Log("All cards Count" + allcards.Count);
    }
    void swap(ref List<GameObject> allcard, int index1, int index2)
    {
        GameObject _tmp = allcard[index1];
        allcard[index1] = allcard[index2];
        allcard[index2] = _tmp;
    }
    // for testing purposes
    public void AddCardsV3()
    {
        int VirtualPlayersNumber;
        if (onlinePlayers.Count < 4 && CheckWith4)
        {
            VirtualPlayersNumber = 4;
        }
        else VirtualPlayersNumber = onlinePlayers.Count + onlinePlayers.Count % 2;
        Debug.Log("Virtual players number =" + VirtualPlayersNumber);
        string CardType;
        for (int i = 0; i < EachPlayerNB; i++)
        {
            CardType = GameHandlerv2.AllowedChar[Random.Range(0, 3)];
            CheckList.Add(i);
            List<Sprite> CurrentList = cardContainer.SpriteContainer[CardType];
            for (int f = 0; f < VirtualPlayersNumber; f++)
            {
                NetworkObject cardee = Runner.Spawn(EmptyCard, new Vector2(0, 0), Quaternion.identity);
                //cardee.GetComponent<Image>().sprite = CurrentList[i];
                cardee.GetComponent<OfflineCardManager>().CurrentSprite = cardee.GetComponent<Image>().sprite;
                cardee.GetComponent<OfflineCardManager>().EqNumber = i;

                //allcards.Add(cardee.gameObject);
                RPCSetCard(cardee, i, CardType);
            }
        }
        Debug.Log(allcards.Count);
    }
    #endregion
    #region Debate
    public void ChoosenDebate() => RPCChoosenDebate();
    public void ChoosenDebate(PlayerRef _playerSource)
    {
        bool ChoicesAreRight = true;
        // Not Really all false card also for true cards;
        List<GameHandlerv2.CardFalse> falseCards = new List<GameHandlerv2.CardFalse>();
        Debug.Log("_player Source =" + SearchForPlayer(_playerSource).isLocalPlayer);
        Debug.Log("SelectedCardNumber; is = " + SelectedCardNumber);

        for (int i = CardsOnField.Count - LastPlayedCards; i < CardsOnField.Count; i++)
        {
            //Debug.Log(i + "   Choosen debate");
            if (CardsOnField[i].GetComponent<OfflineCardManager>().EqNumber != SelectedCardNumber)
            {
                ChoicesAreRight = false;
                Debug.Log("FOUND one ya7chi fih");
                falseCards.Add(new GameHandlerv2.CardFalse(CardsOnField[i], false));
            }
            else falseCards.Add(new GameHandlerv2.CardFalse(CardsOnField[i], true));
        }
        foreach (GameObject item in CardsOnField)
            //all
            RPCVerfyingChoice(item.GetComponent<NetworkObject>(), ChoicesAreRight);
        if (ChoicesAreRight)
            IncTurnInt();
        //add networkobject array for false cards
        RPCVerifyChoice(_playerSource, ChoicesAreRight);
        Debug.Log("choices correct is= " + ChoicesAreRight);
        CloseDead(ChoicesAreRight, _playerSource);
        //other
        // RPCVerifyChoiceForOther();
        //RPCloseDead(ChoicesAreRight);
    }
    #endregion
    #region Flows
    public void ConfirmChoice()
    {
        //LastPlayedCards = TempoSelectedCards.Count;
        NetworkObject[] array = new NetworkObject[TempoSelectedCards.Count];
        for (int i = 0; i < TempoSelectedCards.Count; i++)
            array[i] = TempoSelectedCards[i].GetComponent<NetworkObject>();
        if (isCardsOnField)
            RPCLeaderConfirmChoice(SelectedCardNumber, array, TempoSelectedCards.Count);
        else RPCLeaderConfirmChoice(allButton.IndexOf(PrevSelectedButton), array, TempoSelectedCards.Count);
        ToWaitState();
        //PrevSelectedButton = null;
    }
    #endregion
    #region Command
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPCServerDestroyCards()
    {
        foreach (var item in onlinePlayers)
        {
            //DestroyCards(item);
            item.Clear(CheckWith4);
        }
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPCChoosenDebate(RpcInfo info = default) => ChoosenDebate(info.Source);
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPCLeaderConfirmChoice(int selectedcard, NetworkObject[] toAdd, int lastPlayedcard)
    {
        Debug.Log("Rpc Confirm choice");
        foreach (NetworkObject Card in toAdd)
        {
            if (Card != null)
                //all
                RPCAddToCardOnField(Card.GetComponent<NetworkObject>());
        }
        IncTurnInt();
        SelectedCardNumber = selectedcard;
        DebateOngoingTurns += 1;
        LastPlayedCards = lastPlayedcard;
        isCardsOnField = true;
        RPCchoiceConfirmedLast(onlinePlayers[TurnInt]._player);
    }
    #endregion
    #region Other (proxies)
    [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
    public void RPCVerifyChoiceForOther()
    {
        NetworkBool ChoicesAreRight = true;
        List<GameHandlerv2.CardFalse> falseCards = new List<GameHandlerv2.CardFalse>();
        for (int i = CardsOnField.Count - LastPlayedCards; i < CardsOnField.Count; i++)
        {
            Debug.Log(i + "   Choosen debate");
            if (CardsOnField[i].GetComponent<OfflineCardManager>().EqNumber != SelectedCardNumber)
            {
                ChoicesAreRight = false;
                Debug.Log("FOUND one ya7chi fih");
                falseCards.Add(new GameHandlerv2.CardFalse(CardsOnField[i], false));
            }
            else falseCards.Add(new GameHandlerv2.CardFalse(CardsOnField[i], true));
        }
        OnOthersDebate?.Invoke(ChoicesAreRight);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
    public void RPCToWaitState()
    {
        GetMyPlayer().Area = Areas[0];
        ToWaitState();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
    public void RPCAddPlayer(NetworkObject Player)
    {
        try
        {

            //Player.GetComponent<OnlinePlayer>().SetupPlayer();
            onlinePlayers.Add(Player.GetComponent<OnlinePlayer>());
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    #endregion
    #region Target
    [Rpc]
    public void RPCchoiceConfirmedLast([RpcTarget] PlayerRef _target)
    {

        OnChoiceConfirm?.Invoke();
        TempoSelectedCards.Clear();
        SwitchState(true);

    }
    [Rpc]
    public void RPCVerifyChoice([RpcTarget] PlayerRef player, NetworkBool ChoicesAreRight)
    {
        Debug.Log("Rpc verify choice Player is =" + player.PlayerId);
        if (ChoicesAreRight)
            OnLostDebate?.Invoke();
        else OnWinDebate?.Invoke();
        //Pv.RpcSecure("IncremenateTurnInt", RpcTarget.All, false);
        //RPCloseDead(ChoicesAreRight);
    }
    #endregion
    #region State=>>ALL
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCRestAfterDebate()
    {
        isCardsOnField = false;
        SelectedCardNumber = -1;
        LastPlayedCards = 0;
        DebateOngoingTurns = 0;
        CardsOnField.Clear();
        SortPlayerCards();
        CheckPlayerWon();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCRemoveNulls(int Player)
    {
        try
        {
            OnlinePlayer _player = SearchForPlayer(Player);
            Debug.Log("Player name is =" + _player.Name);
            _player.checkFornull();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCVerfyingChoice(NetworkObject Obj, NetworkBool ChoicesAreRight)
    {
        if (ChoicesAreRight)
        {
            onlinePlayers[TurnInt].AddCards(Obj.gameObject);
            Obj.gameObject.GetComponent<OfflineCardManager>().ChangeSelectable(false);
        }
        else
        {
            Obj.gameObject.GetComponent<OfflineCardManager>().selectable = false;
            onlinePlayers[Extension.DecInt(TurnInt, onlinePlayers.Count)].AddCards(Obj.gameObject);
        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCAddToCardOnField(NetworkObject Card)
    {

        onlinePlayers[TurnInt].RemoveCard(Card.gameObject);
        SetParent?.Invoke(Card.gameObject, DontMindHim.transform);
        card.GetComponent<OfflineCardManager>().selectable = false;
        CardsOnField.Add(Card.gameObject);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCSetCard(NetworkObject Carde, int CardNumber, string CardType)
    {

        Carde.GetComponent<Image>().sprite = cardContainer.SpriteContainer[CardType][CardNumber];
        Carde.GetComponent<OfflineCardManager>().CurrentSprite = Carde.GetComponent<Image>().sprite;
        //Carde.GetComponent<OfflineCardManager>().EqNumber = CardNumber;
        Carde.gameObject.name = CardNumber.ToString();
        Carde.name = CardNumber.ToString();
        allcards.Add(Carde.gameObject);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCAddCard(NetworkObject Carde, int Playerindex) => onlinePlayers[Playerindex].AddCards(Carde.gameObject);
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCCleanUpPlayers()
    {
        foreach (OnlinePlayer item in onlinePlayers)
        {
            if (item == null)
            {
                onlinePlayers.Remove(item);
                break;
            }

        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCSetParent()
    {
        PreGameStart?.Invoke();
        SortPlayerCards();
        OnlinePlayer _myplayer = GetMyPlayer();
        foreach (GameObject item in _myplayer.PlayerCards)
        {
            item.transform.SetParent(Areas[0], false);
        }
        OnShowOthersStart?.Invoke();

    }
    #endregion
    #region States
    [Rpc]
    public void RPCToWaitState([RpcTarget] PlayerRef player) => ToWaitState();
    [Rpc]
    public void RPCToTurnState([RpcTarget] PlayerRef player) => ToTurnState();
    [Rpc]
    public void RPCToDebateState([RpcTarget] PlayerRef player) => ToDebateState();
    public void ToTurnState()
    {
        gameState = new OnlineTurnState(GetMyPlayer(), this);
        gameState.ManageState();
    }
    public void ToWaitState()
    {
        gameState = new OnlineWaitState(GetMyPlayer(), this);
        gameState.ManageState();
    }
    public void ToDebateState()
    {
        gameState = new OnlineDebateState(GetMyPlayer(), this);
        gameState.ManageState();
    }
    public void SwitchState(bool IsDebateState)
    {
        if (onlinePlayers[TurnInt].isLocalPlayer)
        {
            if (IsDebateState)
                ToDebateState();
            else
                ToTurnState();
        }
        else
            ToWaitState();

    }
    #endregion

    #region Other Method in Server
    public IEnumerator closeDead(bool choicesIsRight, PlayerRef player)
    {
        RPCRestAfterDebate();
        DestroyDuplicatesAll();
        yield return new WaitForSeconds(WaitTime);
        Debug.Log("closing dead after yield " + choicesIsRight);
        // RPCAfterDebateTurn(choicesIsRight,player);
        foreach (OnlinePlayer item in onlinePlayers)
        {
            if (IsCurrentPlayerTurn(item))
                RPCToTurnState(item._player);
            else
                RPCToWaitState(item._player);

        }
    }
    public void CheckPlayerWon()
    {
        Debug.Log("CHECKING NIGGA WINNING");
        foreach (OnlinePlayer item in onlinePlayers)
        {
            if (item.CheckIFwon())
            {
                PlayersWonOrder += item.Name;
                Debug.Log("aYO CHECK THIS OUT DIS nigga just won " + item);
                RPCRemovePlayer(item.GetComponent<NetworkObject>());
                break;

            }

        }

        if (onlinePlayers.Count <= 1)
        {
            Destroy(GameObject.Find("Canvas"));
            Debug.LogError("Ma nigga game is OVER");
            return;
        }
    }
    #endregion
    public void ResetSelected()
    {
        foreach (GameObject item in TempoSelectedCards)
        {
            item.GetComponent<OfflineCardManager>().selectable = true;

        }
        TempoSelectedCards.Clear();
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPCRemovePlayer(NetworkObject _player) => onlinePlayers.Remove(_player.GetComponent<OnlinePlayer>());
    public void CloseDead(NetworkBool ChoicesAreRight, PlayerRef _player) => StartCoroutine(closeDead(ChoicesAreRight, _player));
    public void DestroyDuplicatesAll() => RPCServerDestroyCards();
    public void SetPARENT(GameObject card, Transform area) => card.transform.SetParent(area, false);
    public OnlinePlayer SearchForPlayer(int ActorNumber) => onlinePlayers.Where(player => player.number == ActorNumber).First();
    public OnlinePlayer SearchForPlayer(PlayerRef _player) => onlinePlayers.Where(player => player._player == _player).First();
    public OnlinePlayer GetMyPlayer() => onlinePlayers.Where(player => player._player == Runner.LocalPlayer).First();
    public bool IsCurrentPlayerTurn(OnlinePlayer player) => onlinePlayers[TurnInt] == player;
   
    #region Logic Called From UI
    public void ResetGame()
    {
        onlinePlayers.Clear();
        allcards.Clear();
        allcardsInGame.Clear();
        TurnInt = 0;
        LastPlayedCards = -1;
        CardsOnField.Clear();
        GameObject f = Instantiate(StartPanelPreFab);
        //    f.transform.SetParent(canvas.transform, false);
        f.SetActive(true);
    }
    public void SelectedNumberButton(GameObject Button)
    {
        if (PrevSelectedButton != Button)
        {
            SelectedCardNumber = allButton.IndexOf(Button);
            if (PrevSelectedButton != null)
            {
                PrevSelectedButton.GetComponent<ButtonSystem>().DisActive();
            }
            Button.GetComponent<ButtonSystem>().Activate();
            PrevSelectedButton = Button;
        }
        else
        {
            Button.GetComponent<ButtonSystem>().DisActive();
            SelectedCardNumber = -1;
            PrevSelectedButton = null;
        }

    }
    public void AddCardsOrReturn(GameObject carde)
    {
        if (TempoSelectedCards.Contains(carde))
        {
            TempoSelectedCards.Remove(carde);
            Deselected?.Invoke(carde);

        }
        else
        {
            TempoSelectedCards.Add(carde);
            Selected?.Invoke(carde);
        }
    }
    public void Continue()
    {
       CheckPlayerWon();
        ToTurnState();
    }
    #endregion

}

