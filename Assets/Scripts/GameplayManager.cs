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
	public Character playerCharacter;
	[Space]
	public int level = 1;
	public int player1Lives = 4;
	public int player2Lives = 4;
	public float multiplier = 1.0f;
	public PlayerOwnerShip currentPlayerOwnership;
	[Space]
	public ObscuredInt score;
	public ObscuredFloat timePlayed;
	public ObscuredInt hits;
	public ObscuredInt xp;
	[Space]
	public List<GameObject> player1LifePips;
	public List<GameObject> player2LifePips;
	public Ball ball;
	public List<Brick> bricks;
	[Space]
	public ObscuredFloat currentSpeedIncrease;
	public CameraShake cameraShake;
	[Space]
	public TextMeshProUGUI scoreText;
	public TextMeshProUGUI gameOverStatNamesText;
	public TextMeshProUGUI gameOverStatNumbersText;
	public GameObject gameOverSkipPrompt;
	public GameObject scoreGainedUIPrefab;
	public Transform player1Canvas;
	[Space]
	public Transform worldLeftLimit;
	public Transform worldRightLimit;

	public ObscuredFloat maxBallSpeed;
	public Vector2 startTimeoutRange;
	public Vector2 shootRandomTimeoutRange;
	public ObscuredBool hasGameEnded;

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
		UpdateScoreText();
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
		int scoreGainedAmount = amount;
		score += scoreGainedAmount;
		hits++;
		var newScoreGainedUI = Instantiate(scoreGainedUIPrefab, player1Canvas);
		newScoreGainedUI.GetComponent<ScoreGainUI>().Initialize(scoreGainedAmount);
		if (currentScoreTextCoroutine != null)
		{
			StopCoroutine(currentScoreTextCoroutine);
		}
		currentScoreTextCoroutine = StartCoroutine(AnimateScoreText());
		IncreaseSpeed();
		UpdateScoreText();
		audioManager.PlaySound(AudioManager.SoundID.gainPoint);
		//EventController.AddScore(scoreGainedAmount);
	}

	void UpdateScoreText()
	{
		string multiplierText = "";
		if (multiplier > 1.0f)
		{
			multiplierText = "<size=6>x" + multiplier.ToString("0.0");
		}
		scoreText.text = score.ToString("0") + "   " + multiplierText;
	}

	public void LoseBall()
	{
		audioManager.PlaySound(AudioManager.SoundID.lose);
		if (currentPlayerOwnership == PlayerOwnerShip.Player1)
		{
			player1Lives--;

			if (player1Lives <= 0)
			{
				Lose();
				return;
			}
		}
		else if (currentPlayerOwnership == PlayerOwnerShip.Player2)
		{
			player2Lives--;

			if (player2Lives <= 0)
			{
				Lose();
				return;
			}
		}
		UpdateLives();
		Reset();
	}

	private void Reset()
	{
		ball.ResetBall();
		multiplier = 1.0f;
		SetBallOwnership(PlayerOwnerShip.None);
		UpdateScoreText();
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
				ball.spriteRenderer.color = Color.blue;
				break;
			case PlayerOwnerShip.Player2:
				ball.spriteRenderer.color = Color.red;
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
		playerCharacter.Lose();
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

		for (int i = 0; i < player1Lives; i++)
		{
			player1LifePips[i].SetActive(true);
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
			Debug.Log("WON THE STAGE!");
		}
	}

	public void ResetEverythingForANewGame()
	{
		hits = 0;
		score = 0;
		xp = 0;
		timePlayed = 0;

		player1Lives = 4;
		player2Lives = 4;
		UpdateLives();

		gameOverStatNamesText.text = "";
		gameOverStatNumbersText.text = "";

		menuManager.ResetLeaderboardDisplay();
		SetBallOwnership(PlayerOwnerShip.None);

		hasGameEnded = false;

		playerCharacter.UnLose();

		Reset();
		//Analytics.SendPlayerEvent("StartMatch");
	}

	IEnumerator PlayGameOverScreen()
	{
		audioManager.PlaySound(AudioManager.SoundID.lose);

		scoreText.text = "GAME OVER";

		gameOverSkipPrompt.SetActive(true);

		gameOverTimer1 = 3;
		gameOverTimer2 = 3;

		yield return new WaitUntil(() => gameOverTimer1 <= 0);
		//yield return GetMatchResults();

		playerCharacter.StandBackUp();

		scoreText.text = "";

		float secondsPlayed = timePlayed % 60;
		float minutesPlayed = timePlayed / 60;
		float hoursPlayed = timePlayed / 60 / 60;

		string statNames = "SCORE\nTIME PLAYED\nTOTAL BALLs\nHITS\nMISSES";
		string statValeues = score.ToString("0") + "\n" + hoursPlayed.ToString("0") + ":" + minutesPlayed.ToString("00") + ":" + secondsPlayed.ToString("00") + "\n";

		if (xp > 0)
		{
			statNames += "\nXP";
			statValeues += "\n+" + xp.ToString("0");
		}

		gameOverStatNamesText.text = statNames.ToUpper();
		gameOverStatNumbersText.text = statValeues.ToUpper();
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

			this.hits = hits;
			this.score = score;
			this.xp = xp;
			this.timePlayed = timePlayed;


		}
		catch (System.Exception e)
		{
			print(e);
		}
	}

	IEnumerator AnimateScoreText()
	{
		float a = 0.09f;
		float b = 0.1f;

		Tween<float> scaleTween = new Tween<float>(a, b, 0.2f, TweenEaseType.CubicIn);

		while (!scaleTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			scoreText.transform.localScale = new Vector3(0.1f, scaleTween.Update(Time.deltaTime), 0.1f);
		}
	}


	private bool Cleared()
	{
		for (int i = 0; i < this.bricks.Count; i++)
		{
			if (this.bricks[i].gameObject.activeInHierarchy && !this.bricks[i].unbreakable)
			{
				return false;
			}
		}

		return true;
	}
}

public enum PlayerOwnerShip
{
	None,
	Player1,
	Player2,
}
