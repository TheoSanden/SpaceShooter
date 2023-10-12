using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIDController : MonoBehaviour
{
    [SerializeField, Range(0, 1)]
    public float proportional = 1;
    [SerializeField, Range(0, 1)]
    public float integral = 0;
    [SerializeField, Range(0, 1)]
    public float derivative = 0;

    private Vector3 storedIntegral;
    private Vector3 lastError;

    public void Reset()
    {
        storedIntegral = Vector3.zero;
        lastError = Vector3.zero;
    }
    public Vector3 GetForce(Vector3 current, Vector3 target)
    {
        float X_error = target.x - current.x;
        float Y_error = target.y - current.y;
        float Z_error = target.z - current.z;


        float P_X = X_error * proportional;
        float P_Y = Y_error * proportional;
        float P_Z = Z_error * proportional;

        storedIntegral.x += X_error * Time.deltaTime;
        storedIntegral.y += Y_error * Time.deltaTime;
        storedIntegral.z += Z_error * Time.deltaTime;

        float I_X = storedIntegral.x * integral;
        float I_Y = storedIntegral.y * integral;
        float I_Z = storedIntegral.z * integral;

        float Change_X = (X_error - lastError.x) / Time.deltaTime;
        float Change_Y = (Y_error - lastError.y) / Time.deltaTime;
        float Change_Z = (Z_error - lastError.z) / Time.deltaTime;

        float D_X = (lastError == Vector3.zero) ? 0.0f : Change_X * derivative;
        float D_Y = (lastError == Vector3.zero) ? 0.0f : Change_Y * derivative;
        float D_Z = (lastError == Vector3.zero) ? 0.0f : Change_Z * derivative;

        lastError = new Vector3(X_error, Y_error, Z_error);

        return new Vector3(P_X + I_X + D_X, P_Y + I_Y + D_Y, P_Z + I_Z + D_Z);
    }
}
