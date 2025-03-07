using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public float moveSpeed = 2f; // ความเร็วในการเดินของศัตรู
    public float detectionRadius = 5f; // ระยะการตรวจจับของศัตรู
    public float changeDirectionTime = 2f; // เวลาที่ AI จะเปลี่ยนทิศทาง (หน่วยเป็นวินาที)
    public float idleTime = 1f; // เวลาที่ AI จะหยุดนิ่งก่อนที่จะเดินไปทางใหม่

    private Animator animator; // ตัวแปรเก็บ Animator ของศัตรู
    private Vector2 targetPosition; // ตำแหน่งที่ AI จะเดินไป
    private bool isDead = false; // ตัวแปรเช็คว่า AI ตายหรือไม่
    private float minX; // ขอบเขตต่ำสุด X
    private float maxX; // ขอบเขตสูงสุด X
    private bool isWaiting = false; // เช็คว่า AI กำลังหยุดนิ่งหรือไม่

    private void Start()
    {
        animator = GetComponent<Animator>(); // รับ Animator ของศัตรู
        // ตั้งค่าขอบเขต X โดยใช้ตำแหน่งของ AI เป็นจุดกลาง
        minX = transform.position.x - 5f; // ขอบเขตต่ำสุดในแกน X
        maxX = transform.position.x + 5f; // ขอบเขตสูงสุดในแกน X
        SetNewTargetPosition(); // กำหนดตำแหน่งเป้าหมายเริ่มต้น
        InvokeRepeating("SetNewTargetPosition", changeDirectionTime, changeDirectionTime); // ตั้งเวลาการเปลี่ยนทิศทาง
    }

    private void Update()
    {
        if (isDead)
            return;

        // หาก AI กำลังหยุดนิ่ง, อย่าทำอะไร
        if (isWaiting)
            return;

        // เดินไปยังตำแหน่งเป้าหมาย
        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        // คำนวณทิศทางและเดินไปยังตำแหน่งเป้าหมายในแนวแกน X เท่านั้น
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // เปลี่ยนสถานะเป็นเดิน (Walk)
        animator.SetBool("isWalking", true); // ตั้งค่า isWalking เป็น true เมื่อ AI กำลังเคลื่อนไหว

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
