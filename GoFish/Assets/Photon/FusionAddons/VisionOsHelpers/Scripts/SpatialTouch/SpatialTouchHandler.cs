using System;
using System.Collections.Generic;
using Fusion.XR.Shared.Grabbing;
using Fusion.XR.Shared.Rig;
#if POLYSPATIAL_SDK_AVAILABLE
using Unity.PolySpatial;
using Unity.PolySpatial.InputDevices;
#endif
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Fusion.Addons.VisionOsHelpers
{
    public interface ISpatialTouchListener
    {
#if POLYSPATIAL_SDK_AVAILABLE

    void TouchStart(SpatialPointerKind interactionKind, Vector3 interactionPosition);
    void TouchEnd();
    void TouchStay(SpatialPointerKind interactionKind, Vector3 interactionPosition);
#endif
    }

    /**
    * 
    * SpatialTouchHandler class detects user's interactions (touch, pinch, indirect pinch) thanks to Unity Polyspatial.
    * Touch:
    * It raises TouchStart, TouchEnd & TouchStay events for ISpatialTouchListener.
    * Grabbing: 
    * The SpatialTouchTracker struct can keep track of up to 2 spatial touch event at the same time.
    * If present in the scene, up to 2 SpatialGrabber will be associated to the SpatialTouchTracker struct to handle spatial grabbing
    * In unbounded mode, if replaceContactGrabber is set to false, spatial touch for grabbing is not taken into account to avoid duplicate logic,
    *  as, grabbing while touching is already handled by the normal grabber logic. 
    * If replaceContactGrabber is set to true (default, as hand tracking is refreshed less often than spatial touches on visionOS), normal Grabber components on hands will be disabled, to avoid this duplicate logic
    *  
    **/

    public class SpatialTouchHandler : MonoBehaviour
    {
#if POLYSPATIAL_SDK_AVAILABLE
    [System.Serializable]
    public struct SpatialTouchTracker {
        // Touch info
        public GameObject previousObject;
        public GameObject lastSpatialTouchedObject;
        public SpatialPointerKind previousKind;
        // Grab and touch handlers
        public SpatialGrabber grabber;
        public List<ISpatialTouchListener> lastSpatialTouchedListeners;
        public GameObject debugRepresentation;
        // Used for a touch during the current update
        public bool isUsed;


        public void OnTouchUpdate(SpatialPointerState primaryTouchData, VolumeCamera.PolySpatialVolumeCameraMode currentMode, bool doNotUseContactSpatialGrabbingInUnboundedMode) {
            SpatialPointerKind interactionKind = primaryTouchData.Kind;
            GameObject objectBeingInteractedWith = primaryTouchData.targetObject;
            Vector3 interactionPosition = primaryTouchData.interactionPosition;

            if (previousObject != objectBeingInteractedWith || interactionKind != previousKind)
            {
                previousObject = objectBeingInteractedWith;
                previousKind = interactionKind;
            }

            if (objectBeingInteractedWith != lastSpatialTouchedObject)
            {
                if (lastSpatialTouchedObject != null)
                {
                    foreach (var listener in lastSpatialTouchedListeners)
                    {
                        listener.TouchEnd();
                    }
                }
                lastSpatialTouchedObject = objectBeingInteractedWith;
                lastSpatialTouchedListeners = new List<ISpatialTouchListener>(objectBeingInteractedWith.GetComponentsInParent<ISpatialTouchListener>());
                foreach (var listener in lastSpatialTouchedListeners)
                {
                    listener.TouchStart(interactionKind, interactionPosition);
                }
            }
            else
            {
                foreach (var listener in lastSpatialTouchedListeners)
                {
                    listener.TouchStay(interactionKind, interactionPosition);
                }
            }
            if (lastSpatialTouchedListeners.Count == 0 && grabber)
            {
                if (interactionKind == SpatialPointerKind.Touch) {
                    grabber.isGrabbing = false;
                }
                else
                {
                    bool contactGrabbing = interactionKind == SpatialPointerKind.Touch || interactionKind == SpatialPointerKind.DirectPinch;
                    // In unbounded mode, grabbing while touching is already handled by normal grabber logic: skip spatial touch for grabbing to avoid duplicate logic
                    if (doNotUseContactSpatialGrabbingInUnboundedMode && currentMode == VolumeCamera.PolySpatialVolumeCameraMode.Unbounded && contactGrabbing)
                    {
                        grabber.isGrabbing = false;
                        grabber.transform.position = interactionPosition;
                        grabber.transform.rotation = primaryTouchData.inputDeviceRotation;
                    }
                    else
                    {
                        if (grabber.gameObject.activeSelf == false) grabber.gameObject.SetActive(true);
                        grabber.isGrabbing = true;
                        grabber.transform.position = interactionPosition;
                        grabber.transform.rotation = primaryTouchData.inputDeviceRotation;
                    }

                }
            }
            if (debugRepresentation)
            {
                debugRepresentation.SetActive(true);
                debugRepresentation.transform.position = primaryTouchData.inputDevicePosition;
                debugRepresentation.transform.rotation = primaryTouchData.inputDeviceRotation;
            }

        }

        public void OnTouchInactive() {
            if (grabber)
            {
                grabber.isGrabbing = false;
            }
            if (debugRepresentation)
            {
                debugRepresentation.SetActive(false);
            }
            if (lastSpatialTouchedObject != null)
            {
                foreach (var listener in lastSpatialTouchedListeners)
                {
                    listener.TouchEnd();
                }
            }
            lastSpatialTouchedObject = null;
            lastSpatialTouchedListeners.Clear();
        }
    }

    public VolumeCamera volumeCamera;
    const int MAX_TOUCHES = 2;
    public SpatialTouchTracker[] trackers = new SpatialTouchTracker[MAX_TOUCHES];
    public bool replaceContactGrabber = true;

    VolumeCamera.PolySpatialVolumeCameraMode currentMode;
    bool doNotUseContactSpatialGrabbingInUnboundedMode = false;
    bool hardwareRigGrabberDisabled = false;


    private void Awake()
    {
        var grabbers = FindObjectsOfType<SpatialGrabber>();
        for(int i = 0; i < trackers.Length; i++) {
            if (grabbers.Length > i) trackers[i].grabber = grabbers[i];
            trackers[i].lastSpatialTouchedListeners = new List<ISpatialTouchListener>();
        }
        volumeCamera = FindObjectOfType<VolumeCamera>(true);
        if (volumeCamera)
        {
            volumeCamera.OnWindowEvent.AddListener(OnVolumeCameraWindowEvent);
        }
    }

    private void OnVolumeCameraWindowEvent(VolumeCamera.WindowState windowState)
    {
        currentMode = windowState.Mode;
        Debug.Log("OnVolumeCameraWindowEvent: VolumeCameraMode: " + currentMode);
    }

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void Update()
    {
        doNotUseContactSpatialGrabbingInUnboundedMode = !replaceContactGrabber;
#if UNITY_VISIONOS && !UNITY_EDITOR
        if (doNotUseContactSpatialGrabbingInUnboundedMode == false && hardwareRigGrabberDisabled == false) {
            var hardwareRig = FindObjectOfType<HardwareRig>();
            if (hardwareRig)
            {
                var hardwareRigGrabbers = hardwareRig.GetComponentsInChildren<Grabber>();
                foreach (var g in hardwareRigGrabbers) g.enabled = false;
                hardwareRigGrabberDisabled = true;
            }
        }

        for (int i = 0; i < trackers.Length; i++) trackers[i].isUsed = false;
        var activeTouches = Touch.activeTouches;
        // You can determine the number of active inputs by checking the count of activeTouches
        foreach (var activeTouch in activeTouches)
        {
            // For getting access to PolySpatial (visionOS) specific data you can pass an active touch into the EnhancedSpatialPointerSupport()
            SpatialPointerState primaryTouchData = EnhancedSpatialPointerSupport.GetPointerState(activeTouch);

            GameObject objectBeingInteractedWith = primaryTouchData.targetObject;
            int freeTrackerIndex = -1;
            bool touchHandled = false;
            for (int i = 0; i < trackers.Length; i++) {
                if (trackers[i].isUsed == true) {
                    // Multitouch on the same object not handled
                    continue;
                }
                else if (trackers[i].lastSpatialTouchedObject == objectBeingInteractedWith) {
                    trackers[i].OnTouchUpdate(primaryTouchData, currentMode, doNotUseContactSpatialGrabbingInUnboundedMode);
                    trackers[i].isUsed = true;
                    touchHandled = true;
                    break;
                }
                else if(freeTrackerIndex == -1 && trackers[i].lastSpatialTouchedObject == null)
                {
                    freeTrackerIndex = i;
                }
            }
            if(touchHandled == false && freeTrackerIndex != -1) {
                trackers[freeTrackerIndex].OnTouchUpdate(primaryTouchData, currentMode, doNotUseContactSpatialGrabbingInUnboundedMode);
                trackers[freeTrackerIndex].isUsed = true;
            }
        }
        for (int i = 0; i < trackers.Length; i++)
        {
            if(trackers[i].isUsed == false)
            {
                trackers[i].OnTouchInactive();
            }
        }
#endif
    }
#endif
    }
}