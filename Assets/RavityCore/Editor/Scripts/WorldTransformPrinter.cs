#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RavityCore.Editor
{
    public static class WorldTransformPrinter
    {
        [MenuItem("Ravity/Print World Transforms")]
        public static void PrintWorldTransforms()
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            foreach (GameObject gameObject in selectedObjects)
            {
                Debug.Log($"Transform of '{gameObject.name}':\n" +
                          $"Position: {gameObject.transform.position.ToString("F3")}, " +
                          $"Rotation: {gameObject.transform.eulerAngles.ToString("F3")}, " +
                          $"Scale: {gameObject.transform.lossyScale.ToString("F3")}");
            }
        }
    }
}
#endif
