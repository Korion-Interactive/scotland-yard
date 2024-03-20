#pragma once
#include "SharedCoreIncludes.h"
#include <map>
#include <np_cppwebapi.h>
#include "WebApi.h"

namespace sceCppWebApi = sce::Np::CppWebApi::Common;
namespace matchmaking = sce::Np::CppWebApi::Matchmaking::V1;

using namespace sceCppWebApi;
using namespace sce::Np::CppWebApi;
using namespace matchmaking;

namespace psn
{
    class MatchMakingSystem
    {
    public:

        enum Methods
        {
            SubmitTicket = 0x0D00001u,
            GetTicket = 0x0D00002u,
            CancelTicket = 0x0D00003u,
            GetOffer = 0x0D00004u,
            ListUserTickets = 0x0D00005u,
        };

        static void RegisterMethods();

        static void Initialize();

        static void SubmitTicketImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetTicketImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void CancelTicketImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetOfferImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void ListUserTicketsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static Platform GetPlatformType(SceNpPlatformType platformType);
        static SceNpPlatformType GetPlatformType(Platform platform);

        static int ReadAttributes(BinaryReader &reader, Common::Vector<Common::IntrusivePtr<Attribute> >& attributes, int numAttributes);

        static int WriteTicketResponse(BinaryWriter& writer, sce::Np::CppWebApi::Matchmaking::V1::SubmitTicketResponseBody* body);
        static int WriteTicketResponse(BinaryWriter& writer, sce::Np::CppWebApi::Matchmaking::V1::GetTicketResponseBody* body);
        static int WriteOfferResponse(BinaryWriter& writer, sce::Np::CppWebApi::Matchmaking::V1::GetOfferResponseBody* body);

        static int WriteAttributes(BinaryWriter& writer, Common::Vector<Common::IntrusivePtr<Attribute> >* attributes);
        static int WritePlayers(BinaryWriter& writer, Common::Vector<Common::IntrusivePtr<PlayerForTicketCreate> >* players);
        static int WritePlayersForRead(BinaryWriter& writer, Common::Vector<Common::IntrusivePtr<PlayerForRead> >* players);
        static int WritePlayersForOfferRead(BinaryWriter& writer, Common::Vector<Common::IntrusivePtr<PlayerForOfferRead> >* players);

        static int WriteLocation(BinaryWriter& writer, Location* location);
        static int WriteSubmitter(BinaryWriter& writer, Submitter* submitter);
    };
}
