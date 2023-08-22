#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BitBarons.Editor.Util;

/// <summary>
/// custom editor inspector for 'Identifier'
/// will only compile in Editor
/// </summary>
[CustomEditor(typeof(Identifier))]
public class Identifier_Inspector : Editor {
	
	public override void OnInspectorGUI() {

        base.OnInspectorGUI();
		Identifier focus = target as Identifier;

        using(EditorGuiZone.Horizontal())
        {
            if(GUILayout.Button("Find unused ID"))
            {
                focus.SetToNextValidId();
            }

            if(GUILayout.Button("Hashcode ID"))
            {
                focus.GenerateIDFromHashcode();
            }
        }


	}
}
#endif