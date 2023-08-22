using FullInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class BaseBehaviour : BaseBehavior<FullSerializerSerializer>
{
    protected override void Awake()
    {
        try
        {
            base.Awake();
        }
        catch (Exception ex)
        {
            this.LogError(ex);
        }
    }
}