using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {

	private Vector3 Ball;
	private float speed = 15.0f;
	private float zValue;

	void Start()
	{
		Ball = transform.position;
		zValue = Mathf.Abs(transform.position.z - Camera.main.transform.position.z);
	}

	void Update()
	{
		Ball = new Vector3(Input.mousePosition.x, Input.mousePosition.y, zValue);
		Ball = Camera.main.ScreenToWorldPoint(Ball);
		transform.position = Vector3.Lerp(transform.position, Ball, speed * Time.deltaTime);
	}
 
}
