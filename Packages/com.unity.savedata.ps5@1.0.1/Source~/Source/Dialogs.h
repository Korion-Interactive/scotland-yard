#ifndef _DIALOGS_H
#define _DIALOGS_H

#include "../Includes/PluginCommonIncludes.h"
#include "Core.h"

namespace SaveData
{
	//class OpenDialogRequest : public RequestBaseManaged
	//{
	//public:
	//	SceSaveDataDialogMode mode;
	//	SceSaveDataDialogType dispType;

	//	void CopyTo(SceSaveDataDialogParam &destination);
	//};

	class OpenDialogSettings
	{
	public:
		SceSaveDataDialogMode mode;
		SceSaveDataDialogType dispType;

		void CopyTo(SceSaveDataDialogParam &destination);
	};

	class UserMessage
	{
	public:
		SceSaveDataDialogButtonType			buttonType;
		SceSaveDataDialogUserMessageType	msgType;

		char msg[SCE_SAVE_DATA_DIALOG_USER_MSG_MAXSIZE];

		void CopyTo(SceSaveDataDialogUserMessageParam &destination, SceSaveDataDialogParam &params);
	};

	class AnimationParam
	{
	public:
		SceSaveDataDialogAnimation userOK;
		SceSaveDataDialogAnimation userCancel;

		void CopyTo(SceSaveDataDialogAnimationParam &destination, SceSaveDataDialogParam &params);
	};

	class SystemMessage
	{
	public:
		SceSaveDataDialogSystemMessageType	sysMsgType;
		UInt64 value;

		void CopyTo(SceSaveDataDialogSystemMessageParam &destination, SceSaveDataDialogParam &params);
	};

	class ErrorCode
	{
	public:
		Int32 errorCode;

		void CopyTo(SceSaveDataDialogErrorCodeParam &destination, SceSaveDataDialogParam &params);
	};

	class Items
	{
	public:

		DirNameManaged dirNames[SCE_SAVE_DATA_DIRNAME_MAX_COUNT];
		UInt32 dirNameNum;

		SceSaveDataDialogFocusPos focusPos;

		DirNameManaged focusPosDirName;

		SceSaveDataDialogItemStyle itemStyle;

		void CopyTo(SceSaveDataDialogItems &destination, SceSaveDataDialogParam &params, SceSaveDataDirName& focusDirName);
	};

	class NewItem
	{
	public:

		char iconPath[SCE_SAVE_DATA_ICON_PATH_MAXSIZE];

		char title[SCE_SAVE_DATA_TITLE_MAXSIZE];

		void *iconBuf;
		UInt64 iconSize;

		void CopyTo(SceSaveDataDialogNewItem &newItem, SceSaveDataDialogItems &items);
	};

	class ProgressBar
	{
	public:

		SceSaveDataDialogProgressBarType			barType;
		SceSaveDataDialogProgressSystemMessageType	sysMsgType;

	    char msg[SCE_SAVE_DATA_DIALOG_USER_MSG_MAXSIZE];

		void CopyTo(SceSaveDataDialogProgressBarParam &destination, SceSaveDataDialogParam &params);
	};

	class OptionParam
	{
	public:

		SceSaveDataDialogOptionBack back;

		void CopyTo(SceSaveDataDialogOptionParam &destination, SceSaveDataDialogParam &params);
	};

	class CloseParam
	{
	public:
		SceSaveDataDialogAnimation anim;

		void CopyTo(SceSaveDataDialogCloseParam &destination);
	};

	class Dialogs
	{
	public:

		static void OpenDialog(Int32 userId, OpenDialogSettings* basicSettings, Items* itemsSettings, UserMessage* userMsgSettings, SystemMessage* sysMsgSettings,
			                   ErrorCode* errorCodeSettings, ProgressBar* progressBarSettings, NewItem* newItemSettings, OptionParam* optionSettings, 
			                   AnimationParam* animations, APIResult* result);

		static void Close(CloseParam* close, APIResult* result);

		static int DialogUpdateStatus();
		static int DialogGetStatus();
		static int DialogIsReadyToDisplay(APIResult* result);

		static void DialogGetResult(MemoryBufferManaged* outBuffer, APIResult* result);
		static void ProgressBarInc(UInt32 delta, APIResult* result);
		static void ProgressBarSetValue(UInt32 rate, APIResult* result);
		static void InitializeDialog(APIResult* result);
		static void TerminateDialog(APIResult* result);

	};
}

#endif	//_DELETE_H

