using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    [RequireComponent(typeof(Brain),typeof(Health))]
    public abstract class Ship : MonoBehaviour
    {
        protected Health health;
        protected Brain brain;
        protected virtual void Awake()
        {
            health = GetComponent<Health>();
            brain = this.GetComponent<Brain>();
        }
        protected abstract void OnPop();
        protected abstract void OnQueue();

    }
}

