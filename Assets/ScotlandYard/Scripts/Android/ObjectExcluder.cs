using System.Collections.Generic;
using UnityEngine;


#if UNITY_ANDROID
using BluetoothMultiplayer = Bluetooth.Android.MultiplayerRT;
#endif

public class ObjectExcluder : MonoBehaviour
{
    public List<GameObject> ObjectsToExcludeIfNoBluetooth = new List<GameObject>();

    void OnEnable()
    {
        ExcludeNoBluetoothObjects();

        UIGrid grid = this.GetComponent<UIGrid>();
        if(grid != null)
            this.WaitAndDo(null, null, () => grid.Reposition());
    }

    void ExcludeNoBluetoothObjects()
	{
        bool bluetoothAvailable = true;

#if UNITY_ANDROID
        try
        {
            bluetoothAvailable = LostPolygon.AndroidBluetoothMultiplayer.AndroidBluetoothMultiplayer.GetIsBluetoothAvailable();
            this.LogInfo(string.Format("Bluetooth available: {0}", bluetoothAvailable)); 
        }
        catch (AndroidJavaException ex)
        {
            bluetoothAvailable = false;
            this.LogError("Bluetooth not available", ex);
        }
#endif

        if (!bluetoothAvailable)
			DestroyAllObjectsOfListAndClear(ObjectsToExcludeIfNoBluetooth);
    }

    static void DestroyAllObjectsOfListAndClear(List<GameObject> list)
    {
        foreach (var obj in list)
        {
            if (obj != null)
                GameObject.Destroy(obj);
        }
        list.Clear();
    }

}
