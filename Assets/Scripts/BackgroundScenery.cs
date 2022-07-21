using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScenery : MonoBehaviour
{
    public Transform clouds;
    public float cloudSpeed;
	[Space]
	public Transform water;
	public float waterSpeed;
	public Transform waterParent;
	private float waterHeight;

    private void Update()
    {
		clouds.Translate(Vector3.left * cloudSpeed * Time.deltaTime);

		if (clouds.transform.position.x < -40f)
		{
			clouds.Translate(Vector3.right * 40f);
		}

		waterHeight = Mathf.PingPong(Time.time / 5, 0.2f);
		waterHeight = waterHeight - 0.2f;
		waterParent.localPosition = new Vector3(0, waterHeight);

		water.Translate(Vector3.left * waterSpeed * Time.deltaTime);

		if (water.transform.position.x < -30f)
		{
			water.Translate(Vector3.right * 30f);
		}
	}
}
