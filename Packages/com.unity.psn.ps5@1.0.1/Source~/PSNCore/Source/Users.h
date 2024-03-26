#pragma once
#include "SharedCoreIncludes.h"
#include <map>
#include "WebApi.h"

using namespace sce::Np::CppWebApi::UserProfile::V1;
using namespace sce::Np::CppWebApi;

namespace psn
{
    class UserSystem
    {
    public:

        enum Methods
        {
            AddUser = 0x0100001u,
            RemoveUser = 0x0100002u,
            GetFriends = 0x0100003u,
            GetProfiles = 0x0100004u,
            GetBasicPresences = 0x0100005u,
            GetBlockingUsers = 0x0100006u,

            StartSignedStateCallback = 0x0100007u,
            StopSignedStateCallback = 0x0100008u,
            FetchSignedStateEvent =  0x0100009u,

            StartReachabilityStateCallback = 0x0100010u,
            StopReachabilityStateCallback = 0x0100011u,
            FetchReachabilityStateEvent = 0x0100012u,
        };

        class UserIds
        {
        public:

            UserIds(SceUserServiceUserId userId);

            int Create();
            int Destroy();

            SceUserServiceUserId m_userId;
            SceNpAccountId m_accountId;
        };

        static void RegisterMethods();

        static void AddUserImpl(UInt8* sourceData, int sourceSize, APIResult* result);
        static void RemoveUserImpl(UInt8* sourceData, int sourceSize, APIResult* result);

        static void GetFriendsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void HandleUserState(SceUserServiceUserId userId, MsgHandler::UserState state, APIResult* result);

        static int GetFriendsInternal(SceNpAccountId accountId, WebApiUserContext* user, UInt32 offset, UInt32 limit,
            FriendsApi::ParameterToGetFriends::Filter filter,
            FriendsApi::ParameterToGetFriends::Order order,
            Common::IntrusivePtr<Common::Vector<SceNpAccountId> > &accountIdPtr, Int32& nextOffset, Int32& previousOffset);

        static void GetProfilesImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static int GetProfilesInternal(std::string accountIds, WebApiUserContext* user,
            Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<BasicProfile> > > &profilesPtr);

        static void GetBasicPresencesImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static int GetBasicPresencesInternal(std::string accountIds, WebApiUserContext* user,
            Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<BasicPresence> > > &presencesPtr);

        static void GetBlockingUsersImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static int GetBlockingUsersInternal(WebApiUserContext* user, UInt32 offset, UInt32 limit, BinaryWriter& writer);
        //  Common::IntrusivePtr<Common::Vector<SceNpAccountId>> &blocksPtr);

        static void WriteAvatarList(BinaryWriter& writer, Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<Avatar> > > avatarsPtr);

#if !__ORBIS__
        static void SignedStateCallback(SceUserServiceUserId userId, SceNpState state, void *userData);

        static void StartSignedStateCallbackImpl(UInt8* sourceData, int sourceSize, APIResult* result);
        static void StopSignedStateCallbackImpl(UInt8* sourceData, int sourceSize, APIResult* result);
        static void FetchSignedStateEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        struct SigninStateEvent
        {
            SceUserServiceUserId userId;
            SceNpState state;
        };

        static int s_SignInCallbackId;
        static std::list<SigninStateEvent> s_PendingSigninStateList;

        static void ReachabilityStateCallback(SceUserServiceUserId userId, SceNpReachabilityState state, void *userData);

        static void StartReachabilityStateCallbackImpl(UInt8* sourceData, int sourceSize, APIResult* result);
        static void StopReachabilityStateCallbackImpl(UInt8* sourceData, int sourceSize, APIResult* result);
        static void FetchReachabilityStateEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        struct ReachabilityStateEvent
        {
            SceUserServiceUserId userId;
            SceNpReachabilityState state;
        };

        static std::list<ReachabilityStateEvent> s_PendingReachabilityStateList;
#endif

        static UserMap<UserIds> s_UsersList;
    };
}
