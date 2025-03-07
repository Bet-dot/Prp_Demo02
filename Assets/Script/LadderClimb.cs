using UnityEngine;

public class LadderClimb : MonoBehaviour
{
    public float climbSpeed = 5f; // ความเร็วในการปีน
    public LayerMask ladderLayer; // กำหนด Layer ของบันได

    private bool isClimbing = false;
    private Rigidbody2D rb;
    private float gravityScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
    }

    void Update()
    {
        if (isClimbing)
        {
            float vertical = Input.GetAxisRaw("Vertical"); // รับอินพุตการปีน
            rb.velocity = new Vector2(rb.velocity.x, vertical * climbSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & ladderLayer) != 0) // ตรวจจับ Layer ของบันได
        {
            isClimbing = true;
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & ladderLayer) != 0)
        {
            isClimbing = false;
            rb.gravityScale = gravityScale;
        }
    }
}
