using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
[UnityEditor.CustomEditor(typeof(EdgeDetectEffectNormals))]
public class EdgeDetectEffectNormalsEditor : Editor
{
    public SerializedObject serObj;
    public SerializedProperty mode;
    public SerializedProperty sensitivityDepth;
    public SerializedProperty sensitivityNormals;
    public SerializedProperty lumThreshhold;
    public SerializedProperty edgesOnly;
    public SerializedProperty edgesOnlyBgColor;
    public SerializedProperty edgeExp;
    public SerializedProperty sampleDist;
    public virtual void OnEnable()
    {
        this.serObj = new SerializedObject(this.target);
        this.mode = this.serObj.FindProperty("mode");
        this.sensitivityDepth = this.serObj.FindProperty("sensitivityDepth");
        this.sensitivityNormals = this.serObj.FindProperty("sensitivityNormals");
        this.lumThreshhold = this.serObj.FindProperty("lumThreshhold");
        this.edgesOnly = this.serObj.FindProperty("edgesOnly");
        this.edgesOnlyBgColor = this.serObj.FindProperty("edgesOnlyBgColor");
        this.edgeExp = this.serObj.FindProperty("edgeExp");
        this.sampleDist = this.serObj.FindProperty("sampleDist");
    }

    public override void OnInspectorGUI()
    {
        this.serObj.Update();
        GUILayout.Label("Detects spatial differences and converts into black outlines", EditorStyles.miniBoldLabel, new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.mode, new GUIContent("Mode"), new GUILayoutOption[] {});
        if (this.mode.intValue < 2)
        {
            EditorGUILayout.PropertyField(this.sensitivityDepth, new GUIContent(" Depth Sensitivity"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.sensitivityNormals, new GUIContent(" Normals Sensitivity"), new GUILayoutOption[] {});
        }
        else
        {
            if (this.mode.intValue < 4)
            {
                EditorGUILayout.PropertyField(this.edgeExp, new GUIContent(" Edge Exponent"), new GUILayoutOption[] {});
            }
            else
            {
                // lum based mode
                EditorGUILayout.PropertyField(this.lumThreshhold, new GUIContent(" Luminance Threshold"), new GUILayoutOption[] {});
            }
        }
        EditorGUILayout.PropertyField(this.sampleDist, new GUIContent(" Sample Distance"), new GUILayoutOption[] {});
        EditorGUILayout.Separator();
        GUILayout.Label("Background Options", new GUILayoutOption[] {});
        this.edgesOnly.floatValue = EditorGUILayout.Slider(" Edges only", this.edgesOnly.floatValue, 0f, 1f, new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.edgesOnlyBgColor, new GUIContent(" Color"), new GUILayoutOption[] {});
        this.serObj.ApplyModifiedProperties();
    }

}