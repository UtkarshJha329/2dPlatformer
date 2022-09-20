using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public float gravityScale = 3;
    public float movementSpeed = 2.0f;

    public float jumpHeight = 3.0f;
    public float jumpHalfTime = 0.5f;

    public float jumpVelocity = 0.0f;

    public int totalNumberOfJumps = 1;
    private int numberOfJumpsRemaining = 0;

    public Transform groundCheckPos;
    public Transform ceilingCheckPos;

    public float groundCheckDistance = 0.1f;
    public float ceilingCheckDistance = 0.1f;
    public LayerMask groundLayer;

    public int numberOfGroundCheckRays = 6;
    public int numberOfCeilingCheckRays = 6;
    public float distanceBetweenRays = 0.2f;

    public Transform attackPositionRight;
    public Transform attackPositionLeft;
    public float attackCheckRadius = 0.5f;
    public int numberOfAttackCheckRays = 6;
    public float distanceBetweenAttackRays = 0.2f;
    public float distanceCheckAttack = 1.0f;

    public float attackingAnimTime = 0.3f;
    private float attackTimeRemaining = 0.3f;
    private bool attacking = false;

    public bool wasHit = false;
    public float hitTime = 0.3f;
    private float remainingHitTime = 0.0f;
    public float invinciblityTime = 1.0f;
    private float remainingInvincibleTime = 0.0f;
    public bool invincible = false;

    public int health = 3;
    public bool isDead = false;

    private bool inAir = false;

    private Rigidbody2D rb2d;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = gameObject.GetComponent<Rigidbody2D>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();

        rb2d.gravityScale = gravityScale;

        jumpVelocity = (jumpHeight / jumpHalfTime) - (Physics2D.gravity.y * gravityScale * jumpHalfTime) * 0.5f;

        attackTimeRemaining = attackingAnimTime;
        remainingInvincibleTime = invinciblityTime;
        remainingHitTime = hitTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0)
        {
            isDead = true;
            animator.SetBool("isDead", true);
        }

        if (!isDead)
        {
            //Check if touching the ground to reset total number of jumps remaining.
            for (int i = 0; i < numberOfGroundCheckRays; i++)
            {
                Vector3 castPosition = new Vector3(groundCheckPos.position.x - distanceBetweenRays * i, groundCheckPos.position.y, groundCheckPos.position.z);

                Debug.DrawRay(castPosition, Vector3.down * groundCheckDistance, Color.red);
                if (Physics2D.Raycast(castPosition, Vector2.down, groundCheckDistance, groundLayer))
                {
                    numberOfJumpsRemaining = totalNumberOfJumps;
                    inAir = false;
                    break;
                }
                else
                {
                    inAir = true;
                }
            }

            //Calculate velocity both horizontal from input and vertical velocity
            Vector3 velocity = new Vector3(Input.GetAxisRaw("Horizontal") * movementSpeed, rb2d.velocity.y, 0.0f);

            if (Input.GetKeyDown(KeyCode.UpArrow) && numberOfJumpsRemaining > 0)
            {
                velocity.y = jumpVelocity;
                numberOfJumpsRemaining--;
            }

            //Check if touching ceiling, if yes set vertical velocity to 0.0f
            for (int i = 0; i < numberOfCeilingCheckRays; i++)
            {
                Vector3 castPosition = new Vector3(ceilingCheckPos.position.x - distanceBetweenRays * i, ceilingCheckPos.position.y, ceilingCheckPos.position.z);

                Debug.DrawRay(castPosition, Vector3.up * ceilingCheckDistance, Color.red);
                if (Physics2D.Raycast(castPosition, Vector2.up, ceilingCheckDistance, groundLayer))
                {
                    velocity.y = 0.0f;
                    break;
                }
            }

            //If not holding down vertical input, set vertical velocity to 0.
            if (velocity.y > 0 && !Input.GetKey(KeyCode.UpArrow))
                velocity.y = 0.0f;

            if (invincible && remainingInvincibleTime > 0)
            {
                remainingInvincibleTime -= Time.deltaTime;
                animator.SetBool("isInvincible", true);
            }
            else if (invincible && remainingInvincibleTime <= 0.0f)
            {
                invincible = false;
                remainingInvincibleTime = invinciblityTime;
                animator.SetBool("isInvincible", false);
            }

            //Set Rigidbody velocity to calculated velocity.
            if (wasHit && remainingHitTime > 0.0f)
            {
                remainingHitTime -= Time.deltaTime;
                rb2d.velocity = Vector3.zero;
                //Debug.Log("Handeling hit.");
                if (!invincible)
                {
                    health--;
                }
                invincible = true;

                animator.SetBool("wasHit", true);
            }
            else if (wasHit && remainingHitTime <= 0.0f)
            {
                //Debug.Log("Handeled hit.");
                wasHit = false;
                remainingHitTime = hitTime;
                animator.SetBool("wasHit", false);
            }
            else
            {
                rb2d.velocity = velocity;
            }

            //Attack if Spacebar is pressed.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AttackChecking();
                animator.SetBool("isAttacking", true);
                attacking = true;
                attackTimeRemaining = attackingAnimTime;
            }
            else if (attacking && attackTimeRemaining <= 0.0f)
            {
                animator.SetBool("isAttacking", false);
                attacking = false;
            }
            else if (attacking)
            {
                attackTimeRemaining -= Time.deltaTime;
            }

            //Animation Variables handeling below

            //Flip sprite direction depending on movement direciton. So sprite is always looking in the direction of movement.
            if (velocity.x != 0)
            {
                spriteRenderer.flipX = (velocity.x < 0);
            }

            //Set speed value in animator, to change animation.
            animator.SetFloat("speed", Mathf.Abs(velocity.x));

            //Set isFalling value in animator to true if falling.
            animator.SetBool("isFalling", (velocity.y < -0.1f));

            //Set isJumping value in animator to true if jumping.
            animator.SetBool("isJumping", (velocity.y > 0.1f));
        }

    }

    private void AttackChecking()
    {
        for (int i = 0; i < numberOfAttackCheckRays; i++)
        {
            Vector3 currentAttackPosition = spriteRenderer.flipX ? attackPositionLeft.position : attackPositionRight.position;
            Vector2 currentAttackDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;

            float delta = spriteRenderer.flipX ? 0.1f : -0.1f;

            currentAttackPosition -= new Vector3(delta, i * distanceBetweenAttackRays, 0.0f);

            RaycastHit2D hitInfo = Physics2D.Raycast(currentAttackPosition, currentAttackDirection, distanceCheckAttack);
            
            Debug.DrawRay(currentAttackPosition, currentAttackDirection * distanceCheckAttack, Color.red);

            if(hitInfo.collider != null)
            {
                //Debug.Log(hitInfo.collider.gameObject.name);
                if (hitInfo.collider.CompareTag("Enemy"))
                {
                    EnemyController enemyController = hitInfo.collider.GetComponent<EnemyController>();
                    if (!enemyController.invincible)
                    {
                        enemyController.wasHit = true;
                    }
                }
            }
        }

    }
}
