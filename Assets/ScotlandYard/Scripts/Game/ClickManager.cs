using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ClickManager : MonoBehaviour
{
    const float MAX_CLICK_DISTANCE_SQ = 30*30;
    const float MAX_CLICK_TIME = 0.5f;

    public List<Camera> Cameras = new List<Camera>();
    public List<GameObject> ObjectsToDisableOnClickAnywhere = new List<GameObject>();

    Vector2 startClickPosition;
    bool clickTracked;
    float clickTime;

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            List<IClickConsumable> clickables = FindClickables();
            foreach(IClickConsumable c in clickables)
                c.ClickStart();

            startClickPosition = Input.mousePosition;
            clickTracked = true;
            clickTime = 0;
        }

        if(Input.GetMouseButton(0) && clickTracked)
        {
            clickTime += Time.deltaTime;
            float distSq = (startClickPosition - (Vector2)Input.mousePosition).sqrMagnitude;

            clickTracked = clickTime <= MAX_CLICK_TIME && distSq <= MAX_CLICK_DISTANCE_SQ;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (clickTracked)
            {
                List<IClickConsumable> clickables = FindClickables();

                // 3. Call Click Method(s)
                bool anythingClicked = false;
                foreach (IClickConsumable clickable in clickables)
                {
                    if (clickable.TryClick())
                    {
                        anythingClicked = true;
                        break;
                    }
                }

                // if nothing was successfully clicked: perform click anywhere actions
                if (!anythingClicked)
                {
                    foreach (GameObject obj in ObjectsToDisableOnClickAnywhere)
                        obj.SetActive(false);

                    this.Broadcast(GameGuiEvents.ClickedAnywhere);
                }
            }

            this.Broadcast(GameGuiEvents.MouseUp);
        }
    }


    private List<IClickConsumable> FindClickables()
    {
        // 1. collect all clickables
        List<IClickConsumable> clickables = new List<IClickConsumable>();
        foreach (Camera cam in Cameras)
        {
            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            pos.z = cam.transform.position.z;

            RaycastHit[] hits = Physics.RaycastAll(pos, Vector3.forward, float.PositiveInfinity, cam.cullingMask);
            foreach (RaycastHit hit in hits)
                clickables.AddRange(hit.collider.gameObject.GetComponentsWithInterface<IClickConsumable>());

            Collider2D[] hits2D = Physics2D.OverlapPointAll(pos, cam.cullingMask);
            foreach (Collider2D col in hits2D)
                clickables.AddRange(col.GetComponent<Collider2D>().gameObject.GetComponentWithInterface<IClickConsumable>());
        }

        // 2. Sort clickables
        clickables.Sort((a, b) => a.ConsumeOrder.CompareTo(b.ConsumeOrder));

        return clickables;
    }

}