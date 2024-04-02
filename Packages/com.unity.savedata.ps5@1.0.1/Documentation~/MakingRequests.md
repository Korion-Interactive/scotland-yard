
# Making requests

After you [install](Installation.md) the package, you must initialize the system and a response callback needs to be created, as described in the [Initialization](Initialization.md) instructions.

## Creating requests and handling responses

After the system has been initialized, it is ready to receive its first request. All requests can either be asynchronous or block the thread they are called on. 

The following example shows how to create a [searching](../api/Unity.SaveData.PS5.Search.Searching.html) request. All request types follow a similar pattern.

### Creating a request

Each request method requires two objects:

* A request object that contains the request settings
* A response object that contains the results after the request has completed

You should put all calls to the request inside a `try..catch` C# block, because requests can throw exceptions if they use incorrect parameters or options. For brevity, the code example below omits this, but it is recommended to use exception handling. See the [Sample](Sample.md) script code for a more detailed look at how to handle errors and exceptions.

Requests are placed into a queue and processed in the order they are added.

```CSharp
    // Create the request object
    Searching.DirNameSearchRequest request = new Searching.DirNameSearchRequest();

    // Set the request properties including the local user Id 
    request.UserId = userId;
    request.Key = Searching.SearchSortKey.Time;
    request.Order = Searching.SearchSortOrder.Ascending;
    request.IncludeBlockInfo = false;
    request.IncludeParams = false;
    request.MaxDirNameCount = 10;

    // Create a response object
    Searching.DirNameSearchResponse response = new Searching.DirNameSearchResponse();

    // Call the API
    int requestId = Searching.DirNameSearch(request, response);
```

The `requestId` is returned for asynchronous calls. You can use it in [Main.AbortRequest](../api/Unity.SaveData.PS5.Main.html#Unity_SaveData_PS5_Main_AbortRequest_System_Int32_) to cancel a request in the pending queue. If the request is already being processed, you can't cancel it.

### Receiving the response

After a request has completed, the event callback method receives a `SaveDataCallbackEvent` that contains the request and response objects, as well the codes for  errors that occurred during request handling, if there are any.

For an example of the callback, see the `Main_OnAsyncEvent` method in the [Initialization](Initialization.md) instructions. 

The following example omits error handling, but this should be done in a full implementation of the callback method.

```CSharp
    private void Main_OnAsyncEvent(SaveDataCallbackEvent callbackEvent)
    {
        switch (callbackEvent.ApiCalled)
        {
            case FunctionTypes.DirNameSearch:
                {
                    Searching.DirNameSearchResponse response = callbackEvent.Response as Searching.DirNameSearchResponse;

                    // Handle response here. In the case of a DirNameSearchResponse, this contains an array of found save data.
                }
                break;
        }
    }
```

### Polling responses

Instead of handling a response via the callback, you can poll the response to test if it has completed. This can be useful when doing a Unity coroutine. Response polling is used in the [dialog state machine](../api/Unity.SaveData.PS5.Dialog.SaveDataDialogProcess.html).

To stop a request from using the callback, set the `IgnoreCallback` property to true.

For example, a part of a coroutine might look like this:

```CSharp
    Searching.DirNameSearchRequest request = new Searching.DirNameSearchRequest();

    request.UserId = userId;
    // Stop the request using the callback
    request.IgnoreCallback = true;

    Searching.DirNameSearchResponse response = new Searching.DirNameSearchResponse();

    // Call the API
    Searching.DirNameSearch(request, response);

    while (response.Locked == true)
    {
        // Yield the coroutine until the next update
        yield return null;
    }

    // Process the results now the response is no longer locked.
```

### Synchronous calls

You can make request calls to block the calling thread until complete. These calls block the thread until they reach the top of the pending queue and their execution completes.

```CSharp
    request.Async = false;
```

Avoid doing this on the main Unity thread because save data operations might take a while to complete.
