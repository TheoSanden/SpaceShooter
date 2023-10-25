using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Theo 
{
    public abstract class Collider : MonoBehaviour
    {
        [SerializeField]
        protected float3[] points;
        public float3[] LocalPoints 
        {
            get=> points;
        }
        public float3[] WorldPoints 
        {
            get 
            {
                float3[] transformedPoints = new float3[points.Length];
                for(int i = 0; i < transformedPoints.Length; i++) 
                {
                    transformedPoints[i] = transform.localToWorldMatrix.MultiplyPoint(points[i]);
                }
                return transformedPoints;
            }
        }
        public delegate void OnCollision(GameObject Other);
        public OnCollision onCollision;
        public abstract bool IsInside(Collider Other);

        private void OnEnable()
        {
            if (ColliderSystem.Instance) 
            {
                ColliderSystem.Instance.Register(this);
            }
        }
        private void OnDisable()
        {
            if (ColliderSystem.Instance) 
            {
                ColliderSystem.Instance.DeRegister(this);
            }
        }
    }
}
