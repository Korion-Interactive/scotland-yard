using System;

namespace Unity.SaveData.PS5.Core
{
    /// <summary>
    /// A set of return codes that map to the Sony SCE error codes.
    /// </summary>
    public enum ReturnCodes : uint
    {
        //Generics
        /// <summary></summary>
        SUCCESS = 0x00000000,   // SCE_TOOLKIT_NP_V2_SUCCESS																	 

        /// <summary>Set the wrong parameter.</summary>
        SAVE_DATA_ERROR_PARAMETER = 0x809F0000,
        /// <summary>Not initialized save data library yet.</summary>
        SAVE_DATA_ERROR_NOT_INITIALIZED = 0x809F0001,
        /// <summary>Out of memory.</summary>
        SAVE_DATA_ERROR_OUT_OF_MEMORY = 0x809F0002,
        /// <summary>Save data folder has already been mounted, or save data memory setup has already completed.</summary>
        SAVE_DATA_ERROR_BUSY = 0x809F0003,
        /// <summary>Save data folder is not mounted.</summary>
        SAVE_DATA_ERROR_NOT_MOUNTED = 0x809F0004,

        /// <summary>Permission denied.</summary>
        SAVE_DATA_ERROR_NO_PERMISSION = 0x809F0005,
        /// <summary>Fingerprint mismatch.</summary>
        SAVE_DATA_ERROR_FINGERPRINT_MISMATCH = 0x809F0006,
        /// <summary>Save data already exists.</summary>
        SAVE_DATA_ERROR_EXISTS = 0x809F0007,
        /// <summary>Save data not found.</summary>
        SAVE_DATA_ERROR_NOT_FOUND = 0x809F0008,

        /// <summary>Not enough space for mounting save data at file system.</summary>
        DATA_ERROR_NO_SPACE_FS = 0x809F000A,
        /// <summary>Internal error.</summary>
        SAVE_DATA_ERROR_INTERNAL = 0x809F000B,
        /// <summary>Reached maximum mount value.</summary>
        SAVE_DATA_ERROR_MOUNT_FULL = 0x809F000C,
        /// <summary>Not mounted for writing.</summary>
        SAVE_DATA_ERROR_BAD_MOUNTED = 0x809F000D,
        /// <summary>File not found.</summary>
        SAVE_DATA_ERROR_FILE_NOT_FOUND = 0x809F000E,
        /// <summary>Save data broken.</summary>
        SAVE_DATA_ERROR_BROKEN = 0x809F000F,

        /// <summary>Invalid login user.</summary>
        SAVE_DATA_ERROR_INVALID_LOGIN_USER = 0x809F0011,

        // custom error codes
        /// <summary>Mount point name is invalid during a file operation request.</summary>
        InvalidMountPointName = 0x8A000001,
    }

    /// <summary>
    /// Base class that contains common Response data.
    /// </summary>
    public abstract class ResponseBase
    {
        internal Int32 returnCode;
        internal bool locked;
        internal Exception exception = null;

        /// <summary>
        /// Gets the return code value of a Response object when it is ready. The return value can be a
        /// successful result or an error result. See specific functions for possible return codes.
        /// </summary>
        /// <remarks>
        /// This is an interger value. If it is less than 0, it is an error code.
        /// </remarks>
        public Int32 ReturnCodeValue { get { ThrowExceptionIfLocked(); return returnCode; } }

        /// <summary>
        /// Gets the return code enum of a Response object when it is ready. The return value can be a
        /// successful result or an error result. See specific functions for possible return codes.
        /// </summary>
        public ReturnCodes ReturnCode { get { ThrowExceptionIfLocked(); return (ReturnCodes)returnCode; } }

        /// <summary>
        /// Returns any exception that could have occurred during the background processing of the request.
        /// </summary>
        public Exception Exception { get { ThrowExceptionIfLocked(); return exception; } }

        /// <summary>
        /// True if a Response object is being calculated, false if it is ready to be read.
        /// </summary>
        public bool Locked { get { return locked; } }

        internal ResponseBase()
        {
        }

        internal void Populate(APIResult result)
        {
            returnCode = result.sceErrorCode;
        }

        /// <summary>
        /// True if the return code contain an error code, false otherwise.
        /// </summary>
        public bool IsErrorCode
        {
            get
            {
                ThrowExceptionIfLocked();
                if (returnCode < 0)
                {
                    return true;
                }

                return false;
            }
        }

        internal bool IsErrorCodeWithoutLockCheck
        {
            get
            {
                if (returnCode < 0)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Generates a string containing the Hex return code and, if known, the Return code name
        /// taken from the Core.ReturnCodes enum.
        /// Because some of the enums share the same code, the returned string depends on the
        /// API function that generated the return code.
        /// </summary>
        /// <param name="apiCalled">The API function that generated the return code. </param>
        /// <returns>The constructed string with the Hex and return code name.</returns>
        /// <remarks>
        /// By default, apiCalled will be FunctionType.invalid and will return the first Core.ReturnCodes enum that matches the return code value, so it might produce an incorrect string. 
        /// </remarks>
        public string ConvertReturnCodeToString(FunctionTypes apiCalled)
        {
            ThrowExceptionIfLocked();

            string output = "(0x" + returnCode.ToString("X8") + ")";

            ReturnCodes rc = (ReturnCodes)returnCode;

            if (Enum.IsDefined(typeof(ReturnCodes), rc) == true)
            {
                output += " (" + rc.ToString() + ") ";
            }
            else
            {
                output += " (UNKNOWN) ";
            }

            return output;
        }

        internal void ThrowExceptionIfLocked()
        {
            if (locked == true)
            {
                throw new SaveDataException("This response object can't be read while it is waiting to be processed.");
            }
        }
    }

    /// <summary>
    /// Representation of empty data of a ResponseBase class.
    /// This still provides basic data that contains errors and return codes.
    /// </summary>
    public class EmptyResponse : ResponseBase
    {

    }
}
