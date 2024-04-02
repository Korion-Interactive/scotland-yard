using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpriteSwapper : MonoBehaviour
{
    [SerializeField]
    private PlayerBase _player;

    [SerializeField]
    private UIAtlas _atlasVar1;

    [SerializeField]
    private UIAtlas _atlasVar2;

    [SerializeField]
    private UISprite _sprite;

    /// <summary>
    /// Used to set the referenced sprite directly without a reference to the PlayerBase.
    /// </summary>
    /// <param name="playerColor">The player color used to get the player sprite variant from.</param>
    public void SetSprite(PlayerColor playerColor)
    {
        if (PlayerPrefs.GetInt(playerColor.ToString()) == 0)
        {
            _sprite.atlas = _atlasVar1;
        }
        else
        {
            _sprite.atlas = _atlasVar2;
        }
    }

    /// <summary>
    ///  Automatically set the correct player sprite if there is a valid reference to the PlayerBase.
    /// </summary>
    private void OnEnable()
    {
        if(!_player) 
        { 
            return;     // Return if the ref is not valid! A non valid ref is given in the draw card stage of the game setup!
        }


        // Get color from component
        if (PlayerPrefs.GetInt(_player.PlayerInfo.Color.ToString()) == 0)
        {
            _sprite.atlas = _atlasVar1;
        }
        else
        {
            _sprite.atlas = _atlasVar2;
        }
    }
}
