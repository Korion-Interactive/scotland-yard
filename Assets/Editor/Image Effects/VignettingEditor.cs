using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
[UnityEditor.CustomEditor(typeof(Vignetting))]
public class VignettingEditor : Editor
{
    public SerializedObject serObj;
    public SerializedProperty mode;
    public SerializedProperty intensity; // intensity == 0 disables pre pass (optimization)
    public SerializedProperty chromaticAberration;
    public SerializedProperty axialAberration;
    public SerializedProperty blur; // blur == 0 disables blur pass (optimization)
    public SerializedProperty blurSpread;
    public SerializedProperty blurDistance;
    public SerializedProperty luminanceDependency;
    public virtual void OnEnable()
    {
        this.serObj = new SerializedObject(this.target);
        this.mode = this.serObj.FindProperty("mode");
        this.intensity = this.serObj.FindProperty("intensity");
        this.chromaticAberration = this.serObj.FindProperty("chromaticAberration");
        this.axialAberration = this.serObj.FindProperty("axialAberration");
        this.blur = this.serObj.FindProperty("blur");
        this.blurSpread = this.serObj.FindProperty("blurSpread");
        this.luminanceDependency = this.serObj.FindProperty("luminanceDependency");
        this.blurDistance = this.serObj.FindProperty("blurDistance");
    }

    public override void OnInspectorGUI()
    {
        this.serObj.Update();
        EditorGUILayout.LabelField("Simulates the common lens artifacts 'Vignette' and 'Aberration'", EditorStyles.miniLabel, new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.intensity, new GUIContent("Vignetting"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.blur, new GUIContent(" Blurred Corners"), new GUILayoutOption[] {});
        if (this.blur.floatValue > 0f)
        {
            EditorGUILayout.PropertyField(this.blurSpread, new GUIContent(" Blur Distance"), new GUILayoutOption[] {});
        }
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(this.mode, new GUIContent("Aberration"), new GUILayoutOption[] {});
        if (this.mode.intValue > 0)
        {
            EditorGUILayout.PropertyField(this.chromaticAberration, new GUIContent("  Tangential Aberration"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.axialAberration, new GUIContent("  Axial Aberration"), new GUILayoutOption[] {});
            this.luminanceDependency.floatValue = EditorGUILayout.Slider("  Contrast Dependency", this.luminanceDependency.floatValue, 0.001f, 1f, new GUILayoutOption[] {});
            this.blurDistance.floatValue = EditorGUILayout.Slider("  Blur Distance", this.blurDistance.floatValue, 0.001f, 5f, new GUILayoutOption[] {});
        }
        else
        {
            EditorGUILayout.PropertyField(this.chromaticAberration, new GUIContent(" Chromatic Aberration"), new GUILayoutOption[] {});
        }
        this.serObj.ApplyModifiedProperties();
    }

}