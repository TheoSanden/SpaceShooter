using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI 
{
    
    public class Ship_Basic : Ship
    {
        [SerializeField]
        float MovementSpeed = 1;
        // Start is called before the first frame update
        protected virtual void Start()
        {
            brain.Blackboard.SetValueAsFloat("MovementSpeed", MovementSpeed);
            health.OnHealthZero += OnHealthZero;
        }
        // Update is called once per frame
        void Update()
        {

        }
        public override void OnPop()
        {
            base.OnPop();   
        }
        public override void OnQueue()
        {
            base.OnQueue();
        }
        protected virtual void OnHealthZero() 
        {
            brain.DestroyHost();    
        }
        public void SetInitialPosition(Vector2 position) 
        {
            brain.Blackboard.SetValueAsVector("InitialPosition",position);
        }
    }
}
