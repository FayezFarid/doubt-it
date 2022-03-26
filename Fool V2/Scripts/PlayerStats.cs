using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
public class PlayerStats : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI Playername;
    [SerializeField] private TextMeshProUGUI Cardnumber;
    [Networked(OnChanged = nameof(SetPlayernumber))] public int playerCards { get; set; }
    [Networked(OnChanged =nameof(SetPlayername))] public string playername { get; set; }
    public void SetPlayerNumber()=> Cardnumber.text = playerCards.ToString();
    public void SetPlayerName() => Cardnumber.text = playername;
    private OnlinePlayer _player;
    
    public static void SetPlayername(Changed<PlayerStats> Changed)
    {
        Debug.LogWarning("Player name changed");
        Changed.Behaviour.SetPlayerName();
    }
    public static void SetPlayernumber(Changed<PlayerStats> Changed)
    {
        Debug.LogWarning("Player cards changed");
        Changed.Behaviour.SetPlayerNumber();
    }
    public void SetupStat(OnlinePlayer player)
    {
        _player = player;
        playername = _player.Name;
        playerCards = _player.PlayerCards.Count;
    }
    
    //public void HookOnlinePlayerActions(OnlinePlayer _player)
    //{
    //   _player.OnCardChange +
    //}
    //public void UNHook(OnlinePlayer _player)
    //{
        
    //}
}
