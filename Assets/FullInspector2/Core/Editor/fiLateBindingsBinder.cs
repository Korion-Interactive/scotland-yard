using UnityEditor;

namespace FullInspector.Internal {

    // note: See the docs on fiLateBindings
    //       This is just the actual injection code which only gets run if we're in an editor
    //
    // note: If there is ever a binding that doesn't occur quickly enough, then we can use
    //       reflection to discover it immediately

    [InitializeOnLoad]
    public class fiLateBindingsBinder {
        static fiLateBindingsBinder() {
            fiLateBindings._Bindings._EditorUtility_SetDirty = EditorUtility.SetDirty;
            fiLateBindings._Bindings._EditorUtility_InstanceIdToObject = EditorUtility.InstanceIDToObject;
            fiLateBindings._Bindings._EditorUtility_CreateGameObjectWithHideFlags = (name, flags) => EditorUtility.CreateGameObjectWithHideFlags(name, flags);
            fiLateBindings._Bindings._PrefabUtility_IsPrefab = unityObj => PrefabUtility.GetPrefabType(unityObj) == PrefabType.Prefab;
            fiLateBindings._Bindings._EditorApplication_isPlaying = () => EditorApplication.isPlaying;
            fiLateBindings._Bindings._Selection_activeObject = () => Selection.activeObject;
            fiLateBindings._Bindings._EditorPrefs_GetString = EditorPrefs.GetString;
            fiLateBindings._Bindings._EditorPrefs_SetString = EditorPrefs.SetString;
        }
    }
}
