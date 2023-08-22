using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using UnityEngine;

public abstract class TutorialSystem<TSystem> : BaseSystem<GameEvents, GameGuiEvents, TSystem>
    where TSystem : BaseSystem<TSystem>
{
    class PositionKeeper
    {
        public Vector3 gamePosition;
        public bool isGamePosition;
    }

    public Camera GameCamera;
    public Camera FooterCamera;
    protected abstract string LocaStringIdPrefix { get; }
    protected abstract GameMode expectedGameMode { get; }
    protected int tutorialPopupIndex = 0;


    Func<bool> currentClosingCondition;

    PositionKeeper currentPosition = new PositionKeeper();

    protected override void Awake()
    {
        currentClosingCondition = null;
        tutorialPopupIndex = 0;
        if (GameSetupBehaviour.Instance.Setup.Mode != expectedGameMode)
        {
            this.gameObject.SetActive(false);
        }
        base.Awake();
    }

    protected override void RegisterEvents()
    {
        ListenTo(GameEvents.TurnStart, TurnStart, Globals.Listen_Early);
        ListenTo(GameEvents.ChangeGamePausing, Pausing);
    }

    private void Pausing(BaseArgs args)
    {
        float alpha = (GameState.Instance.IsMenuPause) ? 0 : 1;
        PopupManager.Instance.Parent.GetComponent<UIPanel>().alpha = alpha;
    }

    private void TurnStart(BaseArgs args)
    {
        PlayerTurnStarts(args.RelatedObject.GetComponent<PlayerBase>(), GameState.Instance.MrX.Moves);
    }

    protected void PauseNextFrame()
    {
        PauseNextFrame(true);
    }
    protected void PauseNextFrame(bool pause)
    {
        StartCoroutine(CoPauseNextFrame(pause));
    }

    IEnumerator CoPauseNextFrame(bool pause)
    {
        GameState.Instance.SetPausing(!pause, true);
        yield return new WaitForEndOfFrame();
        GameState.Instance.SetPausing(pause, true);
    }

    protected void UnPause()
    {
        GameState.Instance.SetPausing(false, true);
    }

    protected void ShowNextPopupDelayed(float seconds, int currentPopupIndex)
    {
        if (tutorialPopupIndex != currentPopupIndex)
        {
            this.LogWarn("unexpected popup index. expected: " + currentPopupIndex + " actual: " + tutorialPopupIndex );
            return;
        }

        StopCoroutine("WaitAndShowPopup");
        StartCoroutine(WaitAndShowPopup(seconds, currentPopupIndex));
    }

    protected void ShowNextPopupDelayed(Vector3 pointOfInterest, CompassDirection pointingDirection, Func<bool> closingCondition, UIEventListener.VoidDelegate callback, float seconds, bool isGameCoordinate, int currentPopupIndex)
    {
        if (tutorialPopupIndex != currentPopupIndex)
        {
            this.LogWarn("unexpected popup index. expected: " + currentPopupIndex + " actual: " + tutorialPopupIndex);
            return;
        }

        StopCoroutine("WaitAndShowPopup");
        StartCoroutine(WaitAndShowPopup(pointOfInterest, pointingDirection, closingCondition, callback, seconds, isGameCoordinate, currentPopupIndex));
    }

    IEnumerator WaitAndShowPopup(float seconds, int currentPopupIndex)
    {
        yield return new WaitForSeconds(seconds);
        this.ShowNextPopup(currentPopupIndex);
    }

    IEnumerator WaitAndShowPopup(Vector3 pointOfInterest, CompassDirection pointingDirection, Func<bool> closingCondition, UIEventListener.VoidDelegate callback, float seconds, bool isGameCoordinate, int currentPopupIndex)
    {
        yield return new WaitForSeconds(seconds);
        if (isGameCoordinate)
        {
            pointOfInterest = GameToGui(pointOfInterest);
            GameBoardAnimationSystem.Instance.CamSubSystem.AdditionalFocusPoints.Add(pointOfInterest - pointingDirection.ToDirectionVector3());
        }
        this.ShowNextPopup(pointOfInterest, pointingDirection, closingCondition, callback, currentPopupIndex);
    }

    protected void ShowNextPopup(int expectedPopupIndex)
    {
        this.ShowNextPopup(null, expectedPopupIndex);
    }
    protected void ShowNextPopup(UIEventListener.VoidDelegate callback, int currentPopupIndex)
    {
        ShowNextPopup(PopupManager.Instance.Parent.transform.position, CompassDirection.Undefined, null, callback, currentPopupIndex);
    }

    protected void ShowNextPopup(Vector3 pointOfInterest, CompassDirection pointingDirection, Func<bool> closingCondition, UIEventListener.VoidDelegate callback, int currentPopupIndex)
    {
        if(tutorialPopupIndex != currentPopupIndex)
        {
            this.LogWarn("unexpected popup index. expected: " + currentPopupIndex + " actual: " + tutorialPopupIndex);
            return;
        }

        GameState.Instance.SetPausing(pointingDirection == CompassDirection.Undefined, true);
        currentClosingCondition = closingCondition;

        string locID = string.Format("{0}_{1}", LocaStringIdPrefix, tutorialPopupIndex + 1);
        PopupManager.ShowTutorial(locID, pointOfInterest, pointingDirection, closingCondition == null,
            (go) =>
            {
                this.LogInfo("Closing Callback - tut popup #" + tutorialPopupIndex);
                GameState.Instance.SetPausing(false, true);
                currentPosition.isGamePosition = false;
                PopupClosed(tutorialPopupIndex);
                if (callback != null)
                {
                    callback(go);
                }
            });

        currentPosition.gamePosition = GuiToGame(PopupManager.TutorialPopupPosition);
        tutorialPopupIndex++;

        this.LogInfo(string.Format("TUTORIAL: {0} -- tut popup index is now #{1}", locID, tutorialPopupIndex));
    }
    // round is one-based
    protected abstract void PlayerTurnStarts(PlayerBase player, int round);
    protected abstract void PopupClosed(int popupProgress);

    private Vector3 GuiToGame(Vector3 guiPosition)
    {
        Vector3 screenCoordinate = FooterCamera.WorldToScreenPoint(guiPosition);
        return GameCamera.ScreenToWorldPoint(screenCoordinate);
    }

    protected Vector3 GameToGui(Vector3 gamePosition)
    {
        currentPosition.isGamePosition = true;
        Vector3 screenCoordinate = GameCamera.WorldToScreenPoint(gamePosition);
        return FooterCamera.ScreenToWorldPoint(screenCoordinate);
    }

    protected virtual void Update()
    {
        if(currentPosition.isGamePosition)
        {
            PopupManager.TutorialPopupPosition = GameToGui(currentPosition.gamePosition);
        }

        if(currentClosingCondition != null && currentClosingCondition())
        {
            PopupManager.CloseTutorialPopup();
            currentClosingCondition = null;
            currentPosition.isGamePosition = false;
        }
    }

}
