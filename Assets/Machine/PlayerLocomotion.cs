using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
   InputManager inputManager;
   Vector3 moveDirection;
   Transform cameraObject;
   Rigidbody playerRigidbody;
   PlayerManager playerManager;
   AnimatorManager animatorManager;

   [Header ("Falling")]
   public float inAirTimer;
   public float leapingVelocity;
   public float fallingVelocity;
   public LayerMask groundLayer;
   public float rayCastHeightOffset = 0.5f;

   [Header("Movement Flags")]
   public bool isSprinting;
   public bool isGrounded;
   public bool isJumping;

   [Header("Movement Speeds")]
   public float walkingSpeed = 1f;
   public float runningSpeed = 3f;
   public float sprintingSpeed = 4f;
   public float rotationSpeed = 10f;

   [Header("Jumping")]
   public float jumpHeight = 3f;
   public float gravityIntensity = -15f;

   private void Awake()
   {
       inputManager = GetComponent<InputManager>();
       playerRigidbody = GetComponent<Rigidbody>();
       animatorManager = GetComponent<AnimatorManager>();
       cameraObject = Camera.main.transform;

       playerManager = GetComponent<PlayerManager>();
   }
    public void HandleAllMovement()
    {
        HandleFallingAndLanding();

        if(playerManager.isInteracting)
            return;

        HandleMovement();
        HandleRotation();
    }

   private void HandleMovement()
    {
        if(isJumping)
           return;
        
        moveDirection = cameraObject.forward * inputManager.verticalInput;
        moveDirection = moveDirection + cameraObject.right * inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if (isSprinting)
        {
            moveDirection = moveDirection * sprintingSpeed;
        }
        else
        {
            if(inputManager.moveAmount >= 0.5f)
            {
                moveDirection = moveDirection * runningSpeed;
            }
            else
            {
                moveDirection = moveDirection * walkingSpeed;
            }
        }
       
        Vector3 movementVelocity = moveDirection;
        playerRigidbody.linearVelocity = movementVelocity;
    }

    private void HandleRotation()
    {
        if(isJumping)
           return;
        
        Vector3 targetDirection = Vector3.zero;
        targetDirection = cameraObject.forward * inputManager.verticalInput;
        targetDirection = targetDirection + cameraObject.right * inputManager.horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;

        if(targetDirection == Vector3.zero)
        {
            targetDirection = transform.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = playerRotation;
    }

    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        Vector3 rayCastOrigin = transform.position;

        rayCastOrigin.y = rayCastOrigin.y + rayCastHeightOffset;

        if(!isGrounded && !isJumping)
        {
            if(!playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Falling", true);
            }

            inAirTimer = inAirTimer + Time.deltaTime;
            playerRigidbody.AddForce(transform.forward * leapingVelocity);
            playerRigidbody.AddForce(Vector3.down * 3 * fallingVelocity * inAirTimer);
        }
        if(Physics.SphereCast(rayCastOrigin, 0.2f, Vector3.down * 4, out hit, groundLayer))
        {
            if(!isGrounded && !playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Land", true);
            }
            inAirTimer = 0;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    public void HandleJumping()
    {
        if(isGrounded)
        {
            animatorManager.animator.SetBool("isJumping", true);
            animatorManager.PlayTargetAnimation("Jump", false);
            float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 playerVelocity = moveDirection;
            playerVelocity.y = jumpingVelocity;
            playerRigidbody.linearVelocity = playerVelocity;
        }
    }
}
