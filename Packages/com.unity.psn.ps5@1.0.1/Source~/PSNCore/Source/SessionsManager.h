#pragma once
#include "SharedCoreIncludes.h"
#include "WebApiNotifications.h"
#include <map>
#include <np_cppwebapi.h>

#include "PlayerSession.h"
#include "GameSession.h"
#include "Matches.h"
#include "MatchMaking.h"

namespace sceSessionManager = sce::Np::CppWebApi::SessionManager::V1;
using namespace sceSessionManager;

namespace psn
{
    class SessionsManager
    {
    public:

        enum Methods
        {
            //CreatePlayerSession = 0x0A00001u,
        };

        static void RegisterMethods();

        //static void CreatePlayerSessionImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void InitializeLib();
        static void TerminateLib();

    private:

        //static SessionMap<PlayerSession> s_PlayerSessions;
    };
}
