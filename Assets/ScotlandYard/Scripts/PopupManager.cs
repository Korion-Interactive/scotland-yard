using UnityEngine;
using System.Collections;
using System;
using System.Linq;

public class PopupManager : MonoBehaviour
{

    public static PopupManager Instance { get { return instance; } }
    private static PopupManager instance;

    public class PopupInfo
    {
        public GameObject window;
        public GameObject yesButton;
        public GameObject noButton;
        public GameObject windowButton;
        public GameObject text;
        public GameObject header;

    }

    public GameObject Parent;
    public Transform TutorialPopupDefaultPosition, NotificationPosition;
    public GameObject PopUpWindowPrefab, TutorialPopupPrefab, NotificationPrefab;



    private PopupInfo currentPopup, currentTutorialPopup, currentNotification;



    // Use this for initialization
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (currentPopup != null)// && ClickByKey.IsBlockedGlobally)
            {
                var btn = (currentPopup.noButton != null && currentPopup.noButton.activeSelf) ? currentPopup.noButton : currentPopup.windowButton;

                DestroyWindowProgramatically(btn);
                //btn.GetComponent<UIEventListener>().onClick.Invoke(btn);
            }
            EmergencyKillPopup("Prompt");
        }
    }

    public static void ShowQuestion(string headerID, string textID, UIEventListener.VoidDelegate yesCallback, UIEventListener.VoidDelegate noCallback)
    {
        instance.ShowQuestionPopup(headerID, textID, yesCallback, noCallback);
    }

    void ShowQuestionPopup(string headerID, string textID, UIEventListener.VoidDelegate yesCallback, UIEventListener.VoidDelegate noCallback)
    {
        GameObject go = NGUITools.AddChild(Parent, PopUpWindowPrefab);

        if (currentPopup != null)
        {
            DestroyWindowProgramatically((currentPopup.noButton != null) ? currentPopup.noButton : currentPopup.windowButton);
        }

        EmergencyKillPopup("Prompt");


        ClickByKey.IsBlockedGlobally = true;

        currentPopup = InitializeWindow(go);
        go.name = "Prompt";

        currentPopup.header.GetComponent<UILabel>().text = Loc.Get(headerID);
        currentPopup.text.GetComponent<UILabel>().text = Loc.Get(textID);

        if (yesCallback != null)
        {
            currentPopup.yesButton.GetComponent<UIEventListener>().onClick += yesCallback;
        }

        if (noCallback != null)
        {
            currentPopup.noButton.GetComponent<UIEventListener>().onClick += noCallback;
        }

        currentPopup.yesButton.GetComponent<UIEventListener>().onClick += DestroyWindow;
        currentPopup.noButton.GetComponent<UIEventListener>().onClick += DestroyWindow;

        this.LogInfo(string.Format("ShowQuestionPopup({0}, {1}, {2}, {3})", headerID, textID, yesCallback, noCallback));

        #region debug log
        this.LogInfo(string.Format("popup - active self: {0}, active hierachy: {1}", go.activeSelf, go.activeInHierarchy));

        string log = "ACTIVE POPUP PARENTS:";
        Transform p = go.transform;
        while(p != null)
        {
            log += "\n" + p.name + " active self: " + p.gameObject.activeSelf;
            p = p.parent;
        }
        this.LogInfo(log);
        #endregion


    }

    public static void ShowPrompt(string headerID, string textID) { ShowPrompt(headerID, textID, null); }
    public static void ShowPrompt(string headerID, string textID, UIEventListener.VoidDelegate okCallback, params object[] locaParams)
    {
        instance.ShowPromptPopup(headerID, textID, okCallback, locaParams);
    }

    PopupInfo ShowPromptPopup(string headerID, string textID, UIEventListener.VoidDelegate okCallback, params object[] locaParams)
    {
        if (currentPopup != null)
        {
            DestroyWindowProgramatically((currentPopup.noButton != null) ? currentPopup.noButton : currentPopup.windowButton);
        }

        EmergencyKillPopup("Prompt");


        ClickByKey.IsBlockedGlobally = true;

        GameObject go = NGUITools.AddChild(Parent, PopUpWindowPrefab);
        go.name = "Prompt";
        currentPopup = InitializeWindow(go);


        //currentPopup = go;

        currentPopup.header.GetComponent<UILabel>().text = Loc.Get(headerID);
        currentPopup.text.GetComponent<UILabel>().text = string.Format(Loc.Get(textID), locaParams);

        currentPopup.yesButton.SetActive(false);
        currentPopup.noButton.SetActive(false);


        if (okCallback != null)
        {
            currentPopup.windowButton.GetComponent<UIEventListener>().onClick += okCallback;
        }
        currentPopup.windowButton.GetComponent<UIEventListener>().onClick += DestroyWindow;



        this.LogInfo(string.Format("ShowPromptPopup({0}, {1}, {2})", headerID, textID, okCallback));

        this.LogInfo(string.Format("popup - active self: {0}, active hierachy: {1}", go.activeSelf, go.activeInHierarchy));

        return currentPopup;
    }

    public static void ShowNotification(string textID, string spriteName, params object[] locaParams)
    {
        instance.ShowNotificationPopup(textID, spriteName, 4, locaParams);
    }
    
    public static void ShowNotification(string textID, string spriteName, bool closeAutomatically, params object[] locaParams)
    {
        float closeTime =  (closeAutomatically) ? 4 : -1;
        instance.ShowNotificationPopup(textID, spriteName, closeTime, locaParams);
    }
    void ShowNotificationPopup(string textID, string spriteName, float closeWaitTime, params object[] locaParams)
    {
        if (currentNotification != null)
        {
            CloseNotificationPopup();
        }
        GameObject go = NGUITools.AddChild(Parent, NotificationPrefab);
        go.transform.position = NotificationPosition.position;

        currentNotification = InitializeWindow(go);
        currentNotification.text.GetComponent<UILabel>().text = string.Format(Loc.Get(textID), locaParams);

        if (spriteName != null)
        {
            UISprite sprite = currentNotification.window.transform.GetChildByName("Sprite").GetComponent<UISprite>();
            var info = sprite.atlas.GetSprite(spriteName);
            float aspect = (float)info.width / info.height;

            sprite.spriteName = spriteName;
            //sprite.height = Mathf.RoundToInt(sprite.width / aspect);
            sprite.aspectRatio = aspect;
            sprite.SetDimensions(sprite.width, Mathf.RoundToInt(sprite.width * aspect));
            sprite.SetDirty();

            if (spriteName == "loading_icon")
            {
                this.RepeatDo(new WaitForSeconds(0.2f), () => go == null, () => sprite.cachedTransform.Rotate(Vector3.forward, 60));
            }
        }
        PopupInfo infoSave = currentNotification;

        if(closeWaitTime > 0)
            this.WaitAndDo(new WaitForSeconds(closeWaitTime), () => true, () => { if (infoSave == currentNotification) CloseNotificationPopup(); });

        this.LogInfo(string.Format("ShowNote({0}, {1})", textID, spriteName));
    }

    public static void CloseNotification()
    {
        instance.CloseNotificationPopup();
    }
    private void CloseNotificationPopup()
    {
        if(currentNotification != null)
        {
            GameObject.Destroy(currentNotification.window);
            currentNotification = null;
        }
    }

    private void EmergencyKillPopup(string popupName)
    {
        var lostPopups = Parent.transform.GetChildsByName(popupName).ToArray();
        for (int i = 0; i < lostPopups.Length; i++)
        {
            if (lostPopups[i] == null)
                continue;

            try
            {
                GameObject.Destroy(lostPopups[i].gameObject);
                this.LogWarn("EmergencyKillPopup - popup killed: " + popupName);
            }
            catch (Exception ex) 
            { 
                this.LogError("EmergencyKillPopup - popup couldn't be killed: " + popupName + " - because: " + ex.ToString());
            }
        }

    }

    public static bool IsTutorialPopupOpen { get { return instance.currentTutorialPopup != null; } }

    public static Vector3 TutorialPopupPosition
    {
        get { return instance.currentTutorialPopup.window.transform.position; }
        set { instance.currentTutorialPopup.window.transform.position = value; }
    }
    public static void ShowTutorial(string textId, Vector3 position, CompassDirection pointingDirection, bool closeOnClick, UIEventListener.VoidDelegate closeCallback)
    {
        instance.ShowTutorialPopup(textId, position, pointingDirection, closeOnClick, closeCallback);
    }

    void ShowTutorialPopup(string textId, Vector3 position, CompassDirection pointingDirection, bool closeOnClick, UIEventListener.VoidDelegate closeCallback)
    {

        if (currentTutorialPopup != null)
        {
            Log.warn(this, "destroy tut-popup programmatically!");
            DestroyTutorialWindowProgramatically(currentTutorialPopup.yesButton);
        }

        EmergencyKillPopup("TutorialPopup");

        GameObject go = NGUITools.AddChild(Parent, TutorialPopupPrefab);
        go.name = "TutorialPopup";
        currentTutorialPopup = InitializeWindow(go);

        currentTutorialPopup.text.GetComponent<UILabel>().text = Loc.Get(textId);

        UISprite spr = currentTutorialPopup.windowButton.GetComponent<UISprite>();
        float arrowWidth = go.transform.Find("ArrowRight").GetComponent<UISprite>().width + 30;

        Vector3 shift = Vector3.zero;
        switch (pointingDirection)
        {
            case CompassDirection.Undefined:
                shift = TutorialPopupDefaultPosition.position;
                break;
            case CompassDirection.East:
                go.transform.Find("ArrowRight").gameObject.SetActive(true);
                shift.x -= ((spr.width * spr.transform.lossyScale.x) / 2) + (arrowWidth) * spr.transform.lossyScale.x;
                break;

            case CompassDirection.North:
                go.transform.Find("ArrowUp").gameObject.SetActive(true);
                shift.y -= ((spr.height * spr.transform.lossyScale.y) / 2) + (arrowWidth) * spr.transform.lossyScale.y;
                break;

            case CompassDirection.West:
                go.transform.Find("ArrowLeft").gameObject.SetActive(true);
                shift.x += ((spr.width * spr.transform.lossyScale.x) / 2) + (arrowWidth) * spr.transform.lossyScale.x;
                break;

            case CompassDirection.South:
                go.transform.Find("ArrowDown").gameObject.SetActive(true);
                shift.y += ((spr.height * spr.transform.lossyScale.y) / 2) + (arrowWidth) * spr.transform.lossyScale.y;
                break;

            default:
                break;
        }

        foreach (Transform child in go.transform)
        {
            child.position += shift;
        }

        go.transform.position = position;



        if (closeOnClick)
        {
            currentTutorialPopup.yesButton.GetComponent<UIEventListener>().onClick += DestroyTutorialWindow;

            currentTutorialPopup.yesButton.SetActive(true);
            if (closeCallback != null)
            {
                currentTutorialPopup.yesButton.GetComponent<UIEventListener>().onClick += closeCallback;
            }
        }
        else
        {
            currentTutorialPopup.yesButton.SetActive(false);
            currentTutorialPopup.yesButton.GetComponent<BoxCollider>().enabled = false;
            if (closeCallback != null)
            {
                currentTutorialPopup.windowButton.GetComponent<UIEventListener>().onClick += closeCallback;
            }
        }

    }


    public static void CloseTutorialPopup()
    {
        instance.DestroyTutorialWindowProgramatically(instance.currentTutorialPopup.windowButton);
    }

    PopupInfo InitializeWindow(GameObject parent)
    {

        Parent.SetActive(true); // for some reason it sometimes gets disabled...
        var info = new PopupInfo();

        info.window = parent;
        info.header = parent.transform.Find("PopUpHeader").gameObject;
        info.text = parent.transform.Find("PopUpText").gameObject;
        info.yesButton = parent.transform.Find("PopUpYesButton").gameObject;
        info.noButton = parent.transform.Find("PopUpNoButton").gameObject;
        info.windowButton = parent.transform.Find("PopUpBackground").gameObject;

        return info;
    }

    void DestroyWindowProgramatically(GameObject button)
    {
        this.LogInfo("Destroying popup programatically");
        button.BroadcastMessage("OnClick", SendMessageOptions.DontRequireReceiver);
        DestroyWindow(button);
    }

    void DestroyWindow(GameObject button)
    {
        ClickByKey.IsBlockedGlobally = false;
        GameObject.Destroy(button.transform.parent.gameObject);
        currentPopup = null;
    }

    void DestroyTutorialWindowProgramatically(GameObject button)
    {
       // button.SetActive(true);
        button.BroadcastMessage("OnClick", SendMessageOptions.DontRequireReceiver);
        DestroyTutorialWindow(button);
    }

    void DestroyTutorialWindow(GameObject button)
    {
        GameObject.Destroy(button.transform.parent.gameObject);
        currentTutorialPopup = null;
    }
}
