#if false &&  UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// custom editor inspector for 'Station'
/// will only compile in Editor
/// </summary>
[CustomEditor(typeof(Station))]
public class Station_Inspector : Editor 
{
    Station focus;
	public override void OnInspectorGUI() 
    {
		//get the selected object the inspector is revealing
		focus = target as Station;
		
		GUILayout.BeginHorizontal();
		GUILayout.Space(5);
		GUILayout.BeginVertical();
		GUILayout.Space(5);
		
		GUILayout.BeginVertical("Box");
		//draw the default inspector before any own draw instructions
		//you can comment this out, if you don't need the default inspector
		DrawDefaultInspector();
		GUILayout.Space(5);

		GUILayout.Label("Station Neighbors");

        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Station Name", GUILayout.Width(200));
        GUILayout.Label("Taxi", GUILayout.Width(50));
        GUILayout.Label("Bus", GUILayout.Width(50));
        GUILayout.Label("Metro", GUILayout.Width(50));
        GUILayout.Label("Ferry", GUILayout.Width(50));

        GUILayout.EndHorizontal();

        var neighbors = focus.GetAllStationNeighbours();
        foreach(Station station in neighbors)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(station.name, GUILayout.Width(200));
            DrawStationConnection(TransportationType.Taxi, station);
            DrawStationConnection(TransportationType.Bus, station);
            DrawStationConnection(TransportationType.Metro, station);
            DrawStationConnection(TransportationType.Ferry, station);

            GUILayout.EndHorizontal();
        }
		GUILayout.EndVertical();

		GUILayout.Space(5);
		GUILayout.EndHorizontal();
		GUILayout.Space(5);
		GUILayout.EndVertical();
		
		//check for changes in values
		if (GUI.changed) {
			
		}
	}

    void DrawStationConnection(TransportationType transport, Station neighbor)
    {
        bool enabled = focus.HasTransportationNeighbour(transport, neighbor);
        GUILayout.Toggle(enabled, "", GUILayout.Width(50));
    }

}
#endif