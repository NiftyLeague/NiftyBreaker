using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TransitionManager : MonoBehaviour
{
    public GameplayManager gameplayManager;
    [Space]
    public SpriteRenderer background;
    public TextMeshProUGUI currentLevelText;

    private void Start()
    {
        background.color = new Color(0, 0, 0, 1);
    }

    void ResetEverything()
    {
        background.color = new Color(0, 0, 0, 0);
    }

    public IEnumerator FirstStartTransition()
    {
        yield return StartCoroutine(NextLevelOpeningTransition());

        gameplayManager.ResetBall();
    }

    public IEnumerator NextLevelTransition()
    {
        ResetEverything();

        yield return new WaitForSeconds(1);

        Tween<float> backgroundScreenAlpha = new Tween<float>(0, 1f, 1.0f, TweenEaseType.CubicOut);

        while (!backgroundScreenAlpha.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            background.color = new Color(0, 0, 0, backgroundScreenAlpha.Update(Time.deltaTime));
        }

        gameplayManager.ProgressToTheNextLevel();
        yield return StartCoroutine(NextLevelOpeningTransition());

        gameplayManager.ResetBall();
    }

    private IEnumerator NextLevelOpeningTransition()
    {
        currentLevelText.text = "Level " + gameplayManager.currentlevel.ToString("0") + "!";

        Tween<float> backgroundScreenAlpha2 = new Tween<float>(1, 0f, 1.0f, TweenEaseType.CubicIn);

        while (!backgroundScreenAlpha2.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            background.color = new Color(0, 0, 0, backgroundScreenAlpha2.Update(Time.deltaTime));
        }

        Tween<float> wordMove1 = new Tween<float>(30, 0, 1f, TweenEaseType.CubicOut);

        while (!wordMove1.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            currentLevelText.transform.position = new Vector2(wordMove1.Update(Time.deltaTime), 0);
        }

        yield return new WaitForSeconds(0.5f);

        Tween<float> wordMove2 = new Tween<float>(0, -30, 1f, TweenEaseType.CubicIn);

        while (!wordMove2.IsEnded())
        {
            yield return new WaitForEndOfFrame();
            currentLevelText.transform.position = new Vector2(wordMove2.Update(Time.deltaTime), 0);
        }
    }
}
