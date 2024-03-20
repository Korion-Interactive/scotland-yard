using Rewired;
public class InputDevices
{

    public delegate void InputDeviceChanged(Controller controller);
    public static event InputDeviceChanged onInputDeviceChanged;

    public static bool EventInitialized = false;

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
