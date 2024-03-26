#include "SonyCommonIncludes.h"
#if BUILD_AS_PRX
extern "C" int module_start(size_t sz, const void* arg)
{
    return SCE_KERNEL_START_SUCCESS;
}
#endif
