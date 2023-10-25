using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Theo 
{
    public class PolygonCollider : Collider
    {
        private void Start()
        {
            onCollision += HandleCollision;
        }
        // Start is called before the first frame update
        public override bool IsInside(Collider Other)
        {
            if(points.Length < 3){ return false; }
            float3[] TransformedPoints = WorldPoints;
            
            foreach (float3 OtherPoint in Other.WorldPoints) 
            {
                for (int i = 0; i < points.Length; i++)
                {
                    float3 AtoB = Float3Helpers.Subtract((i + 1 == points.Length)? TransformedPoints[0]: TransformedPoints[i+1], TransformedPoints[i]);
                    float3 AtoC = Float3Helpers.Subtract(OtherPoint, TransformedPoints[i]);
                    float3 cross = math.cross(AtoB, AtoC);

                    // the point is to the left of the vector which means that its outside 
                    if (cross.z > 0) 
                    {
                        break;
                    }

                    if (i == points.Length -1 && cross.z < 0) 
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        void HandleCollision(GameObject other) 
        {
            Debug.Log(this.name  + " is colliding with: " + other.name);
        }



#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (points.Length < 3) { return; }
            Handles.color = Color.green;
            for (int i = 0; i < points.Length; i++)
            {
                Handles.DrawAAPolyLine(WorldPoints[i], (i + 1 == points.Length) ? WorldPoints[0] : WorldPoints[i + 1]);
            }
        }
#endif
    }
}