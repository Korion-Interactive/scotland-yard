#pragma once

#define SONY_PLATFORM 1

#ifdef LIBRARY_IMPL
//#pragma message "Using dllexport"
#define PRX_INTERFACE __declspec(dllexport)
#else
//#pragma message "Using dllimport"
#define PRX_INTERFACE __declspec(dllimport)
#endif

// standard results for module startup
#define SCE_KERNEL_START_SUCCESS        (0)
#define SCE_KERNEL_START_NO_RESIDENT    (1)
#define SCE_KERNEL_START_FAILED         (2)

#include <sdk_version.h>

#include <np.h>
#include <system_service.h>

#if __ORBIS__
#include <np_cg.h>
#endif

#include <assert.h>
#define Assert assert
// TODO:
