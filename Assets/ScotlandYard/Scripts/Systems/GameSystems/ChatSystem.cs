using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ChatSystem : NetworkSystem<GameGuiEvents, ChatSystem>
{
    public GameObject ChatWindowContainer;
    public GameObject ChatWindowButton;
    public UITextList ChatText;
    public UISprite NewMessageIndication;

    TouchScreenKeyboard currentKeyboard;
    //StringBuilder chatLog = new StringBuilder();

    protected override byte Context { get { return Globals.Net_Context_Chat; } }

    protected override void Awake()
    {
        if(!GSP.IsMultiplayerRTAvailable)
        {
            ChatWindowContainer.SetActive(false);
            ChatWindowButton.SetActive(false);
        }

        base.Awake();
    }
    protected override void RegisterEvents()
    {
        ListenTo<ChatArgs>(GameGuiEvents.ChatMessage, (args) =>
        {
            if (args != null && GSP.IsMultiplayerRTAvailable && args.ParticipantId == GSP.MultiplayerRT.OwnParticipantId)
            {
                SendEvent(GameGuiEvents.ChatMessage, args);
            }
        });

        //StartCoroutine(CoTest());
    }

    //IEnumerator CoTest()
    //{
    //    for (int i = 0; i < 500; i++)
    //    {
    //        yield return new WaitForSeconds(0.3f);
    //        Append("line " + i);
    //    }
    //}

    public void StartEnterText()
    {
        StartCoroutine(CoEnterText());
    }

    IEnumerator CoEnterText()
    {
        TouchScreenKeyboard keyboard = TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.Default, true, false, false, false, "Chat");

        yield return new WaitUntil(() => keyboard.status == TouchScreenKeyboard.Status.Done ||
                                         keyboard.status == TouchScreenKeyboard.Status.Canceled);

        if(keyboard.status != TouchScreenKeyboard.Status.Canceled)
        {
            if (!string.IsNullOrEmpty(keyboard.text))
                SendChatMessage(keyboard.text);

            keyboard.active = false;
        }
    }

    protected override BaseArgs ArgsFactory(GameGuiEvents eventType)
    {
        switch(eventType)
        {
            case GameGuiEvents.ChatMessage:
                return new ChatArgs();

            default:
                return null;
        }
    }

    protected override bool MessageReceivedSuccessfully(GameGuiEvents eventType, BaseArgs args)
    {
        switch(eventType)
        {
            case GameGuiEvents.ChatMessage:
                ChatArgs chatArgs = args as ChatArgs;
                WriteToChatLog(chatArgs);
                break;
            default:
                throw new ArgumentException();
        }

        return true;
    }

    private void WriteToChatLog(ChatArgs args)
    {
        string entry = string.Format("[{0}]{1}:[FFFFFF] {2}", args.ColorCode, args.PlayerName, args.ChatMessage);
        Append(entry);

        if(!ChatWindowContainer.activeSelf)
        {
            NewMessageIndication.gameObject.SetActive(true);
        }
    }

    public void Btn_ShowOrHideChatLog()
    {
        bool active = ChatWindowContainer.activeSelf;
        ChatWindowContainer.SetActive(!active);

        NewMessageIndication.gameObject.SetActive(false);

        //if(ChatWindowContainer.activeSelf)
        //    StartEnterText();
    }

    void SendChatMessage(string message)
    {
        ChatArgs args = new ChatArgs();
        args.ChatMessage = message;

        PlayerSetup player = GameSetupBehaviour.Instance.IterateAllPlayers(true).FirstOrDefault((o) => o.Controller == PlayerController.Human);
        if(player != null)
        {
            args.ColorCode = player.Color.GetColor().GetRgbHexString();
            args.PlayerName = player.DisplayName;
        }
        else // spectator
        {
            args.ColorCode = "FF00FF";
            args.PlayerName = (GSP.IsStatusAvailable) ? GSP.Status.PlayerName : "(Spectator)";
        }

        WriteToChatLog(args);
        Broadcast(GameGuiEvents.ChatMessage, args);

        
    }

    private void Append(string text)
    {
        //chatLog.AppendLine(text);
        ChatText.Add(text);
    }
}
