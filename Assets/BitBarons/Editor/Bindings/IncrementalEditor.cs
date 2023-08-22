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
    public class IncrementalEditor : BaseNamePartEditor<Incremental>
    {
        public override string NamePartType { get { return BaseNamePartEditor.TYPE_INCREMENT; } }

        public IncrementalEditor()
            :base(null)
        {

        }
        public override void OnGUI()
        {
            part.StartIndex = EditorGUILayout.IntField("Start Index", part.StartIndex);
            part.Increment = EditorGUILayout.IntField("Increment", part.Increment);
        }


        public override void SerializeForEditor(string context)
        {
            EditorPrefs.SetInt(GetKey(context, "start"), part.StartIndex);
            EditorPrefs.SetInt(GetKey(context, "incr"), part.Increment);
        }

        public override void DeserializeForEditor(string context)
        {
            part.StartIndex = EditorPrefs.GetInt(GetKey(context, "start"), 0);
            part.Increment = EditorPrefs.GetInt(GetKey(context, "incr"), 1);
        }

        protected override Incremental CreatePart(GameObject previewObject)
        {
            var result = ScriptableObject.CreateInstance<Incremental>();
            result.Initialize(previewObject);
            return result;
        }
    }
}
