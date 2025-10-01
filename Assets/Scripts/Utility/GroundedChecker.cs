using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedChecker : MonoBehaviour
{
    [Header("Inputs")]
    public Vector3 offset;
    public float groundedRayLength = 0.2f;
    public int groundedRayCount = 8;
    public float canJumpRatio = 0.4f;
    public float Radius = 0.5f;
    
    [Header("Outputs")] 
    [SerializeField]
    private bool isGrounded;   
    [SerializeField]
    private bool canJump;
    public bool IsGrounded { get { return isGrounded; } }  
    public bool CanJump { get { return canJump; } }

    private ChangeTracker<bool> groundedTracker;

    public event Action OnLeaveGround;
    public event Action OnLand;



    private void Start()
    {
        groundedTracker = new ChangeTracker<bool>(() => IsGrounded);
    }
    // Update is called once per frame
    void Update()
    {
        CheckGrounded();

        if (groundedTracker.Update()) 
        {
            if (IsGrounded) OnLand?.Invoke();
            else OnLeaveGround?.Invoke();
        }
    }   

    private void CheckGrounded()
    {
        float angleStep = 360f / groundedRayCount;
        int castHits = 0;

        for (int i = 0; i < groundedRayCount; i++)
        {
            // Calculate the angle for this ray
            float angle = i * angleStep * Mathf.Deg2Rad;

            // Calculate the position of the raycast origin in the circle
            Vector3 circlePosition = transform.position + new Vector3((Mathf.Cos(angle) * Radius + offset.x), offset.y, (Mathf.Sin(angle) * Radius) + offset.z);

            float height = groundedRayLength;

            // Perform the raycast
            RaycastHit hit;
            if (Physics.Raycast(circlePosition, Vector3.down, out hit, height))
            {
                // If the raycast hits something, draw a green ray
                Debug.DrawRay(circlePosition, Vector3.down * hit.distance, Color.green);
                castHits++;
            }
            else
            {
                // If the raycast doesn't hit anything, draw a red ray
                Debug.DrawRay(circlePosition, Vector3.down * height, Color.red);
            }
        }

        if (castHits >= (int)(groundedRayCount * canJumpRatio))
        {
            canJump = true;
        }
        else canJump = false;

        if (castHits >= 1)
        {
            isGrounded = true;
        }
        else isGrounded = false;
    }
}
