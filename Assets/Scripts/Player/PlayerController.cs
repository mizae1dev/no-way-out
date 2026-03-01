using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpDrag = 0.4f;
    [SerializeField] private float airMultiplier = 0.4f;
    
    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundDragDistance = 0.2f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource footstepAudio;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float footstepVolume = 0.3f;
    [SerializeField] private float footstepInterval = 0.5f;

    private CharacterController characterController;
    private Vector3 velocity;
    private float lastFootstepTime;
    private bool isGrounded;
    private bool isCrouching;
    private float currentSpeed;
    private NoiseSystem noiseSystem;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        noiseSystem = GetComponent<NoiseSystem>();
    }

    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + groundDragDistance, groundLayer);
        HandleMovement();
        SpeedControl();
        ApplyDrag();

        if (characterController.isGrounded)
            velocity.y = -2f;
        else
            velocity.y -= 9.81f * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        currentSpeed = isCrouching ? crouchSpeed : walkSpeed;
        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;
        
        if (isGrounded)
        {
            velocity.x = moveDirection.x * currentSpeed;
            velocity.z = moveDirection.z * currentSpeed;
        }
        else
        {
            velocity.x += moveDirection.x * currentSpeed * airMultiplier * Time.deltaTime;
            velocity.z += moveDirection.z * currentSpeed * airMultiplier * Time.deltaTime;
        }

        if ((horizontal != 0 || vertical != 0) && isGrounded)
        {
            float noiseLevel = isCrouching ? 0.3f : 0.7f;
            noiseSystem?.AddNoise(noiseLevel, transform.position);
            PlayFootstep();
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
        if (flatVel.magnitude > currentSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentSpeed;
            velocity.x = limitedVel.x;
            velocity.z = limitedVel.z;
        }
    }

    private void ApplyDrag()
    {
        float drag = isGrounded ? groundDrag : jumpDrag;
        velocity.x *= Mathf.Exp(-drag * Time.deltaTime);
        velocity.z *= Mathf.Exp(-drag * Time.deltaTime);
    }

    private void PlayFootstep()
    {
        if (Time.time - lastFootstepTime < footstepInterval) return;
        if (footstepClips.Length > 0 && footstepAudio != null)
        {
            int randomClip = Random.Range(0, footstepClips.Length);
            footstepAudio.PlayOneShot(footstepClips[randomClip], footstepVolume);
            lastFootstepTime = Time.time;
        }
    }

    public bool IsCrouching => isCrouching;
    public float CurrentSpeed => currentSpeed;
}