using System.Collections;
using UnityEngine;

/// <summary>
/// Responsible for taking input and applying it to the rigidbody component
/// of the player object.
/// </summary>
[RequireComponent(
    typeof(Rigidbody2D),
    typeof(Collider2D),
    typeof(SpriteRenderer)
)]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(GroundCheck))]
[RequireComponent(typeof(Shoot))]
public class PlayerController : MonoBehaviour
{
    #region Tunable Variables

    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float jumpForce = 5f;

    [SerializeField]
    private int maxJumpCount = 2;

    // OLD VALUE:
    // Previously this was used to manually control
    // how long the attack animation played.
    //
    // We now check the actual Attack animation instead.
    [SerializeField]
    private float attackDuration = 0.6f;

    #endregion


    #region Component References

    // private and public - public variables can be accessed from other scripts,
    // private variables cannot - within unity, public variables are also visible
    // in the inspector, private variables are not - by default, variables are
    // private unless specified otherwise

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private Animator anim;
    private GroundCheck check;
    private Shoot shoot;

    #endregion


    #region Player State

    // Keeps track of how many times the player has jumped.
    private int jumpCount = 0;

    // Stores whether the player is currently touching the ground.
    private bool isGrounded = false;

    // Stores the grounded value from the previous frame.
    // This allows us to detect when the player has actually landed.
    private bool wasGrounded = false;

    // Bool used by the Animator for the jump animation.
    private bool isJumping = false;

    // Bool used to stop movement while attacking.
    private bool isAttacking = false;

    // Stores the attack coroutine so another attack
    // cannot start before the first one finishes.
    private Coroutine attackCoroutine;

    #endregion


    #region Start

    // Start is called once before the first execution of Update
    // after the MonoBehaviour is created.
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        check = GetComponent<GroundCheck>();
        shoot = GetComponent<Shoot>();


        // Initialize the GroundCheck.
        check.Init(col, rb);


        // Start player with no velocity.
        rb.linearVelocity = Vector2.zero;


        if (shoot == null)
        {
            Debug.LogError(
                "PlayerController: Shoot component is missing from the player."
            );
        }


        // Starting states.
        isJumping = false;
        isAttacking = false;


        // Reset Animator values.
        anim.SetBool("Jump", false);
        anim.SetBool("Attack", false);
        anim.SetBool("isGrounded", false);

        // IMPORTANT:
        // Animator parameter is named exactly "horizontal".
        anim.SetFloat("horizontal", 0f);


        // stale code but leaving it around so that you can reference
        // how to create a gameobject directly through code
        // and then how to parent it to another game object

        // if (groundCheckTransform == null)
        // {
        //     Debug.LogError(
        //         "Ground check transform is not assigned in the inspector."
        //     );

        //     groundCheckTransform =
        //         new GameObject("GroundCheck").transform;

        //     groundCheckTransform.SetParent(transform);
        //     groundCheckTransform.localPosition = Vector3.zero;
        // }
    }

    #endregion


    #region Update

    // Update is called once per frame.
    private void Update()
    {
        CheckGrounded();


        // A / Left Arrow  = -1
        // D / Right Arrow =  1
        // No movement     =  0
        float horizontalInput =
            Input.GetAxisRaw("Horizontal");


        HandleAttack();

        HandleStun();

        HandleJump();

        HandleMovement(
            ref horizontalInput
        );

        UpdateAnimations(
            horizontalInput
        );
    }

    #endregion


    #region Ground

    private void CheckGrounded()
    {
        // Save the old grounded state.
        wasGrounded = isGrounded;


        // Check the ground.
        isGrounded =
            check.CheckGround();


        // If player just landed,
        // reset the jump counter.
        if (
            isGrounded &&
            !wasGrounded
        )
        {
            jumpCount = 0;
        }


        // Player is jumping when not grounded.
        //
        // Attack overrides Jump animation.
        isJumping =
            !isGrounded &&
            !isAttacking;
    }

    #endregion


    #region Movement

    private void HandleMovement(
        ref float horizontalInput
    )
    {
        // Original horizontal movement:
        //
        // float moveX = horizontalInput * speed;
        // rb.linearVelocityX = moveX;


        // Do not move horizontally while attacking.
        if (isAttacking)
        {
            horizontalInput = 0f;

            rb.linearVelocityX = 0f;

            return;
        }


        float moveX =
            horizontalInput * speed;


        rb.linearVelocityX =
            moveX;


        SpriteFlip(
            horizontalInput
        );
    }


    private void SpriteFlip(
        float horizontalInput
    )
    {
        // Flip only when player is actually moving.

        if (horizontalInput < -0.01f)
        {
            // LEFT
            sr.flipX = true;
        }

        else if (horizontalInput > 0.01f)
        {
            // RIGHT
            sr.flipX = false;
        }
    }

    #endregion


    #region Jump

    private void HandleJump()
    {
        // Original jump input:
        //
        // Input.GetButtonDown("Jump")
        //
        // Space is used directly so J
        // cannot accidentally activate Jump.

        if (
            Input.GetKeyDown(KeyCode.Space) &&
            !isAttacking &&
            jumpCount < maxJumpCount
        )
        {
            jumpCount++;


            isGrounded = false;

            isJumping = true;


            // Remove existing vertical movement.
            rb.linearVelocityY = 0f;


            // Apply jump.
            rb.AddForceY(
                jumpForce,
                ForceMode2D.Impulse
            );


            Debug.Log(
                "Jump Count: " +
                jumpCount +
                " Max Jumps: " +
                maxJumpCount
            );
        }
    }

    #endregion


    #region Attack

    private void HandleAttack()
    {
        // Original attack system used an Animator Trigger:
        //
        // if (Input.GetKeyDown(KeyCode.J))
        // {
        //     anim.ResetTrigger("Stun");
        //     anim.SetTrigger("Attack");
        // }


        // Previous direct shooting:
        //
        // shoot.Fire();
        //
        // DO NOT call Fire() here.
        //
        // Fire() is already called using the
        // Animation Event inside the Attack animation.
        //
        // If Fire() is called here AND by the
        // Animation Event, two projectiles are created.


        if (
            Input.GetKeyDown(KeyCode.J) &&
            !isAttacking
        )
        {
            StartAttack();
        }
    }


    private void StartAttack()
    {
        // Player is now attacking.
        isAttacking = true;


        // Attack overrides Jump animation.
        isJumping = false;


        // Stop horizontal movement.
        //
        // We are NOT changing vertical velocity.
        rb.linearVelocityX = 0f;


        // Walking animation must stop.
        anim.SetFloat(
            "horizontal",
            0f
        );


        // Jump animation must stop.
        anim.SetBool(
            "Jump",
            false
        );


        // Set attack parameter.
        anim.SetBool(
            "Attack",
            true
        );


        // DIRECTLY start the Attack animation.
        //
        // This means you DO NOT need:
        //
        // Any State → Attack
        //
        // in the Animator.
        anim.Play(
            "Attack",
            0,
            0f
        );


        // Stop an old coroutine if somehow one exists.
        if (attackCoroutine != null)
        {
            StopCoroutine(
                attackCoroutine
            );
        }


        // Start checking the actual Attack animation.
        attackCoroutine =
            StartCoroutine(
                WaitForAttackToFinish()
            );
    }


    private IEnumerator WaitForAttackToFinish()
    {
        // Wait one frame so Animator has time
        // to enter the Attack state.
        yield return null;


        // Wait until Animator actually reaches Attack.
        while (
            !anim.GetCurrentAnimatorStateInfo(0)
                 .IsName("Attack")
        )
        {
            yield return null;
        }


        // Now wait for the Attack animation
        // to reach its end.
        //
        // normalizedTime:
        //
        // 0.0 = beginning
        // 0.5 = halfway
        // 1.0 = finished
        while (
            anim.GetCurrentAnimatorStateInfo(0)
                .IsName("Attack") &&
            anim.GetCurrentAnimatorStateInfo(0)
                .normalizedTime < 1f
        )
        {
            yield return null;
        }


        // Attack animation has finished.
        EndAttack();
    }


    private void EndAttack()
    {
        // Player is no longer attacking.
        isAttacking = false;


        // Turn Attack Animator parameter off.
        anim.SetBool(
            "Attack",
            false
        );


        // Read horizontal input immediately.
        //
        // This helps walking start again
        // immediately after the attack.
        float horizontalInput =
            Input.GetAxisRaw("Horizontal");


        float animationSpeed =
            Mathf.Abs(horizontalInput);


        if (animationSpeed < 0.1f)
        {
            animationSpeed = 0f;
        }


        // Restore walking parameter.
        anim.SetFloat(
            "horizontal",
            animationSpeed
        );


        attackCoroutine = null;
    }


    /*
    // OLD ATTACK ROUTINE
    //
    // Previously the attack waited for a manually
    // entered amount of time.
    //
    // This could cause the animation to stop too early,
    // including before the projectile Animation Event.
    //
    // We now check the actual animation instead.

    private IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(
            attackDuration
        );

        isAttacking = false;

        anim.SetBool(
            "Attack",
            false
        );

        attackCoroutine = null;
    }
    */

    #endregion


    #region Stun

    private void HandleStun()
    {
        // K plays Stun.

        if (Input.GetKeyDown(KeyCode.K))
        {
            // Cancel attack coroutine if necessary.
            if (attackCoroutine != null)
            {
                StopCoroutine(
                    attackCoroutine
                );

                attackCoroutine = null;
            }


            // Reset states.
            isAttacking = false;

            isJumping = false;


            anim.SetBool(
                "Attack",
                false
            );


            anim.SetBool(
                "Jump",
                false
            );


            anim.SetTrigger(
                "Stun"
            );


            rb.linearVelocityX = 0f;
        }
    }

    #endregion


    #region Animation

    private void UpdateAnimations(
        float horizontalInput
    )
    {
        // Grounded.
        anim.SetBool(
            "isGrounded",
            isGrounded
        );


        // Jump.
        anim.SetBool(
            "Jump",
            isJumping &&
            !isAttacking
        );


        // Attack.
        anim.SetBool(
            "Attack",
            isAttacking
        );


        // Original movement animation:
        //
        // anim.SetFloat(
        //     "horizontalInput",
        //     Mathf.Abs(horizontalInput)
        // );


        // Left input gives -1.
        // Right input gives +1.
        //
        // Mathf.Abs makes both become 1.
        float animationSpeed =
            Mathf.Abs(horizontalInput);


        if (animationSpeed < 0.1f)
        {
            animationSpeed = 0f;
        }


        // Do not allow Walk during Attack.
        if (isAttacking)
        {
            animationSpeed = 0f;
        }


        // CORRECT parameter:
        anim.SetFloat(
            "horizontal",
            animationSpeed
        );
    }

    #endregion
}