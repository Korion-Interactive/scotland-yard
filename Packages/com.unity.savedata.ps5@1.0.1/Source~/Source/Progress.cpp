
#include "Progress.h"


namespace SaveData
{

	PRX_EXPORT float PrxSaveDataGetProgress(APIResult* result)
	{
		return Progress::GetProgress(result);
	}

	PRX_EXPORT void PrxSaveDataClearProgress(APIResult* result)
	{
		Progress::ClearProgress(result);
	}

	void Progress::ClearProgress(APIResult* result)
	{
		/*int ret = sceSaveDataClearProgress();

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}*/

		SUCCESS_RESULT(result);
	}
	
	float Progress::GetProgress(APIResult* result)
	{
		/*float progress;
		int ret = sceSaveDataGetProgress(&progress);

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return 0.0f;
		}*/

		SUCCESS_RESULT(result);

		//return progress;
		return 0.0f;
	}

}