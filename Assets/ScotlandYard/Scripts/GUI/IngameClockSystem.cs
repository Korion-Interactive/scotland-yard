using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;

public class IngameClockSystem : BaseSystem<GameEvents, GameGuiEvents, IngameClockSystem>
{

    public UILabel ClockLabel;
    public UISprite EndlessSprite;

    private float _roundTimeInSeconds = 0;

    private bool _isInitialized;

    protected override void RegisterEvents()
    {
        ListenTo(GameEvents.GameStart, GameStart);
        ListenTo(GameEvents.GameLoaded, GameStart);

        if (GameSetupBehaviour.Instance != null &&  !float.IsInfinity(GameSetupBehaviour.Instance.Setup.RoundTime) )
        {
            ListenTo(GameEvents.TurnStart, SetTimer);
            ListenTo<MoveArgs>(GameEvents.MrXMove, MovingMrX);
            ListenTo<MoveArgs>(GameEvents.DetectiveMove, MovingDetective);
        }
    }
    private void MovingMrX(MoveArgs args)
    {
        // TODO: why doesn't it stop when AI moves mrX?
        _isInitialized = false;
    }
    private void MovingDetective(MoveArgs args)
    {
        _isInitialized = false;
    }

    private void GameStart(BaseArgs args)
    {
        if (!float.IsInfinity(GameSetupBehaviour.Instance.Setup.RoundTime)) return;
        NGUITools.SetActive(ClockLabel.gameObject, false);
        NGUITools.SetActive(EndlessSprite.gameObject, true);
    }

    private void SetTimer(BaseArgs args)
    {
        //this.LogError(float.IsInfinity(GameSetup.Instance.Setup.RoundTime).ToString());

        _roundTimeInSeconds = GameSetupBehaviour.Instance != null ? GameSetupBehaviour.Instance.Setup.RoundTime : 55;

        _isInitialized = true;
    }

    void Update()
    {
        if (GameState.Instance.IsGamePaused)
            return;

        if (_isInitialized && !float.IsInfinity(GameSetupBehaviour.Instance.Setup.RoundTime))
        {
            float before = _roundTimeInSeconds;
            _roundTimeInSeconds -= Time.deltaTime;

            if(before > 3 && _roundTimeInSeconds <= 3)
            {
                Broadcast(GameGuiEvents.TimeOutInThreeSeconds);
            }

            if (_roundTimeInSeconds < 11f)
            {
                ClockLabel.color = Color.red;
            }
            else
            {
                ClockLabel.color = Color.white;                
            }

            if (_roundTimeInSeconds > 0f)
            {
                var timeSpan = TimeSpan.FromSeconds(_roundTimeInSeconds);
                ClockLabel.text = string.Format("{0}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);

            } 
            else
            {
                _isInitialized = false;

                if(GameSetupBehaviour.Instance.LocalPlayer.IsResponsibleForCurrentPlayer())
                    this.Broadcast(GameEvents.TurnTimeOut);
            }
        }
    }

 
}
