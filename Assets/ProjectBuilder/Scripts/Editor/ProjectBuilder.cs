#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Ravity.ProjectBuilder
{
	public class ProjectBuilder
	{
		public virtual void DrawEditorGUI() {}

		public virtual void PreBuild() { }
		
		public virtual void Build(BuildOptions buildOptions)
		{
			string filepath = GetBuildFilePath();
			Build(buildOptions, ActiveBuildTarget, filepath);
		}

		// ReSharper disable once MemberCanBePrivate.Global
		// ReSharper disable once MemberCanBeMadeStatic.Global
		protected void Build(BuildOptions buildOptions, BuildTarget buildTarget, string filepath)
		{
			// get enabled levels
			List<string> levels = new List<string>();
			foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
			{
				if (scene.enabled)
				{
					levels.Add(scene.path);
				}
			}

			// measure build time
			DateTime timeStart = DateTime.Now;
			
			// build
			BuildPlayerOptions options = new BuildPlayerOptions();
			options.scenes = levels.ToArray();
			options.locationPathName = filepath;
			options.target = buildTarget;
			options.targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
			options.options = buildOptions;
			BuildPipeline.BuildPlayer(options);

			// output build time, etc.
			StringBuilder info = new StringBuilder($"{nameof(ProjectBuilder)}: ");
			TimeSpan timeSpan = DateTime.Now.Subtract(timeStart);
			info.Append(filepath);
			info.Append(" had a build time of ");
			#if NET_2_0 || NET_2_0_SUBSET
				info.Append(timeSpan.ToString());
			#else
				info.Append(timeSpan.ToString(@"hh\:mm\:ss"));
			#endif
			info.Append(".");
			Debug.Log(info.ToString());
		}
		
		private string GetBuildFilePath()
		{
			// Build
			string filepath = GetBuildDirectoryAbsolutePath(ActiveBuildTarget.ToString());
			Directory.CreateDirectory(filepath);

			// product name allow only latin letters and digits
			string productName = PlayerSettings.productName;
			string filteredProductName = System.Text.RegularExpressions.Regex.Replace(productName, @"[^0-9a-zA-Z]+", "");

			// append date time and filename suffix
			bool appendDateTime = false;
			string subfolder = string.Empty;
			string fileNameSuffix = string.Empty;

			if (ActiveBuildTarget == BuildTarget.Android)
			{
				appendDateTime = true;
				fileNameSuffix = EditorUserBuildSettings.buildAppBundle ? ".aab" : ".apk";
			}
			else if (ActiveBuildTarget == BuildTarget.StandaloneOSX
			         || ActiveBuildTarget == BuildTarget.StandaloneWindows
			         || ActiveBuildTarget == BuildTarget.StandaloneWindows64)
			{
				appendDateTime = true;
				if (ActiveBuildTarget == BuildTarget.StandaloneOSX)
				{
					fileNameSuffix = "/" + productName + ".app";
				}
				else if (ActiveBuildTarget == BuildTarget.StandaloneWindows ||
				         ActiveBuildTarget == BuildTarget.StandaloneWindows64)
				{
					fileNameSuffix = "/" + productName + ".exe";
				}
			}

			// apply subfolder and product name
			filepath += subfolder;
			filepath += filteredProductName;

			if (appendDateTime)
			{
				DateTime now = DateTime.Now;
				filepath += $"_{now.Year}-{now.Month:00}-{now.Day:00}_{now.Hour:00}-{now.Minute:00}-{now.Second:00}";
				filepath += $"_v{PlayerSettings.bundleVersion}";
			}

			filepath += fileNameSuffix;
			return filepath;
		}
		
		protected string GetBuildDirectoryAbsolutePath(string platform)
		{
			string folder = Application.dataPath + "/../../builds/" + platform + "/";
			string filepath = new FileInfo(folder).FullName;
			return filepath;
		}

		private static ProjectBuilder _instance = null;
		public static ProjectBuilder Instance
		{
			get
			{
				if (_instance == null)
				{
					// use reflection to get instance from all assemblies
					foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
					{
						Type[] types = assembly.GetTypes().Where(t => t.BaseType == typeof(ProjectBuilder)).ToArray();
						foreach (Type type in types)
						{
							_instance = Activator.CreateInstance(type) as ProjectBuilder;
							if (types.Length > 1)
							{
								Debug.LogError($"Found multiple inheritors of {typeof(ProjectBuilder)} using '{type}'");
							}
							return _instance;
						}
					}
					_instance = new ProjectBuilder();
				}
				return _instance;
			}
		}
		
		// ReSharper disable once UnusedMember.Global
		// (called from Unity Cloud Build)
		public static void UnityCloudBuildPreExport()
		{
			Instance.PreBuild();
		}
		
		public static BuildTarget ActiveBuildTarget => EditorUserBuildSettings.activeBuildTarget;
	}
}
#endif
