
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Theo 
{
    public enum CollisionLayer
    {
        Default,
        Player,
        Enemy
    }
    public struct ColliderInfo 
    {
        public NativeArray<float3> points;
        public Matrix4x4 localToWorld;
    }
    public abstract class Collider : MonoBehaviour
    {
        [SerializeField]
        CollisionLayer inLayer;
        public CollisionLayer InLayer { get => inLayer; }

        [SerializeField]
        protected float3[] points;

        private ColliderInfo info;
        public ColliderInfo Info 
        {
            get 
            {
                info.localToWorld = this.transform.localToWorldMatrix;
                return info;
            }
        }

        protected Bounds bounds;
        public Bounds Bounds 
        {
            get 
            {
                return bounds;
            }
        }
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
        protected virtual void CalculateBounds() 
        {
            float x = 0;
            float y = 0;

            float3[] rotatedPoints = new float3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                rotatedPoints[i] = transform.localToWorldMatrix.MultiplyPoint(points[i]);
                rotatedPoints[i] -= (float3)transform.position;
            }

            foreach (float3 point in rotatedPoints) 
            {
                if(Mathf.Abs(point.x) > x) { x = point.x; }
                if(Mathf.Abs(point.y) > y) { y = point.y; }
            }
            Vector3 size = new Vector3(x * 2,y * 2);

            bounds = new Bounds(this.transform.position, size);
        }
        void CreateInfo() 
        {
            info = new ColliderInfo();
            info.points = new NativeArray<float3>(points.Length, Allocator.Persistent);
            for (int i = 0; i < points.Length; i++)
            {
                info.points[i] = points[i];
            }
        }
        private void Awake()
        {
            CreateInfo();
            CalculateBounds();
        }
        protected virtual void OnEnable()
        {
            if (ColliderSystem.Instance) 
            {
                ColliderSystem.Instance.Register(this);
            }
        }
        protected virtual void OnDisable()
        {
            if (ColliderSystem.Instance) 
            {
                ColliderSystem.Instance.DeRegister(this);
            }
        }
        protected virtual void OnDestroy() 
        {
            if (ColliderSystem.Instance) 
            {
                ColliderSystem.Instance.DeRegister(this);
            }
            info.points.Dispose();
        }


        public void UpdateBounds()
        {
            CalculateBounds();
        }
    }
}
