using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private float groundCheckRadius = 0.15f;

    [SerializeField]
    private float groundCheckOffset = 0.05f;

    public bool isGrounded { get; private set; }

    private Collider2D col;
    private Rigidbody2D rb;

    private Vector2 groundCheckPos => CalculateGroundCheckPos();

    // Foot position helper function to calculate the ground check position
    // based on the collider's bounds.
    private Vector2 CalculateGroundCheckPos()
    {
        Bounds bounds = col.bounds;

        // Move the checking circle slightly below the player's collider.
        return new Vector2(
            bounds.center.x,
            bounds.min.y - groundCheckOffset
        );
    }

    public void Init(Collider2D col, Rigidbody2D rb)
    {
        this.col = col;
        this.rb = rb;
    }

    // Update is called once per frame.
    // This method is called from PlayerController.
    public bool CheckGround()
    {
        if (col == null || rb == null)
        {
            isGrounded = false;
            return false;
        }

        Collider2D groundHit = Physics2D.OverlapCircle(
            groundCheckPos,
            groundCheckRadius,
            groundLayer
        );

        // The player is grounded only when touching ground
        // and not moving upward.
        isGrounded =
            groundHit != null &&
            rb.linearVelocityY <= 0.05f;

        return isGrounded;
    }

    // Draws the ground-check circle in the Scene window.
    private void OnDrawGizmosSelected()
    {
        Collider2D currentCollider = col;

        if (currentCollider == null)
        {
            currentCollider = GetComponent<Collider2D>();
        }

        if (currentCollider == null)
        {
            return;
        }

        Bounds bounds = currentCollider.bounds;

        Vector2 checkPosition = new Vector2(
            bounds.center.x,
            bounds.min.y - groundCheckOffset
        );

        Gizmos.DrawWireSphere(
            checkPosition,
            groundCheckRadius
        );
    }
}