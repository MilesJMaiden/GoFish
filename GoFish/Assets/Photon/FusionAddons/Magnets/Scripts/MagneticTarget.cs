using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XRShared.GrabbableMagnet
{
    public class MagneticTarget : MonoBehaviour, IMagnet
    {
        public string magnetLayer = "Magnets";
        [Header("Snap options")]
        public bool isPlaneMagnet = false;
        [DrawIf(nameof(isPlaneMagnet), true, mode: DrawIfMode.Hide)]
        public MagnetDirection planeAxis = MagnetDirection.Y;
        public Vector3 localOffset = Vector3.zero;

        [Tooltip("If false, the magnets attracted will rotate to match the alignAxis. Otherwise, they will rotate to match the align axis and will rotate a bit more to only have 90° angles between other axis")]
        public bool alignOnAllAxis = true;
        public MagnetDirection alignAxis = MagnetDirection.Y;

        private void Awake()
        {
            if (string.IsNullOrEmpty(magnetLayer) == false)
            {
                int layer = LayerMask.NameToLayer(magnetLayer);
                if (layer == -1)
                {
                    Debug.LogError($"Please add a {magnetLayer} layer (it will be automatically be set to this object)");
                }
                else
                {
                    gameObject.layer = layer;
                    foreach (var collider in GetComponentsInChildren<Collider>())
                    {
                        collider.gameObject.layer = layer;
                    }
                }
            }               
        }

        #region IMagnet
        public bool AlignOnAllAxis
        {
            get => alignOnAllAxis;
            set => alignOnAllAxis = value;
        }

        public MagnetDirection AlignAxis => alignAxis;

        public Vector3 SnapTargetPosition(Vector3 position)
        {
            if (isPlaneMagnet)
            {
                var planeDirection = Vector3.zero;
                if (planeAxis == MagnetDirection.Y) planeDirection = transform.up;
                if (planeAxis == MagnetDirection.X) planeDirection = transform.right;
                if (planeAxis == MagnetDirection.Z) planeDirection = transform.forward;
                var projectionPlane = new Plane(planeDirection, transform.position);
                // Project position on plane
                return projectionPlane.ClosestPointOnPlane(position) + (transform.TransformPoint(localOffset) - transform.position);
            }
            else
            {
                return transform.position + (transform.TransformPoint(localOffset) - transform.position);
            }
        }
        #endregion
    }
}
