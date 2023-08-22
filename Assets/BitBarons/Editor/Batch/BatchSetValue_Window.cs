#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using BitBarons.Editor.Util;
using BitBarons.Util;
using BitBarons.Bindings;
using BitBarons.Editor.Bindings;
using UnityEditorInternal;


namespace BitBarons.Editor.Batch
{
    /// <summary>
    /// BatchCopy_Window
    /// </summary>
    public class BatchSetValue_Window : EditorWindow
    {
        bool objectsFoldOut = true;

        public List<GameObject> Objects = new List<GameObject>();

        ReorderableList listGui;

        GameObject PreviewObject { get { return (Objects.Count > 0) ? Objects[0] : null; } }
        //public SetValue SetValue = new SetValue();

        string format = "";

        public static List<BaseNamePartEditor> ValueParts = new List<BaseNamePartEditor>();

        VariableAccessorGui varToSet = new VariableAccessorGui(VariableAccessType.Set, false);

        //BatchSetValue_Window window;
        Vector2 scrollPos = Vector2.zero;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/Batch/Batch Set Value")]
        static void CallCreateWindow()
        {
            // Get existing open window or if none, make a new one:
            BatchSetValue_Window window = (BatchSetValue_Window)EditorWindow.GetWindow(typeof(BatchSetValue_Window));
            window.autoRepaintOnSceneChange = true;
			window.titleContent.text = "Batch Set Value";
            window.maxSize = new Vector2(500, 600);
            window.Show();
        }

        void OnInspectorUpdate()
        {
            this.Repaint();
        }


        void OnGUI()
        {
            GUILayout.Space(5);
            using (EditorGuiZone.Horizontal())
            {
                GUILayout.Space(5);
                using (EditorGuiZone.Vertical())
                {
                    GUILayout.Space(5);

                    using (EditorGuiZone.ScrollView(ref scrollPos))
                    {

                        // TITLE
                        using (EditorGuiZone.Vertical("Box"))
                            EditorGUILayout.LabelField("Batch Set Value", EditorStyles.boldLabel);

                        GUILayout.Space(5);


                        // AFFECTED OBJECT LIST
                        using (EditorGuiZone.Vertical("Box"))
                        {
                            EditorGUILayout.LabelField("Affected Objects", EditorStyles.boldLabel);

                            using (EditorGuiZone.Vertical("Box"))
                            {
                                if(EditorGUILayout.ObjectField("Drop Objects To Add", null, typeof(GameObject), true) != null)
                                {
                                    foreach(var obj in DragAndDrop.objectReferences.Where((o) => o is GameObject))
                                        Objects.Add(obj as GameObject);
                                }


                                objectsFoldOut = EditorGUILayout.Foldout(objectsFoldOut, string.Format("Object List [{0}]", Objects.Count));


                                if (objectsFoldOut)
                                {
                                    using (EditorGuiZone.Horizontal())
                                    {
                                        //if (GUILayout.Button("Add Current Selection"))
                                        //{
                                        //    foreach (var go in Selection.gameObjects)
                                        //        Objects.Add(go);
                                        //}

                                        if (GUILayout.Button("Sort by Name"))
                                            Objects.Sort((a, b) => a.name.CompareTo(b.name));

                                        if (GUILayout.Button("Clear List"))
                                            Objects.Clear();
                                    } 

                                    //ReorderableListGUI.Title("Affected Objects");
                                    if (listGui == null)
                                    {
                                        listGui = new ReorderableList(Objects, typeof(GameObject), true, true, true, true);
                                        listGui.drawHeaderCallback += (rect) => GUI.Label(rect, "Affected Objects List");
                                        listGui.drawElementCallback += (rect, index, active, focused)
                                            => { EditorGUI.ObjectField(rect, Objects[index], typeof(GameObject), true); };
                                        listGui.onRemoveCallback += (list) => { listGui = list; };
                                    }
                                    listGui.DoLayoutList();
                                }
                            }
                        }

                        GUILayout.Space(5);

                        // VALUE TO SET
                        using (EditorGuiZone.Vertical("Box"))
                        {
                            EditorGUILayout.LabelField("Target Variable", EditorStyles.boldLabel);

                            using (EditorGuiZone.Vertical("Box"))
                            {
                                if (varToSet.Accessor.Object != PreviewObject)
                                    varToSet.Accessor.Object = PreviewObject;

                                if (PreviewObject == null)
                                {
                                    EditorGUILayout.LabelField("Add at least one object to the list below first.");
                                    EditorGUILayout.LabelField("The first object of the list will be used for the setup.");
                                }
                                else
                                {
                                    varToSet.OnGUI();
                                }
                            }
                        }

                        GUILayout.Label("↑");

                        // VARIABLE COMPOSITION
                        SharedEditorMethods.NamePartsGUI(PreviewObject, ValueParts, "Source Value", ref format, null, null);

                        using (EditorGuiZone.Vertical("Box"))
                        {

                            using (EditorGuiZone.Horizontal())
                            {
                                EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel, GUILayout.Width(60));

                                string prev;
                                try
                                {
                                    prev = SharedEditorMethods.GetStringValue(ValueParts, format, Objects[0]);
                                }
                                catch
                                {
                                    prev = "<preview not possible>";
                                }

                                EditorGUILayout.LabelField(prev);

                            }
                        }

                        GUILayout.Space(15);

                        // APPLY
                        if (GUILayout.Button("Apply!", GUILayout.Height(50)))
                            ApplyAll();
                    }

                    GUILayout.Space(5);
                }
                GUILayout.Space(5);
            }
        }

        private void ApplyAll()
        {
            for (int i = 0; i < Objects.Count; i++)
            {
                var o = Objects[i];

                string result = SharedEditorMethods.GetStringValue(ValueParts, format, o, i);

                varToSet.Accessor.Object = o;
                varToSet.Accessor.ParseAndSetValue(result);
                //SetValue.Set(o, result);
            }
        }

        void OnDisable()
        {
            SharedEditorMethods.SerializeNamePartsForEditor("BatchSetValue", ValueParts);
        }

        void OnEnable()
        {
            ValueParts = SharedEditorMethods.DeserializeNamePartsForEditor("BatchSetValue", (Objects != null && Objects.Count > 0) ? Objects[0] : null);
        }
    }

}
#endif