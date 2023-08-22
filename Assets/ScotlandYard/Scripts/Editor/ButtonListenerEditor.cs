using UnityEditor;

[CustomEditor(typeof(ButtonListener), true)]
public class ButtonListenerEditor : Editor
{
    public override void OnInspectorGUI ()
    {
        ButtonListener button = target as ButtonListener;
        NGUIEditorTools.DrawEvents("On Click", button, button._onClickDelegate);

        SerializedProperty serializedProperty = serializedObject.FindProperty("_animations");
        EditorGUILayout.PropertyField(serializedProperty);
        serializedObject.ApplyModifiedProperties();
    }
}
