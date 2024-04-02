using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class CheckCppPlatform
{
    // A Test behaves as an ordinary method
    [TestCase("com.unity.savedata.ps5", BuildTarget.PS5)]
    [Test]
    public void CheckCppMarkedForCorrectPlatforms(string packageName, params BuildTarget[] allowedTargets)
    {
        var pluginImporter = AssetImporter.GetAtPath($"Packages/{packageName}/Plugins/PS5/Source/{packageName}_preprocessed.cpp") as PluginImporter;

        Assert.NotNull(pluginImporter, "Could not find plugin importer for plugin file");
        Assert.False(pluginImporter.GetCompatibleWithAnyPlatform(), "Plugin should not be compatable with 'Any Platform'");

        foreach (BuildTarget target in Enum.GetValues(typeof(BuildTarget)))
        {
            if (!allowedTargets.Contains(target))
            {
                Assert.False(pluginImporter.GetCompatibleWithPlatform(target), $"{packageName}_preprocessed.cpp has been marked for {target.ToString()}");
            }
            else
            {
                Assert.True(pluginImporter.GetCompatibleWithPlatform(target));
            }
        }
    }
}
