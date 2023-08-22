using System;
using UnityEditor;
using JetBrains.Annotations;
using Ravity.ProjectBuilder;
using UnityEditor.Callbacks;
#if UNITY_IOS
using System.Diagnostics;
using UnityEditor.iOS.Xcode;
#endif

[UsedImplicitly]
public class ScotlandYardBuilder : ProjectBuilder
{
	public override void PreBuild()
	{
		PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS,"com.ravensburger-digital.ScotlandYardHD");
		PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.ravensburgerdigital.ScotlandYardHD");

		DateTime firstRelease = DateTime.Parse("2012-05-12");
		TimeBasedVersionNumber.SetBuildVersionForMobilePlatforms(firstRelease);
	}

	[PostProcessBuild]
	public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
	{
		#if UNITY_IOS
		if(buildTarget == BuildTarget.iOS)
		{
			string[] languages = { "en", "fr", "de", "it", "es" };
			XcodeUtils.LocalizeXcodeProject(buildTarget,path,languages);
			XcodeUtils.SetXcodeExportComplianceToNoEncryptionOrEncryptionExempts(path);

			XcodeUtils.ModifyInfoPlist(path, document =>
			{
				document.root["NSLocalNetworkUsageDescription"] = new PlistElementString("Wird für den Mehrspielermodus benötigt.");
				document.root["NSBonjourServices"] = new PlistElementArray
				{
					values =
					{
						new PlistElementString($"_{Bluetooth.iOS.Multipeer.MultiplayerRT.ADVERTISE_NAME}._tcp"),
						new PlistElementString($"_{Bluetooth.iOS.Multipeer.MultiplayerRT.ADVERTISE_NAME}._udp"),
					}
				};
			});
		}
		#endif
	}
}
