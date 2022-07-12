using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreGainUI : MonoBehaviour
{
    public TextMeshProUGUI scoreGainedText;
    public List<Color32> randomPlayer1Colors;
    public List<Color32> randomPlayer2Colors;

    private PlayerOwnerShip playerOwnership;
    private float colorTimer;

    private void Update()
    {
        colorTimer += Time.deltaTime;
        if (colorTimer >= 0.1f)
        {
            if (playerOwnership == PlayerOwnerShip.Player1)
            {
                scoreGainedText.color = randomPlayer1Colors[Random.Range(0, randomPlayer1Colors.Count)];
            }
            else
            {
                scoreGainedText.color = randomPlayer2Colors[Random.Range(0, randomPlayer2Colors.Count)];
            }

            colorTimer = 0;
        }
    }

    public void Initialize(int scoreGainedAmount, PlayerOwnerShip playerOwnership)
    {
        transform.localPosition = new Vector2(Random.Range(-1.5f, 1.5f), -7 + Random.Range(-1.5f, 1.5f));
        scoreGainedText.text = "+" + scoreGainedAmount.ToString("0");
        this.playerOwnership = playerOwnership;
		StartCoroutine(AnimateScoreGained());
    }

    IEnumerator AnimateScoreGained()
    {
		float a = 0;
		float b = 0.1f;

		Tween<float> scalePointMessageTween = new Tween<float>(a, b, 0.3f, TweenEaseType.CubicIn);

		while (!scalePointMessageTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			scoreGainedText.transform.localScale = new Vector3(0.1f, scalePointMessageTween.Update(Time.deltaTime), 0.1f);
		}

		yield return new WaitForSeconds(1.0f);

        Destroy(gameObject);
	}
}
