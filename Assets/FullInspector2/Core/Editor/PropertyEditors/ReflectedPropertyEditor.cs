﻿using System;
using System.Collections.Generic;
using System.Reflection;
using FullSerializer.Internal;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Internal {
    /// <summary>
    /// The general property editor that takes over when there is no specific override. This uses
    /// reflection to discover what values to edit.
    /// </summary>
    internal class ReflectedPropertyEditor : IPropertyEditor, IPropertyEditorEditAPI {
        public bool CanIndentLabelForDropdown {
            get { return true; }
        }

        /// <summary>
        /// The maximum depth that the reflected editor will go to for automatic object reference
        /// instantiation. Beyond this depth, the user will have the manually instantiate
        /// references. We have a depth limit so that we don't end up in an infinite object
        /// construction cycle.
        /// </summary>
        private const int MaximumNestingDepth = 5;
        private static fiCycleDetector _cycleEdit;
        private static fiCycleDetector _cycleHeight;
        private static fiCycleDetector _cycleScene;

        /// <summary>
        /// This returns true if automatic instantiation should be enabled. Automatic instantiation
        /// gets disabled after the reflected editor has gone a x calls deep into itself in an
        /// attempt to prevent infinite recursion.
        /// </summary>
        private bool ShouldAutoInstantiate() {
            if (_attributes != null) {
                if (_attributes.GetCustomAttributes(typeof(InspectorNotDefaultConstructedAttribute), /*inherit:*/ true).Length > 0) {
                    return false;
                }
            }

            if (_cycleEdit != null && _cycleEdit.Depth >= MaximumNestingDepth) return false;
            if (_cycleHeight != null && _cycleHeight.Depth >= MaximumNestingDepth) return false;
            if (_cycleScene != null && _cycleScene.Depth >= MaximumNestingDepth) return false;
            return true;
        }

        private InspectedType _metadata;
        private ICustomAttributeProvider _attributes;

        public PropertyEditorChain EditorChain {
            get;
            set;
        }

        public ReflectedPropertyEditor(InspectedType metadata, ICustomAttributeProvider attributes) {
            _metadata = metadata;
            _attributes = attributes;
        }

        /// <summary>
        /// How tall buttons should be.
        /// </summary>
        private static float ButtonHeight = 18;

        /// <summary>
        /// How tall the label element should be.
        /// </summary>
        private const float TitleHeight = 17;

        /// <summary>
        /// How much space is between each element.
        /// </summary>
        private const float DividerHeight = 2f;

        /// <summary>
        /// Returns true if the given GUIContent element contains any content.
        /// </summary>
        private static bool HasLabel(GUIContent label) {
            return label.text != GUIContent.none.text ||
                label.image != GUIContent.none.image ||
                label.tooltip != GUIContent.none.tooltip;
        }

        /// <summary>
        /// Draws a label at the given region. Returns an indented rectangle that can be used for
        /// drawing properties directly under the label.
        /// </summary>
        private static Rect DrawLabel(Rect region, GUIContent label) {
            Rect titleRect = new Rect(region);
            titleRect.height = TitleHeight;
            region.y += TitleHeight;
            region.height -= TitleHeight;

            EditorGUI.LabelField(titleRect, label);
            return RectTools.IndentedRect(region);
        }

        public object OnSceneGUI(object element) {
            try {
                if (_cycleScene == null) {
                    _cycleScene = new fiCycleDetector(_cycleEdit, _cycleHeight);
                }
                _cycleScene.Enter();

                // cycle; don't do anything
                if (_cycleScene.TryMark(element) == false) {
                    return element;
                }

                // Not showing a scene GUI for the object for this frame should be fine
                if (element == null) {
                    return element;
                }

                var inspectedProperties = _metadata.GetProperties(InspectedMemberFilters.InspectableMembers);
                for (int i = 0; i < inspectedProperties.Count; ++i) {
                    var property = inspectedProperties[i];

                    var editorChain = PropertyEditor.Get(property.StorageType, property.MemberInfo);
                    IPropertyEditor editor = editorChain.FirstEditor;

                    object currentValue = property.Read(element);
                    object updatedValue = editor.OnSceneGUI(currentValue);

                    // We use EqualityComparer instead of == because EqualityComparer will properly unbox structs
                    if (EqualityComparer<object>.Default.Equals(currentValue, updatedValue) == false) {
                        property.Write(element, updatedValue);
                    }
                }

                return element;
            }
            finally {
                _cycleScene.Exit();
                if (_cycleScene.Depth == 0) {
                    _cycleScene = null;
                }
            }
        }

        public bool CanEdit(Type dataType) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// A helper method that draws the inspector for a field/property at the given location.
        /// </summary>
        private void EditProperty(ref Rect region, object element, InspectedProperty property, fiGraphMetadata metadata) {
            bool hasPrefabDiff = fiPrefabTools.HasPrefabDiff(element, property);
            if (hasPrefabDiff) UnityInternalReflection.SetBoldDefaultFont(true);

            // edit the property
            {
                Rect propertyRect = region;
                float propertyHeight = fiEditorGUI.EditPropertyHeight(element, property, metadata.Enter(property.Name));
                propertyRect.height = propertyHeight;

                fiEditorGUI.EditProperty(propertyRect, element, property, metadata.Enter(property.Name));

                region.y += propertyHeight;
            }

            if (hasPrefabDiff) UnityInternalReflection.SetBoldDefaultFont(false);
        }

        /// <summary>
        /// A helper method that draws a button at the given region.
        /// </summary>
        private void EditButton(ref Rect region, object element, InspectedMethod method) {
            Rect buttonRect = region;
            buttonRect.height = ButtonHeight;

            string buttonName = method.DisplayName;

            // Disable the button if invoking it will cause an exception
            if (method.HasArguments) buttonName += " (Remove method parameters to enable this button)";

            EditorGUI.BeginDisabledGroup(method.HasArguments);
            if (GUI.Button(buttonRect, buttonName)) {
                method.Invoke(element);
            }
            EditorGUI.EndDisabledGroup();

            region.y += ButtonHeight;
        }

        private static bool GetBooleanReflectedMember(object element, string memberName, bool defaultValue) {
            MemberInfo[] members = element.GetType().GetFlattenedMember(memberName);
            if (members == null || members.Length == 0) {
                Debug.LogError("Could not find a member with name " + memberName + " on object type " + element.GetType());
                return true;
            }

            MemberInfo member = members[0];

            object result = defaultValue;

            try {
                if (member is FieldInfo) {
                    result = ((FieldInfo)member).GetValue(element);
                }
                else if (member is PropertyInfo) {
                    result = ((PropertyInfo)member).GetValue(element, null);
                }
                else if (member is MethodInfo) {
                    result = ((MethodInfo)member).Invoke(element, null);
                }
            }
            catch (Exception e) {
                Debug.LogError("When running " + member.ReflectedType + "::" + member.Name + ", caught exception " + e);
                Debug.LogException(e);
                result = defaultValue;
            }

            if (result.GetType() != typeof(bool)) {
                Debug.LogError(member.ReflectedType + "::" + member.Name + " needs to return a bool to be used with [InspectorDisplayIf]");
                result = defaultValue;
            }

            return (bool)result;
        }
        private static bool ShouldShowMemberDynamic(object element, MemberInfo member) {
            var showIf = member.GetAttribute<InspectorShowIfAttribute>();
            if (showIf != null) {
                return GetBooleanReflectedMember(element, showIf.ConditionalMemberName, /*defaultValue:*/true);
            }

            return true;
        }

        /// <summary>
        /// Draws the actual property editors.
        /// </summary>
        private object EditPropertiesButtons(Rect region, object element, fiGraphMetadata metadata) {
            var orderedMembers = _metadata.GetMembers(InspectedMemberFilters.InspectableMembers);
            for (int i = 0; i < orderedMembers.Count; ++i) {
                InspectedMember member = orderedMembers[i];

                // requested skip
                if (ShouldShowMemberDynamic(element, member.MemberInfo) == false) {
                    continue;
                }

                if (member.IsMethod) {
                    EditButton(ref region, element, member.Method);
                }
                else {
                    EditProperty(ref region, element, member.Property, metadata);
                }

                region.y += DividerHeight;
            }

            return element;
        }

        public object Edit(Rect region, GUIContent label, object element, fiGraphMetadata metadata) {
            try {
                if (_cycleEdit == null) {
                    _cycleEdit = new fiCycleDetector(_cycleHeight, _cycleScene);
                }
                _cycleEdit.Enter();

                if (_cycleEdit.TryMark(element) == false) {
                    EditorGUI.LabelField(region, label, new GUIContent("<cycle>"));
                    return element;
                }

                if (HasLabel(label)) {
                    region = DrawLabel(region, label);
                }

                if (element == null) {
                    // if the user want's an instance, we'll create one right away We also check to
                    // make sure we should automatically instantiate references, as if we're pretty
                    // far down in the nesting level there may be an infinite recursion going on
                    if (fiSettings.InspectorAutomaticReferenceInstantation &&
                        _metadata.HasDefaultConstructor &&
                        ShouldAutoInstantiate()) {

                        element = _metadata.CreateInstance();
                        GUI.changed = true;
                    }

                    // otherwise we show a button to create an instance
                    else {
                        string buttonMessage = "null - create instance (ctor)?";
                        if (_metadata.HasDefaultConstructor == false) {
                            buttonMessage = "null - create instance (unformatted)?";
                        }
                        if (fiEditorGUI.LabeledButton(region, _metadata.ReflectedType.Name, buttonMessage)) {
                            element = _metadata.CreateInstance();
                            GUI.changed = true;
                        }

                        return element;
                    }
                }

                return EditPropertiesButtons(region, element, metadata);

            }
            finally {
                _cycleEdit.Exit();
                if (_cycleEdit.Depth == 0) {
                    _cycleEdit = null;
                }
            }
        }

        public float GetElementHeight(GUIContent label, object element, fiGraphMetadata metadata) {
            try {
                if (_cycleHeight == null) {
                    _cycleHeight = new fiCycleDetector(_cycleEdit, _cycleScene);
                }
                _cycleHeight.Enter();

                if (_cycleHeight.TryMark(element) == false) {
                    return EditorStyles.label.CalcHeight(GUIContent.none, 100);
                }

                float height = HasLabel(label) ? TitleHeight + RectTools.IndentVertical : 0;

                if (element == null) {
                    // if the user want's an instance, we'll create one right away. We also check to
                    // make sure we should automatically instantiate references, as if we're pretty
                    // far down in the nesting level there may be an infinite recursion going on
                    if (fiSettings.InspectorAutomaticReferenceInstantation &&
                        _metadata.HasDefaultConstructor &&
                        ShouldAutoInstantiate()) {

                        element = _metadata.CreateInstance();
                        GUI.changed = true;
                    }

                    // otherwise we show a button to create an instance
                    else {
                        height += ButtonHeight;
                    }
                }

                if (element != null) {
                    height += (ButtonHeight + DividerHeight) * _metadata.GetMethods(InspectedMemberFilters.ButtonMembers).Count;

                    var inspectedProperties = _metadata.GetProperties(InspectedMemberFilters.InspectableMembers);
                    for (int i = 0; i < inspectedProperties.Count; ++i) {
                        var property = inspectedProperties[i];

                        // requested skip
                        if (ShouldShowMemberDynamic(element, property.MemberInfo) == false) {
                            continue;
                        }

                        height += fiEditorGUI.EditPropertyHeight(element, property, metadata.Enter(property.Name));
                        height += DividerHeight;
                    }

                    // Remove the last divider
                    if (inspectedProperties.Count > 0) height -= DividerHeight;
                }

                return height;
            }
            finally {
                _cycleHeight.Exit();
                if (_cycleHeight.Depth == 0) {
                    _cycleHeight = null;
                }
            }
        }

        public GUIContent GetFoldoutHeader(GUIContent label, object element) {
            return label;
        }

        public static IPropertyEditor TryCreate(Type dataType, ICustomAttributeProvider attributes) {
            // The reflected property editor is applicable to *every* type except collections, where
            // it is expected that the ICollectionPropertyEditor will take over (or something more
            // specific than that, such as the IListPropertyEditor).

            var metadata = InspectedType.Get(dataType);
            if (metadata.IsCollection) {
                return null;
            }

            return new ReflectedPropertyEditor(metadata, attributes);
        }
    }
}