//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System;

public class SimpleCarController : MonoBehaviour {
	public WheelCollider[] wheelColliders = new WheelCollider[4];
	public GameObject[] wheelMeshes = new GameObject[4];

	public float maxSteering;
	public float maxThrottle;

	void Update() {
		Move(Input.GetAxis("Vertical"),Input.GetAxis("Horizontal"));
	}

	public void Move(float throttle, float steering) {
		// Steering input: [-1,1]
		// Throttle input: [0,1]
		steering = Mathf.Clamp(steering,-1,1);
		throttle = Mathf.Clamp(throttle,0,1);
		foreach (WheelCollider wc in wheelColliders) {
			wc.motorTorque = throttle*maxThrottle;
		}
		wheelColliders[0].steerAngle = steering*maxSteering;
		wheelColliders[1].steerAngle = steering*maxSteering;
	}
}
