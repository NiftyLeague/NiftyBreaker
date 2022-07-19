using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScenery : MonoBehaviour
{
    public Transform clouds;
    public float cloudSpeed;

    private void Update()
    {
		clouds.Translate(Vector3.left * cloudSpeed * Time.deltaTime);

		if (clouds.transform.position.x < -40f)
		{
			clouds.Translate(Vector3.right * 40f);
		}
	}
}
