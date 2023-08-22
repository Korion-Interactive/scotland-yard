using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LocalPlayerController
{
    public bool IsHost { get { return ConnectionObserver.Instance.IsHost || !GameSetupBehaviour.Instance.IsNetworkGame; } }

    public bool IsResponsibleFor(PlayerBase player)
    {
        return IsResponsibleFor(player.PlayerInfo);
    }

    public bool IsResponsibleFor(PlayerSetup playerSetup)
    {
        return playerSetup.Controller == PlayerController.Human
            || playerSetup.Controller == PlayerController.TutorialCPU
            || (playerSetup.Controller == PlayerController.Ai && IsHost);
    }

    public bool IsResponsibleForCurrentPlayer()
    {
        return IsResponsibleFor(GameState.Instance.CurrentPlayer.PlayerInfo);
    }
}
