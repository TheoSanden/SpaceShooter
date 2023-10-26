using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    [RequireComponent(typeof(Brain),typeof(Health),typeof(SpriteRenderer))]
    [RequireComponent(typeof(Theo.Collider))]
    public abstract class Ship : MonoBehaviour
    {
        protected Health health;
        protected Brain brain;
        protected SpriteRenderer spriteRenderer;
        protected Theo.Collider col;
        protected virtual void Awake()
        {
            health = GetComponent<Health>();
            brain = this.GetComponent<Brain>();
            spriteRenderer = this.GetComponent<SpriteRenderer>();
            col = this.GetComponent<Theo.Collider>();
        }
        public virtual void OnPop() 
        {
            spriteRenderer.enabled = true;
            brain.enabled = true;
            col.enabled = true;
            health.Reset();
        }
        public virtual void OnQueue() 
        {
            spriteRenderer.enabled = false;
            brain.enabled = false;
            col.enabled = false;
        }

    }
}

