using BitBarons.Bindings;
using BitBarons.Editor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BitBarons.Editor.Bindings
{
    public static class SharedEditorMethods
    {
        static Dictionary<int, bool> foldOuts = new Dictionary<int, bool>();


        internal static string GetStringValue(List<BaseNamePartEditor> nameParts, string format, GameObject obj, int idx = 0)
        {
            return SharedMethods.GetStringValue(nameParts.Select((o) => o.NamePart).ToList(), format, obj, idx);
        }

        // TODO: ON DELETE CALLBACK
        internal static void NamePartsGUI(GameObject previewObject, List<BaseNamePartEditor> nameParts, string title, ref string format, Action<BaseNamePartEditor> onCreate, Action<BaseNamePartEditor> onDelete)
        {
            NamePartsGUI(previewObject, nameParts, title, ref format, onCreate, onDelete, false);
        }
        internal static void NamePartsGUI(GameObject previewObject, List<BaseNamePartEditor> nameParts, string title, ref string format, Action<BaseNamePartEditor> onCreate, Action<BaseNamePartEditor> onDelete, bool displayObjectFields)
        {
            using (EditorGuiZone.Vertical("Box"))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

                format = EditorGUILayout.TextField("Format", format);

                for (int i = 0; i < nameParts.Count; i++)
                {
                    if (nameParts[i] is VariableToStringEditor && (nameParts[i] as VariableToStringEditor).AccessorGui.Accessor.Object == null)
                    {
                        (nameParts[i] as VariableToStringEditor).AccessorGui.Accessor.Object = previewObject;
                    }
                    using (EditorGuiZone.Horizontal())
                    {
                        using (EditorGuiZone.Vertical("Box"))
                        {
                            EditorGUILayout.LabelField("{" +i + "}", GUILayout.Width(25));
                        }

                        using (EditorGuiZone.Vertical("Box"))
                        {
                            using (EditorGuiZone.Horizontal())
                            {

                                nameParts[i].GUIHeader("");//string.Format("{0}. ", i));

                                #region old swap code
                                //
                                // SWAP NOT ALLOWED SINCE SPLIT OF EDITOR AND LOGIC PART
                                //
                                //// swap with below
                                //if (i < nameParts.Count - 1)
                                //{
                                //    if (GUILayout.Button("▼", GUILayout.Width(20)))
                                //    {
                                //        var o = nameParts[i];
                                //        nameParts.RemoveAt(i);
                                //        nameParts.Insert(i + 1, o);
                                //        break;
                                //    }
                                //}

                                //// swap with above
                                //if (i > 0)
                                //{
                                //    if (GUILayout.Button("▲", GUILayout.Width(20)))
                                //    {
                                //        var o = nameParts[i];
                                //        nameParts.RemoveAt(i);
                                //        nameParts.Insert(i - 1, o);
                                //        break;
                                //    }
                                //}
                                //else GUILayout.Space(20);
                                #endregion

                                // Delete button
                                if (GUILayout.Button("×", GUILayout.Width(20)))
                                {
                                    var obj = nameParts[i];

                                    nameParts.RemoveAt(i);
                                    format = format.Replace("{" + nameParts.Count + "}", "");

                                    if (onDelete != null)
                                        onDelete(obj);

                                    break;
                                }
                            }

                            if (GetFoldOut("details", title + i.ToString() + nameParts[i].NamePartType))
                                nameParts[i].OnGUI();
                        }
                    }
                }

                // Add Part
                using (EditorGuiZone.Vertical("Box"))
                {
                    if (GetFoldOut("Add Name Part", title))
                    {
                        AddNamePartButton("Increment", nameParts, ref format, () => new IncrementalEditor(), onCreate);
                        AddNamePartButton("Variable to String", nameParts, ref format, () => new VariableToStringEditor(previewObject, displayObjectFields), onCreate);
                        AddNamePartButton("Regular Expression", nameParts, ref format, () => new RegExMatchesEditor(previewObject, displayObjectFields), onCreate);
                        AddNamePartButton("Complex Binding", nameParts, ref format, () => new ComplexBindingEditor(previewObject), onCreate);
                    }
                }
            }
        }


        public static bool GetFoldOut(string foldoutName, string mainTitle)
        {
            int hash = foldoutName.GetHashCode() ^ mainTitle.GetHashCode();
            if (!foldOuts.ContainsKey(hash))
                foldOuts.Add(hash, true);

            foldOuts[hash] = EditorGUILayout.Foldout(foldOuts[hash], foldoutName);
            return foldOuts[hash];
        }

        private static void AddNamePartButton(string buttonLabel, List<BaseNamePartEditor> nameParts, ref string format, Func<BaseNamePartEditor> createNamePartMethod, Action<BaseNamePartEditor> onCreateCallback)
        {
            if (GUILayout.Button(buttonLabel))
            {
                var obj = createNamePartMethod();
                nameParts.Add(obj);
                format += "{" + (nameParts.Count - 1) + "}";

                if (onCreateCallback != null)
                    onCreateCallback(obj);
            }
        }



        public static void SerializeNamePartsForEditor(string context, List<BaseNamePartEditor> nameParts)
        {
            EditorPrefs.SetInt(context + "cnt", nameParts.Count);
            for(int i = 0; i < nameParts.Count; i++)
            {
                string c = context + i;
                EditorPrefs.SetString(c, nameParts[i].NamePartType);
                nameParts[i].SerializeForEditor(c);
            }
        }
        public static List<BaseNamePartEditor> DeserializeNamePartsForEditor(string context, GameObject previewObject)
        {
            List<BaseNamePartEditor> result = new List<BaseNamePartEditor>();
            int cnt = EditorPrefs.GetInt(context + "cnt");

            for (int i = 0; i < cnt; i++)
            {
                string type = EditorPrefs.GetString(context + i);
                BaseNamePartEditor part;
                switch(type)
                {
                    case BaseNamePartEditor.TYPE_INCREMENT:
                        part = new IncrementalEditor();
                        break;
                    case BaseNamePartEditor.TYPE_VARIABLE:
                        part = new VariableToStringEditor(previewObject, false);
                        break;
                    case BaseNamePartEditor.TYPE_REGEX:
                        part = new RegExMatchesEditor(previewObject, false);
                        break;
                    case BaseNamePartEditor.TYPE_COMPLEX:
                        part = new ComplexBindingEditor(previewObject);
                        break;

                    default:
                        throw new Exception("couldn't deserialize type: " + type);
                }

                part.DeserializeForEditor(context + i);

                result.Add(part);
            }
            return result;
        }


        public static List<BaseNamePartEditor> GetEditorParts(List<BaseNamePart> list, GameObject previewObject, bool displayObjectField)
        {
            List<BaseNamePartEditor> result = new List<BaseNamePartEditor>();
            foreach(BaseNamePart part in list)
            {
                switch(part.NamePartType)
                {
                    case BaseNamePart.TYPE_INCREMENT:
                        result.Add(new IncrementalEditor() { NamePart = part });
                        break;
                    case BaseNamePart.TYPE_VARIABLE:
                        result.Add(new VariableToStringEditor(previewObject, displayObjectField) { NamePart = part });
                        break;
                    case BaseNamePart.TYPE_REGEX:
                        result.Add(new RegExMatchesEditor(previewObject, displayObjectField) { NamePart = part });
                        break;
                    case BaseNamePart.TYPE_COMPLEX:
                        result.Add(new ComplexBindingEditor(previewObject) { NamePart = part });
                        break;
                }
            }
            return result;
        }
    }
}