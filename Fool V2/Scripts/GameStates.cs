using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class OnlineGameState
{
    public OnlinePlayer CurrentPlayerTurn;
    public OnlineGameStateNum _state;
    public GameHandlerv2 gameHandler;
    public abstract void ManageState();
    public abstract OnlineGameState NextState(bool won = false);

}
public class OnlineTurnState : OnlineGameState
{
    public static Action OnURTurn;
    public override void ManageState()
    {
        Debug.Log("Changed Turn.  now its Player " + gameHandler.TurnInt);
        CurrentPlayerTurn.Enablecards(true);
        gameHandler.OnTurnChanged?.Invoke();
        VisualHandler.Instance.RemoveSmokeScreen();
        OnURTurn?.Invoke();
   
    }
    public override OnlineGameState NextState(bool won = false) { Debug.LogError("Next state On onlineTurnState Has been called"); return null; }
    public OnlineTurnState(OnlinePlayer _player, GameHandlerv2 game)
    {
        CurrentPlayerTurn = _player;
        gameHandler = game;
        _state = OnlineGameStateNum.TurnState;
    }
}

public class OnlineWaitState : OnlineGameState
{
    /// <param name="CardsOnField"></param>
    /// <returns></returns>
    public override OnlineGameState NextState(bool CardsOnField) { Debug.LogError("Next state On OnlineWaitState Has been called"); return null; }
    public override void ManageState()
    {
        gameHandler.GetMyPlayer().Enablecards(false);
        gameHandler.PopSmoke?.Invoke();
    }
    public OnlineWaitState(OnlinePlayer _player, GameHandlerv2 game)
    {
        CurrentPlayerTurn = _player;
        gameHandler = game;
        _state = OnlineGameStateNum.WaitState;
    }
}

public class OnlineDebateState : OnlineGameState
{
    public override OnlineGameState NextState(bool won) { Debug.LogError("Next state On onlineDebateState Has been called"); return null; }

    public override void ManageState()
    {
        Debug.Log("Manage state Debate State");
        VisualHandler.Instance.RemoveSmokeScreen();

    }
    public OnlineDebateState(OnlinePlayer _player, GameHandlerv2 game)
    {
        CurrentPlayerTurn = _player;
        gameHandler = game;
        _state = OnlineGameStateNum.DebateState;
    }

}