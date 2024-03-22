using System;
using UnityEngine;

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

        public InputSymbolData InputSymbolsText => _inputSymbolsText;
        public InputSymbolData InputPlaystation => _inputPlaystation;
        public InputSymbolData InputXbox => _inputXbox;
        public InputSymbolData InputSwitch => _inputSwitch;

        public InputSymbolData EmptyInputSymbols => new()
        {
            submit = "",
            cancel = "",
            other1 = "",
            other2 = "",
            leftStick = "",
            rightStick = "",
            leftShoulder1 = "",
            rightShoulder1 = "",
            leftShoulder2 = "",
            rightShoulder2 = "",
            option = ""
        };

        [Serializable]
        public struct InputSymbolData
        {
            public string submit;
            public string cancel;
            public string other1;
            public string other2;
            public string leftStick;
            public string rightStick;
            public string leftShoulder1;
            public string rightShoulder1;
            public string leftShoulder2;
            public string rightShoulder2;
            public string option;
        }
    }
}