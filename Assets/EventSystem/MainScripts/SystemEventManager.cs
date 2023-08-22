 
//#define LOG_ALL_MESSAGES
//#define LOG_ADD_LISTENER
#define LOG_BROADCAST_MESSAGE

//Uncomment this if you want A broadcast method call to produce an error if there are no listeners
//#define REQUIRE_LISTENER

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;


//We're trying to constrain the generic to an enum, but that is not possible in .Net currently.
//We will settle for using some constraints particular to an enum
public class SystemEventManager<TEventType> : EventManager 
    where TEventType : struct, IComparable, IConvertible, IFormattable // enum
{

	private int numOfEventsOfThisType;
	private Delegate[] eventHandlers;
    private List<int>[] eventHandlerOrder;
	
	//Function that allows us to convert an enum to int.
	//It's necessary because you can't cast a generic type to an int normally.
	//Convert.ToInt32(enum) and other methods work but they are a lot slower
	//than this dynamically created function.
	private Func<TEventType, int> ConvertEnumToInt;
		
	//Message handlers that should never be removed, regardless of calling RemoveInterimEvents
	private HashSet<int> persistentEvents;
	
	private bool saveAllEvents;
	private bool hasPersistentEvents;
	
	//Implementation of the base class, allows the EventManagerRepository to check whether
	//this object should be destroyed
	public override bool SaveAllEvents		{ get { return saveAllEvents; } set { saveAllEvents = value; } }
	public override bool HasPersistentEvents		{ get { return hasPersistentEvents; } }
	
	
	
	
	//Used to initialize our static variables, called the first time a method/variable is accessed
	public SystemEventManager()
	{
		if(!typeof(TEventType).IsEnum) 
			throw new InvalidGenericTypeException("TEventType for generic class 'Event Manager' must be an enum!");

		hasPersistentEvents = false;
		saveAllEvents = false;
		persistentEvents = new HashSet<int>();
        ConvertEnumToInt = (o) => o.ToInt32(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);//EnumConverterCreator.ConvertToInt32<TEventType>();//.CreateFromEnumConverter<TEventType, int>();
		
		//Get the max number of events
		numOfEventsOfThisType = Enum.GetNames (typeof(TEventType)).Length;
		
		//Create the delegate array to hold the events -- they are all initialized to null to begin with
		eventHandlers = new Delegate[numOfEventsOfThisType];
        eventHandlerOrder = new List<int>[numOfEventsOfThisType];
	}
	
	
	
	
	//Marks a certain message as permanent.
	public void MarkEventAsPersistent(TEventType eventName) 
	{
		int eventID = ConvertEnumToInt(eventName); 
		#if LOG_ALL_MESSAGES
		Debug.Log("GlobalEventManager MarkAsPermanent \t\"" + eventID + "\"");
		#endif
		
 		//Add the eventID to the hashset of persistent events. Duplicates will be ignored
		//Set hasPersistentEvents to true if the ID isn't a duplicate.
		if(persistentEvents.Add(eventID)) hasPersistentEvents = true;
	}
	
	//Removes event from the list of persistent events, which effectively 'marks' it as interim.
	public void MarkEventAsInterim(TEventType eventName)
	{
		int eventID = ConvertEnumToInt(eventName);
		
		//Tries to remove the event from the persistentEvents HashSet. If the event
		//is successfully removed and the hashset is empty after removal, set hasPersistentEvents to false.
		if(persistentEvents.Remove(eventID) && persistentEvents.Count == 0)
			hasPersistentEvents = false;
	}
 
 
	public override void RemoveInterimEvents()
	{
		#if LOG_ALL_MESSAGES
		Debug.Log("MESSENGER RemoveInterimEvents. Make sure that none of necessary listeners are removed.");
		#endif
 		
		if(saveAllEvents) //If save all events is set to true, do not remove interimEvents.
			return;
		
		//A new delegate array that will hold only those eventHandlers marked as permanent
 		Delegate[] tempGlobalEventHolder = new Delegate[numOfEventsOfThisType];
 
		//For every id stored in the list of persistentEvents, use that id to place a copy of the delegate stored in the current events in the tempGlobalEventHolder
		foreach (int eventID in persistentEvents) { tempGlobalEventHolder[eventID] = eventHandlers[eventID]; }

		//Replace the eventHandlers array with the temp one we just created
		eventHandlers = tempGlobalEventHolder;
	}
 
	public void PrintGlobalEventTable()
	{
		Debug.Log("MESSENGER PrintGlobalEventsTable ===");
 		Debug.Log("\n");
		
		for(int i = 0; i < eventHandlers.Length; i++) 
			Debug.Log("Event Name: " + (TEventType)(object)i + "  ||  Delegate Type: " + eventHandlers[i]);
		
		Debug.Log("\n");
	}
 	
	
	
	//Listener stuff
	
	
	//Called before a Listener is added. Used primarily to make sure the delegate supplied by the listener matches the delegate
	//in the array. If the delegate in the array at index 'eventID' is null or is of the same type as the listeners delegate, we
	//can add the listeners delegate. Otherwise an exception is thrown.
    private void OnListenerAdding(int eventID, Delegate listenerBeingAdded, int orderID) 
	{
		#if LOG_ALL_MESSAGES || LOG_ADD_LISTENER
        string name = Enum.GetName(typeof(TEventType), eventID);
		Debug.Log("MESSENGER OnListenerAdding \t\"" + name + "\"\t{" + listenerBeingAdded.Target + " -> " + listenerBeingAdded.Method + "}");
		#endif
  
        Delegate tempDel = eventHandlers[eventID];
        if (tempDel != null && tempDel.GetType() != listenerBeingAdded.GetType()) 
		{
            throw new ListenerException(string.Format("Attempting to add listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being added has type {2}", (TEventType)(object)eventID, tempDel.GetType().Name, listenerBeingAdded.GetType().Name));
        }

        if (eventHandlerOrder[eventID] == null)
            eventHandlerOrder[eventID] = new List<int>();

        eventHandlerOrder[eventID].Add(orderID);
    }
 
	//Called from RemoveListener (when a listener request to be removed).
	//Primarily used to make sure the event the listener is trying to get removed from exist in the events array
    private void OnListenerRemoving(int eventID, Delegate listenerBeingRemoved) 
	{
		#if LOG_ALL_MESSAGES
		Debug.Log("MESSENGER OnListenerRemoving \t\"" + (TEventType)(object)eventID + "\"\t{" + listenerBeingRemoved.Target + " -> " + listenerBeingRemoved.Method + "}");
		#endif
       
        Delegate tempDel = eventHandlers[eventID];
        if (tempDel == null) 
		{
            throw new ListenerException(string.Format("Attempting to remove listener for event type \"{0}\" but current listener is null.", (TEventType)(object)eventID));
        } 
		else if (tempDel.GetType() != listenerBeingRemoved.GetType()) 
		{
            throw new ListenerException(string.Format("Attempting to remove listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being removed has type {2}", (TEventType)(object)eventID, tempDel.GetType().Name, listenerBeingRemoved.GetType().Name));
        }


        int index = eventHandlers[eventID].GetInvocationList().ToList().IndexOf(listenerBeingRemoved);
        eventHandlerOrder[eventID].RemoveAt(index);
    }
 
    
 
	
	
	//Single parameter
	public void AddListener<T>(TEventType eventName, Action<T> handler, int orderID) 
	{
		int eventID = ConvertEnumToInt(eventName);
        OnListenerAdding(eventID, handler, orderID);
        eventHandlers[eventID] = (Action<T>)eventHandlers[eventID] + handler;

        // ORDERING
        // Re-order events
        int cnt = eventHandlerOrder[eventID].Count;
        if(cnt > 1 && orderID < eventHandlerOrder[eventID][cnt - 2])
        {
            // find index to insert
            int i;
            for(i = 0; i < eventHandlerOrder[eventID].Count -1; i++)
            {
                if (eventHandlerOrder[eventID][i] > orderID)
                    break;
            }

            // sort order and doublecheck
            eventHandlerOrder[eventID].Sort();
            this.Assert(eventHandlerOrder[eventID][i] == orderID);

            // Re-init invocation list
            List<Action<T>> invocationList = eventHandlers[eventID].GetInvocationList().OfType<Action<T>>().ToList();
            eventHandlers[eventID] = null;

            invocationList.Remove(handler);
            invocationList.Insert(i, handler);

            foreach(Action<T> a in invocationList)
            {
                eventHandlers[eventID] = (Action<T>)eventHandlers[eventID] + a;
            }

            //Log.debug(this, string.Format("Invocation List for event {0} reordered.", eventName));
        }

        //Log.debug(this, string.Format("Count of Invocation List for event {0}: {1}", eventName, eventHandlers[eventID].GetInvocationList().Count()));
    }
 
	
 
	//Single parameter
	public void RemoveListener<T>(TEventType eventName, Action<T> handler) 
	{

		int eventID = ConvertEnumToInt(eventName);
        OnListenerRemoving(eventID, handler);
        eventHandlers[eventID] = (Action<T>)eventHandlers[eventID] - handler;
    }
 
 
	#if REQUIRE_LISTENER
    public void OnBroadcasting(int eventID) 
	{
        if (eventHandlers[eventID] == null) 
		{
            throw new BroadcastException(string.Format("Broadcasting message \"{0}\" but no listener found. Try marking the message with GlobalEventManager.MarkAsPermanent.", (TEventType)(object)eventID));
        }
    }
	#endif
	
    private static BroadcastException CreateBroadcastSignatureException(int eventID) 
	{
        return new BroadcastException(string.Format("Broadcasting message for event \"{0}\" but listeners have a different signature than the broadcaster.", (TEventType)(object)eventID));
    }
	
	
	
	
	//Regular Broadcast
	
 
	//Single parameter
    public void Broadcast<T>(TEventType eventName, T args) 
        where T : BaseArgs
	{
		int eventID = ConvertEnumToInt(eventName);

		#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        this.LogInfo(string.Format("\t{0} \t<color=#777700> {1} </color> \t<color=#5555AA> {2} </color> \t {3}",
            System.DateTime.Now.ToString("hh:mm:ss.fff"), eventName, args, args.RelatedObject));
		#endif
        
		#if REQUIRE_LISTENER
        OnBroadcasting(eventID);
		#endif
 
        Delegate tempDel = eventHandlers[eventID];
       	if (tempDel != null)
		{
            Action<T> broadcastEvent = tempDel as Action<T>;
 
            if (broadcastEvent != null) { broadcastEvent(args); } 
			else { throw CreateBroadcastSignatureException(eventID); }
        }
        
	}
 
	
	
	//Delayed Broadcast (using coroutines -- these must be called like so from a MonoBehaviour script : 
	//StartCoroutine(GlobalEventManager(TEventType.DoSomething, new WaitForSeconds(5f));
	
	
 
	//Single parameter
    public IEnumerator Broadcast<T>(TEventType eventName, T arg1, YieldInstruction[] delayInstrunctions) 
	{
		int eventID = ConvertEnumToInt(eventName);
		#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventName + "\"");
		#endif
        
		#if REQUIRE_LISTENER
        OnBroadcasting(eventID);
		#endif
 
        Delegate tempDel = eventHandlers[eventID];
       	if (tempDel != null)
		{
            Action<T> broadcastEvent = tempDel as Action<T>;
 
            if (broadcastEvent != null) 
			{ 
				foreach(YieldInstruction instruction in delayInstrunctions)
					yield return instruction;
				broadcastEvent(arg1); 
			} 
			else { throw CreateBroadcastSignatureException(eventID); }
        }
	}
 
}