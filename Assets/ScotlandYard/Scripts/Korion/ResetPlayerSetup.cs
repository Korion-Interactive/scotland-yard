using UnityEngine;

public class ResetPlayerSetup : MonoBehaviour
{
    [SerializeField]
    private MultiplayerInputIndicator multiplayerInputIndicator;

    private void OnEnable()
    {
        ResetSetup();
    }

    /// <summary>
    /// When returning to the game mode selection, reset 
    /// </summary>
    private void ResetSetup()
    {
        multiplayerInputIndicator.ResetIndicators();
    }
}
