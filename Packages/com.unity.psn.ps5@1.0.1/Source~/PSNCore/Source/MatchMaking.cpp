#include "MatchMaking.h"
#include "HandleMsg.h"

namespace psn
{
    void MatchMakingSystem::RegisterMethods()
    {
        MsgHandler::AddMethod(Methods::SubmitTicket, MatchMakingSystem::SubmitTicketImpl);
        MsgHandler::AddMethod(Methods::GetTicket, MatchMakingSystem::GetTicketImpl);
        MsgHandler::AddMethod(Methods::CancelTicket, MatchMakingSystem::CancelTicketImpl);
        MsgHandler::AddMethod(Methods::GetOffer, MatchMakingSystem::GetOfferImpl);
        MsgHandler::AddMethod(Methods::ListUserTickets, MatchMakingSystem::ListUserTicketsImpl);
    }

    void MatchMakingSystem::Initialize()
    {
    }

    void MatchMakingSystem::SubmitTicketImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        char* rulesetName = reader.ReadStringPtr();


        // prepare params
        Vector<IntrusivePtr<PlayerForTicketCreate> > playersForTicketCreate(libContextPtr);

        // Loop around each player to add
        int numPlayers = reader.ReadInt32();

        for (int i = 0; i < numPlayers; i++)
        {
            SceNpAccountId accountId = reader.ReadUInt64();
            SceNpPlatformType platformType = reader.ReadInt32();

            IntrusivePtr<PlayerForTicketCreate> playerForTicketCreate;
            Platform platform = GetPlatformType(platformType);

            ret = PlayerForTicketCreateFactory::create(libContextPtr, accountId, platform, &playerForTicketCreate);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }

            bool isNameSet = reader.ReadBool();

            if (isNameSet == true)
            {
                char* teamName = reader.ReadStringPtr();

                ret = playerForTicketCreate->setTeamName(teamName);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }
            }

            bool isNatTypeSet = reader.ReadBool();
            if (isNatTypeSet)
            {
                // set natType
                Int32 natType = reader.ReadInt32();
                playerForTicketCreate->setNatType(natType);
            }

            // set playerAttributes
            Int32 numAttributes = reader.ReadInt32();

            if (numAttributes > 0)
            {
                Vector<IntrusivePtr<Attribute> > playerAttributes(libContextPtr);

                ret = ReadAttributes(reader, playerAttributes, numAttributes);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }

                ret = playerForTicketCreate->setPlayerAttributes(playerAttributes);
                if (ret < 0)
                {
                    SCE_ERROR_RESULT(result, ret);
                    return;
                }
            }

            // add to Vector
            ret = playersForTicketCreate.pushBack(playerForTicketCreate);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        IntrusivePtr<SubmitTicketRequestBody> submitTicketRequestBody;
        ret = SubmitTicketRequestBodyFactory::create(libContextPtr, rulesetName, playersForTicketCreate, &submitTicketRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        Int32 numTicketAttributes = reader.ReadInt32();

        if (numTicketAttributes > 0)
        {
            Common::Vector<Common::IntrusivePtr<Attribute> > ticketAttributes(libContextPtr);

            ret = ReadAttributes(reader, ticketAttributes, numTicketAttributes);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }

            ret = submitTicketRequestBody->setTicketAttributes(ticketAttributes);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        // Add Game Session ID here
        bool isGameSessionIdSet = reader.ReadBool();

        if (isGameSessionIdSet)
        {
            char* gameSessionId = reader.ReadStringPtr();

            IntrusivePtr<Location> location;
            ret = LocationFactory::create(libContextPtr, &location);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }

            ret = location->setGameSessionId(gameSessionId);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
            ret = submitTicketRequestBody->setLocation(location);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        Api::ParameterToSubmitTicket param;
        ret = param.initialize(libContextPtr, submitTicketRequestBody);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        typedef Common::IntrusivePtr<SubmitTicketResponseBody> SubmitTicketResponseBody;
        SubmitTicketResponseBody response;

        Common::Transaction<SubmitTicketResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = Api::submitTicket(userCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        sce::Np::CppWebApi::Matchmaking::V1::SubmitTicketResponseBody* body = response.get();

        WriteTicketResponse(writer, body);

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void MatchMakingSystem::GetTicketImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        char* ticketId = reader.ReadStringPtr();

        Api::ParameterToGetTicket param;
        ret = param.initialize(libContextPtr, ticketId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        char* view = reader.ReadStringPtr();

        ret = param.setview(view);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        typedef Common::IntrusivePtr<GetTicketResponseBody> GetTicketResponseBody;
        GetTicketResponseBody response;

        Common::Transaction<GetTicketResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = Api::getTicket(userCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        sce::Np::CppWebApi::Matchmaking::V1::GetTicketResponseBody* body = response.get();

        WriteTicketResponse(writer, body);

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void MatchMakingSystem::CancelTicketImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        char* ticketId = reader.ReadStringPtr();

        Api::ParameterToCancelTicket param;
        ret = param.initialize(libContextPtr, ticketId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        DefaultResponse response;
        Common::Transaction<DefaultResponse> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = Api::cancelTicket(userCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void MatchMakingSystem::GetOfferImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        char* offerId = reader.ReadStringPtr();

        if (offerId == NULL)
        {
            ERROR_RESULT(result, "OfferId is null");
            return;
        }

        Api::ParameterToGetOffer param;
        ret = param.initialize(libContextPtr, offerId);
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        typedef Common::IntrusivePtr<GetOfferResponseBody> GetOfferResponseBody;
        GetOfferResponseBody response;

        Common::Transaction<GetOfferResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = Api::getOffer(userCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        sce::Np::CppWebApi::Matchmaking::V1::GetOfferResponseBody* body = response.get();

        WriteOfferResponse(writer, body);

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    void MatchMakingSystem::ListUserTicketsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result)
    {
        *resultsSize = 0;

        int ret = 0;

        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        BinaryReader reader(sourceData, sourceSize);
        Int32 userId = reader.ReadInt32();

        WebApiUserContext* userCtx = WebApi::Instance()->FindUser(userId);

        if (userCtx == NULL)
        {
            ERROR_RESULT(result, "Can't find user context id");
            return;
        }

        Api::ParameterToListUserTickets param;
        ret = param.initialize(libContextPtr, "me");
        if (ret < 0)
        {
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        bool isPlatformFilterSet = reader.ReadBool();
        if (isPlatformFilterSet)
        {
            char* platformFilter = reader.ReadStringPtr();
            ret = param.setplatformFilter(platformFilter);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        bool isRulesetFilterSet = reader.ReadBool();
        if (isRulesetFilterSet)
        {
            char* rulesetFilter = reader.ReadStringPtr();
            ret = param.setrulesetFilter(rulesetFilter);
            if (ret < 0)
            {
                SCE_ERROR_RESULT(result, ret);
                return;
            }
        }

        typedef Common::IntrusivePtr<ListUserTicketsResponseBody> ListUserTicketsResponseBody;
        ListUserTicketsResponseBody response;

        Common::Transaction<ListUserTicketsResponseBody> transaction;
        transaction.start(libContextPtr);

        // API call
        ret = Api::listUserTickets(userCtx->m_webapiUserCtxId, param, transaction);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        // getResponse
        ret = transaction.getResponse(response);
        if (ret < 0)
        {
            param.terminate();
            transaction.finish();
            SCE_ERROR_RESULT(result, ret);
            return;
        }

        BinaryWriter writer(resultsData, resultsMaxSize);

        sce::Np::CppWebApi::Matchmaking::V1::ListUserTicketsResponseBody* body = response.get();

        if (body != NULL)
        {
            Common::Vector<Common::IntrusivePtr<UserTicket> >* userTickets = body->getTickets().get();

            if (userTickets != NULL)
            {
                writer.WriteInt32(userTickets->size());

                for (auto& it : *userTickets)
                {
                    if (it->ticketIdIsSet())
                    {
                        writer.WriteBool(true);
                        writer.WriteString(it->getTicketId().c_str());
                    }
                    else
                    {
                        writer.WriteBool(false);
                    }

                    if (it->statusIsSet())
                    {
                        writer.WriteBool(true);
                        writer.WriteInt32((Int32)it->getStatus());
                    }
                    else
                    {
                        writer.WriteBool(false);
                    }

                    if (it->rulesetNameIsSet())
                    {
                        writer.WriteBool(true);
                        writer.WriteString(it->getRulesetName().c_str());
                    }
                    else
                    {
                        writer.WriteBool(false);
                    }

                    if (it->platformIsSet())
                    {
                        writer.WriteBool(true);
                        writer.WriteInt32((Int32)GetPlatformType(it->getPlatform()));
                    }
                    else
                    {
                        writer.WriteBool(false);
                    }
                }
            }
            else
            {
                writer.WriteInt32(0); // No tickets.
            }
        }
        else
        {
            writer.WriteInt32(0); // No tickets.
        }

        *resultsSize = writer.GetWrittenLength();

        param.terminate();
        transaction.finish();

        SUCCESS_RESULT(result);
    }

    int MatchMakingSystem::WriteTicketResponse(BinaryWriter& writer, sce::Np::CppWebApi::Matchmaking::V1::SubmitTicketResponseBody* body)
    {
        if (body->ticketIdIsSet())
        {
            writer.WriteBool(true);
            writer.WriteString(body->getTicketId().c_str());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->rulesetNameIsSet())
        {
            writer.WriteBool(true);
            writer.WriteString(body->getRulesetName().c_str());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->ticketAttributesIsSet())
        {
            WriteAttributes(writer, body->getTicketAttributes().get());
        }
        else
        {
            WriteAttributes(writer, NULL);
        }

        if (body->playersIsSet())
        {
            WritePlayers(writer, body->getPlayers().get());
        }
        else
        {
            WritePlayers(writer, NULL);
        }

        if (body->statusIsSet())
        {
            writer.WriteBool(true);
            writer.WriteInt32((Int32)body->getStatus());
        }
        else
        {
            writer.WriteBool(false);
        }

        writer.WriteBool(false); // Never has an OfferId

        if (body->submitterIsSet())
        {
            writer.WriteBool(true);
            WriteSubmitter(writer, body->getSubmitter().get());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->createdDateTimeIsSet())
        {
            writer.WriteBool(true);
            writer.WriteRtcTick(body->getCreatedDateTime());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->updatedDateTimeIsSet())
        {
            writer.WriteBool(true);
            writer.WriteRtcTick(body->getUpdatedDateTime());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->locationIsSet())
        {
            writer.WriteBool(true);
            WriteLocation(writer, body->getLocation().get());
        }
        else
        {
            writer.WriteBool(false);
        }

        return 0;
    }

    int MatchMakingSystem::WriteTicketResponse(BinaryWriter& writer, sce::Np::CppWebApi::Matchmaking::V1::GetTicketResponseBody* body)
    {
        if (body->ticketIdIsSet())
        {
            writer.WriteBool(true);
            writer.WriteString(body->getTicketId().c_str());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->rulesetNameIsSet())
        {
            writer.WriteBool(true);
            writer.WriteString(body->getRulesetName().c_str());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->ticketAttributesIsSet())
        {
            WriteAttributes(writer, body->getTicketAttributes().get());
        }
        else
        {
            WriteAttributes(writer, NULL);
        }

        if (body->playersIsSet())
        {
            WritePlayersForRead(writer, body->getPlayers().get());
        }
        else
        {
            WritePlayersForRead(writer, NULL);
        }

        if (body->statusIsSet())
        {
            writer.WriteBool(true);
            writer.WriteInt32((Int32)body->getStatus());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->offerIdIsSet())
        {
            writer.WriteBool(true);
            writer.WriteString(body->getOfferId().c_str());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->submitterIsSet())
        {
            writer.WriteBool(true);
            WriteSubmitter(writer, body->getSubmitter().get());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->createdDateTimeIsSet())
        {
            writer.WriteBool(true);
            writer.WriteRtcTick(body->getCreatedDateTime());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->updatedDateTimeIsSet())
        {
            writer.WriteBool(true);
            writer.WriteRtcTick(body->getUpdatedDateTime());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->locationIsSet())
        {
            writer.WriteBool(true);
            WriteLocation(writer, body->getLocation().get());
        }
        else
        {
            writer.WriteBool(false);
        }

        return 0;
    }

    int MatchMakingSystem::WriteOfferResponse(BinaryWriter& writer, sce::Np::CppWebApi::Matchmaking::V1::GetOfferResponseBody* body)
    {
        if (body->offerIdIsSet())
        {
            writer.WriteBool(true);
            writer.WriteString(body->getOfferId().c_str());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->rulesetNameIsSet())
        {
            writer.WriteBool(true);
            writer.WriteString(body->getRulesetName().c_str());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->playersIsSet())
        {
            WritePlayersForOfferRead(writer, body->getPlayers().get());
        }
        else
        {
            WritePlayersForOfferRead(writer, NULL);
        }

        if (body->statusIsSet())
        {
            writer.WriteBool(true);
            writer.WriteInt32((Int32)body->getStatus());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->locationIsSet())
        {
            writer.WriteBool(true);
            WriteLocation(writer, body->getLocation().get());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->createdDateTimeIsSet())
        {
            writer.WriteBool(true);
            writer.WriteRtcTick(body->getCreatedDateTime());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (body->updatedDateTimeIsSet())
        {
            writer.WriteBool(true);
            writer.WriteRtcTick(body->getUpdatedDateTime());
        }
        else
        {
            writer.WriteBool(false);
        }

        return 0;
    }

    Platform MatchMakingSystem::GetPlatformType(SceNpPlatformType platformType)
    {
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
        if (platformType == SCE_NP_PLATFORM_TYPE_PS4)
        {
            return Platform::kPs4;
        }

        if (platformType == SCE_NP_PLATFORM_TYPE_PS5)
        {
            return Platform::kPs5;
        }
#else
        if (platformType == SCE_NP_PLATFORM_TYPE_PS4)
        {
            return Platform::PS4;
        }

        if (platformType == SCE_NP_PLATFORM_TYPE_PS5)
        {
            return Platform::PS5;
        }
#endif

        return Platform::_NOT_SET;
    }

    SceNpPlatformType MatchMakingSystem::GetPlatformType(Platform platform)
    {
#if (SCE_PROSPERO_SDK_VERSION > 0x05000000u) || (SCE_ORBIS_SDK_VERSION > 0x09500000u)
        if (platform == Platform::kPs4)
        {
            return SCE_NP_PLATFORM_TYPE_PS4;
        }

        if (platform == Platform::kPs5 || platform == Platform::kProspero)
        {
            return SCE_NP_PLATFORM_TYPE_PS5;
        }
#else
        if (platform == Platform::PS4)
        {
            return SCE_NP_PLATFORM_TYPE_PS4;
        }

        if (platform == Platform::PS5 || platform == Platform::PROSPERO)
        {
            return SCE_NP_PLATFORM_TYPE_PS5;
        }
#endif
        return SCE_NP_PLATFORM_TYPE_NONE;
    }

    int MatchMakingSystem::ReadAttributes(BinaryReader &reader, Common::Vector<Common::IntrusivePtr<Attribute> >& attributes, int numAttributes)
    {
        Common::LibContext* libContextPtr = WebApi::Instance()->GetLibCtx();

        for (int a = 0; a < numAttributes; a++)
        {
            char* name = reader.ReadStringPtr();
            Int32 attributeType = reader.ReadInt32();
            char* value = reader.ReadStringPtr();

            IntrusivePtr<Attribute> attr;
            int ret = AttributeFactory::create(libContextPtr, name, static_cast<AttributeType>(attributeType), value, &attr);
            if (ret < 0)
            {
                return ret;
            }

            ret = attributes.pushBack(attr);
            if (ret < 0)
            {
                return ret;
            }
        }

        return 0;
    }

    int MatchMakingSystem::WriteAttributes(BinaryWriter& writer, Common::Vector<Common::IntrusivePtr<Attribute> >* attributes)
    {
        if (attributes == NULL)
        {
            writer.WriteInt32(0);
            return 0;
        }

        writer.WriteInt32(attributes->size());

        for (auto& it : *attributes)
        {
            writer.WriteString(it->getName().c_str());
            writer.WriteInt32((Int32)it->getType());
            writer.WriteString(it->getValue().c_str());
        }

        return 0;
    }

    // Must match the same output format as WritePlayersForRead
    int MatchMakingSystem::WritePlayers(BinaryWriter& writer, Common::Vector<Common::IntrusivePtr<PlayerForTicketCreate> >* players)
    {
        if (players == NULL)
        {
            writer.WriteInt32(0);
            return 0;
        }

        writer.WriteInt32(players->size());

        for (auto& it : *players)
        {
            writer.WriteBool(true); // Always has an account id
            writer.WriteUInt64(it->getAccountId());

            writer.WriteBool(false); // Never has an Online id

            writer.WriteBool(true); // Always has a platform
            writer.WriteInt32((Int32)GetPlatformType(it->getPlatform()));

            if (it->teamNameIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(it->getTeamName().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->natTypeIsSet())
            {
                writer.WriteBool(true);
                writer.WriteInt32(it->getNatType());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->playerAttributesIsSet())
            {
                WriteAttributes(writer, it->getPlayerAttributes().get());
            }
            else
            {
                WriteAttributes(writer, NULL);
            }
        }

        return 0;
    }

    // Must match the same output format as WritePlayers
    int MatchMakingSystem::WritePlayersForRead(BinaryWriter& writer, Common::Vector<Common::IntrusivePtr<PlayerForRead> >* players)
    {
        if (players == NULL)
        {
            writer.WriteInt32(0);
            return 0;
        }

        writer.WriteInt32(players->size());

        for (auto& it : *players)
        {
            if (it->accountIdIsSet())
            {
                writer.WriteBool(true);
                writer.WriteUInt64(it->getAccountId());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->onlineIdIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(it->getOnlineId().data);
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->platformIsSet())
            {
                writer.WriteBool(true);
                writer.WriteInt32((Int32)GetPlatformType(it->getPlatform()));
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->teamNameIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(it->getTeamName().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->natTypeIsSet())
            {
                writer.WriteBool(true);
                writer.WriteInt32(it->getNatType());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->playerAttributesIsSet())
            {
                WriteAttributes(writer, it->getPlayerAttributes().get());
            }
            else
            {
                WriteAttributes(writer, NULL);
            }
        }

        return 0;
    }

    int MatchMakingSystem::WritePlayersForOfferRead(BinaryWriter& writer, Common::Vector<Common::IntrusivePtr<PlayerForOfferRead> >* players)
    {
        if (players == NULL)
        {
            writer.WriteInt32(0);
            return 0;
        }

        writer.WriteInt32(players->size());

        for (auto& it : *players)
        {
            if (it->accountIdIsSet())
            {
                writer.WriteBool(true);
                writer.WriteUInt64(it->getAccountId());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->onlineIdIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(it->getOnlineId().data);
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->platformIsSet())
            {
                writer.WriteBool(true);
                writer.WriteInt32((Int32)GetPlatformType(it->getPlatform()));
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->teamNameIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(it->getTeamName().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }

            if (it->ticketIdIsSet())
            {
                writer.WriteBool(true);
                writer.WriteString(it->getTicketId().c_str());
            }
            else
            {
                writer.WriteBool(false);
            }
        }

        return 0;
    }

    int MatchMakingSystem::WriteSubmitter(BinaryWriter& writer, Submitter* submitter)
    {
        if (submitter->accountIdIsSet())
        {
            writer.WriteBool(true);
            writer.WriteUInt64(submitter->getAccountId());
        }
        else
        {
            writer.WriteBool(false);
        }

        if (submitter->platformIsSet())
        {
            writer.WriteBool(true);
            writer.WriteInt32((Int32)GetPlatformType(submitter->getPlatform()));
        }
        else
        {
            writer.WriteBool(false);
        }

        return 0;
    }

    int MatchMakingSystem::WriteLocation(BinaryWriter& writer, Location* location)
    {
        if (location->gameSessionIdIsSet())
        {
            writer.WriteBool(true);
            writer.WriteString(location->getGameSessionId().c_str());
        }
        else
        {
            writer.WriteBool(false);
        }

        return 0;
    }
}
