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
    [TestCase("com.unity.psn.ps5", "PS5", null, BuildTarget.PS5)]
    [TestCase("com.unity.psn.ps5","PS4", "com.unity.psn.ps4_preprocessed.cpp", BuildTarget.PS4)]
    [Test]
    public void CheckCppMarkedForCorrectPlatforms(string packageName, string sourceTargetFolderName, string overridecppFileName = "", params BuildTarget[] allowedTargets)
    {
        string cppFileName = string.IsNullOrWhiteSpace(overridecppFileName) ? $"{packageName}_preprocessed.cpp" : overridecppFileName;

        var pluginImporter =
            AssetImporter.GetAtPath($"Packages/{packageName}/Plugins/{sourceTargetFolderName}/Source/{cppFileName}") as PluginImporter;

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
