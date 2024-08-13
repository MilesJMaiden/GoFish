using Fusion.Addons.DataSyncHelpers;
using UnityEngine;

namespace Fusion.Addons.TextureDrawing
{
    [System.Serializable]
    public struct DrawingPoint : RingBuffer.IRingBufferEntry
    {
        public const byte END_DRAW_PRESSURE = 0;

        public Vector2 position;
        public Color color;
        public byte pressureByte;
        // Can be used either to store the target TextureDrawing, or the source TextureDrawer
        public NetworkBehaviourId referenceId;

        #region RingBuffer.IRingBufferEntry
        public byte[] AsByteArray
        {
            get
            {
                Vector3 colorData = new Vector3(color.r, color.g, color.b);
                return SerializationTools.AsByteArray(position, colorData, pressureByte, referenceId);
            }
        }

        public void FillFromBytes(byte[] entryBytes)
        {
            int unserializePosition = 0;
            // Position
            SerializationTools.Unserialize(entryBytes, ref unserializePosition, out position);
            // Color
            SerializationTools.Unserialize(entryBytes, ref unserializePosition, out Vector3 colorData);
            color = new Color(colorData.x, colorData.y, colorData.z, 1);
            // Pressure
            SerializationTools.Unserialize(entryBytes, ref unserializePosition, out pressureByte);
            // drawId
            SerializationTools.Unserialize(entryBytes, ref unserializePosition, out referenceId);
        }
        #endregion
    }
}
