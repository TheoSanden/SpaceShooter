using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player 
{

public class Engine : MonoBehaviour
{
    Vector3 VelocityLastFrame;
    Vector2 velocity;

    public Vector3 Velocity
    {
        get => velocity * Time.deltaTime;
    }

    [SerializeField]
    float MaxVelocityMagnitude = 20;
    [SerializeField]
    float MinVelocityMagnitude = 0.2f;
    [SerializeField]
    float AccelerationInSeconds = 1;
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

        Vector2 AddedVelocity = new Vector2(Input.GetAxisRaw("Horizontal") * (Time.deltaTime/AccelerationInSeconds) * MaxVelocityMagnitude, Input.GetAxisRaw("Vertical") * (Time.deltaTime / AccelerationInSeconds) * MaxVelocityMagnitude);
        AddedVelocity *= GetReverseMultiplier(); 
        if(AddedVelocity != Vector2.zero) 
        {
            velocity = ((velocity + AddedVelocity).magnitude > MaxVelocityMagnitude) ? (velocity + AddedVelocity).normalized * MaxVelocityMagnitude : velocity + AddedVelocity;
        }
        else if (AddedVelocity == Vector2.zero && velocity != Vector2.zero)
        {
            float DecelerationAmount = (Time.deltaTime / DecelerationInSeconds) * MaxVelocityMagnitude;
            velocity = ((velocity - (velocity.normalized * DecelerationAmount)).magnitude < MinVelocityMagnitude) ? Vector2.zero : velocity - velocity.normalized * (DecelerationAmount);
        }
    }
}
}
