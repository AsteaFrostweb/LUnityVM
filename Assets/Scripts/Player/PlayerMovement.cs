using Assets.Scripts.Utility;
using Game.Utility;
using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

using UnityEngine.Experimental.AI;
using UnityEngine.UIElements;


public class PlayerMovement : MonoBehaviour
{
    //Inspector variables
 
 
    [Header("Movement")]
    [SerializeField]
    private float Acceleration = 100f;
    public float MoveSpeed = 25f;
    public float SprintSpeed = 75f;
    [Range(1f,100f)]
    [SerializeField]
    private float Drag = 10f;
    [SerializeField]
    private float jumpHeight = 2f;


    [Header("View")]
    [SerializeField]
    private Transform playerHead;
    [SerializeField]
    private Vector2 mouseSpeed = Vector2.one;
    [SerializeField]
    private float lookAnglecap = 75f;

    [Header("Outputs")]
    [SerializeField]
    private float speed;
    [SerializeField]
    private bool isSprinting;
    [SerializeField]
    private bool isADS;
    [SerializeField]
    private Vector3 velocity;

    public float Speed { get { return speed; } }   
    public bool IsSprinting { get { return isSprinting; } }
    public bool IsADS { get { return isADS; } }


    //Private variables

    private CharacterController controller;        
    public Vector3 playerVelocity { get; private set; }
    public Vector3 playerLocalVelocity { get { return transform.InverseTransformDirection(playerVelocity); } }
    private DateTime previousJumpTime;
    private GroundedChecker GroundChecker;
    private Player player_info;

    //Change trackers
    private ChangeTracker<bool> sprintTracker;  
    private ChangeTracker<bool> adsTracker;

    //Events
    public event Action OnSprintStart;
    public event Action OnSprintEnd;
    public event Action OnJump;
    public event Action OnLand;

    //private input varibles
    float mouseX;
    float mouseY;

    private void Start()
    {
        //Find objects


        controller = GetComponent<CharacterController>();
        GroundChecker = GetComponent<GroundedChecker>();
   

        //Initialize change trackers
        sprintTracker = new ChangeTracker<bool>(() => isSprinting);        
        adsTracker = new ChangeTracker<bool>(() => isADS);

       

        //Assigning Events
        OnJump += () => Jump();
        OnLand += () => Land();
        GroundChecker.OnLeaveGround += () => OnLeaveGround();
        GroundChecker.OnLand += () => this.OnLand.Invoke();
       

    }



    void Update()
    {             
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        GetEvents();
       
        GetMovementInput();      
        GetRotationInput();
          

        ApplyGravity();

        DampenVelocity(Time.deltaTime);
    }

    private void FixedUpdate()
    {
       

        HandleGrounded();

        HandleMovement();

        HandleRotation();
        
    }

    private void GetEvents()
    {
        isSprinting = Input.GetButton("Sprint");         
        

        if (Input.GetButtonDown("Jump") && GroundChecker.CanJump)
        {
            float timeSinceJump = (float)DateTime.Now.Subtract(previousJumpTime).TotalSeconds;
            if (timeSinceJump >= 0.15f) OnJump.Invoke(); 
        }

        if (sprintTracker.Update()) //returns true if the "isSprinting" value is different than last frame
        {
            if (isSprinting && OnSprintStart != null) OnSprintStart.Invoke();
            else if (OnSprintEnd != null) OnSprintEnd.Invoke();
        }

    }


    private void GetMovementInput() 
    {      

        Vector2 inputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));      
        if (inputs.x == 0 && inputs.y == 0) return;
        if (inputs.magnitude > 1) 
        {
            inputs = inputs.normalized;
        }


        Vector3 move = new Vector3(inputs.x, 0, inputs.y) * Acceleration * Time.deltaTime; 
        move = transform.TransformDirection(move);       
        if (GroundChecker.IsGrounded)
        {           
            playerVelocity += move;
            Clampvelocity();          
        }
            
    }
    private void HandleMovement() 
    {
        controller.Move(playerVelocity * Time.fixedDeltaTime);

        velocity = playerVelocity;
        speed = GameMath.VecXZ(playerVelocity).magnitude;
    }


    private void GetRotationInput() 
    {
        mouseX += Input.GetAxis("Mouse X");
        mouseY += Input.GetAxis("Mouse Y");
    }
    private void HandleRotation() 
    {         
        transform.Rotate(Vector3.up * mouseX * mouseSpeed.x * Time.fixedDeltaTime); //rotate player y by mouseX
        mouseX = 0f;
        
        Vector3 currentRotation = playerHead.localEulerAngles;
        currentRotation.x -= mouseY * mouseSpeed.y * Time.fixedDeltaTime;
        currentRotation.x = GameMath.ClampEulerAngle(currentRotation.x, -lookAnglecap, lookAnglecap);
        playerHead.localEulerAngles = currentRotation;
        mouseY = 0f;
    }

    


    private void Jump() 
    {
        if (playerVelocity.y <= 0f)
        {
            previousJumpTime = DateTime.Now;
            float jumpImpulse = Mathf.Sqrt(jumpHeight * -3.0f * Physics.gravity.y);
            playerVelocity += new Vector3(playerVelocity.x, jumpImpulse, playerVelocity.z);
        }
    }
    private void Land()
    {
        //play sound fx if was moving certan speed before collision
        //handle fall damage maybe
      
    }

    private void OnLeaveGround() 
    {
       
    }


   
    private void HandleGrounded()
    {
        float timeSinceJump = (float)DateTime.Now.Subtract(previousJumpTime).TotalSeconds;
        if (GroundChecker.IsGrounded && (timeSinceJump > 0.15f))
        {
            playerVelocity = new Vector3(playerVelocity.x, -2.0f, playerVelocity.z);
        }
    }
    private void ApplyGravity()
    {
        if (!GroundChecker.IsGrounded)
        {
            playerVelocity += Physics.gravity * Time.deltaTime;
        }
    }

    private void Clampvelocity() 
    {              
        Vector3 playerXZVel = GameMath.VecXZ(playerVelocity);
        if (playerXZVel.magnitude > SprintSpeed) 
        {
            playerXZVel = playerXZVel.normalized * SprintSpeed;

            playerVelocity = new Vector3(playerXZVel.x, playerVelocity.y, playerXZVel.z);
        }
        if (playerXZVel.magnitude > MoveSpeed && !isSprinting)
        {
            playerXZVel = playerXZVel.normalized * MoveSpeed;

            playerVelocity = new Vector3(playerXZVel.x, playerVelocity.y, playerXZVel.z);
        }
    }
    private void DampenVelocity(float time) 
    {
        if (!GroundChecker.IsGrounded) return;

        Vector3 playerXZVel = GameMath.VecXZ(playerVelocity);
        if (playerXZVel.magnitude <= 0.05f)
        {
            playerVelocity = new Vector3(0f, playerVelocity.y, 0f);
            return;
        }        
       
        playerXZVel = playerXZVel.normalized * Drag * time;
        playerVelocity -= playerXZVel;

        if (playerVelocity.magnitude <= playerXZVel.magnitude)
        {
            playerVelocity = new Vector3(0f, playerVelocity.y, 0f);        
        }
    }  
  

}
