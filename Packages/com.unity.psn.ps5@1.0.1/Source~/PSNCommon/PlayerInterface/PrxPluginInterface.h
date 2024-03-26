#pragma once
// THIS FILE IS SHARED WITH THE PLAYER AND OTHER PLUGINS

#include <stdint.h>

#include "IPluginUnity.h"
#include "IPluginSceNpParams.h"
#include "IPluginSceAppParams.h"

#include "UnityEventQueue.h"

//#include "../SystemEvents/PS4SystemEventManager.h"

// ============================================================
// Query interface return codes
#define PRX_PLUGIN_QUERY_SUCCESS                0x00000000
#define PRX_PLUGIN_QUERY_NOT_READY              0x00000001

#define PRX_PLUGIN_QUERY_ERR_BAD_ARGS           0x80000001
#define PRX_PLUGIN_QUERY_ERR_NOT_SUPPORTED      0x80000002

// ============================================================
// Function pointer typedef for plugin query interface
typedef int(*fnPrxPluginQueryInterface)(int, void**);

// Application data passed from the player.
//
//  The only data passed through this structure is a pointer to the PrxPluginQueryInterface function in
//  the player runtime, all other data is obtained by calling into the runtime using the appropriate
//  interface from the runtime.
struct PrxPluginArgsV2
{
    static bool Validate(PrxPluginArgsV2* arg, size_t argSize)
    {
        if ((argSize < sizeof(PrxPluginArgsV2)) || (arg->m_Size != sizeof(PrxPluginArgsV2)) || (arg->m_Version != s_Version))
        {
            return false;
        }

        return true;
    }

    PrxPluginArgsV2() : m_Size(0) , m_Version(0), m_QueryInterface(NULL) {}
    static const uint32_t s_Version = 0x0200;   // Version number.
    static const size_t s_LegacySize = 52;      // Size of the old PrxPluginArgs struct.
    uint32_t m_Size;                            // Size of this structure, for validating the structure when the plugin starts.
    uint32_t m_Version;                         // Structure version number, for validating the structure when the plugin starts.
    fnPrxPluginQueryInterface m_QueryInterface; // Pointer to the runtime interface query function.
};

// ============================================================
// Interface IDs

#define PRX_PLUGIN_IFACE_ID_GLOBAL_EVENT_QUEUE      0x00000000
#define PRX_PLUGIN_IFACE_ID_UNITY                   0x00000001
#define PRX_PLUGIN_IFACE_ID_SCE_APP_PARAMS          0x00000002
#define PRX_PLUGIN_IFACE_ID_SCE_NP_PARAMS           0x00000003
#define PRX_PLUGIN_IFACE_ID_PS3                     0x00000004
#define PRX_PLUGIN_IFACE_ID_PS4                     0x00000005
#define PRX_PLUGIN_IFACE_ID_PSP2                    0x00000006

class PrxPluginInterface
{
public:

    fnPrxPluginQueryInterface m_queryInterface;

    IPluginUnity* m_IUnity;
    IPluginSceAppParams* m_ISceAppParams;
    IPluginSceNpParams* m_ISceNpParams;
#if defined(GLOBAL_EVENT_QUEUE)
    UnityEventQueue::IEventQueue* m_IEventQueue;
#endif

    PrxPluginInterface()
    {
        m_queryInterface = NULL;
        m_IUnity = NULL;
        m_ISceAppParams = NULL;
        m_ISceNpParams = NULL;
        m_IEventQueue = NULL;
    }

    // Methods
    void SetupRuntimeInterfaces()
    {
        if (m_queryInterface)
        {
#if defined(GLOBAL_EVENT_QUEUE)
            m_IEventQueue = GetRuntimeInterface<UnityEventQueue::IEventQueue>(PRX_PLUGIN_IFACE_ID_GLOBAL_EVENT_QUEUE, m_queryInterface);
#endif
            m_IUnity = GetRuntimeInterface<IPluginUnity>(PRX_PLUGIN_IFACE_ID_UNITY, m_queryInterface);
            m_ISceAppParams = GetRuntimeInterface<IPluginSceAppParams>(PRX_PLUGIN_IFACE_ID_SCE_APP_PARAMS, m_queryInterface);
            m_ISceNpParams = GetRuntimeInterface<IPluginSceNpParams>(PRX_PLUGIN_IFACE_ID_SCE_NP_PARAMS, m_queryInterface);
        }
    }

    bool InitialisePrxPluginArgs(unsigned int sz, const void* arg, const char* pluginName)
    {
        if (PrxPluginArgsV2::Validate((PrxPluginArgsV2*)arg, sz))
        {
            m_queryInterface = ((PrxPluginArgsV2*)arg)->m_QueryInterface;
        }
        else
        {
            UNITY_TRACE("\nERROR...\n");
            UNITY_TRACE(" %s is an old version that cannot be used by the current player runtime.\n", pluginName);
            UNITY_TRACE(" Please update the %s native module and any associated managed assemblies to the latest versions\n", pluginName);
            UNITY_TRACE("  Plugin args version, found %04x, expected %04x\n", ((PrxPluginArgsV2*)arg)->m_Version, ((PrxPluginArgsV2*)arg)->s_Version);

            return false;
        }

        return true;
    }

    template<typename T> T* GetRuntimeInterface(int interfaceID, fnPrxPluginQueryInterface queryInterface)
    {
        if (queryInterface)
        {
            void* interfacePtr = NULL;
            int queryStatus = queryInterface(interfaceID, &interfacePtr);
            if (queryStatus == PRX_PLUGIN_QUERY_SUCCESS)
            {
                return (T*)interfacePtr;
            }
        }
        return NULL;
    }
};

class PRXHelphers
{
public:
    static int LoadPRX(const char * module_file_name, SceKernelModule& handle)
    {
        int arg = 1;
        int res;
        handle = sceKernelLoadStartModule(module_file_name, 1, &arg, 0, NULL, &res);
        if (handle < SCE_OK)
            return -1;
        return res;
    }

    static int UnloadPRX(SceKernelModule handle)
    {
        int args[2] = { 10, 20 };
        int res;
        int result = sceKernelStopUnloadModule(handle, 2, args, 0, NULL,
            &res);
        if (result < SCE_OK)
            return -1;
        return res;
    }
};
