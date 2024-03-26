using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Sony
{
    namespace PS4
    {
        namespace SaveData
        {
            /// <summary>
            /// Progress for various background savedata tasks.
            /// </summary>
            public class Progress
            {
                #region DLL Imports

                [DllImport("__Internal")]
                private static extern float PrxSaveDataGetProgress(out APIResult result);

                [DllImport("__Internal")]
                private static extern void PrxSaveDataClearProgress(out APIResult result);         

                #endregion

                /// <summary>
                /// Clear progress
                /// </summary>
                public static void ClearProgress()
                {
                    APIResult result;

                    PrxSaveDataClearProgress(out result);

                    if (result.RaiseException == true) throw new SaveDataException(result);
                }

                /// <summary>
                /// Get progress
                /// </summary>
                /// <returns>Progress (0.0f to 1.0f)</returns>
                public static float GetProgress()
                {
                    APIResult result;

                    float progress = PrxSaveDataGetProgress(out result);

                    if (result.RaiseException == true) throw new SaveDataException(result);

                    return progress;
                }
            }
        } // SavedGames
    } // PS4
} // Sony