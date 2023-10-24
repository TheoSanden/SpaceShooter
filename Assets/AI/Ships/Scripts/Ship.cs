using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    [RequireComponent(typeof(Brain),typeof(Health),typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Ship : MonoBehaviour
    {
        protected Health health;
        protected Brain brain;
        protected Rigidbody2D rigidBody;
        protected SpriteRenderer spriteRenderer;
        protected virtual void Awake()
        {
            health = GetComponent<Health>();
            brain = this.GetComponent<Brain>();
            rigidBody = this.GetComponent<Rigidbody2D>();
            spriteRenderer = this.GetComponent<SpriteRenderer>();
        }
        public virtual void OnPop() 
        {
            rigidBody.simulated = true;
            spriteRenderer.enabled = true;
            brain.enabled = true;
            health.Reset();
        }
        public virtual void OnQueue() 
        {
            rigidBody.simulated = false;
            spriteRenderer.enabled = false;
            brain.enabled = false;
        }

    }
}

