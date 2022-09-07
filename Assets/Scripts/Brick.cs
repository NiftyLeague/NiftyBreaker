using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    private GameplayManager gameplayManager;
    public SpriteRenderer spriteRenderer;
    public CameraShake shaker;
    public GameObject hitOverlay;
    public GameObject powerUpOverlay;
    public GameObject bombOverlay;
    public GameObject brickBreakApartPrefab;

    public Sprite[] states;

    private int originalHealth;
    public int health;
    
    private int pointWorth;

    public bool unbreakable;
    public bool bomb;
    bool bossBrick;
    bool hasPowerup;
    bool firstTimeActivated = true;

    public void Hit(int amount = 1, bool isBallFullyCharged = false)
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

            var newBrickBreakApart = Instantiate(brickBreakApartPrefab, transform.parent.transform);
            newBrickBreakApart.transform.position = transform.position;
            gameplayManager.audioManager.PlaySound("BrickBreak");
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
            StartCoroutine(HitFlash());
            shaker.Shake(0.2f, 10);
        }

        int pointsGained = pointWorth;
        if (isBallFullyCharged)
        {
            pointsGained *= 2;
        }
        if (bossBrick)
        {
            pointsGained = 0;
        }
        gameplayManager.Hit(pointsGained);
    }

    public void InitializeBrick(int health, bool indestructableBrick = false, bool bombBrick = false, bool bossBrick = false)
    {
        this.health = health;
        unbreakable = indestructableBrick;
        bomb = bombBrick;
        this.bossBrick = bossBrick;

        ActivateBrick();
    }

    public void ActivateBrick()
    {
        if (firstTimeActivated)
        {
            originalHealth = health;
            firstTimeActivated = false;
        }

        health = originalHealth;
        hasPowerup = false;
        gameObject.SetActive(true);

        if (gameplayManager == null)
        {
            gameplayManager = FindObjectOfType<GameplayManager>();
        }

        if (!unbreakable)
        {
            if (Random.value <= gameplayManager.powerUpSpawnChance)
            {
                hasPowerup = true;
            }
        }
        
        UpdateBrickColor();
    }

    private void UpdateBrickColor()
    {
        spriteRenderer.sprite = states[health];

        powerUpOverlay.SetActive(hasPowerup ? true : false);
        bombOverlay.SetActive(bomb ? true : false);

        pointWorth = health * 10;
    }

    IEnumerator HitFlash()
    {
        hitOverlay.SetActive(true);

        yield return new WaitForSeconds(0.06f);

        hitOverlay.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Projectile"))
        {
            Hit(1, collision.gameObject.GetComponent<Ball>().fullyChargedOverlay.activeInHierarchy);
        }
    }
}
