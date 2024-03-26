#ifndef _TRANSACTIONS_H
#define _TRANSACTIONS_H

#include "Core.h"
#include <map>

namespace SaveData
{
	class Transaction
	{
	public:

		Transaction() :
			m_TransactionId(SCE_SAVE_DATA_TRANSACTION_RESOURCE_ID_INVALID)
		{

		}

		SceSaveDataTransactionResourceId m_TransactionId;
		SceSaveDataMountPoint m_MountPoint;
	};

	class Transactions
	{
	public:

		static Transaction* CreateTransactionId(uint32_t size = SCE_SAVE_DATA_TRANSACTION_RESOURCE_MIN_SIZE);
		static int DeleteTransactionId(Transaction* id);

		static void RecordMountedTransaction(Transaction* id, SceSaveDataMountPoint& mp);
		static int RemoveTransaction(Transaction* id);

		static Transaction* FindTransaction(SceSaveDataMountPoint& mp);

		static std::list<Transaction*> s_TransactionList;
	};
}

#endif