using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
public class VisualHandler : NetworkBehaviour
{
    public Transform MidCards;
    public Transform MidArea;
    public Transform canvas;
    public Transform DontMindHim;
    public Transform DeadAreaContent;
    public GameObject Mid;
    public GameObject Anoucmenet;
    public TextMeshProUGUI Log;
    public GameObject SmokeScreen;
    public GameObject facts;
    public GameObject P1area;
    public GameObject DeadArea;
    public GameObject StartPanel;
    public GameObject ChoicesStep2;
    [Header("Mid/choice")]
    public GameObject choices;
    public TextMeshProUGUI Nbcards;
    [Header("Top Priority references")]
    public Stringcontainer stringcontainer;
    public GameHandlerv2 gameHandler;
    public GameObject SettingMenu;
    [Header("Holds Card For The Animation")]
    public GameObject Holder;
    public GameObject YourTurnGO;

    [Header("Change Sprite According To Whoever Won")]
    public Image Consquences;
    public Sprite WonSprite;
    public Sprite LostSprite;
    //Visual Variables
    List<GameObject> spawnedHolders = new List<GameObject>();

    // Instance
    private static VisualHandler instance;
    public static VisualHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<VisualHandler>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = "GameHandler";
                    instance = obj.AddComponent<VisualHandler>();
                }

            }

            return instance;
        }
    }
    public override void Spawned()
    {
        StartPanel.transform.GetChild(0).GetComponent<Button>().interactable = gameHandler.networkObject.HasStateAuthority;
        //StartPanel.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { gameHandler.StartGame(); });
    }
    void OnEnable()
    {
        gameHandler.Deselected.AddListener(Deselection);
        gameHandler.Selected.AddListener(Selected);
        gameHandler.PopSmoke.AddListener(PoppingSmoke);
        gameHandler.FlipBurgers.AddListener(BurgerFLiper);
        gameHandler.PreGameStart.AddListener(PreStart);
        gameHandler.SetParent.AddListener(SetPARENT);
        gameHandler.OnChoiceConfirm.AddListener(ChoicesConfirm);
        gameHandler.OnLostDebate.AddListener(LostDebate);
        gameHandler.OnWinDebate.AddListener(WonDebate);
        gameHandler.OnTurnChanged.AddListener(TurnChanged);
        gameHandler.OnOthersDebate.AddListener(OnAnotherPlayerBussines);
        gameHandler.OnShowOthersStart.AddListener(OnShowOther);
        OnlineTurnState.OnURTurn += YourTurn;
    }
    void OnShowOther()
    {
        List<OnlinePlayer> TempoList = gameHandler.onlinePlayers;
        List<Transform> TempoListTransform = gameHandler.Areas;
        int MyPlayerIndex = TempoList.IndexOf(gameHandler.GetMyPlayer());
         MyPlayerIndex = Extension.IncInt(MyPlayerIndex, TempoList.Count);
        int AreaIndex = 1;
        for (int i = 0; i < TempoList.Count-1; i++)
        {
            if (!TempoList[MyPlayerIndex].isLocalPlayer)
            {
                Debug.Log("Area Index =" + AreaIndex);
                TempoList[MyPlayerIndex].SetPlayerCardsTo(TempoListTransform[AreaIndex]);
                TempoList[MyPlayerIndex].Area = TempoListTransform[AreaIndex];
                MyPlayerIndex = Extension.IncInt(MyPlayerIndex, TempoList.Count);
                AreaIndex = Extension.IncInt(AreaIndex, TempoListTransform.Count);
                
                if (AreaIndex == 0)
                    AreaIndex++;
            }
            else
                TempoList[MyPlayerIndex].Area = TempoListTransform[0];

        }
    }
    private void OnDisable()
    {
        foreach (OnlinePlayer item in gameHandler.onlinePlayers)
        {
            item.OnCardDestroy -= OnDestroyCard;
        }
        OnlineTurnState.OnURTurn -= YourTurn;
    }
    void OnAnotherPlayerBussines(List<GameHandlerv2.CardFalse> falseCards,bool choicesisRight)
    {
        //string SelectedString = _player.Name + " Has";
        //if (choicesisRight)
        //    SelectedString += "Lost Debate";
        //else
        //    SelectedString += "Won Debate";
        //SmokeScreen.GetComponentInChildren<TextMeshProUGUI>().text = SelectedString;
        CreateGlowingCards(falseCards);
        DeadArea.transform.GetChild(2).gameObject.SetActive(true);
        DeadArea.transform.SetAsLastSibling();
        if (choicesisRight)
        DeadArea.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = gameHandler.onlinePlayers[gameHandler.TurnInt].Name+"Has Lost Debate";
        else DeadArea.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = gameHandler.onlinePlayers[gameHandler.TurnInt].Name + "Has Won Debate";
        StartCoroutine(closeDeadDelayed(falseCards));
        StartCoroutine(ReturnToNormal());
    }
    IEnumerator closeDeadDelayed(List<GameHandlerv2.CardFalse> cardFalses)
    {
        yield return new WaitForSeconds(4);
        try
        {
            foreach (GameHandlerv2.CardFalse item in cardFalses)
                item.Carde.transform.GetChild(0).GetComponent<Image>().enabled = false;
        }
       
        finally
        {
            DeadArea.transform.GetChild(2).gameObject.SetActive(false);
            DeadArea.transform.SetAsFirstSibling();
            ResetCardToPostion();
        }

        // DeadArea.SetActive(false);
    }
    public void ResetCardToPostion()
    {
        foreach (OnlinePlayer item in gameHandler.onlinePlayers)
        {
            foreach (GameObject itemm in item.PlayerCards)
                itemm.transform.SetParent(item.Area);

        }

    }
    IEnumerator ReturnToNormal()
    {
      
        yield return new WaitForSeconds(4.1f);
        gameHandler.gameState.ManageState();
    }
    void YourTurn()
    {
        YourTurnGO.transform.SetAsLastSibling();
        YourTurnGO.SetActive(true);
        StartCoroutine(closeYourTurn());
    }
    IEnumerator closeYourTurn()
    {
        yield return new WaitForSeconds(2);
        YourTurnGO.GetComponent<Animator>().SetTrigger("Exit");
    }
    void TurnChanged()
    {
      
        //if (gameHandler.Flipped)
        //    foreach (OnlinePlayer pplayer in Players)
        //    {
        //        if (!pplayer.isPlayerTurn)
        //            pplayer.flipMyCards(false);
        //        else pplayer.flipMyCards(true);
        //        //  FlipBurgers?.Invoke(pplayer);
        //    }
    }
    void TurnCardsDeClockWise(List<GameObject> PlayerCards, Transform area)
    {
        foreach (GameObject card in PlayerCards)
            card.transform.SetParent(area);
    }
    void Selected(GameObject carde)
    {
        if (!Mid.activeInHierarchy)
            Mid.SetActive(true);
        carde.transform.SetParent(MidArea.transform, false);
    }
    void PreStart()
    {
        //  Name.SetActive(true);
        Destroy(StartPanel);
        
        foreach (OnlinePlayer item in gameHandler.onlinePlayers)
        {
            item.OnCardDestroy += OnDestroyCard;
        }
        if (!gameHandler.ShowOtherPlayers)
        {
           P1area.transform.localPosition = new Vector2(0, P1area.transform.localPosition.y + 10);
            Mid.transform.localPosition = new Vector2(0, Mid.transform.localPosition.y + 50);
            Mid.transform.GetChild(3).localPosition = new Vector2(0, -200);
            DeadArea.transform.localPosition = new Vector2(0, DeadArea.transform.localPosition.y);
        }
    }
    void LostDebate(List<GameHandlerv2.CardFalse> cardFalses)
    {
        string SelectedString = stringcontainer.LangPack[gameHandler.lang].ContainedString[0].Replace("{0}", gameHandler.CardsOnField.Count.ToString());
        DeadArea.transform.GetChild(2).gameObject.SetActive(true);
        DeadArea.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = SelectedString;
        DeadArea.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
        Consquences.sprite = LostSprite;
        CreateGlowingCards(cardFalses);
        StartCoroutine(closeDeadDelayed(cardFalses));
    }
    void WonDebate(List<GameHandlerv2.CardFalse> cardFalses)
    {

        string SelectedString = stringcontainer.LangPack[gameHandler.lang].ContainedString[1].Replace("{0}", gameHandler.CardsOnField.Count.ToString());
        DeadArea.transform.GetChild(2).gameObject.SetActive(true);
        DeadArea.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().text = SelectedString;
        DeadArea.transform.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().color = Color.green;
        Consquences.sprite = WonSprite;
        CreateGlowingCards(cardFalses);
        StartCoroutine(closeDeadDelayed(cardFalses));
    }
    void CreateGlowingCards(List<GameHandlerv2.CardFalse> cardFalses)
    {

        foreach (GameHandlerv2.CardFalse item in cardFalses)
        {
            item.Carde.GetComponent<OfflineCardManager>().FlipBurgers(true);
            item.Carde.transform.GetChild(0).GetComponent<Image>().enabled = true;
            if (item.Correct)
                item.Carde.transform.GetChild(0).GetComponent<Image>().color = Color.green;
            else item.Carde.transform.GetChild(0).GetComponent<Image>().color = Color.red;
            item.Carde.transform.SetParent(DeadAreaContent, false);
        }
    }
    void Deselection(GameObject carde)=>  carde.transform.SetParent(P1area.transform, false);
    void ChoicesConfirm()
    {
        Debug.Log("Choice confirmed");
        string SelectedString = "<color=red>"+ Extension.translateInt(gameHandler.SelectedCardNumber)+"</color>";
        DeadArea.transform.SetAsLastSibling();
        DeadArea.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = gameHandler.LastPlayedCards + " cards of type " + SelectedString;
        if (gameHandler.PrevSelectedButton != null)
        {
            gameHandler.PrevSelectedButton.GetComponent<ButtonSystem>().DisActive();
            gameHandler.PrevSelectedButton = null;      
        }

    }
   
    void OnReturnVisual(List<GameObject> lalist)
    {
        foreach (GameObject item in lalist)
        {
          
            item.transform.SetParent(P1area.transform, false);
            item.GetComponent<OfflineCardManager>().selectable = true;

        }
            
        
    }
     void SetPARENT(GameObject card, Transform area)
    {
      
        if (area == null)
            Debug.LogError("area null!!!!!!!");
       
        card.transform.SetParent(area, false);
    }

    private void OnDestroyCard(int EqNumber)
    {

        Log.text += Extension.translateInt(EqNumber) + " Has been Destroyed\n";

        Anoucmenet.transform.SetAsLastSibling();
        Anoucmenet.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            stringcontainer.LangPack[gameHandler.lang].ContainedString[2].Replace("{0}", Extension.translateInt(EqNumber));
        StartCoroutine(kms());
    }
    IEnumerator kms()
    {
        yield return new WaitForSeconds(2);
        Anoucmenet.transform.SetAsFirstSibling();
    }
    private void PoppingSmoke()
    {
        string SelectedString;
       
        TextHolder _txt = stringcontainer.LangPack[gameHandler.lang];
        string plname;
        if (gameHandler.onlinePlayers[gameHandler.TurnInt].Name != "" && gameHandler.onlinePlayers[gameHandler.TurnInt].Name != null)
            plname = gameHandler.onlinePlayers[gameHandler.TurnInt].Name;
        else plname = "Player "+gameHandler.onlinePlayers[gameHandler.TurnInt].number.ToString();
        if (gameHandler.CardsOnField.Count == 0)
            SelectedString = "It's <color=red>" + plname + "</color> turn"; 
        else 
        {
            SelectedString = _txt.ContainedString[3];
            SelectedString = SelectedString.Replace("{0}", plname);
            SelectedString = SelectedString.Replace("{1}", gameHandler.CardsOnField.Count.ToString());
            SelectedString = SelectedString.Replace("{2}", Extension.translateInt(gameHandler.SelectedCardNumber));
        }
        SmokeScreen.transform.SetAsLastSibling();
        SmokeScreen.GetComponentInChildren<TextMeshProUGUI>().text = SelectedString;
    }
    public void Showstats()
    {
        facts.transform.SetAsLastSibling();

        int a = 0;
        string TextTobeinsert = "It is Player " + (gameHandler.TurnInt + 1).ToString() + " Turn \n";
        foreach (OnlinePlayer pplayer in gameHandler.onlinePlayers)
        {
            a += pplayer.PlayerCards.Count;
            TextTobeinsert += "Player " + pplayer.number + " has " + pplayer.PlayerCards.Count + " cards \n";
        }

        TextTobeinsert += "There is " + a + " On the Field \n" + (52 - a).ToString() + "has been discarded";
        facts.GetComponentInChildren<TextMeshProUGUI>().text = TextTobeinsert;
    }
    void BurgerFLiper(OnlinePlayer Lejouer, bool flipto)
    {
        Lejouer.flipMyCards(flipto);
    }
    public void ReturnFacts() => facts.transform.SetAsFirstSibling();

    public void RemoveDestroyedButtons()
    {

        bool exist;
        for (int i = 0; i < gameHandler.EachPlayerNB; i++)
        {
            exist = false;
            foreach (GameObject item in gameHandler.allcards)
            {
                if (item != null)
                {
                    if (item.GetComponent<OfflineCardManager>().EqNumber == i)
                    {
                        exist = true;
                        break;
                    }

                }
            }
            if (!exist)
                gameHandler.allButton[i].SetActive(false);
        }


    }
   
    public void RemoveSmokeScreen()
    {
        SmokeScreen.transform.SetAsFirstSibling();
   
    }
    #region ASsigned to buttons
    public void Continue()
    {
        gameHandler.Continue();
        DeadArea.transform.SetAsFirstSibling();

    }
    public void Activate()
    {
        if (gameHandler.isCardsOnField)
        {
            Confirm();
            return;
        }
        choices.SetActive(true);
       RemoveDestroyedButtons();
        Nbcards.text = "You have selected  " + MidCards.childCount.ToString() + " Cards";
    }
    public void Confirm()
    {
        if (gameHandler.SelectedCardNumber == -1)
            return;
        gameHandler.ConfirmChoice();
        choices.SetActive(false);
        Mid.SetActive(false);
      //  Debug.Log("Confirm to activate is active " + Mid.active);

    }
    public void ReturnFirst()
    {
        List<GameObject> tempo = new List<GameObject>();
        foreach (GameObject item in gameHandler.TempoSelectedCards)
            tempo.Add(item) ;
        gameHandler.ResetSelected();
        OnReturnVisual(tempo);
        Mid.SetActive(false);

    }
    public void Debate() => gameHandler.ChoosenDebate();
    public void returnSecond() => choices.SetActive(false);
    public void OpenCloseSettings()
    {
        SettingMenu.transform.SetAsLastSibling();
        SettingMenu.SetActive(true);
    }
    public void CloseSettings()
    {
        SettingMenu.transform.SetAsFirstSibling();
        SettingMenu.SetActive(false);
    }
    public void Exit()
    {
        Runner.Disconnect(Runner.LocalPlayer);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    #endregion
}
