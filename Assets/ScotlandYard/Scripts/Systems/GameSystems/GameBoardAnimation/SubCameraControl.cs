using Korion.ScotlandYard.Input;
using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SubCameraControl : SubSystem<GameBoardAnimationSystem>
{
    const float MIN_ZOOM = 0.5f;
    const float MAX_ZOOM = 2.25f;
    const float DRAG_SLOW_DOWN = 5f;

    protected override bool needsUpdate { get { return true; } }
    public Camera Cam;// { get { return System.GameCamera; } }
    public BoxCollider ViewBounds;

    public List<Vector3> AdditionalFocusPoints = new List<Vector3>();

    Vector3 dragVelocity = Vector3.zero;

    bool isGesturesEnabled = true;
    public bool IsGesturesEnabled { get { return isGesturesEnabled; } set { if (!LockGestureEnabledStatus) isGesturesEnabled = value; } }
    public bool LockGestureEnabledStatus = false;

    [Header("Rewired")]

    [SerializeField]
    private int _playerId = 0;

    [SerializeField]
    private Rewired.Player _player;

    private Vector2 _rightJoystickInput = Vector2.zero;

    internal override void RegisterEvents()
    {
        System.ListenTo(GameEvents.TurnStart, TurnStart);
        System.ListenTo(GameEvents.MrXAppear, MrXAppear);
        System.ListenTo<MoveArgs>(GameEvents.DetectiveMove, DetectiveMove);
        System.ListenTo<MoveArgs>(GameEvents.MrXMove, MrXMove);
        System.ListenTo(GameGuiEvents.FocusPosition, FocusOnGameObject);
        
        Gesture.onPinchE += Gesture_onPinchE;
        Gesture.onDraggingE += Gesture_onDraggingE;

        if (ReInput.isReady)
            _player = ReInput.players.GetPlayer(0);

        MultiplayerInputManager.onPlayerChanged += PlayerChanged;

        PlayerChanged(_player);
    }

    private void PlayerChanged(Player player)
    {
        _player.RemoveInputEventDelegate(OnRightStickX);
        _player.RemoveInputEventDelegate(OnRightStickY);

        _player = player;

        _player.AddInputEventDelegate(OnRightStickX, UpdateLoopType.Update, "CameraX");
        _player.AddInputEventDelegate(OnRightStickY, UpdateLoopType.Update, "CameraY");
    }

    internal override void OnDestroy()
    {
        base.OnDestroy();

        Gesture.onPinchE -= Gesture_onPinchE;
        Gesture.onDraggingE -= Gesture_onDraggingE;

        MultiplayerInputManager.onPlayerChanged -= PlayerChanged;
    }

    bool Check(object info)
    {
        if (GameState.Instance.IsGamePaused || !IsGesturesEnabled)
            return true;

        if (info == null)
        {
            this.LogError("info is null!");
            return true;
        }
        else if (Cam == null)
        {
            this.LogError("Cam is null!");
            return true;
        }
        return false;
    }

    void Gesture_onDraggingE(DragInfo dragInfo)
    {
        if (Check(dragInfo))
            return;
        
        dragVelocity = (new Vector3(dragInfo.delta.x, dragInfo.delta.y) / 384) * Cam.orthographicSize;
    }

    void Gesture_onPinchE(PinchInfo info)
    {
        if (Check(info))
            return;

        Cam.orthographicSize = Helpers.Clamp(Cam.orthographicSize + info.magnitude / 384, MIN_ZOOM, MAX_ZOOM);
    }
    void DetectiveMove(MoveArgs args)
    {
        FocusCamera(args.From.transform.position, args.To.transform.position);
    }
    void MrXMove(MoveArgs args)
    {
        var mrX = GameState.Instance.MrX;

        if (mrX.PlayerInfo.Controller == PlayerController.Human)
            FocusCamera(args.From.transform.position, args.To.transform.position);
        else if (mrX.AppearsInXMoves() == 0)
            FocusCamera(args.To.transform.position);
    }
    private void MrXAppear(BaseArgs args)
    {
        FocusCamera(args.RelatedObject.transform.position);
    }


    private void TurnStart(BaseArgs args)
    {
        if (!System.AutoFocusEnabled)
            return;

        PlayerBase player = args.RelatedObject.GetComponent<PlayerBase>();
        if (player.PlayerInfo.Controller == PlayerController.Human)
        {
            var positions = player.GetReachableNeighbours().Select((o) => o.transform.position).Concat(player.transform.position).ToArray();
            FocusCamera(positions);
        }
    }

    public void FocusCamera(params Vector3[] positions)
    {
        FocusCam(-1, positions);
    }

    public void FocusCam(float targetZoom, params Vector3[] positions)
    {
        if (targetZoom > 0 || Cam.orthographicSize < 1)
        {

            iTween.ValueTo(Cam.gameObject,
                iTween.Hash(
                "from", Cam.orthographicSize,
                "to", (targetZoom < 0) ? 1 : targetZoom,
                "time", 1f,
                "easetype", "easeOutCubic",
                "onupdate", "UpdateCameraSize",
                "onupdatetarget", System.gameObject
                ));
        }

        if(AdditionalFocusPoints.Count > 0)
            positions = positions.Concat(AdditionalFocusPoints).ToArray();

        if (positions.Length > 0)
        {
            Vector3 focus = new Vector3();
            foreach (Vector3 vec in positions)
                focus += vec;
            focus /= positions.Length;
            focus.z = Cam.transform.position.z;

            iTween.ValueTo(Cam.gameObject,
                iTween.Hash(
                "from", Cam.transform.position,
                "to", focus,
                "time", 1f,
                "easetype", "easeOutCubic",
                "onupdate", "UpdateCameraPos",
                "onupdatetarget", System.gameObject
                ));
        }
    }

    internal void UpdateCameraSize(float val)
    {
        Cam.orthographicSize = val;
    }
    internal void UpdateCameraPos(Vector3 val)
    {
        Cam.transform.position = val;
        KeepCamInBounds();
    }
    void KeepCamInBounds()
    {
        Vector3 topLeftCam = Cam.ViewportToWorldPoint(new Vector3(0, 0, 1));
        Vector3 botRightCam = Cam.ViewportToWorldPoint(new Vector3(1, 1, 1));
        
        Vector3 topLeftBounds = ViewBounds.transform.position + ViewBounds.transform.lossyScale.x * (ViewBounds.center - 0.5f * ViewBounds.size);
        Vector3 botRightBounds = ViewBounds.transform.position + ViewBounds.transform.lossyScale.x * (ViewBounds.center + 0.5f * ViewBounds.size);

        float vShift = Helpers.Clamp(0, topLeftBounds.y - topLeftCam.y, botRightBounds.y - botRightCam.y);
        float hShift = Helpers.Clamp(0, topLeftBounds.x - topLeftCam.x, botRightBounds.x - botRightCam.x);

        Vector3 shift = new Vector3(hShift, vShift);

        if (shift.sqrMagnitude > 0.000001f)
        {
            Cam.transform.position += shift;

            this.Broadcast<GameGuiEvents>(GameGuiEvents.KeepCamInBounds, this.System.gameObject, new VectorEventArgs() { Vector = shift });
        }
    }

    protected override void Update()
    {
        if (!GameState.Instance.IsGamePaused && IsGesturesEnabled)
        {
            var wheel = 0.1f * Math.Sign(Input.GetAxis("Mouse ScrollWheel"));
#if UNITY_SWITCH || UNITY_STANDALONE || UNITY_PS4 || UNITY_PS5
            if (_player != null)
            {
                wheel = _player.GetAxis("MouseWheel") * 0.01f;
            }
#endif
            
            if (wheel != 0)
                Cam.orthographicSize = Helpers.Clamp(Cam.orthographicSize - wheel, MIN_ZOOM, MAX_ZOOM);


            if (dragVelocity.sqrMagnitude > 0.00001f)
            {
                Cam.transform.position -= dragVelocity;

                dragVelocity *= 1 - (Time.deltaTime * DRAG_SLOW_DOWN);
            }

            if(_rightJoystickInput.x != 0 || _rightJoystickInput.y != 0)
            {
                DragInfo dragInfo = new DragInfo(Vector2.zero, _rightJoystickInput * 10, 1, 0, false, false);
                Gesture_onDraggingE(dragInfo);
            }
        }
        KeepCamInBounds();
    }

    void FocusOnGameObject(BaseArgs args)
    {
        if (GameState.Instance.IsGamePaused)
            return;

        FocusCamera(args.RelatedObject.transform.position);
    }

    private void OnRightStickY(InputActionEventData data)
    {
        if (data.GetAxis() != 0)
        {
            _rightJoystickInput.y = -data.GetAxis();
        }
        else if(_rightJoystickInput.y != 0)
        {
            _rightJoystickInput.y = 0;
        }
    }

    private void OnRightStickX(InputActionEventData data)
    {
        if (data.GetAxis() != 0)
        {
            _rightJoystickInput.x = -data.GetAxis();
        }
        else if (_rightJoystickInput.x != 0)
        {
            _rightJoystickInput.x = 0;
        }
    }
}
