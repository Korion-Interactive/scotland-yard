# Changelog
All notable changes to the package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

Due to package verification, the latest version below is the unpublished version and the date is meaningless.
however, it has to be formatted properly to pass verification tests.

## [1.0.1] - 2023-10-18

### Fixed
   - Stop editor build script executing when an non-PlayStation platform is being built
   
## [1.0.0] - 2023-09-04
   - New package format.

## [0.0.16-preview] - 2023-04-06

### Added
   - Added support for SDK 7.0

## [0.0.15-preview] - 2022-08-10

### Fixed
   - Added support for SDK 6.00

## [0.0.14-preview] - 2022-08-10

### Fixed
   - Changed MountRequest() constructor so it can now be called from a C# thread other than just the Unity main thread.

## [0.0.13-preview] - 2022-05-23

### Changed
   - Included offline HTML documentation in the HTMLDocs~ folder. Documentation can now be viewed without needing to run a local webserver.

## [0.0.12-preview] - 2022-04-25

### Added
   - Added support for SDK 5.0. Exposed MountRequest.SystemBlocks to enable support for new rollback feature (enabled by default)

## [0.0.11-preview] - 2021-11-23

### Changed
   - Added support for SDK 4.0

## [0.0.10-preview] - 2021-05-25

### Changed
   - Added support to read PS4 save games on PS5 (SDK 3.0 only)

## [0.0.9-preview] - 2021-02-17

### Changed
   - Updated the sample ExampleWriteFilesRequest and ExampleReadFilesRequest to demonstrate more efficient C# methods to write save files on PS5.

## [0.0.8-preview] - 2021-01-08

### Added
   - New package documentation and HTML pages updated
   
### Changed
   - Fixed spelling of Unity.SaveData.PS5.Initalization. Changed to Unity.SaveData.PS5.Initialization.
   - Updated MountRequest.BLOCK_SIZE, MountRequest.BLOCKS_MIN  and MountRequest.BLOCKS_MAX to new PS5 SDK values
   
### Fixed
   - Error checks are performed when setting SaveDataParams properties Title, SubTitle and Detail.
       * The byte length of the string is checked and an exception thrown if it exceeds the maximum supported length.

## [0.0.7-preview] - 2020-11-10

### Added
   - Added additional support for SDK 2.0 on PS5
      * Moved existing .prx files to plugin folders 1_00
	  * Added new .prx files to plugin folders 2_00

## [0.0.6-preview] - 2020-09-17

### Changed
- Added AllowNewItemTest callback to StartSaveDialogProcess. Use this to decide if the save dialog should display a save new item button.

## [0.0.5-preview] - 2020-09-14

### Changed
- Rebuilt SaveData.prx using SDK 1.000.050

## [0.0.4-preview] - 2020-03-05

### Changed

- Updated to use SDK 0.95
- Changed namespace to match package name `Unity.SaveData.PS5`
- Split system into additional namespaces to group functionality and produce better documentation
- Fixed issue in native code with sceSaveDataMount3 using SCE_SAVE_DATA_MOUNT_MODE_RDWR as usage has changed in 0.95. No change to the C# API was required.

### Added

- Added HTML documentation to HTML~ directory in package root.

## [0.0.3-preview] - 2020-02-17

### Added

- HTML offline documentation. HTML docs are located in the /HTMLDocs~ directory.

## [0.0.2-preview] - 2020-02-17

### Added

- Test sample now added to package. Installed from Package Manager Window

## [0.0.1-preview] - 2020-01-10

### Fixed

- Initial Version

### Changed

- Initial Version

### Added

- Initial Version



 
  





