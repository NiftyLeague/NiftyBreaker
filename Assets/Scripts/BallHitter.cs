using UnityEngine;

public class BallHitter : MonoBehaviour
{
	public GameplayManager gameplayManager;
	public PlayerController playerController;
	public AudioManager audioManager;
	public Character playerCharacter;
	public Collider2D hitCollider;
	public PlayerOwnerShip playerOwnership;
	public bool isMainHitter;
	public BallHitter shadowCloneHitterLeft;
	public BallHitter shadowCloneHitterRight;
	float hitterTimer;
	float currentChargeAmount;
	bool nextBallHitIsFullyCharged;

	TurnDirection currentTurnDirection = TurnDirection.Right;
	Vector2 hitDirection;

    private void Start()
    {
		TurnRight();
    }

    void Update()
	{
		hitterTimer += Time.deltaTime;
		if (hitterTimer >= 0.05f)
		{
			hitCollider.enabled = true;
		}
		if (hitterTimer >= 0.11f)
		{
			hitterTimer = 0;
			TurnOff();
		}
	}

	public void TurnOn(HitterDirection attackDirection)
	{
		if (gameObject.activeInHierarchy)
		{
			return;
		}

		float attackDirectionX = 0;
		float hitDirectionX = 0;
		switch (currentTurnDirection)
		{
			case TurnDirection.Left:
				attackDirectionX = -1f;
				hitDirectionX = -0.5f;
				break;
			case TurnDirection.Right:
				attackDirectionX = 1f;
				hitDirectionX = 0.5f;
				break;
		}

		float hitDirectionY = 0;
		switch (attackDirection)
		{
			case HitterDirection.Up:
				transform.localPosition = new Vector2(0, 1.3f);
				transform.localEulerAngles = new Vector3(0, 0, 90);
				hitDirectionX = hitDirectionX / 4;
				hitDirectionY = 1;
				break;
			case HitterDirection.Forward:
				transform.localPosition = new Vector2(attackDirectionX, -1.15f);
				transform.localEulerAngles = new Vector3(0, 0, 0);
				hitDirectionX = hitDirectionX * 2;
				hitDirectionY = 0.5f;
				break;
			case HitterDirection.Diagonal:
				transform.localPosition = new Vector2(attackDirectionX * 1.2f, 0.6f);
				transform.localEulerAngles = new Vector3(0, 0, attackDirectionX * 45);
				hitDirectionY = 0.7f;
				break;
		}
		currentChargeAmount = 1 + (playerCharacter.attackChargeM / 2);
		nextBallHitIsFullyCharged = playerCharacter.attackChargeM >= 1;
		hitDirection = new Vector2(hitDirectionX, hitDirectionY);
		hitterTimer = 0;
		gameObject.SetActive(true);

		if (isMainHitter)
		{
			playerController.ChargeEffectReset();
			audioManager.PlaySound("BatSwing");
			shadowCloneHitterLeft.TurnOn(attackDirection);
			shadowCloneHitterRight.TurnOn(attackDirection);
		}
	}

	void TurnOff()
	{
		hitCollider.enabled = false;
		gameObject.SetActive(false);
	}

	public void TurnLeft()
	{
		if (gameObject.activeInHierarchy)
		{
			return;
		}
		currentTurnDirection = TurnDirection.Left;

		if (isMainHitter)
		{
			shadowCloneHitterLeft.TurnLeft();
			shadowCloneHitterRight.TurnLeft();
		}
	}

	public void TurnRight()
	{
		if (gameObject.activeInHierarchy)
		{
			return;
		}
		currentTurnDirection = TurnDirection.Right;

		if (isMainHitter)
		{
			shadowCloneHitterLeft.TurnRight();
			shadowCloneHitterRight.TurnRight();
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Projectile"))
		{
			Ball ball = collision.gameObject.GetComponent<Ball>();

			if (ball.hasBeenHitRecently)
			{
				return;
			}
			ball.rigidBody.velocity = hitDirection * (11 * currentChargeAmount);
			ball.HitBall(true, nextBallHitIsFullyCharged);
			EffectsController.CreateHitEffect(collision.transform.position, currentChargeAmount / 10, false);
			gameplayManager.SetBallOwnership(playerOwnership);
			gameplayManager.cameraShake.Shake(0.2f * currentChargeAmount, 1);
		}

		if (collision.CompareTag("Boss"))
		{
			gameplayManager.bossController.TakeDamage(1);
		}
	}
}

public enum HitterDirection
{
	Forward,
	Up,
	Diagonal,
}

enum TurnDirection
{
	Left,
	Right,
}
