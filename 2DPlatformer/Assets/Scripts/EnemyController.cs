using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public bool wasHit = false;

    public float hitAnimationTime = 0.3f;
    private float hitAnimationRemainingTime = 0.0f;

    public Transform groundCheckPos;
    public Transform obstacleCheckPosRight;
    public Transform obstacleCheckPosLeft;

    public float groundCheckDistance = 0.2f;
    public float numberOfGroundCheckRays = 6;
    public float distanceBetweenRays = 0.2f;
    public float obstacleCheckDistance = 0.2f;
    public float numberOfObstacleCheckRays = 6;
    public float distanceBetweenObstacleRays = 0.2f;
    public LayerMask groundLayer;

    public Transform[] wayPoints = new Transform[2];
    private int currentWayPointIndex = 0;

    public float checkInAirTime = 0.2f;
    private float remainingCheckInAirTime = 0.2f;

    public float movementSpeed = 2.0f;

    public bool invincible = false;
    public float invinciblityTime = 1.0f;
    private float remainingInvincibleTime = 1.0f;

    public float attackingTime = 0.3f;
    private float remainingAttackingTime = 0.3f;
    public bool attacking = false;

    public int health = 2;
    public bool isDead = false;

    private bool pauseMovement = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        remainingInvincibleTime = invinciblityTime;
        remainingAttackingTime = attackingTime;
    }

    // Update is called once per frame
    void Update()
    {
        spriteRenderer.size = new Vector2(1, 1);

        if(health <= 0)
        {
            isDead = true;
            animator.SetBool("isDead", true);
        }

        if (!isDead)
        {
            if (remainingCheckInAirTime <= 0)
            {
                for (int i = 0; i < numberOfGroundCheckRays; i++)
                {
                    Vector3 castPosition = new Vector3(groundCheckPos.position.x - distanceBetweenRays * i, groundCheckPos.position.y, groundCheckPos.position.z);

                    Debug.DrawRay(castPosition, Vector3.down * groundCheckDistance, Color.red);
                    RaycastHit2D hitInfo = Physics2D.Raycast(castPosition, Vector2.down, groundCheckDistance, groundLayer);
                    if (hitInfo.collider == null)
                    {
                        Debug.Log("Out of path.");
                        currentWayPointIndex = (currentWayPointIndex == 0) ? 1 : 0;
                        remainingCheckInAirTime = checkInAirTime;
                        break;
                    }
                    else
                    {
                        //Debug.Log(hitInfo.collider.gameObject.name);
                    }
                }
            }
            else
            {
                remainingCheckInAirTime -= Time.deltaTime;
            }

            for (int i = 0; i < numberOfObstacleCheckRays; i++)
            {
                Vector3 currentCheckPos = (spriteRenderer.flipX) ? obstacleCheckPosRight.position : obstacleCheckPosLeft.position;
                Vector3 currentCheckDirection = (spriteRenderer.flipX) ? Vector3.right : Vector3.left;

                float delta = spriteRenderer.flipX ? -0.1f : 0.1f;

                Vector3 castPosition = new Vector3(currentCheckPos.x - delta, currentCheckPos.y - distanceBetweenRays * i, currentCheckPos.z);

                RaycastHit2D hitInfo = Physics2D.Raycast(castPosition, currentCheckDirection, obstacleCheckDistance, groundLayer);

                Debug.DrawRay(castPosition, currentCheckDirection * obstacleCheckDistance, Color.red);
                if (hitInfo.collider != null)
                {
                    if (hitInfo.collider.CompareTag("Player"))
                    {
                        MovementController playerMovementController = hitInfo.collider.GetComponent<MovementController>();
                        if (!playerMovementController.isDead && !playerMovementController.invincible)
                        {
                            playerMovementController.wasHit = true;
                            attacking = true;
                            //Debug.Log("Attacked Player");
                        }

                        pauseMovement = true;
                    }
                    else
                    {
                        pauseMovement = false;
                        //Debug.Log("Hit obstacle");
                        currentWayPointIndex = (currentWayPointIndex == 0) ? 1 : 0;
                    }

                    break;
                }
                else
                {
                    pauseMovement = false;
                }
            }

            Vector3 directionOfMovement = (wayPoints[currentWayPointIndex].position - transform.position);
            if (directionOfMovement.sqrMagnitude < 0.1f)
            {
                currentWayPointIndex = (currentWayPointIndex == 0) ? 1 : 0;
            }

            directionOfMovement = directionOfMovement.normalized;
            Vector3 velocity = directionOfMovement * movementSpeed;

            spriteRenderer.flipX = (velocity.x > 0.0f);

            if (!pauseMovement && !attacking)
            {
                animator.SetFloat("speed", Mathf.Abs(velocity.magnitude));
                transform.position += velocity * Time.deltaTime;
            }
            else
            {
                animator.SetFloat("speed", 0);
            }


            if (wasHit)
            {
                animator.SetBool("wasHit", true);
                hitAnimationRemainingTime = hitAnimationTime;
                invincible = true;
                health--;
                wasHit = false;
            }
            else if (hitAnimationRemainingTime > 0.0f)
            {
                hitAnimationRemainingTime -= Time.deltaTime;
            }
            else if (hitAnimationRemainingTime <= 0.0f)
            {
                animator.SetBool("wasHit", false);
            }

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

            
            if(attacking && remainingAttackingTime <= 0.0f)
            {
                attacking = false;
                animator.SetBool("attacking", false);
                remainingAttackingTime = attackingTime;
            }
            else if (attacking && remainingAttackingTime > 0.0f)
            {
                remainingAttackingTime -= Time.deltaTime;
                //Debug.Log(animator.GetBool("attacking"));
                if (!animator.GetBool("attacking"))
                {
                    animator.SetBool("attacking", true);
                }
            }

        }
    }
}
