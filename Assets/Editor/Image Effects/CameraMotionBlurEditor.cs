using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
[UnityEditor.CustomEditor(typeof(CameraMotionBlur))]
public class CameraMotionBlurEditor : Editor
{
    public SerializedObject serObj;
    public SerializedProperty filterType;
    public SerializedProperty _preview;
    public SerializedProperty previewScale;
    public SerializedProperty movementScale;
    public SerializedProperty jitter;
    public SerializedProperty rotationScale;
    public SerializedProperty maxVelocity;
    public SerializedProperty minVelocity;
    public SerializedProperty maxNumSamples;
    public SerializedProperty velocityScale;
    public SerializedProperty velocityDownsample;
    public SerializedProperty noiseTexture;
    public SerializedProperty showVelocity;
    public SerializedProperty showVelocityScale;
    public SerializedProperty excludeLayers;
    //var dynamicLayers : SerializedProperty;
    public virtual void OnEnable()
    {
        this.serObj = new SerializedObject(this.target);
        this.filterType = this.serObj.FindProperty("filterType");
        this._preview = this.serObj.FindProperty("preview");
        this.previewScale = this.serObj.FindProperty("previewScale");
        this.movementScale = this.serObj.FindProperty("movementScale");
        this.rotationScale = this.serObj.FindProperty("rotationScale");
        this.maxVelocity = this.serObj.FindProperty("maxVelocity");
        this.minVelocity = this.serObj.FindProperty("minVelocity");
        this.maxNumSamples = this.serObj.FindProperty("maxNumSamples");
        this.jitter = this.serObj.FindProperty("jitter");
        this.excludeLayers = this.serObj.FindProperty("excludeLayers");
        //dynamicLayers = serObj.FindProperty ("dynamicLayers");
        this.velocityScale = this.serObj.FindProperty("velocityScale");
        this.velocityDownsample = this.serObj.FindProperty("velocityDownsample");
        this.noiseTexture = this.serObj.FindProperty("noiseTexture");
    }

    public override void OnInspectorGUI()
    {
        this.serObj.Update();
        EditorGUILayout.LabelField("Simulates camera based motion blur", EditorStyles.miniLabel, new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.filterType, new GUIContent("Technique"), new GUILayoutOption[] {});
        if ((this.filterType.enumValueIndex == 3) && !(this.target as CameraMotionBlur).Dx11Support())
        {
            EditorGUILayout.HelpBox("DX11 mode not supported (need shader model 5)", MessageType.Info);
        }
        EditorGUILayout.PropertyField(this.velocityScale, new GUIContent(" Velocity Scale"), new GUILayoutOption[] {});
        if (this.filterType.enumValueIndex >= 2)
        {
            EditorGUILayout.LabelField(" Tile size used during reconstruction filter:", EditorStyles.miniLabel, new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.maxVelocity, new GUIContent("  Velocity Max"), new GUILayoutOption[] {});
        }
        else
        {
            EditorGUILayout.PropertyField(this.maxVelocity, new GUIContent(" Velocity Max"), new GUILayoutOption[] {});
        }
        EditorGUILayout.PropertyField(this.minVelocity, new GUIContent(" Velocity Min"), new GUILayoutOption[] {});
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Technique Specific", new GUILayoutOption[] {});
        if (this.filterType.enumValueIndex == 0)
        {
            // portal style motion blur
            EditorGUILayout.PropertyField(this.rotationScale, new GUIContent(" Camera Rotation"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.movementScale, new GUIContent(" Camera Movement"), new GUILayoutOption[] {});
        }
        else
        {
            // "plausible" blur or cheap, local blur
            EditorGUILayout.PropertyField(this.excludeLayers, new GUIContent(" Exclude Layers"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.velocityDownsample, new GUIContent(" Velocity Downsample"), new GUILayoutOption[] {});
            this.velocityDownsample.intValue = this.velocityDownsample.intValue < 1 ? 1 : this.velocityDownsample.intValue;
            if (this.filterType.enumValueIndex >= 2) // only display jitter for reconstruction
            {
                EditorGUILayout.PropertyField(this.noiseTexture, new GUIContent(" Sample Jitter"), new GUILayoutOption[] {});
                EditorGUILayout.PropertyField(this.jitter, new GUIContent("  Jitter Strength"), new GUILayoutOption[] {});
            }
        }
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(this._preview, new GUIContent("Preview"), new GUILayoutOption[] {});
        if (this._preview.boolValue)
        {
            EditorGUILayout.PropertyField(this.previewScale, new GUIContent(""), new GUILayoutOption[] {});
        }
        this.serObj.ApplyModifiedProperties();
    }

}