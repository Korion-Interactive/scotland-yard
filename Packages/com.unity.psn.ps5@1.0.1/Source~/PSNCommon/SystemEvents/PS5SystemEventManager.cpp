#if defined(GLOBAL_EVENT_QUEUE)

#include "../SharedInclude/SonyCommonIncludes.h"
#include "../SharedInclude/HandleMsg.h"
#include "../PlayerInterface/UnityEventQueue.h"

#include "PS5SystemEvents.h"
#include "PS5SystemEventManager.h"

namespace UnityPlugin
{
    //PS5SystemEventManager PS5SystemEventManager::s_SystemEventManager;

    UnityEventQueue::ClassBasedEventHandler<PS5OnResume, PS5SystemEventManager>                             g_OnResumeAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnGameLiveStreamingStatusUpdate, PS5SystemEventManager>      g_OnGameLiveStreamingStatusUpdate;
    UnityEventQueue::ClassBasedEventHandler<PS5OnSessionInvitation, PS5SystemEventManager>                  g_OnSessionInvitationAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnEntitlementUpdate, PS5SystemEventManager>                  g_OnEntitlementUpdateAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnGameCustomData, PS5SystemEventManager>                     g_OnGameCustomDataAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnDisplaySafeAreaUpdate, PS5SystemEventManager>              g_OnDisplaySafeAreaUpdateAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnUrlOpen, PS5SystemEventManager>                            g_OnUrlOpenAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnLaunchApp, PS5SystemEventManager>                          g_OnLaunchAppAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnLaunchLink, PS5SystemEventManager>                         g_OnLaunchLinkAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnAddcontentInstall, PS5SystemEventManager>                  g_OnAddcontentInstallAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnResetVrPosition, PS5SystemEventManager>                    g_OnResetVrPositionAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnJoinEvent, PS5SystemEventManager>                          g_OnJoinEventAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnPlaygoLocusUpdate, PS5SystemEventManager>                  g_OnPlaygoLocusUpdateAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnOpenShareMenu, PS5SystemEventManager>                      g_OnOpenShareMenuAdapter;
    UnityEventQueue::ClassBasedEventHandler<PS5OnPlayTogetherHost, PS5SystemEventManager>                   g_OnPlayTogetherHostAdapter;

    UnityEventQueue::ClassBasedEventHandler<PS5OnSystemEvent, PS5SystemEventManager>                        g_OnSystemEventAdapter;

    void PS5SystemEventManager::Initialize(UnityEventQueue::IEventQueue* eventQueue)
    {
        m_EventQueue = eventQueue;

        if (m_EventQueue)
        {
            m_EventQueue->AddHandler(g_OnResumeAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnGameLiveStreamingStatusUpdate.SetObject(this));
            m_EventQueue->AddHandler(g_OnSessionInvitationAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnEntitlementUpdateAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnGameCustomDataAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnDisplaySafeAreaUpdateAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnUrlOpenAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnLaunchAppAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnLaunchLinkAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnAddcontentInstallAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnResetVrPositionAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnJoinEventAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnPlaygoLocusUpdateAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnOpenShareMenuAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnPlayTogetherHostAdapter.SetObject(this));
            m_EventQueue->AddHandler(g_OnSystemEventAdapter.SetObject(this));
        }
    }

    void PS5SystemEventManager::Shutdown()
    {
        if (m_EventQueue)
        {
            m_EventQueue->RemoveHandler(&g_OnResumeAdapter);
            m_EventQueue->RemoveHandler(&g_OnGameLiveStreamingStatusUpdate);
            m_EventQueue->RemoveHandler(&g_OnSessionInvitationAdapter);
            m_EventQueue->RemoveHandler(&g_OnEntitlementUpdateAdapter);
            m_EventQueue->RemoveHandler(&g_OnGameCustomDataAdapter);
            m_EventQueue->RemoveHandler(&g_OnDisplaySafeAreaUpdateAdapter);
            m_EventQueue->RemoveHandler(&g_OnUrlOpenAdapter);
            m_EventQueue->RemoveHandler(&g_OnLaunchAppAdapter);
            m_EventQueue->RemoveHandler(&g_OnLaunchLinkAdapter);
            m_EventQueue->RemoveHandler(&g_OnAddcontentInstallAdapter);
            m_EventQueue->RemoveHandler(&g_OnResetVrPositionAdapter);
            m_EventQueue->RemoveHandler(&g_OnJoinEventAdapter);
            m_EventQueue->RemoveHandler(&g_OnPlaygoLocusUpdateAdapter);
            m_EventQueue->RemoveHandler(&g_OnOpenShareMenuAdapter);
            m_EventQueue->RemoveHandler(&g_OnPlayTogetherHostAdapter);
            m_EventQueue->RemoveHandler(&g_OnSystemEventAdapter);
        }
    }

    // System events...
    void PS5SystemEventManager::HandleEvent(PS5OnResume& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnResume\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnGameLiveStreamingStatusUpdate& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnGameLiveStreamingStatusUpdate\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnSessionInvitation& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnSessionInvitation\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnEntitlementUpdate& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnEntitlementUpdate\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnGameCustomData& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnGameCustomData\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnDisplaySafeAreaUpdate& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnDisplaySafeAreaUpdate\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnUrlOpen& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnUrlOpen\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnLaunchApp& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnLaunchApp\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnLaunchLink& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnLaunchLink\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnAddcontentInstall& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnAddcontentInstall\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnResetVrPosition& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnResetVrPosition\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnJoinEvent& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnJoinEvent\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnPlaygoLocusUpdate& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnPlaygoLocusUpdate\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnOpenShareMenu& data)
    {
        //UNITY_TRACE("PS5SystemEventManager::HandleEvent: PS5OnOpenShareMenu\n");
    }

    void PS5SystemEventManager::HandleEvent(PS5OnPlayTogetherHost& data)
    {
    }

    void PS5SystemEventManager::HandleEvent(PS5OnSystemEvent& data)
    {
        psn::MsgHandler::NotifySystemEvent(data.params);
    }

    DO_EXPORT(void, PrxInjectSystemServiceEvent) (int type, void *paramdata)
    {
        SceSystemServiceEvent serviceEvent;
        serviceEvent.eventType = (SceSystemServiceEventType)type;
        memcpy(&serviceEvent.data.param, paramdata, 8192);
        psn::MsgHandler::NotifySystemEvent(serviceEvent);
    }

    DO_EXPORT(bool, PrxIsSystemEventQueueInitialized) ()
    {
        return psn::MsgHandler::IsSystemEventQueueInitialized();
    }


    // App events...
}
#endif
