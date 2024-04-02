
#include "Dialogs.h"
#include <kernel.h>

namespace SaveData
{
	PRX_EXPORT void PrxSaveDataOpenDialog(Int32 userId, OpenDialogSettings* basicSettings, Items* itemsSettings, UserMessage* userMsgSettings, SystemMessage* sysMsgSettings,
		ErrorCode* errorCodeSettings, ProgressBar* progressBarSettings, NewItem* newItemSettings, OptionParam* optionSettings, 
		AnimationParam* animations, APIResult* result)
	{
		Dialogs::OpenDialog(userId, basicSettings, itemsSettings, userMsgSettings, sysMsgSettings, errorCodeSettings, progressBarSettings, newItemSettings, optionSettings, animations, result);
	}

	PRX_EXPORT int PrxSaveDataDialogUpdateStatus()
	{
		return Dialogs::DialogUpdateStatus();
	}

	PRX_EXPORT int PrxSaveDataDialogGetStatus()
	{
		return Dialogs::DialogGetStatus();
	}

	PRX_EXPORT int PrxSaveDataDialogIsReadyToDisplay(APIResult* result)
	{
		return Dialogs::DialogIsReadyToDisplay(result);
	}

	PRX_EXPORT void PrxSaveDataDialogGetResult(MemoryBufferManaged* outBuffer, APIResult* result)
	{
		Dialogs::DialogGetResult(outBuffer, result);
	}

	PRX_EXPORT void PrxSaveDataDialogProgressBarInc(UInt32 delta, APIResult* result)
	{
		Dialogs::ProgressBarInc(delta, result);
	}

	PRX_EXPORT void PrxSaveDataDialogProgressBarSetValue(UInt32 rate, APIResult* result)
	{
		Dialogs::ProgressBarSetValue(rate, result);
	}

	PRX_EXPORT void PrxSaveDataDialogClose(CloseParam* close, APIResult* result)
	{
		Dialogs::Close(close, result);
	}

	PRX_EXPORT void PrxSaveDataInitializeDialog(APIResult* result)
	{
		Dialogs::InitializeDialog(result);
	}

	PRX_EXPORT void PrxSaveDataTerminateDialog(APIResult* result)
	{
		Dialogs::TerminateDialog(result);
	}

	void Dialogs::InitializeDialog(APIResult* result)
	{
		int32_t ret = sceSaveDataDialogInitialize();

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}

	void Dialogs::TerminateDialog(APIResult* result)
	{
		int32_t ret = sceSaveDataDialogTerminate();

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}

	int Dialogs::DialogUpdateStatus()
	{
		SceCommonDialogStatus stat = sceSaveDataDialogUpdateStatus();
		return stat;
	}

	int Dialogs::DialogGetStatus()
	{
		SceCommonDialogStatus stat = sceSaveDataDialogGetStatus();
		return stat;
	}

	int Dialogs::DialogIsReadyToDisplay(APIResult* result)
	{
		int ret = sceSaveDataDialogIsReadyToDisplay();

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return ret;
		}

		SUCCESS_RESULT(result);

		return ret;
	}

	void Dialogs::DialogGetResult(MemoryBufferManaged* outBuffer, APIResult* result)
	{
		SceSaveDataDialogResult dialogResult;
		SceSaveDataDirName dirName;
		SceSaveDataParam param;

		memset(&dialogResult, 0, sizeof(dialogResult));
		memset(&dirName, 0, sizeof(dirName));
		memset(&param, 0, sizeof(param));

		dialogResult.dirName = &dirName;
		dialogResult.param = &param;

		int ret = sceSaveDataDialogGetResult(&dialogResult);

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		// Write the results
		MemoryBuffer buffer = MemoryBuffer::GetBuffer();
		buffer.StartResponseWrite();

		// Write the save data directories
		buffer.WriteInt32(dialogResult.mode);
		buffer.WriteInt32(dialogResult.result);
		buffer.WriteInt32(dialogResult.buttonId);

		// Write directory name, params and info
		Core::WriteToBuffer(*dialogResult.dirName, buffer);
		Core::WriteToBuffer(*dialogResult.param, buffer);

		//outBuffer
		buffer.FinishResponseWrite();
		buffer.CopyTo(outBuffer);

		SUCCESS_RESULT(result);
	}

	void OpenDialogSettings::CopyTo(SceSaveDataDialogParam &destination)
	{
		sceSaveDataDialogParamInitialize(&destination);

		destination.mode = mode;
		destination.dispType = dispType;
	}

	void UserMessage::CopyTo(SceSaveDataDialogUserMessageParam& destination, SceSaveDataDialogParam &params)
	{	
		memset(&destination, 0, sizeof(destination));

		destination.buttonType = buttonType;
		destination.msgType = msgType;
		destination.msg = msg;

		params.userMsgParam = &destination;
	}

	void AnimationParam::CopyTo(SceSaveDataDialogAnimationParam& destination, SceSaveDataDialogParam &params)
	{
		memset(&destination, 0, sizeof(destination));

		destination.userOK = userOK;
		destination.userCancel = userCancel;

		params.animParam = &destination;
	}

	void SystemMessage::CopyTo(SceSaveDataDialogSystemMessageParam& destination, SceSaveDataDialogParam &params)
	{
		memset(&destination, 0, sizeof(destination));

		destination.sysMsgType = sysMsgType;
		destination.value = value;

		params.sysMsgParam = &destination;
	}

	void ErrorCode::CopyTo(SceSaveDataDialogErrorCodeParam& destination, SceSaveDataDialogParam &params)
	{
		memset(&destination, 0, sizeof(destination));

		destination.errorCode = errorCode;

		params.errorCodeParam = &destination;
	}

	void ProgressBar::CopyTo(SceSaveDataDialogProgressBarParam& destination, SceSaveDataDialogParam &params)
	{
		memset(&destination, 0, sizeof(destination));

		destination.barType = barType;
		destination.sysMsgType = sysMsgType;
		destination.msg = msg;

		params.progBarParam = &destination;
	}

	void OptionParam::CopyTo(SceSaveDataDialogOptionParam& destination, SceSaveDataDialogParam &params)
	{
		memset(&destination, 0, sizeof(destination));

		destination.back = back;

		params.optionParam = &destination;
	}

	void Items::CopyTo(SceSaveDataDialogItems& destination, SceSaveDataDialogParam &params, SceSaveDataDirName& focusDirName)
	{
		memset(&destination, 0, sizeof(destination));

		if (dirNameNum > 0)
		{
			SceSaveDataDirName* nativeDirNames = Core::GetTempDialogDirNamesArray();

			destination.dirName = nativeDirNames;
			destination.dirNameNum = dirNameNum;

			for (int i = 0; i < dirNameNum; i++)
			{
				dirNames[i].CopyTo(nativeDirNames[i]);
			}
		}
		else
		{
			destination.dirName = NULL;
			destination.dirNameNum = 0;
		}

		destination.focusPos = focusPos;
		destination.itemStyle = itemStyle;

		if (focusPos == SCE_SAVE_DATA_DIALOG_FOCUS_POS_DIRNAME)
		{
			focusPosDirName.CopyTo(focusDirName);
			destination.focusPosDirName = &focusDirName;
		}

		params.items = &destination;
	}

	void NewItem::CopyTo(SceSaveDataDialogNewItem& destination, SceSaveDataDialogItems &items)
	{
		memset(&destination, 0, sizeof(destination));

		destination.title = title;
		destination.iconBuf = iconBuf;
		destination.iconSize = iconSize;

		items.newItem = &destination;
	}

	void Dialogs::ProgressBarInc(UInt32 delta, APIResult* result)
	{
		int ret = sceSaveDataDialogProgressBarInc(SCE_SAVE_DATA_DIALOG_PROGRESSBAR_TARGET_BAR_DEFAULT, delta);

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}

	void Dialogs::ProgressBarSetValue(UInt32 rate, APIResult* result)
	{
		int ret = sceSaveDataDialogProgressBarSetValue(SCE_SAVE_DATA_DIALOG_PROGRESSBAR_TARGET_BAR_DEFAULT, rate);

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}

	void CloseParam::CopyTo(SceSaveDataDialogCloseParam &destination)
	{
		memset(&destination, 0x00, sizeof(destination));
		destination.anim = anim;
	}

	void Dialogs::Close(CloseParam* close, APIResult* result)
	{
		SceSaveDataDialogCloseParam closeParam;

		close->CopyTo(closeParam);

		int ret = sceSaveDataDialogClose(&closeParam);

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}

	void Dialogs::OpenDialog(Int32 userId, OpenDialogSettings* basicSettings, Items* itemsSettings, UserMessage* userMsgSettings, SystemMessage* sysMsgSettings,
		                     ErrorCode* errorCodeSettings, ProgressBar* progressBarSettings, NewItem* newItemSettings, OptionParam* optionSettings,
		                     AnimationParam* animations, APIResult* result)
	{
		SceSaveDataDialogParam param;
		SceSaveDataDialogUserMessageParam userMsgParam;
		SceSaveDataDialogAnimationParam animParam;
		SceSaveDataDialogSystemMessageParam sysMsgParam;
		SceSaveDataDialogErrorCodeParam errorParam;
		SceSaveDataDialogProgressBarParam barParam;
		SceSaveDataDialogNewItem newItem;
		SceSaveDataDialogItems items;
		SceSaveDataDialogOptionParam optionParam;
		SceSaveDataDirName focusDirName;

		// first setup the basic params
		basicSettings->CopyTo(param);

		if (itemsSettings != NULL)
		{
			itemsSettings->CopyTo(items, param, focusDirName);
		}
		else
		{
			memset(&items, 0, sizeof(items));
			param.items = &items;
		}

		param.items->userId = userId;

		if (animations != NULL)
		{
			animations->CopyTo(animParam, param);			
		}

		if (newItemSettings != NULL)
		{
			newItemSettings->CopyTo(newItem, items);
		}

		if (optionSettings != NULL)
		{
			optionSettings->CopyTo(optionParam, param);
		}

		if (param.mode == SCE_SAVE_DATA_DIALOG_MODE_USER_MSG && userMsgSettings != NULL)
		{
			userMsgSettings->CopyTo(userMsgParam, param);
		}
		else if (param.mode == SCE_SAVE_DATA_DIALOG_MODE_SYSTEM_MSG && sysMsgSettings != NULL)
		{
			sysMsgSettings->CopyTo(sysMsgParam, param);
		}
		else if (param.mode == SCE_SAVE_DATA_DIALOG_MODE_ERROR_CODE && errorCodeSettings != NULL)
		{
			errorCodeSettings->CopyTo(errorParam, param);
		}
		else if (param.mode == SCE_SAVE_DATA_DIALOG_MODE_PROGRESS_BAR && progressBarSettings != NULL)
		{
			progressBarSettings->CopyTo(barParam, param);
		}

		int ret = sceSaveDataDialogOpen(&param);

		if (ret < 0)
		{
			SCE_ERROR_RESULT(result, ret);
			return;
		}

		SUCCESS_RESULT(result);
	}

}