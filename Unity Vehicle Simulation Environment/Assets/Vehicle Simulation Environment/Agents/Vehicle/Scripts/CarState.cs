//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;
using UnityStandardAssets.Utility;
using UnityStandardAssets.Vehicles.Car;

namespace VehicleSimulation {
    public class CarState : MonoBehaviour {
        public float elapsedTime;

        private Vector3 prevPosition = new Vector3(0.0f,0.0f,0.0f);
        public Vector3 position;
        public Vector3 orientation;
        public Vector3 velocity;
        public float speed;

        // Environment
        public float speedLimit;
        public string roadType;

        void Update() {
            position = gameObject.transform.position;
            orientation = gameObject.transform.rotation.eulerAngles;
            velocity = new Vector3((position.x-prevPosition.x)/Time.deltaTime,(position.y-prevPosition.y)/Time.deltaTime,(position.z-prevPosition.z)/Time.deltaTime);
            speed = velocity.magnitude;

            elapsedTime = elapsedTime + Time.deltaTime;
            prevPosition = position;
        }
        
    }
}