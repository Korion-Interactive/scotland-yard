using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class NoVoiceChat : VoiceChatBaseProvider
{
    public override bool IsAvailable { get { return false; } }

    public override void StartChat()
    {
        this.LogError("StartChat(): voice chat not available");
    }

    public override void StopChat()
    {
        this.LogError("StopChat(): voice chat not available");
    }

    public override void SetVolume(float volume)
    {
        this.LogError("SetVolume(): voice chat not available");
    }
}