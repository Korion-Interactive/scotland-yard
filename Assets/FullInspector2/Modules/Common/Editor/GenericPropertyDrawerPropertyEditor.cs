using System;
using FullInspector.Internal;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Modules {
    public class BaseMonoBehaviorContainer<T> : MonoBehaviour {
        public T Item;
    }

    public class ItemMetadata<T> : IGraphMetadataItem {
        [NonSerialized]
        public BaseMonoBehaviorContainer<T> Container;
        [NonSerialized]
        public SerializedObject SerializedObject;
        [NonSerialized]
        public SerializedProperty SerializedProperty;
    }

    public class GenericPropertyDrawerPropertyEditor<TContainer, T> : PropertyEditor<T>
        where TContainer : BaseMonoBehaviorContainer<T> {
        
        // see http://answers.unity3d.com/questions/436295/how-to-have-a-gradient-editor-in-an-editor-script.html
        // for the inspiration behind this approach

        T DoPropertyField(ItemMetadata<T> metadata, Rect region, GUIContent label, T item) {
            metadata.Container.Item = item;
            metadata.SerializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(region, metadata.SerializedProperty, label);
            if (EditorGUI.EndChangeCheck()) {
                metadata.SerializedObject.ApplyModifiedProperties();
            }

            return metadata.Container.Item;
        }

        float DoPropertyFieldHeight(ItemMetadata<T> metadata, GUIContent label, T item) {
            metadata.Container.Item = item;
            metadata.SerializedObject.Update();

            return EditorGUI.GetPropertyHeight(metadata.SerializedProperty, label);
        }

        private ItemMetadata<T> GetContainer(fiGraphMetadata metadata) {
            var container = metadata.GetMetadata<ItemMetadata<T>>();

            // note: we delay construction of the GameObject because when the metadata is
            //       deserialized, we don't want to construct a new GameObject in another thread
            //       via the GradientContainer constructor
            if (container.Container == null) {
                // note: using HideFlags.HideAndDontSave includes HideFlags.NotEditable
                var obj = EditorUtility.CreateGameObjectWithHideFlags("Proxy editor for " + typeof(T).CSharpName(),
                    HideFlags.HideInHierarchy | HideFlags.DontSave);

                container.Container = obj.AddComponent<TContainer>();

                container.SerializedObject = new SerializedObject(container.Container);
                container.SerializedProperty = container.SerializedObject.FindProperty("Item");
            }

            return container;
        } 

        public override T Edit(Rect region, GUIContent label, T element, fiGraphMetadata metadata) {
            var container = GetContainer(metadata);
            return DoPropertyField(container, region, label, element);
        }

        public override float GetElementHeight(GUIContent label, T element, fiGraphMetadata metadata) {
            var container = GetContainer(metadata);
            return DoPropertyFieldHeight(container, label, element);
        }
    }


    // TODO: to get support for UnityEvent{T}, UnityEvent{T1, T2}, etc we need to dynamically
    //       generate these two classes
    [CustomPropertyEditor(typeof(Gradient))]
    public class GradientPropertyEditor : GenericPropertyDrawerPropertyEditor<GradientMonoBehaviorContainer, Gradient> { }
    public class GradientMonoBehaviorContainer : BaseMonoBehaviorContainer<Gradient> { }
}