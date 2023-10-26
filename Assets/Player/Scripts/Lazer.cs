using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lazer : MonoBehaviour
{
    [SerializeField]
    Theo.Collider col;
    [SerializeField]
    float TravelVelocity = 1;
    [SerializeField]
    LayerMask HitMask;
    [SerializeField]
    int Damage = 1;

    Vector2 cameraBounds;
    Vector2 cameraPosition;

    private void Start()
    {
        cameraPosition = Camera.main.transform.position;
        cameraBounds = CameraBounds.GetCameraBounds();
        col = this.GetComponent<Theo.Collider>();
        col.onCollision += OnCollision;
    }
    // Update is called once per frame
    void Update()
    {
        transform.position += transform.up * Time.deltaTime * TravelVelocity;

        if (!CameraBounds.IsWithinBounds(this.transform.position,cameraBounds,cameraPosition)) 
        {
            Destroy(this.gameObject);
        }
    }
    private void OnCollision(GameObject other)
    {
            Health health;
            if (other.gameObject.TryGetComponent<Health>(out health)) 
            {
                health.Apply(-Damage);
                Destroy(this.gameObject);
            }
    }
}
