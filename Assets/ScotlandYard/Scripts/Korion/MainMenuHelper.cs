using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MainMenuHelper : MonoBehaviour
{
    [SerializeField]
    private UIEventListener _eventListener;

    [SerializeField]
    private UnityEvent _onHover;

    [SerializeField]
    private GameObject _activeGameObjectDependency;

    // Start is called before the first frame update
    void Start()
    {
        _eventListener.onSelect += Hover;
    }

    private void Hover(GameObject go, bool state)
    {
        if (_activeGameObjectDependency != null && _activeGameObjectDependency.activeSelf)
        {
            _onHover?.Invoke();
        }
        
    }
    
}
