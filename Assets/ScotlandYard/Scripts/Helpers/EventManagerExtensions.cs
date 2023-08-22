using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class EventManagerExtensions
{
    private static SystemEventManager<TEvent> GetEventManager<TEvent>()
        where TEvent : struct, IComparable, IConvertible, IFormattable
    {
        return EventManagerRepository.Instance.GetSingleInstanceSystemEventManager<TEvent>();
    }

    // LISTEN EVENTS
    //
    // should not been used by any random game object. 
    // Listen to events only with Systems!
    //
   
    public static void ListenTo<TEvent>(this MonoBehaviour self, TEvent evt, Action<BaseArgs> action)
        where TEvent : struct, IComparable, IConvertible, IFormattable
    {
        GetEventManager<TEvent>().AddListener<BaseArgs>(evt, action, Globals.Listen_Normal);
    }
    public static void ListenTo<TEvent, TArgs>(this MonoBehaviour self, TEvent evt, Action<TArgs> action)
        where TEvent : struct, IComparable, IConvertible, IFormattable
        where TArgs : BaseArgs
    {
        GetEventManager<TEvent>().AddListener<TArgs>(evt, action, Globals.Listen_Normal);
    }


    // BROADCAST EVENTS
    public static void Broadcast<TEvent>(this MonoBehaviour self, TEvent evt)
        where TEvent : struct, IComparable, IConvertible, IFormattable
    {
        self.Broadcast(evt, self.gameObject); 
    }
    public static void Broadcast<TEvent>(this MonoBehaviour self, TEvent evt, GameObject relatedObj)
        where TEvent : struct, IComparable, IConvertible, IFormattable
    {
        GetEventManager<TEvent>().Broadcast<BaseArgs>(evt, new BaseArgs() { RelatedObject = relatedObj }); 
    }

    public static void Broadcast<TEvent>(this MonoBehaviour self, TEvent evt, BaseArgs args)
        where TEvent : struct, IComparable, IConvertible, IFormattable
    {
        self.Broadcast<TEvent>(evt, args.RelatedObject ?? self.gameObject, args); 
    }
    public static void Broadcast<TEvent>(this object self, TEvent evt, GameObject relatedObj, BaseArgs args)
        where TEvent : struct, IComparable, IConvertible, IFormattable
    {
        args.RelatedObject = relatedObj;
        GetEventManager<TEvent>().Broadcast(evt, args);
    }


    public static void BroadcastDelayed<TEvent>(this MonoBehaviour self, TEvent evt, float delay)
        where TEvent : struct, IComparable, IConvertible, IFormattable
    {
        self.BroadcastDelayed<TEvent>(evt, self.gameObject, delay);
    }
    public static void BroadcastDelayed<TEvent>(this MonoBehaviour self, TEvent evt, GameObject relatedObj, float delay)
        where TEvent : struct, IComparable, IConvertible, IFormattable
    {
        self.BroadcastDelayed<TEvent>(evt, relatedObj, new BaseArgs(), delay);
    }

    public static void BroadcastDelayed<TEvent>(this MonoBehaviour self, TEvent evt, BaseArgs args, float delay)
        where TEvent : struct, IComparable, IConvertible, IFormattable
    {
        self.BroadcastDelayed<TEvent>(evt, args.RelatedObject ?? self.gameObject, args, delay);
    }
    public static void BroadcastDelayed<TEvent>(this MonoBehaviour self, TEvent evt, GameObject relatedObj, BaseArgs args, float delay)
        where TEvent : struct, IComparable, IConvertible, IFormattable
    {
        self.StartCoroutine(coBroadcastDelayed<TEvent>(self, evt, relatedObj, args, delay));
    }


    private static IEnumerator coBroadcastDelayed<TEvent>(MonoBehaviour self, TEvent evt, GameObject relatedObj, BaseArgs args, float delay)
        where TEvent : struct, IComparable, IConvertible, IFormattable
    {
        yield return new WaitForSeconds(delay);
        self.Broadcast<TEvent>(evt, relatedObj, args);
    }
}