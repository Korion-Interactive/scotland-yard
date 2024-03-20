# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.10.0] - 2023-09-21

### Added

- Added TYPED_TEXTURE[2D|2D_ARRAY|3D] macros to allow cross platform declaration of explicitly typed Texture[2D|2DArray|3D] resources.

## [1.9.1] - 2023-09-12

### Fixed
- Fixed Foveated rendering header declaring buffers in both the PlayStation5 and PlayStation5NGGC graphics API when they are only hooked up when using the PlayStation5NGGC API. Now if the Foveated rendering header is included in a shader compiled for the PlayStation5 graphics API it will just pass through the values (as PS VR2 and Foveated rendering are only supported under the PlayStation5NGGC graphics API).

## [1.9.0] - 2023-04-05

### Added

- Added new names for the Foveated rendering functions to improve clarity (existing names are kept for backwards compatibility and call the new functions)
    | Previous Name                             | New Name                                          |
    |-------------------------------------------|---------------------------------------------------|
    | RemapFoveatedRenderingResolve             | RemapFoveatedRenderingLinearToNonUniform          |
    | RemapFoveatedRenderingPrevFrameResolve    | RemapFoveatedRenderingPrevFrameLinearToNonUniform |
    | RemapFoveatedRenderingDistort             | RemapFoveatedRenderingNonUniformToLinear          |
    | RemapFoveatedRenderingPrevFrameDistort    | RemapFoveatedRenderingPrevFrameNonUniformToLinear |
    | RemapFoveatedRenderingDistortCS           | RemapFoveatedRenderingNonUniformToLinearCS        |

## [1.8.0] - 2022-12-05

### Added

- Added support for WaveIsHelperLane

## [1.7.0] - 2022-04-01

### Added

- Support for Foveated Rendering via FSR.

## [1.6.1] - 2022-02-25

### Fixed

- Fixed LOAD_TEXTURE macros on RWTextures. (case 1392780)

## [1.6.0] - 2021-07-28

### Fixed

- Fixed several warnings when compiling compute shaders.

## [1.5.0] - 2021-05-04

### Added

- Introduced a new layer of macros for 2d texture sampling. This facilitates scriptable pipelines into adding custom sampling behavior, such as global mip bias for temporal upscalers.

## [1.4.0] - 2021-01-14

### Added

- Added support for Mul24 calling into the intrinsic on PSSL.
- Added Macros to allow render pipelines to control the shader compiler optimization stratergy.
- Added Bitfield Mask intrinsic
- Added ASSIGN_SAMPLER support.

## [1.3.0] - 2020-09-24

### Added
- Split PSSL.hlsl shader API file for PS5 into it's own package.

Started Changelog
