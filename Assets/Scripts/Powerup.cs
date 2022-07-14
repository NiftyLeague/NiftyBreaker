using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    public SpriteRenderer spriteRenderer;
    public float speed = 5;
    public PowerUpType powerUpType;

    float currentColor;

    private void Start()
    {
        Destroy(gameObject, 10);
    }

    void FixedUpdate()
    {
        rigidBody.velocity = new Vector2(0, -1 * speed);
        currentColor = Mathf.PingPong(Time.time, 1);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, currentColor, 1);
    }

    public void CollectPowerup()
    {
        switch (powerUpType)
        {
            case PowerUpType.LifeUp:
                GameplayManager.I.GainLife();
                break;
        }
        Destroy(gameObject);
    }
}

public enum PowerUpType
{
    LifeUp,
    Slow,
    SpikedBall,
    Multiball,
    Clone,
}
