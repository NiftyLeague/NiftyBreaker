using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowClone : MonoBehaviour
{
    public SpriteRenderer cloneSpriteRenderer;
    public SpriteRenderer playerSpriteRenderer;

    void Update()
    {
        cloneSpriteRenderer.sprite = playerSpriteRenderer.sprite;
        cloneSpriteRenderer.transform.localScale = playerSpriteRenderer.transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Powerup"))
        {
            GameplayManager.I.audioManager.PlaySound("PowerupGet");
            collision.GetComponent<Powerup>().CollectPowerup();
        }
    }
}
