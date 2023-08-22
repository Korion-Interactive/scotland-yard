using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class GameBoardAnimationSystem : BaseSystem<GameEvents, GameBoardAnimationSystem>
{
    public bool AutoFocusEnabled = true;


    SubCameraControl camSubSys;
    SubPlayerMove playerMoveSubSys;
    //SubNewspaperEndScreen newspaperEndSubSys;
    SubDirectionIndication directionIndicationSubSys;

    public SubCameraControl CamSubSystem { get { return camSubSys; } }
    public SubDirectionIndication DirectionIndicationSubSystem { get { return directionIndicationSubSys; } }

    protected override void Start()
    {
        camSubSys = GetSubSystem<SubCameraControl>();
        playerMoveSubSys = GetSubSystem<SubPlayerMove>();
        //newspaperEndSubSys = GetSubSystem<SubNewspaperEndScreen>();
        directionIndicationSubSys = GetSubSystem<SubDirectionIndication>();

        base.Start();
    }
    protected override void RegisterEvents()
    {
        ListenTo(GameEvents.ChangeGamePausing, Pause);
    }

    private void Pause(BaseArgs obj)
    {
        if(GameState.Instance.IsGamePaused)
        {
            iTween.Pause();
        }
        else
        {
            iTween.Resume();
        }
    }

  

    void UpdateCameraSize(float val)
    {
        camSubSys.UpdateCameraSize(val);
    }

    void UpdateCameraPos(Vector3 val)
    {
        camSubSys.UpdateCameraPos(val);
    }

    private void PlayerAnimationCompleted()
    {
        playerMoveSubSys.PlayerAnimationCompleted();
    }

 
 
}
