#if UNITY_PS5
using Unity.PSN.PS5.Aysnc;


static class AsyncRequestUtil
{
    public static bool CheckAsyncResultRequest<TRequest>(AsyncRequest<TRequest> request) where TRequest : Request
    {
        if (request == null)
        {
            UnityEngine.Debug.LogError("AsyncRequest is null");
            return false;
        }

        return request.Request.Result.apiResult == Unity.PSN.PS5.APIResultTypes.Success;
    }
}

#endif