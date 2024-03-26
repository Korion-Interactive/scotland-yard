# Interact with Trophies in Unity

To use Trophies in Unity, you must use the Universal Data System (UDS) to send UDS events that update or unlock Trophies.

## Add the `npconfig.zip` file to your project

To set up your Trophy data, you need to share the UDS configuration files and Trophy configuration files with your application. To access these files, you must specify a npconfig.zip file. For more information about downloading the required `npconfig.zip` file for your project, see [Add an npconfig.zip file to your Unity project](Addnpconfig.md). 

**Note**: The PlayStation Network package sample project includes a sample `npconfig.zip` file that contains sample trophy and UDS data for you to test. However, if you want your own trophy names and values, then you need a custom npconfig.zip file.

**Warning**: Your Unity PS5 application crashes if you try to use your own npconfig.zip file in the PS5 Player Settings - without having the Universal Data System (UDS) service enabled - and then attempt to call PlayStation Network functions. You can still use the PlayStation Network package example project including the default config files. To enable the UDS service, see [Universal Data System](UniversalDataSystem.md).



## Unlock and update Trophies from code

After configuring your Trophies and adding the npconfig.zip file to your project, you can use PSN package functions to unlock or update a Trophy for a given user.

Before unlocking or updating Trophies, you must:
* Initialize the PSN package Main class using Unity.PSN.PS5.Main.Initialize()
* Initialize UDS using UniversalDataSystem.StartSystemRequest

To intialise the package and UDS, see [Universal Data System](UniversalDataSystem.md).

Once the PSN package and UDS are initialized, you can interact with Trophies using UDS events. 

### Non-progressive Trophies

To unlock a Trophy immediately (i.e. a Binary Trophy), you can use the `UniversalDataSystem.UnlockTrophyRequest` class. This sends the generated UDS event “`_UnlockTrophy`”.

```
public static void UnlockTrophy(int trophyID)
{
// Declare an UnlockTrophyRequest object
     	UniversalDataSystem.UnlockTrophyRequest unlockRequest = new UniversalDataSystem.UnlockTrophyRequest();

// Add the Trophy ID and active user ID to the object
unlockRequest.TrophyId = trophyID;
unlockRequest.UserId = GamePad.activeGamePad.loggedInUser.userId;

// Create an AsyncRequest with the UnlockTrophyRequest object
var asyncRequest = new AsyncRequest<UniversalDataSystem.UnlockTrophyRequest>(unlockRequest).ContinueWith((antecedent) =>
{
// This code is executed when the Request is complete
});

UniversalDataSystem.Schedule(asyncRequest);
}
```

### Progressive Trophies

To directly affect the progress of a Progressive Trophy, you can use the `UniversalDataSystem.UpdateTrophyProgressRequest` class. This sends the generated UDS event “`_UpdateTrophyProgress`”.

```
public void UnlockProgressTrophy(int trophyId, long progressValue)
{
UniversalDataSystem.UpdateTrophyProgressRequest progressRequest = new UniversalDataSystem.UpdateTrophyProgressRequest();

progressRequest.TrophyId = trophyId;
progressRequest.UserId = GamePad.activeGamePad.loggedInUser.userId;
progressRequest.Progress = progressValue;

var asyncRequest = new AsyncRequest<UniversalDataSystem.UpdateTrophyProgressRequest>(progressRequest).ContinueWith((antecedent) =>
{
// This code is executed when the Request is complete
});

UniversalDataSystem.Schedule(getTrophyOp);
}
```

### Trophies using custom UDS Stats and UDS Events

If you have a Trophy that is dependent on a custom UDS Stat and UDS Event, you can configure any type of UDS Event using the UDSEvent class in the PlayStation Network package. This is then sent using `UniversalDataSystem.PostEventRequest`.

In the example below, the UDS Event ‘`updateCoins`’ is sent, which takes the integer property ‘`coinsCollected`’. 

```
public static void PostUpdateCoinEvent(int currentCoins)
{
 // Declare a UDSEvent object, and call .Create() with the name of your UDS Event to send
UniversalDataSystem.UDSEvent updateEvent = new UniversalDataSystem.UDSEvent();
        updateEvent.Create("updateCoins");

      // Set any properties required by the UDS Event
      updateEvent.Properties.Set("coinsCollected", currentCoins);

      // Create a PostEventRequest based off the UDSEvent data
      UniversalDataSystem.PostEventRequest updateRequest = new UniversalDataSystem.PostEventRequest
      {
UserId = GamePad.activeGamePad.loggedInUser.userId,
EventData = updateEvent
};

var asyncRequest = new AsyncRequest<UniversalDataSystem.PostEventRequest>(updateRequest).ContinueWith(antecedent) =>
{
// This code executes when the Request has completed
};

      UniversalDataSystem.Schedule(asyncRequest);
}
```

For more information about this process, see the “[Implementation of Trophy-Unlocking](https://p.siedev.net/resources/documents/SDK/5.000/Trophy_System-Overview/0005.html#0_Ref5013414)” section of [Overview of Application Development](https://p.siedev.net/resources/documents/SDK/5.000/Trophy_System-Overview/0005.html#0_Ref5013414) on DevNet.

