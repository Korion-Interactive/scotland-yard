using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class SetSaveDataPlatformOptionsPS4
{
	static string SearchForFile(string path, string findFilename)
	{
		var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
		foreach (var file in files)
		{
			if (Path.GetFileNameWithoutExtension(file) == findFilename)
				return file;
		};

		return "";
	}
	
	[MenuItem("SCE Publishing Utils/Set Publish Settings For PS4")]
	// Use this for initialization
	static void SetOptions()
	{
        // Param file settings.
        PlayerSettings.PS4.category = PlayerSettings.PS4.PS4AppCategory.Application;
		PlayerSettings.PS4.appVersion = "01.00";
		PlayerSettings.PS4.masterVersion = "01.00";
		// This is the Unity example title content ID
		PlayerSettings.PS4.contentID = "IV0002-NPXX51363_00-UNITYSAVEDATA000";

		PlayerSettings.productName = "Unity Save Data Example";
		PlayerSettings.PS4.parentalLevel = 1;
		PlayerSettings.PS4.enterButtonAssignment = PlayerSettings.PS4.PS4EnterButtonAssignment.CrossButton;
		PlayerSettings.PS4.paramSfxPath = "";	// "Assets/Editor/SonyNPPS4PublishData/param.sfx";

		// PSN Settings.
		PlayerSettings.PS4.npAgeRating = 12;
		
		MoveSaveDataIconStreamingAssets();
		
		AssetDatabase.Refresh();
	}

	// Replace whatever Input Manager you currently have with one to work with the Nptoolkit Sample
	[MenuItem("SCE Publishing Utils/Set Input Manager PS4")]
	static void ReplaceInputManager()
	{

		// This is the InputManager asset that comes with the example project. Note that to avoid an import error, the '.asset' file extension has been removed
		string sourceFile = SearchForFile(Application.dataPath, "InputManager"); 

		// This is the InputManager in your ProjectSettings folder
		string targetFile = Application.dataPath;
		targetFile = targetFile.Replace("/Assets", "/ProjectSettings/InputManager.asset");

		// Replace the ProjectSettings file with the new one, and trigger a refresh so the Editor sees it
		FileUtil.ReplaceFile(sourceFile, targetFile);
		AssetDatabase.Refresh();

		Debug.Log("InputManager replaced! " + sourceFile + " -> " + targetFile);
	}

    //Move save data icon to streaming assets
    private static void MoveSaveDataIconStreamingAssets()
    {
	    // Copy the SampleStreamingAssets folder to the Assets root.
	    string sourceFile = SearchForFile(Application.dataPath, "SaveIcon");

	    // Remove up to the assets folder
	    sourceFile = sourceFile.Replace(Application.dataPath, "Assets");

	    // string sampleStreamingFolder = Path.GetDirectoryName(sourceFile);

	    string streamingDir = Application.dataPath + "/StreamingAssets";
	    string targetFile = streamingDir + "/SaveIcon.png";

	    targetFile = targetFile.Replace(Application.dataPath, "Assets");

	    if ( Directory.Exists(streamingDir) == false)
	    {
		    Directory.CreateDirectory(streamingDir);
	    }

	    AssetDatabase.CopyAsset(sourceFile, targetFile);
    }
}
