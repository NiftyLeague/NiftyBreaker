using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseZone : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Ball ball = collision.gameObject.GetComponent<Ball>();

        if (ball != null)
        {
            ball.DeactivateBall();
            FindObjectOfType<GameplayManager>().LoseBall();
        }
    }
}
