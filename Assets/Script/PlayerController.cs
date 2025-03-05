using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // Input and component references
    private PlayerInputActions inputActions;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator playerAnimator;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float dashSpeed = 15f;
    private bool isDashing = false;

    private Vector2 moveInput;

    [Header("Attack Settings")]
    [SerializeField] private float attackDelay = 0.5f;
    [SerializeField] private float initialAttackDelay = 0.2f;
    private int attackState = 0;
    private bool isAttacking = false;

    [Header("Movement Delay Settings")]
    public float movementDelay = 0.5f;
    private bool isMovementDelayed = false;

    private void Start()
    {
        // Initialize components
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerAnimator = GetComponent<Animator>();
        Debug.Log("PlayerController Started");
    }

    private void Awake()
    {
        // Initialize input actions and rigidbody
        inputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();

        // Assign input events
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        inputActions.Player.Jump.performed += ctx => Jump();
        inputActions.Player.Dash.performed += ctx => StartCoroutine(Dash());
        inputActions.Player.Attack.performed += ctx => StartCoroutine(Attack());
    }

    private void OnEnable()
    {
        inputActions.Enable();
        Debug.Log("Input Actions Enabled");
    }

    private void OnDisable()
    {
        inputActions.Disable();
        Debug.Log("Input Actions Disabled");
    }

    private void FixedUpdate()
    {
        // Handle movement unless dashing
        if (!isDashing)
        {
            rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        }

        // Grounded state handling
        bool isGrounded = IsGrounded();
        playerAnimator.SetBool("IsGrounded", isGrounded);

        // Allow movement if not attacking or airborne
        if (!isAttacking || !isGrounded)
        {
            rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        }
    }

    private void Update()
    {
        // Handle movement input restrictions during attack or movement delay
        if (isAttacking || isMovementDelayed)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
        }

        // Flip character based on movement direction
        if (moveInput.x < 0)
        {
            FlipPlayer(true);
        }
        else if (moveInput.x > 0)
        {
            FlipPlayer(false);
        }

        // Update animation state
        if (moveInput.x != 0)
        {
            playerAnimator.SetInteger("AnimationState", 1);
        }
        else
        {
            playerAnimator.SetInteger("AnimationState", 0);
        }

        // Move the player
        transform.Translate(Vector2.right * moveInput.x * moveSpeed * Time.deltaTime);
    }

    private void Jump()
    {
        // Handle jumping only when grounded
        if (IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            playerAnimator.SetTrigger("Jump");
            Debug.Log("Player Jumped");
        }
        else
        {
            Debug.LogWarning("Attempted to jump while not grounded!");
        }
    }

    private IEnumerator Dash()
    {
        Debug.Log("Dash Started");
        isDashing = true;
        playerAnimator.SetTrigger("Roll");

        float originalSpeed = moveSpeed;
        float originalDashSpeed = dashSpeed; // Store original dash speed

        dashSpeed *= 1.5f; // Increase dash speed temporarily
        moveSpeed = dashSpeed;

        yield return new WaitForSeconds(0.2f); // Dash duration

        dashSpeed = originalDashSpeed; // Restore original dash speed
        moveSpeed = originalSpeed;
        isDashing = false;

        Debug.Log("Dash Ended");
    }

    private bool IsGrounded()
    {
        // Check if the player is touching the ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
        return hit.collider != null;
    }

    private void FlipPlayer(bool flip)
    {
        // Flip sprite based on movement direction
        spriteRenderer.flipX = flip;
    }

    private IEnumerator Attack()
    {
        // Prevent multiple attacks at once
        if (isAttacking)
        {
            Debug.Log("Attack already in progress");
            yield break;
        }

        // Prevent attacking while airborne
        if (!IsGrounded())
        {
            Debug.Log("Cannot attack while in the air");
            yield break;
        }

        isAttacking = true;
        Debug.Log("Attack Started");

        // Initial attack delay
        yield return new WaitForSeconds(initialAttackDelay);
        Debug.Log("Initial Attack Delay Passed");

        // Cycle through attack states
        attackState++;
        if (attackState > 2)
        {
            attackState = 0;
            Debug.Log("Resetting attack state to 0 (Attack1)");
        }

        // Reset attack animation triggers
        playerAnimator.ResetTrigger("Attack1");
        playerAnimator.ResetTrigger("Attack2");
        playerAnimator.ResetTrigger("Attack3");

        // Set the correct attack animation
        if (attackState == 0)
        {
            playerAnimator.SetTrigger("Attack1");
            Debug.Log("Performing Attack1");
        }
        else if (attackState == 1)
        {
            playerAnimator.SetTrigger("Attack2");
            Debug.Log("Performing Attack2");
        }
        else if (attackState == 2)
        {
            playerAnimator.SetTrigger("Attack3");
            Debug.Log("Performing Attack3");
        }

        // Apply movement delay only when grounded
        if (IsGrounded())
        {
            isMovementDelayed = true;
            yield return new WaitForSeconds(movementDelay);
            isMovementDelayed = false;
        }

        // Attack cooldown
        yield return new WaitForSeconds(attackDelay);
        Debug.Log($"Attack {attackState + 1} Finished");

        isAttacking = false;
    }
}
