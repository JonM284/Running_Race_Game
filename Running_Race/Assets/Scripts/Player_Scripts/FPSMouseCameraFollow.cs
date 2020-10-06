using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSMouseCameraFollow : MonoBehaviour {

    public enum RotationAxes { MouseX, MouseY}

    public RotationAxes axes = RotationAxes.MouseY;

    private float currentSensivity_X = 1.5f;
    private float currentSensivity_Y = 1.5f;

    private float sensivity_X = 1.5f;
    private float sensivity_Y = 1.5f;

    [HideInInspector]
    public float rotation_X, rotation_Y;

    private float minimum_X = -360f;
    private float maximum_X = 360f;

    private float minimum_Y = -89f;
    private float maximum_Y = 89f;

    private Quaternion originalRotation;

    private float mouseSensivity = 1.7f;

    public bool can_Follow_Input = true;

    public float tilt_Speed = 0.1f, current_Tilt_Speed;

    private bool camera_Is_Tilted = false;
    private bool lean_Camera, camera_Lean_Right;


    // Use this for initialization
    void Start () {
        originalRotation = transform.rotation;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }


    

    void LateUpdate()
    {
        if (can_Follow_Input) {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            HandleRotation();
        }
        if (!can_Follow_Input)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
        {
            angle += 360f;
        }
        if (angle > 360f)
        {
            angle -= 360f;
        }

        return Mathf.Clamp(angle, min, max);
    }

    void HandleRotation()
    {
        if (currentSensivity_X != mouseSensivity || currentSensivity_Y != mouseSensivity)
        {
            currentSensivity_X = currentSensivity_Y = mouseSensivity;
        }

        sensivity_X = currentSensivity_X;
        sensivity_Y = currentSensivity_Y;

        if (axes == RotationAxes.MouseX)
        {
            rotation_X += Input.GetAxis("Mouse X") * sensivity_X;

            rotation_X = ClampAngle(rotation_X, minimum_X, maximum_X);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotation_X, Vector3.up);
            

            transform.localRotation = originalRotation * xQuaternion;
        }

        if (axes == RotationAxes.MouseY && !camera_Is_Tilted)
        {
            rotation_Y += Input.GetAxis("Mouse Y") * sensivity_Y;

            rotation_Y = ClampAngle(rotation_Y, minimum_Y, maximum_Y);
            Quaternion yQuaternion = Quaternion.AngleAxis(-rotation_Y, Vector3.right);

            transform.localRotation = originalRotation * yQuaternion;
        }

        
            
        
    }

    

    public void Reset_Timer()
    {
        current_Tilt_Speed = 0;
    }


}
