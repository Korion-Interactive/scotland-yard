#ifndef _PLUGIN_COMMON_H
#define _PLUGIN_COMMON_H

#define PRX_EXPORT extern "C" __declspec (dllexport)
#define	DO_EXPORT( _type, _func ) PRX_EXPORT _type _func

#include <assert.h>
#include <stdio.h>
#include <scetypes.h>
#include <save_data.h>
#include <save_data_dialog.h>
#include <string.h>

#define Assert assert

#include "../PlayerInterface/PrxPluginInterface.h"
#include "../PlayerInterface/UnityPrxPlugin.h"
#include "../PlayerInterface/IPluginUnity.h"
#include "../PlayerInterface/IPluginSceAppParams.h"
#include "../PlayerInterface/IPluginSceNpParams.h"

#include "SonyCommonIncludes.h"
//#include "ErrorCodesSony.h"

#include "../Managed/ManagedRequest.h"
#include "../Managed/ManagedResponse.h"
#include "../Managed/MemoryBufferManaged.h"

#include "../ErrorHandling/Errors.h"

#if NDEBUG
#define UNITY_TRACE(...)
#define UNITY_TRACEIF(condition, ...)
#else
#define UNITY_TRACE(...)				printf(__VA_ARGS__)
#define UNITY_TRACEIF(condition, ...)	if (condition) printf(__VA_ARGS__)
#endif

#include "CommonTypes.h"

#endif	//_PLUGIN_COMMON_H