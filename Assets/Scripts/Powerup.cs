using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    public SpriteRenderer spriteRenderer;
    private float speed = 5;
    public PowerUpType powerUpType;

    float currentColor;

    private void Start()
    {
        Destroy(gameObject, 10);
    }

    void FixedUpdate()
    {
        rigidBody.velocity = new Vector2(0, -1 * speed);
        currentColor = Mathf.PingPong(Time.time * 4, 1);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, currentColor, 1);
        float newSize = (currentColor / 2) + 1f;
        transform.localScale = new Vector3(newSize, newSize, newSize);
    }

    public void CollectPowerup()
    {
        switch (powerUpType)
        {
            case PowerUpType.LifeUp:
                GameplayManager.I.GainLife();
                break;
            case PowerUpType.SpikedBall:
                GameplayManager.I.RunSpikedPowerup();
                break;
            case PowerUpType.Multiball:
                GameplayManager.I.RunMultiballPowerup();
                break;
            case PowerUpType.Clone:
                GameplayManager.I.RunClonePowerup();
                break;
        }

        Destroy(gameObject);
    }
}

public enum PowerUpType
{
    LifeUp,
    SpikedBall,
    Multiball,
    Clone,
}
