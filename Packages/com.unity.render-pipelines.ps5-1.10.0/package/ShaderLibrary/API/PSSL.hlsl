// This file assume SHADER_API_PSSL is defined

// Before declaring macros we need to undef some of them to avoid warnings.
// Said warnings are happening as these macros are included in HLSLSupport.cginc
#undef CBUFFER_START
#undef UNITY_BRANCH
#undef UNITY_FLATTEN
#undef UNITY_UNROLL
#undef UNITY_LOOP
#undef SAMPLE_DEPTH_TEXTURE
#undef SAMPLE_DEPTH_TEXTURE_LOD

// We undef this here as it is defined both in HLSLSupport.cginc and in SRP's Common.hlsl
// where this file is included from.
#undef GLOBAL_CBUFFER_START

#define PLATFORM_SUPPORTS_BUFFER_ATOMICS_IN_PIXEL_SHADER
#define PLATFORM_LANE_COUNT 64

#define PLATFORM_SUPPORTS_WAVE_INTRINSICS

#define INTRINSIC_BITFIELD_MASK
#define GetBitFieldMask BitFieldMask
#define INTRINSIC_BITFIELD_EXTRACT
#define BitFieldExtract __v_bfe_u32
#define INTRINSIC_BITFIELD_EXTRACT_SIGN_EXTEND
#define BitFieldExtractSignExtend __v_bfe_i32
#define INTRINSIC_BITFIELD_INSERT
#define BitFieldInsert __v_bfi_b32
#define INTRINSIC_WAVEREADFIRSTLANE
#define WaveReadLaneFirst ReadFirstLane
#define INTRINSIC_MUL24
#define Mul24 mul24
#define INTRINSIC_MAD24
#define Mad24 mad24
#define INTRINSIC_MINMAX3
#define Min3 min3
#define Max3 max3
#define INTRINSIC_CUBEMAP_FACE_ID
#define INTRINSIC_WAVE_MINMAX
#define WaveActiveMin CrossLaneMin
#define WaveActiveMax CrossLaneMax
#define INTRINSIC_WAVE_SUM
#define WaveActiveSum CrossLaneAdd
#define WaveActiveProduct CrossLaneMul
#define INTRINSIC_WAVE_LOGICAL_OPS
#define WaveActiveBitAnd CrossLaneAnd
#define WaveActiveBitOr CrossLaneOr
#define WaveActiveBitXor CrossLaneXor
#define WaveGetID GetWaveID
#define WavePrefixSum ExclusivePrefixSum
#define WavePrefixProduct ExclusivePrefixProduct
#define WaveReadLaneAt CrossLaneRead

#define INTRINSIC_BALLOT
uint4 WaveActiveBallot(bool expression)
{
    uint2 mask = bit_cast<uint2>(ballot(expression));
    return uint4(mask.x, mask.y, 0, 0);
}

#define INTRINSIC_WAVE_ACTIVE_ALL_ANY
bool WaveActiveAllTrue(bool expression)
{
    return (__s_read_exec() == ballot(expression));
}

uint WaveActiveCountBits(bool expression)
{
    uint2 mask = bit_cast<uint2>(ballot(expression));
    return popcnt(mask.x) + popcnt(mask.y);
}

uint WavePrefixCountBits(bool expression)
{
    return MaskBitCnt(ballot(expression));
}

bool WaveActiveAnyTrue(bool expression)
{
    return (popcnt(ballot(expression))) != 0;
}

uint WaveGetLaneIndex()
{
    return __v_mbcnt_hi_u32_b32(0xffffffff, __v_mbcnt_lo_u32_b32(0xffffffff, 0));
}

bool WaveIsFirstLane()
{
#ifdef SHADER_STAGE_FRAGMENT

    // For fragment shaders we have to make sure to return the first non helper lane that is valid as helper lanes are
    // described as inactive according to the HLSL docs here: https://github.com/Microsoft/DirectXShaderCompiler/wiki/Wave-Intrinsics
    // the MSDN docs appear to be out of date on the Wave functions: https://github.com/Microsoft/DirectXShaderCompiler/issues/699
    // where they describe WaveIsFirstLane as including helper lanes. Inspecting the GCN disassembly from DX12 a DX12 shader with the OpenGPU
    // RGA tool shows that on DX12 WaveIsFirstLane() does indeed appear to exclude helper lanes picking the lowest non-helper lane.
    ulong mask = __s_read_exec() & __s_read_initialpixelvalidmask();

    return __v_cndmask_b32(0, 1, mask & -mask);
#else
    return MaskBitCnt(__s_read_exec()) == 0;
#endif
}

#define INTRINSIC_IS_HELPER_LANE
bool WaveIsHelperLane()
{
    return !(__v_cndmask_b32(0, 1, __s_read_initialpixelvalidmask()));
}

uint WaveGetLaneCount()
{
    return PLATFORM_LANE_COUNT;
}

#define INTRINSIC_QUAD_SHUFFLE

/*
IMPORTANT!! The following are valid only in pixel shaders or when threads are arranged in a quad fashion.

Let's assume the relative indices of threads in a quad are as follow
        +--------- X
        | [0] [1]
        | [2] [3]

        Y

The mask given to the swizzle operation will assume we are in quad mode, meaning that the 16th bit is set to 1 and bits [8:14] are irrelevant. In this mode
the bits [1:0] will point to the relative lane index [0] will get the value from, bits[3:2] to the relative lane index [1] will get the value from and so forth.

The following predetermined functions give us swaps across all directions of quad.

For example, referencing the plot on top of this comment:
This means that ReadAcrossX for [0] will give [1], for [1] will give [0], for [2] it will give [3] and for [3] it will give [2]
*/


#define GENERATE_INTRINSIC_1_VAR_1_IGNORED_UINT2_ARG_1_FIXED_UINT_ARG(FunctionName, BaseIntrinsicName, Parameter0, UintParam) \
    float FunctionName(float Parameter0, uint2 ignore) { return BaseIntrinsicName##_f32(Parameter0, UintParam); } \
    int   FunctionName(int   Parameter0, uint2 ignore) { return BaseIntrinsicName##_i32(Parameter0, UintParam); } \
    uint  FunctionName(uint  Parameter0, uint2 ignore) { return BaseIntrinsicName##_u32(Parameter0, UintParam); }

#define GENERATE_INTRINSIC_1_VAR_ARG_1_VAR_UINT_ARG(FunctionName, BaseIntrinsicName, Parameter0, UintParam) \
    float FunctionName(float Parameter0, uint UintParam) { return BaseIntrinsicName##_f32(Parameter0, UintParam); } \
    int   FunctionName(int   Parameter0, uint UintParam) { return BaseIntrinsicName##_i32(Parameter0, UintParam); } \
    uint  FunctionName(uint  Parameter0, uint UintParam) { return BaseIntrinsicName##_u32(Parameter0, UintParam); }

GENERATE_INTRINSIC_1_VAR_1_IGNORED_UINT2_ARG_1_FIXED_UINT_ARG(QuadReadAcrossX,        __ds_swizzle, value, 32945); // offset: 1000 0000 1011 0001
GENERATE_INTRINSIC_1_VAR_1_IGNORED_UINT2_ARG_1_FIXED_UINT_ARG(QuadReadAcrossY,        __ds_swizzle, value, 32846); // offset: 1000 0000 0100 1110
GENERATE_INTRINSIC_1_VAR_1_IGNORED_UINT2_ARG_1_FIXED_UINT_ARG(QuadReadAcrossDiagonal, __ds_swizzle, value, 32795); // offset: 1000 0000 0001 1011



// The following functions are the generic lane swizzle, doesn't assume QUAD distribution of lanes.
#define INTRINSIC_WAVE_LANE_SWIZZLE
// Some helper functions to get the right arguments for the lane intrinsics when group size is known.
// IMPORTANT: Only valid for compute and it only works when group size horizontally is an even number.
uint GetSwizzleMaskForQuadXSwap()
{
    // And mask = 0x1f, or_mask=0, xor_mask=0x01
    return 0x041f;
}

uint GetSwizzleMaskForQuadYSwap_8x8Group()
{
    // And mask = 0x1f, or_mask=0, xor_mask=0x08
    return 0x201f;
}

uint GetSwizzleMaskForQuadDiagonalSwap_8x8Group()
{
    // And mask = 0x1f, or_mask=0, xor_mask=0x09
    return 0x241f;
}

uint GetSwizzleMaskForQuadYSwap_16x16Group()
{
    // And mask = 0x1f, or_mask=0, xor_mask=0x10
    return 0x401f;
}

// More generic lane swizzling, it follows a different pattern than the quad mode above, see GCN ISA.
GENERATE_INTRINSIC_1_VAR_ARG_1_VAR_UINT_ARG(WaveLaneSwizzle, __ds_swizzle, value, offset);

#define DEFINE_ALL_EQUAL_TYPE(TYPE) \
bool WaveActiveAllEqual(TYPE v) \
{ \
    return WaveActiveAllTrue(v == WaveReadLaneFirst(v)); \
}

DEFINE_ALL_EQUAL_TYPE(uint);
DEFINE_ALL_EQUAL_TYPE(int);
DEFINE_ALL_EQUAL_TYPE(float);

#define UNITY_UV_STARTS_AT_TOP 1
#define UNITY_REVERSED_Z 1
#define UNITY_NEAR_CLIP_VALUE (1.0)
// This value will not go through any matrix projection convertion
#define UNITY_RAW_FAR_CLIP_VALUE (0.0)
#define VERTEXID_SEMANTIC SV_VertexID
#define INSTANCEID_SEMANTIC SV_InstanceID
#define FRONT_FACE_SEMANTIC SV_IsFrontFace
#define FRONT_FACE_TYPE bool
#define IS_FRONT_VFACE(VAL, FRONT, BACK) ((VAL) ? (FRONT) : (BACK))

#define CBUFFER_START(name) cbuffer name {
#define CBUFFER_END };


// flow control attributes
#define UNITY_BRANCH        [branch]
#define UNITY_FLATTEN       [flatten]
#define UNITY_UNROLL        [unroll]
#define UNITY_UNROLLX(_x)   [unroll(_x)]
#define UNITY_LOOP          [loop]

// Initialize arbitrary structure with zero values.
// Do not exist on some platform, in this case we need to have a standard name that call a function that will initialize all parameters to 0
#define ZERO_INITIALIZE(type, name) name = (type)0;
#define ZERO_INITIALIZE_ARRAY(type, name, arraySize) { for (int arrayIndex = 0; arrayIndex < arraySize; arrayIndex++) { name[arrayIndex] = (type)0; } }

// Texture util abstraction

#define CALCULATE_TEXTURE2D_LOD(textureName, samplerName, coord2) textureName.GetLOD(samplerName, coord2)

// Texture abstraction

#define TEXTURE2D(textureName)                Texture2D textureName
#define TEXTURE2D_ARRAY(textureName)          Texture2DArray textureName
#define TEXTURECUBE(textureName)              TextureCube textureName
#define TEXTURECUBE_ARRAY(textureName)        TextureCubeArray textureName
#define TEXTURE3D(textureName)                Texture3D textureName

#define TEXTURE2D_FLOAT(textureName)          TEXTURE2D(textureName)
#define TEXTURE2D_ARRAY_FLOAT(textureName)    TEXTURE2D_ARRAY(textureName)
#define TEXTURECUBE_FLOAT(textureName)        TEXTURECUBE(textureName)
#define TEXTURECUBE_ARRAY_FLOAT(textureName)  TEXTURECUBE_ARRAY(textureName)
#define TEXTURE3D_FLOAT(textureName)          TEXTURE3D(textureName)

#define TEXTURE2D_HALF(textureName)           TEXTURE2D(textureName)
#define TEXTURE2D_ARRAY_HALF(textureName)     TEXTURE2D_ARRAY(textureName)
#define TEXTURECUBE_HALF(textureName)         TEXTURECUBE(textureName)
#define TEXTURECUBE_ARRAY_HALF(textureName)   TEXTURECUBE_ARRAY(textureName)
#define TEXTURE3D_HALF(textureName)           TEXTURE3D(textureName)

#define TEXTURE2D_SHADOW(textureName)         TEXTURE2D(textureName)
#define TEXTURE2D_ARRAY_SHADOW(textureName)   TEXTURE2D_ARRAY(textureName)
#define TEXTURECUBE_SHADOW(textureName)       TEXTURECUBE(textureName)
#define TEXTURECUBE_ARRAY_SHADOW(textureName) TEXTURECUBE_ARRAY(textureName)

#define TYPED_TEXTURE2D(type, textureName)       Texture2D<type> textureName
#define TYPED_TEXTURE2D_ARRAY(type, textureName) Texture2DArray<type> textureName
#define TYPED_TEXTURE3D(type, textureName)       Texture3D<type> textureName
#define RW_TEXTURE2D(type, textureName)          RW_Texture2D<type> textureName
#define RW_TEXTURE2D_ARRAY(type, textureName)    RW_Texture2D_Array<type> textureName
#define RW_TEXTURE3D(type, textureName)          RW_Texture3D<type> textureName

#define SAMPLER(samplerName)                  SamplerState samplerName
#define SAMPLER_CMP(samplerName)              SamplerComparisonState samplerName
#define ASSIGN_SAMPLER(samplerName, samplerValue) samplerName = samplerValue

#define TEXTURE2D_PARAM(textureName, samplerName)                 TEXTURE2D(textureName),         SAMPLER(samplerName)
#define TEXTURE2D_ARRAY_PARAM(textureName, samplerName)           TEXTURE2D_ARRAY(textureName),   SAMPLER(samplerName)
#define TEXTURECUBE_PARAM(textureName, samplerName)               TEXTURECUBE(textureName),       SAMPLER(samplerName)
#define TEXTURECUBE_ARRAY_PARAM(textureName, samplerName)         TEXTURECUBE_ARRAY(textureName), SAMPLER(samplerName)
#define TEXTURE3D_PARAM(textureName, samplerName)                 TEXTURE3D(textureName),         SAMPLER(samplerName)

#define TEXTURE2D_SHADOW_PARAM(textureName, samplerName)          TEXTURE2D(textureName),         SAMPLER_CMP(samplerName)
#define TEXTURE2D_ARRAY_SHADOW_PARAM(textureName, samplerName)    TEXTURE2D_ARRAY(textureName),   SAMPLER_CMP(samplerName)
#define TEXTURECUBE_SHADOW_PARAM(textureName, samplerName)        TEXTURECUBE(textureName),       SAMPLER_CMP(samplerName)
#define TEXTURECUBE_ARRAY_SHADOW_PARAM(textureName, samplerName)  TEXTURECUBE_ARRAY(textureName), SAMPLER_CMP(samplerName)

#define TEXTURE2D_ARGS(textureName, samplerName)                textureName, samplerName
#define TEXTURE2D_ARRAY_ARGS(textureName, samplerName)          textureName, samplerName
#define TEXTURECUBE_ARGS(textureName, samplerName)              textureName, samplerName
#define TEXTURECUBE_ARRAY_ARGS(textureName, samplerName)        textureName, samplerName
#define TEXTURE3D_ARGS(textureName, samplerName)                textureName, samplerName

#define TEXTURE2D_SHADOW_ARGS(textureName, samplerName)         textureName, samplerName
#define TEXTURE2D_ARRAY_SHADOW_ARGS(textureName, samplerName)   textureName, samplerName
#define TEXTURECUBE_SHADOW_ARGS(textureName, samplerName)       textureName, samplerName
#define TEXTURECUBE_ARRAY_SHADOW_ARGS(textureName, samplerName) textureName, samplerName

#define PLATFORM_SAMPLE_TEXTURE2D(textureName, samplerName, coord2)                               textureName.Sample(samplerName, coord2)
#define PLATFORM_SAMPLE_TEXTURE2D_LOD(textureName, samplerName, coord2, lod)                      textureName.SampleLevel(samplerName, coord2, lod)
#define PLATFORM_SAMPLE_TEXTURE2D_BIAS(textureName, samplerName, coord2, bias)                    textureName.SampleBias(samplerName, coord2, bias)
#define PLATFORM_SAMPLE_TEXTURE2D_GRAD(textureName, samplerName, coord2, dpdx, dpdy)              textureName.SampleGrad(samplerName, coord2, dpdx, dpdy)
#define PLATFORM_SAMPLE_TEXTURE2D_ARRAY(textureName, samplerName, coord2, index)                  textureName.Sample(samplerName, float3(coord2, index))
#define PLATFORM_SAMPLE_TEXTURE2D_ARRAY_LOD(textureName, samplerName, coord2, index, lod)         textureName.SampleLevel(samplerName, float3(coord2, index), lod)
#define PLATFORM_SAMPLE_TEXTURE2D_ARRAY_BIAS(textureName, samplerName, coord2, index, bias)       textureName.SampleBias(samplerName, float3(coord2, index), bias)
#define PLATFORM_SAMPLE_TEXTURE2D_ARRAY_GRAD(textureName, samplerName, coord2, index, dpdx, dpdy) textureName.SampleGrad(samplerName, float3(coord2, index), dpdx, dpdy)
#define PLATFORM_SAMPLE_TEXTURECUBE(textureName, samplerName, coord3)                             textureName.Sample(samplerName, coord3)
#define PLATFORM_SAMPLE_TEXTURECUBE_LOD(textureName, samplerName, coord3, lod)                    textureName.SampleLevel(samplerName, coord3, lod)
#define PLATFORM_SAMPLE_TEXTURECUBE_BIAS(textureName, samplerName, coord3, bias)                  textureName.SampleBias(samplerName, coord3, bias)
#define PLATFORM_SAMPLE_TEXTURECUBE_ARRAY(textureName, samplerName, coord3, index)                textureName.Sample(samplerName, float4(coord3, index))
#define PLATFORM_SAMPLE_TEXTURECUBE_ARRAY_LOD(textureName, samplerName, coord3, index, lod)       textureName.SampleLevel(samplerName, float4(coord3, index), lod)
#define PLATFORM_SAMPLE_TEXTURECUBE_ARRAY_BIAS(textureName, samplerName, coord3, index, bias)     textureName.SampleBias(samplerName, float4(coord3, index), bias)
#define PLATFORM_SAMPLE_TEXTURE3D(textureName, samplerName, coord3)                               textureName.Sample(samplerName, coord3)
#define PLATFORM_SAMPLE_TEXTURE3D_LOD(textureName, samplerName, coord3, lod)                      textureName.SampleLevel(samplerName, coord3, lod)

#define SAMPLE_TEXTURE2D(textureName, samplerName, coord2)                               PLATFORM_SAMPLE_TEXTURE2D(textureName, samplerName, coord2)
#define SAMPLE_TEXTURE2D_LOD(textureName, samplerName, coord2, lod)                      PLATFORM_SAMPLE_TEXTURE2D_LOD(textureName, samplerName, coord2, lod)
#define SAMPLE_TEXTURE2D_BIAS(textureName, samplerName, coord2, bias)                    PLATFORM_SAMPLE_TEXTURE2D_BIAS(textureName, samplerName, coord2, bias)
#define SAMPLE_TEXTURE2D_GRAD(textureName, samplerName, coord2, dpdx, dpdy)              PLATFORM_SAMPLE_TEXTURE2D_GRAD(textureName, samplerName, coord2, dpdx, dpdy)
#define SAMPLE_TEXTURE2D_ARRAY(textureName, samplerName, coord2, index)                  PLATFORM_SAMPLE_TEXTURE2D_ARRAY(textureName, samplerName, coord2, index)
#define SAMPLE_TEXTURE2D_ARRAY_LOD(textureName, samplerName, coord2, index, lod)         PLATFORM_SAMPLE_TEXTURE2D_ARRAY_LOD(textureName, samplerName, coord2, index, lod)
#define SAMPLE_TEXTURE2D_ARRAY_BIAS(textureName, samplerName, coord2, index, bias)       PLATFORM_SAMPLE_TEXTURE2D_ARRAY_BIAS(textureName, samplerName, coord2, index, bias)
#define SAMPLE_TEXTURE2D_ARRAY_GRAD(textureName, samplerName, coord2, index, dpdx, dpdy) PLATFORM_SAMPLE_TEXTURE2D_ARRAY_GRAD(textureName, samplerName, coord2, index, dpdx, dpdy)
#define SAMPLE_TEXTURECUBE(textureName, samplerName, coord3)                             PLATFORM_SAMPLE_TEXTURECUBE(textureName, samplerName, coord3)
#define SAMPLE_TEXTURECUBE_LOD(textureName, samplerName, coord3, lod)                    PLATFORM_SAMPLE_TEXTURECUBE_LOD(textureName, samplerName, coord3, lod)
#define SAMPLE_TEXTURECUBE_BIAS(textureName, samplerName, coord3, bias)                  PLATFORM_SAMPLE_TEXTURECUBE_BIAS(textureName, samplerName, coord3, bias)
#define SAMPLE_TEXTURECUBE_ARRAY(textureName, samplerName, coord3, index)                PLATFORM_SAMPLE_TEXTURECUBE_ARRAY(textureName, samplerName, coord3, index)
#define SAMPLE_TEXTURECUBE_ARRAY_LOD(textureName, samplerName, coord3, index, lod)       PLATFORM_SAMPLE_TEXTURECUBE_ARRAY_LOD(textureName, samplerName, coord3, index, lod)
#define SAMPLE_TEXTURECUBE_ARRAY_BIAS(textureName, samplerName, coord3, index, bias)     PLATFORM_SAMPLE_TEXTURECUBE_ARRAY_BIAS(textureName, samplerName, coord3, index, bias)
#define SAMPLE_TEXTURE3D(textureName, samplerName, coord3)                               PLATFORM_SAMPLE_TEXTURE3D(textureName, samplerName, coord3)
#define SAMPLE_TEXTURE3D_LOD(textureName, samplerName, coord3, lod)                      PLATFORM_SAMPLE_TEXTURE3D_LOD(textureName, samplerName, coord3, lod)

#define SAMPLE_TEXTURE2D_SHADOW(textureName, samplerName, coord3)                    textureName.SampleCmpLevelZero(samplerName, (coord3).xy, (coord3).z)
#define SAMPLE_TEXTURE2D_ARRAY_SHADOW(textureName, samplerName, coord3, index)       textureName.SampleCmpLevelZero(samplerName, float3((coord3).xy, index), (coord3).z)
#define SAMPLE_TEXTURECUBE_SHADOW(textureName, samplerName, coord4)                  textureName.SampleCmpLevelZero(samplerName, (coord4).xyz, (coord4).w)
#define SAMPLE_TEXTURECUBE_ARRAY_SHADOW(textureName, samplerName, coord4, index)     textureName.SampleCmpLevelZero(samplerName, float4((coord4).xyz, index), (coord4).w)

#define SAMPLE_DEPTH_TEXTURE(textureName, samplerName, coord2)          SAMPLE_TEXTURE2D(textureName, samplerName, coord2).r
#define SAMPLE_DEPTH_TEXTURE_LOD(textureName, samplerName, coord2, lod) SAMPLE_TEXTURE2D_LOD(textureName, samplerName, coord2, lod).r

#define LOAD_TEXTURE2D(textureName, unCoord2)                                   textureName[unCoord2]
#define LOAD_TEXTURE2D_LOD(textureName, unCoord2, lod)                          textureName.MipMaps(lod, unCoord2)
#define LOAD_TEXTURE2D_MSAA(textureName, unCoord2, sampleIndex)                 textureName.Load(unCoord2, sampleIndex)
#define LOAD_TEXTURE2D_ARRAY(textureName, unCoord2, index)                      textureName[int3(unCoord2, index)]
#define LOAD_TEXTURE2D_ARRAY_MSAA(textureName, unCoord2, index, sampleIndex)    textureName.Load(int3(unCoord2, index), sampleIndex)
#define LOAD_TEXTURE2D_ARRAY_LOD(textureName, unCoord2, index, lod)             textureName.MipMaps(lod, int3(unCoord2, index))
#define LOAD_TEXTURE3D(textureName, unCoord3)                                   textureName[unCoord3]
#define LOAD_TEXTURE3D_LOD(textureName, unCoord3, lod)                          textureName.MipMaps(lod, unCoord3)

#define PLATFORM_SUPPORT_GATHER
#define GATHER_TEXTURE2D(textureName, samplerName, coord2)                textureName.Gather(samplerName, coord2)
#define GATHER_TEXTURE2D_ARRAY(textureName, samplerName, coord2, index)   textureName.Gather(samplerName, float3(coord2, index))
#define GATHER_TEXTURECUBE(textureName, samplerName, coord3)              textureName.Gather(samplerName, coord3)
#define GATHER_TEXTURECUBE_ARRAY(textureName, samplerName, coord3, index) textureName.Gather(samplerName, float4(coord3, index))
#define GATHER_RED_TEXTURE2D(textureName, samplerName, coord2)            textureName.GatherRed(samplerName, coord2)
#define GATHER_GREEN_TEXTURE2D(textureName, samplerName, coord2)          textureName.GatherGreen(samplerName, coord2)
#define GATHER_BLUE_TEXTURE2D(textureName, samplerName, coord2)           textureName.GatherBlue(samplerName, coord2)
#define GATHER_ALPHA_TEXTURE2D(textureName, samplerName, coord2)          textureName.GatherAlpha(samplerName, coord2)

#include "Packages/com.unity.render-pipelines.ps5/ShaderLibrary/Compiler/ShaderCompilerOptions.hlsl"
