using Korion.ScotlandYard.UI;
using Rewired;
using UnityEngine;

namespace Korion.ScotlandYard.Input
{
    public class PlayerSelectInputListener : MonoBehaviour
    {

        private void OnEnable()
        {
            MultiplayerInputManager.Instance.CurrentPlayer.AddInputEventDelegate(OnTogglePlayerSprite, UpdateLoopType.Update, "UITogglePlayerSprite");
        }

        private void OnDisable()
        {
            MultiplayerInputManager.Instance.CurrentPlayer.RemoveInputEventDelegate(OnTogglePlayerSprite);            
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
