using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI 
{
    public class Engine : MonoBehaviour
    {
        float MagnitudeZeroCutoff = 0.001f;
        Vector2 velocity;
        public Vector2 Velocity
        { get => velocity; }

        [SerializeField]
        float Friction;
        // Update is called once per frame
        void Update()
        {
            if(velocity == Vector2.zero) { return; }

            transform.position += (Vector3)velocity * Time.deltaTime;

            velocity -= velocity.normalized * Time.deltaTime * Friction;

            if(velocity.magnitude < MagnitudeZeroCutoff) 
            {
                velocity = Vector2.zero;
            }
        }
        public void AddForce(Vector2 force)
        {
            velocity += force;
        }
    }

}