using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
[UnityEditor.CustomEditor(typeof(DepthOfField34))]
public class DepthOfField34Editor : Editor
{
    public SerializedObject serObj;
    public SerializedProperty simpleTweakMode;
    public SerializedProperty focalPoint;
    public SerializedProperty smoothness;
    public SerializedProperty focalSize;
    public SerializedProperty focalZDistance;
    public SerializedProperty focalStartCurve;
    public SerializedProperty focalEndCurve;
    public SerializedProperty visualizeCoc;
    public SerializedProperty resolution;
    public SerializedProperty quality;
    public SerializedProperty objectFocus;
    public SerializedProperty bokeh;
    public SerializedProperty bokehScale;
    public SerializedProperty bokehIntensity;
    public SerializedProperty bokehThreshholdLuminance;
    public SerializedProperty bokehThreshholdContrast;
    public SerializedProperty bokehDownsample;
    public SerializedProperty bokehTexture;
    public SerializedProperty bokehDestination;
    public SerializedProperty bluriness;
    public SerializedProperty maxBlurSpread;
    public SerializedProperty foregroundBlurExtrude;
    public virtual void OnEnable()
    {
        this.serObj = new SerializedObject(this.target);
        this.simpleTweakMode = this.serObj.FindProperty("simpleTweakMode");
        // simple tweak mode
        this.focalPoint = this.serObj.FindProperty("focalPoint");
        this.smoothness = this.serObj.FindProperty("smoothness");
        // complex tweak mode
        this.focalZDistance = this.serObj.FindProperty("focalZDistance");
        this.focalStartCurve = this.serObj.FindProperty("focalZStartCurve");
        this.focalEndCurve = this.serObj.FindProperty("focalZEndCurve");
        this.focalSize = this.serObj.FindProperty("focalSize");
        this.visualizeCoc = this.serObj.FindProperty("visualize");
        this.objectFocus = this.serObj.FindProperty("objectFocus");
        this.resolution = this.serObj.FindProperty("resolution");
        this.quality = this.serObj.FindProperty("quality");
        this.bokehThreshholdContrast = this.serObj.FindProperty("bokehThreshholdContrast");
        this.bokehThreshholdLuminance = this.serObj.FindProperty("bokehThreshholdLuminance");
        this.bokeh = this.serObj.FindProperty("bokeh");
        this.bokehScale = this.serObj.FindProperty("bokehScale");
        this.bokehIntensity = this.serObj.FindProperty("bokehIntensity");
        this.bokehDownsample = this.serObj.FindProperty("bokehDownsample");
        this.bokehTexture = this.serObj.FindProperty("bokehTexture");
        this.bokehDestination = this.serObj.FindProperty("bokehDestination");
        this.bluriness = this.serObj.FindProperty("bluriness");
        this.maxBlurSpread = this.serObj.FindProperty("maxBlurSpread");
        this.foregroundBlurExtrude = this.serObj.FindProperty("foregroundBlurExtrude");
    }

    public override void OnInspectorGUI()
    {
        this.serObj.Update();
        GameObject go = (this.target as DepthOfField34).gameObject;
        if (!go)
        {
            return;
        }
        if (!go.GetComponent<Camera>())
        {
            return;
        }
        if (this.simpleTweakMode.boolValue)
        {
            GUILayout.Label((((((("Current: " + go.GetComponent<Camera>().name) + ", near ") + go.GetComponent<Camera>().nearClipPlane) + ", far: ") + go.GetComponent<Camera>().farClipPlane) + ", focal: ") + this.focalPoint.floatValue, EditorStyles.miniBoldLabel, new GUILayoutOption[] {});
        }
        else
        {
            GUILayout.Label((((((("Current: " + go.GetComponent<Camera>().name) + ", near ") + go.GetComponent<Camera>().nearClipPlane) + ", far: ") + go.GetComponent<Camera>().farClipPlane) + ", focal: ") + this.focalZDistance.floatValue, EditorStyles.miniBoldLabel, new GUILayoutOption[] {});
        }
        EditorGUILayout.PropertyField(this.resolution, new GUIContent("Resolution"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.quality, new GUIContent("Quality"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.simpleTweakMode, new GUIContent("Simple tweak"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.visualizeCoc, new GUIContent("Visualize focus"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.bokeh, new GUIContent("Enable bokeh"), new GUILayoutOption[] {});
        EditorGUILayout.Separator();
        GUILayout.Label("Focal Settings", EditorStyles.boldLabel, new GUILayoutOption[] {});
        if (this.simpleTweakMode.boolValue)
        {
            this.focalPoint.floatValue = EditorGUILayout.Slider("Focal distance", this.focalPoint.floatValue, go.GetComponent<Camera>().nearClipPlane, go.GetComponent<Camera>().farClipPlane, new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.objectFocus, new GUIContent("Transform"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.smoothness, new GUIContent("Smoothness"), new GUILayoutOption[] {});
            this.focalSize.floatValue = EditorGUILayout.Slider("Focal size", this.focalSize.floatValue, 0f, go.GetComponent<Camera>().farClipPlane - go.GetComponent<Camera>().nearClipPlane, new GUILayoutOption[] {});
        }
        else
        {
            this.focalZDistance.floatValue = EditorGUILayout.Slider("Distance", this.focalZDistance.floatValue, go.GetComponent<Camera>().nearClipPlane, go.GetComponent<Camera>().farClipPlane, new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.objectFocus, new GUIContent("Transform"), new GUILayoutOption[] {});
            this.focalSize.floatValue = EditorGUILayout.Slider("Size", this.focalSize.floatValue, 0f, go.GetComponent<Camera>().farClipPlane - go.GetComponent<Camera>().nearClipPlane, new GUILayoutOption[] {});
            this.focalStartCurve.floatValue = EditorGUILayout.Slider("Start curve", this.focalStartCurve.floatValue, 0.05f, 20f, new GUILayoutOption[] {});
            this.focalEndCurve.floatValue = EditorGUILayout.Slider("End curve", this.focalEndCurve.floatValue, 0.05f, 20f, new GUILayoutOption[] {});
        }
        EditorGUILayout.Separator();
        GUILayout.Label("Blur (Fore- and Background)", EditorStyles.boldLabel, new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.bluriness, new GUIContent("Blurriness"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.maxBlurSpread, new GUIContent("Blur spread"), new GUILayoutOption[] {});
        if (this.quality.enumValueIndex > 0)
        {
            EditorGUILayout.PropertyField(this.foregroundBlurExtrude, new GUIContent("Foreground size"), new GUILayoutOption[] {});
        }
        EditorGUILayout.Separator();
        if (this.bokeh.boolValue)
        {
            EditorGUILayout.Separator();
            GUILayout.Label("Bokeh Settings", EditorStyles.boldLabel, new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.bokehDestination, new GUIContent("Destination"), new GUILayoutOption[] {});
            this.bokehIntensity.floatValue = EditorGUILayout.Slider("Intensity", this.bokehIntensity.floatValue, 0f, 1f, new GUILayoutOption[] {});
            this.bokehThreshholdLuminance.floatValue = EditorGUILayout.Slider("Min luminance", this.bokehThreshholdLuminance.floatValue, 0f, 0.99f, new GUILayoutOption[] {});
            this.bokehThreshholdContrast.floatValue = EditorGUILayout.Slider("Min contrast", this.bokehThreshholdContrast.floatValue, 0f, 0.25f, new GUILayoutOption[] {});
            this.bokehDownsample.intValue = EditorGUILayout.IntSlider("Downsample", this.bokehDownsample.intValue, 1, 3, new GUILayoutOption[] {});
            this.bokehScale.floatValue = EditorGUILayout.Slider("Size scale", this.bokehScale.floatValue, 0f, 20f, new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.bokehTexture, new GUIContent("Texture mask"), new GUILayoutOption[] {});
        }
        this.serObj.ApplyModifiedProperties();
    }

}