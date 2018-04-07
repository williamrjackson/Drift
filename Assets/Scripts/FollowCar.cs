using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCar : MonoBehaviour {
	public Transform car;
	public float smoothTime = 0.3F;

	private Vector3 offset;
	private Vector3 velocity = Vector3.zero;


	void Start()
	{
		offset = new Vector3 (transform.position.x - car.position.x, transform.position.y - car.position.y, transform.position.z - car.position.z);
	}

	void LateUpdate () {
		
		Vector3 targetPosition = car.position + offset;
		transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

	}
}
