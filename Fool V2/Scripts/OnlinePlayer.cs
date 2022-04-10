using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Fusion;
using System.Linq;

[System.Serializable]
public class OnlinePlayer : NetworkBehaviour
{
   
    [Header("Local Player")]
    public bool isLocalPlayer;
    public List<CardStructure> PlayerCards; //{ get; private set; }
    [Networked(OnChanged =nameof(OnNetworkPlayerCardChange)), Capacity(52)]
    public NetworkArray<NetworkCard> NetworkPlayerCards { get; }
    [Networked]
    public int NetCardCount { get; set; }
    [Networked] public NetworkBool Won{ get; set; }
    [Networked] public NetworkBool isPlayerTurn { get; set; }
    [Networked] public int number{ get; set;}
    [Networked] public string Name { get; set; }
    [Networked(OnChanged =nameof(OnPlayerChange))] public PlayerRef _player { get; set; }
    public Transform Area;
    public Action<int>  OnCardDestroy;
    public Action<NetworkObject> OnCardRemoved;
    public Action<int> OnCardChange;
    public PlayerStats _playerstats;
    public bool HasDebugged=false;
    #region fusion OwnerShip
    private bool isOwner
    {
        get => Object.HasStateAuthority;
    }
    #endregion
    public static void OnPlayerChange(Changed<OnlinePlayer> Change)
    {
        Change.Behaviour.isLocalPlayer = Change.Behaviour._player == Change.Behaviour.Runner.LocalPlayer;
    }
    public void Awake()
    {
        PlayerCards = new List<CardStructure>();
    }
    private void OnDisable()
    {
        //_playerstats.UNHook(this);
    }
    public static void OnNetworkPlayerCardChange(Changed<OnlinePlayer> changed)
    {
        changed.Behaviour.PlayerCardChanged();
    }
    void PlayerCardChanged()
    {
        if (!CheckArrayIsAlright())
            return;

        SetCardsInPlace();
    }
    bool CheckArrayIsAlright()
    {
        int trueLength = 0;
       
        foreach (var item in NetworkPlayerCards)
        {
            if (item.EqNumber != -1)
                trueLength++;
        }
        if (NetCardCount != trueLength)
        {
            HasDebugged = true;
            Debug.LogError($"NetworkPlayerCards Error! NetcardCount{NetCardCount} == {trueLength}");
            return false;
        }
        return true;
    }
    void SetCardsInPlace()
    {
        if (NetCardCount == PlayerCards.Count)
            return;
        if (NetCardCount > PlayerCards.Count)
        {
            Debug.Log($"My man {number} Missing card {NetCardCount - PlayerCards.Count}");
            int count = PlayerCards.Count;
            int i = 0;
            while(PlayerCards.Count!=NetCardCount )
            {   
                // wildest debug log do not uncomment !!!!
                // Debug.Log($"Does netid={NetworkPlayerCards[count + i].ObjID}  {CheckIfCardExist(NetworkPlayerCards[count + i].ObjID)} Index= {i} ");
                if(!CheckIfCardExist(NetworkPlayerCards[i].ObjID))
                {
                    PlayerCards.Add(GameHandlerv2.Instance.GetCardByID(NetworkPlayerCards[count + i].ObjID));
                   // Debug.Log($" added Card to My man {number} Index= {i} ");
                }
                i++;
            }
            SortCards();
            if (_player == Runner.LocalPlayer)
                SetCardsToArea();

                
        }
        else
        if (NetCardCount < PlayerCards.Count)
        {
            Debug.Log($"My man {number} Have Extra card {NetCardCount - PlayerCards.Count}");
            int count = PlayerCards.Count;

            List<CardStructure> ToRemove = new List<CardStructure>();

            foreach (var item in PlayerCards)
            {
                if (GetCardIndex(item.Netid) == -1)
                {
                    ToRemove.Add(item);
                    Debug.Log($"Removed item {item}");
                }
            }
            foreach (var item in ToRemove)
            {
                PlayerCards.Remove(item);
            }
            SortCards();
        }
    }
    void SetCardsToArea()
    {
        if (Area is null)
            Area = GameHandlerv2.Instance.Areas[0];
        foreach (var item in PlayerCards)
        {
            item.transform.SetParent(Area, false);
        }


    }
    public void SetupPlayer(PlayerRef player)
    {
        //_playerstats.HookOnlinePlayerActions(this);
        
         Runner = FindObjectOfType<NetworkRunner>();
        _player = player;
        number = _player.PlayerId;
         Name = "Player " + (_player.PlayerId + 1);
        NetCardCount = 0;
        ClearNetCards();   
        //_playerstats.playername = Name;
        //_playerstats.SetPlayerName();
        //_playerstats.SetPlayerNumber();
    }
   
    public void AddCardServer(NetworkCard card)
    {
        NetCardCount++;
        
        NetworkPlayerCards.Set(NetCardCount-1, card);
    }
    public void AddCards(CardStructure card)
    {   if(isOwner)
        {
            AddCardServer(new NetworkCard(card.EqNumber,card.Netid));

        }
        PlayerCards.Add(card);  
        UpdatePlayerStats();
    }
    public void RemoveCardServer(int NetID)
    {
        int IndexOfCard = GetCardIndex(NetID);
        if (IndexOfCard == -1)
            return;
        NetCardCount--;
        NetworkPlayerCards.Set(IndexOfCard, new NetworkCard(-1, -1));
       // SortNetCards();
    }
    public void RemoveCard(CardStructure card)
    {
        if (isOwner)
        {
            RemoveCardServer(card.Netid);
        }
        RPCRemoveCard(card._Netobj);
        UpdatePlayerStats();
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCRemoveCard(NetworkObject card)
    {
       // throw new NotImplementedException();
         PlayerCards.Remove(GameHandlerv2.Instance.GetCardByObject(card));
           //SortCards();
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    void UpdatePlayerStats() 
    {
        if(_playerstats)
            _playerstats.playerCards = PlayerCards.Count; 
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPCCleanUpCards(NetworkObject[] cardsToDelete)
    {
        foreach (NetworkObject item in cardsToDelete)
        {
            OnCardRemoved?.Invoke(item);
           //PlayerCards.Remove(item.gameObject);
        }
        UpdatePlayerStats();
        SortCards();

    }
    public void Clear(NetworkBool CheckWith4)
    {
        SortCards();
        #region First Checks and Init
        int NbDup;
        if (CheckWith4)
            NbDup = 4;
        else NbDup = 2;
        if (PlayerCards.Count < NbDup)
            return;
        int i = 0;
        int j;
        List<NetworkObject> ToBeDeleted = new List<NetworkObject>();
        #endregion
        while (PlayerCards.Count - i >= NbDup)
        {
            bool Same = true;
            j = 0;
            while (j != PlayerCards[i].EqNumber)
            {
                j++;
                if (j > 13)
                    break;
            }
            for (int k = i; k < i + NbDup; k++)
            {
                if (PlayerCards[k].EqNumber != j)
                {
                    Same = false;
                    break;
                }

            }
            // Debug.Log("Card is " + j + " 4 duplicates is " + Same + "i =" + i);
            if (Same)
            {
                for (int Hk = i; Hk < i + NbDup; Hk++)
                {
                    ToBeDeleted.Add(PlayerCards[Hk]._Netobj);
                }
                OnCardDestroy?.Invoke(i);
                i += NbDup;
            }
            else i++;
            j++;
        }
        RPCCleanUpCards(ToBeDeleted.ToArray());
    }
  
    public void Enablecards(NetworkBool decision)
    {
        foreach (CardStructure carde in PlayerCards)
            carde.CardManager.ChangeSelectable(decision);
    }
    public NetworkBool CheckIFwon()
    {
        if (PlayerCards.Count == 0)
            return true;
        else return false;
    }
    public void checkFornull() { } //=> PlayerCards.RemoveAll(item => item == null);
    #region Array methods
    public void SortCards()
    {
        IEnumerable<CardStructure> query = PlayerCards.OrderBy(Card => Card.EqNumber);
        PlayerCards = query.ToList();
    }
    public void SetPlayerCardsTo(Transform area)
    {
        foreach (CardStructure item in PlayerCards)
            item._Netobj.transform.SetParent(area, false);

    }
    
  
    private void SortNetCards()
    {
        IEnumerable<NetworkCard> NonEmpty = NetworkPlayerCards.Where(Eq => Eq.EqNumber >= 0);
        IEnumerable<NetworkCard> query = NonEmpty.OrderBy(Number => Number.EqNumber);
       // NetworkCard[] QueryFinal = new NetworkCard[52];
        NetworkPlayerCards.CopyFrom(query.ToArray(), 0, NetCardCount);
        int QuerySize = query.ToArray().Length;
        Debug.Log($"Query size = {QuerySize}");
        for (int i = 0; i < QuerySize; i++)
        {
            NetworkPlayerCards.Set(i, new NetworkCard(-1, -1));
        }
    }
    void ClearNetCards()
    {

        for (int i = 0; i < NetworkPlayerCards.Length; i++)
        {
            NetworkPlayerCards.Set(i, new NetworkCard(-1, -1));
        }

    }
    /// <summary>
    /// Get from networkPlayer cards index using NetId
    /// </summary>
    /// <param name="NetID"></param>
    /// <returns> return >=0 if it exist else -1</returns>
    private int GetCardIndex(int NetID)
    {   
        // must be this rough
        int i = 0;
        foreach (var item in NetworkPlayerCards)
        {
            if (item.ObjID == NetID)
                return i;
            i++;
        }
        return -1;
    }
    /// <summary>
    /// Check if Card Exists by netId in Players cards
    /// </summary>
    /// <param name="netID"></param>
    /// <returns> True if Card exist false other wise</returns>
    bool CheckIfCardExist(int netID)
    {
        IEnumerable<CardStructure> QUERY = PlayerCards.Where(_Card => _Card.Netid == netID);
        if (QUERY is null || !QUERY.Any())
            return false;
        else
        {
            if (QUERY.First().Netid == -1)
                return false;
            else return true;
        }

    }

    #endregion
}


