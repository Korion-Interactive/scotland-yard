using FullInspector.Internal;
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FullInspector {
    [CustomPropertyDrawer(typeof(fiValueProxyEditor), /*useForChildren:*/ true)]
    public class fiValuePropertyDrawer : PropertyDrawer {
        #region Reflection
        private static Type GetPropertyType(SerializedProperty property) {
            Type holderType = FullInspector.Internal.TypeCache.FindType(property.type);

            while (holderType != null &&
                (holderType.IsGenericType == false || holderType.GetGenericTypeDefinition() != typeof(fiValue<>))) {
                holderType = holderType.BaseType;
            }

            if (holderType == null) return null;
            return holderType.GetGenericArguments()[0];
        }

        private static object ReadFieldOrProperty(object obj, string name) {
            // We cannot use BindingFlags.FlattenHierarchy because that does *not* include
            // private members in the parent type. Instead, we scan fields/properties for each
            // inheritance level which *will* include private members on parent types.

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var type = obj.GetType();

            while (type != null) {
                var field = type.GetField(name, flags);
                if (field != null) {
                    return field.GetValue(obj);
                }

                var prop = type.GetProperty(name, flags);
                if (prop != null) {
                    return prop.GetValue(obj, null);
                }

                type = type.BaseType;
            }

            return null;
        }
        private static object ReadArrayIndex(object obj, int index) {
            var list = (IList)obj;
            if (index >= list.Count) return null;
            return list[index];
        }

        public static fiIValueProxyAPI GetTarget(SerializedProperty property) {
            object result = property.serializedObject.targetObject;

            string[] names = property.propertyPath.Replace("Array.data", "").Split('.');
            for (int i = 0; i < names.Length; ++i) {
                string name = names[i];

                // array
                if (name[0] == '[') {
                    name = name.Substring(1);
                    name = name.Remove(name.Length - 1);
                    int index = int.Parse(name);
                    result = ReadArrayIndex(result, index);
                }

                // member
                else {
                    result = ReadFieldOrProperty(result, name);
                }

                // reading the property from reflection failed for some reason -- we have
                // to return null
                if (result == null) return null;
            }

            return (fiIValueProxyAPI)result;
        }


        private static fiGraphMetadataChild GetMetadata(SerializedProperty property) {
            return fiGraphMetadata.GetGlobal(property.serializedObject.targetObject).Enter(property.propertyPath);
        }
        #endregion

        #region GUI
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var propertyType = GetPropertyType(property);
            if (propertyType == null) return;

            var target = GetTarget(property);
            var metadata = GetMetadata(property);
            var editor = PropertyEditor.Get(propertyType, fieldInfo).FirstEditor;

            // We have to disable animation because GUI.BeginGroup is broken in PropertyDrawers;
            // it does not properly reset the coordinate system.
            bool savedAnimationState = fiSettings.EnableAnimation;
            fiSettings.EnableAnimation = false;
            fiEditorUtility.Repaint = false;

            if (property.prefabOverride) UnityInternalReflection.SetBoldDefaultFont(true);

            EditorGUI.BeginChangeCheck();
            target.Value = editor.Edit(position, label, target.Value, metadata);
            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            if (fiEditorUtility.Repaint) {
                EditorUtility.SetDirty(property.serializedObject.targetObject); // Repaint
                fiEditorUtility.Repaint = false;
            }

            fiSettings.EnableAnimation = savedAnimationState;

            if (property.prefabOverride) UnityInternalReflection.SetBoldDefaultFont(false);

            RevertPrefabContextMenu(position, property);
        }

        private static void RevertPrefabContextMenu(Rect region, SerializedProperty property) {
            if (Event.current.type == EventType.ContextClick &&
                region.Contains(Event.current.mousePosition) &&
                property.prefabOverride) {

                Event.current.Use();

                var content = new GUIContent("Revert Value to Prefab");

                GenericMenu menu = new GenericMenu();
                menu.AddItem(content, /*on:*/false, () => {
                    PropertyModification[] fixedMods = PrefabUtility.GetPropertyModifications(property.serializedObject.targetObject);
                    for (int i = 0; i < fixedMods.Length; ++i) {
                        if (fixedMods[i].propertyPath.StartsWith(property.propertyPath)) {
                            ArrayUtility.RemoveAt(ref fixedMods, i);
                        }
                    }

                    PrefabUtility.SetPropertyModifications(property.serializedObject.targetObject, fixedMods);
                });
                menu.ShowAsContext();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var propertyType = GetPropertyType(property);

            if (propertyType == null) return 0;

            var target = GetTarget(property);
            var metadata = GetMetadata(property);
            var editor = PropertyEditor.Get(propertyType, fieldInfo).FirstEditor;

            // We have to disable animation because GUI.BeginGroup is broken in PropertyDrawers;
            // it does not properly reset the coordinate system.
            bool savedAnimationState = fiSettings.EnableAnimation;
            fiSettings.EnableAnimation = false;
            fiEditorUtility.Repaint = false;
            float height = editor.GetElementHeight(label, target.Value, metadata);
            if (fiEditorUtility.Repaint) {
                EditorUtility.SetDirty(property.serializedObject.targetObject); // Repaint
                fiEditorUtility.Repaint = false;
            }
            fiSettings.EnableAnimation = savedAnimationState;

            return height;
        }
        #endregion
    }
}