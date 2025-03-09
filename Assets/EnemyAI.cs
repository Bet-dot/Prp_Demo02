using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public Transform player;
    public float moveSpeed = 2f;
    public float detectionRadius = 5f;
    public float changeDirectionTime = 2f;
    public float idleTime = 1f;
    public float raycastDistance = 1f;  // ระยะตรวจสอบสิ่งกีดขวาง

    [Header("Movement Boundaries")]
    public float minX = -5f;
    public float maxX = 5f;

    [Header("State Settings")]
    public bool isDead = false;
    private bool isWaiting = false;
    private bool isChasingPlayer = false; // ตรวจสอบสถานะว่าไล่ผู้เล่นอยู่หรือไม่

    private Animator animator;
    private Vector2 targetPosition;
    private Vector2 lastPosition;

    private void Start()
    {
        animator = GetComponent<Animator>();
        SetNewTargetPosition();
        InvokeRepeating("SetNewTargetPosition", changeDirectionTime, changeDirectionTime);

        // เริ่มต้นเดินในพื้นที่ของมันเอง
        isChasingPlayer = false;
    }

    private void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (isChasingPlayer)  // ถ้าไล่ตามผู้เล่น
        {
            if (distanceToPlayer < detectionRadius)  // ถ้าผู้เล่นยังอยู่ในระยะการมองเห็น
                MoveTowardsPlayer();
            else  // ถ้าผู้เล่นออกจากระยะการมองเห็นแล้ว
                StopChasingPlayer();
        }
        else  // ถ้ายังไม่ได้ไล่ตามผู้เล่น
        {
            MoveTowardsTarget();
            if (distanceToPlayer < detectionRadius && IsFacingPlayer())  // หากมองเห็นผู้เล่น
            {
                StartChasingPlayer();
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        animator.SetBool("isWalking", true);
        Flip(direction);
    }

    private void MoveTowardsTarget()
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        bool isMoving = (Vector2)transform.position != lastPosition;

        if (animator.GetBool("isWalking") != isMoving)
        {
            animator.SetBool("isWalking", isMoving);
        }

        lastPosition = transform.position;
        Flip(direction);

        // ตรวจสอบสิ่งกีดขวางหรือหลุม
        if (IsObstacleAhead())
        {
            StartCoroutine(WaitBeforeChangingDirection());
        }

        if (Mathf.Abs(transform.position.x - targetPosition.x) < 0.1f)
        {
            StartCoroutine(WaitBeforeMoving());
        }
    }

    private void SetNewTargetPosition()
    {
        float randomX = Random.Range(minX, maxX);
        targetPosition = new Vector2(randomX, transform.position.y);
    }

    private void Flip(Vector2 direction)
    {
        if (direction.x > 0 && transform.localScale.x < 0)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        else if (direction.x < 0 && transform.localScale.x > 0)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    // ตรวจสอบว่ามีสิ่งกีดขวางข้างหน้า AI หรือไม่
    private bool IsObstacleAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, raycastDistance);
        return hit.collider != null;  // ถ้ามีสิ่งกีดขวาง
    }

    private IEnumerator WaitBeforeChangingDirection()
    {
        animator.SetBool("isWalking", false); // หยุดการเดิน
        isWaiting = true;
        yield return new WaitForSeconds(idleTime); // หยุด idle ก่อนที่จะเปลี่ยนทิศทาง
        isWaiting = false;

        // เปลี่ยนทิศทาง
        SetNewTargetPosition();
    }

    private IEnumerator WaitBeforeMoving()
    {
        animator.SetBool("isWalking", false);
        isWaiting = true;
        yield return new WaitForSeconds(idleTime);
        isWaiting = false;
        SetNewTargetPosition();
    }

    // ตรวจสอบว่า AI มองไปทางผู้เล่นหรือไม่
    private bool IsFacingPlayer()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        return Vector2.Dot(transform.right, directionToPlayer) > 0;  // ถ้าอยู่ในทิศทางที่มอง
    }

    private void StartChasingPlayer()
    {
        isChasingPlayer = true;
    }

    private void StopChasingPlayer()
    {
        isChasingPlayer = false;
        SetNewTargetPosition();  // เมื่อหยุดไล่ตามกลับไปเดินในพื้นที่
    }

    // ฟังก์ชันอื่นๆ ที่เกี่ยวข้อง
    public void TakeHit()
    {
        if (isDead) return;
        animator.SetTrigger("TakeHitTrigger");
    }

    public void Shield()
    {
        if (isDead) return;
        animator.SetTrigger("ShieldTrigger");
    }

    public void Attack()
    {
        if (isDead) return;
        animator.SetBool("isAttacking", true);

        if (Random.value > 0.5f)
            animator.SetTrigger("Attack1Trigger");
        else
            animator.SetTrigger("Attack2Trigger");

        StartCoroutine(ResetAttackState());
    }

    private IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(1f);
        animator.SetBool("isAttacking", false);
    }

    public void Die()
    {
        isDead = true;
        animator.SetTrigger("DeathTrigger");
        animator.SetBool("isWalking", false);
    }
}
