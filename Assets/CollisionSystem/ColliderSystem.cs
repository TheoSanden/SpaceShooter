using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using System;
using Theo;


[System.Serializable]
struct CollisionChannel 
{
    [SerializeField]
    public CollisionLayer layer;
    [SerializeField]
    public CollisionLayer[] canCollideWith;
}
[BurstCompile]
public class ColliderSystem : MonoBehaviour
{
    static ColliderSystem instance;
    [SerializeField]
    CollisionChannel[] collisionChannels;


    HashSet<Theo.Collider> TotalColliders = new HashSet<Theo.Collider>();
    Dictionary<CollisionLayer,HashSet<Theo.Collider>> CollisionLayerDictionary = new Dictionary<CollisionLayer, HashSet<Theo.Collider>>();

    Dictionary<JobHandle, NativeArray<int2>> Jobs = new Dictionary<JobHandle, NativeArray<int2>>();
    HashSet<JobHandle> ActiveJobs = new HashSet<JobHandle>();

    QuadTree quadtree;
    ColliderSystem()
    {
        if (ColliderSystem.Instance)
        {
            Debug.LogError("Only one instance of ColliderSystem Allowed");
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    private void Awake()
    {
        foreach(var i in Enum.GetValues(typeof(CollisionLayer))) 
        {
            CollisionLayerDictionary.Add((CollisionLayer)i,new HashSet<Theo.Collider>());
        }
    }
    private void Start()
    {
        quadtree =  new QuadTree(CameraBounds.GetCameraBoundsClass(),4,5);
    }

    public static ColliderSystem Instance 
    {
        get => instance;
    }

    public void Register(Theo.Collider collider) 
    {
        CollisionLayerDictionary[collider.InLayer].Add(collider);
        TotalColliders.Add(collider);
    }
    public void DeRegister(Theo.Collider collider) 
    {
        CollisionLayerDictionary[collider.InLayer].Remove(collider);
        TotalColliders.Remove(collider);
        quadtree.Remove(collider);
    }
    private void Update()
    {
        foreach (Theo.Collider collider in TotalColliders) 
        {
            quadtree.Remove(collider);
            collider.UpdateBounds();
            quadtree.Insert(collider);
        }
    }

    private void OnDrawGizmos()
    {
        if (quadtree == null) return;
        Gizmos.color = Color.black;
        List<Bounds> allBounds = new List<Bounds>();
        quadtree.GetAllBounds(ref allBounds);

        Vector3[] boundPoints = new Vector3[4];
        foreach(Bounds bound in allBounds)
        {
            boundPoints[0] = bound.center + new Vector3(-bound.extents.x,bound.extents.y);
            boundPoints[1] = bound.center + new Vector3(bound.extents.x, bound.extents.y);
            boundPoints[2] = bound.center + new Vector3(bound.extents.x, -bound.extents.y);
            boundPoints[3] = bound.center + new Vector3(-bound.extents.x, -bound.extents.y);

            Gizmos.DrawLine(boundPoints[0],boundPoints[1]);
            Gizmos.DrawLine(boundPoints[1], boundPoints[2]);
            Gizmos.DrawLine(boundPoints[2], boundPoints[3]);
            Gizmos.DrawLine(boundPoints[3], boundPoints[0]);
        }   
    }

    /*private void FixedUpdate()
    {

    foreach (HashSet<Theo.Collider> set in CollisionLayerDictionary.Values)
    {
        foreach (Theo.Collider collider in set)
        {
            foreach (CollisionLayer layer in collider.CollideWith)
            {
                foreach (Theo.Collider other in CollisionLayerDictionary[layer])
                {
                    if (other == collider) { continue; }

                    collider.CalculateCollision(other);
                }
            }
        }
     }

     }*/
    /*  [BurstCompile]
      private void ProcessResults() 
      {
          if (ActiveJobs.Count == 0) { return; }

          foreach (JobHandle job in ActiveJobs) 
          {
              job.Complete();
          }
          foreach (var pair in Jobs) 
          {
              pair.Key.Complete();

              foreach (int2 collisionPair in pair.Value) 
              {
                  if(collisionPair.x == 0) { continue; }
                  Theo.Collider col1;
                  bool ColOneExists = TotalColliders.TryGetValue(collisionPair.x, out col1);
                  Theo.Collider col2;
                  bool ColTwoExists = TotalColliders.TryGetValue(collisionPair.y, out col2);
                  if (ColOneExists && ColTwoExists) 
                  {
                      col1.onCollision?.Invoke(col2.gameObject);
                  }
              }
              pair.Value.Dispose();
          }
      }
      [BurstCompile]
      private void CreateCollisionJobs() 
      {
          foreach (CollisionChannel channel in collisionChannels)
          {
              HashSet<Theo.Collider> colliderSet = CollisionLayerDictionary[channel.layer];
              NativeArray<NativeArray<float3>> ColliderPoints = new NativeArray<NativeArray<float3>>(colliderSet.Count,Allocator.TempJob);

              int OtherColliderPointsCount = 0;
              foreach(CollisionLayer layer in channel.canCollideWith) 
              {
                  OtherColliderPointsCount += CollisionLayerDictionary[layer].Count;
              }

              NativeArray<NativeArray<float3>> OtherColliderPoints = new NativeArray<NativeArray<float3>>(OtherColliderPointsCount, Allocator.TempJob);
              NativeArray<int> HashList = new NativeArray<int>(colliderSet.Count + OtherColliderPointsCount,Allocator.TempJob);


              NativeArray<Matrix4x4> FirstLocalToWorld = new NativeArray<Matrix4x4>(colliderSet.Count,Allocator.TempJob);
              NativeArray<Matrix4x4> SecondLocalToWorld = new NativeArray<Matrix4x4>(OtherColliderPointsCount,Allocator.TempJob);

              int firstPassIndex = 0;
              foreach (Theo.Collider col in colliderSet)
              {
                  ColliderPoints[firstPassIndex] = col.NativePoints;
                  HashList[firstPassIndex] = col.GetHashCode();
                  FirstLocalToWorld[firstPassIndex] = col.transform.localToWorldMatrix;
                  firstPassIndex++;
              }

              int secondPassIndex = 0;
              foreach (CollisionLayer layer in channel.canCollideWith)
              {
                  foreach (Theo.Collider col in CollisionLayerDictionary[layer]) 
                  {
                      OtherColliderPoints[secondPassIndex] = col.NativePoints;
                      HashList[firstPassIndex + secondPassIndex] = col.GetHashCode();
                      SecondLocalToWorld[secondPassIndex] = col.transform.localToWorldMatrix;
                  }
              }
              NativeArray<int2> results = new NativeArray<int2>(colliderSet.Count + OtherColliderPointsCount,Allocator.TempJob);

              NativeArray<NativeArray<float3>> FirstConvertedPoints = new NativeArray<NativeArray<float3>>(colliderSet.Count,Allocator.TempJob);
              NativeArray<NativeArray<float3>> SecondConvertedPoints = new NativeArray<NativeArray<float3>>(OtherColliderPointsCount, Allocator.TempJob);

              JobHandle DependancyOne = new ConvertLocalPointsToWorld(ColliderPoints, FirstLocalToWorld, FirstConvertedPoints).Schedule(colliderSet.Count,64);
              JobHandle DependancyTwo = new ConvertLocalPointsToWorld(OtherColliderPoints, FirstLocalToWorld, FirstConvertedPoints).Schedule(OtherColliderPointsCount, 64);

              JobHandle job = new ComputePolygonCollisionsInLayer(
                  FirstConvertedPoints,
                  SecondConvertedPoints,
                  HashList,
                  results
                  ).Schedule(OtherColliderPointsCount,1,JobHandle.CombineDependencies(DependancyOne,DependancyTwo));

              ActiveJobs.Add(DependancyOne);
              ActiveJobs.Add(DependancyTwo);
              ActiveJobs.Add(job);

              Jobs.Add(job, results);
          }
      }
  }
  [BurstCompile]
  struct ConvertLocalPointsToWorld: IJobParallelFor 
  {
      [ReadOnly]
      NativeArray<NativeArray<float3>> LocalPoints;
      [ReadOnly]
      NativeArray<Matrix4x4> LocalToWorld;

      NativeArray<NativeArray<float3>> Result;

      public ConvertLocalPointsToWorld(NativeArray<NativeArray<float3>> LocalPoints, NativeArray<Matrix4x4> LocalToWorld, NativeArray<NativeArray<float3>> Result) 
      {
          this.LocalPoints = LocalPoints;
          this.LocalToWorld = LocalToWorld;
          this.Result = Result;
      }
      [BurstCompile]
      public void Execute(int i) 
      {
          NativeArray<float3> result = new NativeArray<float3>(LocalPoints[i].Length,Allocator.TempJob);
          result.CopyFrom(LocalPoints[i]);

          for (int j = 0; j < result.Length; j++) 
          {
              //Result[i][j] = LocalToWorld[i].MultiplyPoint(LocalPoints[i][j]);
              result[j] = LocalToWorld[i].MultiplyPoint(result[j]);
          }
          Result[i] = result;
      }
  }
  [BurstCompile]
  struct ComputePolygonCollisionsInLayer : IJobParallelFor
  {
      [ReadOnly]
      NativeArray<NativeArray<float3>> ColliderPoints;
      [ReadOnly]
      NativeArray<NativeArray<float3>> OtherColliderPoints;
      //Hashlist has to be Colliderpoints + OtherColliderpoints in lenght and in that order, a hashkey has to correspond to appropriate colliderpoints
      [ReadOnly]
      NativeArray<int> HashList;

      NativeArray<int2> Results;

      public ComputePolygonCollisionsInLayer(NativeArray<NativeArray<float3>> ColliderPoints, 
                                             NativeArray<NativeArray<float3>> OtherColliderPoints,
                                             NativeArray<int> HashList,
                                             NativeArray<int2> Results) 
      {
          this.ColliderPoints = ColliderPoints;
          this.OtherColliderPoints = OtherColliderPoints;
          this.HashList = HashList;
          this.Results = Results;
      }
      [BurstCompile]
      public void Execute(int i) 
      {
          for(int j = 0; j < OtherColliderPoints.Length; j++) 
          {
              bool registeredCollisionA = CheckIfPointsInsidePolygon(ColliderPoints[i], OtherColliderPoints[j]);
              bool registeredCollisionB = CheckIfPointsInsidePolygon(ColliderPoints[j], OtherColliderPoints[i]);

              if (registeredCollisionA || registeredCollisionB)
              {
                  Results[i] = new int2(HashList[i], HashList[ColliderPoints.Length + j]);
              }
              else 
              {
                  Results[i] = new int2(0, 0);
              }
          }
      }
      [BurstCompile]
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

                  if (i == a.Length - 1 && cross.z < 0)
                  {
                      return true;
                  }
              }
          }
          return false;
      }*/

}

