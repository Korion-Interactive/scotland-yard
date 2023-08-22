using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Internal.Versioning {
    [InitializeOnLoad]
    public class fiVersionManager {
        /// <summary>
        /// The current version string.
        /// </summary>
        public static string CurrentVersion = "2.5";

        /// <summary>
        /// Full Inspector will effectively force the user to do a clean import if any of the following types are found in
        /// the assembly.
        /// </summary>
        private static string[] OldVersionMarkers = 
            {
                "FullInspector.Internal.Versioning.fiImportVersionMarker_24"
            };

        static fiVersionManager() {
            foreach (var oldVersion in OldVersionMarkers) {
                Type oldType = TypeCache.FindType(oldVersion);
                if (oldType != null) {

                    if (EditorUtility.DisplayDialog("Clean Import Needed", "Full Inspector has detected that you recently upgraded versions but did not do a " +
                        "clean import. This will lead to subtle errors." + Environment.NewLine + Environment.NewLine + "Please delete the \"" +
                        fiSettings.RootDirectory + "\" folder and reimport Full Inspector.", "Delete folder?", "I'll do it later")) {

                        Debug.Log("Deleting \"" + fiSettings.RootDirectory + "\"");
                        Directory.Delete(fiSettings.RootDirectory, /*recursive:*/ true);

                        string metapath = fiSettings.RootDirectory.TrimEnd('/') + ".meta";
                        Debug.Log("Deleting \"" + metapath + "\"");
                        File.Delete(metapath);
                    }
                }
            }
        }
    }
}