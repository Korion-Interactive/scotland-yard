# Initialize the PS5 PlayStation Network package in your project

Before you can use functions in the PS5 PlayStation Network package, you must first initialize the package in your scene. To do so, create an empty GameObject in your scene and attach a script that calls the `Unity.PSN.PS5.Main.Initialise` function as shown in the following code sample.

In order for PSN package functions to execute correctly, you must also call `Main.Update` every frame by calling it in an Update function. 



```
using UnityEngine;
using System;
#if UNITY_PS5 || UNITY_PS4
using Unity.PSN.PS5;
using Unity.PSN.PS5.Aysnc;
#endif

public class PSNInitialize : MonoBehaviour
{
    void Start()
    {
        try
        {
            Unity.PSN.PS5.Main.Initialize();
        }
        catch (PSNException e)
        {
            Debug.LogError("Exception During Initialization : " + e.ExtendedMessage);
        }
#if UNITY_EDITOR
        catch (DllNotFoundException e)
        {
            Debug.LogError("Missing DLL Exception : " + e.Message);
            Debug.LogError("You can't run this sample properly in th
Unity Editor. Try building and running on a PS5 DevKit/TestKit.");
        }
#endif
   }

    void Update()
   {
        try
        {
            Unity.PSN.PS5.Main.Update();
        }
        catch(Exception e)
        {
            Debug.LogError("Exception in Main Update: " + e.Message);
        }
    }
}
```

