using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    public class ReliableMessage : IMessage
    {
        private static uint msgCounter = 0;
        public static void Cleanup()
        {
            msgCounter = 0;
        }


        public byte MessageTypeId { get { return (byte)InternalMessageID.ReliableMessage; } }

        public string ReliableID { get { return SenderID + reliableCounter; } }

        public string SenderID;
        uint reliableCounter;
        public byte[] InnerMessage;

        internal ReliableMessage() { }
        public ReliableMessage(string participantID, byte[] message)
        {
            this.SenderID = participantID;
            this.reliableCounter = msgCounter++;
            this.InnerMessage = message;
        }

        public void Serialize(System.IO.BinaryWriter writer)
        {
            writer.Write(SenderID);
            writer.Write(reliableCounter);

            writer.Write(InnerMessage.Length);
            writer.Write(InnerMessage);
        }

        public void Deserialize(System.IO.BinaryReader reader)
        {
            SenderID = reader.ReadString();
            reliableCounter = reader.ReadUInt32();

            int length = reader.ReadInt32();
            InnerMessage = reader.ReadBytes(length);
        }

        public IMessage CreateEmptyInstance()
        {
            return new ReliableMessage();
        }
    }