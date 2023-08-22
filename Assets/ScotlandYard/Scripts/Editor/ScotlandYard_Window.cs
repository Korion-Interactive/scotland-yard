#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// ScotlandYard_Window
/// </summary>
public class ScotlandYard_Window : EditorWindow
{
    static readonly Color success_color = new Color(0, 0.8f, 0.4f);

    List<GameObject> selectedStations = new List<GameObject>();

    UIAtlas atlasBack, atlasFront;

//	ScotlandYard_Window window;

	[MenuItem("Window/Scotland Yard/Scotland Yard Editor")]
	static void CallCreateWindow()
    {
		// Get existing open window or if none, make a new one:
		ScotlandYard_Window window = (ScotlandYard_Window)EditorWindow.GetWindow(typeof(ScotlandYard_Window));
		window.autoRepaintOnSceneChange = true;
		window.titleContent.text = "Scotland Yard Editor";
		window.maxSize = new Vector2(256f, 256f);
//		window.window = window;
		window.Show();
	}
	
    void OnSelectionChange()
    {
        if (Selection.gameObjects.Length == 0)
        {
            selectedStations.Clear();
        }
        else
        {
            GameObject newStation, oldStation;
            do
            {
                oldStation = null;
                newStation = Selection.gameObjects.FirstOrDefault((o) => !selectedStations.Contains(o) && o.GetComponent<Station>() != null);

                if (newStation != null)
                {
                    selectedStations.Add(newStation);
                }
                else
                {
                    oldStation = selectedStations.FirstOrDefault((o) => !Selection.gameObjects.Contains(o));
                    if (oldStation != null)
                        selectedStations.Remove(oldStation);
                }
            } while (newStation != null || oldStation != null);
        }
    }

    //void OnInspectorUpdate()
    //{

    //}


	void OnGUI()
    {
		//add a space to title bar
		GUILayout.Space(16);

		//add some borders left and right, top and bottom
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(5);
		EditorGUILayout.BeginVertical();
		GUILayout.Space(5);
		
		//your content goes here
		EditorGUILayout.BeginVertical("Box");

		GUILayout.Label("Connections");

        if(GUILayout.Button("Recalculate Station Connections"))
            RecalculateConnections();

        GUILayout.Space(5);
        if (GUILayout.Button("Select Invalid Connections"))
            SelectInvalidConnections();

        GUILayout.Space(5);
        if (GUILayout.Button("Select Next Double Connection"))
            SelectDoubleConnection();

        //if (GUILayout.Button("Create Station Labels"))
        //    CreateStationLabels();

		atlasBack = EditorGUILayout.ObjectField("Atlas Back", atlasBack, typeof(UIAtlas),allowSceneObjects:false) as UIAtlas;
		atlasFront = EditorGUILayout.ObjectField("Atlas Front", atlasFront, typeof(UIAtlas),allowSceneObjects:false) as UIAtlas;
        if (GUILayout.Button("Create Highlights"))
            CreateStationHighlights();

        GUILayout.Space(10);

        if (GUILayout.Button("Adjust Connection Path Positions"))
            AdjustConnectionPathPositions();


        GUILayout.Space(10);
        GUILayout.Label("Connect Selected Stations");

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Taxi"))
            ConnectSelection(TransportationType.Taxi);

        GUILayout.Space(5);
        if (GUILayout.Button("Bus"))
            ConnectSelection(TransportationType.Bus);

        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Metro"))
            ConnectSelection(TransportationType.Metro);

        GUILayout.Space(5);
        if (GUILayout.Button("Ferry"))
            ConnectSelection(TransportationType.Ferry);

        GUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();

		GUILayout.Space(5);
		EditorGUILayout.EndVertical();
		GUILayout.Space(5);
		EditorGUILayout.EndHorizontal();
	}

    private void CreateStationLabels()
    {
        Station[] stations = GameObject.FindObjectsOfType<Station>();
        List<GameObject> select = new List<GameObject>();
        foreach (var s in stations)
        {
            var lbl = s.CreateLabel();
            select.Add(lbl.gameObject);
        }
        Selection.objects = select.ToArray();
    }

    private void CreateStationHighlights()
    {
        Station[] stations = GameObject.FindObjectsOfType<Station>();

        foreach (Station station in stations)
        {
            GameObject highlight = station.transform.GetChildByName("highlight").gameObject;
            GameObject.DestroyImmediate(highlight);
        }
        
        List<GameObject> select = new List<GameObject>();
        foreach (var s in stations)
        {
            var hghlght = s.CreateHighlights(atlasBack, atlasFront);
            select.Add(hghlght);
        }
        Selection.objects = select.ToArray();
    }

    private void AdjustConnectionPathPositions()
    {
        foreach (var item in GameObject.FindObjectsOfType<StationConnection>())
        {
            item.AdjustPathPosition();
        }
    }

    private void ConnectSelection(TransportationType transportationType)
    {
        if (selectedStations.Count < 2)
        {
            this.LogError("To connect stations, you have to select at least 2 stations.");
            return;
        }

        var parent = GameObject.FindObjectsOfType<StationConnectionParent>().FirstOrDefault((o) => o.Transportation == transportationType);
        if (parent == null)
        {
            this.LogError("Couldn't connect stations. No StationConnectionParent found with type " + transportationType.ToString());
            return;
        }
        
        for (int i = 0; i < selectedStations.Count - 1; i++)
        {
            var a = selectedStations[i].GetComponent<Station>();
            var b = selectedStations[i + 1].GetComponent<Station>();

            var go = new GameObject(transportationType.ToString() + " [" + a.Id + "] to [" + b.Id + "]");
            var co = go.AddComponent<StationConnection>();
            go.transform.parent = parent.transform;

            co.StationA = a;
            co.StationB = b;
            co.Transportation = transportationType;

            co.UpdatePosition();
            co.ApplyConnectionToStations();

            a.SaveState();
            b.SaveState();
        }

        this.LogInfo(selectedStations.Count.ToString() + " stations connected.", success_color);

        Selection.objects = new Object[0];
        selectedStations.Clear();

    }


    private void SelectDoubleConnection()
    {
        StationConnection[] connections = GameObject.FindObjectsOfType<StationConnection>();
        for(int i = 0; i < connections.Length; i++)
        {
            for(int k = i + 1; k < connections.Length; k++)
            {
                StationConnection a = connections[i];
                StationConnection b = connections[k];

                if(a.StationA == b.StationA && a.StationB == b.StationB && a.Transportation == b.Transportation)
                {
                    Selection.objects = new Object[] { a.gameObject, b.gameObject };
                    return;
                }

            }
        }
    }

    private void SelectInvalidConnections()
    {
        List<GameObject> foundObjects = new List<GameObject>();
        StationConnection[] connections = GameObject.FindObjectsOfType<StationConnection>();
        foreach (var c in connections)
        {
            if(c.StationA == null || c.StationB == null || c.StationA == c.StationB || c.Transportation == TransportationType.Any)
            {
                foundObjects.Add(c.gameObject);
            }
            else
            {
                var path =  c.GetComponent<iTweenPath>();
                if (path == null || path.nodes == null)
                {
                    foundObjects.Add(c.gameObject);
                }
                else
                {
                    foreach (Vector3 vec in path.nodes)
                    {
                        if (float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsNaN(vec.z))
                        {
                            foundObjects.Add(c.gameObject);
                            break;
                        }
                    }
                }
            }
        }

        Selection.objects = foundObjects.ToArray();
    }

    private void RecalculateConnections()
    {
        Station[] stations = GameObject.FindObjectsOfType<Station>();
        foreach(var s in stations)
            s.ClearNeighbours();

        StationConnection[] connections = GameObject.FindObjectsOfType<StationConnection>();
        foreach (var c in connections)
        {
            c.UpdatePosition();
            c.ApplyConnectionToStations();
        }

        foreach (var s in stations)
        {
            UISprite sprite = s.GetComponent<UISprite>();
            if(sprite != null)
            {
                if (s.HasAnyTransportationOption(TransportationType.Metro))
                    sprite.spriteName = "station_tube";
                else if (s.HasAnyTransportationOption(TransportationType.Bus))
                    sprite.spriteName = "station_bus";
                else
                    sprite.spriteName = "station_taxi";
            }  

            s.SaveState();
            s.RestoreState();
        }
        this.LogInfo("recalculation done.", success_color);
        Selection.objects = new Object[0];
    }
}
#endif 