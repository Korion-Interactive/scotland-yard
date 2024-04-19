using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Station : BaseBehaviour, IClickConsumable
{
    const float HIGHLIGHT_ROTATION_SPEED = 15;

    public static Station FindStation(int id)
    {
        GameObject go = GameObject.Find("s_" + id);

        if(go == null)
        {
            Log.error("Station", "couldn't find station with id " + id);
            return null;
        }

        return go.GetComponent<Station>();
    }

    public int Id;

    [SerializeField]
    [FullInspector.ShowInInspector]
    Dictionary<TransportationType, List<Station>> stationNeighbours = new Dictionary<TransportationType, List<Station>>();

    bool isHighlighted;
    public bool IsHighlighted { get { return isHighlighted; } }

    GameObject highlightContainer;
    Transform highlightBack, highlightFront;
    string spriteName;
    protected override void Awake()
    {
 	    base.Awake();
        highlightContainer = this.transform.GetChildByName("highlight").gameObject;
        highlightBack = highlightContainer.transform.GetChildByName("highlight_back");
        highlightFront = highlightContainer.transform.GetChildByName("highlight_front");

        this.spriteName = GetComponent<UISprite>().spriteName;

        //KORION: We need to hack here because it is not possible to edit this inside Unity Editor without crashing
        if(Id == 29)
        {
            if (stationNeighbours.ContainsKey(TransportationType.Taxi))
            { 
                foreach(Station s in stationNeighbours[TransportationType.Taxi])
                {
                    if(s.Id == 55)
                    {
                        stationNeighbours[TransportationType.Taxi].Remove(s);
                        break;
                    }
                }
            }
        }
        else if(Id == 55)
        {
            if (stationNeighbours.ContainsKey(TransportationType.Taxi))
            {
                foreach (Station s in stationNeighbours[TransportationType.Taxi])
                {
                    if (s.Id == 29)
                    {
                        stationNeighbours[TransportationType.Taxi].Remove(s);
                        break;
                    }
                }
            }
        }
    }

    void Update()
    {
       if(highlightContainer.activeSelf)
       {
           highlightBack.transform.Rotate(0, 0, HIGHLIGHT_ROTATION_SPEED * Time.deltaTime);
           highlightFront.transform.Rotate(0, 0, -HIGHLIGHT_ROTATION_SPEED * Time.deltaTime);
       }
    }

    public bool HasAnyTransportationOption(TransportationType transport)
    {
        if (stationNeighbours == null)
            return false; // something bad happened

        foreach (var key in stationNeighbours.Keys)
        {
            if (((transport & key) == key) && stationNeighbours[key].Count >= 0)
            {
                return true;
            }
        }
        return false;
    }
    public bool HasAllTransportationOptions(TransportationType transport)
    {
        foreach (var key in stationNeighbours.Keys)
        {
            if (((transport & key) != key) || stationNeighbours[key].Count <= 0)
                return false;
        }
        return true;
    }
    public bool HasTransportationNeighbour(TransportationType transport, Station neighbor)
    {
        return stationNeighbours.ContainsKey(transport) && stationNeighbours[transport].Contains(neighbor);
    }

    public void ClearNeighbours()
    {
        if (stationNeighbours == null)
            stationNeighbours = new Dictionary<TransportationType, List<Station>>();

        stationNeighbours.Clear();
    }

    public void AddNeighbour(Station station, TransportationType transport)
    {
        if (!stationNeighbours.ContainsKey(transport))
            stationNeighbours.Add(transport, new List<Station>());

        stationNeighbours[transport].Add(station);

        //base.SaveState();
    }


    public HashSet<Tuple<TransportationType, Station>> GetAllStationNeighbours()
    {
        HashSet<Tuple<TransportationType, Station>> result = new HashSet<Tuple<TransportationType, Station>>();

        foreach(var key in stationNeighbours.Keys)
        {
            foreach (var station in stationNeighbours[key])
                result.Add(new Tuple<TransportationType, Station>(key, station));
        }

        return result;
    }

    public IEnumerable<Station> GetStationNeighbours(TransportationType transport)
    {
        if (transport == TransportationType.Any)
        {
            foreach (var s in GetAllStationNeighbours())
                yield return s.B;
        }
        else
        {
            foreach (var key in stationNeighbours.Keys)
            {
                if (((transport & key) != key) || stationNeighbours[key].Count <= 0)
                    continue;

                foreach (Station s in stationNeighbours[key])
                    yield return s;
            }
        }
    }

    public override int GetHashCode()
    {
        return Id;
    }

    internal IEnumerable<TransportationType> GetAllTransportationsTo(Station to)
    {
        return stationNeighbours.Where((o) => o.Value.Contains(to)).Select((o) => o.Key);
    }

    public int ConsumeOrder
    {
        get { return 5000; }
    }

    public bool TryClick()
    {
        this.Broadcast(GameGuiEvents.StationClicked);
        return IsHighlighted;
    }

    public void SetHighlights(bool active)
    {
        if (active == IsHighlighted)
            return;

        highlightContainer.SetActive(active);
        UISprite spr = GetComponent<UISprite>();

        if(active)
        {
            spr.spriteName = this.spriteName + "_a";
        }
        else
        {
            spr.spriteName = this.spriteName;
        }

        isHighlighted = active;
    }

    public void SetFakeHighlight(bool active)
    {
        SetHighlights(active);
        isHighlighted = false;
    }


    #region EDITOR
#if UNITY_EDITOR
    public UILabel CreateLabel()
    {
        var lbl = NGUITools.AddWidget<UILabel>(this.gameObject);
        lbl.transform.localPosition = new Vector3(-5, 16, 0);
        lbl.text = Id.ToString();
        lbl.fontSize = 55;
        lbl.depth = 1;
        //lbl.trueTypeFont = GameSetup.Instance.StationFont;
        lbl.effectStyle = UILabel.Effect.Outline;
        lbl.effectDistance = new Vector2(2, 2);
        return lbl;
    }

    public GameObject CreateHighlights(UIAtlas atlasBack, UIAtlas atlasFront)
    {
        GameObject go = NGUITools.AddChild(this.gameObject);
        go.name = "highlight";
        UISprite sprBack = CreateHighlightSprite(atlasBack, go);
        UISprite sprFront = CreateHighlightSprite(atlasFront, go);

        sprBack.gameObject.name = "highlight_back";
        sprFront.gameObject.name = "highlight_front";

        sprBack.depth = -2;
        sprFront.depth = -1;

        GetTransportationColor(out Color colorBack, out Color colorFront);
        sprBack.color = colorBack;
        sprFront.color = colorFront;

        float scaleFront = 1.6f;
        float scaleBack = 2.3f;
        if (HasAnyTransportationOption(TransportationType.Metro))
        {
            scaleFront = 1.8f; // bigger for metro
            scaleBack = 2.7f;
        }
        sprFront.transform.localScale = new Vector3(scaleFront, scaleFront);
        sprBack.transform.localScale = new Vector3(scaleBack, scaleBack);

        go.transform.localScale = new Vector3(1, 0.6f);
        go.SetActive(false);

        return go;
    }
    UISprite CreateHighlightSprite(UIAtlas atlas, GameObject parent)
    {
        var spr = NGUITools.AddWidget<UISprite>(parent);
        spr.transform.localPosition = new Vector3(0, 0, 0);
        spr.atlas = atlas;
        spr.spriteName = "station_highlight";
        return spr;
    }

    void GetTransportationColor(out Color colorBack, out Color colorFront)
    {
        if (HasAnyTransportationOption(TransportationType.Metro))
        {
            colorFront = Helpers.ColorFromRGB(220, 220, 255); // white with a bit of blue
            colorBack = Helpers.ColorFromRGB(78, 135, 255); // bright blue
        }
        else if (HasAnyTransportationOption(TransportationType.Bus))
        {
            colorFront = Helpers.ColorFromRGB(255, 255, 255); // white
            colorBack = Helpers.ColorFromRGB(236, 45, 32); // blue
        }
        else // Taxi
        {
            colorFront = Helpers.ColorFromRGB(240, 195, 68);
            colorBack = Helpers.ColorFromRGB(255, 235, 4);
        }
    }

    void OnDrawGizmos()
    {
        Color col;
        if (GameState.Instance.PossibleMrXLocations.Contains(this))
        {
            col = Color.black;
        }
        else if (AiDetectiveSystem.Instance.forbiddenLocations.Contains(this))
        {
            col = Color.red;
        }
        else
        {
            GetTransportationColor(out Color colorBack, out Color colorFront);
            col = colorFront;
        }

        GUIStyle lblstl = new GUIStyle() { fontSize = 10, alignment = TextAnchor.UpperCenter, };
        lblstl.normal.textColor = col;

        UnityEditor.Handles.Label(transform.position + new Vector3(0, -0.05f, 0), Id.ToString(), lblstl);
        Gizmos.color = col;
        Gizmos.DrawSphere(transform.position, 0.05f);


        if (GameState.Instance.CurrentPlayer != null && !GameState.Instance.CurrentPlayer.ConnectionWeight[Id].Invalid)
        {
            if (GameState.Instance.CurrentPlayer.CurrentWayPlan != null && GameState.Instance.CurrentPlayer.CurrentWayPlan.Contains(this))
                col = Color.cyan;
            else
                col = Color.Lerp(Color.white, Color.red, (float)GameState.Instance.CurrentPlayer.ConnectionWeight[Id].WayWeightFromStart / 750f);

            Gizmos.color = col;
            Gizmos.DrawSphere(transform.position, 0.03f);
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.035f);
    }

#endif
    #endregion


    public void ClickStart()
    {
    }
}
