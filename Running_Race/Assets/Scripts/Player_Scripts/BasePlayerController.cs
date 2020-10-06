using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePlayerController : MonoBehaviour {

    protected Transform firstPerson_View;
    protected Transform firstPerson_Camera;

    protected Vector3 firstPerson_View_Rotation = new Vector3 (0,0,0);

    public float walkSpeed = 6.75f;
    public float runSpeed = 10f;
    public float crouchSpeed = 4f;
    public float jumpSpeed = 8f;
    public float gravity = 20f;
    public float lean_Distance;
    public float lean_Speed;

    [Header("Advanced movement values")]
    public float wall_Climb_Timer_Max;
    public float wall_Climb_Constant_Push_Max;
    public float wall_Climb_Jump_Force;

    private float m_Current_Wall_Climb_Timer, m_Climb_Constant_Push_Original;
    private bool m_Can_Do_Advanced_Movements = false , m_Is_Climbing_Wall = false;

    [Header("Advanced movement raycast values")]
    public float front_Wall_SphereCast_Dist;
    public float front_Wall_SphereCast_Rad;

    protected float speed;

    protected bool isMoving, isCrouching, isGrounded, isSprinting, is_Leaning, is_Leaning_Right;

    protected bool Can_Run = false;

    protected float inputX, inputY;
    protected float inputX_Set, inputY_Set;
    private float inputModifyFactor;

    protected bool limitDiagonalSpeed = true;

    protected float antiBumpFactor = 0.75f;

    protected CharacterController charController;
    protected Vector3 moveDirection = new Vector3(0,0,0);
    private Vector3 m_Current_Wall_Norm = Vector3.zero;
    
    private float L_Lean_Pos_X, R_Lean_Pos_X;

    public LayerMask groundLayer, Wall_Layer;
    protected float rayDistance;
    protected float default_ControllerHeight;
    protected Vector3 default_CamPos, New_Cam_Pos = Vector3.zero;
    protected float camHeight;

    
    
	// Use this for initialization
	void Start () {
        //transform.Find() = looks for specific object ONLY in the children of this gameobject NOT the whole hierarchy
        
        firstPerson_Camera = transform.GetChild(0).transform;
        firstPerson_View = firstPerson_Camera.transform.Find("FPS View").transform;
        charController = GetComponent<CharacterController>();
        speed = walkSpeed;
        isMoving = false;
        m_Climb_Constant_Push_Original = wall_Climb_Constant_Push_Max;
        rayDistance = charController.height * 0.5f + charController.radius;
        L_Lean_Pos_X = -lean_Distance;
        R_Lean_Pos_X = lean_Distance;
        default_ControllerHeight = charController.height;
        default_CamPos = firstPerson_Camera.localPosition;
        New_Cam_Pos = firstPerson_Camera.localPosition;
	}
	
	// Update is called once per frame
	void Update () {
        Player_Inputs();
        PlayerMovement();
        Advanced_Movements();
       
    }

    protected void PlayerMovement()
    {

        inputY = Mathf.Lerp(inputY, inputY_Set, Time.deltaTime * 19f);
        inputX = Mathf.Lerp(inputX, inputX_Set, Time.deltaTime * 19f);

        inputModifyFactor = Mathf.Lerp(inputModifyFactor,
            (inputY_Set != 0 && inputX_Set != 0 && limitDiagonalSpeed) ? 0.75f : 1.0f, Time.deltaTime * 19f);


        if (isCrouching)
        {
            Can_Run = false;
            speed = crouchSpeed;
        }
        else
        {
            speed = isSprinting ? runSpeed : walkSpeed;
        }


        if (isGrounded)
        {

            moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);

            moveDirection = transform.TransformDirection(moveDirection) * speed;

            PlayerJump();
            if (m_Can_Do_Advanced_Movements)
            {
                m_Can_Do_Advanced_Movements = false;
            }
        }else
        {
            m_Can_Do_Advanced_Movements = true;
        }

        if (!isGrounded && !m_Is_Climbing_Wall) {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        isGrounded = (charController.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0 ;

        isMoving = charController.velocity.magnitude > 0.15f;

    }

    void Advanced_Movements()
    {
        if (m_Can_Do_Advanced_Movements && Wall_In_Front() && Input.GetKey(KeyCode.Space))
        {
            if (m_Current_Wall_Climb_Timer < wall_Climb_Timer_Max) {
                if (!m_Is_Climbing_Wall) m_Is_Climbing_Wall = true;
                m_Current_Wall_Climb_Timer += Time.deltaTime;
                float prc = m_Current_Wall_Climb_Timer / wall_Climb_Timer_Max;
                float current_Push_Amount = Mathf.Lerp(wall_Climb_Constant_Push_Max, 0, prc);
                moveDirection.y += current_Push_Amount * Time.deltaTime;
            }else if ((m_Current_Wall_Climb_Timer >= wall_Climb_Timer_Max && m_Is_Climbing_Wall)|| !Wall_In_Front())
            {
                if (m_Is_Climbing_Wall) m_Is_Climbing_Wall = false;
                m_Current_Wall_Climb_Timer = 0;
                Jump_Away_From_Wall(m_Current_Wall_Norm);
            }
        }
    }

    protected void Player_Inputs()
    {
        // these are specified controls
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.W))
            {
                inputY_Set = 1;
                Can_Run = true;
            }
            else
            {
                inputY_Set = -1;
                Can_Run = false;
            }
        }
        else     //this means if the player is pressing neither
        {
            inputY_Set = 0;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.A))
            {
                inputX_Set = -1;
            }
            else
            {
                inputX_Set = 1;
            }
        }
        else     //this means if the player is pressing neither
        {
            inputX_Set = 0;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isCrouching && CanGetUp())
            {
                isCrouching = false;
                StartCoroutine(LowerCamera());
            }
            else if (!isCrouching)
            {
                isCrouching = true;
                StartCoroutine(LowerCamera());
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && (!is_Leaning || !is_Leaning_Right) && Can_Lean_Right())
        {
            is_Leaning = true;
            StartCoroutine(Lean_Camera(true));
            
        }else if (Input.GetKeyDown(KeyCode.E) && is_Leaning && is_Leaning_Right)
        {
            is_Leaning = false;
            StartCoroutine(Reset_Cam_Lean());
            
        }
        else if (Input.GetKeyDown(KeyCode.Q) && (!is_Leaning || is_Leaning_Right) && Can_Lean_Left())
        {
            is_Leaning = true;
            StartCoroutine(Lean_Camera(false));
            
        }
        else if (Input.GetKeyDown(KeyCode.Q) && is_Leaning && !is_Leaning_Right)
        {
            is_Leaning = false;
            StartCoroutine(Reset_Cam_Lean());
            
        }else if (is_Leaning && (!Can_Lean_Left() || !Can_Lean_Right()))
        {
            is_Leaning = false;
            StartCoroutine(Reset_Cam_Lean());
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (is_Leaning)
            {
                is_Leaning = false;
                StartCoroutine(Reset_Cam_Lean());
            }
            if (isCrouching && CanGetUp())
            {
                isCrouching = false;
                StartCoroutine(LowerCamera());
                
            }
            isSprinting = true;
            
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
        }
    }

    

   

    protected bool CanGetUp()
    {
        Ray groundRay = new Ray(transform.position, transform.up);
        RaycastHit groundHit;

        if (Physics.SphereCast(groundRay, charController.radius + 0.05f, out groundHit, rayDistance, groundLayer))
        {
           

            if (Vector3.Distance(transform.position, groundHit.point) < 2.3f)
            {
                return false;
            }
        }

        return true;
    }

    

    virtual public IEnumerator LowerCamera()
    {
        charController.height = isCrouching ? default_ControllerHeight / 1.5f : default_ControllerHeight;
        if (isCrouching) {
            charController.center = new Vector3(0f, charController.height / 2f, 0f);
        }else
        {
            charController.center = Vector3.zero;
        }

        camHeight = isCrouching ? default_CamPos.y / 1.5f : default_CamPos.y;
        New_Cam_Pos.y = camHeight;

        while (Mathf.Abs(camHeight - firstPerson_View.localPosition.y) > 0.01f)
        {
            firstPerson_View.localPosition = Vector3.Lerp(firstPerson_View.localPosition, 
                New_Cam_Pos, Time.deltaTime * 11f);

            yield return null;
        }
    }

   virtual public IEnumerator Lean_Camera(bool _lean_Right)
    {
        is_Leaning_Right = _lean_Right;
        float cam_Lean = _lean_Right ? default_CamPos.x + R_Lean_Pos_X : default_CamPos.x + L_Lean_Pos_X;
        New_Cam_Pos.x = cam_Lean;
        

        while (firstPerson_Camera.localPosition != New_Cam_Pos)
        {
            firstPerson_Camera.localPosition = Vector3.Lerp(firstPerson_Camera.localPosition, 
                New_Cam_Pos, Time.deltaTime * lean_Speed);
            
            yield return null;
        }
    }

    virtual public IEnumerator Reset_Cam_Lean()
    {
        New_Cam_Pos.x = 0;
        while (firstPerson_Camera.localPosition != New_Cam_Pos)
        {
            firstPerson_Camera.localPosition = Vector3.Lerp(firstPerson_Camera.localPosition,
                New_Cam_Pos, Time.deltaTime * lean_Speed);

          
            yield return null;
        }
    }

    protected void PlayerJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
                moveDirection.y = jumpSpeed;  
        }
    }

    protected void Jump_Away_From_Wall(Vector3 normal)
    {
        moveDirection = transform.TransformDirection(moveDirection) + (normal * wall_Climb_Jump_Force); 
    }

    protected bool Wall_In_Front()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, front_Wall_SphereCast_Rad, transform.forward, out hit, front_Wall_SphereCast_Dist))
        {
            m_Current_Wall_Norm = hit.normal;
            return true;
        }
        if (m_Current_Wall_Norm != Vector3.zero) m_Current_Wall_Norm = Vector3.zero;
        return false;
    }

    protected bool Can_Lean_Left()
    {
        RaycastHit hit;
        if (Physics.Raycast(firstPerson_Camera.position, -transform.right, out hit, lean_Distance, Wall_Layer))
        {
            return false;
        }
        return true;
    }

    protected bool Can_Lean_Right()
    {
        RaycastHit hit;
        if (Physics.Raycast(firstPerson_Camera.position, transform.right, out hit, lean_Distance, Wall_Layer))
        {
            return false;
        }
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere((transform.position + (transform.forward * front_Wall_SphereCast_Dist)), front_Wall_SphereCast_Rad);
    }

}
