using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePlayerController : MonoBehaviour {

    public Transform firstPerson_View;
    public Transform firstPerson_Camera;
    public Transform peek_Vision_X_Rotator;

    protected Vector3 firstPerson_View_Rotation = new Vector3(0, 0, 0);
    protected Vector3 original_Peek_Vision_Pos;

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
    private bool m_Can_Do_Advanced_Movements = false, m_Is_Climbing_Wall = false;

    [Header("Advanced movement raycast values")]
    public float front_Wall_SphereCast_Dist;
    public float front_Wall_SphereCast_Rad;

    //Hiding values
    private bool can_Hide, can_Knockout, can_Peek;
    //current action values
    private bool m_Is_Hiding, m_Is_Peeking;
    private Transform m_Hiding_Object;

    protected float speed, original_Speed;

    protected bool isMoving, isCrouching, isGrounded, isSprinting, is_Leaning, is_Leaning_Right;

    protected bool Can_Run = false;

    protected float inputX, inputY;
    protected float inputX_Set, inputY_Set;
    private float inputModifyFactor;

    protected bool limitDiagonalSpeed = true;

    protected float antiBumpFactor = 0.75f;

    protected CharacterController charController;
    protected Vector3 moveDirection = new Vector3(0, 0, 0);
    private Vector3 m_Current_Wall_Norm = Vector3.zero;

    private float L_Lean_Pos_X, R_Lean_Pos_X;

    public LayerMask groundLayer, Wall_Layer;
    protected float rayDistance;
    protected float default_ControllerHeight;
    protected Vector3 default_CamPos, New_Cam_Pos = Vector3.zero;
    protected float camHeight;



    // Use this for initialization
    void Start() {
        //transform.Find() = looks for specific object ONLY in the children of this gameobject NOT the whole hierarchy

        
        
        peek_Vision_X_Rotator.GetComponent<FPSMouseCameraFollow>().enabled = false;
        original_Peek_Vision_Pos = peek_Vision_X_Rotator.localPosition;
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
    void Update() {
        Player_Inputs();
        PlayerMovement();
        Advanced_Movements();
        Check_Interactables();

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
        }else if (m_Is_Hiding || m_Is_Peeking)
        {
            Can_Run = false;
            speed = 0;
        }
        else
        {
            speed = isSprinting ? runSpeed : walkSpeed;
        }


        if (isGrounded)
        {

            moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);

            moveDirection = transform.TransformDirection(moveDirection) * speed;

            
            if (m_Can_Do_Advanced_Movements)
            {
                m_Can_Do_Advanced_Movements = false;
            }
        } else
        {
            m_Can_Do_Advanced_Movements = true;
        }

        if (!isGrounded && !m_Is_Climbing_Wall) {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        isGrounded = (charController.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;

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
        // Vertical Movement
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

        //Horizontal Movement
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

        //Check crouching
        if (Input.GetKeyDown(KeyCode.C) && !m_Is_Hiding && !m_Is_Peeking)
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

        //Lean Camera
        if ((Input.GetKeyDown(KeyCode.E) && (!is_Leaning || !is_Leaning_Right) && Can_Lean_Right()) && !m_Is_Hiding && !m_Is_Peeking)
        {
            is_Leaning = true;
            StartCoroutine(Lean_Camera(true));
            
        }else if (Input.GetKeyDown(KeyCode.E) && is_Leaning && is_Leaning_Right)
        {
            is_Leaning = false;
            StartCoroutine(Reset_Cam_Lean());
            
        }
        else if ((Input.GetKeyDown(KeyCode.Q) && (!is_Leaning || is_Leaning_Right) && Can_Lean_Left()) && !m_Is_Hiding && !m_Is_Peeking)
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

        Debug.Log($"Can hide: {can_Hide}");

        //Use
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (can_Knockout)
            {

            }else if (can_Peek && !m_Is_Peeking)
            {
                if (is_Leaning)
                {
                    is_Leaning = false;
                    StartCoroutine(Reset_Cam_Lean());
                }
                Do_Peeking();
                Debug.Log("Called peeking");
            }
            else if (can_Hide && !m_Is_Hiding)
            {
                if (is_Leaning)
                {
                    is_Leaning = false;
                    StartCoroutine(Reset_Cam_Lean());
                }
                Do_Hiding();
                Debug.Log("Called hiding");
            }else if (m_Is_Hiding)
            {
                Reset_Hiding();
            }else if (m_Is_Peeking)
            {
                Reset_Peeking();
            }
        }

        //Sprint
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


    protected void Check_Interactables()
    {
        RaycastHit hit;

        if(Physics.SphereCast(firstPerson_Camera.position, front_Wall_SphereCast_Rad, firstPerson_Camera.forward, out hit, front_Wall_SphereCast_Dist))
        {
            if (hit.collider.tag == "Player")
            {

            }
            else if (hit.collider.tag == "Hiding_Place")
            {
                can_Hide = true;
                m_Hiding_Object = hit.transform;
                Debug.Log($"Can hide in: {hit.collider.name}");
            }
            else if (hit.collider.tag == "Door")
            {
                can_Peek = true;
                m_Hiding_Object = hit.transform;
            }

        }else{
            can_Hide = false;
            can_Peek = false;
            if(m_Hiding_Object != null && !m_Is_Hiding) m_Hiding_Object = null;
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

    protected void Do_Peeking()
    {
        m_Is_Peeking = true;
        charController.enabled = false;
        GetComponent<FPSMouseCameraFollow>().enabled = false;
        
        int other_Side_Door = 0;
        if (m_Hiding_Object.GetComponent<Hiding_Place_Var>().Hiding_Place_Camera_Pos.Length > 1)
        {
            float mag1 = Vector3.Distance(m_Hiding_Object.GetComponent<Hiding_Place_Var>().Hiding_Place_Camera_Pos[0].position, transform.position);
            float mag2 = Vector3.Distance(m_Hiding_Object.GetComponent<Hiding_Place_Var>().Hiding_Place_Camera_Pos[1].position , transform.position);
            if (mag1 > mag2)
            {
                other_Side_Door = 0;
            }else
            {
                other_Side_Door = 1;
            }

        }
        //peek_Vision_X_Rotator.forward = m_Hiding_Object.GetComponent<Hiding_Place_Var>().Hiding_Place_Camera_Pos[other_Side_Door].forward;
        peek_Vision_X_Rotator.position = m_Hiding_Object.GetComponent<Hiding_Place_Var>().Hiding_Place_Camera_Pos[other_Side_Door].position;
        peek_Vision_X_Rotator.GetComponent<FPSMouseCameraFollow>().enabled = true;
        peek_Vision_X_Rotator.GetComponent<FPSMouseCameraFollow>().Limit_Vision_Movement_Range(m_Hiding_Object.GetComponent<Hiding_Place_Var>().Hiding_Place_Camera_Pos[other_Side_Door], m_Hiding_Object.GetComponent<Hiding_Place_Var>().Limit_X_Amount, m_Hiding_Object.GetComponent<Hiding_Place_Var>().Limit_Y_Amount, false);
        firstPerson_Camera.GetComponent<FPSMouseCameraFollow>().Limit_Vision_Movement_Range(m_Hiding_Object.GetComponent<Hiding_Place_Var>().Hiding_Place_Camera_Pos[other_Side_Door], m_Hiding_Object.GetComponent<Hiding_Place_Var>().Limit_X_Amount, m_Hiding_Object.GetComponent<Hiding_Place_Var>().Limit_Y_Amount, false);
    }

    protected void Reset_Peeking()
    {
        m_Is_Peeking = false;
        peek_Vision_X_Rotator.GetComponent<FPSMouseCameraFollow>().enabled = false;
        peek_Vision_X_Rotator.localPosition = original_Peek_Vision_Pos;
        peek_Vision_X_Rotator.localRotation = Quaternion.Euler(original_Peek_Vision_Pos);
        charController.enabled = true;
        GetComponent<FPSMouseCameraFollow>().enabled = true;
        GetComponent<FPSMouseCameraFollow>().Reset_Vision_Movement_Range();
        firstPerson_Camera.GetComponent<FPSMouseCameraFollow>().Reset_Vision_Movement_Range();
    }

    protected void Do_Hiding()
    {
        m_Is_Hiding = true;
        charController.enabled = false;
        transform.position = m_Hiding_Object.transform.position;
        transform.forward = m_Hiding_Object.forward;
        firstPerson_Camera.forward = m_Hiding_Object.forward;
        firstPerson_Camera.transform.position = m_Hiding_Object.GetComponent<Hiding_Place_Var>().Hiding_Place_Camera_Pos[0].position;
        GetComponent<FPSMouseCameraFollow>().Limit_Vision_Movement_Range(m_Hiding_Object, m_Hiding_Object.GetComponent<Hiding_Place_Var>().Limit_X_Amount, m_Hiding_Object.GetComponent<Hiding_Place_Var>().Limit_Y_Amount, true);
        firstPerson_Camera.GetComponent<FPSMouseCameraFollow>().Limit_Vision_Movement_Range(m_Hiding_Object, m_Hiding_Object.GetComponent<Hiding_Place_Var>().Limit_X_Amount, m_Hiding_Object.GetComponent<Hiding_Place_Var>().Limit_Y_Amount, true);
    }

    protected void Reset_Hiding()
    {
        m_Is_Hiding = false;
        transform.position = m_Hiding_Object.transform.position + (transform.forward * 2f);
        charController.enabled = true;
        GetComponent<FPSMouseCameraFollow>().Reset_Vision_Movement_Range();
        firstPerson_Camera.GetComponent<FPSMouseCameraFollow>().Reset_Vision_Movement_Range();
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
        Gizmos.DrawWireSphere((firstPerson_Camera.position + (firstPerson_Camera.forward * front_Wall_SphereCast_Dist)), front_Wall_SphereCast_Rad);
    }

}
