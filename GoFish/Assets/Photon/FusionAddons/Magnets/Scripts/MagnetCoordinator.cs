using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.XR.Shared.Grabbing;

namespace Fusion.XRShared.GrabbableMagnet
{
    public enum MagnetDirection { X, Y, Z }

    public interface IMagnet
    {
        public Vector3 SnapTargetPosition(Vector3 position);
        public bool AlignOnAllAxis { get; set; }
        public MagnetDirection AlignAxis { get; }

#pragma warning disable IDE1006 // Naming Styles
        public Transform transform { get; }
#pragma warning restore IDE1006 // Naming Styles
    }

    public interface IMovableMagnet : IMagnet {
        public bool CheckOnUngrab { get; set; }
        public float MagnetRadius { get; set; }
        public bool TryFindClosestMagnetInRange(out IMagnet closestMagnet, out float distance);
        public void SnapToMagnet(IMagnet magnet);
        public MagnetCoordinator MagnetCoordinator { get; }
    }

    [DefaultExecutionOrder(MagnetPoint.EXECUTION_ORDER)]
    public class MagnetCoordinator : NetworkBehaviour
    {
        NetworkGrabbable networkGrabbable;
        public bool overrideMagnetRadius = true;
        public float magnetRadius = 0.1f;

        List<IMovableMagnet> magnets = new List<IMovableMagnet>();
        private void Awake()
        {
            networkGrabbable = GetComponentInParent<NetworkGrabbable>();
            networkGrabbable.onDidUngrab.AddListener(OnDidUngrab);
            magnets = new List<IMovableMagnet>(GetComponentsInChildren<IMovableMagnet>());
            foreach (var magnet in magnets)
            {
                magnet.CheckOnUngrab = false;
            }
        }

        private void OnDidUngrab()
        {
            if (overrideMagnetRadius)
            {
                foreach (var magnet in magnets)
                {
                    magnet.MagnetRadius = magnetRadius;
                }
            }
            CheckMagnetProximity();
        }

        [ContextMenu("CheckMagnetProximity")]
        public void CheckMagnetProximity()
        {
            if (Object && Object.HasStateAuthority && networkGrabbable.IsGrabbed == false)
            {
                float minDistance = float.PositiveInfinity;
                IMovableMagnet closestLocalMagnet = null;
                IMagnet closestRemoteMagnet = null;
                foreach (var magnet in magnets)
                {
                    if (magnet.TryFindClosestMagnetInRange(out var remoteMagnet, out var distance))
                    {
                        if (distance < minDistance)
                        {
                            closestLocalMagnet = magnet;
                            closestRemoteMagnet = remoteMagnet;
                            minDistance = distance;
                        }
                    }
                }
                if (closestLocalMagnet != null)
                {
                    closestLocalMagnet.SnapToMagnet(closestRemoteMagnet);
                }
            }
        }
    }

}
