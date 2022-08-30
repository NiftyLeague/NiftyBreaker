using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;

public class BossController : MonoBehaviour
{
    public GameplayManager gameplayManager;
    [Space]
    public Transform bossTransform;
    public SpriteRenderer bossSpriteRenderer;
    public SpriteRenderer bossAlmostDeadOverlaySpriteRenderer;
    public Material defaultSpriteMaterial;
    public Material hitSpriteMaterial;
    public CameraShake bossShaker;
    public GameObject bombProjectile;
    public GameObject healthBarParent;
    public Image healthBarCurrent;
    public Image healthBarBefore;
    public Transform bombProjectileStart;
    public GameObject bombProjectilePrefab;
    [Space]
    public ObscuredInt health = 20;
    public ObscuredInt almostDeadHealth = 10;
    public ObscuredFloat moveSpeed = 5;
    [Space]
    public ObscuredFloat bossOnStageYPosition = -0.82f;

    private Vector2 currentMoveDirection = Vector2.left;
    private bool isMoving = true;
    private float almostDeadAlpha;
    private Coroutine currentActionCoroutine;
    private float healthBarBeforeAmount;
    private float bombDropTimer = 1;

    private void Update()
    {
        bossAlmostDeadOverlaySpriteRenderer.sprite = bossSpriteRenderer.sprite;

        if (health <= almostDeadHealth)
        {
            almostDeadAlpha = Mathf.PingPong(Time.time * (10 - health), 0.7f);
            bossAlmostDeadOverlaySpriteRenderer.color = new Color(bossAlmostDeadOverlaySpriteRenderer.color.r, bossAlmostDeadOverlaySpriteRenderer.color.g, bossAlmostDeadOverlaySpriteRenderer.color.b, almostDeadAlpha);
        }

        healthBarParent.transform.position = new Vector3(bossTransform.position.x, bossTransform.position.y + 3.5f);
        healthBarBeforeAmount = Mathf.Lerp(healthBarBeforeAmount, (float)health, 0.005f);
        healthBarBefore.fillAmount = healthBarBeforeAmount / (float)20;

        if (!isMoving)
        {
            return;
        }

        if (bossTransform.position.x <= -11f)
        {
            bossTransform.position = new Vector2(-10, bossOnStageYPosition);
            SetDirection(FacingDirection.Right);
        }
        else if (bossTransform.position.x >= 11f)
        {
            bossTransform.position = new Vector2(10, bossOnStageYPosition);
            SetDirection(FacingDirection.Left);
        }

        bombDropTimer -= Time.deltaTime;

        if (bombDropTimer <= 0)
        {
            bombDropTimer = Random.Range(0.1f, 2.0f);

            Instantiate(bombProjectilePrefab, bombProjectileStart.position, bombProjectilePrefab.transform.rotation, transform);
        }

        bossTransform.Translate((currentMoveDirection * moveSpeed) * Time.deltaTime);
    }

    void UpdateHealthBar()
    {
        healthBarParent.SetActive(true);
        healthBarCurrent.fillAmount = (float)((float)health / (float)20);
    }

    public void TakeDamage(int amount = 1)
    {
        StartCoroutine(TakeDamageAnimation());
        health -= amount;
        UpdateHealthBar();
        if (health <= 0)
        {
            health = 0;
            healthBarParent.SetActive(false);
            WonBossFight();
        }
    }

    public void WonBossFight()
    {
        EndBossFight();
        gameplayManager.ScorePoints(500);
        //gameplayManager.bossHasBeenDefeated = true;
        //StartCoroutine(Dead());
    }

    public void EndBossFight()
    {
        //isBossModeOn = false;
        //CanHurtThePlayer(false);
        bossAlmostDeadOverlaySpriteRenderer.color = new Color(bossAlmostDeadOverlaySpriteRenderer.color.r, bossAlmostDeadOverlaySpriteRenderer.color.g, bossAlmostDeadOverlaySpriteRenderer.color.b, 0);
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
        }
        StopAllCoroutines();
    }

    void SetDirection(FacingDirection direction)
    {
        switch (direction)
        {
            case FacingDirection.Left:
                currentMoveDirection = Vector2.left;
                bossTransform.localScale = new Vector3(1, 1, 1);
                break;
            case FacingDirection.Right:
                currentMoveDirection = Vector2.right;
                bossTransform.localScale = new Vector3(-1, 1, 1);
                break;
        }
    }

    IEnumerator TakeDamageAnimation()
    {
        bossShaker.Shake(0.2f, 10);
        bossSpriteRenderer.material = hitSpriteMaterial;

        yield return new WaitForSeconds(0.1f);

        bossSpriteRenderer.material = defaultSpriteMaterial;
    }
}

public enum FacingDirection
{
    None,
    Left,
    Right,
}