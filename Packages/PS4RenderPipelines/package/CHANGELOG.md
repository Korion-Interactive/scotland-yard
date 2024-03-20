# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.9.0] - 2023-09-21

### Added

- Added TYPED_TEXTURE[2D|2D_ARRAY|3D] macros to allow cross platform declaration of explicitly typed Texture[2D|2DArray|3D] resources.

## [1.8.0] - 2022-12-05

### Added

- Added support for WaveIsHelperLane

## [1.7.0] - 2022-12-05

The version number for this package has increased due to a version update of a related graphics package.

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
- Added ASSIGN_SAMPLER support.

## [1.3.0] - 2020-12-10

### Added

- Added Bitfield Mask intrinsic

## [1.2.0] - 2020-06-12

### Fixed
- Fixed WaveIsFirstLane() behaviour on pixel shaders.

## [1.1.0] - 2020-03-31

### Added
- Added new quad intrinsics

## [1.0.0] - 2020-01-20

### Added
- Split PSSL.hlsl shader API file for PS4 into it's own package.

Started Changelog
