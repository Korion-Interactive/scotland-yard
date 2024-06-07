using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RebuildMoveHistory : MonoBehaviour
{
    [SerializeField]
    private UIScrollView _scrollView;

    private Coroutine _rebuildBoundsDelayed = null;
    private void OnEnable()
    {
        if(_rebuildBoundsDelayed != null)
        {
            StopCoroutine(_rebuildBoundsDelayed);
        }

        _rebuildBoundsDelayed = StartCoroutine(RebuildBoundsDelayed());
    }

    private IEnumerator RebuildBoundsDelayed()
    {
        yield return new WaitForEndOfFrame();
        
        _scrollView.MoveRelative(new Vector3(0, 100, 0));
        _scrollView.panel.RebuildAllDrawCalls();
        _scrollView.ResetPosition();
        _rebuildBoundsDelayed = null;
    }
}
