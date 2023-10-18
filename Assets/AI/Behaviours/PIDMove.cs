using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI 
{
    [CreateAssetMenu(fileName = "MoveBehaviour", menuName = "AI/Behaviours/PIDMove")]
    public class PIDMove : AIBehaviour
    {
        [SerializeField]
        float TimeBetweenPickingTarget = 2;
        float TimeBetweenPickingTargetTimer = 0;

        [SerializeField]
        float MovementTime = 1;
        float MovementTimer = 0;

        bool Initialized = false;

        PIDController PIDC;
        Engine Engine;
        Vector2 MovementBounds;
        Vector2 CameraCenter;


        Vector2 StartPosition;
        Vector2 Path;
        public override BehaviourState Process(Brain brain)
        {
            if (!Initialized) 
            {
                return BehaviourState.Processing;
            }

            if(TimeBetweenPickingTargetTimer == 0) 
            {
                TimeBetweenPickingTargetTimer = TimeBetweenPickingTarget;
                MovementTimer = 0;

                StartPosition = brain.transform.position;
                Path = CameraCenter + new Vector2(Random.Range(-MovementBounds.x, MovementBounds.x), Random.Range(-MovementBounds.y, MovementBounds.y)) - StartPosition;
            }
            else 
            {
                TimeBetweenPickingTargetTimer = (TimeBetweenPickingTargetTimer - Time.deltaTime < 0)? 0: TimeBetweenPickingTargetTimer - Time.deltaTime;
            }

            if(MovementTimer == MovementTime) 
            {
              
            }
            else 
            {
                MovementTimer = (MovementTimer + Time.deltaTime >= MovementTime) ? MovementTime : MovementTimer + Time.deltaTime;
            }

            Vector2 Target = StartPosition + Path * (MovementTimer / MovementTime);

            if (!PIDC)
            {
                PIDC = brain.GetComponent<PIDController>();
            }
            Vector3 force = PIDC.GetForce(brain.transform.position, Target);

            Engine.AddForce(force);

            brain.gameObject.transform.up = Engine.Velocity;
            return BehaviourState.Processing;
        }
        public override void Initalize(Brain brain)
        {
            CameraCenter = Camera.main.transform.position;
            float yBound = Camera.main.orthographicSize;
            float xBound = yBound * (float)(16.0 / 9.0);
            MovementBounds = new Vector2(xBound,yBound);

            if (!brain.gameObject.TryGetComponent<Engine>(out Engine) && !brain.TryGetComponent<PIDController>(out PIDC)) 
            {
                Debug.Log("Tried To initalized PIDMove behaviour, but gameobject is lacking Engine or PIDController Component");
                return;
            }
            Initialized = true;
        }

        public override void OnBehaviourEnd(Brain brain)
        {
          
        }
        public override void OnBehaviourStart(Brain brain)
        {
           
        }
    }
}

