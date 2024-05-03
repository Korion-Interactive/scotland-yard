// Copyright (c) 2018 Augie R. Maddox, Guavaman Enterprises. All rights reserved.

#region Defines

#if UNITY_2020 || UNITY_2021 || UNITY_2022 || UNITY_2023 || UNITY_2024 || UNITY_2025
#define UNITY_2020_PLUS
#endif
#if UNITY_2019 || UNITY_2020_PLUS
#define UNITY_2019_PLUS
#endif
#if UNITY_2018 || UNITY_2019_PLUS
#define UNITY_2018_PLUS
#endif
#if UNITY_2017 || UNITY_2018_PLUS
#define UNITY_2017_PLUS
#endif
#if UNITY_5 || UNITY_2017_PLUS
#define UNITY_5_PLUS
#endif
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_1_PLUS
#endif
#if UNITY_5_2 || UNITY_5_3_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_2_PLUS
#endif
#if UNITY_5_3_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_3_PLUS
#endif
#if UNITY_5_4_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_4_PLUS
#endif
#if UNITY_5_5_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_5_PLUS
#endif
#if UNITY_5_6_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_6_PLUS
#endif
#if UNITY_5_7_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_7_PLUS
#endif
#if UNITY_5_8_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_8_PLUS
#endif
#if UNITY_5_9_OR_NEWER || UNITY_2017_PLUS
#define UNITY_5_9_PLUS
#endif
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_PLUS
#define UNITY_4_6_PLUS
#endif
#pragma warning disable 0219
#pragma warning disable 0618
#pragma warning disable 0649
#pragma warning disable 0067
#endregion

namespace Rewired.Demos {
    using Korion.ScotlandYard.Input;
    using UnityEngine;
    using static Korion.ScotlandYard.Input.MultiplayerInputManager;

    [AddComponentMenu("")]
    public class PlayerMouseSpriteExample : MonoBehaviour {

#if UNITY_4_6_PLUS
        [Tooltip("The Player that will control the mouse")]
#endif
        public int playerId = 0;

#if UNITY_4_6_PLUS
        [Tooltip("The Rewired Action used for the mouse horizontal axis.")]
#endif
        public string horizontalAction = "MouseX";

#if UNITY_4_6_PLUS
        [Tooltip("The Rewired Action used for the mouse vertical axis.")]
#endif
        public string verticalAction = "MouseY";

#if UNITY_4_6_PLUS
        [Tooltip("The Rewired Action used for the mouse wheel axis.")]
#endif
        public string wheelAction = "MouseWheel";

#if UNITY_4_6_PLUS
        [Tooltip("The Rewired Action used for the mouse left button.")]
#endif
        public string leftButtonAction = "MouseLeftButton";

#if UNITY_4_6_PLUS
        [Tooltip("The Rewired Action used for the mouse right button.")]
#endif
        public string rightButtonAction = "MouseRightButton";

#if UNITY_4_6_PLUS
        [Tooltip("The Rewired Action used for the mouse middle button.")]
#endif
        public string middleButtonAction = "MouseMiddleButton";

#if UNITY_4_6_PLUS
        [Tooltip("The distance from the camera that the pointer will be drawn.")]
#endif
        public float distanceFromCamera = 1f;

#if UNITY_4_6_PLUS
        [Tooltip("The scale of the sprite pointer.")]
#endif
        public float spriteScale = 0.05f;

#if UNITY_4_6_PLUS
        [Tooltip("The pointer prefab.")]
#endif
        public GameObject pointerPrefab;

#if UNITY_4_6_PLUS
        [Tooltip("The click effect prefab.")]
#endif
        public GameObject clickEffectPrefab;

#if UNITY_4_6_PLUS
        [Tooltip("Should the hardware pointer be hidden?")]
#endif
        public bool hideHardwarePointer = true;

        public float PointerSpeed = 1;

        public Vector3 PointerPosition => pointer.transform.position;

        [System.NonSerialized]
        private GameObject pointer;

        [System.NonSerialized]
        private PlayerMouse mouse;

        public PlayerMouse Mouse => mouse;

        [SerializeField]
        private Camera _camera;
        
        private Camera _mainCamera;
        
        private bool _isTutorialPopupOpened;

        public bool CursorVisible { get; private set; }
        
        private static PlayerMouseSpriteExample _instance;
        
        public static PlayerMouseSpriteExample Instance => _instance;
        private bool _awakeFinished = false;
        private Vector3 _pointerOriginalPosition = Vector3.zero;

        void Awake() {

            if(_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            _mainCamera = Camera.main;
            
            pointer = Instantiate(pointerPrefab);
            _pointerOriginalPosition = pointer.transform.position;
            pointer.transform.localScale = new Vector3(spriteScale, spriteScale, spriteScale);

#if UNITY_5_PLUS
            if(hideHardwarePointer) Cursor.visible = false; // hide the hardware pointer
#endif

            // Create the Player Mouse
            mouse = PlayerMouse.Factory.Create();

            // Set the owner
            mouse.playerId = playerId;

            // Set up Actions for each axis and button
            mouse.xAxis.actionName = horizontalAction;
            mouse.yAxis.actionName = verticalAction;
            mouse.wheel.yAxis.actionName = wheelAction;
            mouse.leftButton.actionName = leftButtonAction;
            mouse.rightButton.actionName = rightButtonAction;
            mouse.middleButton.actionName = middleButtonAction;

            // If you want to change joystick pointer speed
            mouse.pointerSpeed = PointerSpeed;

            // If you want to change the wheel to tick more often
            mouse.wheel.yAxis.repeatRate = 5;

            // If you want to set the screen position
            mouse.screenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            // If you want to use Actions to drive the X/Y position of the mouse
            // instead of using the hardware cursor position
            // mouse.useHardwareCursorPositionForMouseInput = false;

            // Additionally you'd need to bind mouse X/Y to the X/Y Actions on the Player's Mouse Map.
            // The result of this is that the mouse pointer will no longer pop to the hardware cursor
            // position when you start using the mouse. You would also need to hide the mouse
            // pointer using Cursor.visible = false;

            // Subscribe to position changed event (or you could just poll for it)
            mouse.ScreenPositionChangedEvent += OnScreenPositionChanged;

            // Get the initial position
            OnScreenPositionChanged(mouse.screenPosition);

            CursorVisible = pointer.activeInHierarchy;

            MultiplayerInputManager.onPlayerChanged += OnPlayerChanged;

        }

        private void OnPlayerChanged(Player player)
        {
            mouse.playerId = MultiplayerInputManager.Instance.CurrentPlayer.id;
        }

        void Update() {
            if (!ReInput.isReady || !CursorVisible) return;

            // Use the mouse wheel to rotate the pointer
            pointer.transform.Rotate(Vector3.forward, mouse.wheel.yAxis.value * 20f);

            // Use the left or right button to create an object where you clicked
            /*
            if (mouse.leftButton.justPressed) CreateClickEffect(new Color(0f, 1f, 0f, 1f)); // green for left
            if (mouse.rightButton.justPressed) CreateClickEffect(new Color(1f, 0f, 0f, 1f)); // red for right
            if(mouse.middleButton.justPressed) CreateClickEffect(new Color(1f, 1f, 0f, 1f)); // yellow for middle
         */   
        }
        void OnDestroy() {

            MultiplayerInputManager.onPlayerChanged -= OnPlayerChanged;

            if (!ReInput.isReady) return;
            mouse.ScreenPositionChangedEvent -= OnScreenPositionChanged;
        }

        void CreateClickEffect(Color color) {
            GameObject go = Instantiate(clickEffectPrefab);
            go.transform.localScale = new Vector3(spriteScale, spriteScale, spriteScale);
            if (_mainCamera)
            {
                go.transform.position = _mainCamera.ScreenToWorldPoint(new Vector3(mouse.screenPosition.x, mouse.screenPosition.y, distanceFromCamera));
            }
            else if(_camera)
            {
                go.transform.position = _camera.ScreenToWorldPoint(new Vector3(mouse.screenPosition.x, mouse.screenPosition.y, distanceFromCamera));
            }
            else if(UICamera.currentCamera)
            {
                go.transform.position = UICamera.currentCamera.ScreenToWorldPoint(new Vector3(mouse.screenPosition.x, mouse.screenPosition.y, distanceFromCamera));
            }
            go.GetComponentInChildren<SpriteRenderer>().color = color;
            Object.Destroy(go, 0.5f);
        }

        // Callback when the screen position changes
        void OnScreenPositionChanged(Vector2 position) {

            // Convert from screen space to world space
            Vector3 worldPos = Vector3.zero;
            if (_mainCamera)
            {
                worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, distanceFromCamera));
            }
            else if(_camera != null) //Ingame
            {
                worldPos = _camera.ScreenToWorldPoint(new Vector3(position.x, position.y, distanceFromCamera));
            }
            else if (UICamera.currentCamera != null)
            {
                worldPos = UICamera.currentCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, distanceFromCamera));
            }

            // Move the pointer object
            pointer.transform.position = worldPos;
        }

        public void SetVisibility(bool cursorVisible)
        {
            //TODO: Check Controller Type
            Debug.Log("SetVisibility: " + cursorVisible, pointer);
            
            pointer.SetActive(cursorVisible);
            CursorVisible = cursorVisible;
        }

        public void DisableCursor()
        {
            SetVisibility(false);
        }

        public void EnableCursor()
        {
            SetVisibility(true);
        }

        public void ResetCursorPosition()
        {
            if (_mainCamera)
            {
                mouse.screenPosition = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
                pointer.transform.position = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
            }
            else if (_camera != null)
            {
                mouse.screenPosition = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
                pointer.transform.position = _camera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
            }
            else
            {
                mouse.screenPosition = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);
                pointer.transform.position = UICamera.currentCamera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
            }
        }
    }
}