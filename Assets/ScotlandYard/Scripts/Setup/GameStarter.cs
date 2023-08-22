using UnityEngine;
using System.Collections;

/// <summary>
/// component 'GameStarter'
/// ADD COMPONENT DESCRIPTION HERE
/// </summary>
[AddComponentMenu("Scripts/GameStarter")]
public class GameStarter : MonoBehaviour
{
    IEnumerator Start()
    {
        GSP.AllowInvites = false;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // Create all the players
        GameSetupBehaviour.Instance.CreatePlayers();
        GameState.Instance.Init();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        this.LogInfo(string.Format("Is Host: {0}", GameSetupBehaviour.Instance.LocalPlayer.IsHost));

        if(!GameState.Instance.HasBeenLoadedFromFile)
            this.Broadcast<GameEvents>(GameEvents.GameStart);
    }

    void OnDestroy()
    {
        GSP.AllowInvites = true;
    }

}
