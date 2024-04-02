// On PS5, foveated rendering is implemented with FSR (please refer to SCE documents for more details)
// The lookup tables (LUT) are encoded with the following convention :
// - in multipass, each LUT (vertical or horizontal) has the same size of the eye buffers
// - in single-pass, each view (left, then right eye) is stored contiguously in the LUTs

// We only support FSR and PS VR2 on the NGGC (Pure AGC) graphics device.

#if SHADER_API_PS5_NGGC
// Used to undistort a distorted buffer
StructuredBuffer<float> _FSR_LUT_resolve_H;
StructuredBuffer<float> _FSR_LUT_resolve_V;
StructuredBuffer<float> _FSR_LUT_resolve_prev_H;
StructuredBuffer<float> _FSR_LUT_resolve_prev_V;

// Contain the density of each pixel and can be used in post effects to adjust kernel weight
StructuredBuffer<float> _FSR_LUT_density_H;
StructuredBuffer<float> _FSR_LUT_density_V;
StructuredBuffer<float> _FSR_LUT_density_prev_H;
StructuredBuffer<float> _FSR_LUT_density_prev_V;

// Used from linear window space into distorted raster space
StructuredBuffer<float> _FSR_LUT_distortion_H;
StructuredBuffer<float> _FSR_LUT_distortion_V;
StructuredBuffer<float> _FSR_LUT_distortion_prev_H;
StructuredBuffer<float> _FSR_LUT_distortion_prev_V;

inline void ComputeStereoLutIndices(in float2 uv, out int2 indices0, out int2 indices1, out float2 blend)
{
    uv = clamp(uv, float2(0, 0), float2(1, 1));
    float2 texels = (unity_StereoEyeIndex + uv) * _ScreenSize.xy;
    int2 maxIndex = _ScreenSize.xy * (unity_StereoEyeIndex + 1) - 1;
    indices0 = int2(texels);
    indices1 = min(indices0 + int2(1,1), maxIndex);
    blend = frac(texels);
}

inline float2 computeUVs(in float2 uv, in StructuredBuffer<float> horizontalBuffer, in StructuredBuffer<float> verticalBuffer)
{
    int2 indices0, indices1;
    float2 blend;
    ComputeStereoLutIndices(uv, indices0, indices1, blend);
    return float2 (lerp(horizontalBuffer[indices0.x], horizontalBuffer[indices1.x], blend.x),
                   lerp(verticalBuffer[indices0.y],   verticalBuffer[indices1.y],   blend.y));
}

float2 RemapFoveatedRenderingLinearToNonUniform(float2 uv)
{
    return computeUVs(uv, _FSR_LUT_resolve_H, _FSR_LUT_resolve_V);
}

float2 RemapFoveatedRenderingPrevFrameLinearToNonUniform(float2 uv)
{
    // Previous frame resolve can be outside the UV range (especially for for motion vectors), just ignore the out-of-range values
    float2 transformedUVs = computeUVs(uv, _FSR_LUT_resolve_prev_H, _FSR_LUT_resolve_prev_V);
    return float2(uv.x >= 0.0 && uv.x <= 1.0 ? transformedUVs.x : uv.x, uv.y >= 0.0 && uv.y <= 1.0 ? transformedUVs.y : uv.y);
}

float2 RemapFoveatedRenderingDensity(float2 uv)
{
    return computeUVs(uv, _FSR_LUT_density_H, _FSR_LUT_density_V);
}

float2 RemapFoveatedRenderingPrevFrameDensity(float2 uv)
{
    return computeUVs(uv, _FSR_LUT_density_prev_H, _FSR_LUT_density_prev_V);
}

float2 RemapFoveatedRenderingNonUniformToLinear(float2 uv)
{
    return computeUVs(uv, _FSR_LUT_distortion_H, _FSR_LUT_distortion_V);
}

float2 RemapFoveatedRenderingPrevFrameNonUniformToLinear(float2 uv)
{
    // Previous frame resolve can be outside the UV range (especially for for motion vectors), just ignore the out-of-range values
    float2 transformedUVs = computeUVs(uv, _FSR_LUT_distortion_prev_H, _FSR_LUT_distortion_prev_V);
    return float2(uv.x >= 0.0 && uv.x <= 1.0 ? transformedUVs.x : uv.x, uv.y >= 0.0 && uv.y <= 1.0 ? transformedUVs.y : uv.y);
}

int2 RemapFoveatedRenderingNonUniformToLinearCS(int2 positionCS, bool yflip)
{
    if (yflip)
        positionCS.y = _ScreenSize.y - positionCS.y;

    int2 lutIndex = unity_StereoEyeIndex * _ScreenSize.xy + positionCS;

    positionCS.x = _FSR_LUT_distortion_H[lutIndex.x] * _ScreenSize.x;
    positionCS.y = _FSR_LUT_distortion_V[lutIndex.y] * _ScreenSize.y;

    if (yflip)
        positionCS.y = _ScreenSize.y - positionCS.y;

    return positionCS;
}
#else
int2 RemapFoveatedRenderingNonUniformToLinearCS(int2 positionCS, bool yflip)
{
    return positionCS;
}

float2 RemapFoveatedRenderingPrevFrameNonUniformToLinear(float2 uv)
{
    return uv;
}
float2 RemapFoveatedRenderingNonUniformToLinear(float2 uv)
{
    return uv;
}
float2 RemapFoveatedRenderingPrevFrameDensity(float2 uv)
{
    return uv;
}
float2 RemapFoveatedRenderingDensity(float2 uv)
{
    return uv;
}
float2 RemapFoveatedRenderingPrevFrameLinearToNonUniform(float2 uv)
{
    return uv;
}
float2 RemapFoveatedRenderingLinearToNonUniform(float2 uv)
{
    return uv;
}

#endif

// Adapt old remap functions to their new name
float2 RemapFoveatedRenderingResolve(float2 uv) { return RemapFoveatedRenderingLinearToNonUniform(uv); }
float2 RemapFoveatedRenderingPrevFrameResolve(float2 uv) {return RemapFoveatedRenderingPrevFrameLinearToNonUniform(uv); }
float2 RemapFoveatedRenderingDistort(float2 uv) { return RemapFoveatedRenderingNonUniformToLinear(uv); }
float2 RemapFoveatedRenderingPrevFrameDistort(float2 uv) { return RemapFoveatedRenderingPrevFrameNonUniformToLinear(uv); }
int2 RemapFoveatedRenderingDistortCS(int2 positionCS, bool yflip) { return RemapFoveatedRenderingNonUniformToLinearCS(positionCS, yflip); }
