using System;
using System.Reflection;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace FullInspector.Internal {
    /// <summary>
    /// Full Inspector has to support running in both DLL and source code mode. This sometimes introduces
    /// issues when non-editor code has to access editor-related code. This is achieved via a late-binding
    /// mechanism; the editor code will automatically inject the relevant pointers into this data
    /// structure. If the binding does not exist yet, then a warning will be emitted.
    /// </summary>
    public static class fiLateBindings {
        public static class _Bindings {
            public static Action<UnityObject> _EditorUtility_SetDirty;
            public static Func<int, UnityObject> _EditorUtility_InstanceIdToObject;
            public static Func<string, HideFlags, GameObject> _EditorUtility_CreateGameObjectWithHideFlags;
            public static Func<UnityObject, bool> _PrefabUtility_IsPrefab;
            public static Func<bool> _EditorApplication_isPlaying;
            public static Func<UnityObject> _Selection_activeObject;
            public static Func<string, string, string> _EditorPrefs_GetString;
            public static Action<string, string> _EditorPrefs_SetString;
        }

        public static class EditorUtility {
            public static void SetDirty(UnityObject unityObject) {
                if (VerifyBinding("EditorUtility.SetDirty", _Bindings._EditorUtility_SetDirty)) {
                    _Bindings._EditorUtility_SetDirty(unityObject);
                }
            }

            public static UnityObject InstanceIDToObject(int instanceId) {
                if (VerifyBinding("EditorUtility.InstanceIdToObject", _Bindings._EditorUtility_InstanceIdToObject)) {
                    return _Bindings._EditorUtility_InstanceIdToObject(instanceId);
                }
                return null;
            }

            public static GameObject CreateGameObjectWithHideFlags(string name, HideFlags hideFlags) {
                if (VerifyBinding("EditorUtility.CreateGameObjectWithHideFlags", _Bindings._EditorUtility_CreateGameObjectWithHideFlags)) {
                    return _Bindings._EditorUtility_CreateGameObjectWithHideFlags(name, hideFlags);
                }

                var go = new GameObject(name);
                go.hideFlags = hideFlags;
                return go;
            }
        }

        public static class PrefabUtility {
            /// <summary>
            /// Returns true if UnityEditor.PrefabUtility.GetPrefabType(unityObj) == UnityEditor.PrefabType.Prefab
            /// </summary>
            public static bool IsPrefab(UnityObject unityObject) {
                if (VerifyBinding("PrefabUtility.IsPrefab", _Bindings._PrefabUtility_IsPrefab)) {
                    return _Bindings._PrefabUtility_IsPrefab(unityObject);
                }
                return false;
            }
        }

        public static class EditorApplication {
            public static bool isPlaying {
                get {
                    if (VerifyBinding("EditorApplication.isPlaying", _Bindings._EditorApplication_isPlaying)) {
                        return _Bindings._EditorApplication_isPlaying();
                    }
                    return true;
                }
            }
        }

        public static class Selection {
            public static UnityObject activeObject {
                get {
                    if (VerifyBinding("Selection.activeObject", _Bindings._Selection_activeObject)) {
                        return _Bindings._Selection_activeObject();
                    }
                    return null;
                }
            }
        }

        public static class EditorPrefs {

            public static string GetString(string key, string defaultValue) {
                if (VerifyBinding("EditorPrefs.GetString", _Bindings._EditorPrefs_GetString)) {
                    return _Bindings._EditorPrefs_GetString(key, defaultValue);
                }
                return defaultValue;
            }

            public static void SetString(string key, string value) {
                if (VerifyBinding("EditorPrefs.SetString", _Bindings._EditorPrefs_SetString)) {
                    _Bindings._EditorPrefs_SetString(key, value);
                }
            }
        }

        private static bool VerifyBinding(string name, object obj) {
            if (obj == null) {
                if (Application.isEditor) {
                    Debug.Log("There is no binding for " + name + " even though we are in an editor");
                }

                return false;
            }

            return true;
        }
    }
}