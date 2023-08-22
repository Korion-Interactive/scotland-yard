using BitBarons.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BitBarons.Editor.Bindings
{
    [Serializable]
    public class RegExMatchesEditor : VariableToStringEditor
    {
        public override string NamePartType { get { return BaseNamePartEditor.TYPE_REGEX; } }

        RegExMatches regex { get { return base.NamePart as RegExMatches; } }

        public RegExMatchesEditor(GameObject previewObject, bool displayObjectField)
            : base(previewObject, displayObjectField)
        { }

        public override void OnGUI()
        {
            base.OnGUIPickVariable();

            regex.RegularExpression = EditorGUILayout.TextField("Regular Expression", regex.RegularExpression);

            base.OnGUISetSubstring("Match");

            //using(EditorGuiZone.ToggleGroup("Replace Match", ref ReplaceMatch))
            //{
            //    Replacement = EditorGUILayout.TextField("Replacement", Replacement);
            //}
        }

        public override void SerializeForEditor(string context)
        {
            base.SerializeForEditor(context);
            EditorPrefs.SetString(GetKey(context, "regex"), regex.RegularExpression);
        }
        public override void DeserializeForEditor(string context)
        {
            base.DeserializeForEditor(context);
            regex.RegularExpression = EditorPrefs.GetString(GetKey(context, "regex"));
        }

        protected override BitBarons.Bindings.VariableToString CreatePart(GameObject previewObject)
        {
            var result = ScriptableObject.CreateInstance<RegExMatches>();
            result.Initialize(previewObject);
            return result;
        }
    }
}
