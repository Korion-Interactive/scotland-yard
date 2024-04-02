# C# API features

This page provides a high level view of the main features provided by the C# API for working with save data.

## Asynchronous Support

The C# API is based on a request and response API; your title creates Request and Response objects, and then you can call the corresponding C# API. The call will be processed on a background thread. When the request has finished, a response callback provides the results so the title can process them.

You can use the following APIs:

|**Purpose**|**API class**|**Notes**|
|---|---|---|
|Starting the system|[Main.Initialize](../api/Unity.SaveData.PS5.Main.html#Unity_SaveData_PS5_Main_Initialize_Unity_SaveData_PS5_Initalization_InitSettings_)|You must initialize the save data system before you can use it.<br/><br/>For more information, see the [Getting Started](GettingStarted.md) page. To learn how to initialize the API, see [Initialization](Initialization.md).|
|Mounting a save data|[Mounting](../api/Unity.SaveData.PS5.Mount.Mounting.html)|The `Mounting` class provides methods to create and open a save data for read and write access.|
|Deleting a save data|[Deleting](../api/Unity.SaveData.PS5.Delete.Deleting.html)| Deletes a user save data |
|Backup|[Backups](../api/Unity.SaveData.PS5.Backup.Backups.html)| You can configure the system to automatically back up a save data after it has been written. |

|Search|[Searching](../api/Unity.SaveData.PS5.Search.Searching.html)|Use the `Search` class to enumerate all the save data within the title for a specific user.|
|System dialogs|[Dialogs](../api/Unity.SaveData.PS5.Dialog.Dialogs.html)|The dialog system provides access to the built-in PS5 system dialogs for saving. This supports multiple types of dialog display as defined in the [Dialog Mode](../api/Unity.SaveData.PS5.Dialog.Dialogs.DialogMode.html).<br/><br/>The C# API also provides an example of a Save, Load and Delete dialog state machine. This contains a Unity coroutine which uses the various dialog and other package C# APIs to create a save, load or delete sequence. You can use this as-is, or create your own implementation using this as an example.<br/><br/>For more information, see [Save Data Dialog Process](../api/Unity.SaveData.PS5.Dialog.SaveDataDialogProcess.html).|
|File operations|[FileOps](../api/Unity.SaveData.PS5.Info.FileOps.html)|The `FileOps` system provides asynchronous callback methods. Your title uses these callbacks to execute title specific read and write file operations. <br/><br/>You can use calls to standard C# IO methods such as .NET File and Directory classes. Your save data can contain multiple files of any text or binary file type.|
