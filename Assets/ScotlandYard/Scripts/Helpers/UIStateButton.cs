using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// component 'UIStateButton'
/// switches the button appearance on click
/// </summary>

[AddComponentMenu("NGUI/Bit Barons/State Button")]
public class UIStateButton : MonoBehaviour
{
    [Serializable]
    public class ButtonState
    {
        public string SpriteNameNormal, SpriteNameHover, SpriteNamePressed, SpriteNameDisabled;

        public Color ColorNormal = Color.white;
        public Color ColorHover = Color.white;
        public Color ColorPressed = Color.white;
        public Color ColorDisabled = Color.white;

        public Color ColorNormal2 = Color.white;
        public Color ColorHover2 = Color.white;
        public Color ColorPressed2 = Color.white;
        public Color ColorDisabled2 = Color.white;

        public string LabelTextId;

        public Color LabelColor = Color.white;
    }

    public List<int> StateSwitchOrder = new List<int>();
    int stateSwitchOrderIndex = 0;


    public UIButton Button;
    public UIButtonColor ColorButton, ColorButton2;
    public LabelTranslator Label;

    public List<ButtonState> States = new List<ButtonState>();
    int currentState = 0;

    public List<EventDelegate> stateChangedListeners = new List<EventDelegate>();

    public int CurrentStateIndex { get { return currentState; } set { ApplyState(value); } }
    public ButtonState CurrentState { get { return States[currentState]; } }

    public void TriggerOnClick()
    {
        OnClick();
    }

    void OnClick()
    {
        if (!enabled)
            return;

        if (StateSwitchOrder.Count == 0)
        {
            ApplyState((currentState + 1) % States.Count);
        }
        else
        {
            stateSwitchOrderIndex = (stateSwitchOrderIndex + 1) % StateSwitchOrder.Count;
            int state = StateSwitchOrder[stateSwitchOrderIndex];
            ApplyState(state);
        }
    }

    public void ApplyState(int state)
    {
        if (currentState == state)
            return;

        currentState = state;

        if(StateSwitchOrder.Count > 0)
        {
            int idx = StateSwitchOrder.IndexOf(state);
            if(idx >= 0 && StateSwitchOrder[stateSwitchOrderIndex] != state)
            {
                stateSwitchOrderIndex = idx;
            }
        }


        ButtonState b = States[currentState];

        if(Button != null)
        {
            Button.normalSprite = b.SpriteNameNormal;
            Button.hoverSprite = b.SpriteNameHover;
            Button.pressedSprite = b.SpriteNamePressed;
            Button.disabledSprite = b.SpriteNameDisabled;
        }

        if(ColorButton != null)
        {
            this.LogDebug("Set button color... " + b.ColorNormal.ToString());
            ColorButton.defaultColor = b.ColorNormal;
            ColorButton.hover = b.ColorHover;
            ColorButton.pressed = b.ColorPressed;
            ColorButton.disabledColor = b.ColorDisabled;
        }

        if (ColorButton2 != null)
        {
            this.LogDebug("Set button color... " + b.ColorNormal2.ToString());
            ColorButton2.defaultColor = b.ColorNormal2;
            ColorButton2.hover = b.ColorHover2;
            ColorButton2.pressed = b.ColorPressed2;
            ColorButton2.disabledColor = b.ColorDisabled2;
        }

        if(Label != null)
        {
            this.LogDebug("Set label... " + b.LabelTextId);

            if (string.IsNullOrEmpty(b.LabelTextId))
                Label.ClearText();
            else
                Label.SetText(b.LabelTextId);

            var lbl = Label.GetComponent<UILabel>();
            if (lbl != null)
                lbl.color = b.LabelColor;
        }

        if (Button.isEnabled)
        {
            UIButton.current = Button;
            EventDelegate.Execute(stateChangedListeners);
            UIButton.current = null;
        }
    }

    public void ResetState()
    {
        currentState = 0;
    }
}
