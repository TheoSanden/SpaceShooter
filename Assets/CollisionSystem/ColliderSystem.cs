using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Theo;
public class ColliderSystem : MonoBehaviour
{
    HashSet<Theo.Collider> Colliders = new HashSet<Theo.Collider>();
    static ColliderSystem instance;

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

    public static ColliderSystem Instance 
    {
        get => instance;
    }

    public void Register(Theo.Collider collider) 
    {
        Colliders.Add(collider);
    }
    public void DeRegister(Theo.Collider collider) 
    {
        Colliders.Remove(collider);
    }
    private void FixedUpdate()
    {
        foreach (Theo.Collider collider in Colliders) 
        {
            foreach (Theo.Collider other in Colliders) 
            {
                if(other == collider) { continue; }

                if (collider.IsInside(other)) 
                {
                    collider.onCollision(other.gameObject);
                }
            }
        }
    }
}
