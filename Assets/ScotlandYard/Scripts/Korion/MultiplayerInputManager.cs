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

        public Player CurrentPlayer => _players[_currentPlayerIndex];

        public List<Player> AllPlayers => _players;

        private static MultiplayerInputManager _instance;

        [SerializeField]
        private List<Player> _players = new();
        private Player _mainPlayer;

        private int _currentPlayerIndex = 0;

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
            // Deactivate old
            _players[_currentPlayerIndex].isPlaying = false;
            _players[_currentPlayerIndex].controllers.hasKeyboard = false;
            _players[_currentPlayerIndex].controllers.hasMouse = false;

            // Change player
            ++_currentPlayerIndex;

            if(_currentPlayerIndex >= ReInput.players.playerCount)
                _currentPlayerIndex = 0;

            // Activate new player
            _players[_currentPlayerIndex].isPlaying = true;
            _players[_currentPlayerIndex].controllers.hasKeyboard = true;
            _players[_currentPlayerIndex].controllers.hasMouse = true;

            // Invoke event
            onPlayerChanged?.Invoke(ReInput.players.GetPlayer(_currentPlayerIndex));
        }

    }
}
