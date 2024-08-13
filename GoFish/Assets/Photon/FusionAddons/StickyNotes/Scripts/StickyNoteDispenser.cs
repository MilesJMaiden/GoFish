
using Fusion.XRShared.Demo;
using System.Collections;
using UnityEngine;

public class StickyNoteDispenser : GrabbablePrefabSpawner
{

    [Header("StickyNoteDispenser")]
    [SerializeField] Transform stickyNoteTargetTransform;
    float animationDuration = 1f;

    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(MoveStickyNote(spawnerGrabbableReference.transform, animationDuration));
    }


    protected override void ResetReferencePose()
    {
        base.ResetReferencePose();
        StartCoroutine(MoveStickyNote(spawnerGrabbableReference.transform,  animationDuration));
    }

    IEnumerator MoveStickyNote(Transform objTransform, float duration = 1f)
    {
        float timeElapsed = 0;
        Vector3 startPosition = stickyNoteTargetTransform.InverseTransformPoint(objTransform.position);

        while (timeElapsed < duration)
        {
        
            var position = Vector3.Lerp(startPosition, Vector3.zero, timeElapsed / duration);
            objTransform.position = stickyNoteTargetTransform.TransformPoint(position);

            timeElapsed += Time.deltaTime;
            yield return null;
        }
        objTransform.position = stickyNoteTargetTransform.position;
    }



}
