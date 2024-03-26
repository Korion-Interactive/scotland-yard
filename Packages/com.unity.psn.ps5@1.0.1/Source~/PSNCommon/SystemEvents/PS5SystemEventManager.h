#pragma once

#include "PS5SystemEvents.h"

#if defined(GLOBAL_EVENT_QUEUE)
namespace UnityPlugin
{
    class PS5SystemEventManager
    {
    public:
        PS5SystemEventManager() : m_EventQueue(NULL) {}
        ~PS5SystemEventManager() {}

        void Initialize(UnityEventQueue::IEventQueue* eventQueue);
        void Shutdown();

        void HandleEvent(PS5OnResume& data);
        void HandleEvent(PS5OnGameLiveStreamingStatusUpdate& data);
        void HandleEvent(PS5OnSessionInvitation& data);
        void HandleEvent(PS5OnEntitlementUpdate& data);
        void HandleEvent(PS5OnGameCustomData& data);
        void HandleEvent(PS5OnDisplaySafeAreaUpdate& data);
        void HandleEvent(PS5OnUrlOpen& data);
        void HandleEvent(PS5OnLaunchApp& data);
        void HandleEvent(PS5OnLaunchLink& data);
        void HandleEvent(PS5OnAddcontentInstall& data);
        void HandleEvent(PS5OnResetVrPosition& data);
        void HandleEvent(PS5OnJoinEvent& data);
        void HandleEvent(PS5OnPlaygoLocusUpdate& data);
        void HandleEvent(PS5OnOpenShareMenu& data);
        void HandleEvent(PS5OnPlayTogetherHost& data);
        void HandleEvent(PS5OnSystemEvent& data);

        bool IsEventQueueValid() { return (m_EventQueue!=NULL); }
    private:
        UnityEventQueue::IEventQueue* m_EventQueue;
    };
}
#endif
