#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Ravity.Editor
{
    public static class FolderUtils
    {
        [MenuItem("Ravity/Open Folders/Persistent Data Path")]
        public static void OpenPersistenceDataPath()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }
        
        [MenuItem("Ravity/Open Folders/Temporary Cache")]
        public static void OpenTemporaryCachePath()
        {
            EditorUtility.OpenWithDefaultApp(Application.temporaryCachePath);
        }
    }
}
#endif