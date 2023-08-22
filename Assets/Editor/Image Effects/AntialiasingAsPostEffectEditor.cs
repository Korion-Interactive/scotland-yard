using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
[UnityEditor.CustomEditor(typeof(AntialiasingAsPostEffect))]
public class AntialiasingAsPostEffectEditor : Editor
{
    public SerializedObject serObj;
    public SerializedProperty mode;
    public SerializedProperty showGeneratedNormals;
    public SerializedProperty offsetScale;
    public SerializedProperty blurRadius;
    public SerializedProperty dlaaSharp;
    public SerializedProperty edgeThresholdMin;
    public SerializedProperty edgeThreshold;
    public SerializedProperty edgeSharpness;
    public virtual void OnEnable()
    {
        this.serObj = new SerializedObject(this.target);
        this.mode = this.serObj.FindProperty("mode");
        this.showGeneratedNormals = this.serObj.FindProperty("showGeneratedNormals");
        this.offsetScale = this.serObj.FindProperty("offsetScale");
        this.blurRadius = this.serObj.FindProperty("blurRadius");
        this.dlaaSharp = this.serObj.FindProperty("dlaaSharp");
        this.edgeThresholdMin = this.serObj.FindProperty("edgeThresholdMin");
        this.edgeThreshold = this.serObj.FindProperty("edgeThreshold");
        this.edgeSharpness = this.serObj.FindProperty("edgeSharpness");
    }

    public override void OnInspectorGUI()
    {
        this.serObj.Update();
        GUILayout.Label("Luminance based fullscreen antialiasing", EditorStyles.miniBoldLabel, new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.mode, new GUIContent("Technique"), new GUILayoutOption[] {});
        Material mat = (this.target as AntialiasingAsPostEffect).CurrentAAMaterial();
        if ((null == mat) && (this.target as AntialiasingAsPostEffect).enabled)
        {
            EditorGUILayout.HelpBox("This AA technique is currently not supported. Choose a different technique or disable the effect and use MSAA instead.", MessageType.Warning);
        }
        if (((AAMode) this.mode.enumValueIndex) == AAMode.NFAA)
        {
            EditorGUILayout.PropertyField(this.offsetScale, new GUIContent("Edge Detect Ofs"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.blurRadius, new GUIContent("Blur Radius"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.showGeneratedNormals, new GUIContent("Show Normals"), new GUILayoutOption[] {});
        }
        else
        {
            if (((AAMode) this.mode.enumValueIndex) == AAMode.DLAA)
            {
                EditorGUILayout.PropertyField(this.dlaaSharp, new GUIContent("Sharp"), new GUILayoutOption[] {});
            }
            else
            {
                if (((AAMode) this.mode.enumValueIndex) == AAMode.FXAA3Console)
                {
                    EditorGUILayout.PropertyField(this.edgeThresholdMin, new GUIContent("Edge Min Threshhold"), new GUILayoutOption[] {});
                    EditorGUILayout.PropertyField(this.edgeThreshold, new GUIContent("Edge Threshhold"), new GUILayoutOption[] {});
                    EditorGUILayout.PropertyField(this.edgeSharpness, new GUIContent("Edge Sharpness"), new GUILayoutOption[] {});
                }
            }
        }
        this.serObj.ApplyModifiedProperties();
    }

}