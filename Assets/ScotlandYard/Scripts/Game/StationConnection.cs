using UnityEngine;
using System.Collections;
using System.Linq;

public class StationConnection : MonoBehaviour 
{
    const float HEIGHT_OFFSET = -0.1f;

    public TransportationType Transportation = TransportationType.Taxi;

    public Station StationA, StationB;

    public void AdjustPathPosition()
    {
        var path = GetComponent<iTweenPath>();
        if (path != null)
        {
            Vector2 preA = path.nodes[0];
            Vector2 preB = path.nodes[path.nodes.Count - 1];

            path.nodes[0] = StationA.transform.position;
            path.nodes[path.nodes.Count - 1] = StationB.transform.position;

            Vector2 postA = path.nodes[0];
            Vector2 postB = path.nodes[path.nodes.Count - 1];


            for (int i = 1; i < path.nodes.Count - 1; i++)
            {
                float scalX = (path.nodes[i].x - preA.x) / (preB.x - preA.x);
                float scalY = (path.nodes[i].y - preA.y) / (preB.y - preA.y);

                float x = postA.x + scalX * (postB.x - postA.x);
                float y = postA.y + scalY * (postB.y - postA.y);

                if (float.IsNaN(x))
                    x = path.nodes[i].x;
                if (float.IsNaN(y))
                    y = path.nodes[i].y;

                path.nodes[i] = new Vector3(x, y, StationA.transform.position.z + HEIGHT_OFFSET);
            }

            //if (path.nodes.Count >= 1 && path.nodes[0] == Vector3.zero)
            //{
            //    path.nodes[0] = StationA.transform.position;
            //}
            //if (path.nodes.Count >= 2 && path.nodes[1] == Vector3.zero)
            //{
            //    path.nodes[1] = 0.5f * (StationA.transform.position + StationB.transform.position);
            //}
            //if (path.nodes.Count >= 3 && path.nodes[2] == Vector3.zero)
            //{
            //    path.nodes[2] = StationB.transform.position;
            //}

        }
    }
	// Use this for initialization
	void Start () 
    {
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnEnable()
    {
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(StationA != null && StationB != null)
        {
            Vector3 delta = new Vector3();
            Color col = Color.blue;
            switch(Transportation)
            {
                case TransportationType.Taxi:
                    col = Color.yellow;
                    break;
                case TransportationType.Bus:
                    col = Color.green;
                    delta += new Vector3(0, 0.01f, 0);
                    break;
                case TransportationType.Metro:
                    col = Color.magenta;
                    delta += new Vector3(0, -0.01f, 0);
                    break;
                case TransportationType.Ferry:
                    col = Color.black;
                    break;

            }

            DrawConnectionGizmo(delta, col);
            //Gizmos.DrawLine(StationA.transform.position + delta, StationB.transform.position + delta);
        }
    }
    void OnDrawGizmosSelected()
    {
        if (StationA != null && StationB != null)
        {
            //Gizmos.color = Color.white;

            UpdatePosition();

            Vector3 delta = new Vector3(0, -0.005f, 0);
            //Gizmos.DrawLine(StationA.transform.position + delta, StationB.transform.position + delta);
            DrawConnectionGizmo(delta, Color.white);

            delta = new Vector3(0, 0.005f, 0);
            //Gizmos.DrawLine(StationA.transform.position + delta, StationB.transform.position + delta);
            DrawConnectionGizmo(delta, Color.white);
        }
    }

    void DrawConnectionGizmo(Vector3 delta, Color color)
    {
        var path = GetComponent<iTweenPath>();
        if (path != null)
        {
            iTween.DrawPathGizmos(path.nodes.Select((n) => n + delta).ToArray(), color);
        }
        else
        {
            Gizmos.color = color;
            Gizmos.DrawLine(StationA.transform.position + delta, StationB.transform.position + delta);
        }
    }

    

    public void UpdatePosition()
    {
        transform.position = Vector3.zero;
        //if (StationA != null && StationB != null)
        //{
        //    Vector3 pos = (StationA.transform.position + StationB.transform.position) / 2;
        //    pos += Vector3.one; // a bit shifted to not hinder the path tool
        //    transform.position = pos;
        //}
    }

    public void ApplyConnectionToStations()
    {
        if (StationA != null && StationB != null)
        { 
            StationA.AddNeighbour(StationB, Transportation);
            StationB.AddNeighbour(StationA, Transportation);
        }
    }
#endif

    public static StationConnection Find(Station a, Station b, TransportationType transport)
    {
        var connections = GameObject.FindObjectsOfType<StationConnection>().Where((c)
            => ((c.StationA == a && c.StationB == b) || (c.StationA == b && c.StationB == a))).ToList();

        for(int i = 0; i < connections.Count; i++)
        {
            if (connections[i].Transportation == transport)
                return connections[i];
        }

        // fallback
        Log.warn(transport, "no connection found: " + a.Id + " > " + b.Id + " -- trying to use another transportation.");
        return (connections.Count > 0) ? connections[0] : null;
    }
}
