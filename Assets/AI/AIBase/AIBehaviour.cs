using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{

    public abstract class AIBehaviour : ScriptableObject
    {
        public enum BehaviourState
        {
            Processing,
            Finished
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="brain"> Reference to the brain of the object</param>
        /// <returns>Returns the current state of the process</returns>
        public abstract BehaviourState Process(Brain brain);
        public abstract void Initalize(Brain brain);
        public abstract void OnBehaviourStart(Brain brain);
        public abstract void OnBehaviourEnd(Brain brain);
    }
}
