
using System.Runtime.InteropServices;
using Unity.SaveData.PS5.Core;

namespace Unity.SaveData.PS5.Files
{
    /// <summary>
    /// Progress for various background save data tasks.
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
        /// Clears task progress.
        /// </summary>
        public static void ClearProgress()
        {
            APIResult result;

            PrxSaveDataClearProgress(out result);

            if (result.RaiseException == true) throw new SaveDataException(result);
        }

        /// <summary>
        /// Gets task progress.
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
}
