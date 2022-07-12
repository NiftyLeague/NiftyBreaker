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
	public AudioManager audioManager;
	public Character player1Character;
	public Character player2Character;
	[Space]
	public int level = 1;
	public int player1Lives = 4;
	public int player2Lives = 4;
	public float multiplier = 1.0f;
	public PlayerOwnerShip currentPlayerOwnership;
	public Color32 player1OwnershipColor;
	public Color32 player2OwnershipColor;
	[Space]
	public ObscuredFloat timePlayed;
	public ObscuredInt player1Score;
	public ObscuredInt player2Score;
	public ObscuredInt player1Hits;
	public ObscuredInt player2Hits;
	public ObscuredInt player1Xp;
	public ObscuredInt player2Xp;
	[Space]
	public List<GameObject> player1LifePips;
	public List<GameObject> player2LifePips;
	public Ball ball;
	public List<Transform> stages;
	public GameObject powerup;
	[Space]
	public ObscuredFloat currentSpeedIncrease;
	public CameraShake cameraShake;
	[Space]
	public TextMeshProUGUI endOfGameStatsText;
	public TextMeshProUGUI player1NameText;
	public TextMeshProUGUI player2NameText;
	public TextMeshProUGUI player1ScoreText;
	public TextMeshProUGUI player2ScoreText;
	public TextMeshProUGUI gameOverStatNamesText;
	public TextMeshProUGUI gameOverStatNumbersText;
	public GameObject gameOverBackground;
	public GameObject gameOverSkipPrompt;
	public GameObject scoreGainedUIPrefab;
	public Transform player1Canvas;
	public Transform player2Canvas;
	[Space]
	public Transform worldLeftLimit;
	public Transform worldRightLimit;

	public ObscuredFloat maxBallSpeed;
	public Vector2 startTimeoutRange;
	public Vector2 shootRandomTimeoutRange;
	public ObscuredBool hasGameEnded;
	public ObscuredFloat powerUpSpawnChance = 0.1f;
	[Space]
	public bool multiplayerMode;
	public bool godMode;

	Coroutine currentScoreTextCoroutine;

	float gameOverTimer1;
	float gameOverTimer2;

	private InputState input = new InputState();

	private void Awake()
	{
		I = this;
	}

	void Start()
	{
		player2Character.gameObject.SetActive(multiplayerMode);
		player2ScoreText.gameObject.SetActive(multiplayerMode);
		player2NameText.gameObject.SetActive(multiplayerMode);

		UpdateScoreTexts();
		ResetEverythingForANewGame();
		menuManager.menuPanel.SetActive(false);
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
					audioManager.PlaySound(AudioManager.SoundID.menuOptionSelect);
					return;
				}
				else if (gameOverTimer2 > 0)
				{
					gameOverTimer2 = 0;
					audioManager.PlaySound(AudioManager.SoundID.menuOptionSelect);
					return;
				}
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

		switch (currentPlayerOwnership)
		{
			case PlayerOwnerShip.Player1:
				player1Score += scoreGainedAmount;
				player1Hits++;

				var newScoreGainedUIPlayer1 = Instantiate(scoreGainedUIPrefab, player1Canvas);
				newScoreGainedUIPlayer1.GetComponent<ScoreGainUI>().Initialize(scoreGainedAmount, PlayerOwnerShip.Player1);
				currentScoreTextCoroutine = StartCoroutine(AnimatePlayer1ScoreText());
				break;
			case PlayerOwnerShip.Player2:
				player2Score += scoreGainedAmount;
				player2Hits++;

				var newScoreGainedUIPlayer2 = Instantiate(scoreGainedUIPrefab, player2Canvas);
				newScoreGainedUIPlayer2.GetComponent<ScoreGainUI>().Initialize(scoreGainedAmount, PlayerOwnerShip.Player2);
				currentScoreTextCoroutine = StartCoroutine(AnimatePlayer2ScoreText());
				break;
		}

		IncreaseSpeed();
		UpdateScoreTexts();
		audioManager.PlaySound(AudioManager.SoundID.gainPoint);
		//EventController.AddScore(scoreGainedAmount);
	}

	void UpdateScoreTexts()
	{
		string multiplierText = "";
		if (multiplier > 1.0f)
		{
			multiplierText = "   <size=6>x" + multiplier.ToString("0.0");
		}

		player1ScoreText.text = player1Score.ToString("0");
		player2ScoreText.text = player2Score.ToString("0");

		switch (currentPlayerOwnership)
		{
			case PlayerOwnerShip.Player1:
				player1ScoreText.text += multiplierText;
				break;
			case PlayerOwnerShip.Player2:
				player2ScoreText.text += multiplierText;
				break;
		}
	}

	public void LoseBall()
	{
		if (currentPlayerOwnership == PlayerOwnerShip.None)
		{
			audioManager.PlaySound(AudioManager.SoundID.loseNoOwnership);
		}
		else
		{
			audioManager.PlaySound(AudioManager.SoundID.lose);
		}

		if (currentPlayerOwnership == PlayerOwnerShip.Player1 && !godMode)
		{
			player1Lives--;
			UpdateLives();
			if (player1Lives <= 0)
			{
				Lose();
				return;
			}
		}
		else if (currentPlayerOwnership == PlayerOwnerShip.Player2 && !godMode)
		{
			player2Lives--;
			UpdateLives();
			if (player2Lives <= 0)
			{
				Lose();
				return;
			}
		}
		Reset();
	}

	private void Reset()
	{
		ball.ResetBall();
		multiplier = 1.0f;
		SetBallOwnership(PlayerOwnerShip.None);
		UpdateScoreTexts();
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
				ball.spriteRenderer.color = Color.white;
				break;
			case PlayerOwnerShip.Player1:
				ball.spriteRenderer.color = player1OwnershipColor;
				break;
			case PlayerOwnerShip.Player2:
				ball.spriteRenderer.color = player2OwnershipColor;
				break;
		}
	}

	public void Lose()
	{
		if (hasGameEnded)
		{
			return;
		}
		hasGameEnded = true;
		cameraShake.Shake(0.5f, 5);
		IncreaseSpeed(true);
		player1Character.Lose();
		ball.gameObject.SetActive(false);
		menuManager.UpdateLeaderboards();
		//EventController.AddMatchEnd(PlayerSpriteManager.lastDegenIdUsed);

		StartCoroutine(PlayGameOverScreen());

		//Analytics.SendPlayerEvent("EndMatch", new Dictionary<string, string>() { { "Score", score.ToString() } });

	}

	public void UpdateLives()
	{
		foreach (GameObject pip in player1LifePips)
		{
			pip.SetActive(false);
		}

		foreach (GameObject pip in player2LifePips)
		{
			pip.SetActive(false);
		}

		for (int i = 0; i < player1Lives; i++)
		{
			player1LifePips[i].SetActive(true);
		}

		for (int i = 0; i < player2Lives; i++)
		{
			player2LifePips[i].SetActive(true);
		}
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
		points = (int)(points * multiplier);
		multiplier += 0.1f;
		ScorePoints(points);

		if (Cleared())
		{
			level++;
			if (level > stages.Count)
			{
				level = 1;
			}
			Reset();
			SetupNewMap();
		}
	}

	public void ResetEverythingForANewGame()
	{
		player1Hits = 0;
		player1Score = 0;
		player1Xp = 0;
		player2Hits = 0;
		player2Score = 0;
		player2Xp = 0;
		timePlayed = 0;

		player1Lives = 4;
		player2Lives = 4;
		UpdateLives();

		gameOverStatNamesText.text = "";
		gameOverStatNumbersText.text = "";

		menuManager.ResetLeaderboardDisplay();
		SetBallOwnership(PlayerOwnerShip.None);

		hasGameEnded = false;

		player1Character.UnLose();
		player2Character.UnLose();

		gameOverBackground.SetActive(false);

		SetupNewMap();

		Reset();
		//Analytics.SendPlayerEvent("StartMatch");
	}

	IEnumerator PlayGameOverScreen()
	{
		audioManager.PlaySound(AudioManager.SoundID.lose);
		gameOverBackground.SetActive(true);
		endOfGameStatsText.text = "GAME OVER";

		gameOverSkipPrompt.SetActive(true);

		gameOverTimer1 = 3;
		gameOverTimer2 = 3;

		yield return new WaitUntil(() => gameOverTimer1 <= 0);
		//yield return GetMatchResults();

		player1Character.StandBackUp();
		player2Character.StandBackUp();

		endOfGameStatsText.text = "";

		float secondsPlayed = timePlayed % 60;
		float minutesPlayed = timePlayed / 60;
		float hoursPlayed = timePlayed / 60 / 60;

		string statNames = "TIME PLAYED\n\nSCORE\nHITS\nXP";
		string statValues = hoursPlayed.ToString("0") + ":" + minutesPlayed.ToString("00") + ":" + secondsPlayed.ToString("00") + "\n\n" + player1Score.ToString("0") + "\n" + player1Hits.ToString("0") + "\n+" + player1Xp.ToString("0");

		if (multiplayerMode)
		{
			statNames = "TIME PLAYED\n\nPLAYER 1 SCORE\nPLAYER 1 HITS\nPLAYER 1 XP\n\nPLAYER 2 SCORE\nPLAYER 2 HITS\nPLAYER 2 XP";
			statValues = hoursPlayed.ToString("0") + ":" + minutesPlayed.ToString("00") + ":" + secondsPlayed.ToString("00") + "\n\n" + player1Score.ToString("0") + "\n" + player1Hits.ToString("0") + "\n+" + player1Xp.ToString("0");
            statValues += "\n\n" + player2Score.ToString("0") + "\n" + player2Hits.ToString("0") + "\n+" + player2Xp.ToString("0");
		}

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

	IEnumerator AnimatePlayer1ScoreText()
	{
		float a = 0.09f;
		float b = 0.1f;

		Tween<float> scaleTween = new Tween<float>(a, b, 0.2f, TweenEaseType.CubicIn);

		while (!scaleTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			player1ScoreText.transform.localScale = new Vector3(0.1f, scaleTween.Update(Time.deltaTime), 0.1f);
		}
	}

	IEnumerator AnimatePlayer2ScoreText()
	{
		float a = 0.09f;
		float b = 0.1f;

		Tween<float> scaleTween = new Tween<float>(a, b, 0.2f, TweenEaseType.CubicIn);

		while (!scaleTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			player2ScoreText.transform.localScale = new Vector3(0.1f, scaleTween.Update(Time.deltaTime), 0.1f);
		}
	}

	private bool Cleared()
	{
		for (int i = 0; i < stages[level-1].childCount; i++)
		{
			if (stages[level-1].GetChild(i).gameObject.activeInHierarchy && !stages[level-1].GetChild(i).GetComponent<Brick>().unbreakable)
			{
				return false;
			}
		}

		return true;
	}

	private void SetupNewMap()
	{
		foreach (Transform stage in stages)
		{
			stage.gameObject.SetActive(false);
		}

		stages[level - 1].gameObject.SetActive(true);

		foreach (Transform brick in stages[level-1])
		{
			if (Random.value <= powerUpSpawnChance)
			{
				brick.GetComponent<Brick>().AddPowerup();
			}
		}
	}
}

public enum PlayerOwnerShip
{
	None,
	Player1,
	Player2,
}
