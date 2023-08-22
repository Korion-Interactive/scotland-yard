using UnityEngine;
using UnityEngine.Networking;

// The high level API classes are deprecated and will be removed in the future.
#pragma warning disable CS0618

namespace LostPolygon.AndroidBluetoothMultiplayer.Examples.UNet {
    public class CreateTapMarkerMessage : MessageBase {
        // Some arbitrary message type id number
        public const short kMessageType = 12345;

        // Position of the tap
        public Vector2 Position;
    }
}
