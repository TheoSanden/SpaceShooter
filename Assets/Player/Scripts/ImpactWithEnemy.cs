using System.Collections;
using System.Collections.Generic;
using Theo;
using UnityEngine;

[RequireComponent(typeof(Theo.Collider))]
public class ImpactWithEnemy : MonoBehaviour
{
    Theo.Collider col;
    Health health;

    private void Awake()
    {
        col = this.GetComponent<Theo.Collider>();
        col.onCollision += OnCollision;
        health = this.GetComponent<Health>();
    }
    void OnCollision(GameObject other) 
    {
        AI.Ship ship;
        if(other.TryGetComponent<AI.Ship>(out ship))
        {
            health.Apply(-1);
            ship.GetComponent<AI.Brain>().DestroyHost();
        }
       
    }
}
