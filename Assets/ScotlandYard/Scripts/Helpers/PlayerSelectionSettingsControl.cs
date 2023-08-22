using UnityEngine;
using System.Collections;

public class PlayerSelectionSettingsControl : MonoBehaviour
{

    public bool IsMisterX = false;

    public UIButton AiDifficlutyButton;
    public UIButton PlayerNameButton;

    public void SwitchSettinsVisibilty(int state)
    {
        //state = (state + 1);
        //if (IsMisterX)
        //{
        //    state += 1;
        //}

        PlayerController controller = (PlayerController)state;

        switch (controller)
        {
            case PlayerController.None:
                NGUITools.SetActive(AiDifficlutyButton.gameObject, false);
                NGUITools.SetActive(PlayerNameButton.gameObject, false);
                break;
            case PlayerController.Human:
                NGUITools.SetActive(AiDifficlutyButton.gameObject, false);
                NGUITools.SetActive(PlayerNameButton.gameObject, true);
                break;
            case PlayerController.Ai:
                NGUITools.SetActive(AiDifficlutyButton.gameObject, true);
                NGUITools.SetActive(PlayerNameButton.gameObject, false);
                break;
            case PlayerController.Network:
                NGUITools.SetActive(AiDifficlutyButton.gameObject, false);
                NGUITools.SetActive(PlayerNameButton.gameObject, true);
                // TODO: disable ability to set the player name!
                break;

        }
    }
}
