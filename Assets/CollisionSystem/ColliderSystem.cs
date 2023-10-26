using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Theo;
public class ColliderSystem : MonoBehaviour
{
    static ColliderSystem instance;

    Dictionary<CollisionLayer,HashSet<Theo.Collider>> CollisionLayerDictionary = new Dictionary<CollisionLayer, HashSet<Theo.Collider>>();

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

    public static ColliderSystem Instance 
    {
        get => instance;
    }

    public void Register(Theo.Collider collider) 
    {
        CollisionLayerDictionary[collider.InLayer].Add(collider);
    }
    public void DeRegister(Theo.Collider collider) 
    {
        CollisionLayerDictionary[collider.InLayer].Remove(collider);
    }
    private void FixedUpdate()
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
    }
}

