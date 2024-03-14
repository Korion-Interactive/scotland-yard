using Korion.ScotlandYard.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class MultiplayerInputIndicator : MonoBehaviour
{
    private Dictionary<Player, UILabel> keyValuePairs = new Dictionary<Player, UILabel>();

    private bool _oneControllerOnly = false;

    [SerializeField]
    private UILabel mrX;

    public bool OneControllerOnly { get => _oneControllerOnly; set => _oneControllerOnly = value; }

    public void HandleHumanPlayer(int state, GameObject nguiTextLabel)
    {
        if (_oneControllerOnly)
            return;

        //addPlayer
        if (state == 1)
        {
            //for all players
            for (int i = 0; i < MultiplayerInputManager.Instance.AllPlayers.Count; i++)
            {
                //exists already a player with a label?
                if (keyValuePairs.ContainsKey(MultiplayerInputManager.Instance.AllPlayers[i]))
                {
                    //if every player is ocupied
                    if(i == MultiplayerInputManager.Instance.AllPlayers.Count -1)
                    {
                        //skip to cpu
                        nguiTextLabel.transform.parent.gameObject.GetComponent<UIStateButton>().TriggerOnClick();
                        return;
                    }

                    continue;
                }
                else //if player is free
                {
                    //add
                    string newText = nguiTextLabel.GetComponent<UILabel>().text + (i+1).ToString();
                    nguiTextLabel.GetComponent<UILabel>().text = newText;
                    keyValuePairs.Add(MultiplayerInputManager.Instance.AllPlayers[i], nguiTextLabel.GetComponent<UILabel>());
                    return;
                }
            }
        }
        //remove player
        else
        {
            //for all players
            for (int i = 0; i < MultiplayerInputManager.Instance.AllPlayers.Count; i++)
            {
                //existiert already a label?
                if (keyValuePairs.TryGetValue(MultiplayerInputManager.Instance.AllPlayers[i], out UILabel value))
                {
                    //yes? -> delete if we are on this label
                    //remove
                    if(value == nguiTextLabel.GetComponent<UILabel>())
                        keyValuePairs.Remove(MultiplayerInputManager.Instance.AllPlayers[i]);
                }
            }
        }
    }
}