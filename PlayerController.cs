using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
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

    [Header("Ghost Settings")]
    public GameObject ghostPrefab;
    private GameObject ghostInstance;

    // Attack Settings
    [Header("Attack Settings")]
    [SerializeField] private float attackDelay = 0.5f;
    [SerializeField] private float initialAttackDelay = 0.2f;
    private int attackState = 0;
    private bool isAttacking = false;

    [Header("Movement Delay Settings")]
    public float movementDelay = 0.5f; // ตั้งค่าได้
    private bool isMovementDelayed = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerAnimator = GetComponent<Animator>();
        Debug.Log("PlayerController Started");
    }

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();

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
        if (!isDashing)
        {
            rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        }

        bool isGrounded = IsGrounded();
        playerAnimator.SetBool("IsGrounded", isGrounded);
    }

    private void Update()
    {
        if (isAttacking || isMovementDelayed) // หากกำลังโจมตีหรือดีเลย์การเคลื่อนที่
        {
            moveInput = Vector2.zero; // หยุดการเคลื่อนที่
        }
        else
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
        }

        if (moveInput.x < 0)
        {
            FlipPlayer(true);
        }
        else if (moveInput.x > 0)
        {
            FlipPlayer(false);
        }

        if (moveInput.x != 0)
        {
            playerAnimator.SetInteger("AnimationState", 1);
        }
        else
        {
            playerAnimator.SetInteger("AnimationState", 0);
        }

        transform.Translate(Vector2.right * moveInput.x * moveSpeed * Time.deltaTime);
    }

    private void Jump()
    {
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

        // เพิ่ม trigger roll ในการ Dash
        playerAnimator.SetTrigger("Roll"); // เรียกใช้ Trigger roll

        float originalSpeed = moveSpeed;
        moveSpeed = dashSpeed;

        if (ghostInstance == null)
        {
            ghostInstance = Instantiate(ghostPrefab, transform.position, Quaternion.identity);
            ghostInstance.SetActive(true);
        }

        for (int i = 0; i < 5; i++)
        {
            CreateGhost();
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.2f);

        moveSpeed = originalSpeed;
        isDashing = false;

        if (ghostInstance != null)
        {
            ghostInstance.SetActive(false);
        }

        Debug.Log("Dash Ended");
    }

    private void CreateGhost()
    {
        GameObject ghost = Instantiate(ghostPrefab, transform.position, Quaternion.identity);
        FlipGhost(ghost, moveInput.x);

        Animator ghostAnimator = ghost.GetComponent<Animator>();
        if (ghostAnimator != null)
        {
            ghostAnimator.runtimeAnimatorController = playerAnimator.runtimeAnimatorController;
        }

        StartCoroutine(FadeOutGhost(ghost.GetComponent<SpriteRenderer>()));
    }

    private void FlipGhost(GameObject ghost, float moveDirection)
    {
        if (moveDirection < 0)
        {
            ghost.transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (moveDirection > 0)
        {
            ghost.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private IEnumerator FadeOutGhost(SpriteRenderer ghostRenderer)
    {
        float elapsedTime = 0f;
        Color initialColor = ghostRenderer.color;

        while (elapsedTime < 0.5f)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / 0.5f);
            ghostRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(ghostRenderer.gameObject);
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
        return hit.collider != null;
    }

    private void FlipPlayer(bool flip)
    {
        if (flip)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private IEnumerator Attack()
    {
        if (isAttacking)
        {
            Debug.Log("Attack already in progress");
            yield break;
        }

        isAttacking = true;
        Debug.Log("Attack Started");

        // ดีเลย์ก่อนเริ่มท่าแรก
        yield return new WaitForSeconds(initialAttackDelay);
        Debug.Log("Initial Attack Delay Passed");

        attackState++;

        if (attackState > 2)
        {
            attackState = 0;
            Debug.Log("Resetting attack state to 0 (Attack1)");
        }

        playerAnimator.ResetTrigger("Attack1");
        playerAnimator.ResetTrigger("Attack2");
        playerAnimator.ResetTrigger("Attack3");

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

        // ดีเลย์การเคลื่อนที่หลังจากโจมตี
        isMovementDelayed = true;
        yield return new WaitForSeconds(movementDelay); // ดีเลย์การเคลื่อนที่
        isMovementDelayed = false;

        yield return new WaitForSeconds(attackDelay);
        Debug.Log($"Attack {attackState + 1} Finished");

        isAttacking = false;
    }


}
