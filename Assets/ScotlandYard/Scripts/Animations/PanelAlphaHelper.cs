using UnityEngine;

[RequireComponent(typeof(UIPanel))]
public class PanelAlphaHelper : MonoBehaviour
{
    [Range(0,1)] public float alpha;
    
    public void Awake()
    {
        Panel = gameObject.GetComponent<UIPanel>();
    }

    void Update()
    {
        Panel.alpha = alpha;
    }

    private UIPanel Panel { get; set; }
}
