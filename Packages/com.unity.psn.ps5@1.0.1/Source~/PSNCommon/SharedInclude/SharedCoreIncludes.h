#ifndef _PLUGIN_COMMON_H
#define _PLUGIN_COMMON_H

#define PRX_EXPORT extern "C" __declspec (dllexport)
#define DO_EXPORT(_type, _func) PRX_EXPORT _type _func

#include <assert.h>
#include <stdio.h>
#define Assert assert

#if NDEBUG
#define UNITY_TRACE(...)
#define UNITY_TRACEIF(condition, ...)
#else
#define UNITY_TRACE(...)                printf(__VA_ARGS__)
#define UNITY_TRACEIF(condition, ...)   if (condition) printf(__VA_ARGS__)
#endif

#include "SonyCommonIncludes.h"

#include "BinaryBuffers.h"
#include "Errors.h"
#include "HandleMsg.h"
#include "UserMap.h"

#include "../PlayerInterface/PrxPluginInterface.h"
#include "../PlayerInterface/UnityEventQueue.h"

#include <assert.h>
#include <stdio.h>
#define Assert assert

#include "CommonTypes.h"


#endif  //_PLUGIN_COMMON_H
