#include "SessionsManager.h"
#include "HandleMsg.h"
#include "WebApi.h"
#include "WebApiNotifications.h"

#include <vector>

namespace psn
{
    void SessionsManager::RegisterMethods()
    {
        PlayerSessionCommands::RegisterMethods();
        GameSessionCommands::RegisterMethods();
        MatchesCommands::RegisterMethods();
        MatchMakingSystem::RegisterMethods();
    }

    void SessionsManager::InitializeLib()
    {
    }

    void SessionsManager::TerminateLib()
    {
    }
}
