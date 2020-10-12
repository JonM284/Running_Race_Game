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

    public float additive_X, additive_Y;

    private float minimum_X = -360f;
    private float maximum_X = 360f;

    private float minimum_Y = -89f;
    private float maximum_Y = 89f;

    public float[] original_Rotation_Min_Max = new float[2];

    private Quaternion originalRotation, current_Norm_Rotation;

    private float mouseSensivity = 1.7f;

    public bool can_Follow_Input = true;

    public float tilt_Speed = 0.1f, current_Tilt_Speed;

    private bool camera_Is_Tilted = false;
    private bool lean_Camera, camera_Lean_Right;


    // Use this for initialization
    void Start () {
        originalRotation = transform.rotation;
        current_Norm_Rotation = originalRotation;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        original_Rotation_Min_Max[0] = maximum_X;
        original_Rotation_Min_Max[1] = maximum_Y;
        
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

            rotation_X = ClampAngle(rotation_X, minimum_X + additive_X, maximum_X + additive_X);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotation_X, Vector3.up);
            

            transform.localRotation = current_Norm_Rotation * xQuaternion;
        }

        if (axes == RotationAxes.MouseY && !camera_Is_Tilted)
        {
            rotation_Y += Input.GetAxis("Mouse Y") * sensivity_Y;

            rotation_Y = ClampAngle(rotation_Y, minimum_Y, maximum_Y);
            Quaternion yQuaternion = Quaternion.AngleAxis(-rotation_Y, Vector3.right);

            transform.localRotation = current_Norm_Rotation * yQuaternion;
        }

        
            
        
    }

    public void Limit_Vision_Movement_Range(Transform _hiding_Object, float _limit_X, float _limit_Y, bool _Hiding)
    {
        minimum_X = -_limit_X;
        maximum_X = _limit_X;
        minimum_Y = -_limit_Y;
        maximum_Y = _limit_Y;
        //current_Norm_Rotation = _hiding_Object.rotation;
        additive_X = _Hiding ? _hiding_Object.rotation.eulerAngles.y : 0;
       //additive_X = _hiding_Object.rotation.eulerAngles.y;
        
    }

    public void Reset_Vision_Movement_Range()
    {
        minimum_X = -original_Rotation_Min_Max[0];
        maximum_X = original_Rotation_Min_Max[0];
        minimum_Y = -original_Rotation_Min_Max[1];
        maximum_Y = original_Rotation_Min_Max[1];
        //current_Norm_Rotation = originalRotation;
        additive_X = 0;
    }

    

    public void Reset_Timer()
    {
        current_Tilt_Speed = 0;
    }


}
