using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public abstract class SubSystem<TSystem> 
    where TSystem : BaseSystem<TSystem>
{
    public TSystem System { get { return BaseSystem<TSystem>.Instance; } }
    protected abstract bool needsUpdate { get; }

    internal void Enable()
    {
        if(needsUpdate)
        {
            System.StartCoroutine(CoroutineUpdate());
        }
    }
    internal abstract void RegisterEvents();

    IEnumerator CoroutineUpdate()
    {
        this.LogInfo("CoUpdate Start");
        while (System.enabled)
        {
            Update();
            yield return new WaitForEndOfFrame();
        }
        this.LogInfo("CoUpdate End");

    }

    protected virtual void Update() { this.LogWarn("UPDATE: called in base class (BaseSystem<TSystem>). This is not needed."); }

    internal virtual void OnDestroy()
    {
        System.StopCoroutine(CoroutineUpdate());
    }
}