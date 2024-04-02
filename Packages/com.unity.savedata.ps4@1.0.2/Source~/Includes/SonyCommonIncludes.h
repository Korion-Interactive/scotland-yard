#pragma once


#define SONY_PLATFORM 1

#define PRX_EXPORT extern "C" __declspec (dllexport)
#define	DO_EXPORT( _type, _func ) PRX_EXPORT _type _func

// standard results for module startup
#define SCE_KERNEL_START_SUCCESS		(0)
#define SCE_KERNEL_START_NO_RESIDENT	(1)
#define SCE_KERNEL_START_FAILED			(2)

#define SAVE_DATA_FILEPATH_LENGTH (SCE_KERNEL_PATH_MAX)
// TODO:

