using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreGainUI : MonoBehaviour
{
    public TextMeshProUGUI scoreGainedText;
	public List<Color32> randomScoreGainedColors;

	public void Initialize(int scoreGainedAmount)
    {
        transform.localPosition = new Vector2(Random.Range(-1.5f, 1.5f), -7 + Random.Range(-1.5f, 1.5f));
        scoreGainedText.text = "+" + scoreGainedAmount.ToString("0");

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

		float timeBetweenRandomColors = 0.1f;
		int repeatTimes = 10;

		while (repeatTimes > 0)
		{
			yield return new WaitForSeconds(timeBetweenRandomColors);
			scoreGainedText.color = randomScoreGainedColors[Random.Range(0, randomScoreGainedColors.Count)];
			repeatTimes--;
		}

		Destroy(gameObject);
	}
}
