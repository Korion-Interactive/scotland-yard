#pragma once
#include "SharedCoreIncludes.h"
#include "WebApiNotifications.h"
#include <map>
#include <np_cppwebapi.h>
#include <vector>

namespace sceCppWebApi = sce::Np::CppWebApi::Common;
namespace sceMatches = sce::Np::CppWebApi::Matches::V1;

using namespace sceCppWebApi;
using namespace sceMatches;

namespace psn
{
    class MatchesCommands
    {
    public:

        enum Methods
        {
            CreateMatch = 0x0C00001u,
            GetMatchDetails = 0x0C00002u,
            UpdateMatchStatus = 0x0C00003u,
            JoinMatch = 0x0C00004u,
            LeaveMatch = 0x0C00005u,
            ReportResults = 0x0C00006u,
            UpdateDetails = 0x0C00007u,
        };

        struct MatchPlayer
        {
            char* m_PlayerId;
            char* m_PlayerName;
            SceNpAccountId m_AccountId;
            PlayerType m_PlayerType;
            void Deserialise(BinaryReader& reader);
        };

        struct MatchTeamMember
        {
            char* m_PlayerId;
            void Deserialise(BinaryReader& reader);
        };

        struct MatchTeam
        {
            char* m_TeamId;
            char* m_TeamName;
            std::vector<MatchTeamMember> m_Members;

            void Deserialise(BinaryReader& reader);
        };

        class InitializationParams
        {
        public:
            InitializationParams();
            ~InitializationParams();

            char* m_ActivityId;

            int32_t m_ServiceLabel;

            char* m_ZoneId;

            std::vector<MatchPlayer> m_Players;
            std::vector<MatchTeam> m_Teams;

            void Deserialise(BinaryReader& reader);
        };

        static void RegisterMethods();

        static void InitializeLib();
        static void TerminateLib();

        static void CreateMatchImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void GetMatchDetailsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void UpdateMatchStatusImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void JoinMatchImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void LeaveMatchImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void ReportResultsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);
        static void UpdateDetailsImpl(UInt8* sourceData, int sourceSize, UInt8* resultsData, int resultsMaxSize, int* resultsSize, APIResult* result);

        static void SerialiseMatchDetail(BinaryWriter& writer, IntrusivePtr<GetMatchDetailResponse> matchDetail);

        static void SerialiseGameRoster(BinaryWriter& writer, IntrusivePtr<ResponseInGameRoster> roster);
        static void SerialiseMatchResults(BinaryWriter& writer, IntrusivePtr<ResponseMatchResults> matchResults);
        static void SerialiseMatchStats(BinaryWriter& writer, IntrusivePtr<ResponseMatchStatistics> matchStats);

        static void SerialisePlayers(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<ResponseMatchPlayer> > > players);
        static void SerialiseTeams(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<ResponseMatchTeam> > > teams);
        static void SerialisePlayerResults(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<ResponsePlayerResults> > > playerResults);

        static void SerialiseTeamResults(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<ResponseTeamResults> > > teamResults);
        static void SerialiseTeamMemberResults(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<ResponseTeamMemberResult> > > teamMemberResults);

        static void SerialiseAdditionalStats(BinaryWriter& writer, IntrusivePtr<Vector<IntrusivePtr<AdditionalStatistic> > > additionalStats);

        static int Create(Int32 userId, InitializationParams& initParams, BinaryWriter& writer);

        static int DeserialiseMatchResults(Common::LibContext* libContextPtr, BinaryReader &reader, CompetitionType competition, GroupingType groupType, ResultType resultType, Common::IntrusivePtr<RequestMatchResults> &requestMatchResults);
        static int DeserialiseTemporaryMatchResults(Common::LibContext* libContextPtr, BinaryReader &reader, GroupingType groupType, ResultType resultType, Common::IntrusivePtr<RequestTemporaryMatchResults> &requestMatchResults);

        static int DeserialiseCompetitiveResults(Common::LibContext* libContextPtr, BinaryReader &reader, GroupingType groupType, ResultType resultType, IntrusivePtr<RequestCompetitiveResult> &requestCompetitiveResult);
        static int DeserialiseTemporaryCompetitiveResults(Common::LibContext* libContextPtr, BinaryReader &reader, GroupingType groupType, ResultType resultType, IntrusivePtr<RequestTemporaryCompetitiveResult> &requestCompetitiveResult);

        static int DeserialiseMatchStats(Common::LibContext* libContextPtr, BinaryReader &reader, CompetitionType competition, GroupingType groupType, ResultType resultType, IntrusivePtr<RequestMatchStatistics> &requestMatchStatistics);

        static int DeserialisePlayerStats(Common::LibContext* libContextPtr, BinaryReader &reader, GroupingType groupType, ResultType resultType, IntrusivePtr<RequestMatchStatistics> &requestMatchStatistics);
        static int DeserialiseTeamStats(Common::LibContext* libContextPtr, BinaryReader &reader, GroupingType groupType, ResultType resultType, IntrusivePtr<RequestMatchStatistics> &requestMatchStatistics);
        static int DeserialiseTeamMemberStats(Common::LibContext* libContextPtr, BinaryReader &reader, IntrusivePtr<RequestTeamStatistic>& requestTeamStatistic);

        static int DeserialiseAdditionalStats(Common::LibContext* libContextPtr, BinaryReader &reader, Vector<IntrusivePtr<AdditionalStatistic> >& stats);
    };
}
