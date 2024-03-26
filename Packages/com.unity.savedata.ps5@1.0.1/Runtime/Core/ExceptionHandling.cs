using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Unity.SaveData.PS5.Core
{
    /// <summary>
    /// Returned from PRX calls. Specifies whether an API call was successful,
    /// or if a warning or error was generated.
    /// </summary>
    public enum APIResultTypes
    {
        /// <summary>API call was successful.</summary>
        Success = 0,
        /// <summary>A warning has occured.</summary>
        Warning = 1,
        /// <summary>An error had occured.</summary>
        Error = 2,
    };

    /// <summary>
    /// Contains a successful API call, or further details if a warning or error
    /// occurred.
    /// 
    /// This is also used to fill out the NpToolkitException class when throwing an exception.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct APIResult
    {
        public APIResultTypes apiResult;

        IntPtr _message;
        public string message { get { return Marshal.PtrToStringAnsi(_message); } }

        IntPtr _filename;
        public string filename { get { return Marshal.PtrToStringAnsi(_filename); } }

        public Int32 lineNumber;

        public Int32 sceErrorCode;

        public bool RaiseException
        {
            get { return apiResult != APIResultTypes.Success; }
        }
    };

    /// <summary>
    /// Creates an exception to throw back to the Unity project.
    /// This can be created in the normal way or via an APIResult structure that has
    /// been returned from the native plug-in.
    /// </summary>
    public class SaveDataException : Exception
    {
        internal APIResultTypes resultType = APIResultTypes.Error;
        internal string filename;
        internal Int32 lineNumber;
        internal Int32 sceErrorCode;

        /// <summary>
        /// The type of result. Can be success, warning, or error.
        /// </summary>
        public APIResultTypes ResultType { get { return resultType; } }

        /// <summary>
        /// If a native plug-in error occurred, returns the name of teh .cpp file.
        /// </summary>
        public string Filename { get { return filename; } }

        /// <summary>
        /// If a native plug-in error occurred, returns the line number in the .cpp file.
        /// </summary>
        public Int32 LineNumber { get { return lineNumber; } }

        /// <summary>
        /// If a native plug-in error occurred, returns the SCE error code.
        /// </summary>
        public Int32 SceErrorCode { get { return sceErrorCode; } }

        /// <summary>
        /// Empty exception.
        /// </summary>
        public SaveDataException()
        {
        }

        /// <summary>
        /// Message-only exception.
        /// </summary>
        /// <param name="message">Message string.</param>
        public SaveDataException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Message with an inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public SaveDataException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal SaveDataException(APIResult apiResult)
            : base(apiResult.message)
        {
            resultType = apiResult.apiResult;
            filename = apiResult.filename;
            lineNumber = apiResult.lineNumber;
            sceErrorCode = apiResult.sceErrorCode;
        }

        /// <summary>
        /// Gets the extended message for this exception.
        /// If the exception came from an error in the native plug-in, this includes any Sce error code, the .cpp filename, and the line number.
        /// The Sce error code is returned as a Hex character representation.
        /// </summary>
        public string ExtendedMessage
        {
            get
            {
                string output = Message;

                if (sceErrorCode != 0)
                {
                    output += " (Sce : 0x" + sceErrorCode.ToString("X") + " ) ";
                }

                if (filename != null && filename.Length > 0)
                {
                    output += " ( " + filename + " : Line = " + lineNumber + " ) ";
                }

                return output;
            }
        }
    }
}
