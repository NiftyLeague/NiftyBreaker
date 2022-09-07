using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json.Linq;

public class GameplayManager : MonoBehaviour
{
	public static GameplayManager I;

	public MenuManager menuManager;
	public BossController bossController;
	public AudioManager audioManager;
	public Character playerCharacter;
	public PlayerController playerController;
	public TransitionManager transitionManager;
	[Space]
	public int currentlevel = 1;
	public int totalLevel = 1;
	public int lives = 4;
	public float multiplier = 1.0f;
	public PlayerOwnerShip currentPlayerOwnership;
	public Color32 playerOwnershipColor;
	[Space]
	public ObscuredFloat timePlayed;
	public ObscuredInt score;
	public ObscuredInt hits;
	public ObscuredInt xp;
	[Space]
	public List<SpriteRenderer> playerLifePips;
	public List<Ball> balls;
	public List<Transform> stages;
	public List<GameObject> powerups;
	public List<GameObject> clones;
	[Space]
	public ObscuredFloat currentSpeedIncrease;
	public CameraShake cameraShake;
	[Space]
	public TextMeshProUGUI endOfGameStatsText;
	public TextMeshProUGUI playerNameText;
	public TextMeshProUGUI playerScoreText;
	public TextMeshProUGUI levelText;
	public TextMeshProUGUI gameOverStatNamesText;
	public TextMeshProUGUI gameOverStatNumbersText;
	public TextMeshProUGUI powerupCloneTimerText;
	public TextMeshProUGUI powerupSpikedballTimerText;
	public GameObject powerupCloneTimerIcon;
	public GameObject powerupSpikedballTimerIcon;
	public GameObject gameOverBackground;
	public GameObject gameOverSkipPrompt;
	public GameObject scoreGainedUIPrefab;
	public Transform playerCanvas;
	[Space]
	public Transform worldLeftLimit;
	public Transform worldRightLimit;

	public ObscuredFloat maxBallSpeed;
	public Vector2 startTimeoutRange;
	public Vector2 shootRandomTimeoutRange;
	public ObscuredBool hasGameEnded;
	public ObscuredFloat powerUpSpawnChance = 0.1f;
	[Space]
	public bool godMode;

	Coroutine currentScoreTextCoroutine;

	ObscuredFloat spikedBallPowerupTimer;
	ObscuredFloat clonesPowerupTimer;
	float gameOverTimer1;
	float gameOverTimer2;

	private InputState input = new InputState();

	private void Awake()
	{
		I = this;
	}

	void Start()
	{
		UpdateScoreTexts();
		ResetEverythingForANewGame();
		menuManager.menuPanel.SetActive(false);
		StartCoroutine(transitionManager.FirstStartTransition());
	}

    private void Update()
    {
		InputReader.GetInput(input);

		if (input.PressedA)
		{
			if (hasGameEnded)
			{
				if (gameOverTimer1 > 0)
				{
					gameOverTimer1 = 0;
					audioManager.PlaySound("MenuOptionSelect");
					return;
				}
				else if (gameOverTimer2 > 0)
				{
					gameOverTimer2 = 0;
					audioManager.PlaySound("MenuOptionSelect");
					return;
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.Q))
		{
			if (godMode)
			{
				foreach (Transform brick in stages[currentlevel - 1])
				{
					brick.gameObject.SetActive(false);
				}

				CheckIfClearedLevel();
			}
		}
	}

    private void FixedUpdate()
	{
		if (gameOverTimer1 > 0)
		{
			gameOverTimer1 -= Time.deltaTime;
		}

		if (gameOverTimer2 > 0)
		{
			gameOverTimer2 -= Time.deltaTime;
		}

		if (hasGameEnded)
		{
			return;
		}

		timePlayed += Time.deltaTime;

		if (spikedBallPowerupTimer > 0)
		{
			spikedBallPowerupTimer -= Time.deltaTime;

			if (spikedBallPowerupTimer <= 0)
			{
				TurnOffPowerup(PowerUpType.SpikedBall);
			}

			powerupSpikedballTimerText.text = spikedBallPowerupTimer.ToString("0.0");
		}

		if (clonesPowerupTimer > 0)
		{
			clonesPowerupTimer -= Time.deltaTime;

			if (clonesPowerupTimer <= 0)
			{
				TurnOffPowerup(PowerUpType.Clone);
			}

			powerupCloneTimerText.text = clonesPowerupTimer.ToString("0.0");
		}
	}

	public void ScorePoints(int amount)
	{
		if (currentPlayerOwnership == PlayerOwnerShip.None)
		{
			return;
		}

		int scoreGainedAmount = amount;
		if (currentScoreTextCoroutine != null)
		{
			StopCoroutine(currentScoreTextCoroutine);
		}

		score += scoreGainedAmount;
		hits++;

		var newScoreGainedUIPlayer1 = Instantiate(scoreGainedUIPrefab, playerCanvas);
		newScoreGainedUIPlayer1.GetComponent<ScoreGainUI>().Initialize(scoreGainedAmount, PlayerOwnerShip.Player1);
		currentScoreTextCoroutine = StartCoroutine(AnimateScoreText());

		IncreaseSpeed();
		UpdateScoreTexts();
		audioManager.PlaySound("GainPoint");
		//EventController.AddScore(scoreGainedAmount);
	}

	void UpdateScoreTexts()
	{
		string multiplierText = "";
		if (multiplier > 1.0f)
		{
			multiplierText = "   <size=6>x" + multiplier.ToString("0.0");
		}

		playerScoreText.text = score.ToString("0") + multiplierText;
	}

	public void LoseBall()
	{
		bool isAtleastOneBallThere = IsAtleastOneBallActive();

		if (currentPlayerOwnership == PlayerOwnerShip.None || isAtleastOneBallThere)
		{
			audioManager.PlaySound("LoseNoOwnership");
		}
		else
		{
			audioManager.PlaySound("Lose");
		}

		if (isAtleastOneBallThere)
		{
			return;
		}

		if (currentPlayerOwnership == PlayerOwnerShip.Player1)
		{
			LoseLife();
		}
	
		ResetBall();
	}

	public void LoseLife(bool animateHurtFlash = false)
	{
		if (godMode)
		{
			return;
		}

		lives--;
		UpdateLives();
		if (lives <= 0)
		{
			Lose();
		}

		if (animateHurtFlash)
		{
			playerController.AnimateHurtFlash();
		}
	}

	public void ResetBall()
	{
		balls[0].ResetBall();
		SetBallOwnership(PlayerOwnerShip.None);
		ResetMultiplier();
	}

	public void ResetMultiplier()
	{
		multiplier = 1.0f;
		UpdateScoreTexts();
	}	

	public void Explosion(Vector3 position)
	{
		EffectsController.CreateExplosion(position);
		audioManager.PlaySound("Explosion", 0.25f);
		cameraShake.Shake(0.2f, 5);
	}

	public void SetBallOwnership(PlayerOwnerShip nextOwnership)
	{
		currentPlayerOwnership = nextOwnership;
		UpdateBallOwnership();
	}

	private void UpdateBallOwnership()
	{
		switch (currentPlayerOwnership)
		{
			case PlayerOwnerShip.None:
				foreach (Ball ball in balls)
				{
					ball.spriteRenderer.color = Color.white;
				}
				break;
			case PlayerOwnerShip.Player1:
				foreach (Ball ball in balls)
				{
					ball.spriteRenderer.color = playerOwnershipColor;
				}
				break;
		}
	}

	public void Lose()
	{
		if (hasGameEnded)
		{
			return;
		}
		
		TurnOffAllPowerups();
		hasGameEnded = true;
		cameraShake.Shake(0.5f, 5);
		IncreaseSpeed(true);
		playerCharacter.Lose();
		balls[0].gameObject.SetActive(false);
		bossController.LostBossFight();
		menuManager.UpdateLeaderboards();
		//EventController.AddMatchEnd(PlayerSpriteManager.lastDegenIdUsed);

		StartCoroutine(PlayGameOverScreen());

		//Analytics.SendPlayerEvent("EndMatch", new Dictionary<string, string>() { { "Score", score.ToString() } });

	}

	public void RunMultiballPowerup()
	{
		Transform mainBall = balls[0].transform;

		foreach (Ball ball in balls)
		{
			if (ball.gameObject.activeInHierarchy)
			{
				mainBall = ball.transform;
			}
		}

		foreach (Ball ball in balls)
		{
			if (!ball.gameObject.activeInHierarchy)
			{
				ball.SetAsMultiball(mainBall);
			}
		}
	}

	public void RunClonePowerup()
	{
		powerupCloneTimerText.gameObject.SetActive(true);
		powerupCloneTimerIcon.SetActive(true);

		clonesPowerupTimer = 10;

		foreach (GameObject clone in clones)
		{
			clone.SetActive(true);
		}
	}

	public void RunSpikedPowerup()
	{
		powerupSpikedballTimerText.gameObject.SetActive(true);
		powerupSpikedballTimerIcon.SetActive(true);

		spikedBallPowerupTimer = 10;

		foreach (Ball ball in balls)
		{
			ball.UpdateSpikePowerup();
		}
	}

	public bool IsSpikedPowerupRunning()
	{
		if (spikedBallPowerupTimer > 0)
		{
			return true;
		}

		return false;
	}

	public void GainLife()
	{
		lives++;
		lives = Mathf.Clamp(lives, 0, 4);
		UpdateLives();
	}

	public void UpdateLives()
	{
		foreach (SpriteRenderer pip in playerLifePips)
		{
			pip.enabled = false;
		}

		for (int i = 0; i < lives; i++)
		{
			playerLifePips[i].enabled = true;
		}

		if (lives == 1)
		{
			playerLifePips[0].color = Color.red;
			playerLifePips[0].GetComponent<SimpleAnim>().animSpeed = 0.1f;
			audioManager.PlaySound("AlmostDead");
		}
		else
		{
			playerLifePips[0].color = playerOwnershipColor;
			playerLifePips[0].GetComponent<SimpleAnim>().animSpeed = 0.5f;
		}
	}

	public void TurnOffPowerup(PowerUpType powerUpType)
	{
		switch (powerUpType)
		{
			case PowerUpType.SpikedBall:
				powerupSpikedballTimerText.gameObject.SetActive(false);
				powerupSpikedballTimerIcon.SetActive(false);
				spikedBallPowerupTimer = 0;
				foreach (Ball ball in balls)
				{
					ball.UpdateSpikePowerup();
				}
				break;
			case PowerUpType.Clone:
				powerupCloneTimerText.gameObject.SetActive(false);
				powerupCloneTimerIcon.SetActive(false);
				clonesPowerupTimer = 0;
				foreach (GameObject clone in clones)
				{
					clone.SetActive(false);
				}
				break;
		}
	}

	public void TurnOffAllPowerups()
	{
		TurnOffPowerup(PowerUpType.SpikedBall);
		TurnOffPowerup(PowerUpType.Clone);
	}

	public void IncreaseSpeed(bool reset = false)
	{
		currentSpeedIncrease += 0.5f;
		if (reset)
		{
			currentSpeedIncrease = 0;
		}
	}

	public void Hit(int points)
	{
		if (points <= 0)
		{
			return;
		}
		points = (int)(points * multiplier);
		multiplier += 0.1f;
		ScorePoints(points);
		CheckIfClearedLevel();
	}

	private void WinLevel()
	{
		audioManager.PlaySound("PowerupGet");
		playerCharacter.Win();
		foreach (Ball ball in balls)
		{
			ball.DeactivateBall();
		}
		StartCoroutine(transitionManager.NextLevelTransition());
	}

	public void ProgressToTheNextLevel()
	{
		currentlevel++;
		totalLevel++;
		if (currentlevel > stages.Count)
		{
			currentlevel = 1;
		}
		playerCharacter.StandBackUp();
		SetupNewMap();
	}

	public void ResetEverythingForANewGame()
	{
		hits = 0;
		score = 0;
		xp = 0;
		timePlayed = 0;

		lives = 4;
		UpdateLives();

		gameOverStatNamesText.text = "";
		gameOverStatNumbersText.text = "";

		menuManager.ResetLeaderboardDisplay();
		SetBallOwnership(PlayerOwnerShip.None);

		hasGameEnded = false;

		playerCharacter.UnLose();

		gameOverBackground.SetActive(false);

		SetupNewMap();

		foreach (Ball ball in balls)
		{
			ball.DeactivateBall();
		}
		TurnOffAllPowerups();
		//Analytics.SendPlayerEvent("StartMatch");
	}

	IEnumerator PlayGameOverScreen()
	{
		audioManager.PlaySound("Lose", 0.5f);
		gameOverBackground.SetActive(true);
		endOfGameStatsText.text = "GAME OVER";

		gameOverSkipPrompt.SetActive(true);

		gameOverTimer1 = 3;
		gameOverTimer2 = 3;

		yield return new WaitUntil(() => gameOverTimer1 <= 0);
		//yield return GetMatchResults();

		playerCharacter.StandBackUp();

		endOfGameStatsText.text = "";

		float secondsPlayed = timePlayed % 60;
		float minutesPlayed = timePlayed / 60;
		float hoursPlayed = timePlayed / 60 / 60;

		string statNames = "TIME PLAYED\n\nSCORE\nHITS\nXP";
		string statValues = hoursPlayed.ToString("0") + ":" + minutesPlayed.ToString("00") + ":" + secondsPlayed.ToString("00") + "\n\n" + score.ToString("0") + "\n" + hits.ToString("0") + "\n+" + xp.ToString("0");

		gameOverStatNamesText.text = statNames.ToUpper();
		gameOverStatNumbersText.text = statValues.ToUpper();
		yield return new WaitUntil(() => gameOverTimer2 <= 0);

		gameOverSkipPrompt.SetActive(false);

		gameOverStatNamesText.text = "";
		gameOverStatNumbersText.text = "";

		menuManager.leaderboardType = 0;
		menuManager.UpdateLeaderboardDisplay();

		menuManager.SetMenuEnabled(true);
	}

	private IEnumerator GetMatchResults()
	{
		print(EventController.GetLastestMatchId());
		string result = null;
		yield return WebRequestHelper.GetRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/matches/wen-game/results",
			$"id={EventController.GetLastestMatchId()}", true, false, resp => result = resp);
		try
		{
			JObject stats = JObject.Parse(result);
			int hits = stats["hits"] != null ? (int)stats["hits"] : 0;
			int misses = stats["misses"] != null ? (int)stats["misses"] : 0;
			int dodges = stats["dodges"] != null ? (int)stats["dodges"] : 0;
			int score = stats["score"] != null ? (int)stats["score"] : 0;
			int machineHits = stats["machine_hits"] != null ? (int)stats["machine_hits"] : 0;
			int xp = stats["xp"] != null ? (int)stats["xp"] : 0;
			int timePlayed = stats["time_played"] != null ? (int)stats["time_played"] : 0;

			//this.hits = hits;
			//this.score = score;
			//this.xp = xp;
			this.timePlayed = timePlayed;


		}
		catch (System.Exception e)
		{
			print(e);
		}
	}

	IEnumerator AnimateScoreText()
	{
		playerScoreText.color = Color.white;

		float a = 0.09f;
		float b = 0.1f;

		Tween<float> scaleTween = new Tween<float>(a, b, 0.2f, TweenEaseType.CubicIn);

		while (!scaleTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			playerScoreText.transform.localScale = new Vector3(0.1f, scaleTween.Update(Time.deltaTime), 0.1f);
		}

		playerScoreText.color = playerOwnershipColor;
	}

	private bool CheckIfClearedLevel()
	{
		for (int i = 0; i < stages[currentlevel - 1].childCount; i++)
		{
			if (stages[currentlevel - 1].GetChild(i).gameObject.activeInHierarchy && !stages[currentlevel - 1].GetChild(i).GetComponent<Brick>().unbreakable)
			{
				return false;
			}
		}

		WinLevel();

		return true;
	}

	private bool IsAtleastOneBallActive()
	{
		foreach (Ball ball in balls)
		{
			if (ball.gameObject.activeInHierarchy)
			{
				return true;
			}
		}

		return false;
	}

	public GameObject GetRandomPowerup()
	{
		return powerups[Random.Range(0, powerups.Count)];
	}

	private void SetupNewMap()
	{
		foreach (Transform stage in stages)
		{
			stage.gameObject.SetActive(false);
		}

		stages[currentlevel - 1].gameObject.SetActive(true);

		foreach (Transform brick in stages[currentlevel -1])
		{
			brick.GetComponent<Brick>().ActivateBrick();
		}

		levelText.text = "STAGE\n<size=10>" + totalLevel.ToString("0");
	}
}

public enum PlayerOwnerShip
{
	None,
	Player1,
	Player2,
}
