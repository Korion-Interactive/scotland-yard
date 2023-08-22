using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class BlurCam : MonoBehaviour
{
    //public static BlurCam Instance { get; private set; }

    public Camera RelatedCam;
    Camera cam;

    public GameObject BlurredUi, NonBlurredUi;
    public Transform ObjectToBeScaled;

    bool blurState;

    void Start()
    {
        //Instance = this;

        this.cam = this.GetComponent<Camera>();
        cam.enabled = false;
        cam.aspect = (float)Screen.width / Screen.height;

        BlurredUi.SetActive(false);
        NonBlurredUi.SetActive(true);
    }
    public void Blur()
    {
        SetBlurred(true);
    }
    public void Unblur()
    {
        SetBlurred(false);
    }

    public void SetBlurred(bool blurred)
    {
        if (blurState == blurred)
            return;

        if(blurred)
        {
            if(RelatedCam != null)
            {
                cam.transform.position = RelatedCam.transform.position;
                cam.orthographicSize = RelatedCam.orthographicSize;

                if (ObjectToBeScaled != null)
                    ObjectToBeScaled.localScale = new Vector3(cam.orthographicSize, cam.orthographicSize);
            }

            cam.enabled = true;
            cam.Render();
            cam.enabled = false;
        }

        BlurredUi.SetActive(blurred);
        NonBlurredUi.SetActive(!blurred);

        blurState = blurred;
    }

}