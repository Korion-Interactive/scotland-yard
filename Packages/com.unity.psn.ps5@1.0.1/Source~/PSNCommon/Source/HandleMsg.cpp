#include "HandleMsg.h"
#include "../SystemEvents/PS5SystemEventManager.h"

namespace psn
{
    static std::map<int, MsgHandler::MethodCallbackSimple>  s_MethodSimpleList;
    static std::map<int, MsgHandler::MethodCallbackWithData>  s_MethodWithDataList;
    static std::map<int, MsgHandler::MethodCallbackFull>  s_MethodFullList;

    static std::list<MsgHandler::UserStateCallback> s_UserStateCallbackList;
    static std::list<MsgHandler::SystemEventCallback> s_SystemEventCallbackList;

    static UnityPlugin::PS5SystemEventManager* s_SystemEventManager = NULL;
    //m_IEventQueue

    void MsgHandler::ProcessMsg(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        MsgHeader* header = (MsgHeader*)(sourceData);
        sourceData += sizeof(MsgHeader);

        if (header->version != 1)
        {
            ERROR_RESULT(result, "Unexpected message header version");
            return;
        }

        // Find the handle method for the messages
        MethodCallbackSimple methodSimple;
        MethodCallbackWithData methodWithData;
        MethodCallbackFull methodFull;

        if (GetSimpleMethod(header->methodId, &methodSimple) == true)
        {
            // Found simple method
            methodSimple(result);
        }
        else if (GetWithDataMethod(header->methodId, &methodWithData) == true)
        {
            // Found method with input paramaters
            methodWithData(sourceData, sourceSize, result);
        }
        else if (GetFullMethod(header->methodId, &methodFull) == true)
        {
            // Found method with input paramaters and returned data
            methodFull(sourceData, sourceSize, resultsData, resultsMaxSize, resultsSize, result);
        }
        else
        {
            ERROR_RESULT(result, "Can't find registered method from id");
            return;
        }
    }

    bool MsgHandler::GetSimpleMethod(UInt32 methodId, MethodCallbackSimple* foundMethod)
    {
        auto it = s_MethodSimpleList.find(methodId);

        if (it == s_MethodSimpleList.end())
        {
            return false;
        }

        if (it->second == NULL)
        {
            return false;
        }

        *foundMethod = it->second;

        return true;
    }

    bool MsgHandler::GetWithDataMethod(UInt32 methodId, MethodCallbackWithData* foundMethod)
    {
        auto it = s_MethodWithDataList.find(methodId);

        if (it == s_MethodWithDataList.end())
        {
            return false;
        }

        *foundMethod = it->second;

        return true;
    }

    bool MsgHandler::GetFullMethod(UInt32 methodId, MethodCallbackFull* foundMethod)
    {
        auto it = s_MethodFullList.find(methodId);

        if (it == s_MethodFullList.end())
        {
            return false;
        }

        *foundMethod = it->second;

        return true;
    }

    void MsgHandler::AddMethod(int methodId, MsgHandler::MethodCallbackSimple methodCallback)
    {
        s_MethodSimpleList.insert(std::pair<int, MethodCallbackSimple>(methodId, methodCallback));
    }

    void MsgHandler::AddMethod(int methodId, MsgHandler::MethodCallbackWithData methodCallback)
    {
        s_MethodWithDataList.insert(std::pair<int, MethodCallbackWithData>(methodId, methodCallback));
    }

    void MsgHandler::AddMethod(int methodId, MsgHandler::MethodCallbackFull methodCallback)
    {
        s_MethodFullList.insert(std::pair<int, MethodCallbackFull>(methodId, methodCallback));
    }

    void MsgHandler::NotifyAddUser(SceUserServiceUserId userId, APIResult* result)
    {
        for (std::list<UserStateCallback>::iterator it = s_UserStateCallbackList.begin(); it != s_UserStateCallbackList.end(); ++it)
        {
            (*it)(userId, UserState::Added, result);
        }
    }

    void MsgHandler::NotifyRemoveUser(SceUserServiceUserId userId, APIResult* result)
    {
        for (std::list<UserStateCallback>::iterator it = s_UserStateCallbackList.begin(); it != s_UserStateCallbackList.end(); ++it)
        {
            (*it)(userId, UserState::Removed, result);
        }
    }

    void MsgHandler::RegisterUserCallback(UserStateCallback callback)
    {
        s_UserStateCallbackList.push_back(callback);
    }

    void MsgHandler::NotifySystemEvent(SceSystemServiceEvent& sevent)
    {
        for (std::list<SystemEventCallback>::iterator it = s_SystemEventCallbackList.begin(); it != s_SystemEventCallbackList.end(); ++it)
        {
            (*it)(sevent);
        }
    }

    void MsgHandler::InitialiseSystemEventManager(UnityEventQueue::IEventQueue* eventQueue)
    {
#if defined(GLOBAL_EVENT_QUEUE)
        if (s_SystemEventManager == NULL)
        {
            s_SystemEventManager = new UnityPlugin::PS5SystemEventManager();
            s_SystemEventManager->Initialize(eventQueue);
        }
#endif
    }

    void MsgHandler::ShutdownSystemEventManager()
    {
#if defined(GLOBAL_EVENT_QUEUE)
        if (s_SystemEventManager != NULL)
        {
            s_SystemEventManager->Shutdown();
            delete s_SystemEventManager;
            s_SystemEventManager = NULL;
        }
#endif
    }

    void MsgHandler::RegisterSystemEventCallback(SystemEventCallback callback)
    {
        if (s_SystemEventManager != NULL)
        {
            s_SystemEventCallbackList.push_back(callback);
        }
    }

    bool MsgHandler::IsSystemEventQueueInitialized()
    {
        if (s_SystemEventManager==NULL)
            return false;
        return s_SystemEventManager->IsEventQueueValid();
    }
}
