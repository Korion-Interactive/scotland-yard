using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMoveHistoryStatus : MonoBehaviour
{
    private void OnEnable()
    {
        GameSetupSettings.MoveHistoryOpen = true;
    }

    private void OnDisable()
    {
        GameSetupSettings.MoveHistoryOpen = false;
    }
}
