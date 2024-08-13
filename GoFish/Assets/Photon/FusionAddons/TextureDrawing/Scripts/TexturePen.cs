using Fusion.Addons.BlockingContact;
using Fusion.XR.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.Addons.TextureDrawing
{
    /***
     * 
     * The `TexturePen` is located on the pen (with a `BlockableTip` component) and try to detect a contact with a `TextureDrawing` (with a `BlockingSurface` component).
     * When a contact is detected, the local list of points to draw is updated. Then for each point, the method `AddDrawingPoint()` of `TextureDrawer` is called during the `FixedUpdateNetwork()`.
     * 
     ***/

    public class TexturePen : NetworkBehaviour
    {
        List<PendingDrawingPoint> pointsToAdd = new List<PendingDrawingPoint>();

        BlockableTip blockableTip;
        TextureDrawer textureDrawer;

        BlockingSurface lastBlockingsurface;
        TextureDrawing lastTextureDrawing;
        bool isDrawing = false;

        public Color color = Color.black;
        [Tooltip("If greater than 0, it will throttle the amount of point added per second")]
        public int maxPointInsertionPerSeconds = 0;
        int maxPointInsertionPerTick = int.MaxValue;
        float delayBeforeNewTransmission;
        float lastTransmission;

        IColorProvider colorProvider;
        public struct PendingDrawingPoint 
        {
            public Vector2 position;
            public Color color;
            public byte pressureByte;
            public TextureDrawing drawing;
            public bool alreadyDrawn;
        }

        private void Awake()
        {
            blockableTip = GetComponent<BlockableTip>();
            textureDrawer = GetComponent<TextureDrawer>();
            colorProvider = GetComponent<IColorProvider>();
        }

        public override void Spawned()
        {
            base.Spawned();
            if(maxPointInsertionPerSeconds > 0)
            {
                maxPointInsertionPerTick = Mathf.Max(1, (int)(maxPointInsertionPerSeconds * Runner.DeltaTime));
                delayBeforeNewTransmission = 1f / maxPointInsertionPerSeconds;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(Object && colorProvider != null)
                color = colorProvider.CurrentColor;

            if (Object == null ||  Object.HasStateAuthority == false) return;

            if (blockableTip.IsGrabbed && blockableTip.IsInContact && blockableTip.lastSurfaceInContact != null)
            {
                if(blockableTip.lastSurfaceInContact != lastBlockingsurface)
                {
                    lastBlockingsurface = blockableTip.lastSurfaceInContact;
                    lastTextureDrawing = blockableTip.lastSurfaceInContact.GetComponentInParent<TextureDrawing>();
                }

                if (lastTextureDrawing)
                {
                    isDrawing = true;

                    float blockableTipPressure = 0;
                    if (blockableTip.lastSurfaceInContact.maxDepth == 0)
                    {
                        blockableTipPressure = 1;
                    }
                    else
                    {
                        var depth = blockableTip.lastSurfaceInContact.referential.InverseTransformPoint(blockableTip.tip.position).z;
                        blockableTipPressure = Mathf.Clamp01(1f - ((blockableTip.lastSurfaceInContact.maxDepth - depth) / blockableTip.lastSurfaceInContact.maxDepth));
                    }

                    byte pressure = (byte)(1 + (byte)(254 * blockableTipPressure));
                    var coordinate = blockableTip.SurfaceContactCoordinates;
                    var surface = lastTextureDrawing.textureSurface;
                    Vector2 textureCoord = new Vector2(surface.TextureWidth * (coordinate.x + 0.5f), surface.TextureHeight * (0.5f - coordinate.y));
                    // Draw the local point
                    lastTextureDrawing.AddDrawingPoint(textureCoord, pressure, color, textureDrawer);
                    // Plan to store in the network data the drawing
                    pointsToAdd.Add(new PendingDrawingPoint { position = textureCoord, pressureByte = pressure, color = color, drawing = lastTextureDrawing, alreadyDrawn = true });
                }
            } 
            else if (isDrawing && lastTextureDrawing)
            {
                // Add stop point
                isDrawing = false;
                // Draw the local point
                lastTextureDrawing.AddDrawingPoint(Vector2.zero, DrawingPoint.END_DRAW_PRESSURE, color, textureDrawer);
                // Plan to store in the network data the drawing
                pointsToAdd.Add(new PendingDrawingPoint { position = Vector2.zero, pressureByte = DrawingPoint.END_DRAW_PRESSURE, color = color, drawing = lastTextureDrawing });
            }
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (pointsToAdd.Count == 0) return;
            if (maxPointInsertionPerTick == 1 && (Time.time - lastTransmission) < delayBeforeNewTransmission)
            {
                return;
            }
            int addedPoints = 0;
            while (pointsToAdd.Count > 0 && (maxPointInsertionPerTick == 0 || addedPoints < maxPointInsertionPerTick))
            {
                var point = pointsToAdd[0];
                textureDrawer.AddDrawingPoint(point.position, point.pressureByte, point.color, point.drawing, drawPointLocaly: point.alreadyDrawn == false);
                pointsToAdd.RemoveAt(0);
                addedPoints++;
                lastTransmission = Time.time;
            }
        }
    }

}

