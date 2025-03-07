using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("AI Settings")]
    public Transform player; // ตำแหน่งของผู้เล่น
    public float moveSpeed = 2f; // ความเร็วในการเดินของศัตรู
    public float detectionRadius = 5f; // ระยะการตรวจจับของศัตรู
    public float changeDirectionTime = 2f; // เวลาที่ AI จะเปลี่ยนทิศทาง
    public float idleTime = 1f; // เวลาที่ AI จะหยุดนิ่งก่อนที่จะเดินไปทางใหม่

    [Header("Movement Boundaries")]
    public float minX = -5f; // ขอบเขตต่ำสุดในแกน X
    public float maxX = 5f; // ขอบเขตสูงสุดในแกน X

    [Header("State Settings")]
    public bool isDead = false; // เช็คว่า AI ตายหรือไม่

    private Animator animator; // ตัวแปรเก็บ Animator ของศัตรู
    private Vector2 targetPosition; // ตำแหน่งที่ AI จะเดินไป
    private bool isWaiting = false; // เช็คว่า AI กำลังหยุดนิ่งหรือไม่

    private void Start()
    {
        animator = GetComponent<Animator>(); // รับ Animator ของศัตรู
        SetNewTargetPosition(); // กำหนดตำแหน่งเป้าหมายเริ่มต้น
        InvokeRepeating("SetNewTargetPosition", changeDirectionTime, changeDirectionTime); // ตั้งเวลาการเปลี่ยนทิศทาง
    }

    private void Update()
    {
        if (isDead)
            return;

        // ตรวจสอบระยะห่างจากผู้เล่น
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius) // หากผู้เล่นอยู่ในระยะการตรวจจับ
        {
            // เดินไปหาผู้เล่น
            MoveTowardsPlayer();
        }
        else
        {
            // หากผู้เล่นห่างออกไป
            if (!isWaiting) // ถ้า AI กำลังไม่หยุดนิ่ง
                MoveTowardsTarget(); // เดินไปยังตำแหน่งที่สุ่ม
        }
    }

    private void MoveTowardsPlayer()
    {
        // คำนวณทิศทางและเดินไปหาผู้เล่น
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        // เปลี่ยนสถานะเป็นเดิน (Walk)
        animator.SetBool("isWalking", true);

        // พลิกทิศทางการเดินของ AI (flip) ตามทิศทางที่มันเคลื่อนที่
        Flip(direction);
    }

    private void MoveTowardsTarget()
    {
        // คำนวณทิศทางและเดินไปยังตำแหน่งเป้าหมายในแนวแกน X เท่านั้น
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // เปลี่ยนสถานะเป็นเดิน (Walk)
        animator.SetBool("isWalking", true); // ตั้งค่า isWalking เป็น true เมื่อ AI กำลังกระทำการเดิน

        // พลิกทิศทางการเดินของ AI (flip) ในแนว X
        Flip(direction);

        // ถ้า AI ถึงตำแหน่งเป้าหมายแล้ว ให้ตั้งตำแหน่งใหม่
        if (Mathf.Abs(transform.position.x - targetPosition.x) < 0.1f)
        {
            StartCoroutine(WaitBeforeMoving()); // รอให้ AI หยุดนิ่งก่อนแล้วค่อยเดินไปทางใหม่
        }
    }

    private void SetNewTargetPosition()
    {
        // ตั้งตำแหน่งสุ่มภายในขอบเขต X ที่กำหนด
        float randomX = Random.Range(minX, maxX);
        targetPosition = new Vector2(randomX, transform.position.y); // คงค่า Y เดิมไว้
    }

    private void Flip(Vector2 direction)
    {
        // เช็คทิศทางการเดิน ถ้าทิศทางไปทางซ้ายให้พลิกทิศทาง
        if (direction.x > 0 && transform.localScale.x < 0)
        {
            // เดินไปขวา (พลิกกลับ)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0 && transform.localScale.x > 0)
        {
            // เดินไปซ้าย (พลิกกลับ)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private IEnumerator WaitBeforeMoving()
    {
        // ตั้งค่าให้ AI หยุดนิ่ง
        animator.SetBool("isWalking", false); // หยุดแอนิเมชันการเดิน
        isWaiting = true; // ตั้งค่าให้ AI หยุดนิ่ง
        yield return new WaitForSeconds(idleTime); // รอเวลาที่กำหนด
        isWaiting = false; // AI พร้อมที่จะเดินไปทางใหม่
        SetNewTargetPosition(); // ตั้งตำแหน่งใหม่ให้ AI
    }

    public void TakeHit()
    {
        animator.SetTrigger("TakeHitTrigger");
    }

    public void Shield()
    {
        animator.SetTrigger("ShieldTrigger");
    }

    public void Die()
    {
        isDead = true;
        animator.SetTrigger("DeathTrigger");
        animator.SetBool("isWalking", false); // เมื่อ AI ตาย ให้หยุดเดิน
    }
}
