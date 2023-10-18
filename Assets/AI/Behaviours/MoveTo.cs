using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AI
{

    public enum MovementType
    {
        Spherical,
        Linear
    }
    [CreateAssetMenu(fileName = "MoveToBehaviour", menuName = "AI/Behaviours/MoveTo")]
    public class MoveTo : AIBehaviour
    {
        [SerializeField]
        MovementType MovementType;
        [SerializeField]
        string TargetKeyName;
        [SerializeField]
        string SpeedKeyName;

        Vector2 Start;
        Vector2 Target;
        float Speed;

        float LerpTime;
        float LerpTimer;
        public override void Initalize(Brain brain)
        {

        }
        public override BehaviourState Process(Brain brain)
        {
            LerpTimer += Time.deltaTime;

            switch(MovementType) 
            {
                case MovementType.Linear:
                    brain.transform.position = Vector3.Lerp(Start,Target,LerpTimer/LerpTime);
                    break;
                case MovementType.Spherical:
                    brain.transform.position = Vector3.Slerp(Start, Target, LerpTimer / LerpTime);
                    break;
            }

            if (LerpTimer >= LerpTime) 
            {
                return BehaviourState.Finished;
            }
            
            return BehaviourState.Processing;
        }
        public override void OnBehaviourEnd(Brain brain)
        {
           
        }
        public override void OnBehaviourStart(Brain brain)
        {
            LerpTimer = 0;
            Target = brain.Blackboard.GetValueAsVector(TargetKeyName);
            Start = brain.gameObject.transform.position;
            float EstimatedPathMagnitude = (Target - (Vector2)brain.gameObject.transform.position).magnitude;
            float UnScaledSpeed = brain.Blackboard.GetValueAsFloat(SpeedKeyName);
            LerpTime = EstimatedPathMagnitude/UnScaledSpeed;
        }
    }
}