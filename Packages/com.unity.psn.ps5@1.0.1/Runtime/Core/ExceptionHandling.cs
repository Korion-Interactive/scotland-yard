using System;
using System.Runtime.InteropServices;

namespace Unity.PSN.PS5
{
    /// <summary>
    /// API Results types returned from PRX calls contain details if the API call was successful,
    /// or if a warning or error was generated.
    /// </summary>
    public enum APIResultTypes
    {
        /// <summary>Result was successful</summary>
        Success = 0,
        /// <summary>A warning has occured.</summary>
        Warning = 1,
        /// <summary>An error had occured.</summary>
        Error = 2,
    };

    /// <summary>
    /// The structure even containing a successful API call or if it was a warning/error the details
    /// about the error.
    ///
    /// This is also used to fill out the NpToolkitException class when throwing an exception
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct APIResult
    {
        /// <summary>
        /// The result of the API call
        /// </summary>
        public APIResultTypes apiResult;

        IntPtr _message;

        /// <summary>
        /// Any message returned from the API call
        /// </summary>
        public string message { get { return Marshal.PtrToStringAnsi(_message); } }

        IntPtr _filename;
        /// <summary>
        /// The native C++ filename if an error or warning occured
        /// </summary>
        public string filename { get { return Marshal.PtrToStringAnsi(_filename); } }

        /// <summary>
        /// The C++ line number of any error or warning
        /// </summary>
        public Int32 lineNumber;

        /// <summary>
        /// The SCE error code returned from any native SDK methods called
        /// </summary>
        public Int32 sceErrorCode;

        /// <summary>
        /// Should the result raise an expection. Return true if an error or warning has occured.
        /// </summary>
        public bool RaiseException
        {
            get { return apiResult != APIResultTypes.Success; }
        }

        /// <summary>
        /// Return an error message for the result of a native call.
        /// </summary>
        /// <returns>The error message or null if there was no error</returns>
        public String ErrorMessage()
        {
            if (apiResult == APIResultTypes.Success)
            {
                return null;
            }

            string output = message;

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

    };

    /// <summary>
    /// Creates an exception to throw back to the Unity project.
    /// This can be created in the normal way or via a APIResult structure that has
    /// been returned from the Native plug-in
    /// </summary>
    public class PSNException : Exception
    {
        internal APIResultTypes resultType = APIResultTypes.Error;
        internal string filename;
        internal Int32 lineNumber;
        internal Int32 sceErrorCode;

        /// <summary>
        /// The type of results, either success, warning or error
        /// </summary>
        public APIResultTypes ResultType { get { return resultType; } }

        /// <summary>
        /// If from a native plug-in error the .cpp filename
        /// </summary>
        public string Filename { get { return filename; } }

        /// <summary>
        /// If from a native plug-in error the .cpp linenumber
        /// </summary>
        public Int32 LineNumber { get { return lineNumber; } }

        /// <summary>
        /// If from a native plug-in error the Sce error code
        /// </summary>
        public Int32 SceErrorCode { get { return sceErrorCode; } }

        /// <summary>
        /// Empty Exception
        /// </summary>
        public PSNException()
        {
        }

        /// <summary>
        /// Message only exception
        /// </summary>
        /// <param name="message">Message string</param>
        public PSNException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Message plus inner exception
        /// </summary>
        /// <param name="message">Message string</param>
        /// <param name="inner">Inner exception</param>
        public PSNException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Create exception with results returned from native method call
        /// </summary>
        /// <param name="apiResult">The results of an native api call.</param>
        public PSNException(APIResult apiResult)
            : base(apiResult.message)
        {
            resultType = apiResult.apiResult;
            filename = apiResult.filename;
            lineNumber = apiResult.lineNumber;
            sceErrorCode = apiResult.sceErrorCode;
        }

        /// <summary>
        /// Get the extended message for this exception.
        /// If the exception came from an error in the native plug-in it will include any Sce error code and the .cpp filename and line number.
        /// The Sce error code will be returned as a Hex character representation
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

    public class ExceededMaximumOperations : Exception
    {
        public ExceededMaximumOperations()
        {
        }

        public ExceededMaximumOperations(string message)
            : base(message)
        {
        }

        public ExceededMaximumOperations(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
