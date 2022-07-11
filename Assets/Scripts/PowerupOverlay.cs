using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupOverlay : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    private float currentAlpha;

    void Update()
    {
        currentAlpha = Mathf.PingPong(Time.time * 2, 1);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, currentAlpha);
    }
}
