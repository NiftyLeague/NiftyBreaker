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
		PlayerSpriteManager.I.SetCharacterSprites(player);
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
			gameplayManager.audioManager.PlaySound(AudioManager.SoundID.menuOptionSelect);
			PlayerSpriteManager.I.ChangeCharacter(player);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Powerup"))
		{
			gameplayManager.audioManager.PlaySound(AudioManager.SoundID.powerupGet);
			collision.GetComponent<Powerup>().CollectPowerup();
		}
	}
}
