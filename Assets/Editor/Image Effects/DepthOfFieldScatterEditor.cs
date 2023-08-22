using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
[UnityEditor.CustomEditor(typeof(DepthOfFieldScatter))]
public class DepthOfFieldScatterEditor : Editor
{
    public SerializedObject serObj;
    public SerializedProperty visualizeFocus;
    public SerializedProperty focalLength;
    public SerializedProperty focalSize;
    public SerializedProperty aperture;
    public SerializedProperty focalTransform;
    public SerializedProperty maxBlurSize;
    public SerializedProperty highResolution;
    public SerializedProperty blurType;
    public SerializedProperty blurSampleCount;
    public SerializedProperty nearBlur;
    public SerializedProperty foregroundOverlap;
    public SerializedProperty dx11BokehThreshhold;
    public SerializedProperty dx11SpawnHeuristic;
    public SerializedProperty dx11BokehTexture;
    public SerializedProperty dx11BokehScale;
    public SerializedProperty dx11BokehIntensity;
    public virtual void OnEnable()
    {
        this.serObj = new SerializedObject(this.target);
        this.visualizeFocus = this.serObj.FindProperty("visualizeFocus");
        this.focalLength = this.serObj.FindProperty("focalLength");
        this.focalSize = this.serObj.FindProperty("focalSize");
        this.aperture = this.serObj.FindProperty("aperture");
        this.focalTransform = this.serObj.FindProperty("focalTransform");
        this.maxBlurSize = this.serObj.FindProperty("maxBlurSize");
        this.highResolution = this.serObj.FindProperty("highResolution");
        this.blurType = this.serObj.FindProperty("blurType");
        this.blurSampleCount = this.serObj.FindProperty("blurSampleCount");
        this.nearBlur = this.serObj.FindProperty("nearBlur");
        this.foregroundOverlap = this.serObj.FindProperty("foregroundOverlap");
        this.dx11BokehThreshhold = this.serObj.FindProperty("dx11BokehThreshhold");
        this.dx11SpawnHeuristic = this.serObj.FindProperty("dx11SpawnHeuristic");
        this.dx11BokehTexture = this.serObj.FindProperty("dx11BokehTexture");
        this.dx11BokehScale = this.serObj.FindProperty("dx11BokehScale");
        this.dx11BokehIntensity = this.serObj.FindProperty("dx11BokehIntensity");
    }

    public override void OnInspectorGUI()
    {
        this.serObj.Update();
        EditorGUILayout.LabelField("Simulates camera lens defocus", EditorStyles.miniLabel, new GUILayoutOption[] {});
        GUILayout.Label("Focal Settings", new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.visualizeFocus, new GUIContent(" Visualize"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.focalLength, new GUIContent(" Focal Distance"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.focalSize, new GUIContent(" Focal Size"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.focalTransform, new GUIContent(" Focus on Transform"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.aperture, new GUIContent(" Aperture"), new GUILayoutOption[] {});
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(this.blurType, new GUIContent("Defocus Type"), new GUILayoutOption[] {});
        if (!(this.target as DepthOfFieldScatter).Dx11Support() && (this.blurType.enumValueIndex > 0))
        {
            EditorGUILayout.HelpBox("DX11 mode not supported (need shader model 5)", MessageType.Info);
        }
        if (this.blurType.enumValueIndex < 1)
        {
            EditorGUILayout.PropertyField(this.blurSampleCount, new GUIContent(" Sample Count"), new GUILayoutOption[] {});
        }
        EditorGUILayout.PropertyField(this.maxBlurSize, new GUIContent(" Max Blur Distance"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.highResolution, new GUIContent(" High Resolution"), new GUILayoutOption[] {});
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(this.nearBlur, new GUIContent("Near Blur"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.foregroundOverlap, new GUIContent("  Overlap Size"), new GUILayoutOption[] {});
        EditorGUILayout.Separator();
        if (this.blurType.enumValueIndex > 0)
        {
            GUILayout.Label("DX11 Bokeh Settings", new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.dx11BokehTexture, new GUIContent(" Bokeh Texture"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.dx11BokehScale, new GUIContent(" Bokeh Scale"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.dx11BokehIntensity, new GUIContent(" Bokeh Intensity"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.dx11BokehThreshhold, new GUIContent(" Min Luminance"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.dx11SpawnHeuristic, new GUIContent(" Spawn Heuristic"), new GUILayoutOption[] {});
        }
        this.serObj.ApplyModifiedProperties();
    }

}