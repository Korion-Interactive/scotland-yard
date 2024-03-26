#pragma once

#include "../PlayerInterface/UnityEventQueue.h"
#include <system_service.h>

// ============================================================
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_ON_RESUME.
struct PS5OnResume { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0xD725C2DB79674D8CULL, 0xA52A1009670D0880ULL, PS5OnResume)
// ============================================================
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_GAME_LIVE_STREAMING_STATUS_UPDATE.
struct PS5OnGameLiveStreamingStatusUpdate { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0x11C8947A2EF44E45ULL, 0xA1F77F0AC927272EULL, PS5OnGameLiveStreamingStatusUpdate)
// ============================================================
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_SESSION_INVITATION.
struct PS5OnSessionInvitation { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0xCF791F68B6884AEFULL, 0x8198EC6D98D70F8AULL, PS5OnSessionInvitation)
// ============================================================
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_ENTITLEMENT_UPDATE.
struct PS5OnEntitlementUpdate { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0xDE76F015C0DE4BE8ULL, 0x9046B1153C877E39ULL, PS5OnEntitlementUpdate)
// ============================================================
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_GAME_CUSTOM_DATA.
struct PS5OnGameCustomData { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0x7D073AAAF3004C2BULL, 0x810A278660A015D6ULL, PS5OnGameCustomData)
// ============================================================
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_DISPLAY_SAFE_AREA_UPDATE.
struct PS5OnDisplaySafeAreaUpdate { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0xD69939D64B2544ABULL, 0xB5E6A5B6BC273194ULL, PS5OnDisplaySafeAreaUpdate)
// ============================================================
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_URL_OPEN.
struct PS5OnUrlOpen { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0xC89E302816C644DBULL, 0x832F32CD9410F333ULL, PS5OnUrlOpen)
// ============================================================
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_LAUNCH_APP.
struct PS5OnLaunchApp { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0xA1458D5B05264EA2ULL, 0xA70506FE4FCD11F3ULL, PS5OnLaunchApp)
// ============================================================
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_APP_LAUNCH_LINK.
struct PS5OnLaunchLink { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0x477AFB5C1CA045D6ULL, 0x95E9C61B8365A66AULL, PS5OnLaunchLink)
// ============================================================
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_ADDCONTENT_INSTALL.
struct PS5OnAddcontentInstall { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0x01E30CD6D6764564ULL, 0x9F2D243AC685D381ULL, PS5OnAddcontentInstall)
// ============================================================
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_RESET_VR_POSITION.
struct PS5OnResetVrPosition { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0x2BD3588AC2A34A03ULL, 0x8401C671EAB0964BULL, PS5OnResetVrPosition)
// ============================================================
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_JOIN_EVENT.
struct PS5OnJoinEvent { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0x8ABE8C89A5AD4C65ULL, 0xA818D17C31FD215EULL, PS5OnJoinEvent)
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_PLAYGO_LOCUS_UPDATE.
struct PS5OnPlaygoLocusUpdate { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0x21A78E6026A443FBULL, 0xB4E9B96B3B18FDA0ULL, PS5OnPlaygoLocusUpdate)
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_OPEN_SHARE_MENU.
struct PS5OnOpenShareMenu { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0xB7AD0B23F68B4361ULL, 0x802C36342FF448A7ULL, PS5OnOpenShareMenu)
// This event is triggered for SCE_SYSTEM_SERVICE_EVENT_PLAY_TOGETHER_HOST.
struct PS5OnPlayTogetherHost { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0xA136D11F66DB9F7FULL, 0x83EC5A5608DDDB55ULL, PS5OnPlayTogetherHost)

struct PS5OnSystemEvent { SceSystemServiceEvent params; };
REGISTER_EVENT_ID(0x926962B0FC4940C5ULL, 0x81DCC522B0823744ULL, PS5OnSystemEvent)
