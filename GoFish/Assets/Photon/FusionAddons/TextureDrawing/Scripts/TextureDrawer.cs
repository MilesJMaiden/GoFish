using Fusion.Addons.DataSyncHelpers;
using System;
using UnityEngine;

namespace Fusion.Addons.TextureDrawing
{
    /***
     * 
     * Bufferize points to be added to a TextureDrawing.
     * 
     * The `AddDrawingPoint()` method can be called, for instance by the `TexturePen`, to add `DrawingPoint` on a `TextureSurface` through a `TextureDrawing`.
     * It is used to record the drawing points that must be edited on the texture when a contact is detected between the pen and the drawing surface. 
     * When receiving the underlying ring buffer new entries, the new points are shared to the `TextureDrawing`, that will finally apply the changes to the `TextureSurface`
     * 
     ***/
    public class TextureDrawer : RingBufferSyncBehaviour<DrawingPoint>
    {
        public void AddStopDrawingPoint(TextureDrawing targetDrawing)
        {
            AddDrawingPoint(Vector2.zero, DrawingPoint.END_DRAW_PRESSURE, Color.clear, targetDrawing);
        }

        public void AddDrawingPointLocaly(Vector2 textureCoord, byte pressure, Color color, TextureDrawing targetDrawing)
        {
            targetDrawing.AddDrawingPoint(textureCoord, pressure, color, this);
        }

        public void AddDrawingPoint(Vector2 textureCoord, byte pressure, Color color, TextureDrawing targetDrawing, bool drawPointLocaly = true)
        {
            if(Object.HasStateAuthority)
            {
                var entry = new DrawingPoint { 
                    position = textureCoord, 
                    pressureByte = pressure, 
                    color = color, 
                    referenceId = targetDrawing.Id };
                AddEntry(entry);
                // AddEntry does not trigger OnNewEntries for the local user: we have to deal with the new entry directly
                if (drawPointLocaly)
                {
                    AddDrawingPointLocaly(entry.position, entry.pressureByte, entry.color, targetDrawing);
                }
            }
            else
            {
                throw new Exception("Should only be called on the state auth of the Drawer");
            }
        }

        public override void OnNewEntries(byte[] newPaddingStartBytes, DrawingPoint[] newEntries)
        {
            foreach (var entry in newEntries)
            {
                if (Runner.TryFindBehaviour(entry.referenceId, out var drawing))
                {
                    if (drawing is TextureDrawing textureDrawing)
                    {
                        textureDrawing.AddDrawingPoint(entry.position, entry.pressureByte, entry.color, this);
                    }
                }
            }
        }

        public override void OnDataloss(RingBuffer.LossRange lossRange)
        {
            base.OnDataloss(lossRange);
            if(lossRange.start != 0)
            {
                // If the loss is at 0, it is normal for late joiners, and is handled in the TextureDrawing code
                Debug.LogWarning($"Data loss {lossRange.start} - {lossRange.end}: either increase RingBufferSyncBehaviour.BUFFER_SIZE, or decrease the maximum number of point insertioned per second of the object adding points through AddDrawingPoint (TexturePen, ...)");
            }
        }

        #region Debug
        [Header("Debug")]
        [SerializeField] TextureDrawing drawing;
        [SerializeField] int debugPosX = 0;
        [SerializeField] int debugPosY = 0;
        [SerializeField] int demoStepX = 1;
        [SerializeField] int demoStepY = 1;
        [SerializeField] byte debugPressure = 1;
        [SerializeField] Color debugColor = Color.black;
        [EditorButton("Add point")]
        void AddPoint()
        {
            if (drawing == null) drawing = FindObjectOfType<TextureDrawing>();
            AddDrawingPoint(new Vector2(debugPosX, debugPosY), debugPressure, debugColor, drawing);

            debugPosX += demoStepX;
            debugPosY += demoStepY;
        }

        #endregion
    }
}
