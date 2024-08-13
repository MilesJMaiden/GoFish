using Fusion.Addons.DataSyncHelpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Addons.TextureDrawing
{

    /***
     * This component contains all the `DrawingPoint` of the texture that have been added, in the referenced `TextureSurface`.
     * When new entries are added by the `TextureDrawer`, on all clients, the `Draw()` method updates the `TextureSurface` to add a point or draw a line. 
     *
     * Thanks to this StreamSynchedBehaviour mechanism, tracking the expected TotalDataLength, late joiners will receive all the points that need to be drawn on the `TextureSurface` when they join the room.
     * They are then merged with points already received (late joining data usually take a bit more time to arrive)
     * 
     * The class stores the latest point drawn for each `TextureDrawer`, to create lines between the previous point added and the latest one.
     * 
     * It calls the `OnRedrawRequired` method when the `onRedrawRequired` event of the `TextureSurface` occurs (when merge late joining data, or when an external component triggers it)
     ***/
    public class TextureDrawing : StreamSynchedBehaviour
    {
        Dictionary<NetworkBehaviourId, DrawingPoint> lastDrawingPointByDrawer = new Dictionary<NetworkBehaviourId, DrawingPoint>();

        public TextureSurface textureSurface;
        private void Awake()
        {
            if (textureSurface == null) textureSurface = GetComponent<TextureSurface>();
            textureSurface.onRedrawRequired.AddListener(OnRedrawRequired);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            textureSurface.onRedrawRequired.RemoveListener(OnRedrawRequired);
        }

        void OnRedrawRequired()
        {
            // An external component manually edited the texture: we redraw the full drawing
            RedrawFullDrawing();
        }

        public void AddDrawingPoint(Vector2 position, byte pressureByte, Color color, TextureDrawer textureDrawer)
        {
            Draw(position, pressureByte, color, textureDrawer.Id);
            var entry = new DrawingPoint
            {
                position = position,
                pressureByte = pressureByte,
                color = color,
                referenceId = textureDrawer.Id
            };
            AddLocalData(entry.AsByteArray);
        }

        void RedrawFullDrawing() {
            DrawingPoint[] entriesArray = SplitCompleteData();

            // We restart the drawing: there should be no "previous" drawing points
            lastDrawingPointByDrawer.Clear();

            var points = new List<DrawingPoint>(entriesArray);
            foreach (var entry in points)
            {

                Draw(entry.position, entry.pressureByte, entry.color, entry.referenceId);
            }
        }

        #region Drawing
        public void Draw(Vector2 textureCoord, byte pressure, Color color, NetworkBehaviourId textureDrawerId)
        {
            // Determine if it is the first point of a line (either the first point of for this drawer, of if the last point had a end of line DrawingPoint.END_DRAW_PRESSURE pressure),
            //  or if we should draw a line from the last point to the new one
            DrawingPoint lastPoint = new DrawingPoint();
            bool hasLastPoint = false;
            if (lastDrawingPointByDrawer.ContainsKey(textureDrawerId))
            {
                lastPoint = lastDrawingPointByDrawer[textureDrawerId];
                hasLastPoint = lastPoint.pressureByte != DrawingPoint.END_DRAW_PRESSURE;
            }

            if (pressure == DrawingPoint.END_DRAW_PRESSURE)
            {
                // We do not draw an endline
            }
            else if (hasLastPoint == false)
            {
                textureSurface.AddPoint(textureCoord, pressure, color);
            }
            else
            {
                textureSurface.AddLine(lastPoint.position, lastPoint.pressureByte, lastPoint.color, textureCoord, pressure, color);
            }

            var entry = new DrawingPoint
            {
                position = textureCoord,
                pressureByte = pressure,
                color = color,
                referenceId = textureDrawerId
            };
            lastDrawingPointByDrawer[textureDrawerId] = entry;
        }
        #endregion

        #region StreamSynchedBehaviour    
        public override void Send(byte[] data)
        {
            // We replace here the logic of StreamSynchBehaviour: it is not used to send directly data, then recovered by late joiner, but to store local data (coming from TextureDrawer), and then sharing them with late joiner
        }

        // Called upon reception of the cache of existing point, through the streaming API, for late joiners
        // We override the OnDataChunkReceived (that will be used here only as a late joiner)
        //  to avoid storing the data received locally at the start as it does usually (since we want the full cache to be at the start of the storage in our case)
        public override void OnDataChunkReceived(byte[] newData, PlayerRef source, float time)
        {
            if (newData.Length == 0) return;

            ByteArrayTools.Split<DrawingPoint>(newData, out var newPaddingStartBytes, out var points);
            
            List<NetworkBehaviourId> drawingIds = new List<NetworkBehaviourId>();
            foreach(var point in points)
            {
                if(drawingIds.Contains(point.referenceId) == false && point.pressureByte != DrawingPoint.END_DRAW_PRESSURE)
                {
                    drawingIds.Add(point.referenceId);
                }
                else if (drawingIds.Contains(point.referenceId) && point.pressureByte == DrawingPoint.END_DRAW_PRESSURE)
                {
                    drawingIds.Remove(point.referenceId);
                }
            }

            foreach(var drawingId in drawingIds)
            {
                // We inject endlines points, between the existing points and the full cache received from already connected players, as some of the already received data might be also in the full data cache - a better implementation would be to drop the data in double
                var entry = new DrawingPoint
                {
                    pressureByte = DrawingPoint.END_DRAW_PRESSURE,
                    referenceId = drawingId
                };
                InsertLocalDataAtStart(entry.AsByteArray, source, time);
            }

            InsertLocalDataAtStart(newData, source, time);
            RedrawFullDrawing();
        }
        #endregion

        private DrawingPoint[] SplitCompleteData()
        {
            var data = new byte[totalLocalDataLength];
            int cursor = 0;
            foreach (var chunk in cachedDataChunks)
            {
                System.Buffer.BlockCopy(chunk.data, 0, data, cursor, chunk.data.Length);
                cursor += chunk.data.Length;
            }

            ByteArrayTools.Split<DrawingPoint>(data, out var newPaddingStartBytes, out var points);

            return points;
        }
    }
}
