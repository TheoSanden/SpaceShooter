using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : MonoBehaviour
{
    Vector3 VelocityLastFrame;
    Vector2 Velocity;
    [SerializeField]
    float MaxVelocityMagnitude = 20;
    [SerializeField]
    float MinVelocityMagnitude = 0.2f;
    [SerializeField]
    float AccelerationInSeonds = 1;
    [SerializeField]
    float DecelerationInSeconds = 2;
    [SerializeField]
    float MaxReverseAccelerationMultiplier = 2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CalculateVelocity();
        Throttle();

        if(Velocity.magnitude > MaxVelocityMagnitude) { Debug.Log("Velocity Higher Than Allowed"); }
    }

    void Throttle()
    {
        if(Velocity.magnitude == 0) { return; }
        transform.position += (Vector3)Velocity;
        //transform.up = Velocity.normalized;
    }
    private float GetReverseMultiplier()
    {
        float reverseMultiplier = Vector2.Dot(Velocity.normalized, new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized);
        reverseMultiplier = 1 + ((1 - reverseMultiplier) / 2) * MaxReverseAccelerationMultiplier;
        return reverseMultiplier;
    }
    void CalculateVelocity()
    {
        VelocityLastFrame = Velocity;

        Vector2 AddedVelocity = new Vector2(Input.GetAxisRaw("Horizontal") * (Time.deltaTime/AccelerationInSeonds) * MaxVelocityMagnitude, Input.GetAxisRaw("Vertical") * (Time.deltaTime / AccelerationInSeonds) * MaxVelocityMagnitude);
        AddedVelocity *= GetReverseMultiplier(); 
        if(AddedVelocity != Vector2.zero) 
        {
            Velocity = ((Velocity + AddedVelocity).magnitude > MaxVelocityMagnitude) ? (Velocity + AddedVelocity).normalized * MaxVelocityMagnitude : Velocity + AddedVelocity;
        }
        else if (AddedVelocity == Vector2.zero && Velocity != Vector2.zero)
        {
            float DecelerationAmount = (Time.deltaTime / DecelerationInSeconds) * MaxVelocityMagnitude;
            Velocity = ((Velocity - (Velocity.normalized * DecelerationAmount)).magnitude < MinVelocityMagnitude) ? Vector2.zero : Velocity - Velocity.normalized * (DecelerationAmount);
        }
    }
}
