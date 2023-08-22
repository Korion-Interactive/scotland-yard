using UnityEngine.Networking;
#if UNITY_ANDROID

// The high level API classes are deprecated and will be removed in the future.
#pragma warning disable CS0618
public class UnetByteArrayMessage : MessageBase
{
	public const short MessageType = 12345;
	public byte[] Bytes;
	
	public override void Deserialize(NetworkReader reader)
	{
		Bytes = reader.ReadBytesAndSize();
	}

	public override void Serialize(NetworkWriter writer)
	{
		writer.WriteBytesFull(Bytes);
	}
}

#endif
