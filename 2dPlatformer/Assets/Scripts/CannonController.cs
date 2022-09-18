using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonController : MonoBehaviour
{
    public GameObject cannonBall;
    public Transform shootPoint;
    public Vector3 shootDirection;

    public float shootAnimationTime = 0.3f;
    private float remainingShootAnimTime = 0.3f;
    private bool shoot = false;

    public float numOfTimesShootPerSec = 2;
    private float timeBtwShots = 0.5f;
    private float timeTillNextShot = 0.5f;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();

        remainingShootAnimTime = shootAnimationTime;
        timeBtwShots = 1 / (float)numOfTimesShootPerSec;
        timeTillNextShot = timeBtwShots;
        
    }

    // Update is called once per frame
    void Update()
    {
        if(timeTillNextShot <= 0.0f)
        {
            GameObject curCannonBall = Instantiate(cannonBall, shootPoint.position, Quaternion.identity, transform);
            curCannonBall.GetComponent<CannonBallController>().direction = shootDirection;
            shoot = true;
            timeTillNextShot = timeBtwShots;
        }
        else
        {
            timeTillNextShot -= Time.deltaTime;
        }

        if(shoot && remainingShootAnimTime > 0.0f)
        {
            remainingShootAnimTime -= Time.deltaTime;
            animator.SetBool("shoot", true);
        }
        else if(remainingShootAnimTime <= 0.0f)
        {
            shoot = false;
            remainingShootAnimTime = shootAnimationTime;
            animator.SetBool("shoot", false);
        }
    }
}
