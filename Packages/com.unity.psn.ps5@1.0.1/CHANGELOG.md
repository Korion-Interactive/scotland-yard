# Changelog
All notable changes to the package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

Due to package verification, the latest version below is the unpublished version and the date is meaningless.
however, it has to be formatted properly to pass verification tests.

## [1.0.1] - 2023-10-18

### Fixed
   - Stop editor build script executing when an non-PlayStation platform is being built
   
## [1.0.0] - 2023-09-04
   - New package format.
   - Fix issue with strings when using TitleCloudStorage
   
## [0.0.38-preview] - 2023-06-05

### Added
   - Added Match.RemoveMatch static method to cleanup Match instances that no longer need to be tracked.
      * Allows for Match instances to be forgotten about and released by the GC when they are no longer required.
   - Added SessionsManager.RemovePlayerSession static method to cleanup PlayerSession instances that no longer need to be tracked.
      * Allows for PlayerSession instances to be forgotten about and released by the GC when they are no longer required.
      * Player sessions are cleaned up if the title recieves a notification when a player session has been destroyed. However if a player leavers a session then the session needs to be cleaned up separately. 
   - Added SessionsManager.RemoveGameSession static method to cleanup GameSession instances that no longer need to be tracked.
      * Allows for GameSession instances to be forgotten about and released by the GC when they are no longer required.
      * Game sessions are cleaned up if the title recieves a notification when a game session has been destroyed. However if a player leavers a session then the session needs to be cleaned up separately. 

### Fixed
   - Fixed problems that can occur when a single accountId joins a session from both PS5 and PS4 simultaneously.

## [0.0.37-preview] - 2023-06-01

### Fixed
   - Fixed issue with MatchRequests.ReportResultsRequest failing to run in the MatchesCommands::ReportResultsImpl native code.
   
## [0.0.36-preview] - 2023-05-16

### Added
   - Added support for Crossplatform PS4 SDK 10.5

## [0.0.35-preview] - 2023-04-06

### Added
   - Added support for SDK 7.0
### Fixed
   - Fixed a bug where showing the PlayStation store icon prevented other Common Dialogs from opening.
   - Fixed reporting of API errors when retrieving friends, profiles and basic presences.

## [0.0.34-preview] - 2023-01-27

### Fixed
   - Fixed platform filter errors with PS4 10.0 and PS5 6.0 native plugins.
   - Fixed errors in PlayerSession disableSystemUiMenu and privilege flags (introduced in 0.0.33) when running on PS4.  See https://p.siedev.net/forums/thread/224892/
   - Made `GameSessionInitMember.AccountId` publicly settable, for use in `GameSessionCreationParams.AdditionalMembers`.

## [0.0.33-preview] - 2023-01-06

### Added
   - Added support for NpBandwidthTest library.
      * MeasureBandwidthRequest - Calculate the bps download or upload bandwidth to PlayStation Network.
   - Added DisableSystemUiMenu to Player Sessions to disable certain system UI items for session leaders, for SDK 6 and above.
   - Added support for PromoteToLeader in LeaderPrivilegeFlags for Player Sessions.

## [0.0.32-preview] - 2022-12-16

### Added
   - Added support for Crossplatform PS5 SDK 10.0

## [0.0.31-preview] - 2022-10-19

### Added
   - Added support for SDK 6.0
   - `SessionSignalling.ConnectionEvent` now has a property for the GroupId, corresponding to the `grpId` parameter of the native `SceNpSessionSignalingConnectionCallback2` function.
   - Added support for PlayerInvitationDialog
   - Fixed for GetGameSessionsRequest and GetPlayerSessionsRequest to support multiple sessions
   
## [0.0.30-preview] - 2022-08-18

### Added
   - Updated 'ConnectionEvent.Reasons' enum with new 'Activate' value.

### Fixed
   - Fixed error in native MatchesCommands::GetMatchDetailsImpl method that was leaking HTTP pool memory.

## [0.0.29-preview] - 2022-06-24

### Added
   - Commerce
      * OpenJoinPremiumDialogRequest - Open Join premium dialog.
	  * OpenBrowseCategoryDialogRequest - Open Browse Category dialog.
	  * OpenBrowseProductDialogRequest - Open browse product dialog.
	  * OpenRedeemPromotionCodeDialogRequest - Open redeem promotion code dialog.
	  * OpenCheckoutDialogRequest - Open checkout dialog.
	  * OpenDownloadDialogRequest - Open download dialog.
   - Entitlements
      * GetAdditionalContentEntitlementListRequest - Gets a list of additional content information for which the entitlement is valid
	  * GetSkuFlagRequest - Gets the SKU type
      * GetAdditionalContentEntitlementInfoRequest - Gets additional content information
      * GetEntitlementKeyRequest - Gets the entitlement key of additional content
      * GetUnifiedEntitlementInfoListRequest - Requests to obtain a list of unified entitlement information
      * GetUnifiedEntitlementInfoRequest - Requests to obtain unified entitlement information.
      * GetServiceEntitlementInfoListRequest - Requests to obtain a list of service entitlement information
      * GetServiceEntitlementInfoRequest - Requests to obtain service entitlement information.
      * GenerateTransactionIdRequest -  Generates a transaction ID
      * ConsumeUnifiedEntitlementRequest - Requests to consume a unified entitlement
      * ConsumeServiceEntitlementRequest - Requests to consume a service entitlement
	- Added CommerceSystem.PSStoreIconRequest to show, hide and set the layout of the PlayStation Store Icon.
	  
### Changed
   - Included offline HTML documentation in the HTMLDocs~ folder. Documentation can now be viewed without needing to run a local webserver.
   - Added options to re-initialize PSN to the sample and also to exit the application after shutdown.
   - Added several fixes to correctly shutdown parts of the PSN plugin; SystemEventManager, WebApiInstance, WebApiNotifications.
   - Leaderboards.GetRankingRequest: Added properties for UserCenteredAround and CenterToEdgeLimit to return a set of ranking centered around a given user.
   - MatchMakingRequests.SubmitTicketRequest: Added GameSessionId to support Matchmaking backfill.  

## [0.0.28-preview] - 2022-04-25

### Added 
   - Added support for GameSessionSearch
     - Search for an existing game session based on wide range of parameters, including SearchIndex and SearchAttributes
   - Added new fields in GameSession Creation. 
     - SearchIndex
     - Searchable
     - SearchAttributes
   - Added new fields to GameSession class
     - NatType
     - Searchable
     - SearchIndex

   - Added support for GameUpdate SDK API calls. Allows checking for updates for programs, data and additional content.

   - Added support for request to obtain NAT router information. Calls sdk function sceNetCtlGetNatInfo() to obtain detailed information about the NAT router..

## [0.0.27-preview] - 2022-01-04

### Changed
   - Changed how the recipients of a `SendGameSessionMessageRequest` and `SendPlayerSessionMessageRequest` are specified.  The recipients of the messages are now specified using a new structure `SessionMemberIdentifier`, containing both the account id and the platform of the recipient.  Previously it was just the account id, and the recipients were assumed to be on PS5.  This change fixes a bug where messages sent to session members who were on PS4 would fail to send.  This is a breaking change that requires corresponding alterations to users' code.
   - Added ExceededMaximumOperations exception to all Schedule() calls. This occurs if too many operations are waiting in the work queue. This catches issues where a title is flooding the queue with more work than it can safely process.
   - Improved example of RecordScoreRequest in sample. Sample now better explains what happens when the request has a SCE_NP_WEBAPI_SERVER_ERROR_LEADERBOARDS_LARGE_DATA_EXCEEDS_NUMBER_LIMIT error.
   - Removed "Get Group Info (0)" button from sample as 'Group 0' doesn't exist and it was creating an error message.

### Fixed
   - Error in UploadDataRequest which caused a random crash due to incorrect data serialization.

## [0.0.26-preview] - 2021-11-23

### Added
   - Added support for SDK 4.0

## [0.0.25-preview] - 2021-07-28

### Added
   - Added support for PS4 SDK 8.5 cross-gen

## [0.0.24-preview] - 2021-07-13

### Added
   - Added support for SDK 3.0

## [0.0.23-preview] - 2021-02-23

### Added
   - GameSessionJoinState to SessionMember for game session members. This replaces the IsReserved property.
   - Support for Representative data added to GameSession and RetrievedSessionData. 
   - Handler for "psn:sessionManager:gs:representative:updated" added to game session notification update callback.
   - New 'Start UDS' button added to Trophy menu in sample app to aid in unlocking trophies.
   - Error messages during start-up if any of the modules fail to load or initialization fails.

### Changed
   - Replaced UnlockTrophyProgressRequest with UpdateTrophyProgressRequest. UnlockTrophyProgressRequest has been marked as obsolete.
   - Removed the IsReserved property from SessionMember and replaced it with enum GameSessionJoinState.
   - SessionFilters in PlayerSession and GameSession requests now use a static instance of PlayerSessionFilters or GameSessionFilters. This shares the same set of filters with all the players WebApiPushEvent that join the session. This helps reduce the number of resources allocated by session. This default behaviour can be changed by assigning any custom filter set to the SessionFilters property. 
   - Sample now has improved handling of off-line users and handles user logging into PSN while the sample is running. 

### Fixed
   - JoinGameSessionRequest now uses correct GameSession.SessionWebApiEventHandler instead of PlayerSession.SessionWebApiEventHandler.
   - JoinGameSessionRequest incorrectly initialised SessionFilters to use PlayerSessionFilters. Fixed to now use GameSessionFilters.
   - The Joined or Reserved state of a game session member is now correctly updated when RetrievedSessionData is applied to a GameSession. 
   - WebApiPushEvent created for each user joining a PlayerSession or GameSession are now released automatically when the player leaves that session.
   - Fixed typo in GetActivePashEvents to GetActivePushEvents
   - Removed unused SessionsManager.PlayerSessionFilters property.

## [0.0.22-preview] - 2021-02-11

### Added
   - Title Cloud Storage
      * AddAndGetVariableRequest - Add a value to a TCS variable and read the result.
      * SetVariableWithConditionRequest - Write to a TCS variable conditionally.
      * GetMultiVariablesBySlotRequest - Read TCS variables from multiple users.
      * SetMultiVariablesByUserRequest -  Write TCS variables.
      * GetMultiVariablesByUserRequest - Read TCS variables.
      * DeleteMultiVariablesByUserRequest - Delete TCS variables
      * UploadDataRequest - Write TCS data
      * DownloadDataRequest - Read TCS data
      * DeleteMultiDataBySlotRequest - Delete TCS data from multiple users
      * DeleteMultiDataByUserRequest - Delete TCS data from multiple slots belonging to the specified user
      * GetMultiDataStatusesBySlotRequest - Read TCS data statuses from multiple users for a slot
      * GetMultiDataStatusesByUserRequest - Read TCS data statuses from multiple slots for the specified user  

### Fixed
   - MatchPlayerStats now serializes PlayerId correctly.
   - OnlineSafety::GetCRSImpl C++ code now cleans up its parameters and transaction objects.
   - PS4 Crash fixed: Crash in 0.0.21 was caused by SCE_SYSMODULE_NP_SESSION_SIGNALING not be loaded in the PS4 prx.

### Changed
   - GetCommunicationRestrictionStatusRequest - This will now return correctly if a user isn't signed up or signed into PSN. This can now be called before a user is registered with the system (AddUserRequest). If a user is signed in but not registered with the system it will return CRStatus.SignedInNotRegistered. If this occurs add the user, using AddUserRequest, then call GetCommunicationRestrictionStatusRequest to get the Restricted or Unrestricted state. If AddUserRequest is called on a user not signed up or signed in for PSN then it will fail to create the internal contexts required by the various PSN libraries. 

## [0.0.21-preview] - 2021-01-18

### Added
   - UDS API
      * UnlockTrophyProgressRequest - Unlocks a progress trophy that uses a default Stat.
   - TrophySystem API
	  * OnUnlockNotification - Callback when a trophy has become unlocked
	  * TrophyDetails.IsProgress - Boolean value to indicate if the trophy is a progress type.
	  * TrophyDetails.TargetValue - Target Value that will unlock the trophy. Only valid if IsProgress is true.
	  * TrophyData.IsProgress - Boolean value to indicate if the users trophy is a progress type.
	  * TrophyDetails.ProgressValue - The current progress value of the trophy for the user. Only valid if IsProgress is true.

### Changed
   - Sample app
      * Updated Sample App to demonstrate two methods to unlock progress trophies.
	  * Added new menu option to unlock the next locked trophy. This uses a list of locked (non-progress) trophies and proceeds to unlock them in order. 
	  * The app can unlock a simple progress trophy with a default stat using UnlockTrophyProgressRequest. The trophy unlocks once the value reaches 100.
	  * The app has two progress trophies that are linked to a single custom stat called 'killCount'. One trophy will unlock once this number reaches 3 and the second one will unlock when it reaches 20.
	  * The custom stat 'killCount' contains a 'Stat Extraction' rule which updates the state when an event called `UpdateKillCount` is received and contains a value called 'newKillCount'.

### Fixed
   - Leaderboards
      * Fixed crash when RecordScoreRequest.NeedsTmpRank is set to false.

## [0.0.20-preview] - 2020-11-26

### Fixed
   - Critical Fix:
      * Fixed a problem where sceNpDeleteRequest() wasn't being called after sceNpCheckPremium was used.
      * This is called as part of the FeatureGating CheckPremiumRequest

## [0.0.19-preview] - 2020-11-10

### Added
   - Added additional support for SDK 2.0 on PS5 and cross-gen PS4 support for SDK 8.0
      * Moved existing .prx files to plug-in folders 1_00 and 7_50
	  * Added new .prx files to plug-in folders 2_00 and 8_00

### Fixed
   - Using Commerce or Message dialog could cause the dialog to not open again.
      * Fixed an issue where the native code wasn't calling sceNpCommerceDialogTerminate or sceMsgDialogTerminate when dialog was closed.
   - Player Review Dialog not available on PS4
      * Player Review Dialog has now been disabled on PS4 as its not available.
	  * Also fixed issue where scePlayerReviewDialogTerminate wasn't called when the dialog was closed.

### Changed
   - Enable Message dialog on PS4. Before this was only enabled for the PS5.

## [0.0.18-preview] - 2020-11-01

### Fixed
   - C# Marshalling
      * Improved performance of marshalling data between the C# and C++ native plug-ins. 
	  * There was a slow copy of marshal data occurring each frame during an update call to game intent. 

## [0.0.17-preview] - 2020-10-14

### Added
   - Matchmaking
      * SubmitTicketRequest - Create a matchmaking ticket
	  * GetTicketRequest - Obtain a matchmaking ticket
	  * CancelTicketRequest - Delete a matchmaking ticket
	  * GetOfferRequest - Obtain a matchmaking offer
	  * ListUserTicketsRequest - Obtain a list of matchmaking tickets that a user has joined
	  * WebAPINotifications for matchmaking notifications
	  * Example of matchmaking rules 'Doubles_Match_Ignoring_NatType.json'
   - UniversalDataSystem
   	  * Added AppendValues method for EventPropertyArray 
	  
### Changed
   - UserSessionFilters
      * Added additional "psn:matchmaking" notification datatypes for MatchMaking Ticket and Offer.
	  
### Fixed
  - GameSessions
	  * Fixed issue with GetGameSessionsRequest. The returned RetrievedSessionData didn't have flags set for Matchmaking and ReservationTimeoutSeconds properties.
	  * Fixed issue with GameSessionNotifications.NotificationTypes.InvitationsCreated not being sent. When Matchmaking tickets are matched the application will not receive the GameSessionNotifications.NotificationTypes.InvitationsCreated notification.
  - NpPlatformType
      * Fixed the NpPlatformType enum as PS5 value was set to 4 and should have been 5 to match SCE_NP_PLATFORM_TYPE_PROSPERO	
	  
## [0.0.16-preview] - 2020-10-09

### Fixed
   - Fixed crash in WebApiNotifications::FindOrderedPushEvent native code when a user wasn't registered with the system.

## [0.0.15-preview] - 2020-10-01

### Fixed
   - Removed calls to printf function in native plugin

## [0.0.14-preview] - 2020-09-29

### Fixed 
  - Removed calls to Debug.Log methods for Non-Development builds
  - Add missing documentation for various enums, classes and methods.

## [0.0.13-preview] - 2020-09-22

### Added
  - Leaderboards
      * GetBoardDefinitionRequest - Get the leaderboard definition state.
	  * RecordScoreRequest - Record score, including attaching large data if required
	  * GetRankingRequest - Read the current leaderboard results
	  * GetLargeDataRequest - Download any large data attached to a leaderboard entry.
  - Large data restrictions:
      * Only one of the upload strategies is currently supported. Large data is uploaded after the score is sent. 
	  * The internal buffers need to be large enough to handle the size of the data both being uploaded and downloaded.
	  * Use the Main.Initialize method to set the size of the buffers. By default they are set to 2mb

### Changed 
  -  Main.Initialize - Added parameters to set the internal read and write buffers used to marshal data to the native .prx.
      * These are both defaulted to 2mb. 
	 
## [0.0.12-preview] - 2020-09-14

### Added
  - Support for user sign in/out notifications (OnSignedInNotification, StartSignedStateCallbackRequest and StopSignedStateCallbackRequest)
  - Support for PSN reachability notifications (OnReachabilityNotification, StartReachabilityStateCallbackRequest and StopReachabilityStateCallbackRequest)

## [0.0.11-preview] - 2020-09-07

### Added
  - Matches
      * CreateMatchRequest - Create a match.
	  * GetMatchDetailRequest - Retrieve the details and status of a match.
	  * UpdateMatchStatusRequest - Change the match status to playing, on-hold or cancelled.
	  * JoinMatchRequest - Have a player join a match.
	  * LeaveMatchRequest - Have a player leave a match.
	  * ReportResultsRequest - Submit the results of a match and match stats.
	  * UpdateMatchDetailRequest - Update the details of the match, including submitting interim results and stats.
  - Player review dialog
	  * OpenPlayerReviewDialogRequest - Display player review dialog.
  - Message dialog
      * OpenMsgDialogRequest - Display the common message dialog, including support for showing the PSN Communication Restriction message

### Fixed
  - Sample
      * Display publishing settings path when using `Set Publish Settings For PS5` editor script in Unity 2020.1 or above

## [0.0.10-preview] - 2020-08-28

### Changed 
  - GetBlockingUsersRequest - Added support to return NextOffset, PreviousOffset and TotalItemCount 

## [0.0.9-preview] - 2020-07-31

### Added
  - Game Sessions
      * CreateGameSessionRequest - Create a game session
	  * LeaveGameSessionRequest - A user leaves the game session
	  * JoinGameSessionRequest - A user joins an existing game session as a player or spectator
	  * GetGameSessionsRequest - Get info about a game session
	  * SetGameSessionPropertiesRequest - Change some of the parameters of a game session
	  * Added PlayerNotification and GameNotification type for user based notifications for player and game sessions.
	  * SetGameSessionMemberSystemPropertiesRequest - Set custom data for a game session member
	  * SendGameSessionMessageRequest - Send player session message
	  * GetJoinedGameSessionsByUserRequest - Get a list of game sessions belonging to the user
	  * DeleteGameSessionRequest - Delete a game session.
  - Commerce Dialog
      * OpenJoinPremiumDialogRequest = Support to open and close Join premium dialog

### Changed  
  - Renamed NotificationPlayerSessionData to NotificationSessionData as both player and game session notification share the same json notification structure.
  - Changed Notification struct to class so it can contain player and game notification types

## [0.0.8-preview] - 2020-07-02

### Added
  - Player Sessions
      * Merge session changes in the RetrievedSessionData class, received from calling the GetPlayerSessionsRequest, into the PlayerSession object. This will allow the current state of the player session to be cached locally.
      * GetPlayerSessionInvitationsRequest used to retrieve invitations for a user.
      * ChangePlayerSessionLeaderRequest used change the leader of a player session
      * AddPlayerSessionJoinableSpecifiedUsersRequest - Add list of join-able specified users to a session.
      * DeletePlayerSessionJoinableSpecifiedUsersRequest - Delete list of join-able specified users from a session.
      * SetPlayerSessionMemberSystemPropertiesRequest - Set member system properties for a player session (CustomData1)
      * SendPlayerSessionMessageRequest - Sends a message to a list of user.
      * GetJoinedPlayerSessionsByUserRequest - Obtain a list of Player Sessions in which a user is participating
      * SetPlayerSessionPropertiesRequest - Update Player Session information
      * SwapPlayerSessionMemberRequest - Swap session member between between player and spectator.
      * Create session now supports setting both CustomData1 and CustomData2 during creation.
  - WebApi Notifications
      * Added user event callback, UserSessionEventHandler, called when non-guaranteed user events are received.
  - PS4 Cross-gen
      * Added PS4 Cross-gen .prx plugins to package
      * Supports Authentication, Player Sessions, Game Intent, Online Safety, WebApi Notifications

### Changed  
  - Updated the RetrievedSessionData class to include all the data returned from the GetPlayerSessionsRequest
  - Improved error messages in Sample app. Each async requests output errors if they occur.
  - Updated many request class to use List<> objects instead of Arrays to reduce memory garbage. 
  - Reduced number of delegates to just one for a PlayerSession and another for User events. (OnSessionUpdated, OnUserEvent)
  
## [0.0.7-preview] - 2020-06-29

### Changed

- Removed from WebApiNotifications:
  * CreateFilterRequest and RemoveFilterRequest
  * RegisterUserCallbackRequest and UnregisterUserCallbackRequest
- Added to WebApiNotifications:
  * CreatePushEventRequest and DeletePushEventRequest
  * WebApiPushEvent -> Filters are now included inside the a new Push event object
  * Push events can be configured to be order-guaranteed
- WebApiFilterSet renamed to WebApiFilters

### Added

- PlayerSession:
  * Create session, setting max players, max spectators, allow swapping, join and invite restrictions, leader privileges and localised session name
  * Leave session
  * User can join an already existing session as a player or spectator, including join a session created by another local user.
  * Retrieve player session settings and data
  * Send session invites
  * Merge player and spectator info retrieved from WebApi notifications into the local PlayerSession object.
  * Added callbacks to PlayerSession when WebApiNotifications are received. 
    * OnSessionMemberUpdated is used when member joins, leaves or changes role. The data from the notification is automatically updated in the PlayerSession
    * OnSessionUpdated is used when a session parameter is changed. The notification doesn't contain any data about the change therefore RetrievedSessionData should be used to retrieve the data.  
    * SetExternalEventCallback is used to return raw notification data. 

- WebApiFilters
  * Supports ServiceName and ServiceLabel parameters
  * Now reference counted when registered with a WebApiPushEvent
  * Can only be unregistered when its reference count is 0

## [0.0.6-preview] - 2020-05-20

### Added

- Feature Gating provides methods to check if a user is allowed to access premium features or notify the system when a premium feature is being used
  * CheckPremiumRequest
- Authentication system provides a feature to obtain the authorization code required by the application server to access user information on the PSN server 
  * GetAuthorizationCodeRequest
  * GetIdTokenRequest

## [0.0.5-preview] - 2020-04-01

### Changed

- PostEventRequest can now return the estimated size of the sent event.
- Renamed TrophyService to TrophySystem
- Renamed GameIntentService to GameIntentSystem
- Renamed RegisterUserFiltersRequest to RegisterUserCallbackRequest
- Renamed UnregisterUserFiltersRequest to UnregisterUserCallbackRequest

### Added

- StartSystemRequest and StopSystemRequest added to UDS. 
- Can now set the UDS memory pool size using StartSystemRequest.
- GetMemoryStatsRequest returns memory stats for UDS.
- Included support for Float64 (double), Bool and Binary property types for UDS EventProperty and EventPropertyArray
- Retrieve Game Icons, Group Icons, Trophy Icons and Reward Icons from the Trophy system.
- ShowTrophyListRequest will display the Trophy list system dialog.
- Online Safety features supported - GetCommunicationRestrictionStatusRequest and FilterProfanityRequest added. 

## [0.0.4-preview] - 2020-03-05

### Changed

- Updated to use SDK 0.95
- Move Trophy support from separate package into this one.
- Split system into additional namespaces to group functionality and produce better documentation

### Added

- Added HTML documentation to HTML~ directory in package root.

## [0.0.3-preview] - 2019-12-13
 
### Changed

- Updated to work with SDK 0.9
- Renamed Pzazz to PS5

### Added

- WebApi Notifications
  * Create Filters and notifications for webapi events.
- User presence
  * Return online/offline basic presence for user(s)

## [0.0.2-preview] - 2019-10-31
 
### Changed

- Moved location of C++ source files and update VS projects

## [0.0.1-preview] - 2019-10-17

### Fixed

- Initial Version

### Changed

- Initial Version

### Added

- Asynchronous Requests
  * Provides a way to call the API using asynchronous calls.
- User Management
  * User can be registered and unregistered with the underlying native services. This is required for all other API's to work on a system level.
- Universal Data System
  * Supports sending events and creating Event data, include custom event data structures. 
- Game Intent
  * Supports a callback when a user selects an Activity. 
  




 
  





