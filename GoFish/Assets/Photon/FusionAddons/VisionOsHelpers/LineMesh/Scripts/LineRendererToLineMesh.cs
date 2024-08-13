using UnityEngine;

namespace Fusion.Addons.VisionOsHelpers
{
    /**
     * 
     * LineRendererToLineMesh is an alternative solution to the LineRenderer component (currently not compatible with Polyspatial).
     * It offers a way to automatically feed a LineMesh with the line renderer info.
     * The stored Point include a color property that will affect the mesh vertex colors.
     * A sample shader, LineSG, compatible with Polyspatial, uses those vertex color info to render the mesh.
     * The LineSGMaterial material in the addon is using this shader. 
     * 
     * checkPositionsEveryFrame bool is useful for a line renderer whose existing points changes (it is not needed if you just add points)
     * replicateLineRendererEnabledStatus bool is useful if you had script enabling/disabling the linerenderer, and you want the replacement to mimic this
     *  
     **/
    public class LineRendererToLineMesh : MonoBehaviour
    {
        LineRenderer lineRenderer;
        LineMesh lineMesh;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        bool rigTypeChecked = false;

        public bool visionOsOnly = true;
        public bool checkPositionsEveryFrame = false;
        public bool replicateLineRendererEnabledStatus = false;

        bool prepared = false;

        void Init()
        {
            if (prepared) return;
            prepared = true;
            meshRenderer = GetComponent<MeshRenderer>();
            lineRenderer = GetComponent<LineRenderer>();
            lineMesh = GetComponent<LineMesh>();
            meshFilter = GetComponent<MeshFilter>();
            if (meshRenderer == null)
            {
                Debug.LogError("Mesh filter required");
            }
            if (meshFilter == null)
            {
                Debug.LogError("Mesh renderer required");
            }
            if (lineMesh == null)
            {
                lineMesh = gameObject.AddComponent<LineMesh>();
            }
            lineMesh.width = 1f;
        }

        private void Update()
        {
            if (visionOsOnly && rigTypeChecked == false)
            {
#if UNITY_VISIONOS
            Init();
            rigTypeChecked = true;
            Debug.Log("LineRendererToLineMesh: AVP detected, replacing line renderer");
            if(replicateLineRendererEnabledStatus == false)
            {
                lineRenderer.enabled = false;
            }
            if (meshRenderer) meshRenderer.enabled = true;
#else
                // Not AVP detected: LineRendererToLineMesh is useful for visionOS only
                enabled = false;
                return;
#endif
            }
            if (replicateLineRendererEnabledStatus)
            {
                meshRenderer.enabled = lineRenderer.enabled;
            }

            UpdateMeshWithLineRendererPoints();
        }

        [ContextMenu("UpdateMeshWithLineRendererPoints")]
        void UpdateMeshWithLineRendererPoints()
        {
            bool pointsChange = false;
            if (lineRenderer.useWorldSpace) pointsChange = true;
            if (checkPositionsEveryFrame && pointsChange == false)
            {
                for (int i = 0; i < lineMesh.points.Count; i++)
                {
                    if (i >= lineRenderer.positionCount)
                    {
                        pointsChange = true;
                        break;
                    }
                    if (lineRenderer.GetPosition(i) != lineMesh.points[i].relativePosition || lineMesh.points[i].color != lineRenderer.startColor)
                    {
                        pointsChange = true;
                        break;
                    }
                }
            }
            if (pointsChange)
            {
                lineMesh.points.Clear();
                lineMesh.ResetMesh();
            }
            while (lineRenderer.positionCount > lineMesh.points.Count)
            {
                var i = lineMesh.points.Count;
                var point = new LineMesh.Point();
                if (lineRenderer.useWorldSpace)
                {
                    point.relativePosition = lineRenderer.transform.InverseTransformPoint(lineRenderer.GetPosition(i));
                }
                else
                {
                    point.relativePosition = lineRenderer.GetPosition(i);

                }
                point.color = lineRenderer.startColor;
                point.pressure = lineRenderer.startWidth;
                lineMesh.points.Add(point);
                lineMesh.UpdateMesh();
            }
        }
    }
}
