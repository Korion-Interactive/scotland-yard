using Rewired;
using Rewired.Demos;
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

    public PlayerMouseSpriteExample _playerPointer;

    Vector2 startClickPosition;
    bool clickTracked;
    float clickTime;

    void Update()
    {
#if UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
        if(Input.GetMouseButtonDown(0))
#elif UNITY_SWITCH
        if (Input.GetMouseButtonDown(0) || _playerPointer.Mouse.leftButton.justPressed)
#else
        if (_playerPointer.Mouse.leftButton.justPressed)
#endif
        {
            List<IClickConsumable> clickables = FindClickables();
            foreach(IClickConsumable c in clickables)
                c.ClickStart();
#if UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
            startClickPosition = Input.mousePosition;
#elif UNITY_SWITCH
            startClickPosition = Input.mousePosition;
            _playerPointer.Mouse.screenPosition = startClickPosition;
#else
            startClickPosition = _playerPointer.Mouse.screenPosition;
#endif
            clickTracked = true;
            clickTime = 0;
        }

#if UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
        if(Input.GetMouseButton(0) && clickTracked)
#elif UNITY_SWITCH
        if ((_playerPointer.Mouse.leftButton.value == true || Input.GetMouseButton(0)) && clickTracked)
#else
        if(_playerPointer.Mouse.leftButton.value == true && clickTracked)
#endif
        {
            clickTime += Time.deltaTime;
#if UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
            float distSq = (startClickPosition - (Vector2)Input.mousePosition).sqrMagnitude;
#elif UNITY_SWITCH
            float distSq = (startClickPosition - (Vector2)Input.mousePosition).sqrMagnitude;
            distSq += (startClickPosition - (Vector2)_playerPointer.Mouse.screenPosition).sqrMagnitude;
#else
            float distSq = (startClickPosition - (Vector2)_playerPointer.Mouse.screenPosition).sqrMagnitude;
#endif

            clickTracked = clickTime <= MAX_CLICK_TIME && distSq <= MAX_CLICK_DISTANCE_SQ;
        }

#if UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
        if (Input.GetMouseButtonUp(0))
#elif UNITY_SWITCH
        if (Input.GetMouseButtonUp(0) || _playerPointer.Mouse.leftButton.justReleased)
#else
        if (_playerPointer.Mouse.leftButton.justReleased)
#endif
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

                    UICamera.selectedObject = null;
                }
            }

            this.Broadcast(GameGuiEvents.MouseUp);
        }

        if (_playerPointer.Mouse.rightButton.justPressed)
        {
            foreach (GameObject obj in ObjectsToDisableOnClickAnywhere)
                obj.SetActive(false);

            this.Broadcast(GameGuiEvents.ClickedAnywhere);

            UICamera.selectedObject = null;
        }
    }

    public void ClickedAnywhere()
    {
        foreach (GameObject obj in ObjectsToDisableOnClickAnywhere)
            obj.SetActive(false);

        this.Broadcast(GameGuiEvents.ClickedAnywhere);

        UICamera.selectedObject = null;
    }


    private List<IClickConsumable> FindClickables()
    {
        // 1. collect all clickables
        List<IClickConsumable> clickables = new List<IClickConsumable>();
        foreach (Camera cam in Cameras)
        {
#if UNITY_SWITCH || UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID
            Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 pos2 = cam.ScreenToWorldPoint(_playerPointer.Mouse.screenPosition);
#else
            Vector3 pos = cam.ScreenToWorldPoint(_playerPointer.Mouse.screenPosition);
#endif
            pos = cam.ScreenToWorldPoint(_playerPointer.Mouse.screenPosition);
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