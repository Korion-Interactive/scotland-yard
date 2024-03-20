#pragma once


#include "SharedCoreIncludes.h"
#include <map>

namespace UnityEventQueue
{
    class IEventQueue;
}

namespace psn
{
    class  MsgHandler
    {
    public:

        typedef void(*MethodCallbackSimple)(APIResult* result);
        typedef void(*MethodCallbackWithData)(UInt8* sourceData, int sourceSize, APIResult* result);
        typedef void(*MethodCallbackFull)(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        struct MsgHeader
        {
            UInt32 version;
            UInt32 methodId;
        };
        static void ProcessMsg(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static bool GetSimpleMethod(UInt32 methodId, MethodCallbackSimple* foundMethod);
        static bool GetWithDataMethod(UInt32 methodId, MethodCallbackWithData* foundMethod);
        static bool GetFullMethod(UInt32 methodId, MethodCallbackFull* foundMethod);

        static void AddMethod(int methodId, MethodCallbackSimple methodCallback);
        static void AddMethod(int methodId, MethodCallbackWithData methodCallback);
        static void AddMethod(int methodId, MethodCallbackFull methodCallback);

        enum UserState
        {
            Added,
            Removed,
        };

        typedef void(*UserStateCallback)(SceUserServiceUserId userId, UserState state, APIResult* result);


        static void NotifyAddUser(SceUserServiceUserId userId, APIResult* result);
        static void NotifyRemoveUser(SceUserServiceUserId userId, APIResult* result);
        static void RegisterUserCallback(UserStateCallback callback);

        typedef void(*SystemEventCallback)(SceSystemServiceEvent& sevent);

        static void NotifySystemEvent(SceSystemServiceEvent& sevent);

        static void RegisterSystemEventCallback(SystemEventCallback callback);
        static bool IsSystemEventQueueInitialized();

        static void InitialiseSystemEventManager(UnityEventQueue::IEventQueue* eventQueue);
        static void ShutdownSystemEventManager();
    };


    //static std::map<int, MsgHandler::MethodCallbackSimple> s_MethodSimpleList;
    //static std::map<int, MsgHandler::MethodCallbackWithData> s_MethodWithDataList;
    //static std::map<int, MsgHandler::MethodCallbackFull> s_MethodFullList;
    //static std::list<MsgHandler::UserStateCallback> s_UserStateCallbackList;
    //static std::list<MsgHandler::SystemEventCallback> s_SystemEventCallbackList;
}
