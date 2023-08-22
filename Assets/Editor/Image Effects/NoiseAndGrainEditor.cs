using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
[UnityEditor.CustomEditor(typeof(NoiseAndGrain))]
public class NoiseAndGrainEditor : Editor
{
    public SerializedObject serObj;
    public SerializedProperty intensityMultiplier;
    public SerializedProperty generalIntensity;
    public SerializedProperty blackIntensity;
    public SerializedProperty whiteIntensity;
    public SerializedProperty midGrey;
    public SerializedProperty dx11Grain;
    public SerializedProperty softness;
    public SerializedProperty monochrome;
    public SerializedProperty intensities;
    public SerializedProperty tiling;
    public SerializedProperty monochromeTiling;
    public SerializedProperty noiseTexture;
    public SerializedProperty filterMode;
    public virtual void OnEnable()
    {
        this.serObj = new SerializedObject(this.target);
        this.intensityMultiplier = this.serObj.FindProperty("intensityMultiplier");
        this.generalIntensity = this.serObj.FindProperty("generalIntensity");
        this.blackIntensity = this.serObj.FindProperty("blackIntensity");
        this.whiteIntensity = this.serObj.FindProperty("whiteIntensity");
        this.midGrey = this.serObj.FindProperty("midGrey");
        this.dx11Grain = this.serObj.FindProperty("dx11Grain");
        this.softness = this.serObj.FindProperty("softness");
        this.monochrome = this.serObj.FindProperty("monochrome");
        this.intensities = this.serObj.FindProperty("intensities");
        this.tiling = this.serObj.FindProperty("tiling");
        this.monochromeTiling = this.serObj.FindProperty("monochromeTiling");
        this.noiseTexture = this.serObj.FindProperty("noiseTexture");
        this.filterMode = this.serObj.FindProperty("filterMode");
    }

    public override void OnInspectorGUI()
    {
        this.serObj.Update();
        EditorGUILayout.LabelField("Overlays animated noise patterns", EditorStyles.miniLabel, new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.dx11Grain, new GUIContent("DirectX 11 Grain"), new GUILayoutOption[] {});
        if (this.dx11Grain.boolValue && !(this.target as NoiseAndGrain).Dx11Support())
        {
            EditorGUILayout.HelpBox("DX11 mode not supported (need shader model 5)", MessageType.Info);
        }
        EditorGUILayout.PropertyField(this.monochrome, new GUIContent("Monochrome"), new GUILayoutOption[] {});
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(this.intensityMultiplier, new GUIContent("Intensity Multiplier"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.generalIntensity, new GUIContent(" General"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.blackIntensity, new GUIContent(" Black Boost"), new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.whiteIntensity, new GUIContent(" White Boost"), new GUILayoutOption[] {});
        this.midGrey.floatValue = EditorGUILayout.Slider(new GUIContent(" Mid Grey (for Boost)"), this.midGrey.floatValue, 0f, 1f, new GUILayoutOption[] {});
        if (this.monochrome.boolValue == false)
        {
            Color c = new Color(this.intensities.vector3Value.x, this.intensities.vector3Value.y, this.intensities.vector3Value.z, 1f);
            c = EditorGUILayout.ColorField(new GUIContent(" Color Weights"), c, new GUILayoutOption[] {});

            {
                float _1 = c.r;
                Vector3 _2 = this.intensities.vector3Value;
                _2.x = _1;
                this.intensities.vector3Value = _2;
            }

            {
                float _3 = c.g;
                Vector3 _4 = this.intensities.vector3Value;
                _4.y = _3;
                this.intensities.vector3Value = _4;
            }

            {
                float _5 = c.b;
                Vector3 _6 = this.intensities.vector3Value;
                _6.z = _5;
                this.intensities.vector3Value = _6;
            }
        }
        if (!this.dx11Grain.boolValue)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Noise Shape", new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.noiseTexture, new GUIContent(" Texture"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.filterMode, new GUIContent(" Filter"), new GUILayoutOption[] {});
        }
        else
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Noise Shape", new GUILayoutOption[] {});
        }
        this.softness.floatValue = EditorGUILayout.Slider(new GUIContent(" Softness"), this.softness.floatValue, 0f, 0.99f, new GUILayoutOption[] {});
        if (!this.dx11Grain.boolValue)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Advanced", new GUILayoutOption[] {});
            if (this.monochrome.boolValue == false)
            {

                {
                    float _7 = EditorGUILayout.FloatField(new GUIContent(" Tiling (Red)"), this.tiling.vector3Value.x, new GUILayoutOption[] {});
                    Vector3 _8 = this.tiling.vector3Value;
                    _8.x = _7;
                    this.tiling.vector3Value = _8;
                }

                {
                    float _9 = EditorGUILayout.FloatField(new GUIContent(" Tiling (Green)"), this.tiling.vector3Value.y, new GUILayoutOption[] {});
                    Vector3 _10 = this.tiling.vector3Value;
                    _10.y = _9;
                    this.tiling.vector3Value = _10;
                }

                {
                    float _11 = EditorGUILayout.FloatField(new GUIContent(" Tiling (Blue)"), this.tiling.vector3Value.z, new GUILayoutOption[] {});
                    Vector3 _12 = this.tiling.vector3Value;
                    _12.z = _11;
                    this.tiling.vector3Value = _12;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(this.monochromeTiling, new GUIContent(" Tiling"), new GUILayoutOption[] {});
            }
        }
        this.serObj.ApplyModifiedProperties();
    }

}