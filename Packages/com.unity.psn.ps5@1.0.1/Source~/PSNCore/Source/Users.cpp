#include "Users.h"
#include "HandleMsg.h"
#include "WebApi.h"
#include <string>

namespace psn
{
    UserMap<UserSystem::UserIds> UserSystem::s_UsersList;

#if !__ORBIS__
    int UserSystem::s_SignInCallbackId = -1;
    std::list<UserSystem::SigninStateEvent> UserSystem::s_PendingSigninStateList;
    std::list<UserSystem::ReachabilityStateEvent> UserSystem::s_PendingReachabilityStateList;
#endif

    void UserSystem::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::AddUser, UserSystem::AddUserImpl);
        MsgHandler::AddMethod(Methods::RemoveUser, UserSystem::RemoveUserImpl);
        MsgHandler::AddMethod(Methods::GetFriends, UserSystem::GetFriendsImpl);
        MsgHandler::AddMethod(Methods::GetProfiles, UserSystem::GetProfilesImpl);
        MsgHandler::AddMethod(Methods::GetBasicPresences, UserSystem::GetBasicPresencesImpl);
        MsgHandler::AddMethod(Methods::GetBlockingUsers, UserSystem::GetBlockingUsersImpl);

#if !__ORBIS__
        MsgHandler::AddMethod(Methods::StartSignedStateCallback, UserSystem::StartSignedStateCallbackImpl);
        MsgHandler::AddMethod(Methods::StopSignedStateCallback, UserSystem::StopSignedStateCallbackImpl);
        MsgHandler::AddMethod(Methods::FetchSignedStateEvent, UserSystem::FetchSignedStateEventImpl);

        MsgHandler::AddMethod(Methods::StartReachabilityStateCallback, UserSystem::StartReachabilityStateCallbackImpl);
        MsgHandler::AddMethod(Methods::StopReachabilityStateCallback, UserSystem::StopReachabilityStateCallbackImpl);
        MsgHandler::AddMethod(Methods::FetchReachabilityStateEvent, UserSystem::FetchReachabilityStateEventImpl);
#endif

        MsgHandler::RegisterUserCallback(HandleUserState);
    }


    void UserSystem::AddUserImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        InitialiseData* data = (InitialiseData *)(sourceData);

        MsgHandler::NotifyAddUser(data->userId, result);
    }

    void UserSystem::RemoveUserImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        InitialiseData* data = (InitialiseData *)(sourceData);

        MsgHandler::NotifyRemoveUser(data->userId, result);
    }

    UserSystem::UserIds::UserIds(SceUserServiceUserId userId)
    {
        m_userId = userId;
        m_accountId = 0;
    }

    int UserSystem::UserIds::Create()
    {
        return sceNpGetAccountIdA(m_userId, &m_accountId);
    }

    int UserSystem::UserIds::Destroy()
    {
        return 0;
    }

    void UserSystem::HandleUserState(SceUserServiceUserId userId, MsgHandler::UserState state, APIResult* result)
    {
        if (state == MsgHandler::UserState::Added)
        {
            if (s_UsersList.DoesUserExist(userId) == true)
            {
                // User already registered so don't do this again
                WARNING_RESULT(result, "User already initialised with User system");
                return;
            }

            UserIds* user = s_UsersList.CreateUser(userId);

            user->Create();
        }
        else if (state == MsgHandler::UserState::Removed)
        {
            UserIds* user = s_UsersList.FindUser(userId);

            if (user == NULL)
            {
                WARNING_RESULT(result, "User not registered with User system");
                return;
            }

            user->Destroy();

            s_UsersList.DeleteUser(userId);
        }

        SUCCESS_RESULT(result);
    }

    void UserSystem::GetFriendsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = WebApi::Initialise();
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        *resultsSize = 0;

        //printf("UserSystem::GetFriendsImpl\n");

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        UserIds* user = s_UsersList.FindUser(userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with User service");
            return;
        }

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        UInt32 offset = reader.ReadInt32();
        UInt32 limit = reader.ReadInt32();

        FriendsApi::ParameterToGetFriends::Filter filter = (FriendsApi::ParameterToGetFriends::Filter)reader.ReadUInt32();
        FriendsApi::ParameterToGetFriends::Order order = (FriendsApi::ParameterToGetFriends::Order)reader.ReadUInt32();

        Common::IntrusivePtr<Common::Vector<SceNpAccountId> > accountIdPtr;

        Int32 nextOffset, previousOffset;

        ret = GetFriendsInternal(user->m_accountId, userWebCtx, offset, limit, filter, order, accountIdPtr, nextOffset, previousOffset);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Prepare to write results to output buffer
        BinaryWriter writer(resultsData, resultsMaxSize);

        if (accountIdPtr.get() == nullptr || accountIdPtr->size() <= 0)
        {
            writer.WriteUInt32(0); // count
            writer.WriteInt32(nextOffset);
            writer.WriteInt32(previousOffset);
        }
        else
        {
            size_t size = accountIdPtr->size();

            writer.WriteUInt32(size);
            writer.WriteInt32(nextOffset);
            writer.WriteInt32(previousOffset);

            for (auto it = accountIdPtr->begin(); it != accountIdPtr->end(); ++it)
            {
                SceNpAccountId friendId = (*it);
                writer.WriteUInt64(friendId);
            }
        }

        SUCCESS_RESULT(result);
    }

    int UserSystem::GetFriendsInternal(SceNpAccountId accountId, WebApiUserContext* user, UInt32 offset, UInt32 limit,
        FriendsApi::ParameterToGetFriends::Filter filter,
        FriendsApi::ParameterToGetFriends::Order order,
        Common::IntrusivePtr<Common::Vector<SceNpAccountId> > &accountIdPtr, Int32& nextOffset, Int32& previousOffset)
    {
        nextOffset = -1;
        previousOffset = -1;

        Common::LibContext* libContextPtr = &WebApi::Instance()->m_cppWebapiLibCtx;

        int ret = SCE_OK;
        // GET /v1/users/{accountId}/friends
        Common::Transaction<Common::IntrusivePtr<GetFriendsResponse> > transaction;
        ret = transaction.start(libContextPtr);
        if (ret < 0)
        {
            return ret;
        }

        char errorBuf[WEB_API_ERROR_BUFFURE_SIZE] = {};
        SceNpWebApi2ResponseInformationOption respInfoOpt = {};
        respInfoOpt.pErrorObject = static_cast<char*>(errorBuf);
        respInfoOpt.errorObjectSize = WEB_API_ERROR_BUFFURE_SIZE;
        transaction.setResponseInformationOption(&respInfoOpt);

        FriendsApi::ParameterToGetFriends param;
        param.initialize(libContextPtr, std::to_string(accountId).c_str());
        param.setoffset(offset);
        param.setlimit(limit);
        param.setfilter(filter);
        param.setorder(order);

        ret = FriendsApi::getFriends(user->m_webapiUserCtxId, param, transaction);
        param.terminate();
        if (ret < 0)
        {
            transaction.getResponseInformationOption();

            /*std::string errorBuffer;

            errorBuffer = "getFriends()\n";
            errorBuffer += "httpStatus:";
            errorBuffer += std::to_string(respInfoOpt.httpStatus);
            errorBuffer += "\n";
            errorBuffer += "nerrorObject:";
            errorBuffer += respInfoOpt.pErrorObject;
            errorBuffer += "\n";
            printf("%s", errorBuffer.c_str());*/
            transaction.finish();
            return ret;
        }

        Common::IntrusivePtr<GetFriendsResponse> respPtr;

        ret = transaction.getResponse(respPtr);
        if (ret == SCE_OK)
        {
            accountIdPtr = respPtr->getFriends();

            if (respPtr->nextOffsetIsSet())
            {
                nextOffset = respPtr->getNextOffset();
            }

            if (respPtr->previousOffsetIsSet())
            {
                previousOffset = respPtr->getPreviousOffset();
            }
        }
        transaction.finish();
        return ret;
    }

    void UserSystem::WriteAvatarList(BinaryWriter& writer, Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<Avatar> > > avatarsPtr)
    {
        size_t numAvatars = avatarsPtr->size();

        writer.WriteUInt32(numAvatars);

        for (auto it = avatarsPtr->begin(); it != avatarsPtr->end(); ++it)
        {
            auto avatarPtr = (*it);

            bool sizeIsSet = avatarPtr->sizeIsSet();
            writer.WriteBool(sizeIsSet);
            if (sizeIsSet)
            {
                writer.WriteUInt32((UInt32)avatarPtr->getSize());  // AvatarSize _NOT_SET, XL, L, M, S
            }

            bool urlIsSet = avatarPtr->urlIsSet();
            writer.WriteBool(urlIsSet);
            if (urlIsSet)
            {
                writer.WriteString(avatarPtr->getUrl().c_str());
            }
        }
    }

    void UserSystem::GetProfilesImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = WebApi::Initialise();
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        *resultsSize = 0;

        //printf("UserSystem::GetFriendsImpl\n");

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        UserIds* user = s_UsersList.FindUser(userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with User service");
            return;
        }

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        UInt32 accountIdCount = reader.ReadUInt32();

        std::string accountIdsStr = "";

        if (accountIdCount == 0)
        {
            // If no account id's are set just retrieve the calling users profile
            accountIdsStr = std::to_string(user->m_accountId);
        }
        else
        {
            for (int i = 0; i < accountIdCount; i++)
            {
                UInt64 accountIds = reader.ReadUInt64();

                accountIdsStr += std::to_string(accountIds);

                if (i < accountIdCount - 1)
                {
                    accountIdsStr += ",";
                }
            }
        }

        Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<BasicProfile> > > profilesPtr;

        ret = GetProfilesInternal(accountIdsStr, userWebCtx, profilesPtr);

        if(ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Prepare to write results to output buffer
        BinaryWriter writer(resultsData, resultsMaxSize);

        if (profilesPtr.get() == nullptr || profilesPtr->size() <= 0)
        {
            writer.WriteUInt32(0); // count
        }
        else
        {
            size_t size = profilesPtr->size();

            writer.WriteUInt32(size);

            for (auto it = profilesPtr->begin(); it != profilesPtr->end(); ++it)
            {
                auto basicProfilePtr = (*it);

                bool onlineIdSet = basicProfilePtr->onlineIdIsSet();
                writer.WriteBool(onlineIdSet);
                if (onlineIdSet == true)
                {
                    writer.WriteString(basicProfilePtr->getOnlineId().data);
                }

                bool personalDetailsSet = basicProfilePtr->personalDetailIsSet();
                writer.WriteBool(personalDetailsSet);
                if (personalDetailsSet == true)
                {
                    Common::IntrusivePtr<PersonalDetail> personalDetailPtr = basicProfilePtr->getPersonalDetail();

                    bool firstNameIsSet = personalDetailPtr->firstNameIsSet();
                    writer.WriteBool(firstNameIsSet);
                    if (firstNameIsSet == true) writer.WriteString(personalDetailPtr->getFirstName().c_str());

                    bool middleNameIsSet = personalDetailPtr->middleNameIsSet();
                    writer.WriteBool(middleNameIsSet);
                    if (middleNameIsSet == true) writer.WriteString(personalDetailPtr->getMiddleName().c_str());

                    bool lastNameIsSet = personalDetailPtr->lastNameIsSet();
                    writer.WriteBool(lastNameIsSet);
                    if (lastNameIsSet == true) writer.WriteString(personalDetailPtr->getLastName().c_str());

                    bool displayNameIsSet = personalDetailPtr->displayNameIsSet();
                    writer.WriteBool(displayNameIsSet);
                    if (displayNameIsSet == true) writer.WriteString(personalDetailPtr->getDisplayName().c_str());

                    // todo profile image
                    bool profilePicturesIsSet = personalDetailPtr->profilePicturesIsSet();
                    writer.WriteBool(profilePicturesIsSet);
                    if (profilePicturesIsSet == true)
                    {
                        Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<Avatar> > > profilePicturesPtr = personalDetailPtr->getProfilePictures();

                        WriteAvatarList(writer, profilePicturesPtr);
                    }
                }

                bool aboutMeIsSet = basicProfilePtr->aboutMeIsSet();
                writer.WriteBool(aboutMeIsSet);
                if (aboutMeIsSet)
                {
                    writer.WriteString(basicProfilePtr->getAboutMe().c_str());
                }

                bool avatarsIsSet = basicProfilePtr->avatarsIsSet();
                writer.WriteBool(avatarsIsSet);
                if (avatarsIsSet)
                {
                    Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<Avatar> > > avatarsPtr = basicProfilePtr->getAvatars();

                    WriteAvatarList(writer, avatarsPtr);
                }

                bool languagesIsSet = basicProfilePtr->languagesIsSet();
                writer.WriteBool(languagesIsSet);
                if (languagesIsSet)
                {
                    Common::IntrusivePtr<Common::Vector<Common::String> > languagesPtr = basicProfilePtr->getLanguages();

                    size_t numlanguages = languagesPtr->size();

                    writer.WriteUInt32(numlanguages);

                    for (auto languagePtr = languagesPtr->begin(); languagePtr != languagesPtr->end(); ++languagePtr)
                    {
                        writer.WriteString(languagePtr->c_str());
                    }
                }

                bool isOfficiallyVerifiedSet = basicProfilePtr->isOfficiallyVerifiedIsSet();
                writer.WriteBool(isOfficiallyVerifiedSet);
                if (isOfficiallyVerifiedSet)
                {
                    writer.WriteBool(basicProfilePtr->getIsOfficiallyVerified());
                }
            }
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    int UserSystem::GetProfilesInternal(std::string accountIds, WebApiUserContext* user,
        Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<BasicProfile> > > &profilesPtr)
    {
        Common::LibContext* libContextPtr = &WebApi::Instance()->m_cppWebapiLibCtx;

        int ret = SCE_OK;
        // GET /v1/users/profiles
        Common::Transaction<Common::IntrusivePtr<GetPublicProfilesResponse> > transaction;
        ret = transaction.start(libContextPtr);
        if (ret < 0)
        {
            return ret;
        }

        char errorBuf[WEB_API_ERROR_BUFFURE_SIZE] = {};
        SceNpWebApi2ResponseInformationOption respInfoOpt = {};
        respInfoOpt.pErrorObject = static_cast<char*>(errorBuf);
        respInfoOpt.errorObjectSize = WEB_API_ERROR_BUFFURE_SIZE;
        transaction.setResponseInformationOption(&respInfoOpt);

        BasicProfileApi::ParameterToGetPublicProfiles param;
        ret = param.initialize(libContextPtr, accountIds.c_str());

        if (ret < 0)
        {
            transaction.finish();
            return ret;
        }

        ret = BasicProfileApi::getPublicProfiles(user->m_webapiUserCtxId, param, transaction);
        param.terminate();
        if (ret < 0)
        {
            transaction.getResponseInformationOption();

            /*  std::string errorBuffer;

                errorBuffer = "getProfiles()\n";
                errorBuffer += "httpStatus:";
                errorBuffer += std::to_string(respInfoOpt.httpStatus);
                errorBuffer += "\n";
                errorBuffer += "nerrorObject:";
                errorBuffer += respInfoOpt.pErrorObject;
                errorBuffer += "\n";
                printf("%s", errorBuffer.c_str());*/
            transaction.finish();
            return ret;
        }

        Common::IntrusivePtr<GetPublicProfilesResponse> respPtr;
        ret = transaction.getResponse(respPtr);
        if (ret == 0)
        {
            profilesPtr = respPtr->getProfiles();
        }

        transaction.finish();

        return ret;
    }

    void UserSystem::GetBasicPresencesImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = WebApi::Initialise();
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        *resultsSize = 0;

        //printf("UserSystem::GetBasicPresencesImpl\n");

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        UserIds* user = s_UsersList.FindUser(userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with User service");
            return;
        }

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        UInt32 accountIdCount = reader.ReadUInt32();

        std::string accountIdsStr = "";

        if (accountIdCount == 0)
        {
            WARNING_RESULT(result, "No user presences requested");
            return;
        }
        else
        {
            for (int i = 0; i < accountIdCount; i++)
            {
                UInt64 accountIds = reader.ReadUInt64();

                accountIdsStr += std::to_string(accountIds);

                if (i < accountIdCount - 1)
                {
                    accountIdsStr += ",";
                }
            }
        }

        Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<BasicPresence> > > presencesPtr;

        ret = GetBasicPresencesInternal(accountIdsStr, userWebCtx, presencesPtr);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // Prepare to write results to output buffer
        BinaryWriter writer(resultsData, resultsMaxSize);

        if (presencesPtr.get() == nullptr || presencesPtr->size() <= 0)
        {
            writer.WriteUInt32(0); // count
        }
        else
        {
            size_t size = presencesPtr->size();

            writer.WriteUInt32(size);

            for (auto it = presencesPtr->begin(); it != presencesPtr->end(); ++it)
            {
                auto basicPresencePtr = (*it);

                bool accountIdIsSet = basicPresencePtr->accountIdIsSet();
                writer.WriteBool(accountIdIsSet);
                if (accountIdIsSet == true)
                {
                    writer.WriteUInt64(basicPresencePtr->getAccountId());
                }

                bool onlineStatusIsSet = basicPresencePtr->onlineStatusIsSet();
                writer.WriteBool(onlineStatusIsSet);
                if (onlineStatusIsSet == true)
                {
                    writer.WriteInt32((Int32)basicPresencePtr->getOnlineStatus());
                }

                bool inContextIsSet = basicPresencePtr->inContextIsSet();
                writer.WriteBool(inContextIsSet);
                if (inContextIsSet == true)
                {
                    writer.WriteBool(basicPresencePtr->getInContext());
                }
            }
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    int UserSystem::GetBasicPresencesInternal(std::string accountIds, WebApiUserContext* user,
        Common::IntrusivePtr<Common::Vector<Common::IntrusivePtr<BasicPresence> > > &presencesPtr)
    {
        Common::LibContext* libContextPtr = &WebApi::Instance()->m_cppWebapiLibCtx;

        int ret = SCE_OK;
        // GET /v1/users/profiles
        Common::Transaction<Common::IntrusivePtr<GetBasicPresencesResponse> > transaction;
        ret = transaction.start(libContextPtr);
        if (ret < 0)
        {
            return ret;
        }

        char errorBuf[WEB_API_ERROR_BUFFURE_SIZE] = {};
        SceNpWebApi2ResponseInformationOption respInfoOpt = {};
        respInfoOpt.pErrorObject = static_cast<char*>(errorBuf);
        respInfoOpt.errorObjectSize = WEB_API_ERROR_BUFFURE_SIZE;
        transaction.setResponseInformationOption(&respInfoOpt);

        PresenceApi::ParameterToGetBasicPresences param;
        ret = param.initialize(libContextPtr, accountIds.c_str());

        if (ret < 0)
        {
            transaction.finish();
            return ret;
        }

        ret = PresenceApi::getBasicPresences(user->m_webapiUserCtxId, param, transaction);
        param.terminate();
        if (ret < 0)
        {
            transaction.getResponseInformationOption();

            /*std::string errorBuffer;

            errorBuffer = "getBasicPresences()\n";
            errorBuffer += "httpStatus:";
            errorBuffer += std::to_string(respInfoOpt.httpStatus);
            errorBuffer += "\n";
            errorBuffer += "nerrorObject:";
            errorBuffer += respInfoOpt.pErrorObject;
            errorBuffer += "\n";
            printf("%s", errorBuffer.c_str());*/
            transaction.finish();
            return ret;
        }

        Common::IntrusivePtr<GetBasicPresencesResponse> respPtr;
        ret = transaction.getResponse(respPtr);
        if (ret == 0)
        {
            presencesPtr = respPtr->getBasicPresences();
        }

        transaction.finish();

        return ret;
    }

    void UserSystem::GetBlockingUsersImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        int ret = WebApi::Initialise();
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        *resultsSize = 0;

        //  printf("UserSystem::GetBlockingUsersImpl\n");

        BinaryReader reader(sourceData, sourceSize);

        Int32 userId = reader.ReadInt32();

        UserIds* user = s_UsersList.FindUser(userId);

        if (user == NULL)
        {
            WARNING_RESULT(result, "User not registered with User service");
            return;
        }

        WebApiUserContext* userWebCtx = WebApi::Instance()->FindUser(userId);

        if (userWebCtx == NULL)
        {
            WARNING_RESULT(result, "User not registered with WebApi");
            return;
        }

        UInt32 offset = reader.ReadInt32();
        UInt32 limit = reader.ReadInt32();

        // Prepare to write results to output buffer
        BinaryWriter writer(resultsData, resultsMaxSize);

        ret = GetBlockingUsersInternal(userWebCtx, offset, limit, writer);

        if(ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    int UserSystem::GetBlockingUsersInternal(WebApiUserContext* user, UInt32 offset, UInt32 limit, BinaryWriter& writer) // Common::IntrusivePtr<Common::Vector<SceNpAccountId>> &blocksPtr)
    {
        Common::LibContext* libContextPtr = &WebApi::Instance()->m_cppWebapiLibCtx;

        int ret = SCE_OK;

        Common::Transaction<Common::IntrusivePtr<GetBlockingUsersResponse> > transaction;
        ret = transaction.start(libContextPtr);
        if (ret < 0)
        {
            return ret;
        }

        char errorBuf[WEB_API_ERROR_BUFFURE_SIZE] = {};
        SceNpWebApi2ResponseInformationOption respInfoOpt = {};
        respInfoOpt.pErrorObject = static_cast<char*>(errorBuf);
        respInfoOpt.errorObjectSize = WEB_API_ERROR_BUFFURE_SIZE;
        transaction.setResponseInformationOption(&respInfoOpt);

        BlocksApi::ParameterToGetBlockingUsers param;
        param.initialize(libContextPtr);
        param.setoffset(offset);
        param.setlimit(limit);

        ret = BlocksApi::getBlockingUsers(user->m_webapiUserCtxId, param, transaction);
        param.terminate();
        if (ret < 0)
        {
            transaction.getResponseInformationOption();

            //std::string errorBuffer;

            //errorBuffer = "getBlockingUsers()\n";
            //errorBuffer += "httpStatus:";
            //errorBuffer += std::to_string(respInfoOpt.httpStatus);
            //errorBuffer += "\n";
            //errorBuffer += "nerrorObject:";
            //errorBuffer += respInfoOpt.pErrorObject;
            //errorBuffer += "\n";
            //printf("%s", errorBuffer.c_str());
            transaction.finish();
            return ret;
        }

        Common::IntrusivePtr<GetBlockingUsersResponse> respPtr;
        ret = transaction.getResponse(respPtr);
        if (ret < 0)
        {
            return ret;
        }

        Common::IntrusivePtr<Common::Vector<SceNpAccountId> > blocksPtr;
        blocksPtr = respPtr->getBlocks();

        if (blocksPtr.get() == nullptr || blocksPtr->size() <= 0)
        {
            writer.WriteUInt32(0); // count
        }
        else
        {
            size_t size = blocksPtr->size();

            writer.WriteUInt32(size);

            for (auto it = blocksPtr->begin(); it != blocksPtr->end(); ++it)
            {
                SceNpAccountId friendId = (*it);
                writer.WriteUInt64(friendId);
            }
        }

        if (respPtr->nextOffsetIsSet())
        {
            writer.WriteInt32(respPtr->getNextOffset());
        }
        else
        {
            writer.WriteInt32(-1);
        }

        if (respPtr->previousOffsetIsSet())
        {
            writer.WriteInt32(respPtr->getPreviousOffset());
        }
        else
        {
            writer.WriteInt32(-1);
        }

        writer.WriteInt32(respPtr->getTotalItemCount());

        transaction.finish();

        return ret;
    }

#if !__ORBIS__
    void UserSystem::SignedStateCallback(SceUserServiceUserId userId, SceNpState state, void *userData)
    {
        SigninStateEvent event;

        event.userId = userId;
        event.state = state;

        s_PendingSigninStateList.push_back(event);
    }

    void UserSystem::StartSignedStateCallbackImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        if (s_SignInCallbackId != -1)
        {
            ERROR_RESULT(result, "Signed In callback already registered");
            return;
        }

        int ret = sceNpRegisterStateCallbackA(SignedStateCallback, NULL);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        s_SignInCallbackId = ret;

        SUCCESS_RESULT(result);
    }

    void UserSystem::StopSignedStateCallbackImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        if (s_SignInCallbackId == -1)
        {
            ERROR_RESULT(result, "Signed In callback not registered");
            return;
        }

        int ret = sceNpUnregisterStateCallbackA(s_SignInCallbackId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        s_SignInCallbackId = -1;

        SUCCESS_RESULT(result);
    }

    void UserSystem::FetchSignedStateEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        if (s_PendingSigninStateList.empty() == true)
        {
            *resultsSize = 0;
            SUCCESS_RESULT(result);
            return;
        }

        // Pop the first event off the list and return the results
        SigninStateEvent event = s_PendingSigninStateList.front();
        s_PendingSigninStateList.pop_front();

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(event.userId);
        writer.WriteInt32(event.state);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

    void UserSystem::ReachabilityStateCallback(SceUserServiceUserId userId, SceNpReachabilityState state, void *userData)
    {
        ReachabilityStateEvent event;

        event.userId = userId;
        event.state = state;

        s_PendingReachabilityStateList.push_back(event);
    }

    void UserSystem::StartReachabilityStateCallbackImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        int ret = sceNpRegisterNpReachabilityStateCallback(ReachabilityStateCallback, NULL);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void UserSystem::StopReachabilityStateCallbackImpl(UInt8* sourceData, int sourceSize, APIResult* result)
    {
        int ret = sceNpUnregisterNpReachabilityStateCallback();
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        SUCCESS_RESULT(result);
    }

    void UserSystem::FetchReachabilityStateEventImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        if (s_PendingReachabilityStateList.empty() == true)
        {
            *resultsSize = 0;
            SUCCESS_RESULT(result);
            return;
        }

        // Pop the first event off the list and return the results
        ReachabilityStateEvent event = s_PendingReachabilityStateList.front();
        s_PendingReachabilityStateList.pop_front();

        // Write data to output
        BinaryWriter writer(resultsData, resultsMaxSize);

        writer.WriteInt32(event.userId);
        writer.WriteInt32(event.state);

        *resultsSize = writer.GetWrittenLength();

        SUCCESS_RESULT(result);
    }

#endif
}
