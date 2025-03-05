using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    private int collisionCount = 0;
    private float disableTimer;

    public bool IsGrounded()
    {
        if (disableTimer > 0)
        {
            Debug.Log("[GroundSensor] ❌ Disabled: " + disableTimer.ToString("F2") + "s remaining");
            return false;
        }

        bool grounded = collisionCount > 0;
        Debug.Log("[GroundSensor] ✅ IsGrounded: " + grounded);
        return grounded;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        collisionCount++;
        Debug.Log("[GroundSensor] 🟢 Entered collision with: " + other.gameObject.name + " | Total Collisions: " + collisionCount);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        collisionCount--;
        Debug.Log("[GroundSensor] 🔴 Exited collision with: " + other.gameObject.name + " | Total Collisions: " + collisionCount);
    }

    void Update()
    {
        if (disableTimer > 0)
        {
            disableTimer -= Time.deltaTime;
            Debug.Log("[GroundSensor] ⏳ Disable Timer: " + disableTimer.ToString("F2") + "s");
        }
    }

    public void Disable(float duration)
    {
        disableTimer = duration;
        Debug.Log("[GroundSensor] ❌ Disabled for " + duration + "s");
    }
}
