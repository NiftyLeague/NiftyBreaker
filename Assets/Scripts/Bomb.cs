using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public Rigidbody2D rigidBody;
    private float speed = 5;

    private void Start()
    {
        Destroy(gameObject, 10);
    }

    private void FixedUpdate()
    {
        rigidBody.velocity = new Vector2(0, -1 * speed);
    }

    public void BlowUpBomb()
    {
        GameplayManager.I.Explosion(transform.position);
        Destroy(gameObject);
    }
}
