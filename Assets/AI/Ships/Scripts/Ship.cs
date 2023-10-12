using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    [RequireComponent(typeof(Brain))]
    public abstract class Ship : MonoBehaviour
    {
        Brain brain;
        protected virtual void Start()
        {
            brain = this.GetComponent<Brain>();
        }
        protected abstract void OnPop();
        protected abstract void OnQueue();

    }
}

