using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SubNewspaperEndScreen : SubSystem<GameBoardAnimationSystem>
{

    public GameObject NewsPaper;
    public LabelTranslator NewsHeader;
    public LabelTranslator NewsText;
    public UISprite NewsSprite;
    public GameBoardAnimationSystem GameBoardSys;
    private Vector3 rotationAmount = new Vector3(0, 0, 2160);
    //private Vector3 scaleAmount = new Vector3(0.2f, 0.2f, 0);
    private float animationTime = 1.5f;

    protected override bool needsUpdate { get { return false; } }

    internal override void RegisterEvents()
    {
        System.ListenTo<GameOverArgs>(GameEvents.GameEnd, GameEnded);
    }
    private void GameEnded(GameOverArgs args)
    {
        // early out if event came second time (through network)
        if (NewsPaper.activeSelf)
            return;


        NewsPaper.SetActive(true);
        Rotate();
        Scale();
        NewsHeader.SetText("game_end_title_" + args.Reason.ToString());
        NewsText.SetTextWithStaticParams("game_end_text_" + args.Reason.ToString(), GameState.Instance.MrX.Location.Id.ToString(), GameState.Instance.CurrentPlayer.PlayerDisplayName);
        NewsSprite.spriteName = "end_screen_pic_" + ((args.Reason == GameOverReason.MrXCaught || args.Reason == GameOverReason.MrXSurrounded) ? "caught" : "escape");
    }

    private void Rotate()
    {
        iTween.RotateTo(NewsPaper,
            iTween.Hash(
            "rotation", rotationAmount,
            "time", animationTime,
            "easetype", "easeInQuint",
            "oncomplete", "MakeClickable",
            "oncompletetarget", NewsPaper)
            );
    }

    private void Scale()
    {
        NewsPaper.transform.localScale = new Vector3(0.2f, 0.2f);

        iTween.ScaleTo(NewsPaper,
            iTween.Hash(
            "scale", Vector3.one,
            "time", animationTime,
            "easetype", "easeInQuint")
            );
    }

}