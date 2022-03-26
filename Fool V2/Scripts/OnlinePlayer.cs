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
    public NetworkBool isLocalPlayer;
    public List<GameObject> PlayerCards = new List<GameObject>();
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
    public static void OnPlayerChange(Changed<OnlinePlayer> Change)
    {
        Change.Behaviour.isLocalPlayer = Change.Behaviour._player == Change.Behaviour.Runner.LocalPlayer;
    }
    public void Awake()
    { 
    }
    private void OnDisable()
    {
        //_playerstats.UNHook(this);
    }
    public void SetupPlayer(PlayerRef player)
    {
        //_playerstats.HookOnlinePlayerActions(this);
        
         Runner = FindObjectOfType<NetworkRunner>();
        _player = player;
        number = _player.PlayerId;
         Name = "Player " + _player.PlayerId + 1;
        //_playerstats.playername = Name;
        //_playerstats.SetPlayerName();
        //_playerstats.SetPlayerNumber();
    }
    public void AddCards(GameObject card) { PlayerCards.Add(card);  UpdatePlayerStats();}
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCRemoveCard(NetworkObject card) { PlayerCards.Remove(card.gameObject); }
    public void RemoveCard(GameObject card)
    {
        RPCRemoveCard(card.GetComponent<NetworkObject>());
        UpdatePlayerStats();
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
            PlayerCards.Remove(item.gameObject);
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
            while (j != PlayerCards[i].GetComponent<OfflineCardManager>().EqNumber)
            {
                j++;
                if (j > 13)
                    break;
            }
            for (int k = i; k < i + NbDup; k++)
            {
                if (PlayerCards[k].GetComponent<OfflineCardManager>().EqNumber != j)
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
                    ToBeDeleted.Add(PlayerCards[Hk].GetComponent<NetworkObject>());
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
        foreach (GameObject carde in PlayerCards)
            carde.GetComponent<OfflineCardManager>().ChangeSelectable(decision);
    }
    public NetworkBool CheckIFwon()
    {
        if (PlayerCards.Count == 0)
            return true;
        else return false;
    }
    public void SortCards()
    {
        IEnumerable<GameObject> query = PlayerCards.OrderBy(Card => Card.GetComponent<OfflineCardManager>().EqNumber);
        PlayerCards = query.ToList();
    }
    public void SetPlayerCardsTo(Transform area)
    {
        foreach (GameObject item in PlayerCards)
            item.transform.SetParent(area, false);

    }
    public void checkFornull() { } //=> PlayerCards.RemoveAll(item => item == null);
}


