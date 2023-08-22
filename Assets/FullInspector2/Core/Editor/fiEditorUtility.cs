using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Internal {
    /// <summary>
    /// Utility class that is enabled when it has been pushed to.
    /// </summary>
    public class StackEnabled {
        private int _count;
        public void Push() {
            ++_count;
        }
        public void Pop() {
            --_count;
            if (_count < 0) _count = 0;
        }
        public bool Enabled {
            get {
                return _count > 0;
            }
        }
    }

    /// <summary>
    /// This class is used to cache results for some expensive fiEditorUtility method calls.
    /// </summary>
    public class fiEditorUtilityCache : UnityEditor.AssetModificationProcessor {
        public static void OnWillCreateAsset(string path) {
            ClearCache();
        }
        public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options) {
            // NOTE: this method is only called when the user has a TeamLicense
            ClearCache();
            return AssetDeleteResult.DidNotDelete;
        }
        public static void ClearCache() {
            CachedAssetLookups = new Dictionary<Type, List<UnityObject>>();
            CachedPrefabLookups = new Dictionary<Type, List<UnityObject>>();
            CachedScenes = null;
        }
        public static Dictionary<Type, List<UnityObject>> CachedAssetLookups = new Dictionary<Type, List<UnityObject>>();
        public static Dictionary<Type, List<UnityObject>> CachedPrefabLookups = new Dictionary<Type, List<UnityObject>>();
        public static List<string> CachedScenes;
    }

    public static class fiEditorUtility {
        /// <summary>
        /// Returns the paths of all .scene files in the Unity project.
        /// </summary>
        public static List<string> GetAllScenes() {
            if (fiEditorUtilityCache.CachedScenes == null) {
                List<string> found = new List<string>();
                string fileExtension = ".unity";

                string[] files = Directory.GetFiles(Application.dataPath, "*" + fileExtension, SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; ++i) {
                    string file = files[i];
                    file = file.Replace(Application.dataPath, "Assets");
                    file = file.Replace("\\", "/");
                    found.Add(file);
                }

                fiEditorUtilityCache.CachedScenes = found;
            }

            return fiEditorUtilityCache.CachedScenes;
        }

        /// <summary>
        /// Find all prefabs of a given type, regardless of location.
        /// </summary>
        /// <param name="type">The type of object to fetch</param>
        /// <remarks>Please note that this method can return UnityObject instances that have been deleted.</remarks>
        public static List<UnityObject> GetAllPrefabsOfType(Type type) {
            List<UnityObject> found;

            if (fiEditorUtilityCache.CachedPrefabLookups.TryGetValue(type, out found) == false) {
                string fileExtension = ".prefab";

                found = new List<UnityObject>();
                string[] files = Directory.GetFiles(Application.dataPath, "*" + fileExtension, SearchOption.AllDirectories);

                for (int i = 0; i < files.Length; ++i) {
                    string file = files[i];
                    file = file.Replace(Application.dataPath, "Assets");

                    var obj = AssetDatabase.LoadAssetAtPath(file, type);
                    if (obj != null) {
                        found.Add(obj);
                    }
                }

                fiEditorUtilityCache.CachedPrefabLookups[type] = found;
            }

            return found;
        }

        /// <summary>
        /// Find all assets of a given type, regardless of location.
        /// </summary>
        /// <param name="type">The (ScriptableObject derived) type of object to fetch</param>
        /// <remarks>Please note that this method can return UnityObject instances that have been deleted.</remarks>
        public static List<UnityObject> GetAllAssetsOfType(Type type) {
            List<UnityObject> found;

            if (fiEditorUtilityCache.CachedAssetLookups.TryGetValue(type, out found) == false) {
                string fileExtension = ".asset";

                found = new List<UnityObject>();
                string[] files = Directory.GetFiles(Application.dataPath, "*" + fileExtension, SearchOption.AllDirectories);

                for (int i = 0; i < files.Length; ++i) {
                    string file = files[i];
                    file = file.Replace(Application.dataPath, "Assets");

                    var obj = AssetDatabase.LoadAssetAtPath(file, type);
                    if (obj != null) {
                        found.Add(obj);
                    }
                }

                fiEditorUtilityCache.CachedAssetLookups[type] = found;
            }

            return found;
        }

        /// <summary>
        /// Returns the amount of width that should be used for a label for the given total width.
        /// </summary>
        public static float GetLabelWidth(float width) {
            width = width * fiSettings.LabelWidthPercentage;
            width = Mathf.Max(width, fiSettings.LabelWidthMin);
            width = Mathf.Min(width, fiSettings.LabelWidthMax);
            return width;
        }

        /// <summary>
        /// If enabled, then the inspector should be constantly redrawn. This is used to work around
        /// some rendering issues within Unity.
        /// </summary>
        public static StackEnabled ShouldInspectorRedraw = new StackEnabled();

        /// <summary>
        /// If set to true by editor code, then the inspector will repaint.
        /// </summary>
        public static bool Repaint = false;

        /// <summary>
        /// Attempts to fetch a MonoScript that is associated with the given obj.
        /// </summary>
        /// <param name="obj">The object to fetch the script for.</param>
        /// <param name="script">The script, if found.</param>
        /// <returns>True if there was a script, false otherwise.</returns>
        public static bool TryGetMonoScript(object obj, out MonoScript script) {
            script = null;

            if (obj is MonoBehaviour) {
                var behavior = (MonoBehaviour)obj;
                script = MonoScript.FromMonoBehaviour(behavior);
            }

            else if (obj is ScriptableObject) {
                var scriptable = (ScriptableObject)obj;
                script = MonoScript.FromScriptableObject(scriptable);
            }

            return script != null;
        }

        /// <summary>
        /// Returns true if the given obj has a MonoScript associated with it.
        /// </summary>
        public static bool HasMonoScript(object obj) {
            MonoScript script;
            return TryGetMonoScript(obj, out script);
        }
    }
}