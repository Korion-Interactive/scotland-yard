using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RotatingLoadingIndicator : MonoBehaviour
{
    public int Steps = 0;
    public float Speed = 1f;
    //float currentAngle = 0;

    void Start()
    {
        StartCoroutine(Rotate());
    }

    public IEnumerator Rotate()
    {

        while (this.enabled)
        {
            //currentAngle += rotationStep;

            if (Steps > 0)
            {
                float angle = 360f / Steps;
                this.transform.Rotate(Vector3.forward, angle, Space.Self);

                float waitTime = Speed / Steps;
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                float angle = 360 * (Time.deltaTime / Speed);
                this.transform.Rotate(Vector3.forward, angle, Space.Self);

                yield return new WaitForEndOfFrame();
            }

        }
    }
}