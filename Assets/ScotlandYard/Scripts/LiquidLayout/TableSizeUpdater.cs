using UnityEngine;

/// <summary>
/// Liquid layout component that is meant to change de width of the game tables collider according
/// to the Screen width. This will prevent the Camera from jerking back and forth on very wide devices.
/// </summary>
public class TableSizeUpdater : MonoBehaviour
{
    public void Awake()
    {
        TableCollider = gameObject.GetComponent<BoxCollider>();
        OriginalWidth = TableCollider.size.x;
    }

#if UNITY_IOS || UNITY_ANDRTOID
    public void Update()
    {
        float aspect = (float)Screen.width / (float)Screen.height;

        TableCollider.size = new Vector3(
            TableCollider.size.y * aspect,
            TableCollider.size.y,
            TableCollider.size.z);
    }
#endif
    private BoxCollider TableCollider { get; set; }
    private float OriginalWidth { get; set; }
}
