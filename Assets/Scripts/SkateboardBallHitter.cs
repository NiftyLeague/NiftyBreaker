using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardBallHitter : MonoBehaviour
{
	public GameplayManager gameplayManager;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Ball ball = collision.gameObject.GetComponent<Ball>();

		if (ball != null)
		{
			ball.rigidBody.velocity = new Vector2(Random.Range(0.1f, 1.0f), Random.Range(0.3f, 1.0f)) * 10;
			ball.HitBall(false);
			EffectsController.CreateHitEffect(collision.transform.position, 0.1f, false);
			gameplayManager.SetBallOwnership(PlayerOwnerShip.Player1);
			gameplayManager.cameraShake.Shake(0.1f, 1);
			gameplayManager.ResetMultiplier();
		}
	}
}
