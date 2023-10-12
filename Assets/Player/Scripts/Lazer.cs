using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lazer : MonoBehaviour
{
    [SerializeField]
    float TravelVelocity = 1;
    [SerializeField]
    LayerMask HitMask;
    [SerializeField]
    int Damage = 1;
    // Update is called once per frame
    void Update()
    {
        transform.position += transform.up * Time.deltaTime * TravelVelocity;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 6) 
        {
            Health health;
            if (collision.gameObject.TryGetComponent<Health>(out health)) 
            {
                health.Apply(-Damage);
                Destroy(this.gameObject);
            }
        }
    }
}
