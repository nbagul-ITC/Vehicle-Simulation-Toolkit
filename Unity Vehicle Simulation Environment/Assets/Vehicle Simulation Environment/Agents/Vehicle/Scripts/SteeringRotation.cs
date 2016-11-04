//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

namespace VehicleSimulation {
    public class SteeringRotation : MonoBehaviour {
        [SerializeField] private Transform target = null;
        [SerializeField] private Vector3 rotationAxisEulerAngles = new Vector3(0f, 0f, 0f);
        [SerializeField] private float maxRotationAngle = 160.0f;

        private Vector3 initialRotation;
        private Vector3 rotationAxis;

        void Start () {
            initialRotation = new Vector3(
                target.rotation.eulerAngles.x,
                target.rotation.eulerAngles.y,
                target.rotation.eulerAngles.z
            );
            rotationAxis = Quaternion.Euler(rotationAxisEulerAngles) * Vector3.up;
            rotationAxis = Quaternion.Euler(initialRotation) * rotationAxis;
        }
        
        void Update () {
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            if(target != null) {
                target.rotation = target.root.rotation * Quaternion.AngleAxis(maxRotationAngle * h, rotationAxis) * Quaternion.Euler(initialRotation);
            }
        }
    }
}
