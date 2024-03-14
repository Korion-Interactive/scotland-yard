using UnityEngine;
using Rewired;
using System.Collections.Generic;

namespace Korion.ScotlandYard.Input
{
    public class MultiplayerInputManager : MonoBehaviour
    {
        public delegate void PlayerChanged(Player player);
        public static event PlayerChanged onPlayerChanged;

        public static MultiplayerInputManager Instance => _instance;
        public Player MainPlayer => _mainPlayer;

        public Player CurrentPlayer => _players[_currentInputIndex];

        public List<Player> AllPlayers => _players;

        private static MultiplayerInputManager _instance;

        private List<Player> _players = new();
        private Player _mainPlayer;
        private int _currentInputIndex = 0;


        private void Awake()
        {
            if(_instance == null)
                _instance = this;

            if(ReInput.isReady)
            {
                _mainPlayer = ReInput.players.GetPlayer(0);
                
                foreach(var player in ReInput.players.AllPlayers)
                {
                    if (player.descriptiveName == "System")
                        continue;

                    _players.Add(player);
                }
            }
        }

        private void OnDestroy()
        {
            _players.Clear();
        }

        public void NextPlayer()
        {
            Debug.Log("Next player");
            // Deactivate old
            _players[_currentInputIndex].isPlaying = false;
            _players[_currentInputIndex].controllers.hasKeyboard = false;
            _players[_currentInputIndex].controllers.hasMouse = false;

            // Change player
            ++_currentInputIndex;

            if(_currentInputIndex >= ReInput.players.playerCount)
                _currentInputIndex = 0;

            if (_currentInputIndex >= GameSetupBehaviour.Instance.HumanPlayers)
            {
                _currentInputIndex = 0;
            }

            // Activate new player
            _players[_currentInputIndex].isPlaying = true;
            _players[_currentInputIndex].controllers.hasKeyboard = true;
            _players[_currentInputIndex].controllers.hasMouse = true;

            // Invoke event
            onPlayerChanged?.Invoke(ReInput.players.GetPlayer(_currentInputIndex));
        }

    }
}
