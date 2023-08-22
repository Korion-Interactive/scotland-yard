using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#region Base System : Singleton and SubSystems
public abstract class BaseSystem<TSystem> : BaseBehaviour
    where TSystem : BaseSystem<TSystem>
{
    protected virtual string expectedParentName { get { return "Systems"; } }

    protected static TSystem instance;
    public static TSystem Instance
    {
        get
        {
            if (instance == null)
            {
                var sys = GameObject.FindObjectOfType<TSystem>();
                if (sys != null)
                {
                    instance = sys;
                }
                else
                {
                    var go = new GameObject();

                    instance = go.AddComponent<TSystem>();
                    go.name = instance.GetClassName();

                    if (instance.expectedParentName != null)
                    {
                        var parent = GameObject.Find(instance.expectedParentName);
                        if (parent == null)
                            parent = new GameObject(instance.expectedParentName);

                        go.transform.parent = parent.transform;
                    }


                    instance.LogInfo("Instance created");
                }
            }
            return instance;
        }
    }

    public List<SubSystem<TSystem>> SubSystems = new List<SubSystem<TSystem>>();

    public T GetSubSystem<T>()
        where T : SubSystem<TSystem>
    {
        return SubSystems.FirstOrDefault((o) => o.GetType() == typeof(T)) as T;
    }

    protected override void Awake()
    {
        base.Awake();

        this.Assert(this is TSystem);

        if (instance == null)
            instance = this as TSystem;
    }

    protected virtual void OnEnable()
    {
        foreach(var sub in SubSystems)
        {
            sub.Enable();
        }
    }

    protected virtual void OnDestroy()
    {
        StopAllCoroutines();

        foreach (var s in SubSystems)
            s.OnDestroy();

        SubSystems.Clear();

        if (this == instance)
            instance = null;
    }
    protected abstract void RegisterEvents();



}
#endregion

public abstract class BaseSystem<TEvent, TSystem> : BaseSystem<TSystem>
    where TEvent : struct, IComparable, IConvertible, IFormattable // = enum
    where TSystem : BaseSystem<TSystem>
{
    SystemEventManager<TEvent> eventManager { get { return EventManagerRepository.Instance.GetSingleInstanceSystemEventManager<TEvent>(); } }

    //protected Dictionary<object, Action<BaseArgs>> registerdEvents = new Dictionary<object, Action<BaseArgs>>();

    #region start / destroy
    protected virtual void Start()
    {
        RegisterEvents();

        foreach(SubSystem<TSystem> sub in SubSystems)
        {
            sub.RegisterEvents();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        //RemoveEvents<TEvent>();
        //registerdEvents.Clear();
    }

    //protected void RemoveEvents<T>()
    //    where T : struct, IComparable, IConvertible, IFormattable // = enum
    //{
    //    var mgr = EventManagerRepository.Instance.GetSingleInstanceEventManager<T>();
    //    foreach (var key in registerdEvents.Keys)
    //    {
    //        if (key.GetType() == typeof(T))
    //            mgr.RemoveListener((T)key, registerdEvents[key]);
    //    }
    //}

    #endregion

    #region listen
    internal void ListenTo(TEvent evt, Action<BaseArgs> action)
    {
        //registerdEvents.Add(evt, action);
        eventManager.AddListener<BaseArgs>(evt, action, Globals.Listen_Normal);
    }

    internal void ListenTo(TEvent evt, Action<BaseArgs> action, int callOrderId)
    {
        eventManager.AddListener<BaseArgs>(evt, action, callOrderId);
    }

    internal void ListenTo<TArgs>(TEvent evt, Action<TArgs> action)
        where TArgs : BaseArgs
    {
        ListenTo<TArgs>(evt, action, Globals.Listen_Normal);
    }
    internal void ListenTo<TArgs>(TEvent evt, Action<TArgs> action, int callOrderId)
        where TArgs : BaseArgs
    {
        Action<BaseArgs> a = (args) => action(args as TArgs);

        //if (registerdEvents.ContainsKey(evt))
        //    registerdEvents[evt] += a;
        //else
        //    registerdEvents.Add(evt, a);

        eventManager.AddListener<BaseArgs>(evt, a, callOrderId);
    }

    #endregion

    #region broadcast
    internal void Broadcast(TEvent evt)
    {
        Broadcast(evt, this.gameObject); 
    }
    internal void Broadcast(TEvent evt, GameObject relatedObj)
    {
        eventManager.Broadcast<BaseArgs>(evt, new BaseArgs() { RelatedObject = relatedObj }); 
    }

    internal void Broadcast(TEvent evt, BaseArgs args)
    {
        Broadcast(evt, args.RelatedObject ?? this.gameObject, args); 
    }
    internal void Broadcast(TEvent evt, GameObject relatedObj, BaseArgs args)
    {
        args.RelatedObject = relatedObj;

        eventManager.Broadcast<BaseArgs>(evt, args);
    }

    #endregion

    #region broadcast delayed
    internal void BroadcastDelayed(TEvent evt, float delay)
    {
        BroadcastDelayed(evt, this.gameObject, delay);
    }
    internal void BroadcastDelayed(TEvent evt, GameObject relatedObj, float delay)
    {
        BroadcastDelayed(evt, relatedObj, new BaseArgs(), delay);
    }
    internal void BroadcastDelayed(TEvent evt, BaseArgs args, float delay)
    {
        BroadcastDelayed(evt, args.RelatedObject ?? this.gameObject, args, delay);
    }
    internal void BroadcastDelayed(TEvent evt, GameObject relatedObj, BaseArgs args, float delay)
    {
        StartCoroutine(coBroadcastDelayed(evt, relatedObj, args, delay));
    }

    private IEnumerator coBroadcastDelayed(TEvent evt, GameObject relatedObj, BaseArgs args, float delay)
    {
        yield return new WaitForSeconds(delay);
        Broadcast(evt, relatedObj, args);
    }
    #endregion

}

public abstract class BaseSystem<TEvent, TEventBase, TSystem> : BaseSystem<TEventBase, TSystem>
    where TEvent : struct, IComparable, IConvertible, IFormattable      // = enum
    where TEventBase : struct, IComparable, IConvertible, IFormattable  // = enum
    where TSystem : BaseSystem<TSystem>
{
    SystemEventManager<TEvent> eventManager { get { return EventManagerRepository.Instance.GetSingleInstanceSystemEventManager<TEvent>(); } }

    #region start / destroy
    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        //RemoveEvents<TEvent>();
    }
    #endregion

    #region listen
    internal void ListenTo(TEvent evt, Action<BaseArgs> action)
    {
        //registerdEvents.Add(evt, action);
        eventManager.AddListener<BaseArgs>(evt, action, Globals.Listen_Normal);
    }

    internal void ListenTo(TEvent evt, Action<BaseArgs> action, int callOrderId)
    {
        eventManager.AddListener<BaseArgs>(evt, action, callOrderId);
    }

    internal void ListenTo<TArgs>(TEvent evt, Action<TArgs> action)
        where TArgs : BaseArgs
    {
        ListenTo<TArgs>(evt, action, Globals.Listen_Normal);
    }
    internal void ListenTo<TArgs>(TEvent evt, Action<TArgs> action, int callOrderId)
        where TArgs : BaseArgs
    {
        Action<BaseArgs> a = (args) => action(args as TArgs);

        //if (registerdEvents.ContainsKey(evt))
        //    registerdEvents[evt] += a;
        //else
        //    registerdEvents.Add(evt, a);

        eventManager.AddListener<BaseArgs>(evt, a, callOrderId);
    }

    #endregion

    #region broadcast
    internal void Broadcast(TEvent evt)
    {
        Broadcast(evt, this.gameObject);
    }
    internal void Broadcast(TEvent evt, GameObject relatedObj)
    {
        eventManager.Broadcast<BaseArgs>(evt, new BaseArgs() { RelatedObject = relatedObj });
    }

    internal void Broadcast(TEvent evt, BaseArgs args)
    {
        Broadcast(evt, args.RelatedObject ?? this.gameObject, args);
    }
    internal void Broadcast(TEvent evt, GameObject relatedObj, BaseArgs args)
    {
        args.RelatedObject = relatedObj;

        eventManager.Broadcast<BaseArgs>(evt, args);
    }

    #endregion

    #region broadcast delayed
    internal void BroadcastDelayed(TEvent evt, float delay)
    {
        BroadcastDelayed(evt, this.gameObject, delay);
    }
    internal void BroadcastDelayed(TEvent evt, GameObject relatedObj, float delay)
    {
        BroadcastDelayed(evt, relatedObj, new BaseArgs(), delay);
    }
    internal void BroadcastDelayed(TEvent evt, BaseArgs args, float delay)
    {
        BroadcastDelayed(evt, args.RelatedObject ?? this.gameObject, args, delay);
    }
    internal void BroadcastDelayed(TEvent evt, GameObject relatedObj, BaseArgs args, float delay)
    {
        StartCoroutine(coBroadcastDelayed(evt, relatedObj, args, delay));
    }

    private IEnumerator coBroadcastDelayed(TEvent evt, GameObject relatedObj, BaseArgs args, float delay)
    {
        yield return new WaitForSeconds(delay);
        Broadcast(evt, relatedObj, args);
    }
    #endregion
}
public abstract class BaseSystem<TEvent, TEventSub, TEventBase, TSystem> : BaseSystem<TEventSub, TEventBase, TSystem>
    where TEvent : struct, IComparable, IConvertible, IFormattable       // = enum
    where TEventSub : struct, IComparable, IConvertible, IFormattable    // = enum
    where TEventBase : struct, IComparable, IConvertible, IFormattable   // = enum
    where TSystem : BaseSystem<TSystem>
{
    SystemEventManager<TEvent> eventManager { get { return EventManagerRepository.Instance.GetSingleInstanceSystemEventManager<TEvent>(); } }

    #region start / destroy
    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        //RemoveEvents<TEvent>();
    }
    #endregion

    #region listen
    internal void ListenTo(TEvent evt, Action<BaseArgs> action)
    {
        //registerdEvents.Add(evt, action);
        eventManager.AddListener<BaseArgs>(evt, action, Globals.Listen_Normal);
    }

    internal void ListenTo(TEvent evt, Action<BaseArgs> action, int callOrderId)
    {
        eventManager.AddListener<BaseArgs>(evt, action, callOrderId);
    }

    internal void ListenTo<TArgs>(TEvent evt, Action<TArgs> action)
        where TArgs : BaseArgs
    {
        ListenTo<TArgs>(evt, action, Globals.Listen_Normal);
    }
    internal void ListenTo<TArgs>(TEvent evt, Action<TArgs> action, int callOrderId)
        where TArgs : BaseArgs
    {
        Action<BaseArgs> a = (args) => action(args as TArgs);

        //if (registerdEvents.ContainsKey(evt))
        //    registerdEvents[evt] += a;
        //else
        //    registerdEvents.Add(evt, a);

        eventManager.AddListener<BaseArgs>(evt, a, callOrderId);
    }

    #endregion

    #region broadcast
    internal void Broadcast(TEvent evt)
    {
        Broadcast(evt, this.gameObject); 
    }
    internal void Broadcast(TEvent evt, GameObject relatedObj)
    {
        eventManager.Broadcast<BaseArgs>(evt, new BaseArgs() { RelatedObject = relatedObj }); 
    }

    internal void Broadcast(TEvent evt, BaseArgs args)
    {
        Broadcast(evt, args.RelatedObject ?? this.gameObject, args); 
    }
    internal void Broadcast(TEvent evt, GameObject relatedObj, BaseArgs args)
    {
        args.RelatedObject = relatedObj;

        eventManager.Broadcast<BaseArgs>(evt, args);
    }

    #endregion

    #region broadcast delayed
    internal void BroadcastDelayed(TEvent evt, float delay)
    {
        BroadcastDelayed(evt, this.gameObject, delay);
    }
    internal void BroadcastDelayed(TEvent evt, GameObject relatedObj, float delay)
    {
        BroadcastDelayed(evt, relatedObj, new BaseArgs(), delay);
    }
    internal void BroadcastDelayed(TEvent evt, BaseArgs args, float delay)
    {
        BroadcastDelayed(evt, args.RelatedObject ?? this.gameObject, args, delay);
    }
    internal void BroadcastDelayed(TEvent evt, GameObject relatedObj, BaseArgs args, float delay)
    {
        StartCoroutine(coBroadcastDelayed(evt, relatedObj, args, delay));
    }

    private IEnumerator coBroadcastDelayed(TEvent evt, GameObject relatedObj, BaseArgs args, float delay)
    {
        yield return new WaitForSeconds(delay);
        Broadcast(evt, relatedObj, args);
    }
    #endregion
}