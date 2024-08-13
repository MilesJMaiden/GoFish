using System.Collections;
using System.Collections.Generic;
using Fusion.Addons.XRHandsSync;
using Fusion.XR.Shared.Locomotion;
using UnityEngine;

namespace Fusion.Addons.VisionOsHelpers
{

    /**
    * 
    * VisionOSHandsConfiguration allows a quick automatic configuration of the hands for the Polyspatial vision OS platform.
    * 
    * The script : 
    *   - ensures that even if the hands are not detected for a short duration, the grabbing triggered by finger detection in FingerDrivenControllerInput still continues
    *   - uses LineMesh to display the beam used by a RayBeamer component
    *   - applies a specific layer to all collider in hands, that should be removed from Polyspatial handled colliders, to be sure that the grabbing/touching collider are not spatial touched by visionOS (which is probably not desired)
    *   - ensures that in case of no detection of the hands, the hand representation components do not try to fallback to controller, as there are no hand controller on visionOS
    * 
    **/
    public class VisionOSHandsConfiguration : MonoBehaviour
    {
        public string polyspatialIgnoredLayer = "PolySpatialIgnored";
        public bool ignoreHandColliderInPolyspatial = true;
        public Material rayBeamerMaterial = null;
        private void Awake()
        {
#if UNITY_VISIONOS
            VisionOsConfiguration();
#endif
        }

        void VisionOsConfiguration()
        {
            // Finger grabbing
            var fingerDriverControllerInput = GetComponentInChildren<FingerDrivenControllerInput>();
            if (fingerDriverControllerInput)
            {
                // On vision OS, there is no need to fallback to controller actions (we don't have any controllers)
                // Besides, during fast movement, hand tracking can be lost, and object would be ungrabbed upon loosing control
                fingerDriverControllerInput.alwaysKeepHandCommandControl = true;
            }

            // Hand colliders
            if (ignoreHandColliderInPolyspatial)
            {
                int layer = LayerMask.NameToLayer(polyspatialIgnoredLayer);
                if (layer == -1)
                {
                    Debug.LogError($"The layer '{polyspatialIgnoredLayer}' does not exists. Create it add remove it from the 'Collider object layer mask' in 'Project settings>Polyspatial'");
                }
                else
                {
                    foreach(var collider in GetComponentsInChildren<Collider>())
                    {
                        collider.gameObject.layer = layer;
                    }
                }
            }

            // Ray beamer
            // Line renderers are not yet available on polyspatial: placing a LineRendererToLineMesh to replace them
            var beamer = GetComponentInChildren<RayBeamer>();
            if (beamer)
            {
                var lineRendererObject = beamer.gameObject;
                if (beamer.lineRenderer) lineRendererObject = beamer.lineRenderer.gameObject;
                var meshRenderer = lineRendererObject.AddComponent<MeshRenderer>();
                if (rayBeamerMaterial) {
                    meshRenderer.material = rayBeamerMaterial;
                }
                else
                {
                    meshRenderer.material = Resources.Load<Material>("LineSGMaterial");
                }
                lineRendererObject.AddComponent<MeshFilter>();
                var lineRendererToLineMesh = lineRendererObject.AddComponent<LineRendererToLineMesh>();
                lineRendererToLineMesh.checkPositionsEveryFrame = true;
                lineRendererToLineMesh.replicateLineRendererEnabledStatus = true;
            }

            // Hand representation
            // vision OS do not have controllers. So if the hand stop being detected, we should not switch back to controller mode by default
            XRHandCollectableSkeletonDriver xrHandCollectableSkeletonDriver = GetComponentInChildren<XRHandCollectableSkeletonDriver>();
            if (xrHandCollectableSkeletonDriver)
            {
                xrHandCollectableSkeletonDriver.controllerTrackingMode = XRHandCollectableSkeletonDriver.ControllerTrackingMode.NeverAvailable;
            }
        }

    }

}
