#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Sunbow.Util.IO;
using BitBarons.Editor.Util;
using System.IO;
using System.Linq;


public class TableEditor_Window : EditorWindow
{
    Table table;

    List<float> columnWidths = new List<float>();
    Vector2 scroll;
    bool foldout;

	[MenuItem("Assets/Bit Barons/CSV Editor")]
	static void CallCreateWindow() 
    {
		// Get existing open window or if none, make a new one:
		TableEditor_Window window = (TableEditor_Window)EditorWindow.GetWindow(typeof(TableEditor_Window));
		window.titleContent.text = "CSV Table Editor";

        TextAsset asset = Selection.activeObject as TextAsset;
        if (asset != null)
            window.table = new Table(asset.text,
                string.Format("{0}{1}", Application.dataPath.Remove(Application.dataPath.Length - "Assets".Length), AssetDatabase.GetAssetPath(asset.GetInstanceID()))
                , new CSVSetting(false, false) { ColumnSeparator = '\t' });

        window.CalculateSizes();
		window.Show();
	}
	
    void CalculateSizes()
    {
        columnWidths.Clear();

        for (int c = 0; c < table.Columns; c++)
        {
            float maxCol = 50;

            for (int r = 0; r < table.Rows; r++)
            {

                var size = GUIStyle.none.CalcSize(new GUIContent(table[c, r]));

                if (size.x > maxCol)
                    maxCol = size.x;

            }
            columnWidths.Add(maxCol + 10);
        }

    }



	void OnGUI()
    {
        if(table == null)
        {
            EditorGUILayout.LabelField("Couldn't interpret file as table - text based tables only!");
            return;
        }

        if(EditorGuiZone.FoldOut(ref foldout, "Table Info & Settings"))
        {
            table.FileName = EditorGUILayout.TextField("File Path", table.FileName);
            using (EditorGuiZone.Horizontal())
            {
                if (GUILayout.Button("Reload", GUILayout.Width(200)))
                {
                    // TODO
                }
                else if (GUILayout.Button("Save", GUILayout.Width(200)))
                {
                    table.Save();
                }
            }
        }

        EditorGUILayout.Separator();

        using (EditorGuiZone.ScrollView(ref scroll))
        {
            // Print the header row
            PrintRow(0);
            EditorGUILayout.Separator();

            for (int r = 1; r < table.Rows; r++)
            {
                PrintRow(r);
            }

            if(GUILayout.Button("+"))
            {
                table.AppendRow("...");
            }


            using (EditorGuiZone.Horizontal())
            {
                float left = columnWidths.Sum() + columnWidths.Count * 4;
                float top = (foldout) ? 56 : 18;

                for (int i = columnWidths.Count - 1; i >= 0; i--)
                {
                    columnWidths[i] = EditorGUI.Slider(new Rect(left, top, 0, 50 * table.Rows), columnWidths[i], 10, 1600);

                    left -= columnWidths[i] + 4;
                }
            }
        }
	}

    void PrintRow(int row)
    {
        using (EditorGuiZone.Horizontal())
        {
            for (int c = 0; c < table.Columns; c++)
            {
                try
                {
                    table[c, row] = EditorGUILayout.TextArea(table[c, row], GUILayout.Width(columnWidths[c]));
                }
                catch (System.Exception ex)
                {
                    this.LogError(string.Format("error print cell {0}/{1}. \n{2}", row, c, ex));
                }
            }
        }
    }
}
#endif