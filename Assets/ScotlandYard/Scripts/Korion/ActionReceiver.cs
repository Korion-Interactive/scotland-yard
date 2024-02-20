using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActionReceiver : MonoBehaviour
{
    [SerializeField] string _actionToTrigger;
    [SerializeField] UnityEvent _onActionTriggered;
    private Player _player;
    public int PlayerID = 0;

    private void OnEnable()
    {
        //KORION TODO: Get current player
        _player = ReInput.players.GetPlayer(PlayerID);

        _player.AddInputEventDelegate(OnActionPressed, UpdateLoopType.Update, _actionToTrigger);
    }

    private void OnDisable()
    {
        _player.RemoveInputEventDelegate(OnActionPressed);
    }

    private void OnActionPressed(InputActionEventData action)
    {
        if (action.GetButtonDown())
        {
            _onActionTriggered?.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        

    }
}
