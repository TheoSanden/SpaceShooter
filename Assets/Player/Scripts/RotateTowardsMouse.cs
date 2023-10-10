using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsMouse : MonoBehaviour
{
    [SerializeField]
    float rotationSpeed = 0.2f;
    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.up = GetDirectionToMouse();
    }
    Vector2 GetDirectionToMouse()
    {
        Vector2 MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 DirectionToMouse = (MousePosition - (Vector2)this.transform.position).normalized;
        return DirectionToMouse;
    }
}
