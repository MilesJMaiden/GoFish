using Fusion.XR.Shared.Touch;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.LowLevel;

namespace Fusion.Addons.VisionOsHelpers
{
    /**
    * 
    * SpatialTouchable class implements both ISpatialTouchListener and ITouchable interfaces.
    * So it reacts both to spatial touch, and to regular touch, working on all platforms.
    * It raises onTouchStart & onTouchEnd events.
    *  
    **/

    public class SpatialTouchable : MonoBehaviour, ISpatialTouchListener, ITouchable
    {

        [Header("Unity Event")]
        public UnityEvent onTouchStart;
        public UnityEvent onTouchEnd;

#if POLYSPATIAL_SDK_AVAILABLE
    #region ISpatialTouchListener
    public virtual void TouchStart(SpatialPointerKind interactionKind, Vector3 interactionPosition)
    {
        //Debug.LogError($"Spatial touch ({interactionKind}) => OnTouchStart");
        OnTouchStart();
    }

    public virtual void TouchEnd()
    {
        //Debug.LogError($"Spatial touch => OnTouchEnd");
        OnTouchEnd();
    }

    public virtual void TouchStay(SpatialPointerKind interactionKind, Vector3 interactionPosition) {}
    #endregion
#endif

        #region ITouchable
        public virtual void OnToucherContactStart(Toucher toucher)
        {
            //Debug.LogError("Toucher contact => OnTouchStart");
            OnTouchStart();
        }

        public virtual void OnToucherStay(Toucher toucher)
        {
        }

        public virtual void OnToucherContactEnd(Toucher toucher)
        {
            //Debug.LogError("Toucher contact => OnTouchEnd");
            OnTouchEnd();
        }
        #endregion


        [ContextMenu("OnTouchStart")]
        public virtual void OnTouchStart()
        {
            if (onTouchStart != null) onTouchStart.Invoke();
        }

        [ContextMenu("OnTouchEnd")]
        public virtual void OnTouchEnd()
        {
            if (onTouchEnd != null) onTouchEnd.Invoke();
        }
    }
}
