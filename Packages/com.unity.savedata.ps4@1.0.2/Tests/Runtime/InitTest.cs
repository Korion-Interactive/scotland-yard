#if UNITY_PS4
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Tests
{
    [Test]
    public void PluginInit()
    {
        Sony.PS4.SaveData.InitSettings settings = new Sony.PS4.SaveData.InitSettings();
        settings.Affinity = Sony.PS4.SaveData.ThreadAffinity.Core5;
        Sony.PS4.SaveData.InitResult initResult = default;
        
        Assert.DoesNotThrow(() =>
        {
            initResult = Sony.PS4.SaveData.Main.Initialize(settings);
        });
        Assert.IsTrue(initResult.Initialized);
    }
}
#endif