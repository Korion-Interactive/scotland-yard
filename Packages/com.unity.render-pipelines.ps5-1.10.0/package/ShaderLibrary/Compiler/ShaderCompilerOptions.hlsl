// Contains #defines that can be referenced from public shader code to control the optimization level of the PSSL compiler.
#pragma once

// Global shader compiler options that will apply to the entire source file
#define SHADER_COMPILER_GLOBAL_OPTIMIZE_REGISTER_USAGE "Packages/com.unity.render-pipelines.ps5/ShaderLibrary/Compiler/OptimizeForMinRegisterUsage.hlsl"
#define SHADER_COMPILER_GLOBAL_OPTIMIZE_LATENCY "Packages/com.unity.render-pipelines.ps5/ShaderLibrary/Compiler/OptimizeForLatency.hlsl"
#define SHADER_COMPILER_GLOBAL_OPTIMIZE_SLOW_MEMORY_ACCESS "Packages/com.unity.render-pipelines.ps5/ShaderLibrary/Compiler/OptimizeForSlowMemoryAccess.hlsl"
#define SHADER_COMPILER_GLOBAL_OPTIMIZE_DEFAULT "Packages/com.unity.render-pipelines.ps5/ShaderLibrary/Compiler/OptimizeForDefault.hlsl"

// Place on a line above the entrypoint function to apply to just that entrypoint in the source file
#define SHADER_COMPILER_ENTRYPOINT_OPTIMIZE_REGISTER_USAGE [argument(scheduler=minpressure)]
#define SHADER_COMPILER_ENTRYPOINT_OPTIMIZE_LATENCY [argument(scheduler=latency)]
#define SHADER_COMPILER_ENTRYPOINT_OPTIMIZE_SLOW_MEMORY_ACCESS [argument(scheduler=minpressure)]
#define SHADER_COMPILER_ENTRYPOINT_OPTIMIZE_DEFAULT [argument(scheduler=balanced)]
