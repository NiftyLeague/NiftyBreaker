using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    private GameplayManager gameplayManager;
    public SpriteRenderer spriteRenderer;
    public CameraShake shaker;
    public GameObject powerUpOverlay;
    public GameObject bombOverlay;

    public Sprite[] states;

    public int health;
    
    private int pointWorth;

    public bool unbreakable;
    public bool bomb;
    bool hasPowerup;

    private void Start()
    {
        gameplayManager = FindObjectOfType<GameplayManager>();

        if (!unbreakable)
        {
            UpdateBrickColor();
        }
    }

    public void Hit(int amount = 1)
    {
        if (unbreakable)
        {
            return;
        }

        health -= amount;
        if (gameplayManager.godMode || gameplayManager.IsSpikedPowerupRunning())
        {
            health = 0;
        }

        if (health <= 0)
        {
            health = 0;

            gameObject.SetActive(false);

            if (hasPowerup)
            {
                gameplayManager.audioManager.PlaySound("PowerupSpawn");
                Instantiate(gameplayManager.GetRandomPowerup(), transform.position, transform.rotation);
            }

            if (bomb)
            {
                gameplayManager.Explosion(transform.position);
            }
        }
        else
        {
            UpdateBrickColor();
            shaker.Shake(0.1f, 10);
        }

        gameplayManager.Hit(pointWorth);
    }

    private void UpdateBrickColor()
    {
        spriteRenderer.sprite = states[health - 1];

        powerUpOverlay.SetActive(hasPowerup ? true : false);
        bombOverlay.SetActive(bomb ? true : false);

        pointWorth = health * 10;
    }

    public void AddPowerup()
    {
        if (unbreakable)
        {
            return;
        }
        hasPowerup = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Projectile"))
        {
            Hit();
        }
    }
}
