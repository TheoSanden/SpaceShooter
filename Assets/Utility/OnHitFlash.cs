using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer),typeof(Health))]
public class OnHitFlash : MonoBehaviour
{
    [SerializeField] List<string> hitTag;
    [SerializeField] Material hitMaterial;
    [SerializeField] float hitDuration = 0.001f;
    Material baseMaterial;
    Health health;

    private SpriteRenderer sr;
    private void Start()
    {
        sr = this.GetComponent<SpriteRenderer>();
        baseMaterial = sr.material;

        health = this.GetComponent<Health>();

       
    }
    private void OnEnable()
    {
        if(health == null) { health = this.GetComponent<Health>(); }
        health.OnHealthLoss += Hit_Wrapper;
    }

    private void OnDisable()
    {
        StopCoroutine(Hit());
        sr.sharedMaterial = sr.material = baseMaterial;
        health.OnHealthLoss -= Hit_Wrapper;
    }
    /*
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hitTag.Contains(collision.tag) && gameObject.activeInHierarchy)
        {
            StartCoroutine(Hit());
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hitTag.Contains(collision.collider.tag))
        {
            StartCoroutine(Hit());
        }
    }*/
    private IEnumerator Hit()
    {
        sr.sharedMaterial = sr.material = hitMaterial;
        yield return new WaitForSeconds(hitDuration);
        sr.sharedMaterial = sr.material = baseMaterial;
    }
    public void Hit_Wrapper(float Change) 
    {
        StopCoroutine(Hit());
        StartCoroutine(Hit());
    }
}
