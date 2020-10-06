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
    public float jump_Height;

    private CharacterController char_Controller;

    [SerializeField]
    private Vector3 vel, jumpingVel;
    private float speed = 1f;
    private float input_Y, input_X, sensitivity = 1.0f;
    [SerializeField]
    private float antiBumpFactor = 0.75f;
    private bool m_can_Sprint = false, isGrounded;
    [HideInInspector]
    public bool m_Is_Sprinting = false;
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
        char_Controller = GetComponent<CharacterController>();
        cam = transform.Find("Main Camera").GetComponent<Camera>();
        respawn_Pos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        Check_Inputs();
        Movement();
    }

    void Check_Inputs()
    {
        if (Input.GetKey(KeyCode.W))
        {
            vertical_Comp = 1;
            if (isGrounded)
            {
                m_can_Sprint = true;
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            vertical_Comp = -1;
            m_can_Sprint = false;
        }
        else
        {
            vertical_Comp = 0;
            m_can_Sprint = false;
        }

        if (Input.GetKey(KeyCode.D))
        {
            horizontal_Comp = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            horizontal_Comp = -1;
        }
        else
        {
            horizontal_Comp = 0;
        }


        if (Input.GetKeyDown(KeyCode.LeftShift) && m_can_Sprint && Input.GetKey(KeyCode.W))
        {
            m_Is_Sprinting = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.W) || Input.GetMouseButton(0))
        {
            m_Is_Sprinting = false;
        }

        if (Can_Jump()) {
            if (Input.GetKeyDown(KeyCode.Space)) Jump();
        }

        Debug.Log($"Can Jump: {Can_Jump()}");
    }

    void Movement()
    {
        input_Y = Mathf.Lerp(input_Y, vertical_Comp, Time.deltaTime * 19f);
        input_X = Mathf.Lerp(input_X, horizontal_Comp, Time.deltaTime * 19f);

        sensitivity = Mathf.Lerp(sensitivity,
            (input_Y != 0 && input_X != 0 && limitDiagonalSpeed) ? 0.75f : 1.0f, Time.deltaTime * 19f);

        if (isGrounded)
        {
            vel = new Vector3(input_X * sensitivity, -antiBumpFactor, input_Y * sensitivity);
            vel = transform.TransformDirection(vel) * speed;
        }

        vel.y -= gravity * Time.deltaTime;
        
        

        if (!isGrounded)
        {

            jumpingVel = new Vector3(input_X * sensitivity, 0, input_Y * sensitivity);
            jumpingVel = transform.TransformDirection(jumpingVel) * speed;
            jumpingVel = new Vector3((jumpingVel.x - vel.x), 0, (jumpingVel.z - vel.z));
            char_Controller.Move(Vector3.ClampMagnitude(jumpingVel, speed) * Time.deltaTime);

        }


        speed = m_Is_Sprinting && m_can_Sprint ? run_Speed : walk_Speed;

        isGrounded = (char_Controller.Move(vel * Time.deltaTime) & CollisionFlags.Below) != 0;
        
    }

    void Jump()
    {
        vel.y = jump_Height;
    }

    bool Can_Jump()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, sphere_Radius, Vector3.down, out hit, sphere_Dist))
        {
            return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + (Vector3.down * sphere_Dist), sphere_Radius);
    }

}
