using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Fusion;

[System.Serializable]
public class OnlinePlayer : NetworkBehaviour
{
    [Header("Local Player")]
    public NetworkBool isLocalPlayer;
    public List<GameObject> PlayerCards = new List<GameObject>();
    public List<GameObject> ToBeRemovedCards = new List<GameObject>();
    [Networked] public NetworkBool Won{ get; set; }
    [Networked] public NetworkBool isPlayerTurn { get; set; }
    [Networked] public int number{ get; set;}
    [Networked] public string Name { get; set; }
    [Networked(OnChanged =nameof(OnPlayerChange))] public PlayerRef _player { get; set; }
    public Transform Area;
    public Action<int>  OnCardDestroy;
    public static void OnPlayerChange(Changed<OnlinePlayer> Change)
    {
        Change.Behaviour.isLocalPlayer = Change.Behaviour._player == Change.Behaviour.Runner.LocalPlayer;
    }
    public void SetupPlayer(PlayerRef player)
    {
       
        Runner = FindObjectOfType<NetworkRunner>();
        _player = player;
       // isLocalPlayer = _player == Runner.LocalPlayer;
        number = _player.PlayerId;
        //if (_player.NickName == string.Empty || _player.NickName == null)
            Name = "Player " + _player.PlayerId + 1;
        //else Name = _player.NickName;
    }
    public bool ReadyToRemoveCards() => ToBeRemovedCards.Count == 0;
    public void CleanUpCards()
    {
        foreach (GameObject item in ToBeRemovedCards)
            PlayerCards.Remove(item);

    }
    public void AddCards(GameObject card)=>  PlayerCards.Add(card);
    public void RemoveCard(GameObject card)
    {
        RPCRemoveCard(card.GetComponent<NetworkObject>());
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCRemoveCard(NetworkObject card) => PlayerCards.Remove(card.gameObject);
    public void Clear(NetworkBool CheckWith4)
    {

        RpcClearDuplicates(CheckWith4);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcClearDuplicates(NetworkBool CheckWith4)
    {
        int NbDup;
        if (CheckWith4)
            NbDup = 4;
        else NbDup = 2;
        if (PlayerCards.Count < NbDup)
            return;
        int i = 0;
        int j;
        List<GameObject> ToBeDeleted = new List<GameObject>();
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
                    ToBeDeleted.Add(PlayerCards[Hk]);
                }
                OnCardDestroy?.Invoke(i);
                i += NbDup;
            }
            else i++;
            j++;
        }
        //foreach (var item in ToBeDeleted)
        //{
        //    Debug.Log("Card is " + item.GetComponent<OfflineCardManager>().EqNumber);
        //}
        ToBeRemovedCards = ToBeDeleted;
        CleanUpCards();
     
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
    public void flipMyCards(NetworkBool flipto)
    {
        foreach (GameObject card in PlayerCards)
            card.GetComponent<OfflineCardManager>().FlipBurgers(flipto);
    }
    public void SetPlayerCardsTo(Transform area)
    {
        foreach (GameObject item in PlayerCards)
            item.transform.SetParent(area, false);

    }
    public void checkFornull() { } //=> PlayerCards.RemoveAll(item => item == null);
}
public enum OnlineGameStateNum
{
    TurnState,
    WaitState,
    DebateState
}

