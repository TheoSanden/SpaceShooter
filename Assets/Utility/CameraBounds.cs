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
    void Start()
    {
        Bounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height, Camera.main.transform.position.z));
        print(Bounds);
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
