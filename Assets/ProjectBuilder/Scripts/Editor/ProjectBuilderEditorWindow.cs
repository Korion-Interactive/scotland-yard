#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Ravity.ProjectBuilder
{
	public class ProjectBuilderEditorWindow : EditorWindow
	{
		[MenuItem("Ravity/Open Project Builder",isValidateFunction:false,priority:0)]
		public static void OpenProjectBuilderEditorWindow () 
		{
			GetWindow<ProjectBuilderEditorWindow>("Project Builder").Show(false);
		}

		#region GUI
		public void OnGUI () 
		{
			// Current Settings 
			string activeBuildTargetName = ProjectBuilder.ActiveBuildTarget.ToString();
			GUILayout.Label("Platform: " + activeBuildTargetName, EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
				GUILayout.Label("Version: " + PlayerSettings.bundleVersion);
				#if UNITY_IOS
					GUILayout.Label("Build: " + PlayerSettings.iOS.buildNumber);
				#elif UNITY_ANDROID
					GUILayout.Label("Version Code: " + PlayerSettings.Android.bundleVersionCode);
				#elif UNITY_STANDALONE_OSX
					GUILayout.Label("Build: " + PlayerSettings.macOS.buildNumber);
				#endif
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			const int width = 60;
			const int height = 30;
			if (AddButton("Player\nSettings", width,height))
			{
				OpenProjectSettings("Player");
			}
			if (AddButton("Build\nSettings", width,height))
			{
				EditorApplication.ExecuteMenuItem("File/Build Settings...");
			}
			GUILayout.EndHorizontal();

			// draw custom editor GUI 
			ProjectBuilder.Instance.DrawEditorGUI();

			// Build and Run
			GUILayout.BeginHorizontal();
			AddBuildButton("Build",BuildOptions.None);
			AddBuildButton("Build and Run",BuildOptions.AutoRunPlayer);
			GUILayout.EndHorizontal();
		}

		private static bool AddButton(string text, int width, int height)
		{
			return GUILayout.Button(text, 
				GUILayout.MinHeight(height),
				GUILayout.MaxHeight(height),
				GUILayout.MinWidth(width-10),
				GUILayout.MaxWidth(width+10)
			);
		}

		private static bool AddButton(string text)
		{
			return GUILayout.Button(text, GUILayout.MinHeight(25));
		}

		private static void AddBuildButton(string buttonName, BuildOptions buildOptions)
		{
			if (AddButton(buttonName))
			{
				ProjectBuilder.Instance.PreBuild();
				ProjectBuilder.Instance.Build(buildOptions);
			}
		}
		#endregion

		#region Helper 
		private static void OpenProjectSettings(string name)
		{
			SettingsService.OpenProjectSettings("Project/" + name);
		}
		#endregion
	}
}
#endif