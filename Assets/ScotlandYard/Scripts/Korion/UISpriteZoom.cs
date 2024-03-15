using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISpriteZoom : MonoBehaviour
{

    [SerializeField]
    private UITexture _sprite;

    [SerializeField]
    private float _minZoom, _maxZoom;

    [SerializeField]
    private float _duration;

    private float t = 0;
    private Vector3 _origin;
    private Vector3 _target;

    private bool _animateBackwards;


    // Start is called before the first frame update
    void Start()
    {
        _origin = _sprite.transform.localScale;
        _target = new Vector3(_minZoom, _minZoom, _minZoom);
    }

    // Update is called once per frame
    void Update()
    {
        if (t < 1)
        {
            t += Time.deltaTime / _duration;
        }
        else
        {
            _animateBackwards = !_animateBackwards;
            _origin = _sprite.transform.localScale;
            if (_animateBackwards)
            {
                _origin = _target;
                _target = new Vector3(_maxZoom, _maxZoom, _maxZoom);
                
            }
            else
            {
                _origin = _target;
                _target = new Vector3(_minZoom, _minZoom, _minZoom);
            }
            t = 0;
        }

        if (_animateBackwards)
        {
            _sprite.transform.localScale = Vector3.Lerp(_origin, _target, t);
        }
        else
        {
            _sprite.transform.localScale = Vector3.Lerp(_origin, _target, t);
        }

        
    }
}
