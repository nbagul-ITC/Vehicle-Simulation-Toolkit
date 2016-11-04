//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

public class TrafficLightController : MonoBehaviour {
	public GameObject greenLight;
	public GameObject yellowLight;
	public GameObject redLight;
	public float greenTiming = 10.0f;
	public float yellowTiming = 4.0f;
	public float redTiming = 10.0f;
	public float currTime;
	public SpeedLimit speedLimit;
	public float speedLimitNominal = 11.0f;
	public float speedLimitOffset = 3.0f;

	void Start () {
		greenLight.SetActive(true);
		yellowLight.SetActive(false);
		redLight.SetActive(false);
	}
	
	void Update () {
		if (currTime > greenTiming + yellowTiming + redTiming) {
			currTime = 0.0f;
			greenLight.SetActive(true);
			yellowLight.SetActive(false);
			redLight.SetActive(false);
			speedLimit.speedLimit = speedLimitNominal;	
		} else if ((currTime <= yellowTiming + greenTiming) && (currTime >= greenTiming)) {
			greenLight.SetActive(false);
			yellowLight.SetActive(true);
			redLight.SetActive(false);
			speedLimit.speedLimit = speedLimitNominal + Random.Range(0.0f,speedLimitOffset);
		} else if ((currTime > yellowTiming + greenTiming) && (currTime <= greenTiming + yellowTiming + redTiming)) {
			greenLight.SetActive(false);
			yellowLight.SetActive(false);
			redLight.SetActive(true);
			speedLimit.speedLimit = 0.0f;
		}
		currTime = currTime + Time.deltaTime;
	}

	public float TimeUntilGreen() {
		if (currTime < greenTiming) {
			return 0;
		} else {
			return greenTiming + yellowTiming + redTiming - currTime;
		}
	}

	public float TimeUntilRed() {
		if (currTime > greenTiming + yellowTiming) {
			return 0.1f;
		} else {
			return greenTiming + yellowTiming - currTime;
		}
	}
}
