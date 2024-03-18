using UnityEngine;

namespace Korion.ScotlandYard.UI
{
    public class PlayerSpriteToggleHandler : MonoBehaviour
    {
        private UISprite _sprite;

        private void OnEnable()
        {
            _sprite = GetComponent<UISprite>();
        }

        private void OnDisable() 
        { 
            _sprite = null;
        }

        public void TogglePlayerSprite()
        {
            UpdateAtlas();
        }

        private void UpdateAtlas()
        {
            string currentName = _sprite.spriteName;
            _sprite.spriteName = currentName.EndsWith("1") ? currentName.Substring(0, currentName.Length - 1) + "2" : 
                currentName.Substring(0, currentName.Length - 1) + "1";
        }
    }
}


