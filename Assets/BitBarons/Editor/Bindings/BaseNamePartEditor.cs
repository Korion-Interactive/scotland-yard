using BitBarons.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BitBarons.Editor.Bindings
{
    [Serializable]
    public abstract class BaseNamePartEditor
    {

        public const string TYPE_INCREMENT = "Incremental";
        public const string TYPE_VARIABLE = "Var-2-String";
        public const string TYPE_REGEX = "RegEx Matches";
        public const string TYPE_COMPLEX = "Complex Binding";

        public abstract string NamePartType { get; }

        public BaseNamePart NamePart;

        public void GUIHeader(string prefix)
        {
            EditorGUILayout.LabelField(prefix + NamePartType, EditorStyles.boldLabel);
        }
        public string GetNamePart(GameObject prefab, int number)
        {
            return NamePart.GetNamePart(prefab, number);
        }
        public abstract void OnGUI();

        public abstract void SerializeForEditor(string context);
        public abstract void DeserializeForEditor(string context);

        protected string GetKey(string context, string varName)
        {
            return context + NamePartType + varName;
        }
    }

    public abstract class BaseNamePartEditor<T> : BaseNamePartEditor
        where T : BaseNamePart
    {
        protected T part { get { return NamePart as T; } }

        protected BaseNamePartEditor(GameObject previewObject)
        {
            NamePart = CreatePart(previewObject);
        }

        protected abstract T CreatePart(GameObject previewObject);
    }
}
