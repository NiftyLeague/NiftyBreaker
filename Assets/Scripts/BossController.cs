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
    public Transform laserStart;
    public GameObject laserPrefab;
    public Transform bossStageBlocks;
    public List<SimpleAnim> landingDustClouds;
    [Space]
    public ObscuredInt health = 20;
    public ObscuredInt almostDeadHealth = 10;
    public ObscuredFloat moveSpeed = 5;
    [Space]
    public ObscuredFloat bossTopStageYPosition = 3.7f;
    public ObscuredFloat bossBottomStageYPosition = -6f;

    [HideInInspector] public ObscuredBool isBossModeOn = false;
    private ActionPhase currentActionPhase;
    private Vector2 currentMoveDirection = Vector2.left;
    private ObscuredBool isMoving = true;
    private ObscuredBool isAttacking = false; 
    private float almostDeadAlpha;
    private Coroutine currentActionCoroutine;
    private float healthBarBeforeAmount;
    private ObscuredFloat attackActionTimer = 1;
    private ObscuredFloat attackingTimer;
    private ObscuredFloat nextAttackTimer;
    private ObscuredBool isTackleSlamming;
    private ObscuredBool canTakeDamage = true;

    private void Start()
    {
        isBossModeOn = true;
    }

    private void Update()
    {
        if (!isBossModeOn) 
        {
            return;
        }

        bossAlmostDeadOverlaySpriteRenderer.sprite = bossSpriteRenderer.sprite;

        if (health <= almostDeadHealth)
        {
            almostDeadAlpha = Mathf.PingPong(Time.time * (10 - health), 0.7f);
            bossAlmostDeadOverlaySpriteRenderer.color = new Color(bossAlmostDeadOverlaySpriteRenderer.color.r, bossAlmostDeadOverlaySpriteRenderer.color.g, bossAlmostDeadOverlaySpriteRenderer.color.b, almostDeadAlpha);
        }

        healthBarParent.transform.position = new Vector3(bossTransform.position.x, bossTransform.position.y + 3.5f);
        healthBarBeforeAmount = Mathf.Lerp(healthBarBeforeAmount, (float)health, 0.005f);
        healthBarBefore.fillAmount = healthBarBeforeAmount / (float)20;

        if (isMoving)
        {
            if (bossTransform.position.x <= -11f)
            {
                bossTransform.position = new Vector2(-10, bossTopStageYPosition);
                SetDirection(FacingDirection.Right);
            }
            else if (bossTransform.position.x >= 11f)
            {
                bossTransform.position = new Vector2(10, bossTopStageYPosition);
                SetDirection(FacingDirection.Left);
            }

            bossTransform.Translate((currentMoveDirection * (moveSpeed + (5 - health/4))) * Time.deltaTime);
        }

        if (isAttacking)
        {
            attackActionTimer -= Time.deltaTime;

            if (attackActionTimer <= 0)
            {
                attackActionTimer = GetNewActionTimerAmount();

                switch (currentActionPhase)
                {
                    case ActionPhase.DropDombs:
                        gameplayManager.audioManager.PlaySound("BossDropBomb");
                        Instantiate(bombProjectilePrefab, bombProjectileStart.position, bombProjectilePrefab.transform.rotation, transform);
                        break;
                    case ActionPhase.FireLasers:
                        gameplayManager.audioManager.PlaySound("BossFireLaser");
                        Instantiate(laserPrefab, laserStart.position, laserPrefab.transform.rotation, transform);
                        break;
                    case ActionPhase.PlaceBlocks:
                        List<Brick> bossBricksToChooseFrom = new List<Brick>();
                    
                        for (int i = 0; i < bossStageBlocks.childCount; i++)
                        {
                            if (!bossStageBlocks.GetChild(i).gameObject.activeInHierarchy)
                            {
                                bossBricksToChooseFrom.Add(bossStageBlocks.GetChild(i).GetComponent<Brick>());
                            }
                        }

                        if (bossBricksToChooseFrom.Count > 0) 
                        {
                            gameplayManager.audioManager.PlaySound("BossSpawnBrick");
                            bossBricksToChooseFrom[Random.Range(0, bossBricksToChooseFrom.Count)].InitializeBrick(Random.Range(1, 5), false, Random.value > 0.8f, true);
                        }
                        break;
                    case ActionPhase.TackleGround:
                        if (!isTackleSlamming)
                        {
                            StartCoroutine(TackleGroundAnimation());
                        }
                        break;
                }
            }

            attackingTimer -= Time.deltaTime;

            if (attackingTimer <= 0)
            {
                if (!isTackleSlamming)
                {
                    isAttacking = false;
                }
            }   
        }
        else
        {
            nextAttackTimer -= Time.deltaTime;

            if (nextAttackTimer <= 0)
            {
                nextAttackTimer = UnityEngine.Random.Range(1, 4);
                isAttacking = true;
                PerformNextAttack();
            }
        }
    }

    IEnumerator TackleGroundAnimation()
    {
        isMoving = false;
        isTackleSlamming = true;

        Tween<float> yPositionTweenDrop = new Tween<float>(bossTopStageYPosition, bossBottomStageYPosition, 0.8f, TweenEaseType.CubicIn);

        while (!yPositionTweenDrop.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            bossTransform.position = new Vector3(bossTransform.position.x, yPositionTweenDrop.Update(Time.deltaTime), 0);
        }

        foreach (SimpleAnim dustcloud in landingDustClouds)
        {
            dustcloud.gameObject.SetActive(true);
            dustcloud.Play();
        }
        gameplayManager.audioManager.PlaySound("BossSlamGround");
        gameplayManager.cameraShake.Shake(0.8f, 8);

        yield return new WaitForSeconds(0.6f);

        Tween<float> yPositionTweenRise = new Tween<float>(bossBottomStageYPosition, bossTopStageYPosition, 1f, TweenEaseType.CubicOut);

        while (!yPositionTweenRise.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            bossTransform.position = new Vector3(bossTransform.position.x, yPositionTweenRise.Update(Time.deltaTime), 0);
        }

        yield return new WaitForSeconds(0.2f);

        isMoving = true;
        isTackleSlamming = false;
    }

    void UpdateHealthBar()
    {
        healthBarParent.SetActive(true);
        healthBarCurrent.fillAmount = (float)((float)health / (float)20);
    }

    public void TakeDamage(int amount = 1)
    {
        if (!canTakeDamage)
        {
            return;
        }

        gameplayManager.audioManager.PlaySound("BossTakeDamage");
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

    public void StartBossFight()
    {
        isBossModeOn = true;
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
        isBossModeOn = false;
        bossAlmostDeadOverlaySpriteRenderer.color = new Color(bossAlmostDeadOverlaySpriteRenderer.color.r, bossAlmostDeadOverlaySpriteRenderer.color.g, bossAlmostDeadOverlaySpriteRenderer.color.b, 0);
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
        }
        StopAllCoroutines();
    }

    public void LostBossFight()
    {
        EndBossFight();
        StartCoroutine(LeaveArenaAnimation());
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
        canTakeDamage = false;

        bossShaker.Shake(0.2f, 10);
        bossSpriteRenderer.material = hitSpriteMaterial;

        yield return new WaitForSeconds(0.1f);

        bossSpriteRenderer.material = defaultSpriteMaterial;

        canTakeDamage = true;
    }

    void PerformNextAttack()
    {
        int nextAttack = UnityEngine.Random.Range(1, 5);
        currentActionPhase = (ActionPhase)nextAttack;
        attackingTimer = Random.Range(8.0f, 12.0f);
    }

    float GetNewActionTimerAmount()
    {
        switch (currentActionPhase)
        {
            case ActionPhase.DropDombs:
                return Random.Range(0.1f, 2.0f);
                break;
            case ActionPhase.FireLasers:
                return Random.Range(1.0f, 4.0f);
                break;
            case ActionPhase.PlaceBlocks:
                return Random.Range(1.0f, 4.0f);
                break;
            case ActionPhase.TackleGround:
                return Random.Range(2.0f, 6.0f);
                break;
        }

        return 0;
    }

    IEnumerator LeaveArenaAnimation()
    {
        Tween<float> yPositionTweenRise = new Tween<float>(bossTransform.position.y, 15, 2f, TweenEaseType.CubicOut);

        while (!yPositionTweenRise.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            bossTransform.position = new Vector3(bossTransform.position.x, yPositionTweenRise.Update(Time.deltaTime), 0);
        }
    }
}

public enum FacingDirection
{
    None,
    Left,
    Right,
}

public enum ActionPhase
{
    Idle,
    DropDombs,
    FireLasers,
    PlaceBlocks,
    TackleGround,
}