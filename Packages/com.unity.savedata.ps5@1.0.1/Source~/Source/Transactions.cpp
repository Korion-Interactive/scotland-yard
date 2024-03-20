
#include "Transactions.h"

namespace SaveData
{
	std::list<Transaction*> Transactions::s_TransactionList;

	Transaction* Transactions::CreateTransactionId(uint32_t size /*= SCE_SAVE_DATA_TRANSACTION_RESOURCE_MIN_SIZE*/)
	{
		if (size < SCE_SAVE_DATA_TRANSACTION_RESOURCE_MIN_SIZE) size = SCE_SAVE_DATA_TRANSACTION_RESOURCE_MIN_SIZE;

		int32_t ret = sceSaveDataCreateTransactionResource(size);

		if (ret < 0)
		{
			return NULL;
		}

		Transaction* id = new Transaction();

		id->m_TransactionId = ret;

		return id;
	}

	int Transactions::DeleteTransactionId(Transaction* id)
	{
		if (id == NULL)
		{
			return 0;
		}

		int ret = sceSaveDataDeleteTransactionResource(id->m_TransactionId);

		delete id;

		return ret;
	}

	void Transactions::RecordMountedTransaction(Transaction* id, SceSaveDataMountPoint& mp)
	{
		id->m_MountPoint = mp;

		// Add transaction to map
		s_TransactionList.push_back(id);
	}

	int Transactions::RemoveTransaction(Transaction* id)
	{
		if (id == NULL)
		{
			return 0;
		}

		s_TransactionList.remove(id);

		return DeleteTransactionId(id);
	}

	Transaction* Transactions::FindTransaction(SceSaveDataMountPoint& mp)
	{
		for (std::list<Transaction*>::iterator it = s_TransactionList.begin(); it != s_TransactionList.end(); ++it)
		{
			Transaction* trans = *it;

			if (strcmp(trans->m_MountPoint.data, mp.data) == 0)
			{
				return trans;
			}
		}

		return NULL;
	}

}