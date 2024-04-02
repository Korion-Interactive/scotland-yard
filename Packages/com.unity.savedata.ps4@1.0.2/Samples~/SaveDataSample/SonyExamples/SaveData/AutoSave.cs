

using System;
using System.Collections;
using UnityEngine;

namespace SaveData
{
    /// <summary>
    /// Dialog state machine for running the save/load/delete savedata process
    /// </summary>
    public class AutoSaveProcess
    {
        public delegate void ErrorHandler(uint errorCode);

        //  delegate 
        /// <summary>
        /// Save process states
        /// </summary>
        private enum SaveState
        {
            Begin,
            SaveFiles,
            WriteIcon,
            WriteParams,
            Unmount,
            HandleError,

            LoadFiles,

            Exit
        }

        /// <summary>
        /// Start the auto-save process as a Unity Coroutine.
        /// </summary>
        public static IEnumerator StartAutoSaveProcess(int userId, Sony.PS4.SaveData.Dialogs.NewItem newItem, Sony.PS4.SaveData.DirName autoSaveDirName, UInt64 newSaveDataBlocks, Sony.PS4.SaveData.SaveDataParams saveDataParams,
                                                          Sony.PS4.SaveData.FileOps.FileOperationRequest fileRequest, Sony.PS4.SaveData.FileOps.FileOperationResponse fileResponse, bool backup, ErrorHandler errHandler)
        {
            SaveState currentState = SaveState.Begin;

            Sony.PS4.SaveData.Mounting.MountResponse mountResponse = new Sony.PS4.SaveData.Mounting.MountResponse();
            Sony.PS4.SaveData.Mounting.MountPoint mp = null;

            int errorCode = 0;

            while (currentState != SaveState.Exit)
            {
                switch (currentState)
                {
                    case SaveState.Begin:
                        {
                            Sony.PS4.SaveData.Mounting.MountModeFlags flags = Sony.PS4.SaveData.Mounting.MountModeFlags.Create2 | Sony.PS4.SaveData.Mounting.MountModeFlags.ReadWrite;

                            Sony.PS4.SaveData.DirName dirName = autoSaveDirName;

                            errorCode = MountSaveData(userId, newSaveDataBlocks, mountResponse, dirName, flags);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                // Wait for save data to be mounted.
                                while (mountResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                if (mountResponse.IsErrorCode == true)
                                {
                                    errorCode = mountResponse.ReturnCodeValue;

                                    // Must handle no space and broken save games
                                    //    Sony.PS4.SaveData.ReturnCodes.DATA_ERROR_NO_SPACE_FS
                                    //    Sony.PS4.SaveData.ReturnCodes.SAVE_DATA_ERROR_BROKEN)
                                    currentState = SaveState.HandleError;
                                }
                                else
                                {
                                    // Save data is now mounted, so files can be saved.
                                    mp = mountResponse.MountPoint;
                                    currentState = SaveState.SaveFiles;
                                }
                            }
                        }
                        break;
                    case SaveState.SaveFiles:
                        {
                            // Do actual saving
                            fileRequest.MountPointName = mp.PathName;
                            fileRequest.Async = true;
                            fileRequest.UserId = userId;

                            errorCode = Sony.PS4.SaveData.FileOps.CustomFileOp(fileRequest, fileResponse);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                while (fileResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                // Write the icon and any detail parmas set here.
                                Sony.PS4.SaveData.EmptyResponse iconResponse = new Sony.PS4.SaveData.EmptyResponse();

                                errorCode = WriteIcon(userId, iconResponse, mp, newItem);

                                if (errorCode < 0)
                                {
                                    currentState = SaveState.HandleError;
                                }
                                else
                                {
                                    Sony.PS4.SaveData.EmptyResponse paramsResponse = new Sony.PS4.SaveData.EmptyResponse();

                                    errorCode = WriteParams(userId, paramsResponse, mp, saveDataParams);

                                    if (errorCode < 0)
                                    {
                                        currentState = SaveState.HandleError;
                                    }
                                    else
                                    {
                                        // Wait for save icon to be mounted.
                                        while (iconResponse.Locked == true || paramsResponse.Locked == true)
                                        {
                                            yield return null;
                                        }

                                        currentState = SaveState.WriteIcon;
                                    }
                                }
                            }
                        }
                        break;
                    case SaveState.WriteIcon:
                        {
                            // Write the icon and any detail parmas set here.
                            Sony.PS4.SaveData.EmptyResponse iconResponse = new Sony.PS4.SaveData.EmptyResponse();

                            errorCode = WriteIcon(userId, iconResponse, mp, newItem);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                while (iconResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                currentState = SaveState.WriteParams;
                            }
                        }
                        break;
                    case SaveState.WriteParams:
                        {
                            Sony.PS4.SaveData.EmptyResponse paramsResponse = new Sony.PS4.SaveData.EmptyResponse();

                            errorCode = WriteParams(userId, paramsResponse, mp, saveDataParams);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                // Wait for save icon to be mounted.
                                while (paramsResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                currentState = SaveState.Unmount;
                            }
                        }
                        break;
                    case SaveState.Unmount:
                        {
                            Sony.PS4.SaveData.EmptyResponse unmountResponse = new Sony.PS4.SaveData.EmptyResponse();

                            errorCode = UnmountSaveData(userId, unmountResponse, mp, backup);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                while (unmountResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                currentState = SaveState.Exit;
                            }
                        }
                        break;
                    case SaveState.HandleError:
                        {
                            if (mp != null)
                            {
                                Sony.PS4.SaveData.EmptyResponse unmountResponse = new Sony.PS4.SaveData.EmptyResponse();

                                UnmountSaveData(userId, unmountResponse, mp, backup);
                            }

                            if(errHandler != null)
                            {
                                errHandler((uint)errorCode);
                            }
                        }
                        currentState = SaveState.Exit;
                        break;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Start the auto-save process as a Unity Coroutine.
        /// </summary>
        public static IEnumerator StartAutoSaveLoadProcess(int userId, Sony.PS4.SaveData.DirName dirName, Sony.PS4.SaveData.FileOps.FileOperationRequest fileRequest, Sony.PS4.SaveData.FileOps.FileOperationResponse fileResponse, ErrorHandler errHandler)
        {
            SaveState currentState = SaveState.Begin;

            Sony.PS4.SaveData.Mounting.MountResponse mountResponse = new Sony.PS4.SaveData.Mounting.MountResponse();
            Sony.PS4.SaveData.Mounting.MountPoint mp = null;

            int errorCode = 0;

            while (currentState != SaveState.Exit)
            {
                switch (currentState)
                {
                    case SaveState.Begin:
                        {
                            Sony.PS4.SaveData.Mounting.MountModeFlags flags = Sony.PS4.SaveData.Mounting.MountModeFlags.ReadOnly;

                            errorCode = MountSaveData(userId, 0, mountResponse, dirName, flags);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                // Wait for save data to be mounted.
                                while (mountResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                if (mountResponse.IsErrorCode == true)
                                {
                                    errorCode = mountResponse.ReturnCodeValue;
                                    // Must handle broken save games
                                    //    Sony.PS4.SaveData.ReturnCodes.SAVE_DATA_ERROR_BROKEN)
                                    currentState = SaveState.HandleError;
                                }
                                else
                                {
                                    // Save data is now mounted, so files can be saved.
                                    mp = mountResponse.MountPoint;
                                    currentState = SaveState.LoadFiles;
                                }
                            }
                        }
                       break;
                    case SaveState.LoadFiles:
                        {
                            // Do actual loading
                            fileRequest.MountPointName = mp.PathName;
                            fileRequest.Async = true;
                            fileRequest.UserId = userId;

                            errorCode = Sony.PS4.SaveData.FileOps.CustomFileOp(fileRequest, fileResponse);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                while (fileResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                currentState = SaveState.Unmount;
                            }
                        }
                        break;
                    case SaveState.Unmount:
                        {
                            Sony.PS4.SaveData.EmptyResponse unmountResponse = new Sony.PS4.SaveData.EmptyResponse();

                            errorCode = UnmountSaveData(userId, unmountResponse, mp, false);

                            if (errorCode < 0)
                            {
                                currentState = SaveState.HandleError;
                            }
                            else
                            {
                                while (unmountResponse.Locked == true)
                                {
                                    yield return null;
                                }

                                currentState = SaveState.Exit;
                            }
                        }
                        break;
                    case SaveState.HandleError:
                        {
                            if (mp != null)
                            {
                                Sony.PS4.SaveData.EmptyResponse unmountResponse = new Sony.PS4.SaveData.EmptyResponse();

                                UnmountSaveData(userId, unmountResponse, mp, false);
                            }

                            if (errHandler != null)
                            {
                                errHandler((uint)errorCode);
                            }
                        }
                        currentState = SaveState.Exit;
                        break;
                }

                yield return null;
            }
        }

        internal static int MountSaveData(int userId, UInt64 blocks, Sony.PS4.SaveData.Mounting.MountResponse mountResponse, Sony.PS4.SaveData.DirName dirName, Sony.PS4.SaveData.Mounting.MountModeFlags flags)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Sony.PS4.SaveData.Mounting.MountRequest request = new Sony.PS4.SaveData.Mounting.MountRequest();

                request.UserId = userId;
                request.IgnoreCallback = true;
                request.DirName = dirName;

                request.MountMode = flags;

                if (blocks < Sony.PS4.SaveData.Mounting.MountRequest.BLOCKS_MIN)
                {
                    blocks = Sony.PS4.SaveData.Mounting.MountRequest.BLOCKS_MIN;
                }

                request.Blocks = blocks;

                Sony.PS4.SaveData.Mounting.Mount(request, mountResponse);
                errorCode = 0;
            }
            catch
            {
                if (mountResponse.ReturnCodeValue < 0)
                {
                    errorCode = mountResponse.ReturnCodeValue;
                }
            }

            return errorCode;
        }

        internal static int UnmountSaveData(int userId, Sony.PS4.SaveData.EmptyResponse unmountResponse, Sony.PS4.SaveData.Mounting.MountPoint mp, bool backup)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Sony.PS4.SaveData.Mounting.UnmountRequest request = new Sony.PS4.SaveData.Mounting.UnmountRequest();

                request.UserId = userId;
                request.MountPointName = mp.PathName;
                request.Backup = backup;
                request.IgnoreCallback = true;

                Sony.PS4.SaveData.Mounting.Unmount(request, unmountResponse);

                errorCode = 0;
            }
            catch
            {
                if (unmountResponse.ReturnCodeValue < 0)
                {
                    errorCode = unmountResponse.ReturnCodeValue;
                }
            }

            return errorCode;
        }

        internal static int WriteIcon(int userId, Sony.PS4.SaveData.EmptyResponse iconResponse, Sony.PS4.SaveData.Mounting.MountPoint mp, Sony.PS4.SaveData.Dialogs.NewItem newItem)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Sony.PS4.SaveData.Mounting.SaveIconRequest request = new Sony.PS4.SaveData.Mounting.SaveIconRequest();

                if (mp == null) return errorCode;

                request.UserId = userId;
                request.MountPointName = mp.PathName;
                request.RawPNG = newItem.RawPNG;
                request.IconPath = newItem.IconPath;
                request.IgnoreCallback = true;

                Sony.PS4.SaveData.Mounting.SaveIcon(request, iconResponse);

                errorCode = 0;
            }
            catch
            {
                if (iconResponse.ReturnCodeValue < 0)
                {
                    errorCode = iconResponse.ReturnCodeValue;
                }
            }

            return errorCode;
        }

        internal static int WriteParams(int userId, Sony.PS4.SaveData.EmptyResponse paramsResponse, Sony.PS4.SaveData.Mounting.MountPoint mp, Sony.PS4.SaveData.SaveDataParams saveDataParams)
        {
            int errorCode = unchecked((int)0x80B8000E);

            try
            {
                Sony.PS4.SaveData.Mounting.SetMountParamsRequest request = new Sony.PS4.SaveData.Mounting.SetMountParamsRequest();

                if (mp == null) return errorCode;

                request.UserId = userId;
                request.MountPointName = mp.PathName;
                request.IgnoreCallback = true;

                request.Params = saveDataParams;

                Sony.PS4.SaveData.Mounting.SetMountParams(request, paramsResponse);

                errorCode = 0;
            }
            catch
            {
                if (paramsResponse.ReturnCodeValue < 0)
                {
                    errorCode = paramsResponse.ReturnCodeValue;
                }
            }

            return errorCode;
        }

    }
}
