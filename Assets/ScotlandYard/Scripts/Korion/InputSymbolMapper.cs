using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Korion.ScotlandYard
{
    [CreateAssetMenu(fileName = "InputSymbolMapper", menuName = "InputSymbolMapper")]
    public class InputSymbolMapper : ScriptableObject
    {
        [Header("Input Symbols")] [SerializeField]
        private InputSymbolData _inputSymbolsText;

        [Header("Data")] [SerializeField] private InputSymbolData _inputPlaystation;

        [SerializeField] private InputSymbolData _inputXbox;

        [SerializeField] private InputSymbolData _inputSwitch;
        
        [SerializeField] private InputSymbolData _inputKeyboard;

        public InputSymbolData InputSymbolsText => _inputSymbolsText;
        public InputSymbolData InputPlaystation => _inputPlaystation;
        public InputSymbolData InputXbox => _inputXbox;
        public InputSymbolData InputSwitch => _inputSwitch;
        public InputSymbolData InputKeyboard => _inputKeyboard;

        [Serializable]
        public struct InputSymbolData
        {
            public string submit;
            public string cancel;
            public string other1;
            public string other2;
            public string leftStick;
            public string move;
            public string leftShoulder1;
            public string rightShoulder1;
            public string leftShoulder2;
            public string rightShoulder2;
            public string option;
            public string share;
            public string selectPlayers;
            public string zoom;
        }
    }
}