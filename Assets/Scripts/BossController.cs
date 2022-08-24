using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.ObscuredTypes;

public class BossController : MonoBehaviour
{
    public GameplayManager gameplayManager;
    [Space]
    public ObscuredInt health = 20;
    public ObscuredInt almostDeadHealth = 10;
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

    private float almostDeadAlpha;
    private Coroutine currentActionCoroutine;
    private float healthBarBeforeAmount;

    private void Update()
    {
        bossAlmostDeadOverlaySpriteRenderer.sprite = bossSpriteRenderer.sprite;

        if (health <= almostDeadHealth)
        {
            almostDeadAlpha = Mathf.PingPong(Time.time * (10 - health), 0.7f);
            bossAlmostDeadOverlaySpriteRenderer.color = new Color(bossAlmostDeadOverlaySpriteRenderer.color.r, bossAlmostDeadOverlaySpriteRenderer.color.g, bossAlmostDeadOverlaySpriteRenderer.color.b, almostDeadAlpha);
        }

        healthBarBeforeAmount = Mathf.Lerp(healthBarBeforeAmount, (float)health, 0.005f);
        healthBarBefore.fillAmount = healthBarBeforeAmount / (float)20;
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

    IEnumerator TakeDamageAnimation()
    {
        bossShaker.Shake(0.2f, 10);
        bossSpriteRenderer.material = hitSpriteMaterial;

        yield return new WaitForSeconds(0.1f);

        bossSpriteRenderer.material = defaultSpriteMaterial;
    }
}
