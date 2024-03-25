using Rewired;
using UnityEngine;

public class InputDevices
{

    public delegate void InputDeviceChanged(Controller controller);
    public static event InputDeviceChanged onInputDeviceChanged;

    public static bool EventInitialized = false;
    
    public static Controller LastActiveController
    {
        get
        {
            if (ReInput.isReady)
                return ReInput.controllers.GetLastActiveController();
            return null;
        }
    }

    public InputDevices()
    {
        if (ReInput.isReady)
            ReInput.controllers.AddLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
    }

    public void DeInit()
    {
        if (ReInput.isReady)
            ReInput.controllers.RemoveLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
    }

    private void OnLastActiveControllerChanged(Controller controller)
    {
        onInputDeviceChanged?.Invoke(controller);
    }
}
