using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    private GameplayManager gameplayManager;
    public SpriteRenderer spriteRenderer;
    public CameraShake shaker;
    public GameObject powerUpOverlay;

    public Sprite[] states;

    public int health;
    public int points = 10;

    public bool unbreakable;
    bool hasPowerup;

    private void Start()
    {
        gameplayManager = FindObjectOfType<GameplayManager>();

        if (!this.unbreakable)
        {
            UpdateBrickColor();
        }
    }

    private void Hit()
    {
        if (this.unbreakable)
        {
            return;
        }

        this.health--;
        if (gameplayManager.godMode)
        {
            health = 0;
        }

        if (this.health <= 0)
        {
            this.gameObject.SetActive(false);

            if (hasPowerup)
            {
                gameplayManager.audioManager.PlaySound(AudioManager.SoundID.powerupDrop);
                Instantiate(gameplayManager.powerup, transform.position, transform.rotation);
            }
        }
        else
        {
            UpdateBrickColor();
            shaker.Shake(0.1f, 10);
        }

        gameplayManager.Hit((health+1) * points);
    }

    private void UpdateBrickColor()
    {
        this.spriteRenderer.sprite = this.states[this.health - 1];

        powerUpOverlay.SetActive(hasPowerup ? true : false);
    }

    public void AddPowerup()
    {
        hasPowerup = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Ball ball = collision.gameObject.GetComponent<Ball>();

        if (ball != null)
        {
            Hit();
        }
    }
}
