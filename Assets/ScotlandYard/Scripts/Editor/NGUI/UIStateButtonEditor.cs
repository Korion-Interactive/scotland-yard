using BitBarons.Editor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIStateButton), true)]
public class UIStateButtonEditor : UIWidgetContainerEditor
{
    private ReorderableList list;

    void OnEnable()
    {
        list = new ReorderableList(serializedObject,
                serializedObject.FindProperty("StateSwitchOrder"),
                true, true, true, true);

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), "element " + index.ToString());
                EditorGUI.PropertyField(new Rect(rect.x + 100, rect.y, 60, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            };

        list.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "State Switch Order");
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UIStateButton obj = target as UIStateButton;
       
        obj.Button = EditorGUILayout.ObjectField("Button Object", obj.Button, typeof(UIButton), true) as UIButton;
        obj.ColorButton = EditorGUILayout.ObjectField("Color Button Object", obj.ColorButton, typeof(UIButtonColor), true) as UIButtonColor;
        if (obj.ColorButton != null)
        {
            obj.ColorButton2 = EditorGUILayout.ObjectField("Color Button Object 2", obj.ColorButton2, typeof(UIButtonColor), true) as UIButtonColor;      
        }
        obj.Label = EditorGUILayout.ObjectField("Label", obj.Label, typeof(LabelTranslator), true) as LabelTranslator;
        
        NGUIEditorTools.DrawEvents("On State Changed", obj, obj.stateChangedListeners);

        //obj.StateSwitchOrder = EditorGUILayout.("State Switch Order", obj.StateSwitchOrder);

        using (EditorGuiZone.Vertical("Box"))
        {
            EditorGUILayout.LabelField("STATES");

            int cnt = EditorGUILayout.IntField("State Count", obj.States.Count);

            while(cnt < obj.States.Count)
                obj.States.RemoveAt(obj.States.Count - 1);

            while(cnt > obj.States.Count)
                obj.States.Add(new UIStateButton.ButtonState());

            for (int i = 0; i < obj.States.Count; i++)
            {
                var item = obj.States[i];

                if (BitBarons.Editor.Bindings.SharedEditorMethods.GetFoldOut("State " + i, "States"))
                {
                    using (EditorGuiZone.Vertical("Box"))
                    {
                        
                        if (obj.Button != null)
                        {
                            DrawSprites(item);
                        }

                        if (obj.ColorButton != null)
                        {
                            DrawColors(item);
                        }

                        if (obj.ColorButton2 != null)
                        {
                            DrawColors2(item);
                        }

                        if(obj.Label != null)
                        {
                            item.LabelTextId = EditorGUILayout.TextField("Label Text ID", item.LabelTextId);
                            item.LabelColor = EditorGUILayout.ColorField("Text Color", item.LabelColor);
                        }
                    }
                }
                EditorGUILayout.Space();
            }
        }

        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawColors(UIStateButton.ButtonState state)
    {
        if (NGUIEditorTools.DrawHeader("Colors"))
        {
            NGUIEditorTools.BeginContents();

            state.ColorNormal = EditorGUILayout.ColorField("Normal", state.ColorNormal);
            state.ColorHover = EditorGUILayout.ColorField("Hover", state.ColorHover);
            state.ColorPressed = EditorGUILayout.ColorField("Pressed", state.ColorPressed);
            state.ColorDisabled = EditorGUILayout.ColorField("Disabled", state.ColorDisabled);

            NGUIEditorTools.EndContents();
        }
    }

    private void DrawColors2(UIStateButton.ButtonState state)
    {
        if (NGUIEditorTools.DrawHeader("Colors 2"))
        {
            NGUIEditorTools.BeginContents();

            state.ColorNormal2 = EditorGUILayout.ColorField("Normal", state.ColorNormal2);
            state.ColorHover2 = EditorGUILayout.ColorField("Hover", state.ColorHover2);
            state.ColorPressed2 = EditorGUILayout.ColorField("Pressed", state.ColorPressed2);
            state.ColorDisabled2 = EditorGUILayout.ColorField("Disabled", state.ColorDisabled2);

            NGUIEditorTools.EndContents();
        }
    }

    private void DrawSprites(UIStateButton.ButtonState state)
    {
        if (NGUIEditorTools.DrawHeader("Sprites"))
        {
            NGUIEditorTools.BeginContents();

            state.SpriteNameNormal = EditorGUILayout.TextField("Sprite Normal", state.SpriteNameNormal);
            state.SpriteNameHover = EditorGUILayout.TextField("Sprite Hover", state.SpriteNameHover);
            state.SpriteNamePressed = EditorGUILayout.TextField("Sprite Pressed", state.SpriteNamePressed);
            state.SpriteNameDisabled = EditorGUILayout.TextField("Sprite Disable", state.SpriteNameDisabled);

            //NGUIEditorTools.DrawSpriteField("Hover", serializedObject, obj.Button., serializedObject.FindProperty("hoverSprite"), true);

            NGUIEditorTools.EndContents();
        }
    }
}