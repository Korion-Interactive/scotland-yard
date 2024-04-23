using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Korion.ScotlandYard.Input;

public class DrawStartStation : MonoBehaviour {

    const string IDLE_ICON = "notification_icon";
    const string LOAD_ICON = "loading_icon";
    
    public GameObject[] StartStationCards;

    //public event System.Action<PlayerSetup, Identifier> CardSelected;

    public GameObject TopBar;

    public UISprite Icon;

    public Color IconColorNormal, IconColorLoading;

    public Camera Cam;

    private UISprite sprite;


    private List<GameObject> startStationList = new List<GameObject>();

    private GameObject currentCard;

    private int currentPlayerID;

    private bool functionCalled = false;

    private PlayerSetup player;

    public bool IsAvailable { get { return startScript && currentCard == null; } }
    private bool startScript = false;

	// Use this for initialization
	void OnEnable ()
    {
        Reset();
	}
    public void Reset()
    {
        this.transform.parent.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(CoReset());
    }
    private IEnumerator CoReset()
    {
        startScript = false;
        currentPlayerID = 0;
        currentCard = null;
        functionCalled = false;

        iTween.Stop();

        Icon.spriteName = IDLE_ICON;
        Icon.color = IconColorNormal;
        Icon.gameObject.SetComponentsEnabled<RotatingLoadingIndicator>(false);

        List<Vector3> cardPositions = new List<Vector3>();
        startStationList.Clear();

        foreach (GameObject tempPos in StartStationCards)
        {
            cardPositions.Add(tempPos.transform.position);
            startStationList.Add(tempPos);
        }

        foreach (var card in StartStationCards)
        {
            int tempRandom = 0;
            tempRandom = Random.Range(0, cardPositions.Count);
            card.transform.position = cardPositions[tempRandom];
            cardPositions.Remove(cardPositions[tempRandom]);

            card.transform.GetChildByName("MrX").gameObject.SetActive(false);
            card.transform.GetChildByName("Frontside").gameObject.SetActive(false);
            card.transform.GetChildByName("Backside").gameObject.SetActive(true);
            card.transform.localScale = Vector3.one;
        }

        yield return new WaitForSeconds(1);

        startScript = true;
    }
	
	// Update is called once per frame
	void Update () {

        if (!startScript || currentCard != null || currentPlayerID > GameSetupBehaviour.Instance.LastPlayerID())
        {
            return;
        }

        player = GameSetupBehaviour.Instance.GetPlayer(currentPlayerID);

        sprite = TopBar.transform.GetChildByName("PlayerActor").GetComponent<UISprite>();
        sprite.spriteName = player.Color.GetActorSpriteName();
        TopBar.transform.GetChildByName("Label").GetComponent<LabelTranslator>().SetTextWithStaticParams("select_card_player", player.DisplayName);

        if (player.Controller == PlayerController.Human)
        {
            if(UICamera.selectedObject == null || !UICamera.selectedObject.activeSelf)
            {
                // KORION: Select a new random card for each human player! 
                UICamera.currentScheme = UICamera.ControlScheme.Controller;
                int randomCard = Random.Range(0, startStationList.Count);
                UICamera.selectedObject = startStationList[randomCard].GetComponentInChildren<UIKeyNavigation>().gameObject;
            }
                

            //TODO Korion: Make these to buttons. Why aren't these already buttons?
            if (Input.GetMouseButtonDown(0))
            {
                var mousePos = Cam.ScreenToWorldPoint(Input.mousePosition);

                Collider2D coll = Physics2D.OverlapPoint(mousePos);
                if (coll)
                {
                    GameObject card = coll.transform.parent.gameObject;
                    if (card)
                    {
                        currentCard = card;
                        TurnCardFaceUp(card);
                        //CallCardSelected(player, card);
                    }
                }
            }        
        }
        else if(player.Controller == PlayerController.Ai && GameSetupBehaviour.Instance.LocalPlayer.IsResponsibleFor(player))
        {
            player.StartAtStationId = SelectRandomCard();
        }
        else if(player.Controller == PlayerController.None)
        {
            currentPlayerID++;
        }
	}
    
    /// <summary>
    /// KORION Click simulation because somehow these buttons have not been used like buttons. Sigh...
    /// </summary>
    public void SimulateClick(GameObject go)
    {
        if (go)
        {
            currentCard = go;
            TurnCardFaceUp(go);
            //CallCardSelected(player, card);
        }
    }

    //void CallCardSelected(PlayerSetup player, GameObject card)
    //{
    //    if (CardSelected != null)
    //    {
    //        CardSelected(player, card.GetComponentInParent<Identifier>());
    //    }
    //}

    void TurnClickedCard()
    {
        string functionToCall = "";
        GameObject go;
        if (player.PlayerId == 0) // is MrX
        {
            // Set correct 
            currentCard.transform.GetChildByName("Frontside").GetComponentInChildren<PlayerSpriteSwapper>().SetSprite(player.Color);

            if (functionCalled || (player.Controller == PlayerController.Ai || player.Controller == PlayerController.Network))
            {
                currentCard.transform.GetChildByName("MrX").gameObject.SetActive(true);
                currentCard.transform.GetChildByName("Frontside").gameObject.SetActive(false);
                currentCard.transform.GetChildByName("Backside").gameObject.SetActive(false);
                functionToCall = "TurnComplete";
            }
            else
            {
                currentCard.transform.GetChildByName("Frontside").gameObject.SetActive(true);
                currentCard.transform.GetChildByName("Backside").gameObject.SetActive(false);
                functionToCall = (functionCalled) ? "TurnComplete" : "TurnCardFaceUp";
            }
        }
        else // is Detective
        {
            currentCard.transform.GetChildByName("Frontside").GetComponentInChildren<PlayerSpriteSwapper>().SetSprite(player.Color);
            currentCard.transform.GetChildByName("Frontside").gameObject.SetActive(true);
            currentCard.transform.GetChildByName("Backside").gameObject.SetActive(false);
            functionToCall = "TurnComplete";
        }

        functionCalled = !functionCalled;
        go = currentCard.transform.GetChildByName("Frontside").gameObject;
        var s = go.transform.GetChildByName("PlayerActor").GetComponent<UISprite>();
        s.spriteName = player.Color.GetActorSpriteName();

        this.LogInfo("Animate to: " + functionToCall);

        iTween.ScaleTo(currentCard,
                        iTween.Hash("x", 1,
                        "time", 0.5f,
                        "oncomplete", functionToCall,
                        "oncompletetarget", this.gameObject
                       ));
    }

    void TurnComplete()
    {
        currentCard = null;
        if (currentPlayerID == GameSetupBehaviour.Instance.LastPlayerID() + 1)
        {
            StartCoroutine(LoadNextLevel());
        }
    }

    IEnumerator LoadNextLevel()
    {

        Icon.spriteName = LOAD_ICON;
        Icon.color = IconColorLoading;
        Icon.gameObject.SetComponentsEnabled<RotatingLoadingIndicator>(true);

        sprite.gameObject.SetActive(false);
        TopBar.transform.GetChildByName("Label").GetComponent<LabelTranslator>().SetText("game_is_loading");

        yield return new WaitForSeconds(0.5f);
		SceneManager.LoadSceneAsync("Game");
    }

    int SelectRandomCard()
    {
        int randomCard = Random.Range(0, startStationList.Count);
        currentCard = startStationList[randomCard];
        
        TurnCardFaceUp(currentCard);


        //startStationList.Remove(startStationList[randomCard]);
        return currentCard.GetComponent<Identifier>().GameID;
    }


    void TurnCardFaceUp()
    {
        iTween.ScaleTo(currentCard,
                           iTween.Hash("x", 0.05f,
                           "time", 0.5f,
                           "easetype", "easeInCubic",
                           "oncomplete", "TurnClickedCard",
                           "oncompletetarget", this.gameObject
                           ));

    }

    public void TurnCardFaceUp(GameObject card) { TurnCardFaceUp(card, true); }
    public void TurnCardFaceUp(GameObject card, bool broadcastEvent)
    {
        player = GameSetupBehaviour.Instance.GetPlayer(currentPlayerID);
        player.StartAtStationId = card.GetComponent<Identifier>().GameID;
        startStationList.Remove(card);

        currentCard = card;

        TurnCardFaceUp();
      

        if(broadcastEvent)
            this.Broadcast(GameSetupEvents.PlayerChoseCard, card.gameObject);

        UICamera.selectedObject = null;
        currentPlayerID++;

        if (currentPlayerID <= 6 && GameSetupBehaviour.Instance.GetPlayer(currentPlayerID).Controller == PlayerController.Human)
        {
            MultiplayerInputManager.Instance.NextPlayer();
        }
    }


    //internal void ReplaceNetworkPlayerWithAi(string participantId)
    //{
    //    foreach (PlayerSetup setup in GameSetup.Instance.IterateAllPlayers())
    //    {
    //        if (setup.Controller == PlayerController.Network && setup.ControllingParticipantID == participantId)
    //        {
                
    //        }
    //    }
    //}

}

//13 = Metro, 
//26 = Taxi, 
//29 = Bus, 
//34 = Bus, 
//50 = Taxi, 
//53 = Taxi, 
//91 = Taxi, 
//94 = Bus, 
//103 = Taxi, 
//112 = Taxi, 
//117 = Taxi, 
//132 = Taxi, 
//138 = Taxi, 
//141 = Taxi, 
//155 = Taxi, 
//174 = Taxi, 
//197 = Taxi, 
//198 = Taxi
