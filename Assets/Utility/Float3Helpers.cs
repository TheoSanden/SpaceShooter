using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Float3Helpers
{
    public static float3 Cross(float3 a, float3 b) 
    {
        float cx = a.y * b.z - a.z * b.y;
        float cy = a.z * b.x - a.x * b.z;
        float cz = a.x * b.y - a.y * b.x;
        return new float3(cx,cy,cz);
    }
    public static float Magnitude(float3 a) 
    {
        return Mathf.Sqrt(Mathf.Pow(a.x,2)+ Mathf.Pow(a.y, 2)+ Mathf.Pow(a.z, 2));
    }
    public static float3 Subtract(float3 a, float3 b) 
    {
        return new float3(a.x-b.x,a.y-b.y,a.z-b.z);
    }
    public static float3 Add(float3 a, float3 b)
    {
        return new float3(a.x + b.x, a.y + b.y, a.z + b.z);
    }
}
