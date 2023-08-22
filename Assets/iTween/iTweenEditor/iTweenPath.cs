//by Bob Berkebile : Pixelplacement : http://www.pixelplacement.com

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class iTweenPath : MonoBehaviour
{
	public string pathName ="";
	public Color pathColor = Color.cyan;
	public List<Vector3> nodes = new List<Vector3>(){Vector3.zero, Vector3.zero, Vector3.zero};
	public int nodeCount;
	public static Dictionary<string, iTweenPath> paths = new Dictionary<string, iTweenPath>();
	public bool initialized = false;
	public string initialName = "";
	
	void OnEnable(){
		paths.Add(pathName.ToLower(), this);
	}

    void OnDestroy()
    {
        paths.Remove(pathName.ToLower());
    }
	
	void OnDrawGizmosSelected(){
		if(enabled) { // dkoontz
			if(nodes.Count > 0){
				iTween.DrawPath(nodes.ToArray(), pathColor);
			}
		} // dkoontz
	}
	
	public static Vector3[] GetPath(string requestedName)
    {
		requestedName = requestedName.ToLower();
		if(paths.ContainsKey(requestedName))
        {
            return GetPath(paths[requestedName]);
		}
        else
        {
			Debug.Log("No path with that name exists! Are you sure you wrote it correctly?");
			return null;
		}
	}

    public static Vector3[] GetPath(iTweenPath path)
    {
        return path.nodes.ToArray();
    }

    public static Vector3[] GetPathReversed(string requestedName)
    {
        requestedName = requestedName.ToLower();
        if (paths.ContainsKey(requestedName))
        {
            return GetPathReversed(paths[requestedName]);
        }
        else
        {
            Debug.Log("No path with that name exists! Are you sure you wrote it correctly?");
            return null;
        }
    }

    public static Vector3[] GetPathReversed(iTweenPath path)
    {
        var nodes = path.nodes.ToList();
        nodes.Reverse();
        return nodes.ToArray();
    }

}

