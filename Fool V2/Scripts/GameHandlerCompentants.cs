
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;

using Random = UnityEngine.Random;
public class TurnLogic
{
    private GameHandlerv2 gameHandler;
    private NetworkRunner Runner;
    public TurnLogic(GameHandlerv2 game,NetworkRunner runner)
    {
        gameHandler = game;
        Runner = runner;
    }
    public void CheckPlayerWon()
    {
        List<OnlinePlayer> tempoList = new List<OnlinePlayer>();
        Debug.Log("CHECKING NIGGA WINNING");
        foreach (OnlinePlayer item in gameHandler.onlinePlayers)
        {
            if (item.CheckIFwon())
            {
                gameHandler.PlayersWonOrder += item.Name;
                Debug.Log("aYO CHECK THIS OUT DIS nigga just won " + item);
                gameHandler.RPCRemovePlayer(item.GetComponent<NetworkObject>());
              //  gameHandler.StartRpc("CleanUpPlayers");
                break;
                // Players.Remove(pllayer);

            }

        }

        if (gameHandler.onlinePlayers.Count <= 1)
        {
            UnityEngine.Object.Destroy(GameObject.Find("Canvas"));
            Debug.LogError("Ma nigga game is OVER");
            return;
        }
    }
}
public class DebateComponent
{
    public DebateComponent(GameHandlerv2 game,NetworkRunner runner)
    {
        gameHandler = game;
        Runner = runner;
    }
    private GameHandlerv2 gameHandler;
    private NetworkRunner Runner;
    public void ChoosenDebate(PlayerRef _playerSource)
    {
        bool ChoicesAreRight = true;
        // Not Really all false card also for true cards;
        List<GameHandlerv2.CardFalse> falseCards = new List<GameHandlerv2.CardFalse>();
        Debug.Log("_player Source =" + gameHandler.SearchForPlayer(_playerSource).isLocalPlayer);
        Debug.Log("SelectedCardNumber; is = " + gameHandler.SelectedCardNumber);

        for (int i = gameHandler.CardsOnField.Count - gameHandler.LastPlayedCards; i < gameHandler.CardsOnField.Count; i++)
        {
            //Debug.Log(i + "   Choosen debate");
            if (gameHandler.CardsOnField[i].GetComponent<OfflineCardManager>().EqNumber != gameHandler.SelectedCardNumber)
            {
                ChoicesAreRight = false;
                Debug.Log("FOUND one ya7chi fih");
                falseCards.Add(new GameHandlerv2.CardFalse(gameHandler.CardsOnField[i], false));
            }
            else falseCards.Add(new GameHandlerv2.CardFalse(gameHandler.CardsOnField[i], true));
        }
        gameHandler.RPCVerifyChoice(_playerSource,ChoicesAreRight, falseCards.ToArray());
        Debug.Log("choices correct is= " + ChoicesAreRight);
        //other
        // gameHandler.RPCVerifyChoiceForOther();
        //RPCloseDead(ChoicesAreRight);
    }
   

}
public class GameStarter
{
    public GameHandlerv2 gameHandler;
    public GameStarter(GameHandlerv2 game,NetworkRunner runner)
    {
        gameHandler = game;
        Runner = runner;
    }
    private NetworkRunner Runner;
    public void StartGamePre(List<PlayerRef> SortedPlayers)
    {
        gameHandler.CheckWith4 = true;
        gameHandler.Flipped = false;
        if (!gameHandler.ShowOtherPlayers)
        {
            //  SetParent.RemoveAllListeners();
        }
        gameHandler.PreGameStart?.Invoke();
        if (gameHandler.networkObject.HasStateAuthority)
        {
            
            foreach (PlayerRef item in SortedPlayers)
            {
                NetworkObject obj = Runner.Spawn(gameHandler.PlayerObject, new Vector3(0, 0, 0), Quaternion.identity);
                obj.GetComponent<OnlinePlayer>().SetupPlayer(item);
                //obj.GetComponent<NetworkObject>().State(item);
                gameHandler.onlinePlayers.Add(obj.GetComponent<OnlinePlayer>());
                //Rpc Target OTher
                gameHandler.RPCAddPlayer(obj);
            }
            StartGameV2();
            gameHandler.gameState = new OnlineTurnState(gameHandler.onlinePlayers[0], gameHandler);
            gameHandler.gameState.ManageState();

        }
        //RpcTarget Other
        gameHandler.RPCToWaitState();

    }
    public void StartGameV2()
    {

        Debug.Log("Start Game v2");
        AddCardsV3();
        ///important
        for (int i = 0; i < gameHandler.allcards.Count; i++)
        {
            swap(ref gameHandler.allcards, Random.Range(0, gameHandler.allcards.Count), Random.Range(0, gameHandler.allcards.Count));
        }
        int PlayerIndex = 0;
        foreach (GameObject item in gameHandler.allcards)
        { //RPC target All
            gameHandler.RPCAddCard(item.GetComponent<NetworkObject>(), PlayerIndex);
            PlayerIndex=Extension.IncInt(PlayerIndex, gameHandler.onlinePlayers.Count);
        }
        ///End of important
        //RpcTarget All
        gameHandler.RPCSetParent();
        gameHandler.DestroyDuplicatesAll();
        if (gameHandler.Flipped)
            for (int i = 1; i < gameHandler.onlinePlayers.Count; i++)
                gameHandler.onlinePlayers[i].flipMyCards(false);
        Debug.Log("All cards Count" + gameHandler.allcards.Count);
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
        if (gameHandler.onlinePlayers.Count < 4 && gameHandler.CheckWith4)
        {
            VirtualPlayersNumber = 4;
        }
        else VirtualPlayersNumber = gameHandler.onlinePlayers.Count + gameHandler.onlinePlayers.Count % 2;
        Debug.Log("Virtual players number =" + VirtualPlayersNumber);
        string CardType;
        for (int i = 0; i < gameHandler.EachPlayerNB; i++)
        {
            CardType = GameHandlerv2.AllowedChar[Random.Range(0, 3)];
            gameHandler.CheckList.Add(i);
            List<Sprite> CurrentList = gameHandler.cardContainer.SpriteContainer[CardType];
            for (int f = 0; f < VirtualPlayersNumber; f++)
            {
                NetworkObject cardee = Runner.Spawn(gameHandler.EmptyCard, new Vector2(0, 0), Quaternion.identity);
                //cardee.GetComponent<Image>().sprite = CurrentList[i];
                cardee.GetComponent<OfflineCardManager>().CurrentSprite = cardee.GetComponent<Image>().sprite;
                cardee.GetComponent<OfflineCardManager>().EqNumber = i;
               
                //gameHandler.allcards.Add(cardee.gameObject);
                gameHandler.RPCSetCard(cardee, i, CardType);
            }
        }
        Debug.Log(gameHandler.allcards.Count);
    }
}
