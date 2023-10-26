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
        bool ProcessCollisions;
        List<NativeArray<bool>> results = new List<NativeArray<bool>>();
        List<Collider> collidingWithList = new List<Collider>();
        List<JobHandle> jobHandles = new List<JobHandle>();
        NativeArrayPooler<bool> BoolPool = new NativeArrayPooler<bool>(1);
        
        public override void CalculateCollision(Collider other)
        {
            ProcessCollisions = true;
            collidingWithList.Add(other);
            NativeArray<bool> result = BoolPool.Pop();
            results.Add(result);
            jobHandles.Add(new ComputePolygonCollisionJob(NativePoints, transform.localToWorldMatrix, other.NativePoints, other.transform.localToWorldMatrix, result).Schedule());
        }
        private void LateUpdate()
        {
            if (ProcessCollisions) 
            {
                for (int i = 0; i < results.Count; i++) 
                {
                    jobHandles[i].Complete();
                    if (results[i][0]) 
                    {
                        onCollision?.Invoke(collidingWithList[i].gameObject);
                    }
                }
                ClearWorkload();
            }
        }
        private void ClearWorkload() 
        {
            ProcessCollisions = false;

            foreach (JobHandle job in jobHandles) 
            {
                    job.Complete();
            }
            foreach (NativeArray<bool> result in results)
            {
                BoolPool.Queue(result);
            }
            results.Clear();
            collidingWithList.Clear();
            jobHandles.Clear();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ClearWorkload();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearWorkload();
            BoolPool.ClearAll();
        }


        [BurstCompile]
        public struct ComputePolygonCollisionJob : IJob
        {
            [DeallocateOnJobCompletionAttribute]
            private NativeArray<float3> nativePoints;
            private Matrix4x4 nativeLocalToWorldMatrix;
            [DeallocateOnJobCompletionAttribute]
            private NativeArray<float3> otherPoints;
            private Matrix4x4 otherLocalToWorldMatrix;

            private NativeArray<bool> isInside;

            public ComputePolygonCollisionJob(NativeArray<float3> nativePoints, Matrix4x4 nativeLocalToWorldMatrix, NativeArray<float3> otherPoints, Matrix4x4 otherLocalToWorldMatrix, NativeArray<bool> isInside) 
            {
                this.nativePoints = new NativeArray<float3>(nativePoints.Length,Allocator.TempJob);
                this.nativePoints.CopyFrom(nativePoints);
                this.nativeLocalToWorldMatrix = nativeLocalToWorldMatrix;
                this.otherPoints = new NativeArray<float3>(otherPoints.Length, Allocator.TempJob);
                this.otherPoints.CopyFrom(otherPoints);
                this.otherLocalToWorldMatrix = otherLocalToWorldMatrix;
                this.isInside = isInside;
            }
            [BurstCompile]
            public void Execute() 
            {
                isInside[0] = CalculateIfIsInside();
            }
            [BurstCompile]
            private bool CalculateIfIsInside()
            {
                if (nativePoints.Length < 3) { return false; }
                NativeArray<float3> convertedNativePoints =  ConvertToWorldPoints(nativePoints,nativeLocalToWorldMatrix);
                NativeArray<float3> convertedOtherPoints = ConvertToWorldPoints(otherPoints, otherLocalToWorldMatrix);

                bool AInsideB = CheckIfPointsInsidePolygon(convertedNativePoints,convertedOtherPoints);
                bool BInsideA = CheckIfPointsInsidePolygon(convertedOtherPoints, convertedNativePoints);
               
                return (AInsideB || BInsideA);
            }
            private bool CheckIfPointsInsidePolygon(NativeArray<float3> a, NativeArray<float3> b) 
            {
                foreach (float3 otherPoint in a)
                {
                    for (int i = 0; i < b.Length; i++)
                    {
                        float3 AtoB = ((i + 1 == b.Length) ? b[0] : b[i + 1]) - b[i];
                        float3 AtoC = otherPoint - b[i];
                        float3 cross = math.cross(AtoB, AtoC);

                        // the point is to the left of the vector which means that its outside 
                        if (cross.z > 0)
                        {
                            break;
                        }

                        if (i == nativePoints.Length - 1 && cross.z < 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            [BurstCompile]
            private NativeArray<float3> ConvertToWorldPoints(NativeArray<float3> points, Matrix4x4 matrix) 
            {
                NativeArray<float3> ConvertedPoints = new NativeArray<float3>(points.Length, Allocator.Temp);
                for(int i = 0; i < points.Length; i++) 
                {
                    ConvertedPoints[i] = matrix.MultiplyPoint(points[i]);
                }
                return ConvertedPoints;
            }
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