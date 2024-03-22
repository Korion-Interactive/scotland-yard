using Rewired;
using UnityEngine;

namespace Korion.ScotlandYard
{
    public class InputSymbolTextChanger : MonoBehaviour
    {
        [SerializeField] private InputSymbolMapper _inputSymbolMapper;

        private string _text;

        private InputSymbolMapper.InputSymbolData _currentInputSymbols;

        private UILabel _label;

        private void Awake()
        {
            _label = GetComponent<UILabel>();
#if UNITY_PS5 && !UNITY_EDITOR
        _currentInputSymbols = _inputSymbolMapper.InputPlaystation;
#elif UNITY_SWITCH && !UNITY_EDITOR
        _currentInputSymbols = _inputSymbolMapper.InputSwitch;
#elif UNITY_STANDALONE || UNITY_EDITOR
            _currentInputSymbols = _inputSymbolMapper.EmptyInputSymbols;
            InputDevices.onInputDeviceChanged += OnInputDeviceChanged;
#endif
        }

        private void OnInputDeviceChanged(Controller controller)
        {
            _currentInputSymbols = controller.type switch
            {
                ControllerType.Joystick => _inputSymbolMapper.InputXbox,
                ControllerType.Keyboard => _inputSymbolMapper.EmptyInputSymbols,
                ControllerType.Mouse => _inputSymbolMapper.EmptyInputSymbols,
                _ => _currentInputSymbols
            };
            Debug.Log("Input device changed to: " + controller.type);
            UpdateUILabel();
        }

        private void OnDestroy()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            InputDevices.onInputDeviceChanged -= OnInputDeviceChanged;
#endif
        }

        public void Setup(string text)
        {
            _text = text;
            UpdateUILabel();
        }

        private void UpdateUILabel()
        {
            _label.text = ChangeText();
        }

        private string ChangeText()
        {
            if (_text.Contains(_inputSymbolMapper.InputSymbolsText.submit))
                return _text.Replace(_inputSymbolMapper.InputSymbolsText.submit, _currentInputSymbols.submit);
            if (_text.Contains(_inputSymbolMapper.InputSymbolsText.cancel))
                return _text.Replace(_inputSymbolMapper.InputSymbolsText.cancel, _currentInputSymbols.cancel);
            if (_text.Contains(_inputSymbolMapper.InputSymbolsText.other1))
                return _text.Replace(_inputSymbolMapper.InputSymbolsText.other1, _currentInputSymbols.other1);
            if (_text.Contains(_inputSymbolMapper.InputSymbolsText.other2))
                return _text.Replace(_inputSymbolMapper.InputSymbolsText.other2, _currentInputSymbols.other2);
            if (_text.Contains(_inputSymbolMapper.InputSymbolsText.leftStick))
                return _text.Replace(_inputSymbolMapper.InputSymbolsText.leftStick, _currentInputSymbols.leftStick);
            if (_text.Contains(_inputSymbolMapper.InputSymbolsText.rightStick))
                return _text.Replace(_inputSymbolMapper.InputSymbolsText.rightStick, _currentInputSymbols.rightStick);
            if (_text.Contains(_inputSymbolMapper.InputSymbolsText.leftShoulder1))
                return _text.Replace(_inputSymbolMapper.InputSymbolsText.leftShoulder1,
                    _currentInputSymbols.leftShoulder1);
            if (_text.Contains(_inputSymbolMapper.InputSymbolsText.rightShoulder1))
                return _text.Replace(_inputSymbolMapper.InputSymbolsText.rightShoulder1,
                    _currentInputSymbols.rightShoulder1);
            if (_text.Contains(_inputSymbolMapper.InputSymbolsText.leftShoulder2))
                return _text.Replace(_inputSymbolMapper.InputSymbolsText.leftShoulder2,
                    _currentInputSymbols.leftShoulder2);
            if (_text.Contains(_inputSymbolMapper.InputSymbolsText.rightShoulder2))
                return _text.Replace(_inputSymbolMapper.InputSymbolsText.rightShoulder2,
                    _currentInputSymbols.rightShoulder2);
            if (_text.Contains(_inputSymbolMapper.InputSymbolsText.option))
                return _text.Replace(_inputSymbolMapper.InputSymbolsText.option, _currentInputSymbols.option);
            return _text;
        }
    }
}