
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
    public abstract class Collider : MonoBehaviour
    {
        [SerializeField]
        CollisionLayer inLayer;
        [SerializeField]
        CollisionLayer[] collideWith;
        public CollisionLayer InLayer { get => inLayer; }
        public CollisionLayer[] CollideWith { get => collideWith; }

        [SerializeField]
        protected float3[] points;

        [ReadOnly,NativeDisableContainerSafetyRestriction]
        protected NativeArray<float3> points_NativeArray;

        protected Bounds bounds;
        public Bounds Bounds 
        {
            get 
            {
                return bounds;
            }
        }

        public NativeArray<float3> NativePoints 
        {
            get => points_NativeArray;
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
        //public abstract bool IsInside(Collider Other);
        public abstract void CalculateCollision(Collider other);
        protected virtual void CalculateBounds() 
        {
            float x = 0;
            float y = 0;
            foreach(float3 point in points) 
            {
                if(Mathf.Abs(point.x) > x) { x = point.x; }
                if(Mathf.Abs(point.y) > y) { y = point.y; }
            }
            Vector3 size = new Vector3(x * 2,y * 2);

            bounds = new Bounds(this.transform.position, size);
        }
        void CreateNativeArrayPoints() 
        {
            points_NativeArray = new NativeArray<float3>(points.Length, Allocator.Persistent);
            for (int i = 0; i < points.Length; i++)
            {
                points_NativeArray[i] = points[i];
            }
        }
        private void Awake()
        {
            CreateNativeArrayPoints();
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
            points_NativeArray.Dispose();
        }


        public void UpdateBounds()
        {
            bounds.center = this.transform.position;
        }
    }
}
