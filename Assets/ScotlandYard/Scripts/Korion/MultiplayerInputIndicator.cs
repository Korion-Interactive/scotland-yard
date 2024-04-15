using Korion.ScotlandYard.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class MultiplayerInputIndicator : MonoBehaviour
{
    private Dictionary<int, UILabel> keyValuePairs = new Dictionary<int, UILabel>();

    private bool _oneControllerOnly = false;

    [SerializeField]
    private UILabel mrX;

    public bool OneControllerOnly { get => _oneControllerOnly; set => _oneControllerOnly = value; }

    public void HandleHumanPlayer(int state, GameObject nguiTextLabel)
    {
        if (_oneControllerOnly)
            return;

        if(GameSetupBehaviour.Instance.IsMultiControllerGame())
        {
            HandleHumanPlayersMultiController(state, nguiTextLabel);
        }
        else
        {
            HandleHumanPlayersHotSeat(state, nguiTextLabel);
        }
    }

    private void HandleHumanPlayersMultiController(int state, GameObject nguiTextLabel)
    {
        //addPlayer
        if (state == 1)
        {
            //for all players
            for (int i = 0; i < MultiplayerInputManager.Instance.AllPlayers.Count; i++)
            {
                //exists already a player with a label?
                if (keyValuePairs.ContainsKey(MultiplayerInputManager.Instance.AllPlayers[i].id)
                    || i > 0 && i >= ReInput.controllers.joystickCount) // && i > 0 -> if no joystick is connected, still allow player1 to be assigned (keyboard & mouse)
                {
                    //if every player is ocupied
                    if (i == MultiplayerInputManager.Instance.AllPlayers.Count - 1)
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
                    UILabel label = nguiTextLabel.GetComponent<UILabel>();
                    string newText = label.text + (i + 1).ToString();
                    label.text = newText;
                    keyValuePairs.Add(MultiplayerInputManager.Instance.AllPlayers[i].id, label);
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
                if (keyValuePairs.TryGetValue(MultiplayerInputManager.Instance.AllPlayers[i].id, out UILabel value))
                {
                    //yes? -> delete if we are on this label
                    //remove
                    if (value == nguiTextLabel.GetComponent<UILabel>())
                        keyValuePairs.Remove(MultiplayerInputManager.Instance.AllPlayers[i].id);
                }
            }
        }
    }

    private void HandleHumanPlayersHotSeat(int state, GameObject nguiTextLabel)
    {
        //addPlayer
        if (state == 1)
        {
            //for all players
            for (int i = 0; i < 6; i++)
            {
                //exists already a player with a label?
                if (keyValuePairs.ContainsKey(i))
                {
                    continue;
                }
                else //if player is free
                {
                    //add
                    UILabel label = nguiTextLabel.GetComponent<UILabel>();
                    string newText = label.text + (i + 1).ToString();
                    label.text = newText;

                    if(i < MultiplayerInputManager.Instance.AllPlayers.Count)
                        keyValuePairs.Add(MultiplayerInputManager.Instance.AllPlayers[i].id, label);
                    else
                        keyValuePairs.Add(i, label);

                    return;
                }
            }
        }
        //remove player
        else
        {
            //for all players
            for (int i = 0; i < 6; i++)
            {
                //exists already a label?
                if (keyValuePairs.TryGetValue(i, out UILabel value))
                {
                    //yes? -> delete if we are on this label
                    //remove
                    if (value == nguiTextLabel.GetComponent<UILabel>())
                    {
                        if (i < MultiplayerInputManager.Instance.AllPlayers.Count)
                        {
                            keyValuePairs.Remove(MultiplayerInputManager.Instance.AllPlayers[i].id);
                        }
                        else
                        {
                            keyValuePairs.Remove(i);
                        }
                    }
                }
            }
        }
    }

}