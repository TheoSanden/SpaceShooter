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
        protected override void OnPop()
        {
            
        }
        protected override void OnQueue()
        {
            
        }
        protected virtual void OnHealthZero() 
        {
            Destroy(this.gameObject);
        }
        public void SetInitialPosition(Vector2 position) 
        {
            brain.Blackboard.SetValueAsVector("InitialPosition",position);
        }
    }
}
