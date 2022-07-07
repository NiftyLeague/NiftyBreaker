using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    private GameplayManager gameplayManager;
    public SpriteRenderer spriteRenderer;
    public CameraShake shaker;

    public Sprite[] states;

    public int health;
    public int points = 10;

    public bool unbreakable;

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

        if (this.health <= 0)
        {
            this.gameObject.SetActive(false);
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
