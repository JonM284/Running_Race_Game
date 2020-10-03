using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;


//[RequireComponent()]
public class WalkingBeahaviour : MonoBehaviour {

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
    private CharacterController char_Controller;
    private Vector3 vel, jumpingVel;
    private float speed = 1f;
    private float input_Y, input_X, sensitivity = 1.0f, jump_timer, jump_Timer_Max = 0.2f;
    private float m_Scale_Wall_Timer, antiBumpFactor = 0.75f;
    private bool  m_can_Sprint = false, isGrounded, can_Do_Advanced_Movement = false, 
        has_Done_Extended_Jump = false, has_Done_Wall_Climb = false, wall_Climb_Padding_Done = false,
        m_Running_Along_Right_Wall = false, m_Can_Wall_Run = false, can_Move_Forward = true;
    [HideInInspector]
    public bool m_Is_Sprinting = false, is_Scaling_Wall = false, is_Walking = false, is_Wall_Running = false, m_Wall_Run_Cooldown = false;
    private bool limitDiagonalSpeed = true;
    
    //rewired stuff
    
    
    [Header("Rewired")]
    [Tooltip("Identification number of this player, to be used to find specified inputs.")]
    public int playerID;
    private Player myPlayer;

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

    private Shooting_Behaviour shoot_Behave;

    public enum Player_Weight
    {
        LIGHT,
        MEDIUM,
        HEAVY
    };
    public Player_Weight weight;
    
	// Use this for initialization
	void Start () {
        //rb = GetComponent<Rigidbody>();
        char_Controller = GetComponent<CharacterController>();
        myPlayer = ReInput.players.GetPlayer(playerID);
        wall_Scale_Padding_Time_Max = wall_Scale_Padding_Time;
        Movement_Sphere_Pos = new Vector3(transform.position.x, transform.position.y - 0.7f, transform.position.z);
        respawn_Pos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
        shoot_Behave = GetComponent<Shooting_Behaviour>();
    }
	
	// Update is called once per frame
	void Update () {

        


        if (Input.GetKey(KeyCode.W))
        {
            if ((is_Scaling_Wall || !can_Move_Forward)) {
                vertical_Comp = 0;
            }else 
            {
                vertical_Comp = 1;
            }
            if (isGrounded) {
                m_can_Sprint = true;
            }
        }else if (Input.GetKey(KeyCode.S))
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
        }else if (Input.GetKey(KeyCode.A))
        {
            horizontal_Comp = -1;
        }else
        {
            horizontal_Comp = 0;
        }

        if (Input.GetMouseButton(0))
        {
            m_can_Sprint = false;
        }
        if (Input.GetMouseButtonUp(0))
        {
            m_can_Sprint = true;
        }

        

        if (isGrounded) {
            if (myPlayer.GetButton("Sprint") && m_can_Sprint && Input.GetKey(KeyCode.W) 
                && !shoot_Behave.Is_Meleeing)
            {
                m_Is_Sprinting = true;
            }
        }
        if (myPlayer.GetButtonUp("Sprint") || Input.GetKeyUp(KeyCode.W) || Input.GetMouseButton(0) 
            || shoot_Behave.Is_Meleeing)
        {
            m_Is_Sprinting = false;
        }


        if ((Input.GetKey(KeyCode.W)|| Input.GetKey(KeyCode.S)|| Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) &&
            !m_Is_Sprinting && !is_Scaling_Wall)
        {
            is_Walking = true;
        }else
        {
            is_Walking = false;
        }
        
        speed = m_Is_Sprinting && m_can_Sprint ? run_Speed : walk_Speed;



        


        
        

        Movement();
        


    }

    /// <summary>
    /// moves the position of the player in the desired direction.
    /// </summary>
    void Movement()
    {
        input_Y = Mathf.Lerp(input_Y, vertical_Comp, Time.deltaTime * 19f);
        input_X = Mathf.Lerp(input_X, horizontal_Comp, Time.deltaTime * 19f);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Wave_Dash();
        }

        sensitivity = Mathf.Lerp(sensitivity,
            (input_Y != 0 && input_X != 0 && limitDiagonalSpeed) ? 0.75f : 1.0f, Time.deltaTime * 19f);

       

        if (isGrounded)
        {
            vel = new Vector3(input_X * sensitivity, -antiBumpFactor, input_Y * sensitivity);
            
            vel = transform.TransformDirection(vel) * speed;
            if (can_Do_Advanced_Movement) can_Do_Advanced_Movement = false;
            if (has_Done_Extended_Jump) has_Done_Extended_Jump = false;
            if (jump_timer > 0) jump_timer = 0;
            if (has_Done_Wall_Climb) has_Done_Wall_Climb = false;
            if (m_Scale_Wall_Timer > 0) m_Scale_Wall_Timer = 0;
            if (Input.GetKeyDown(KeyCode.Space))Jump();
            if (wall_Scale_Padding_Time < wall_Scale_Padding_Time_Max) wall_Scale_Padding_Time = wall_Scale_Padding_Time_Max;
            if (wall_Climb_Padding_Done) wall_Climb_Padding_Done = false;
        }


        if (!isGrounded) {
            jump_timer += Time.deltaTime;
            
            if (jump_timer > jump_Timer_Max && !has_Done_Wall_Climb) can_Do_Advanced_Movement = true;


            if (weight == Player_Weight.LIGHT || weight == Player_Weight.MEDIUM) {
                if (can_Do_Advanced_Movement && !has_Done_Wall_Climb && Obstacle_In_Front() && Input.GetKey(KeyCode.Space)
                    && Input.GetKey(KeyCode.W) && !is_Wall_Running && !Obstacle_To_Left_Side() && !Obstacle_To_Right_Side())
                {
                    is_Scaling_Wall = true;
                    Debug.Log("Cimbing wall");
                    Scale_Obstacle();
                }

                if (is_Scaling_Wall && !Obstacle_In_Front() && !has_Done_Extended_Jump || is_Scaling_Wall && Input.GetKeyUp(KeyCode.Space) && !has_Done_Extended_Jump
                    || is_Scaling_Wall && m_Scale_Wall_Timer > m_Scale_Wall_Timer_Max || is_Scaling_Wall && Input.GetKeyUp(KeyCode.W) && !has_Done_Extended_Jump)
                {
                    is_Scaling_Wall = false;
                    has_Done_Wall_Climb = true;
                    has_Done_Extended_Jump = true;
                    Debug.Log("Jumping off wall");
                    StartCoroutine(Forward_Input_Padding());
                    WallJump();
                }
            }

            if (weight == Player_Weight.LIGHT) {
                if (can_Do_Advanced_Movement && !is_Scaling_Wall && (Obstacle_To_Left_Side() || Obstacle_To_Right_Side()) &&
                    !m_Wall_Run_Cooldown)
                {
                    m_Can_Wall_Run = true;
                }

                if (can_Do_Advanced_Movement && (Obstacle_To_Left_Side() || Obstacle_To_Right_Side())
                    && Input.GetKey(KeyCode.Space) && !is_Wall_Running && m_Can_Wall_Run)
                {
                    is_Wall_Running = true;
                    Debug.Log("Should run along wall");
                    Run_Along_Wall();
                }

                if (((!Obstacle_To_Right_Side() && !Obstacle_To_Left_Side()) || Input.GetKeyUp(KeyCode.Space) || Obstacle_In_Front()) && is_Wall_Running)
                {
                    StartCoroutine(Off_Wall_Run_Padding());
                    WallJump();
                    GetComponent<FPSMouseCameraFollow>().Reset_Timer();
                    Jump_Away_From_Wall(m_Running_Along_Right_Wall);
                    is_Wall_Running = false;

                }
            }


            

            /*if ((is_Scaling_Wall) || (!is_Scaling_Wall && !wall_Climb_Padding_Done && can_Do_Advanced_Movement && has_Done_Extended_Jump))
            {
                vertical_Comp = 0;
            }*/

            if (!wall_Climb_Padding_Done && wall_Scale_Padding_Time > 0 && has_Done_Extended_Jump)
            {
                wall_Scale_Padding_Time -= Time.deltaTime;
            }

            if (wall_Scale_Padding_Time <= 0)
            {
                wall_Climb_Padding_Done = true;
            }
        }

        if(Obstacle_To_Right_Side()){
            Debug.Log("Right");
        }
        if(Obstacle_To_Left_Side()){
            Debug.Log("Left");
        }
        if (Obstacle_In_Front())
        {
            Debug.Log("In Front");
        }


        if (!is_Scaling_Wall && !is_Wall_Running) {
            vel.y -= gravity * Time.deltaTime;
        }

        if (!isGrounded && !is_Scaling_Wall && !is_Wall_Running)
        {
            
            jumpingVel = new Vector3(input_X * sensitivity, 0, input_Y * sensitivity);
            jumpingVel = transform.TransformDirection(jumpingVel) * speed;
            jumpingVel = new Vector3((jumpingVel.x - vel.x), 0, (jumpingVel.z - vel.z));
            char_Controller.Move(Vector3.ClampMagnitude(jumpingVel, speed) * Time.deltaTime);
            
        }

        isGrounded = (char_Controller.Move(vel * Time.deltaTime) & CollisionFlags.Below) != 0;
        
        //rb.MovePosition(transform.position + Vector3.ClampMagnitude(vel, speed) * Time.deltaTime);
    }

    void WallJump()
    {
        float wall_Jump_Height = jump_Height * 1.45f;
        vel.y = wall_Jump_Height;
        wall_Climb_Padding_Done = false;
    }

    

    void Jump()
    {
        vel.y = jump_Height;
    }

    /// <summary>
    /// Applies an upward force along the wall in order to reach a higher platform.
    /// </summary>
    void Scale_Obstacle()
    {
        vel.z = 0;
        jumpingVel.z = 0;
        m_Scale_Wall_Timer += Time.deltaTime;
        if (m_Scale_Wall_Timer < m_Scale_Wall_Timer_Max) vel.y += jump_Height * Time.deltaTime;
        if (m_Scale_Wall_Timer > m_Scale_Wall_Timer_Max && !has_Done_Extended_Jump )
        {
            has_Done_Extended_Jump = true;
            vel.z = 0;
            WallJump();
        }

        //check to see if the wall can be climbed if the player performs a ledge jump
        
    }

    void Run_Along_Wall()
    {
        
        if (Obstacle_To_Right_Side())
        {
            m_Running_Along_Right_Wall = true;
            transform.right = -wall_Normal;
        }else if (Obstacle_To_Left_Side())
        {
            m_Running_Along_Right_Wall = false;
            transform.right = wall_Normal;
        }

        vel = transform.forward * run_Speed;
        

        
    }


    void Jump_Away_From_Wall(bool right_Side)
    {
        vel.x = wall_Normal.x * run_Speed;
        vel.z = wall_Normal.z * run_Speed;
        //if (right_Side) vel = new Vector3(-wall_Normal.x, jump_Height, transform.forward.z);
        //if (!right_Side) vel = new Vector3(wall_Normal.x, jump_Height, transform.forward.z);
    }

    /// <summary>
    /// This will check to see if there is a wall directly in front of the player.
    /// </summary>
    /// <returns></returns>
    bool Obstacle_In_Front()
    {
        RaycastHit info;
        Vector3 rayPos = new Vector3(transform.position.x, transform.position.y - 0.7f, transform.position.z);
        if (Physics.SphereCast(rayPos, sphere_Radius, transform.forward, out info, ray_Dist, layerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(rayPos, info.point, Color.blue, 0.5f);
            if (info.transform.tag != "Player")
            {
                
                return true;
            }
        }

        Vector3 ray_Two_Pos = new Vector3(transform.position.x, transform.position.y - 0.7f, transform.position.z);
        if (Physics.SphereCast(ray_Two_Pos, sphere_Radius/2f, 
            transform.forward + transform.TransformDirection(new Vector3(side_Ray_Offset, transform.forward.y)), out info, ray_Dist, layerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(ray_Two_Pos, info.point, Color.red, 0.5f);
            if (info.transform.tag != "Player")
            {

                return true;
            }
        }

        Vector3 ray_Three_Pos = new Vector3(transform.position.x, transform.position.y - 0.7f, transform.position.z);
        if (Physics.SphereCast(ray_Three_Pos, sphere_Radius/2f,
            transform.forward - transform.TransformDirection(new Vector3(side_Ray_Offset, transform.forward.y)), out info, ray_Dist, layerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(ray_Three_Pos, info.point, Color.magenta, 0.5f);
            if (info.transform.tag != "Player")
            {

                return true;
            }
        }

        

        return false;
    }



    public bool Obstacle_To_Right_Side()
    {
        RaycastHit rightInfo;

        //m_Right_Side_Ray_Dir = cam.transform.forward;   
        //m_Right_Side_Ray_Dir = cam.transform.forward + new Vector3(side_Ray_Offset, 0,0);

        if (Physics.SphereCast(transform.position, sphere_Radius, transform.right, out rightInfo, ray_Dist, layerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(transform.position, rightInfo.point, Color.red);
            if (rightInfo.transform.tag != "Player")
            {
                wall_Normal = rightInfo.normal;
                return true;
            }
        }


        return false;
    }

    public bool Obstacle_To_Left_Side()
    {
        

        RaycastHit leftInfo;


        //m_Left_Side_Ray_Dir = cam.transform.forward;
        //m_Left_Side_Ray_Dir.x -= side_Ray_Offset;
        if (Physics.SphereCast(transform.position, sphere_Radius, -transform.right, out leftInfo, ray_Dist, layerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(transform.position, leftInfo.point, Color.magenta);
            if (leftInfo.transform.tag != "Player")
            {
                wall_Normal = leftInfo.normal;
                return true;
            }
        }


        return false;
    }

    

    IEnumerator Off_Wall_Run_Padding()
    {
        m_Wall_Run_Cooldown = true;
        m_Can_Wall_Run = false;
        yield return new WaitForSeconds(0.5f);
        m_Wall_Run_Cooldown = false;
        m_Can_Wall_Run = true;
    }

    IEnumerator Forward_Input_Padding()
    {
        can_Move_Forward = false;
        yield return new WaitForSeconds(wall_Scale_Padding_Time);
        can_Move_Forward = true;
        StopCoroutine(Forward_Input_Padding());
    }

    /*bool is_Grounded()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, grounded_Sphere_Radius, -transform.up, out hit, grounded_Sphere_Dist, layerMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(cam.transform.position, hit.point, Color.magenta);
            if (hit.transform.tag != "Player")
            {

                return true;
            }
        }

        return false;
    }*/


    /// <summary>
    /// MOVEMENT ABILITIES PAST THIS POINT
    /// </summary>
    /// 

    public void Wave_Dash()
    {
        Vector3 slideDir = cam.transform.forward;
        char_Controller.Move(slideDir);
        vel = slideDir * speed;
        //vel.z = cam.transform.forward.z + jump_Height;
    }

    public void Change_Current_Weight(int new_Weight)
    {
        switch (new_Weight)
        {
            case 2:
                weight = Player_Weight.HEAVY;
                break;
            case 1:
                weight = Player_Weight.MEDIUM;
                break;
            default:
                weight = Player_Weight.LIGHT;
                break;

        }

        Debug.Log(weight);
    }

    private void OnDrawGizmos()
    {
        Vector3 forward_Dir = transform.forward;
            

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - 0.7f, transform.position.z) + forward_Dir + transform.TransformDirection(new Vector3(side_Ray_Offset,transform.forward.y)) * sphere_Dist, sphere_Radius/2f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - 0.7f, transform.position.z) + forward_Dir - transform.TransformDirection(new Vector3(side_Ray_Offset, transform.forward.y)) * sphere_Dist, sphere_Radius/2f);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(new Vector3(cam.transform.position.x,
                    cam.transform.position.y + grounded_Sphere_Dist, cam.transform.position.z + 1f), grounded_Sphere_Radius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - 0.7f, transform.position.z) + transform.forward * ray_Dist, sphere_Radius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.right * ray_Dist, sphere_Radius);
        Gizmos.DrawWireSphere(transform.position - transform.right * ray_Dist, sphere_Radius);

    }
    
}
