#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BitBarons.Editor.Util;
using BitBarons.Bindings;
using BitBarons.Editor.Bindings;


namespace BitBarons.Editor.Batch
{
    /// <summary>
    /// BatchCopy_Window
    /// </summary>
    public class BatchCopy_Window : EditorWindow
    {

        public GameObject Prefab;
        public Transform Parent;
        public int Count;

        public Vector3 RowOffset = new Vector3(1, 0, 0);
        public Vector3 ColumnOffset = new Vector3(0, 1, 0);
        public Vector3 LineOffset = new Vector3(0, 0, 1);
        public int RowCellCount = 10;
        public int ColumnCellCount = 10;
        public int LineCellCount = 10;
        bool offsetToggle = true;
        Vector2 scrollPos;

        string format = "";//"{0}_{1}";
        public List<BaseNamePartEditor> NameParts = new List<BaseNamePartEditor>();
        //{ 
        //    new VariableToString() { Variable = "name" }, 
        //    new Incremental(),
        //};

        BatchCopy_Window window;

        [MenuItem("Window/Batch/Batch Copy")]
        static void CallCreateWindow()
        {
            // Get existing open window or if none, make a new one:
            BatchCopy_Window window = (BatchCopy_Window)EditorWindow.GetWindow(typeof(BatchCopy_Window));
            window.autoRepaintOnSceneChange = true;
			window.titleContent.text = "Batch Copy";
            window.maxSize = new Vector2(500, 600);
            window.window = window;
            window.Show();
        }

        void OnInspectorUpdate()
        {
            if (window != null) window.Repaint();
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
                        {
                            EditorGUILayout.LabelField("Batch Copy", EditorStyles.boldLabel);
                        }

                        GUILayout.Space(5);

                        // TEMPLATE
                        using (EditorGuiZone.Vertical("Box"))
                        {

                            Prefab = EditorGUILayout.ObjectField("Prefab", Prefab, typeof(GameObject), true) as GameObject;
                            Parent = EditorGUILayout.ObjectField("Parent", Parent, typeof(Transform), true) as Transform;
                            Count = EditorGUILayout.IntField("Instance Count", Count);
                        }

                        GUILayout.Space(5);

                        // NAME PARTS
                        SharedEditorMethods.NamePartsGUI(Prefab, NameParts, "Object Name Composition", ref format, null, null);

                        GUILayout.Space(5);

                        // PREVIEW
                        using (EditorGuiZone.Vertical("Box"))
                        {

                            using (EditorGuiZone.Horizontal())
                            {
                                EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel, GUILayout.Width(60));

                                string prev;
                                try
                                {
                                    prev = SharedEditorMethods.GetStringValue(NameParts, format, Prefab);
                                }
                                catch
                                {
                                    prev = "<preview not possible>";
                                }

                                EditorGUILayout.LabelField(prev);

                            }
                        }

                        GUILayout.Space(5);

                        // OFFSET
                        using (EditorGuiZone.Vertical("Box"))
                        {
                            using (EditorGuiZone.ToggleGroup("Batch Offset", ref offsetToggle))
                            {
                                if (offsetToggle)
                                {
                                    using (EditorGuiZone.Vertical("Box"))
                                    {
                                        GUILayout.Label("Row Offset:");

                                        RowOffset = EditorGUILayout.Vector3Field("", RowOffset);
                                        RowCellCount = EditorGUILayout.IntField("Row Cell Count:", RowCellCount);
                                        //GUILayout.Space(20);
                                    }
                                    using (EditorGuiZone.Vertical("Box"))
                                    {
                                        GUILayout.Label("Column Offset:");

                                        ColumnOffset = EditorGUILayout.Vector3Field("", ColumnOffset);
                                        ColumnCellCount = EditorGUILayout.IntField("Column Cell Count:", ColumnCellCount);
                                        //GUILayout.Space(20);
                                    }
                                    using (EditorGuiZone.Vertical("Box"))
                                    {
                                        GUILayout.Label("Line Offset:");

                                        LineOffset = EditorGUILayout.Vector3Field("", LineOffset);
                                        LineCellCount = EditorGUILayout.IntField("Line Cell Count:", LineCellCount);
                                        //GUILayout.Space(20);
                                    }
                                }
                            }
                        }

                        GUILayout.Space(15);

                        // APPLY
                        using (EditorGuiZone.Horizontal())
                        {

                            if (GUILayout.Button("Batch Copy!", GUILayout.Height(50)))
                                Apply();

                            GUILayout.Space(5);
                        }

                        GUILayout.Space(5);
                    }
                }
            }
        }

        private void Apply()
        {
            Object[] select = new Object[Count];

            for (int i = 0; i < Count; i++)
            {
                GameObject go = GameObject.Instantiate(Prefab) as GameObject;

                string name = SharedEditorMethods.GetStringValue(NameParts, format, go, i);

                go.name = name;
                go.transform.parent = Parent;

                if (offsetToggle)
                {
                    int x = (i) % RowCellCount;
                    int y = ((i - x) / ColumnCellCount) % ColumnCellCount;
                    int z = ((i - y * ColumnCellCount - x) / (RowCellCount * LineCellCount)) % LineCellCount;

                    go.transform.localPosition = x * RowOffset + y * ColumnOffset + z * LineOffset;
                }

                select[i] = go;
            }

            Selection.objects = select;
        }
    }
}
#endif