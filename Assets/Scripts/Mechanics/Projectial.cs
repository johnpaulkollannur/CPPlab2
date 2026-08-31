using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    #region Projectile Settings

    [SerializeField]
    private float lifetime = 10f;

    // How long the snowball takes to fade
    // from 100% opacity to 0%.
    [SerializeField]
    private float groundFadeDuration = 1.5f;

    // How much horizontal speed remains
    // when the snowball touches the ground.
    [SerializeField]
    private float groundSpeedMultiplier = 0.5f;

    // Floor / Edge / Roof layer.
    [SerializeField]
    private LayerMask groundLayer;

    #endregion


    #region Component References

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    #endregion


    #region Projectile State

    // Prevent ground behaviour from starting
    // multiple times.
    private bool touchedGround = false;

    // Stores the fade coroutine.
    private Coroutine fadeCoroutine;

    #endregion


    // --------------------------------------------------
    // START
    // --------------------------------------------------

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();


        // If the SpriteRenderer happens to be
        // on a child object, find it there.
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }


        // Helps fast projectiles detect collisions.
        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;
    }


    // Start is called once before the first execution
    // of Update after the MonoBehaviour is created.
    private void Start()
    {
        // Safety:
        // If the projectile never hits anything,
        // destroy it after the lifetime.
        Destroy(
            gameObject,
            lifetime
        );
    }


    // --------------------------------------------------
    // PROJECTILE MOVEMENT
    // --------------------------------------------------

    /// <summary>
    /// Gives the projectile its starting velocity.
    /// Called by the Shoot script.
    /// </summary>
    public void SetVelocity(Vector2 velocity)
    {
        rb.linearVelocity = velocity;
    }


    // --------------------------------------------------
    // NORMAL COLLISION
    // --------------------------------------------------

    // collision detection functions -
    // one of the two colliding bodies has to be
    // a dynamic rigidbody for these functions
    // to be called.
    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        GameObject otherObject =
            collision.gameObject;


        Debug.Log(
            "Projectile hit: " +
            otherObject.name +
            " | Layer: " +
            LayerMask.LayerToName(otherObject.layer) +
            " | Tag: " +
            otherObject.tag
        );


        // --------------------------------------------------
        // LAB #4 EXCEPTIONS
        // --------------------------------------------------

        // Projectile should NOT be destroyed when
        // touching:
        //
        // Player
        // Collectible
        // PowerUp
        if (ShouldIgnoreObject(otherObject))
        {
            return;
        }


        // --------------------------------------------------
        // GROUND
        // --------------------------------------------------

        // Floor / Edge / Roof:
        //
        // Roll + fade instead of disappearing immediately.
        if (IsGround(otherObject))
        {
            HandleGroundCollision();

            return;
        }


        // --------------------------------------------------
        // ALREADY ON GROUND
        // --------------------------------------------------

        // If the snowball has already landed and
        // started fading, do not destroy it because
        // it touches another nearby ground collider.
        if (touchedGround)
        {
            return;
        }


        // --------------------------------------------------
        // EVERYTHING ELSE
        // --------------------------------------------------

        // Walls, enemies and other scene objects
        // destroy the projectile immediately.
        Debug.Log(
            "Projectile destroyed after hitting: " +
            otherObject.name
        );


        Destroy(gameObject);
    }


    private void OnCollisionExit2D(
        Collision2D collision
    )
    {
        // Not required for Lab #4.
        // Leaving this here for reference.
    }


    private void OnCollisionStay2D(
        Collision2D collision
    )
    {
        // Not required for Lab #4.
        // Leaving this here for reference.
    }


    // --------------------------------------------------
    // GROUND / SNOWBALL BEHAVIOUR
    // --------------------------------------------------

    private void HandleGroundCollision()
    {
        // Only start ground behaviour once.
        if (touchedGround)
        {
            return;
        }


        touchedGround = true;


        Debug.Log(
            "Snowball landed - rolling and fading."
        );


        // Slow the snowball when it lands.
        rb.linearVelocity =
            new Vector2(
                rb.linearVelocity.x *
                groundSpeedMultiplier,
                0f
            );


        // Gradually slow down while rolling.
        rb.linearDamping = 2f;


        // Rotate while rolling.
        rb.angularVelocity =
            -rb.linearVelocity.x * 120f;


        // Start fading instead of using:
        //
        // Destroy(gameObject, groundDestroyDelay);
        //
        // OLD:
        // projectile stays visible at 100%
        // then suddenly disappears.
        //
        // NEW:
        // 100% → 0% opacity gradually.

        if (fadeCoroutine == null)
        {
            fadeCoroutine =
                StartCoroutine(
                    FadeAndDestroy()
                );
        }
    }


    // --------------------------------------------------
    // FADE
    // --------------------------------------------------

    private IEnumerator FadeAndDestroy()
    {
        // Safety check.
        if (spriteRenderer == null)
        {
            Destroy(gameObject);

            yield break;
        }


        // Get the original sprite colour.
        Color originalColor =
            spriteRenderer.color;


        float elapsedTime = 0f;


        // Start at 100% opacity.
        originalColor.a = 1f;

        spriteRenderer.color =
            originalColor;


        // Gradually fade:
        //
        // 100%
        // ↓
        // 75%
        // ↓
        // 50%
        // ↓
        // 25%
        // ↓
        // 0%

        while (elapsedTime < groundFadeDuration)
        {
            elapsedTime +=
                Time.deltaTime;


            float fadeAmount =
                Mathf.Clamp01(
                    elapsedTime /
                    groundFadeDuration
                );


            Color newColor =
                spriteRenderer.color;


            newColor.a =
                Mathf.Lerp(
                    1f,
                    0f,
                    fadeAmount
                );


            spriteRenderer.color =
                newColor;


            yield return null;
        }


        // Make absolutely sure opacity reaches 0.
        Color finalColor =
            spriteRenderer.color;

        finalColor.a = 0f;

        spriteRenderer.color =
            finalColor;


        // Now destroy the invisible projectile.
        Destroy(gameObject);
    }


    // --------------------------------------------------
    // TRIGGER COLLISION
    // --------------------------------------------------

    // collision detection functions for trigger colliders -
    // useful for pickups such as the PowerUp.
    private void OnTriggerEnter2D(
        Collider2D collision
    )
    {
        GameObject otherObject =
            collision.gameObject;


        Debug.Log(
            "Projectile trigger touched: " +
            otherObject.name +
            " | Tag: " +
            otherObject.tag
        );


        // Ignore Player, Collectible and PowerUp.
        if (ShouldIgnoreObject(otherObject))
        {
            return;
        }


        // If this trigger is Ground,
        // roll and fade.
        if (IsGround(otherObject))
        {
            HandleGroundCollision();

            return;
        }


        // If already fading after landing,
        // don't suddenly destroy it.
        if (touchedGround)
        {
            return;
        }


        // Other trigger objects destroy projectile.
        Destroy(gameObject);
    }


    private void OnTriggerExit2D(
        Collider2D collision
    )
    {
        // Not currently required.
    }


    private void OnTriggerStay2D(
        Collider2D collision
    )
    {
        // Not currently required.
    }


    // --------------------------------------------------
    // LAB #4 COLLISION FILTER
    // --------------------------------------------------

    private bool ShouldIgnoreObject(
        GameObject otherObject
    )
    {
        // Player
        if (otherObject.CompareTag("Player"))
        {
            return true;
        }


        // Collectible
        if (otherObject.CompareTag("Collectible"))
        {
            return true;
        }


        // PowerUp
        if (otherObject.CompareTag("PowerUp"))
        {
            return true;
        }


        return false;
    }


    // --------------------------------------------------
    // GROUND CHECK
    // --------------------------------------------------

    private bool IsGround(
        GameObject otherObject
    )
    {
        // First check the Ground layer.
        bool isOnGroundLayer =
            (groundLayer.value &
            (1 << otherObject.layer)) != 0;


        if (isOnGroundLayer)
        {
            return true;
        }


        // Backup for your existing objects:
        //
        // Floor
        // Floor (1)
        // Floor (2)
        // Edge
        // Edge (1)
        // etc.

        if (
            otherObject.name.StartsWith("Floor") ||
            otherObject.name.StartsWith("Edge")
        )
        {
            return true;
        }


        return false;
    }
}