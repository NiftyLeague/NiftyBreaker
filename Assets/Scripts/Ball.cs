using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public AudioManager audioManager;
    public Rigidbody2D rigidBody;
    public SpriteRenderer spriteRenderer;
    public GameObject spikePowerupOverlay;
    public GameObject fullyChargedOverlay;
    public Transform startingDirectionPointer;

    public float speed = 500f;
    [HideInInspector] public bool hasBeenHitRecently;

    private float noBounceTimer;
    private Vector2 startingForceDirection;

    private void Update()
    {
        noBounceTimer += Time.deltaTime;

        if (noBounceTimer > 0.3f)
        {
            hasBeenHitRecently = false;
        }
    }

    public void ResetBall()
    {
        if (GameplayManager.I.hasGameEnded)
        {
            return;
        }

        gameObject.SetActive(true);
        fullyChargedOverlay.SetActive(false);

        noBounceTimer = 0;

        transform.position = Vector2.down;
        rigidBody.velocity = Vector2.zero;

        startingDirectionPointer.gameObject.SetActive(true);

        UpdateSpikePowerup();

        startingForceDirection = Vector2.zero;
        startingForceDirection.x = Random.Range(-1f, 1f);
        startingForceDirection.y = -1f;

        startingDirectionPointer.eulerAngles = new Vector3(0, 0, startingForceDirection.x * 60);

        Invoke(nameof(SetRandomDownTrajectory), 1f);
    }

    public void DeactivateBall()
    {
        gameObject.SetActive(false);
    }

    public void HitBall(bool hitWithBat, bool isBallFullyCharged = false)
    {
        if (hitWithBat)
        {
            GameplayManager.I.ScorePoints(5);
        }
        noBounceTimer = 0;
        hasBeenHitRecently = true;
        audioManager.PlaySound("ProjectileHit");
        fullyChargedOverlay.SetActive(isBallFullyCharged);
    }

    public void UpdateSpikePowerup()
    {
        spikePowerupOverlay.SetActive(GameplayManager.I.IsSpikedPowerupRunning());
    }

    private void SetRandomDownTrajectory()
    {
        noBounceTimer = 0;

        rigidBody.AddForce(startingForceDirection.normalized * speed);

        startingDirectionPointer.gameObject.SetActive(false);
    }

    public void SetAsMultiball(Transform mainBallTransform)
    {
        gameObject.SetActive(true);
        UpdateSpikePowerup();

        noBounceTimer = 0;

        transform.position = mainBallTransform.position;
        rigidBody.velocity = Vector2.zero;

        Vector2 force = Vector2.zero;
        force.x = Random.Range(-1f, 1f);
        force.y = 1f;

        rigidBody.AddForce(force.normalized * speed);
    }

    //private void SetRandomTrajectory()
    //{
    //    Vector2 force = Vector2.zero;
    //    force.x = Random.Range(-1f, 1f);
    //    force.y = Random.Range(-1f, 1f);

    //    rigidBody.AddForce(force.normalized * speed);
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        audioManager.PlaySound("BallBounce");

        //if (noBounceTimer >= 2)
        //{
        //    SetRandomTrajectory();
        //}

        noBounceTimer = 0;

        Vector2 currentDirection = rigidBody.velocity;
        currentDirection = new Vector2(currentDirection.x + Random.Range(-0.05f, 0.05f), currentDirection.y + Random.Range(-0.05f, 0.05f));
        rigidBody.velocity = currentDirection;

        if (rigidBody.velocity.magnitude < 25)
        {
            rigidBody.velocity = rigidBody.velocity * 1.02f;
        }

        if (collision.transform.CompareTag("Boss"))
        {
            GameplayManager.I.bossController.TakeDamage(1);
        }
        
        EffectsController.CreateHitEffect(transform.position, 0.1f, false);
    }
}
