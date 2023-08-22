#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor;

#if UNITY_IOS || UNITY_TVOS
using System;
using UnityEditor.iOS.Xcode;
#endif

// ReSharper disable UnusedMember.Global
namespace Ravity.ProjectBuilder
{
	public static class XcodeUtils
	{
		// Localize Xcode project, so that those languages show up in iTunes and in system settings for app
		// legal input: 
		//   EFIGS: "en", "fr", "de", "it", "es", "pt-PT"
		//   Other European Languages:  "sv", "nl", "da", "fi"
		//   Asian: "ru", "ja", "ko", "zh-Hans", "zh-Hant"
		public static void LocalizeXcodeProject(BuildTarget target, string path, params string[] languages)
		{
#if UNITY_IOS || UNITY_TVOS
			if (target != BuildTarget.iOS && target != BuildTarget.tvOS)
			{
				return;
			}
			AddLanguagesToXcodeProjectFile(path, languages);
			AddLanguagesToInfoPListAsCFBundleLocalizations(path, languages);
#endif
		}

		// configure iOS Export Compliance in Xcode: ITSAppUsesNonExemptEncryption NO
		// only call this the app either uses no encryption, or only uses encryption that’s exempt from export compliance requirements,
		// e.g. "Limited to intellectual property and copyright protection" or "Limited to authentication, digital signature, or the decryption of data or files"
		// see https://developer.apple.com/documentation/bundleresources/information_property_list/itsappusesnonexemptencryption
		public static void SetXcodeExportComplianceToNoEncryptionOrEncryptionExempts(string path)
		{
#if UNITY_IOS || UNITY_TVOS	
			SetInfoPlistElement(path, "ITSAppUsesNonExemptEncryption", new PlistElementBoolean(false));
#endif
		}
	
#if UNITY_IOS || UNITY_TVOS
		private static void AddLanguagesToXcodeProjectFile(string path, string[] languages)
		{
			const string PATH_PBX = "/Unity-iPhone.xcodeproj/project.pbxproj";
			string pathPBXProject = path + PATH_PBX;

			string fileContent = File.ReadAllText(pathPBXProject);

			// switch development region from deprecated "English" to "en"
			fileContent = fileContent.Replace("developmentRegion = English;", "developmentRegion = en;");
			
			// inject knownRegions
			string newline = "\n";
			string replaceWith = "knownRegions = ("+newline;
			foreach (string language in languages)
			{
				string insertedLanguage = language;
				if (insertedLanguage.Contains("-"))
				{
					// i.e. "zh-Hans" or "pt-PT"
					insertedLanguage = "\"" + language + "\"";
				}
				replaceWith += "\t\t\t\t"+ insertedLanguage +"," + newline;
			}

			string patternStart = "knownRegions = (";
			string patternEnd = ");";

			int start = fileContent.IndexOf(patternStart);
			int end = fileContent.IndexOf(patternEnd,start);  
			fileContent = fileContent.Remove(start,end-start);
			fileContent = fileContent.Insert(start,replaceWith);

			File.WriteAllText(pathPBXProject,fileContent);
		}
		
		private static void SetInfoPlistElement(string path, string key, PlistElement element)
		{
			ModifyInfoPlist(path, document =>
			{
				document.root[key] = element;
			});
		}

		public static void ModifyInfoPlist(string path, Action<PlistDocument> action)
		{
			path += "/Info.plist";
			PlistDocument doc = new PlistDocument();
			doc.ReadFromFile(path);
			action.Invoke(doc);
			doc.WriteToFile(path);
			Debug.Log($"Modified: {path}");
		}
		
		private static void AddLanguagesToInfoPListAsCFBundleLocalizations(string path, string[] languages)
		{
			ModifyInfoPlist(path, doc =>
			{
				PlistElementArray array = doc.root["CFBundleLocalizations"] as PlistElementArray;
				if (array == null)
				{
					array = doc.root.CreateArray("CFBundleLocalizations");
				}

				foreach (string language in languages)
				{
					if (!ContainsString(array, language))
					{
						array.AddString(language);
					}
				}
			});
		}
		
		private static bool ContainsString(PlistElementArray array, string value)
		{
			return array.values.Any(element => value.Equals(element.AsString()));
		}
#endif
	}
}
#endif