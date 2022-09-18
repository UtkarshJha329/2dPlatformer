using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBallController : MonoBehaviour
{
    public Vector3 direction;

    public float ballSpeed = 3.0f;

    public float dieInSecs = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mvAmt = direction.normalized * ballSpeed * Time.deltaTime;
        transform.Translate(mvAmt);

        if(dieInSecs <= 0.0f)
        {
            Destroy(gameObject);
        }
        else
        {
            dieInSecs -= Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            MovementController playerMovementController = collision.collider.GetComponent<MovementController>();
            if(!playerMovementController.isDead && !playerMovementController.invincible)
            {
                playerMovementController.wasHit = true;
                dieInSecs = 0.0f;
                Debug.Log("hit player");
            }
        }
    }
}
