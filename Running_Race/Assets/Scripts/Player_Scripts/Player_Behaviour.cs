using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Behaviour : MonoBehaviour
{

    [Header("Camera")]
    public Camera cam;

    [Header("Movement Speeds")]
    [Tooltip("Average speed player will be moving at when not sprinting.")]
    public float walk_Speed;
    [Tooltip("Speed player will be moving at when sprinting forward")]
    public float run_Speed;

    [Header("Movement Affectors")]
    [Tooltip("how fast the player will fall towards the ground")]
    public float gravity;
    [HideInInspector]
    public float horizontal_Comp, vertical_Comp;
    [Tooltip("How high player can jump off ground vertically")]
    public float jump_Height, m_Scale_Wall_Timer_Max = 1f, wall_Scale_Padding_Time = 0.1f;
    private float wall_Scale_Padding_Time_Max;

    private Rigidbody rb;

    private Vector3 vel, jumpingVel;
    private float speed = 1f;
    private float input_Y, input_X, sensitivity = 1.0f, jump_timer, jump_Timer_Max = 0.2f;
    private float m_Scale_Wall_Timer, antiBumpFactor = 0.75f;
    private bool m_can_Sprint = false, isGrounded, can_Do_Advanced_Movement = false,
        has_Done_Extended_Jump = false, has_Done_Wall_Climb = false, wall_Climb_Padding_Done = false,
        m_Running_Along_Right_Wall = false, m_Can_Wall_Run = false, can_Move_Forward = true;
    [HideInInspector]
    public bool m_Is_Sprinting = false, is_Scaling_Wall = false, is_Walking = false, is_Wall_Running = false, m_Wall_Run_Cooldown = false;
    private bool limitDiagonalSpeed = true;

    //variables for Sphere cast
    [Header("SphereCast Variables")]
    [Tooltip("Radius of the sphere to be created")]
    public float sphere_Radius;
    public float sphere_Dist, grounded_Sphere_Radius, grounded_Sphere_Dist;

    //variables for raycasts
    [Header("Raycast variables")]
    [Tooltip("How far the raycast will shoot out.")]
    public float ray_Dist;
    [Tooltip("Variable to be added to offset ray direction")]
    public float side_Ray_Offset = 2f;
    public LayerMask layerMask;
    private Vector3 m_Right_Side_Ray_Dir, m_Left_Side_Ray_Dir, wall_Normal, Movement_Sphere_Pos, respawn_Pos;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = transform.Find("Main Camera").GetComponent<Camera>();
        respawn_Pos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Movement()
    {

    }
}
