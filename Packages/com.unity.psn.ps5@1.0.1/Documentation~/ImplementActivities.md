# Implement Activities in your Unity Project

To implement and interact with Activities in C#, you must use UDS Events since Activities are a part of the UDS service.

You can send UDS events to:
* Start a given Activity (`activityStart`).
* Resume a given Activity (`activityResume`).
* Stop a given Activity (`activityEnd`).
* Make Activities available or unavailable (`activityAvailabilityChange`)
* Change the priority of Activities (the order in which they appear on screen). (`activityPriorityChange`)
* Terminate all Activities (`activityTerminate`).

To find the full list of available UDS Events, see [Universal Data System documentation](https://p.siedev.net/resources/documents/SDK/latest/UniversalDataSystem-Overview/0011.html) on DevNet. You can also create your own custom UDS Events in the [UDS Management Tool](https://tools.partners.playstation.net/uds/app/uds-management/dashboard?sid=26&pid=psn).

## Initialization

In order to send UDS Events in the PSN package, you will need to:
1. Initialize the PSN.PS5.Main class using `Unity.PSN.PS5.Main.Initialize()`.
2. Initialize the UDS library using `UniversalDataSystem.StartSystemRequest`.

To intialise the package and UDS, see [Universal Data System](UniversalDataSystem.md).

## Creating UDS Events

In the PSN package, UDS events can be configured using the UDSEvent class. This class is designed to handle UDS events with multiple properties of different data types, hich is why the class can be used for PS5 system UDS events and custom UDS events.

The steps for creating and sending a UDS event are:
1. Declare a UDSEvent object.
2. Call the object’s member function Create() with the name of the UDS event you wish to send.
3. Add any properties required for that UDS event.
4. Declare a PostEventRequest object with the ID of the PS5’s active user and the UDSEvent data.
5. Create an AsyncRequest object with the PostEventRequest class as a parameter.
6. Schedule the AsyncRequest.
7. Inspect the AsyncRequest once it is complete to determine if the UDS event was sent successfully. <br/> **Note**: this does not mean the event was successful in UDS, just that it was sent.


Below is an example of how you can create and structure a UDS event using the `UDSEvent` class.

```
// Declare a UDSEvent object
UniversalDataSystem.UDSEvent myEvent = new UniversalDataSystem.UDSEvent();

// Call Create() with the name of the UDS event you’re calling to (activityStart, activityEnd etc.)
myEvent.Create("activityEventName");

// Add whatever properties are required for that event
// This can be done by calling myEvent.Properties.Set()
// ...
// ...

// When your event is ready, you need to make a PostEventRequest to send it
// You also need to attach the active user’s PSN user ID
// The PSN package sample project shows how you can do this
UniversalDataSystem.PostEventRequest postEvent = new UniversalDataSystem.PostEventRequest
{
UserId = GamePad.activeGamePad.loggedInUser.userId,
EventData = myEvent
};

// Create the AsyncRequest operation for scheduling
var asyncOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(postEvent);

UniversalDataSystem.Schedule(asyncOp);
```

## Examples of UDS Events

### activityTerminate

The `activityTerminate` event requires no additional properties. It resets all the user’s Activities to their default state.

```
public IEnumerator TerminateAllActivities()
{
UniversalDataSystem.UDSEvent activityTerminateEvent = new UniversalDataSystem.UDSEvent();
activityTerminateEvent.Create("activityTerminate");

// You do not need to add anything here, as there are no properties!

	// Create a PostEventRequest with the UDSEvent data
UniversalDataSystem.PostEventRequest postEvent = new UniversalDataSystem.PostEventRequest
{
UserId = GamePad.activeGamePad.loggedInUser.userId,
EventData = activityTerminateEvent
};

// Create the AsyncRequest operation for scheduling
var asyncOp = new AsyncRequest<UniversalDataSystem.PostEventRequest>(postEvent);

UniversalDataSystem.Schedule(asyncOp);

yield return new WaitUntil(() => asyncOp.IsSequenceCompleted);

// AsyncRequest is complete here
// CheckAsyncRequestOK() is a function that checks if the returned Request is null/fail/success
if (!NpManager.CheckAsyncRequestOK(startOp))
    {
        throw new System.Exception(“Failed to terminate Activities!");
}
}
```

### activityEnd

The `activityEnd` event has one required property - the ID of the Activity to end. This can be added by calling `UDSEvent.Properties.Set()` with the name of the property, and the relevant data.

```
public IEnumerator ActivityEnd(string activityID)
{
UniversalDataSystem.UDSEvent activityEndEvent = new UniversalDataSystem.UDSEvent();

activityEndEvent.Create("activityEnd");

activityEndEvent.Properties.Set("activityId", activityID);

// …
// PostEventRequest, AsyncRequest<>, etc
}
```

### activityAvailabilityChange

The `activityAvailabilityChange` event requires properties which are string arrays; either a list of the Activities to be made available, a list of Activities to be made unavailable, or both.

This is done by using the `EventPropertyArray` class. The class allows you to declare a data type for your array and copy in the values you want in the property.

In the example below, the values in the string array `myActivitiesList` are copied into the EventPropertyArray class `availableActivities`. This can then be set as a property using `UDSEvent.Properties.Set()`.

```
public IEnumerator MakeActivitiesAvailable()
{
	string[] myActivitiesList = {“activity-1”, “activity-2”, “activity-3”};

UniversalDataSystem.UDSEvent changeAvailabilityEvent = new UniversalDataSystem.UDSEvent();
changeAvailabilityEvent.Create("activityAvailabilityChange");

// To set an array as a property of the UDSEvent class, you need to use an EventPropertyArray
UniversalDataSystem.EventPropertyArray availableActivities = new
UniversalDataSystem.EventPropertyArray(UniversalDataSystem.PropertyType.String);

availableActivities.CopyValues(myActivitiesList);

changeAvailabilityEvent.Properties.Set("availableActivities", 
availableActivities);
changeAvailabilityEvent.Properties.Set("mode", "full");

// …
// PostEventRequest, AsyncRequest<>, etc!

}
```

## Tasks and Sub-Tasks

If you have created an Activity that has child Activities such as Tasks and Sub-Tasks, no extra code is needed to manage those Activities. They can be treated in the same way as standalone Activities by calling UDS events like `activityStart` and `activityEnd`.
