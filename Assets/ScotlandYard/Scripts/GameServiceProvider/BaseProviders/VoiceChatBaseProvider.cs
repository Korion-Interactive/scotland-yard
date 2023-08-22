using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class VoiceChatBaseProvider
{
    public abstract bool IsAvailable { get; }

    public abstract void StartChat();
    public abstract void StopChat();

    public abstract void SetVolume(float volume);

}
