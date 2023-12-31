using UnityEngine;

public class EnemyController : ThreatController
{
    public enum EnemyState
    {
        idle,
        patrolling,
        chasing,
        returning
    };

    // Generic
    public Rigidbody rb;

    // Move and Rotation
    public float moveSpeed;
    private Vector3 moveDirection;
    public float rotationSpeed;
    public float jumpSpeed;
    private Vector3 lookDirection;

    // Route
    public EnemyState currentState;
    public Transform[] routePoints;
    private int nextPoint = 0;
    public float chaseDistance;
    public float chaseSpeed;
    public float chaseDelay;
    public float chaseDelayCounter;
    public float returningDelay;
    public float returnCounter;
    public float loseDistance;
    public float waitDelay;
    public float waitCounter;
    public float waitProbability;
    private float yAux;

    // Events
    public float blinkDuration = 0.1f;
    public float dyingDelay;
    public float dyingCounter;
    public float deformationSpeed;
    public GameObject deathEffect;

    private void Start()
    {
        this.waitCounter = this.waitDelay;
        this.currentState = EnemyState.idle;
        foreach (Transform routePoint in routePoints)
            routePoint.parent = null;
    }

    private void Update()
    {
        if (MainManager.Instance.playerController == null)
            return;

        if (dyingCounter > 0)
        {
            dyingCounter -= Time.deltaTime;
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1.25f, 0.5f, 1.25f), deformationSpeed * Time.deltaTime);
            if (dyingCounter <= 0)
            {
                if (deathEffect != null)
                    Instantiate(deathEffect, transform.position, Quaternion.identity);
                AudioManager.instance.PlaySFX(6);
                Destroy(gameObject);
            }
        }
        else
        {
            switch (currentState)
            {
                case EnemyState.idle:
                    rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
                    waitCounter -= Time.deltaTime;
                    if (waitCounter <= 0)
                    {
                        currentState = EnemyState.patrolling;
                        NextDestinyPoint();
                    }
                    break;

                case EnemyState.patrolling:
                    yAux = rb.velocity.y;
                    moveDirection = routePoints[nextPoint].position - transform.position;
                    moveDirection.y = 0;
                    moveDirection.Normalize();
                    rb.velocity = moveDirection * moveSpeed;
                    rb.velocity = new Vector3(rb.velocity.x, yAux, rb.velocity.z);

                    if (Vector3.Distance(transform.position, routePoints[nextPoint].position) <= 0.1f)
                        NextDestinyPoint();
                    else
                        lookDirection = routePoints[nextPoint].position;

                    break;

                case EnemyState.chasing:
                    lookDirection = MainManager.Instance.playerController.transform.position;
                    if (chaseDelayCounter > 0)
                        chaseDelayCounter -= Time.deltaTime;
                    else if (MainManager.Instance.playerController.characterController.isGrounded)
                    {
                        yAux = rb.velocity.y;
                        moveDirection = MainManager.Instance.playerController.transform.position - transform.position;
                        moveDirection.y = 0;
                        moveDirection.Normalize();
                        rb.velocity = moveDirection * chaseSpeed;
                        rb.velocity = new Vector3(rb.velocity.x, yAux, rb.velocity.z);
                    }
                    if (Vector3.Distance(MainManager.Instance.playerController.transform.position, this.transform.position) > loseDistance)
                    {
                        currentState = EnemyState.returning;
                        returnCounter = returningDelay;
                    }
                    break;

                case EnemyState.returning:
                    returnCounter -= Time.deltaTime;
                    if (returnCounter <= 0)
                        currentState = EnemyState.patrolling;
                    break;
            }
        }

        if (Vector3.Distance(MainManager.Instance.playerController.transform.position, this.transform.position) < chaseDistance && currentState != EnemyState.chasing)
        {
            currentState = EnemyState.chasing;
            rb.velocity = Vector3.up * jumpSpeed;
            chaseDelayCounter = chaseDelay;
        }

        lookDirection.y = transform.position.y;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection - transform.position), rotationSpeed * Time.deltaTime);
    }

    public void NextDestinyPoint()
    {
        if (Random.Range(0f, 100f) < waitProbability)
        {
            waitCounter = waitDelay;
            currentState = EnemyState.idle;
        }
        else
        {
            if (nextPoint == routePoints.Length - 1)
                nextPoint = 0;
            else
                ++nextPoint;
        }
    }

    /// Collision
    private void HandleCollision(string colliderTag)
    {
        if (colliderTag == "Player" && dyingCounter == 0)
        {
            this.HurtPlayer();
            chaseDelayCounter = chaseDelay;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        this.HandleCollision(collision.gameObject.tag);
    }

    private void OnCollisionStay(Collision collision)
    {
        this.HandleCollision(collision.gameObject.tag);
    }

    new private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        { // Player kills enemy
            MainManager.Instance.playerController.Bounce();
            dyingCounter = dyingDelay;
            AudioManager.instance.PlaySFX(7);
            MainManager.Instance.playerController.Blink();
            MainManager.Instance.currentLevelData.IncreaseEnemiesKilled(1);
        }
    }

}