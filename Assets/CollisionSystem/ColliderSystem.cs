using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using System;
using System.Linq;
using Theo;


[System.Serializable]
struct CollisionChannel
{
    [SerializeField]
    public CollisionLayer layer;
    [SerializeField]
    public CollisionLayer[] canCollideWith;
}
public struct BoxCollisionJobInfo 
{
    NativeArray<Bounds> originalBounds;
    NativeArray<Bounds> otherBounds;
    NativeArray<int> hashList;

    public BoxCollisionJobInfo(NativeArray<Bounds> originalBounds,
                                NativeArray<Bounds> otherBounds,
                                NativeArray<int> hashList) 
    {
        this.originalBounds = originalBounds;
        this.otherBounds = otherBounds;
        this.hashList = hashList;
    }
    public void Dispose() 
    {
        originalBounds.Dispose();
        otherBounds.Dispose();
        hashList.Dispose();
    }
} 

[BurstCompile]
public class ColliderSystem : MonoBehaviour
{
    static ColliderSystem instance;
    [SerializeField]
    CollisionChannel[] collisionChannels;


    Dictionary<int, Theo.Collider> TotalColliders = new Dictionary<int, Theo.Collider>();
    Dictionary<CollisionLayer, HashSet<Theo.Collider>> CollisionLayerDictionary = new Dictionary<CollisionLayer, HashSet<Theo.Collider>>();

    Dictionary<JobHandle, NativeArray<int2>> Jobs = new Dictionary<JobHandle, NativeArray<int2>>();
    HashSet<BoxCollisionJobInfo> BoxCollisionJobInfoSet = new HashSet<BoxCollisionJobInfo>();

    //QuadTree quadtree;
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
        foreach (var i in Enum.GetValues(typeof(CollisionLayer)))
        {
            CollisionLayerDictionary.Add((CollisionLayer)i, new HashSet<Theo.Collider>());
        }
    }
    private void Start()
    {
        //quadtree =  new QuadTree(CameraBounds.GetCameraBoundsClass(),4,5);
    }

    public static ColliderSystem Instance
    {
        get => instance;
    }

    public void Register(Theo.Collider collider)
    {
        CollisionLayerDictionary[collider.InLayer].Add(collider);
        TotalColliders.Add(collider.GetHashCode(), collider);
    }
    public void DeRegister(Theo.Collider collider)
    {
        CollisionLayerDictionary[collider.InLayer].Remove(collider);
        TotalColliders.Remove(collider.GetHashCode());
        //quadtree.Remove(collider);
    }
    private void FixedUpdate()
    {
        ProcessResults();
        foreach (KeyValuePair<int, Theo.Collider> pair in TotalColliders)
        {
            pair.Value.UpdateBounds();
        }

        CreateCollisionJobs();
    }

    private void OnDrawGizmos()
    {
     
    }

    [BurstCompile]
    private void ProcessResults()
    {
        if (Jobs.Count == 0) { return; }

        foreach (var pair in Jobs)
        {
            pair.Key.Complete();

            foreach (int2 collisionPair in pair.Value)
            {
                if (collisionPair.x == 0) { continue; }
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
        foreach (BoxCollisionJobInfo info in BoxCollisionJobInfoSet)
        {
            info.Dispose();
        }
    }
    [BurstCompile]
    private void CreateCollisionJobs()
    {
        Jobs.Clear();
        BoxCollisionJobInfoSet.Clear();
        foreach (CollisionChannel channel in collisionChannels)
        {
  
            HashSet<Theo.Collider> colliderSet = CollisionLayerDictionary[channel.layer];
            if (colliderSet.Count == 0) { continue; }
            int CollidersInOriginalLayer = CollisionLayerDictionary[channel.layer].Count;


            int CollidersInOtherLayers = 0;
            foreach (CollisionLayer layer in channel.canCollideWith)
            {
                CollidersInOtherLayers += CollisionLayerDictionary[layer].Count;
            }
            if (CollidersInOriginalLayer == 0) { continue; }

            NativeArray<Bounds> ColliderBounds = new NativeArray<Bounds>(CollidersInOriginalLayer, Allocator.TempJob);



            NativeArray<Bounds> OtherColliderBounds = new NativeArray<Bounds>(CollidersInOtherLayers, Allocator.TempJob);
            NativeArray<int> HashList = new NativeArray<int>(CollidersInOriginalLayer + CollidersInOtherLayers, Allocator.TempJob);


            int firstPassIndex = 0;

            foreach (Theo.Collider col in colliderSet)
            {
                ColliderBounds[firstPassIndex] = col.Bounds;
                HashList[firstPassIndex] = col.GetHashCode();
                firstPassIndex++;
            }

            int secondPassIndex = 1;

            foreach (CollisionLayer layer in channel.canCollideWith)
            {
                foreach (Theo.Collider col in CollisionLayerDictionary[layer])
                {
                    OtherColliderBounds[secondPassIndex - 1] = col.Bounds;
                    HashList[firstPassIndex + secondPassIndex-1] = col.GetHashCode();
                    secondPassIndex++;
                }
            }

            NativeArray<int2> results = new NativeArray<int2>(CollidersInOriginalLayer + CollidersInOtherLayers, Allocator.TempJob);

            JobHandle job = new ComputeBoxCollisionsInLayer(
                ColliderBounds,
                OtherColliderBounds,
                HashList,
                results
                ).Schedule(CollidersInOriginalLayer, (CollidersInOriginalLayer < 100)? 128: 64);
            Jobs.Add(job, results);
            BoxCollisionJobInfoSet.Add(new BoxCollisionJobInfo(ColliderBounds,OtherColliderBounds,HashList));
        }
    }
}

[BurstCompile]
struct ComputeBoxCollisionsInLayer : IJobParallelFor
{
    [ReadOnly]
    NativeArray<Bounds> OriginalLayerBounds;
    [ReadOnly]
    NativeArray<Bounds> OtherLayerBounds;
    //Hashlist has to be Colliderpoints + OtherColliderpoints in lenght and in that order, a hashkey has to correspond to appropriate colliderpoints
    [ReadOnly]
    NativeArray<int> HashList;
    NativeArray<int2> Results;

    public ComputeBoxCollisionsInLayer(NativeArray<Bounds> OriginalLayerBounds,
                                           NativeArray<Bounds> OtherLayerBounds,
                                           NativeArray<int> HashList,
                                           NativeArray<int2> Results)
    {
        this.OriginalLayerBounds = OriginalLayerBounds;
        this.OtherLayerBounds = OtherLayerBounds;
        this.HashList = HashList;
        this.Results = Results;
    }
    [BurstCompile]
    public void Execute(int i)
    {
        for (int j = 0; j < OtherLayerBounds.Length; j++)
        {
            if (OriginalLayerBounds[i].Intersects(OtherLayerBounds[j]))
            {
                Results[i] = new int2(HashList[i], HashList[OriginalLayerBounds.Length + j]);
                break;
            }
        }
    }
}

