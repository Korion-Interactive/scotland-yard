using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
[UnityEditor.CustomEditor(typeof(BloomAndLensFlares))]
public class BloomAndLensFlaresEditor : Editor
{
    public SerializedProperty tweakMode;
    public SerializedProperty screenBlendMode;
    public SerializedObject serObj;
    public SerializedProperty hdr;
    public SerializedProperty sepBlurSpread;
    public SerializedProperty useSrcAlphaAsMask;
    public SerializedProperty bloomIntensity;
    public SerializedProperty bloomThreshhold;
    public SerializedProperty bloomBlurIterations;
    public SerializedProperty lensflares;
    public SerializedProperty hollywoodFlareBlurIterations;
    public SerializedProperty lensflareMode;
    public SerializedProperty hollyStretchWidth;
    public SerializedProperty lensflareIntensity;
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
        this.sepBlurSpread = this.serObj.FindProperty("sepBlurSpread");
        this.useSrcAlphaAsMask = this.serObj.FindProperty("useSrcAlphaAsMask");
        this.bloomIntensity = this.serObj.FindProperty("bloomIntensity");
        this.bloomThreshhold = this.serObj.FindProperty("bloomThreshhold");
        this.bloomBlurIterations = this.serObj.FindProperty("bloomBlurIterations");
        this.lensflares = this.serObj.FindProperty("lensflares");
        this.lensflareMode = this.serObj.FindProperty("lensflareMode");
        this.hollywoodFlareBlurIterations = this.serObj.FindProperty("hollywoodFlareBlurIterations");
        this.hollyStretchWidth = this.serObj.FindProperty("hollyStretchWidth");
        this.lensflareIntensity = this.serObj.FindProperty("lensflareIntensity");
        this.lensflareThreshhold = this.serObj.FindProperty("lensflareThreshhold");
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
        GUILayout.Label(("HDR " + (this.hdr.enumValueIndex == 0 ? "auto detected, " : (this.hdr.enumValueIndex == 1 ? "forced on, " : "disabled, "))) + (this.useSrcAlphaAsMask.floatValue < 0.1f ? " ignoring alpha channel glow information" : " using alpha channel glow information"), EditorStyles.miniBoldLabel, new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.tweakMode, new GUIContent("Tweak mode"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.screenBlendMode, new GUIContent("Blend mode"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.hdr, new GUIContent("HDR"), new GUILayoutOption[] {});
        // display info text when screen blend mode cannot be used
        Camera cam = (this.target as BloomAndLensFlares).GetComponent<Camera>();
        if (cam != null)
        {
            if ((this.screenBlendMode.enumValueIndex == 0) && ((cam.allowHDR && (this.hdr.enumValueIndex == 0)) || (this.hdr.enumValueIndex == 1)))
            {
                EditorGUILayout.HelpBox("Screen blend is not supported in HDR. Using 'Add' instead.", MessageType.Info);
            }
        }
        if (1 == this.tweakMode.intValue)
        {
            EditorGUILayout.PropertyField(this.lensflares, new GUIContent("Cast lens flares"), new GUILayoutOption[] {});
        }
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(this.bloomIntensity, new GUIContent("Intensity"), new GUILayoutOption[] {});
        this.bloomThreshhold.floatValue = EditorGUILayout.Slider("Threshhold", this.bloomThreshhold.floatValue, -0.05f, 4f, new GUILayoutOption[] {});
        this.bloomBlurIterations.intValue = EditorGUILayout.IntSlider("Blur iterations", this.bloomBlurIterations.intValue, 1, 4, new GUILayoutOption[] {});
        this.sepBlurSpread.floatValue = EditorGUILayout.Slider("Blur spread", this.sepBlurSpread.floatValue, 0.1f, 10f, new GUILayoutOption[] {});
        if (1 == this.tweakMode.intValue)
        {
            this.useSrcAlphaAsMask.floatValue = EditorGUILayout.Slider(new GUIContent("Use alpha mask", "Make alpha channel define glowiness"), this.useSrcAlphaAsMask.floatValue, 0f, 1f, new GUILayoutOption[] {});
        }
        else
        {
            this.useSrcAlphaAsMask.floatValue = 0f;
        }
        if (1 == this.tweakMode.intValue)
        {
            EditorGUILayout.Separator();
            if (this.lensflares.boolValue)
            {
                // further lens flare tweakings
                if (0 != this.tweakMode.intValue)
                {
                    EditorGUILayout.PropertyField(this.lensflareMode, new GUIContent("Lens flare mode"), new GUILayoutOption[] {});
                }
                else
                {
                    this.lensflareMode.enumValueIndex = 0;
                }
                EditorGUILayout.PropertyField(this.lensFlareVignetteMask, new GUIContent("Lens flare mask", "This mask is needed to prevent lens flare artifacts"), new GUILayoutOption[] {});
                EditorGUILayout.PropertyField(this.lensflareIntensity, new GUIContent("Local intensity"), new GUILayoutOption[] {});
                this.lensflareThreshhold.floatValue = EditorGUILayout.Slider("Local threshhold", this.lensflareThreshhold.floatValue, 0f, 1f, new GUILayoutOption[] {});
                if (this.lensflareMode.intValue == 0)
                {
                    // ghosting	
                    EditorGUILayout.BeginHorizontal(new GUILayoutOption[] {});
                    EditorGUILayout.PropertyField(this.flareColorA, new GUIContent("1st Color"), new GUILayoutOption[] {});
                    EditorGUILayout.PropertyField(this.flareColorB, new GUIContent("2nd Color"), new GUILayoutOption[] {});
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal(new GUILayoutOption[] {});
                    EditorGUILayout.PropertyField(this.flareColorC, new GUIContent("3rd Color"), new GUILayoutOption[] {});
                    EditorGUILayout.PropertyField(this.flareColorD, new GUIContent("4th Color"), new GUILayoutOption[] {});
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    if (this.lensflareMode.intValue == 1)
                    {
                        // hollywood
                        EditorGUILayout.PropertyField(this.hollyStretchWidth, new GUIContent("Stretch width"), new GUILayoutOption[] {});
                        this.hollywoodFlareBlurIterations.intValue = EditorGUILayout.IntSlider("Blur iterations", this.hollywoodFlareBlurIterations.intValue, 1, 4, new GUILayoutOption[] {});
                        EditorGUILayout.PropertyField(this.flareColorA, new GUIContent("Tint Color"), new GUILayoutOption[] {});
                    }
                    else
                    {
                        if (this.lensflareMode.intValue == 2)
                        {
                            // both
                            EditorGUILayout.PropertyField(this.hollyStretchWidth, new GUIContent("Stretch width"), new GUILayoutOption[] {});
                            this.hollywoodFlareBlurIterations.intValue = EditorGUILayout.IntSlider("Blur iterations", this.hollywoodFlareBlurIterations.intValue, 1, 4, new GUILayoutOption[] {});
                            EditorGUILayout.BeginHorizontal(new GUILayoutOption[] {});
                            EditorGUILayout.PropertyField(this.flareColorA, new GUIContent("1st Color"), new GUILayoutOption[] {});
                            EditorGUILayout.PropertyField(this.flareColorB, new GUIContent("2nd Color"), new GUILayoutOption[] {});
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal(new GUILayoutOption[] {});
                            EditorGUILayout.PropertyField(this.flareColorC, new GUIContent("3rd Color"), new GUILayoutOption[] {});
                            EditorGUILayout.PropertyField(this.flareColorD, new GUIContent("4th Color"), new GUILayoutOption[] {});
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }
        else
        {
            this.lensflares.boolValue = false; // disable lens flares in simple tweak mode
        }
        this.serObj.ApplyModifiedProperties();
    }

}