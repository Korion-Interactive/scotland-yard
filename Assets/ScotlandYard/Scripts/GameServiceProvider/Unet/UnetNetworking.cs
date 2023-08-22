using System;

#if UNITY_ANDROID

public static class UnetNetworking
{
    public static bool IsServer = false;
    
    public static event Action SynchCreatedEvent;

    private static UnetTransmission _transmission;

    public static void OnCreatedPlayer()
    {
        SynchCreatedEvent?.Invoke();
    }
        
    public static UnetTransmission StartHost()
    {
        IsServer = true;
        _transmission = UnetTransmission.Instance;
        _transmission.NetworkManagerHelper.StartHost();
        return _transmission;
    }

    public static UnetTransmission StartClient()
    {
        IsServer = false;
        _transmission = UnetTransmission.Instance;
        _transmission.NetworkManagerHelper.StartClient();
        return _transmission;
    }

    public static void Disconnect()
    {
        _transmission = null;
    }
}

#endif
