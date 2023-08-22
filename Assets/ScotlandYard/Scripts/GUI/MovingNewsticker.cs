using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MovingNewsticker : MonoBehaviour
{
    public float SpeedPerSecond;
    public float NewsWidth;
 
    void Update()
    {
        this.transform.localPosition += new Vector3(SpeedPerSecond * Time.deltaTime, 0, 0);

        if (transform.localPosition.x <= -NewsWidth)
            transform.localPosition += new Vector3(NewsWidth, 0, 0);
        else if (transform.localPosition.x >= NewsWidth)
            transform.localPosition -= new Vector3(NewsWidth, 0, 0);
    }

}