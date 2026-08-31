using UnityEngine;

public class Shoot : MonoBehaviour
{
    #region Component References

    // SpriteRenderer is used to determine
    // which direction the player is facing.
    private SpriteRenderer sr;

    // The player's collider is used so the projectile
    // can ignore collision with the player who fired it.
    private Collider2D playerCollider;

    #endregion

    #region Shooting Variables

    [SerializeField]
    private Vector2 initShotVelocity = new Vector2(5f, 5f);

    [SerializeField]
    private Transform spawnPointLeft;

    [SerializeField]
    private Transform spawnPointRight;

    [SerializeField]
    private Projectile projectilePrefab;

    #endregion

    // Start is called once before the first execution of Update
    // after the MonoBehaviour is created.
    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();

        if (sr == null)
        {
            Debug.LogError(
                "Shoot: No SpriteRenderer was found on the player."
            );
        }

        if (playerCollider == null)
        {
            Debug.LogError(
                "Shoot: No Collider2D was found on the player."
            );
        }

        if (initShotVelocity == Vector2.zero)
        {
            initShotVelocity = new Vector2(5f, 5f);

            Debug.LogWarning(
                "Shoot: InitShotVelocity not defined - " +
                "setting it to the default value 5, 5."
            );
        }

        if (spawnPointLeft == null ||
            spawnPointRight == null ||
            projectilePrefab == null)
        {
            Debug.LogError(
                "Shoot: One or more spawn points or the projectile prefab " +
                "is not assigned. All references must be assigned."
            );
        }
    }

    // Update is called once per frame.
    //
    // This script does not require Update because Fire()
    // is called from the Attack Animation Event.

    // This public method is called once by the Attack Animation Event.
    public void Fire()
    {
        Debug.Log("Shoot.Fire was called by the Animation Event.");

        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
        }

        if (playerCollider == null)
        {
            playerCollider = GetComponent<Collider2D>();
        }

        if (sr == null)
        {
            Debug.LogError(
                "Fire failed because no SpriteRenderer was found."
            );

            return;
        }

        if (spawnPointLeft == null ||
            spawnPointRight == null ||
            projectilePrefab == null)
        {
            Debug.LogError(
                "Fire failed because Spawn Point Left, Spawn Point Right, " +
                "or Projectile Prefab is not assigned."
            );

            return;
        }

        Projectile currentProjectile;

        if (!sr.flipX)
        {
            // Player is facing right.
            currentProjectile = Instantiate(
                projectilePrefab,
                spawnPointRight.position,
                Quaternion.identity
            );

            Vector2 rightVelocity = new Vector2(
                Mathf.Abs(initShotVelocity.x),
                initShotVelocity.y
            );

            currentProjectile.SetVelocity(rightVelocity);
        }
        else
        {
            // Player is facing left.
            currentProjectile = Instantiate(
                projectilePrefab,
                spawnPointLeft.position,
                Quaternion.identity
            );

            Vector2 leftVelocity = new Vector2(
                -Mathf.Abs(initShotVelocity.x),
                initShotVelocity.y
            );

            currentProjectile.SetVelocity(leftVelocity);
        }

        // Prevent the projectile from physically pushing the player.
        Collider2D projectileCollider =
            currentProjectile.GetComponent<Collider2D>();

        if (playerCollider != null &&
            projectileCollider != null)
        {
            Physics2D.IgnoreCollision(
                playerCollider,
                projectileCollider,
                true
            );
        }
    }
}