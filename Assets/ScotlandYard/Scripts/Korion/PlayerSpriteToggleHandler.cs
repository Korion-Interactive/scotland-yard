using UnityEngine;

namespace Korion.ScotlandYard.UI
{
    public class PlayerSpriteToggleHandler : MonoBehaviour
    {
        [SerializeField]
        private PlayerColor _playerColor;

        private UISprite _sprite;
        private int spriteVariant = 0;

        private void OnEnable()
        {
            _sprite = GetComponent<UISprite>();
            GameSetupSystem.onStartClicked += ApplySelection;
        }

        private void OnDisable() 
        {
            GameSetupSystem.onStartClicked -= ApplySelection;
            _sprite = null;
        }

        public void TogglePlayerSprite()
        {
            UpdateAtlas();
        }

        private void UpdateAtlas()
        {
            string currentName = _sprite.spriteName;
            if(currentName.EndsWith("1"))
            {
                _sprite.spriteName = currentName.Substring(0, currentName.Length - 1) + "2";
                spriteVariant = 1;
            }
            else
            {
                _sprite.spriteName = currentName.Substring(0, currentName.Length - 1) + "1";
                spriteVariant = 0;
            }
        }

        private void ApplySelection()
        {
            // Save player sprite selection to PlayerPrefs
            PlayerPrefs.SetInt(_playerColor.ToString(), spriteVariant);
            PlayerPrefs.Save();
        }
    }
}


