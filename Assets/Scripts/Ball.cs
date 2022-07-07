using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public AudioManager audioManager;
    public Rigidbody2D rigidBody;
    public SpriteRenderer spriteRenderer;

    public float speed = 500f;

    public void ResetBall()
    {
        gameObject.SetActive(true);

        transform.position = Vector2.down;
        rigidBody.velocity = Vector2.zero;

        Invoke(nameof(SetRandomTrajectory), 1f);
    }

    private void SetRandomTrajectory()
    {
        Vector2 force = Vector2.zero;
        force.x = Random.Range(-1f, 1f);
        force.y = -1f;

        rigidBody.AddForce(force.normalized * speed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        audioManager.PlaySound(AudioManager.SoundID.ballBounce);
        rigidBody.velocity = rigidBody.velocity * 1.02f;
        EffectsController.CreateHitEffect(transform.position, 0.1f, false);
    }
}
