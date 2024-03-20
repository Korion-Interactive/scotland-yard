
# Initializing the API

This page illustrates how to initialize the Save Data system and asynchronous callback. 

First, you must initialize the system:

```CSharp
    // Contains the results of initialization including the SDK version of the native plugin.
    InitResult initResult;

    try
    {
        // Set response callback
        Main.OnAsyncEvent += Main_OnAsyncEvent;

        InitSettings settings = new InitSettings();
        // Set which core(s) the Save Data thread can execute on
        settings.Affinity = ThreadAffinity.Core5;

        // Initialize the system.
        initResult = Main.Initialize(settings);

        if (initResult.Initialized == true)
        {
            // Initialization succeeded
        }
        else
        {
            // Initialization failed
        }
    }
    catch (SaveDataException e)
    {
        // Exception - See e.ExtendedMessage for more info
    }
```

Then, define the response callback in the project script code:

```CSharp
    private void Main_OnAsyncEvent(SaveDataCallbackEvent callbackEvent)
    {
        if (callbackEvent.Response != null)
        {
            if (callbackEvent.Response.ReturnCodeValue < 0)
            {
                // An error has occurred. This is a Sony error code value that you can look up in Sony's developer documentation
            }

            if (callbackEvent.Response.Exception != null)
            {
                // An exception occurred in the async code. Get the extended error message for more details.
                if (callbackEvent.Response.Exception is SaveDataException)
                {
                    string errorMessage = ((SaveDataException)callbackEvent.Response.Exception).ExtendedMessage;
                    // Handle the error
                }
            }
        }
    }
```

Finally, terminate the system:

```CSharp
    try
    {
        Main.Terminate();
    }
    catch (SaveDataException e)
    {
        // Exception - See e.ExtendedMessage for more info
    }
```


