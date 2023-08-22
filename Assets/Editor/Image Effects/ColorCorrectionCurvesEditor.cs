using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
[UnityEditor.CustomEditor(typeof(ColorCorrectionCurves))]
public class ColorCorrectionCurvesEditor : Editor
{
    public SerializedObject serObj;
    public SerializedProperty mode;
    public SerializedProperty redChannel;
    public SerializedProperty greenChannel;
    public SerializedProperty blueChannel;
    public SerializedProperty useDepthCorrection;
    public SerializedProperty depthRedChannel;
    public SerializedProperty depthGreenChannel;
    public SerializedProperty depthBlueChannel;
    public SerializedProperty zCurveChannel;
    public SerializedProperty saturation;
    public SerializedProperty selectiveCc;
    public SerializedProperty selectiveFromColor;
    public SerializedProperty selectiveToColor;
    private bool applyCurveChanges;
    public virtual void OnEnable()
    {
        this.serObj = new SerializedObject(this.target);
        this.mode = this.serObj.FindProperty("mode");
        this.saturation = this.serObj.FindProperty("saturation");
        this.redChannel = this.serObj.FindProperty("redChannel");
        this.greenChannel = this.serObj.FindProperty("greenChannel");
        this.blueChannel = this.serObj.FindProperty("blueChannel");
        this.useDepthCorrection = this.serObj.FindProperty("useDepthCorrection");
        this.zCurveChannel = this.serObj.FindProperty("zCurve");
        this.depthRedChannel = this.serObj.FindProperty("depthRedChannel");
        this.depthGreenChannel = this.serObj.FindProperty("depthGreenChannel");
        this.depthBlueChannel = this.serObj.FindProperty("depthBlueChannel");
        if (this.redChannel.animationCurveValue.length == 0)
        {
            this.redChannel.animationCurveValue = new AnimationCurve(new Keyframe[] {new Keyframe(0, 0f, 1f, 1f), new Keyframe(1, 1f, 1f, 1f)});
        }
        if (this.greenChannel.animationCurveValue.length == 0)
        {
            this.greenChannel.animationCurveValue = new AnimationCurve(new Keyframe[] {new Keyframe(0, 0f, 1f, 1f), new Keyframe(1, 1f, 1f, 1f)});
        }
        if (this.blueChannel.animationCurveValue.length == 0)
        {
            this.blueChannel.animationCurveValue = new AnimationCurve(new Keyframe[] {new Keyframe(0, 0f, 1f, 1f), new Keyframe(1, 1f, 1f, 1f)});
        }
        if (this.depthRedChannel.animationCurveValue.length == 0)
        {
            this.depthRedChannel.animationCurveValue = new AnimationCurve(new Keyframe[] {new Keyframe(0, 0f, 1f, 1f), new Keyframe(1, 1f, 1f, 1f)});
        }
        if (this.depthGreenChannel.animationCurveValue.length == 0)
        {
            this.depthGreenChannel.animationCurveValue = new AnimationCurve(new Keyframe[] {new Keyframe(0, 0f, 1f, 1f), new Keyframe(1, 1f, 1f, 1f)});
        }
        if (this.depthBlueChannel.animationCurveValue.length == 0)
        {
            this.depthBlueChannel.animationCurveValue = new AnimationCurve(new Keyframe[] {new Keyframe(0, 0f, 1f, 1f), new Keyframe(1, 1f, 1f, 1f)});
        }
        if (this.zCurveChannel.animationCurveValue.length == 0)
        {
            this.zCurveChannel.animationCurveValue = new AnimationCurve(new Keyframe[] {new Keyframe(0, 0f, 1f, 1f), new Keyframe(1, 1f, 1f, 1f)});
        }
        this.serObj.ApplyModifiedProperties();
        this.selectiveCc = this.serObj.FindProperty("selectiveCc");
        this.selectiveFromColor = this.serObj.FindProperty("selectiveFromColor");
        this.selectiveToColor = this.serObj.FindProperty("selectiveToColor");
    }

    public virtual void CurveGui(string name, SerializedProperty animationCurve, Color color)
    {
        // @NOTE: EditorGUILayout.CurveField is buggy and flickers, using PropertyField for now
        //animationCurve.animationCurveValue = EditorGUILayout.CurveField (GUIContent (name), animationCurve.animationCurveValue, color, Rect (0.0,0.0,1.0,1.0));
        EditorGUILayout.PropertyField(animationCurve, new GUIContent(name), new GUILayoutOption[] {});
        if (GUI.changed)
        {
            this.applyCurveChanges = true;
        }
    }

    public virtual void BeginCurves()
    {
        this.applyCurveChanges = false;
    }

    public virtual void ApplyCurves()
    {
        if (this.applyCurveChanges)
        {
            this.serObj.ApplyModifiedProperties();
            (this.serObj.targetObject as ColorCorrectionCurves).gameObject.SendMessage("UpdateTextures");
        }
    }

    public override void OnInspectorGUI()
    {
        this.serObj.Update();
        GUILayout.Label("Use curves to tweak RGB channel colors", EditorStyles.miniBoldLabel, new GUILayoutOption[] {});
        this.saturation.floatValue = EditorGUILayout.Slider("Saturation", this.saturation.floatValue, 0f, 5f, new GUILayoutOption[] {});
        EditorGUILayout.PropertyField(this.mode, new GUIContent("Mode"), new GUILayoutOption[] {});
        EditorGUILayout.Separator();
        this.BeginCurves();
        this.CurveGui(" Red", this.redChannel, Color.red);
        this.CurveGui(" Green", this.greenChannel, Color.green);
        this.CurveGui(" Blue", this.blueChannel, Color.blue);
        EditorGUILayout.Separator();
        if (this.mode.intValue > 0)
        {
            this.useDepthCorrection.boolValue = true;
        }
        else
        {
            this.useDepthCorrection.boolValue = false;
        }
        if (this.useDepthCorrection.boolValue)
        {
            this.CurveGui(" Red (depth)", this.depthRedChannel, Color.red);
            this.CurveGui(" Green (depth)", this.depthGreenChannel, Color.green);
            this.CurveGui(" Blue (depth)", this.depthBlueChannel, Color.blue);
            EditorGUILayout.Separator();
            this.CurveGui(" Blend Curve", this.zCurveChannel, Color.grey);
        }
        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(this.selectiveCc, new GUIContent("Selective"), new GUILayoutOption[] {});
        if (this.selectiveCc.boolValue)
        {
            EditorGUILayout.PropertyField(this.selectiveFromColor, new GUIContent(" Key"), new GUILayoutOption[] {});
            EditorGUILayout.PropertyField(this.selectiveToColor, new GUIContent(" Target"), new GUILayoutOption[] {});
        }
        this.ApplyCurves();
        if (!this.applyCurveChanges)
        {
            this.serObj.ApplyModifiedProperties();
        }
    }

}