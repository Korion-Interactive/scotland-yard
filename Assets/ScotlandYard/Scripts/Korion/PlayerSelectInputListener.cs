using Korion.ScotlandYard.UI;
using Rewired;
using System;
using UnityEngine;

namespace Korion.ScotlandYard.Input
{
    public class PlayerSelectInputListener : MonoBehaviour
    {
        private void OnEnable()
        {
            // Reset old player sprite selections
            RemoveGraphicsPlayerPrefs();

            MultiplayerInputManager.Instance.CurrentPlayer.AddInputEventDelegate(OnTogglePlayerSprite, UpdateLoopType.Update, "UITogglePlayerSprite");
        }

        private void OnDisable()
        {
            MultiplayerInputManager.Instance.CurrentPlayer.RemoveInputEventDelegate(OnTogglePlayerSprite);            
        }

        private void RemoveGraphicsPlayerPrefs()
        {
            foreach (var color in Enum.GetValues(typeof(PlayerColor)))
                PlayerPrefs.DeleteKey(color.ToString());
        }

        private void OnTogglePlayerSprite(InputActionEventData data)
        {
            if(data.GetButtonDown())
            {
                UICamera.selectedObject?.GetComponentInParent<PlayerSpriteToggleHandler>().TogglePlayerSprite();
            }
        }
    }
}
