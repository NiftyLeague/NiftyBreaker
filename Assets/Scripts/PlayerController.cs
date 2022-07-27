using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
	public GameplayManager gameplayManager;
	public SpriteRenderer playerSpriteRenderer;
	public SpriteRenderer chargeEffectSpriteRenderer;
	public GameObject chargeEffectClouds;
	public Transform playerTransform;

	private InputState input = new InputState();
	bool canPlayChargeEffect = true;

	private void Start()
	{
		PlayerSpriteManager.I.SetCharacterSprites();
		ChargeEffectReset();
	}

	void FixedUpdate()
	{
		if (gameplayManager.hasGameEnded)
		{
			return;
		}

		InputReader.GetInput(input);

		if (input.PressedY)
		{
			if (!PlayerSpriteManager.I.CanChangeCharacters())
			{
				return;
			}
			gameplayManager.audioManager.PlaySound("MenuOptionSelect");
			PlayerSpriteManager.I.ChangeCharacter();
		}
	}

	private void Update()
	{
		chargeEffectSpriteRenderer.sprite = playerSpriteRenderer.sprite;
		chargeEffectSpriteRenderer.transform.localScale = playerSpriteRenderer.transform.localScale;
	}

	public void CommenceChargeEffect()
	{
		if (!canPlayChargeEffect)
		{
			return;
		}
		gameplayManager.audioManager.PlaySound("ChargedHitIndicator");
		canPlayChargeEffect = false;
		chargeEffectClouds.SetActive(true);
		StartCoroutine(ChargeEffect());
	}

	public void ChargeEffectReset()
	{
		canPlayChargeEffect = true;
		chargeEffectClouds.SetActive(false);
	}

	IEnumerator ChargeEffect()
	{
		float a = 0;
		float b = 1;

		Tween<float> alphaTween = new Tween<float>(a, b, 0.1f, TweenEaseType.CubicIn);

		while (!alphaTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			chargeEffectSpriteRenderer.color = new Color(1, 1, 1, alphaTween.Update(Time.deltaTime));
		}

		a = 1;
		b = 0;

		alphaTween = new Tween<float>(a, b, 0.2f, TweenEaseType.CubicIn);

		while (!alphaTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			chargeEffectSpriteRenderer.color = new Color(1, 1, 1, alphaTween.Update(Time.deltaTime));
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Powerup"))
		{
			gameplayManager.audioManager.PlaySound("PowerupGet");
			collision.GetComponent<Powerup>().CollectPowerup();
		}
	}
}
