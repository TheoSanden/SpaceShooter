using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    private Vector2 Bounds;
    [SerializeField]
    float BoundsHeight;
    [SerializeField]
    float BoundsWidth;

    public static Vector2 GetCameraBounds() 
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
    }
    public static Bounds GetCameraBoundsClass()
    {
        return new Bounds((Vector2)Camera.main.transform.position,GetCameraBounds() * 2);
    }
    public static bool IsWithinBounds(Vector2 position,Vector2 bounds, Vector2 center) 
    {
        return (position.x < (center.x + bounds.x) && position.x > (center.x - bounds.x) && position.y < (center.y + bounds.y) && position.y > (center.y - bounds.y));
    }
    void Start()
    {
        Bounds = GetCameraBounds();
    }

    private void LateUpdate()
    {
        Vector2 viewPos = transform.position;
        viewPos.x = Mathf.Clamp(viewPos.x, -Bounds.x + BoundsHeight * 2, Bounds.x - BoundsHeight * 2);
        viewPos.y = Mathf.Clamp(viewPos.y, -Bounds.y + BoundsWidth * 2, Bounds.y - BoundsWidth * 2);
        transform.position = viewPos;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.matrix = transform.localToWorldMatrix;
        Handles.color = Color.green;
        Vector3[] points = new Vector3[5]{new Vector2(-BoundsWidth,BoundsHeight), new Vector2(BoundsWidth, BoundsHeight), new Vector2(BoundsWidth, -BoundsHeight), new Vector2(-BoundsWidth,-BoundsHeight), new Vector2(-BoundsWidth, BoundsHeight) };
        Handles.DrawAAPolyLine(5,points);
    }
#endif
}
