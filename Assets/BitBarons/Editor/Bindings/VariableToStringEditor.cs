using BitBarons.Bindings;
using BitBarons.Editor.Util;
using BitBarons.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BitBarons.Editor.Bindings
{
    [Serializable]
    public class VariableToStringEditor : BaseNamePartEditor<VariableToString>
    {
        public override string NamePartType { get { return BaseNamePartEditor.TYPE_VARIABLE; } }

        public GameObject PreviewObject { get { return AccessorGui.Accessor.Object; } set { AccessorGui.Accessor.Object = value; } }
        //public string Component, Variable;
        public VariableAccessorGui AccessorGui;

        
        public VariableToStringEditor(GameObject previewObject, bool displayObjectField)
            : base(previewObject)
        {
            AccessorGui = new VariableAccessorGui(VariableAccessType.Get, displayObjectField);
            AccessorGui.Accessor = part.Accessor;

            if (AccessorGui.Accessor.Object == null)
                AccessorGui.Accessor.Object = previewObject;
        }

        public override void OnGUI()
        {
            OnGUIPickVariable();
            OnGUISetSubstring("Substring");
        }
        protected void OnGUIPickVariable()
        {
            AccessorGui.OnGUI();
        }
        protected void OnGUISetSubstring(string prefix)
        {
            using (EditorGuiZone.ToggleGroup("Substring", ref part.substringEnabled))
            {
                if (part.substringEnabled)
                {
                    part.SubstringStart = EditorGUILayout.IntField(prefix + " Start Index", part.SubstringStart);
                    part.SubstringLength = EditorGUILayout.IntField(prefix + " Length", part.SubstringLength);
                }
            }
        }
        public override void SerializeForEditor(string context)
        {
            EditorPrefs.SetBool(GetKey(context, "objField"), AccessorGui.DisplayObjectField);
            EditorPrefs.SetString(GetKey(context, "comp"), AccessorGui.Accessor.ComponentName);
            EditorPrefs.SetString(GetKey(context, "var"), AccessorGui.Accessor.VariableHierachy);
            EditorPrefs.SetBool(GetKey(context, "sub"), part.substringEnabled);
            EditorPrefs.SetInt(GetKey(context, "subStart"), part.SubstringStart);
            EditorPrefs.SetInt(GetKey(context, "subLength"), part.SubstringLength);
        }

        public override void DeserializeForEditor(string context)
        {
            AccessorGui.DisplayObjectField = EditorPrefs.GetBool(GetKey(context, "objField"));
            AccessorGui.Accessor.ComponentName = EditorPrefs.GetString(GetKey(context, "comp"));
            AccessorGui.Accessor.VariableHierachy = EditorPrefs.GetString(GetKey(context, "var"));
            part.substringEnabled = EditorPrefs.GetBool(GetKey(context, "sub"), false);
            part.SubstringStart = EditorPrefs.GetInt(GetKey(context, "subStart"), 0);
            part.SubstringLength = EditorPrefs.GetInt(GetKey(context, "subLength"), 1);
        }


        protected override VariableToString CreatePart(GameObject previewObject)
        {
            var result = ScriptableObject.CreateInstance<VariableToString>();
            result.Initialize(previewObject);
            return result;
        }
    }
}
