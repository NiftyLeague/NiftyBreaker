using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
	public GameplayManager gameplayManager;
	public int player = 1;
	[Space]
	public Transform playerTransform;

	private InputState input = new InputState();

	private void Start()
	{
		PlayerSpriteManager.I.SetCharacterSprites();
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

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Powerup"))
		{
			gameplayManager.audioManager.PlaySound("PowerupGet");
			collision.GetComponent<Powerup>().CollectPowerup();
		}
	}
}
