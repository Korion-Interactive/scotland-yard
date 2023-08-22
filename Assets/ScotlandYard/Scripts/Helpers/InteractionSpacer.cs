using UnityEngine;
using System.Collections;

public static class InteractionSpacer 
{
	private const float MINIMUM_GAP_BETWEEN_INTERACTIONS = 0.3f;

	private static float _lastInteraction = -1.0f;
	public static bool IsTooNarrow()
	{
		if (Block)
		{
			return true;
		}

		float delta = Time.realtimeSinceStartup - _lastInteraction;
		bool isTooNarrow = false;
		if (delta < MINIMUM_GAP_BETWEEN_INTERACTIONS)
		{
			isTooNarrow = true;	
		}
		else
		{
			_lastInteraction = Time.realtimeSinceStartup;
		}
		//Debug.Log("InteractionSpacer IsTooNarrow " + isTooNarrow + " delta="+delta);
		return isTooNarrow;
	}

	private static int _blockCount = 0;
	public static bool Block 
	{
		get { return _blockCount != 0; }
		set 
		{
			if (value)
			{
				_blockCount++;
				//Debug.Log("InteractionSpacer.Block ++ to " + _blockCount);
			}
			else
			{
				_blockCount--;
				//Debug.Log("InteractionSpacer.Block -- to " + _blockCount);
			}
		}
	}

	public static void BlockInteractions()
	{
		Block = true;
	}

	public static void UnblockInteractions()
	{
		Block = false;
	}
}
