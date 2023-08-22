using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
[UnityEditor.CustomEditor(typeof(Bloom))]
public class BloomEditor : Editor
{
    public SerializedProperty tweakMode;
    public SerializedProperty screenBlendMode;
    public SerializedObject serObj;
    public SerializedProperty hdr;
    public SerializedProperty quality;
    public SerializedProperty sepBlurSpread;
    public SerializedProperty bloomIntensity;
    public SerializedProperty bloomThreshholdColor;
    public SerializedProperty bloomThreshhold;
    public SerializedProperty bloomBlurIterations;
    public SerializedProperty hollywoodFlareBlurIterations;
    public SerializedProperty lensflareMode;
    public SerializedProperty hollyStretchWidth;
    public SerializedProperty lensflareIntensity;
    public SerializedProperty flareRotation;
    public SerializedProperty lensFlareSaturation;
    public SerializedProperty lensflareThreshhold;
    public SerializedProperty flareColorA;
    public SerializedProperty flareColorB;
    public SerializedProperty flareColorC;
    public SerializedProperty flareColorD;
    public SerializedProperty blurWidth;
    public SerializedProperty lensFlareVignetteMask;
    public virtual void OnEnable()
    {
        this.serObj = new SerializedObject(this.target);
        this.screenBlendMode = this.serObj.FindProperty("screenBlendMode");
        this.hdr = this.serObj.FindProperty("hdr");
        this.quality = this.serObj.FindProperty("quality");
        this.sepBlurSpread = this.serObj.FindProperty("sepBlurSpread");
        this.bloomIntensity = this.serObj.FindProperty("bloomIntensity");
        this.bloomThreshhold = this.serObj.FindProperty("bloomThreshhold");
        this.bloomThreshholdColor = this.serObj.FindProperty("bloomThreshholdColor");
        this.bloomBlurIterations = this.serObj.FindProperty("bloomBlurIterations");
        this.lensflareMode = this.serObj.FindProperty("lensflareMode");
        this.hollywoodFlareBlurIterations = this.serObj.FindProperty("hollywoodFlareBlurIterations");
        this.hollyStretchWidth = this.serObj.FindProperty("hollyStretchWidth");
        this.lensflareIntensity = this.serObj.FindProperty("lensflareIntensity");
        this.lensflareThreshhold = this.serObj.FindProperty("lensflareThreshhold");
        this.lensFlareSaturation = this.serObj.FindProperty("lensFlareSaturation");
        this.flareRotation = this.serObj.FindProperty("flareRotation");
        this.flareColorA = this.serObj.FindProperty("flareColorA");
        this.flareColorB = this.serObj.FindProperty("flareColorB");
        this.flareColorC = this.serObj.FindProperty("flareColorC");
        this.flareColorD = this.serObj.FindProperty("flareColorD");
        this.blurWidth = this.serObj.FindProperty("blurWidth");
        this.lensFlareVignetteMask = this.serObj.FindProperty("lensFlareVignetteMask");
        this.tweakMode = this.serObj.FindProperty("tweakMode");
    }

    public override void OnInspectorGUI()
    {
        this.serObj.Update();
        EditorGUILayout.LabelField("Glow and Lens Flares for bright screen pixels", EditorStyles.miniLabel, new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.quality, new GUIContent("Quality", "High quality preserves high frequencies with bigger blurs and uses a better blending and down-/upsampling"), new GUILayoutOption[] {});
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(this.tweakMode, new GUIContent("Mode"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.screenBlendMode, new GUIContent("Blend"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.hdr, new GUIContent("HDR"), new GUILayoutOption[] {});
        EditorGUILayout.Separator();
        // display info text when screen blend mode cannot be used
        Camera cam = (this.target as Bloom).GetComponent<Camera>();
        if (cam != null)
        {
            if ((this.screenBlendMode.enumValueIndex == 0) && ((cam.allowHDR && (this.hdr.enumValueIndex == 0)) || (this.hdr.enumValueIndex == 1)))
            {
                EditorGUILayout.HelpBox("Screen blend is not supported in HDR. Using 'Add' instead.", MessageType.Info);
            }
        }
        EditorGUILayout.PropertyField(this.bloomIntensity, new GUIContent("Intensity"), new GUILayoutOption[] {});
        this.bloomThreshhold.floatValue = EditorGUILayout.Slider("Threshhold", this.bloomThreshhold.floatValue, -0.05f, 4f, new GUILayoutOption[] {});
        if (1 == this.tweakMode.intValue)
        {
            EditorGUILayout.PropertyField(this.bloomThreshholdColor, new GUIContent(" RGB Threshhold"), new GUILayoutOption[] {});
        }
        EditorGUILayout.Separator();
        this.bloomBlurIterations.intValue = EditorGUILayout.IntSlider("Blur Iterations", this.bloomBlurIterations.intValue, 1, 4, new GUILayoutOption[] {});
        this.sepBlurSpread.floatValue = EditorGUILayout.Slider(" Sample Distance", this.sepBlurSpread.floatValue, 0.1f, 10f, new GUILayoutOption[] {});
        EditorGUILayout.Separator();
        if (1 == this.tweakMode.intValue)
        {
            // further lens flare tweakings
            if (0 != this.tweakMode.intValue)
            {
                EditorGUILayout.PropertyField(this.lensflareMode, new GUIContent("Lens Flares"), new GUILayoutOption[] {});
            }
            else
            {
                this.lensflareMode.enumValueIndex = 0;
            }
            EditorGUILayout.PropertyField(this.lensflareIntensity, new GUIContent(" Local Intensity", "0 disables lens flares entirely (optimization)"), new GUILayoutOption[] {});
            this.lensflareThreshhold.floatValue = EditorGUILayout.Slider("  Threshhold", this.lensflareThreshhold.floatValue, 0f, 4f, new GUILayoutOption[] {});
            if (Mathf.Abs(this.lensflareIntensity.floatValue) > Mathf.Epsilon)
            {
                if (this.lensflareMode.intValue == 0)
                {
                    // ghosting	
                    EditorGUILayout.BeginHorizontal(new GUILayoutOption[] {});
                    EditorGUILayout.PropertyField(this.flareColorA, new GUIContent(" 1st Color"), new GUILayoutOption[] {});
                    EditorGUILayout.PropertyField(this.flareColorB, new GUIContent(" 2nd Color"), new GUILayoutOption[] {});
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal(new GUILayoutOption[] {});
                    EditorGUILayout.PropertyField(this.flareColorC, new GUIContent(" 3rd Color"), new GUILayoutOption[] {});
                    EditorGUILayout.PropertyField(this.flareColorD, new GUIContent(" 4th Color"), new GUILayoutOption[] {});
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    if (this.lensflareMode.intValue == 1)
                    {
                        // hollywood
                        EditorGUILayout.PropertyField(this.hollyStretchWidth, new GUIContent(" Stretch width"), new GUILayoutOption[] {});
                        EditorGUILayout.PropertyField(this.flareRotation, new GUIContent(" Rotation"), new GUILayoutOption[] {});
                        this.hollywoodFlareBlurIterations.intValue = EditorGUILayout.IntSlider(" Blur Iterations", this.hollywoodFlareBlurIterations.intValue, 1, 4, new GUILayoutOption[] {});
                        EditorGUILayout.PropertyField(this.lensFlareSaturation, new GUIContent(" Saturation"), new GUILayoutOption[] {});
                        EditorGUILayout.PropertyField(this.flareColorA, new GUIContent(" Tint Color"), new GUILayoutOption[] {});
                    }
                    else
                    {
                        if (this.lensflareMode.intValue == 2)
                        {
                            // both
                            EditorGUILayout.PropertyField(this.hollyStretchWidth, new GUIContent(" Stretch width"), new GUILayoutOption[] {});
                            this.hollywoodFlareBlurIterations.intValue = EditorGUILayout.IntSlider(" Blur Iterations", this.hollywoodFlareBlurIterations.intValue, 1, 4, new GUILayoutOption[] {});
                            EditorGUILayout.PropertyField(this.lensFlareSaturation, new GUIContent(" Saturation"), new GUILayoutOption[] {});
                            EditorGUILayout.BeginHorizontal(new GUILayoutOption[] {});
                            EditorGUILayout.PropertyField(this.flareColorA, new GUIContent(" 1st Color"), new GUILayoutOption[] {});
                            EditorGUILayout.PropertyField(this.flareColorB, new GUIContent(" 2nd Color"), new GUILayoutOption[] {});
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal(new GUILayoutOption[] {});
                            EditorGUILayout.PropertyField(this.flareColorC, new GUIContent(" 3rd Color"), new GUILayoutOption[] {});
                            EditorGUILayout.PropertyField(this.flareColorD, new GUIContent(" 4th Color"), new GUILayoutOption[] {});
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                EditorGUILayout.PropertyField(this.lensFlareVignetteMask, new GUIContent(" Mask", "This mask is needed to prevent lens flare artifacts"), new GUILayoutOption[] {});
            }
        }
        this.serObj.ApplyModifiedProperties();
    }

}