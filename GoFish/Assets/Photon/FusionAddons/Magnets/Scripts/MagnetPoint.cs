using UnityEngine;
using Fusion.XR.Shared.Grabbing;
using UnityEngine.Events;

namespace Fusion.XRShared.GrabbableMagnet
{
    [DefaultExecutionOrder(MagnetPoint.EXECUTION_ORDER)]
    public class MagnetPoint : NetworkBehaviour, IMovableMagnet
    {
        public const int EXECUTION_ORDER = NetworkGrabbable.EXECUTION_ORDER + 5;
        [HideInInspector]
        public NetworkTRSP rootNTRSP;
        NetworkGrabbable networkGrabbable;
        Rigidbody rb;
        public float magnetRadius = 0.1f;
        [Tooltip("If set, this object and its children collider will be set to this layer")]
        public string magnetLayer = "Magnets";

        [Header("Snap options")]
        public bool isPlaneMagnet = false;
        [DrawIf(nameof(isPlaneMagnet), true, mode: DrawIfMode.Hide)]
        public MagnetDirection planeAxis = MagnetDirection.Y;
        [Tooltip("If false, the magnets attracted will rotate to match the alignAxis. Otherwise, they will rotate to match the align axis and will rotate a bit more to only have 90° angles between other axis")]
        public bool alignOnAllAxis = true;
        public MagnetDirection alignAxis = MagnetDirection.Y;
        [Header("Target layers")]
        public LayerMask compatibleLayers;
        public string additionalCompatibleLayer = "";
        public bool addObjectLayerToCompatibleLayers = true;

        [Header("Snap animation")]
        public bool instantSnap = true;
        public float snapDuration = 1;

        public bool AlignOnAllAxis {
            get => alignOnAllAxis;
            set => alignOnAllAxis = value;
        }
        public MagnetDirection AlignAxis => alignAxis;

        public float MagnetRadius
        {
            get => magnetRadius;
            set => magnetRadius = value;
        }

        public bool CheckOnUngrab { get; set; } = true;

        MagnetCoordinator _magnetCoordinator;
        public MagnetCoordinator MagnetCoordinator => _magnetCoordinator;


        public UnityEvent onSnapToMagnet;

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (snapRequest != null && networkGrabbable && networkGrabbable.IsGrabbed)
            {
                // Cancel snap
                snapRequest = null;
            }
            if (snapRequest != null)
            {
                DoSnapToMagnet(snapRequest);
            }
        }

        private void Awake()
        {
            _magnetCoordinator = GetComponentInParent<MagnetCoordinator>();

            rootNTRSP = GetComponentInParent<NetworkTRSP>();
            networkGrabbable = GetComponentInParent<NetworkGrabbable>();
            rb = GetComponentInParent<Rigidbody>();
            if(networkGrabbable) networkGrabbable.onDidUngrab.AddListener(OnDidUngrab);
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
            if (string.IsNullOrEmpty(additionalCompatibleLayer) == false)
            {
                int layer = LayerMask.NameToLayer(additionalCompatibleLayer);
                if (layer == -1)
                {
                    Debug.LogError($"Please add a {magnetLayer} layer (it will be automatically be set to this object magnet mask)");
                }
                else
                {
                    compatibleLayers |= (1 << layer);
                }
            }
        }

        private void OnDidUngrab()
        {
            if (CheckOnUngrab)
            {
                CheckMagnetProximity();
            }
        }

        public bool TryFindClosestMagnetInRange(out IMagnet closestMagnet, out float minDistance)
        {
            var layerMask = compatibleLayers;
            if (addObjectLayerToCompatibleLayers) {
                layerMask  = layerMask  | (1 << gameObject.layer);
            }
            var colliders = Physics.OverlapSphere(transform.position, magnetRadius, layerMask: layerMask);
            closestMagnet = null;
            minDistance = float.PositiveInfinity;
            for (int i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                IMagnet magnet = collider.GetComponentInParent<IMagnet>();
                if (magnet == null)
                {
                    Debug.LogError($"No magnet ({collider})");
                    continue;
                }
                if((Object)magnet == this)
                {
                    continue;
                }
                if(magnet is IMovableMagnet movableMagnet)
                {
                    if (MagnetCoordinator != null && movableMagnet.MagnetCoordinator == MagnetCoordinator)
                    {
                        continue;
                    }
                }

                var distance = Vector3.Distance(transform.position, magnet.SnapTargetPosition(transform.position));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestMagnet = magnet;
                }
            }
            return closestMagnet != null;
        }

        [ContextMenu("CheckMagnetProximity")]
        public void CheckMagnetProximity()
        {
            if (Object && Object.HasStateAuthority && (networkGrabbable == null || networkGrabbable.IsGrabbed == false))
            {
                if (TryFindClosestMagnetInRange(out var closestMagnet, out _))
                {
                    SnapToMagnet(closestMagnet);
                }
            }
        }

        IMagnet snapRequest = null;
        float snapStart = -1;

        public void SnapToMagnet(IMagnet magnet)
        {
            snapRequest = magnet;
            snapStart = Time.time;
        }

        public void DoSnapToMagnet(IMagnet magnet)
        {
            float progress = 1;
            if (instantSnap)
            {
                snapRequest = null;
            }
            else
            {
                progress = (Time.time - snapStart) / snapDuration;
                if(progress >= 1)
                {
                    progress = 1;
                    snapRequest = null;
                }
            }
            if (rb)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // Rotate the parent NT to match the magnet positions
            Quaternion targetRotation;
            if (magnet.AlignOnAllAxis == false)
            {
                targetRotation = AdaptedRotationOnAlignAxis(magnet.transform, magnet.AlignAxis);
            }
            else
            {
                targetRotation = AdaptedRotationOnAllAxis(magnet.transform, magnet.AlignAxis);
            }
            ApplyRotation(targetRotation, progress);

            // Move the parent NT to match the magnet positions
            var targetPosition = magnet.SnapTargetPosition(transform.position);
            ApplyPosition(targetPosition, progress);

            // Send event
            if (onSnapToMagnet != null)
            {
                onSnapToMagnet.Invoke();
            }
        }

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
                return projectionPlane.ClosestPointOnPlane(position);
            }
            else
            {
                return transform.position;
            }
        }

        protected virtual Quaternion AdaptedRotationOnAlignAxis(Transform targetTransform, MagnetDirection targetAlignAxis)
        {
            Vector3 forward = Vector3.zero;
            Vector3 up = Vector3.zero;
            if (targetAlignAxis == MagnetDirection.Y)
            {
                forward = Vector3.ProjectOnPlane(transform.forward, targetTransform.up);
                up = -targetTransform.up;
            }
            if (targetAlignAxis == MagnetDirection.X)
            {
                // Handling X axis is more complex (and use cases are rare). Let's simply rotate to align the axis directly
                return transform.rotation * Quaternion.FromToRotation(transform.right, targetTransform.right);
            }
            if (targetAlignAxis == MagnetDirection.Z)
            {
                forward = targetTransform.forward;
                up = Vector3.ProjectOnPlane(transform.up, targetTransform.forward);
            }
            var targetRotation = Quaternion.LookRotation(forward, up);
            return targetRotation;
        }


        // Find the most appropriate axis to adapt on the align axis while aligning other axis too
        protected virtual Quaternion AdaptedRotationOnAllAxis(Transform targetTransform, MagnetDirection targetAlignAxis)
        {
            if (targetAlignAxis == MagnetDirection.Y)
            {
                var upTarget = -targetTransform.up;
                var forwardCandidates = new Vector3[] { targetTransform.right, -targetTransform.right, targetTransform.forward, -targetTransform.forward };
                var forwardTarget = targetTransform.forward;
                var minAngle = float.PositiveInfinity;
                for (int i = 0; i < forwardCandidates.Length; i++)
                {
                    var forwardCandidate = forwardCandidates[i];
                    var angle = Vector3.Angle(transform.forward, forwardCandidate);
                    if (angle < minAngle)
                    {
                        minAngle = angle;
                        forwardTarget = forwardCandidate;
                    }
                }

                var targetRotation = Quaternion.LookRotation(forwardTarget, upTarget);
                return targetRotation;
            }
            else if (targetAlignAxis == MagnetDirection.Z)
            {
                var upTarget = targetTransform.up;
                var upCandidates = new Vector3[] { targetTransform.up, -targetTransform.up, targetTransform.right, -targetTransform.right };
                var forwardTarget = targetTransform.forward;
                var minAngle = float.PositiveInfinity;
                for (int i = 0; i < upCandidates.Length; i++)
                {
                    var upCandidate = upCandidates[i];
                    var angle = Vector3.Angle(transform.up, upCandidate);
                    if (angle < minAngle)
                    {
                        minAngle = angle;
                        upTarget = upCandidate;
                    }
                }

                var targetRotation = Quaternion.LookRotation(forwardTarget, upTarget);
                return targetRotation;
            }
            // Handling X axis is more complex (and use cases are rare). Let's simply apply the same rotation
            return targetTransform.rotation;
        }

        void ApplyRotation(Quaternion targetRotation, float progress)
        {
            var localMagnetRotation = Quaternion.Inverse(rootNTRSP.transform.rotation) * transform.rotation;
            var rotation = targetRotation * Quaternion.Inverse(localMagnetRotation);

            if (progress < 1) rotation = Quaternion.Slerp(rootNTRSP.transform.rotation, rotation, progress);

            if (rb)
            {
                rb.rotation = rotation;
                rootNTRSP.transform.rotation = rotation;
            }
            else
            {
                rootNTRSP.transform.rotation = rotation;
            }
        }

        void ApplyPosition(Vector3 targetPosition, float progress)
        {
            var position = targetPosition - transform.position + rootNTRSP.transform.position;
            if (progress < 1) position = Vector3.Lerp(rootNTRSP.transform.position, position, progress);
            if (rb)
            {
                rb.position = position;
                rootNTRSP.transform.position = position;
            }
            else
            {
                rootNTRSP.transform.position = position;
            }
        }
    }
}


