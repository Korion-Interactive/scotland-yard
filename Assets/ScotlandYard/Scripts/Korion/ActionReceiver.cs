using Korion.ScotlandYard.Input;
using Rewired;
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
        _player = MultiplayerInputManager.Instance.CurrentPlayer;

        MultiplayerInputManager.onPlayerChanged += OnPlayerChanged;

        OnPlayerChanged(_player);   // Initial subscription
    }

    private void OnPlayerChanged(Player player)
    {
        _player.RemoveInputEventDelegate(OnActionPressed);

        _player = player;

        _player.AddInputEventDelegate(OnActionPressed, UpdateLoopType.Update, _actionToTrigger);
    }

    private void OnDisable()
    {
        _player.RemoveInputEventDelegate(OnActionPressed);
        MultiplayerInputManager.onPlayerChanged -= OnPlayerChanged;
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
