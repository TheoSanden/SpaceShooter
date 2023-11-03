using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Theo 
{
    [BurstCompile]
    public class PolygonCollider : Collider
    {

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (points.Length < 3) { return; }
            Handles.color = Color.green;
            for (int i = 0; i < points.Length; i++)
            {
                Handles.DrawAAPolyLine(WorldPoints[i], (i + 1 == points.Length) ? WorldPoints[0] : WorldPoints[i + 1]);
            }

            Bounds _b = Bounds;
            Vector3 extents = _b.extents;
            Vector3 center = _b.center;

            Vector3[] boundsPoints = new Vector3[]{ center + new Vector3(-extents.x,extents.y),
                                                    center + new Vector3(extents.x,extents.y),
                                                    center + new Vector3(extents.x,-extents.y),
                                                    center + new Vector3(-extents.x,-extents.y),
                                                    center + new Vector3(-extents.x,extents.y)};

            Handles.color = Color.blue;
            for (int i = 0; i < boundsPoints.Length; i++)
            {
                Handles.DrawAAPolyLine(boundsPoints[i], (i + 1 == boundsPoints.Length) ? boundsPoints[0] : boundsPoints[i + 1]);
            }
        }
#endif
    }
}