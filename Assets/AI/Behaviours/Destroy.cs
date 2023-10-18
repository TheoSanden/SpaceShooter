using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    [CreateAssetMenu(fileName = "Destroy", menuName = "AI/Behaviours/Destroy")]
    public class Destroy : AIBehaviour
    {
        public override void Initalize(Brain brain)
        {

        }
        public override void OnBehaviourEnd(Brain brain)
        {
            
        }
        public override void OnBehaviourStart(Brain brain)
        {
            Destroy(brain.gameObject);
        }
        public override BehaviourState Process(Brain brain)
        {
            return BehaviourState.Finished;
        }
    }
}

