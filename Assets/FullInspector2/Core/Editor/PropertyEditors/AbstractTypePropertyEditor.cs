﻿using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Internal {
    /// <summary>
    /// Provides a property editor for types which cannot be instantiated directly and require the
    /// user to select a specific instance to instantiate.
    /// </summary>
    public class AbstractTypePropertyEditor : IPropertyEditor, IPropertyEditorEditAPI {
        private AbstractTypeInstanceOptionManager _options;

        public AbstractTypePropertyEditor(Type baseType) {
            _options = new AbstractTypeInstanceOptionManager(baseType);
        }

        public PropertyEditorChain EditorChain {
            get;
            set;
        }

        public bool CanIndentLabelForDropdown {
            get { return true; }
        }


        public object OnSceneGUI(object element) {
            if (element != null) {
                PropertyEditorChain chain = PropertyEditor.Get(element.GetType(), null);
                IPropertyEditor editor = chain.SkipUntilNot(typeof(AbstractTypePropertyEditor));

                return editor.OnSceneGUI(element);
            }
            return element;
        }

        public object Edit(Rect region, GUIContent label, object element, fiGraphMetadata metadata) {
            metadata.Enter("AbstractTypeEditor").Metadata.GetMetadata<DropdownMetadata>().OverrideDisable = true;

            try {
                fiEditorGUI.AnimatedBegin(ref region, metadata);

                _options.RemoveExtraneousOptions();


                // draw the popup
                {
                    int popupHeight = (int)EditorStyles.popup.CalcHeight(GUIContent.none, 100);

                    Rect popupRegion = new Rect(region);
                    popupRegion.height = popupHeight;
                    region.y += popupRegion.height;
                    region.height -= popupRegion.height;

                    int selectedIndex = _options.GetDisplayOptionIndex(element);
                    int updatedIndex = EditorGUI.Popup(popupRegion, label, selectedIndex, _options.GetDisplayOptions());

                    if (selectedIndex != updatedIndex) {
                        metadata.GetMetadata<AbstractTypeAnimationMetadata>().ChangedTypes = true;
                    }

                    element = _options.UpdateObjectInstance(element, selectedIndex, updatedIndex);
                }

                // no element; no editor
                if (element == null) {
                    return null;
                }

                // draw the instance specific property editor
                {
                    Rect selectedRegion = new Rect(region);
                    selectedRegion = RectTools.IndentedRect(selectedRegion);
                    region.y += selectedRegion.height;
                    region.height -= selectedRegion.height;

                    // show custom editor
                    PropertyEditorChain chain = PropertyEditor.Get(element.GetType(), null);
                    IPropertyEditor editor = chain.SkipUntilNot(typeof(AbstractTypePropertyEditor));

                    return editor.Edit(selectedRegion, GUIContent.none, element, metadata.Enter("AbstractTypeEditor"));
                }
            }
            finally {
                fiEditorGUI.AnimatedEnd(metadata);
            }
        }

        public float GetElementHeight(GUIContent label, object element, fiGraphMetadata metadata) {
            float height = EditorStyles.popup.CalcHeight(label, 100);

            height += RectTools.IndentVertical;

            if (element != null) {
                PropertyEditorChain chain = PropertyEditor.Get(element.GetType(), null);
                IPropertyEditor editor = chain.SkipUntilNot(typeof(AbstractTypePropertyEditor));

                height += editor.GetElementHeight(GUIContent.none, element, metadata.Enter("AbstractTypeEditor"));
            }

            var abstractTypeMetadata = metadata.GetMetadata<AbstractTypeAnimationMetadata>();
            height = fiEditorGUI.AnimatedHeight(height, abstractTypeMetadata.ChangedTypes, metadata);
            abstractTypeMetadata.ChangedTypes = false;
            return height;
        }

        public GUIContent GetFoldoutHeader(GUIContent label, object element) {
            return new GUIContent(label.text + " (" + fiReflectionUtilitity.GetObjectTypeNameSafe(element) + ")");
        }

        public bool CanEdit(Type dataType) {
            throw new NotSupportedException();
        }

        public static IPropertyEditor TryCreate(Type dataType) {
            if (dataType.IsAbstract || dataType.IsInterface ||
                fiReflectionUtilitity.GetCreatableTypesDeriving(dataType).Count() > 1) {

                return new AbstractTypePropertyEditor(dataType);
            }

            return null;
        }
    }
}