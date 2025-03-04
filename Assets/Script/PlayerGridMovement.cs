using UnityEngine;

public class PlayerGridMovement : MonoBehaviour
{
    public Vector2 gridSize = new Vector2(1, 1); // ขนาดของ Grid (1x1 เซลล์)
    private Vector2 playerPosition;

    void Start()
    {
        // กำหนดตำแหน่งเริ่มต้นของผู้เล่น
        playerPosition = new Vector2(transform.position.x, transform.position.y);
    }

    void Update()
    {
        // คำนวณตำแหน่งใน Grid
        Vector2 gridPosition = new Vector2(Mathf.Floor(playerPosition.x / gridSize.x), Mathf.Floor(playerPosition.y / gridSize.y));
        Debug.Log("ตำแหน่งผู้เล่นใน Grid: " + gridPosition);

        // คำนวณการกระโดด (จำลองการกระโดดบน Grid)
        if (IsGrounded())
        {
            Jump();
        }
    }

    // ฟังก์ชันตรวจสอบว่าอยู่บนพื้นหรือไม่
    bool IsGrounded()
    {
        // ตรวจสอบการชนกับพื้น (ใช้ Raycast หรือ Trigger)
        return true; // แค่ตัวอย่าง
    }

    void Jump()
    {
        // คำนวณการกระโดดจากตำแหน่งใน Grid
        float jumpHeight = 2.0f;
        playerPosition = new Vector2(playerPosition.x, playerPosition.y + jumpHeight);
        Debug.Log("กระโดดขึ้นจาก Grid: " + playerPosition);
    }
}
