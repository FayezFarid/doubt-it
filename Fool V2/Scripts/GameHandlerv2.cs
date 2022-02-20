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
    [SerializeField] public List<GameObject> allcards = new List<GameObject>();
    public List<GameObject> allcardsInGame = new List<GameObject>();
    public List<GameObject> CardsOnField = new List<GameObject>();
    public List<GameObject> allButton = new List<GameObject>();
    public List<GameObject> TempoSelectedCards = new List<GameObject>();
    public List<Transform> Areas = new List<Transform>();
    public List<OnlinePlayer> onlinePlayers = new List<OnlinePlayer>();
    private List<GameObject> TempoCardHolder = new List<GameObject>();
    #endregion
    #region Bools 
    [HideInInspector] [Networked] public NetworkBool isCardsOnField { get; set; }
    public bool Flipped = false;
    public bool ShowOtherPlayers { get; set; }
    [HideInInspector] [Networked] public NetworkBool CheckWith4 { get; set; }
    #endregion
    #region int Checkers
    [Header("Int for checking")]
    public int EachPlayerNB = 13;
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
    private GameStarter gameStarter;
    private TurnLogic turnLogic;
    private DebateComponent Debater;
    public CardsContainer cardContainer;
    public List<int> CheckList = new List<int>();
    public NetworkObject networkObject;
    [System.Serializable]
    public struct CardFalse :INetworkStruct
    {
        public GameObject Carde;
        public NetworkBool Correct;
        public CardFalse(GameObject cc, NetworkBool ccorr)
        {
            Carde = cc;
            Correct = ccorr;
        }
    }
    [HideInInspector] public Stringcontainer stringcontainer;
    #region  Events
    [HideInInspector] public UnityEvent<GameObject, Transform> SetParent;
    [HideInInspector] public UnityEvent<GameObject, Transform> OnConfirm;
    [HideInInspector] public UnityEvent<GameObject> ResetAreaPar;
    [HideInInspector] public UnityEvent<string> OnCardDestroy;
    [HideInInspector] public UnityEvent PopSmoke;
    [HideInInspector] public UnityEvent<OnlinePlayer, bool> FlipBurgers;
    [HideInInspector] public UnityEvent PreGameStart;
    [HideInInspector] public UnityEvent OnChoiceConfirm;
    [HideInInspector] public UnityEvent<List<CardFalse>> OnLostDebate;
    [HideInInspector] public UnityEvent<List<CardFalse>> OnWinDebate;
    [HideInInspector] public UnityEvent<List<CardFalse>, bool> OnOthersDebate;
    [HideInInspector] public UnityEvent<GameObject> Deselected;
    [HideInInspector] public UnityEvent<GameObject> Selected;
    [HideInInspector] public UnityEvent OnTurnChanged;
    [HideInInspector] public UnityEvent CleanUpButton;
    [HideInInspector] public UnityEvent OnShowOthersStart;
    #endregion 

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
    void checkCardsAreInPlace()
    {
        int Checker;
        for (int i = 0; i < CheckList.Count; i++)
        {
            Checker = 0;
            for (int j = 0; j < allcards.Count; j++)
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
            for (int i = allcards.Count - 1; i >= 0; i--)
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
    //Photon
    private void Start()
    {

        SetParent.AddListener(SetPARENT);
        //  ShowOtherPlayers = true;

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
        gameStarter = new GameStarter(this, Runner);
        turnLogic = new TurnLogic(this, Runner);
        Debater = new DebateComponent(this, Runner);
        #endregion
        Debug.Log(Runner.LocalPlayer);
        // Runner.AddCallbacks(this);
    }
    public void StartGame()
    {
        if (networkObject.HasStateAuthority)
            gameStarter.StartGamePre(Runner.ActivePlayers.ToList());
        GetMyPlayer().Area = Areas[0];
    }
    //public IEnumerator StartRpc(string MethodName, object[] para = null, int WaitTime = 1, RpcTarget target = RpcTarget.Others)
    //{
    //    yield return new WaitForSeconds(WaitTime);
    //    Pv.RPC(MethodName, target, para);
    //}

    public void DestroyCards(OnlinePlayer Player)
    {
        Player.Clear(CheckWith4);
    }
    public void SortPlayerCards()
    {
        foreach (OnlinePlayer item in onlinePlayers)
        {
            IEnumerable<GameObject> query = item.PlayerCards.OrderBy(Card => Card.GetComponent<OfflineCardManager>().EqNumber);
            item.PlayerCards = query.ToList();
        }
    }

    public void ConfirmChoice()
    {
        LastPlayedCards = TempoSelectedCards.Count;
        NetworkObject[] array = new NetworkObject[TempoSelectedCards.Count];
        for (int i = 0; i < TempoSelectedCards.Count; i++)
            array[i] = TempoSelectedCards[i].GetComponent<NetworkObject>();
        RPCLeaderConfirmChoice(SelectedCardNumber, array);
    }
    public void ChoosenDebate() => RPCChoosenDebate();
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPCChoosenDebate(RpcInfo info=default)=> Debater.ChoosenDebate(info.Source);
    public void IncTurnInt()
    {
        TurnInt = Extension.IncInt(TurnInt, onlinePlayers);
    }

    //others
    [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
    public void RPCVerifyChoiceForOther()
    {
        bool ChoicesAreRight = true;
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
        OnOthersDebate?.Invoke(falseCards, ChoicesAreRight);
    }
    [Rpc]
    public void RPCVerifyChoice([RpcTarget] PlayerRef player, bool ChoicesAreRight,CardFalse[] FalseCards)
    {
        Debug.Log("Rpc verify choice Player is =" + player.PlayerId);
        List<GameHandlerv2.CardFalse> falseCards = new List<CardFalse>();
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
        if (ChoicesAreRight) 
            OnLostDebate?.Invoke(falseCards);
        else OnWinDebate?.Invoke(falseCards);
        foreach (GameObject item in CardsOnField)
            //all
            RPCVerfyingChoice(item.GetComponent<NetworkObject>(), ChoicesAreRight);
        if (ChoicesAreRight)
            IncTurnInt();
        //gameHandler.Pv.RpcSecure("IncremenateTurnInt", RpcTarget.All, false);
        StartCoroutine(closeDead(ChoicesAreRight,player));
       //RPCloseDead(ChoicesAreRight);
    }
    //all
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCRemovePlayer(NetworkObject _player)
    {
        onlinePlayers.Remove(_player.GetComponent<OnlinePlayer>());
        //Runner.Despawn(GetMyPlayer().GetComponent<NetworkObject>());
    }
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
    //[PunRPC]
    //public void IncremenateTurnInt()
    //{

    //    if ((onlinePlayers.Count - 1) == TurnInt)
    //        TurnInt = 0;
    //    else TurnInt++;
    //    Debug.Log("Rpc IncTurn int Turnint= " + TurnInt);

    //}
    [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
    public void RPCToWaitState()
    {
      
            if (!ShowOtherPlayers)
                Flipped = false;
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
        if (ShowOtherPlayers)
            OnShowOthersStart?.Invoke();

    }
    //RPC target All
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCAddCard(NetworkObject Carde, int Playerindex) => onlinePlayers[Playerindex].AddCards(Carde.gameObject);
    //Rpc Other
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
    public void RPCAddToCardOnField(NetworkObject Card)
    {

        onlinePlayers[TurnInt].RemoveCard(Card.gameObject);
        SetParent?.Invoke(Card.gameObject, DontMindHim.transform);
        card.GetComponent<OfflineCardManager>().selectable = false;
        CardsOnField.Add(Card.gameObject);
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPCLeaderConfirmChoice(int selectedcard, NetworkObject[] toAdd)
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
        LastPlayedCards = LastPlayedCards;
        isCardsOnField = true;
        TempoSelectedCards.Clear();
        RPCchoiceConfirmedLast();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCchoiceConfirmedLast()
    {
        if (onlinePlayers[TurnInt].isLocalPlayer)
            OnChoiceConfirm?.Invoke();
        SwitchState(true);
       
    }
    //all
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCloseDead(NetworkBool ChoicesAreRight, RpcInfo info = default) => StartCoroutine(closeDead(ChoicesAreRight, info.Source));
    //all
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCVerfyingChoice(NetworkObject Obj, NetworkBool ChoicesAreRight)
    {

        TempoCardHolder.Add(Obj.gameObject);
        if (ChoicesAreRight)
        {
            onlinePlayers[TurnInt].AddCards(Obj.gameObject);
            Obj.gameObject.GetComponent<OfflineCardManager>().ChangeSelectable(false);
            //if (onlinePlayers[TurnInt].isLocalPlayer)
            //{
            //    SetParent?.Invoke(_pv.gameObject, P1area.transform);
            //}
        }
        else
        {

            Obj.gameObject.GetComponent<OfflineCardManager>().selectable = false;
            onlinePlayers[Extension.DecInt(TurnInt, onlinePlayers.Count)].AddCards(Obj.gameObject);
            if (Flipped)
                Obj.gameObject.GetComponent<OfflineCardManager>().FlipBurgers(false);
        }
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


    public void DestroyDuplicatesAll()
    {
        foreach (var item in onlinePlayers)
        {
            //DestroyCards(item);
            item.Clear(CheckWith4);
        }

    }
    IEnumerator closeDead(bool choicesIsRight, PlayerRef player)
    {
        isCardsOnField = false;
        SelectedCardNumber = -1;
        LastPlayedCards = 0;
        DebateOngoingTurns = 0;
        CardsOnField.Clear();
        SortPlayerCards();
        yield return new WaitForSeconds(4);
        DestroyDuplicatesAll();
        Debug.Log("closing dead after yield " + choicesIsRight);
        if (networkObject.HasStateAuthority)
            turnLogic.CheckPlayerWon();
        if (choicesIsRight)
        {
            SwitchState(false);
        }
        else
        {
            if (SearchForPlayer(player).isLocalPlayer)
            {
                ToTurnState();
            }

        }
        foreach (OnlinePlayer pplayer in onlinePlayers)
            pplayer.checkFornull();

    }
    public void ResetSelected()
    {
        foreach (GameObject item in TempoSelectedCards)
        {
            item.GetComponent<OfflineCardManager>().selectable = true;

        }
        TempoSelectedCards.Clear();
    }
    public void SetPARENT(GameObject card, Transform area) => card.transform.SetParent(area, false);
    public OnlinePlayer SearchForPlayer(int ActorNumber) => onlinePlayers.Where(player => player.number == ActorNumber).First();
    public OnlinePlayer SearchForPlayer(PlayerRef _player) => onlinePlayers.Where(player => player._player == _player).First();
    public OnlinePlayer GetMyPlayer() => onlinePlayers.Where(player => player._player == Runner.LocalPlayer).First();
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
        turnLogic.CheckPlayerWon();
        ToTurnState();
    }
    #endregion
   
}

