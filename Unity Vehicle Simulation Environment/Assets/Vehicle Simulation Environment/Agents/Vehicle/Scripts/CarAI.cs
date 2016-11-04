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
    public class CarAI : MonoBehaviour {
        // Vehicle Components
        private CarController m_Car;
        public Rigidbody m_RigidBody;

        // Optional Steering Wheel
        public Transform steeringWheel;
        public Vector3 rotationAxisEulerAngles = new Vector3(25.0f, 0f, 0f);
        private float maxRotationAngle = 160.0f;
        //private string steerAxisName = "Horizontal";
        private Vector3 initialSteeringPosition;
        private Vector3 initialCarPosition;
        private Vector3 steeringRotationAxis;

        // Vehicle Control Properties
        public Transform m_Target;
        private GameObject targetCar;
        private Rigidbody targetRigidBody;
        private SpeedLimit m_speedLimit;
        public float throttle = 0.0f;
        public float steeringAngle = 0.0f;

        public float currSpeedLimit = 0.0f;
        public float tolerance = 0.1f;

        // Vehicle Sensor Properties
        private float sensorRange = 30.0f;
        private float handbrakeDistance = 5.0f;
        // public float sensorWidth = 5.0f;
        // public int speedLimitOffset = 0;

        // Vehicle State Properties
        private bool isFollowing = false;
        private bool hasCollided = false;
        private bool withinHandbrakeDistance = false;
        public bool forceStop = false;
        public bool selfDriving = true;
        public bool selfSteering = true;
        public bool selfLaneChanging = false;
        public bool stopIfCollision = false;
        public bool scriptControl = false;

        // Script vars
        public float scriptThrottle = 0.0f;
        public float scriptSteering = 0.0f;

        // Previous State
        private Vector3 prevPosition = new Vector3(0.0f,0.0f,0.0f);
        //private Vector3 prevVelocity = new Vector3(0.0f,0.0f,0.0f);
        public float prevSpeed = 0.0f;
        private float i_term = 0.0f;

        // Follow PID Parameters
        public float followKp = 1.0f;
        private float followKd = 0.0f;
        private float followKi = 0.0f;
        private float followDistance = 5.0f;

        // Cruise PID Parameters
        public float cruiseKp = 1.0f;
        private float cruiseKd = 0.0f;
        private float cruiseKi = 0.0f;
        public float setPointSpeed = 10.0f;

        // Control Scaling Factors
        private float steeringCoefficient = 0.1f;
        private float followDistanceCoefficient = 0.5f;
        // public float mergeProbability = 100;

        // Random Variables
        public float speedLimitOffsetRange = 2.0f;
        public float speedLimitOffsetRangeMultiplier = 0.1f;
        public float steeringOffsetRange = 0.01f;
        public float distanceSensorOffsetRange = 2.0f;
        public float followDistanceOffsetRange = 3.0f;
        public float handbrakeDistanceOffsetRange = 1.0f;
        public float speedLimitTimer = 3.0f;
        public float currSpeedLimitTime = 0.0f;
        public float laneChangeThreshold = 0.9995f;

        // Debug
        public bool debug = false;
        public Text GUIText;

        public void increaseSeverity(float i) {
            followKp = 0.5f + 9.5f * (i/100.0f);
            cruiseKp = 0.5f + 9.5f * (i/100.0f);
            speedLimitOffsetRange = 2.0f + 10.0f * (i/100.0f);
            // Mass differential
        }

        public void increaseFrequency(float i){
            //distanceSensorOffsetRange = 2.0f*i;
            //handbrakeDistanceOffsetRange = i;
            handbrakeDistance = 5.0f - 5.0f*(i/100.0f);
            sensorRange = 35.0f - 25.0f*(i/100.0f);
            followDistance = 5.0f - 10.0f*(i/100.0f);
            steeringCoefficient = 0.1f + 0.5f*(i/100.0f);
            followDistanceCoefficient = 0.5f - 0.3f*(i/100.0f);
        }

        public void increaseLaneChanging(float i){
            laneChangeThreshold = 0.999f - 0.05f*(i/100.0f);
        }

        public void increaseSteering(float i){
            steeringCoefficient = 0.1f + 1.0f*(i/100.0f);
        }

        private void Awake() {
            m_Car = GetComponent<CarController>();
            m_RigidBody = GetComponent<Rigidbody>();
            hasCollided = false;

            if (steeringWheel != null) {
                initialSteeringPosition = new Vector3(
                    steeringWheel.localRotation.eulerAngles.x,
                    steeringWheel.localRotation.eulerAngles.y,
                    steeringWheel.localRotation.eulerAngles.z
                );
                steeringRotationAxis = Quaternion.Euler(rotationAxisEulerAngles) * Vector3.up;
                steeringRotationAxis = Quaternion.Euler(initialSteeringPosition) * steeringRotationAxis ;
            }

            followDistance = followDistance + Random.Range(-followDistanceOffsetRange,followDistanceOffsetRange);
            handbrakeDistance = handbrakeDistance + Random.Range(-handbrakeDistanceOffsetRange, handbrakeDistanceOffsetRange);
            sensorRange = sensorRange + Random.Range(-distanceSensorOffsetRange, distanceSensorOffsetRange);
        }

        private void FixedUpdate() {
            if (selfDriving) {                
                activateDistanceSensor();
                activateEmergencySensor();
                if (selfLaneChanging) {
                    float laneChangeAssist = Random.Range(0.0f,1.0f);
                    if (laneChangeAssist > laneChangeThreshold) {
                        activateLaneChange();
                    }
                }
                if ((hasCollided && stopIfCollision) || withinHandbrakeDistance || forceStop) {
                    activateHandbrake();
                } else if (isFollowing) {
                    activateDistanceControl();
                } else {
                    activateCruiseControl();
                }
            } else if (scriptControl) {
                m_Car.Move(scriptSteering, scriptThrottle, scriptThrottle, 0.0f);
            } else {
                activateUserControl();
            }
        }

        private void OnTriggerStay(Collider col) {
            if (col.gameObject.tag == "Speed Limit") {
                if ((m_speedLimit == null) || (m_speedLimit.speedLimit != ((SpeedLimit) col.gameObject.GetComponent("SpeedLimit")).speedLimit) || (currSpeedLimitTime > speedLimitTimer))
                {
                    currSpeedLimitTime = 0.0f;
                    m_speedLimit = (SpeedLimit) col.gameObject.GetComponent("SpeedLimit");
                    currSpeedLimit = m_speedLimit.speedLimit;
                    setPointSpeed = m_speedLimit.speedLimit + Random.Range(-1.0f * (speedLimitOffsetRange + speedLimitOffsetRangeMultiplier * m_speedLimit.speedLimit), (speedLimitOffsetRange + speedLimitOffsetRangeMultiplier * m_speedLimit.speedLimit));
                    i_term = 0.0f;
                    // Prevent a negative speed limit
                    if (setPointSpeed < 0) {
                        setPointSpeed = 0;
                    }
                } else {
                    currSpeedLimitTime = currSpeedLimitTime + Time.deltaTime;
                }

                if (debug){
                    UnityEngine.Debug.Log("Speed Limit: " + setPointSpeed.ToString()); 
                }

            }
        }

        private void OnCollisionEnter(Collision collision) {
            hasCollided = true;
        }

        public void toggleForceStop() {
            forceStop = !forceStop;
        }

        private void activateHandbrake() {
            m_Car.Move(0, -0.0f, -0.0f, 1.0f);
        }

        private void activateFullReverse() {
            m_Car.Move(0, -1.0f, -1.0f, 1.0f);
        }

        public float getSteering() {
            if (m_Target == null || !selfSteering) {
                return 0.0f;
            } else {
                Vector3 offsetTargetPos = m_Target.position;
                Vector3 localTarget = transform.InverseTransformPoint(offsetTargetPos);
                float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
                // Map velocity to a coefficient for reducing angle for steering
                targetAngle = targetAngle * (1.0f / m_RigidBody.velocity.magnitude) * steeringCoefficient + Random.Range(-steeringOffsetRange,steeringOffsetRange);
                // Changed sign term
                float steer = Mathf.Clamp(targetAngle, -1, 1) * Mathf.Sign(m_RigidBody.velocity.magnitude);
                return steer;
            }
        }

        private void moveSteeringWheel(float steeringInput) {
            // SteeringInput between -1 and 1
            if ((steeringWheel != null) && (Mathf.Abs(steeringInput) > 0.01)) {
                steeringWheel.localRotation =  Quaternion.AngleAxis(maxRotationAngle * steeringInput, steeringRotationAxis) * Quaternion.Euler(initialSteeringPosition);
            }
        }

        private void activateUserControl() {
            float steeringInput = CrossPlatformInputManager.GetAxis("Horizontal");
            steeringAngle = steeringInput;
            throttle = CrossPlatformInputManager.GetAxis("Vertical");
            float handbrake = CrossPlatformInputManager.GetAxis("Jump");
            m_Car.Move(steeringInput, throttle, throttle, handbrake);
            moveSteeringWheel(steeringInput);
            UnityEngine.Debug.Log("User Control",gameObject);
        }

        public void leftLaneChange() {
            WaypointProgressTracker customPT = transform.GetComponentInParent<WaypointProgressTracker>();
            ConnectedWaypoints z = customPT.circuit.transform.GetComponentInParent<ConnectedWaypoints>();
            WaypointCircuit nextCircuit = z.GetLeftCircuit();
            if (nextCircuit != null) {
                customPT.circuit = nextCircuit;
            }
        }

        public void rightLaneChange() {
            WaypointProgressTracker customPT = transform.GetComponentInParent<WaypointProgressTracker>();
            ConnectedWaypoints z = customPT.circuit.transform.GetComponentInParent<ConnectedWaypoints>();
            WaypointCircuit nextCircuit = z.GetRightCircuit();
            if (nextCircuit != null) {
                customPT.circuit = nextCircuit;
            }
        }

        public void toggleSelfDriving(){
            selfDriving = !selfDriving;
            selfSteering = !selfSteering;
        }

        private void activateDistanceControl() {
            float currDistance = Mathf.Abs(Vector3.Distance(m_RigidBody.position, targetRigidBody.position));
            float scaledFollowDistance = followDistance + m_RigidBody.velocity.magnitude * followDistanceCoefficient;
            drawTargetDistanceSensor(scaledFollowDistance);

            if (m_RigidBody.velocity.magnitude < setPointSpeed) {
                Vector3 currPosition = m_RigidBody.position;
                float positionError = - scaledFollowDistance + currDistance;
                if (Mathf.Abs(positionError) < tolerance) {
                    positionError = 0.0f;
                }
                float p_term = followKp * positionError;
                float d_term = followKd * Vector3.Distance(currPosition, prevPosition) / Time.deltaTime;
                i_term = followKi * (i_term + positionError * Time.deltaTime);
                float controlOutput = p_term + d_term + i_term;
                prevPosition = currPosition;
                float steeringInput = getSteering();
                steeringAngle = steeringInput;
                throttle = controlOutput;
                float handbrake = 0.0f;
                //if (debug) {
                //    UnityEngine.Debug.Log("Throttle: " + throttle.ToString()); 
                //}
                if (withinHandbrakeDistance || (m_RigidBody.velocity.magnitude < tolerance)) {
                    handbrake = 1.0f;
                }
                if ((m_RigidBody.velocity.magnitude < tolerance*4.0f) && (throttle < -1.0f*tolerance)) {
                    throttle = 0.0f;
                }
                m_Car.Move(steeringInput, throttle, throttle, handbrake);
                moveSteeringWheel(steeringInput);

            } else {
                activateCruiseControl();
            }

            // UnityEngine.Debug.Log("Set Follow Distance: " + scaledFollowDistance.ToString() + " Curr Distance: " + currDistance.ToString(), gameObject);
        }

        private void activateCruiseControl() {
            float currSpeed = Mathf.Sqrt(Mathf.Pow(m_RigidBody.velocity[0],2.0f)+Mathf.Pow(m_RigidBody.velocity[1],2.0f)+Mathf.Pow(m_RigidBody.velocity[2],2.0f));
            float speedError = setPointSpeed - Mathf.Abs(currSpeed);
            if (float.IsNaN(speedError) || Mathf.Abs(speedError) < tolerance) {
                speedError = 0.0f;
            }
            float p_term = cruiseKp * speedError;
            float d_term = cruiseKd * (currSpeed - prevSpeed) / Time.deltaTime;
            i_term = cruiseKi * (i_term + speedError * Time.deltaTime);
            float controlOutput = p_term + d_term + i_term;
            prevSpeed = currSpeed;
            //prevVelocity = m_RigidBody.velocity;
            prevPosition = m_RigidBody.position;
            float steeringInput = getSteering();
            steeringAngle = steeringInput;
            throttle = controlOutput;
            float handbrake = 0.0f;
            m_Car.Move(steeringInput, throttle, throttle, handbrake);
            moveSteeringWheel(steeringInput);
        }

        private void activateEmergencySensor() {
            bool carPresent = false;
            float range = handbrakeDistance + m_RigidBody.velocity.magnitude * 1.0f;
            RaycastHit[] hits;
            List<Ray> landingRays = generateFrontSensors(range, 3.5f);
            for (int j = 0; j < landingRays.Count; j++) {
                Ray landingRay = landingRays[j]; 
                hits = Physics.RaycastAll(landingRay, range);
                for (int i = 0; i < hits.Length; i++) {
                    RaycastHit hit = hits[i];
                    if (hit.collider.tag == "Car") {   
                        activateHandbrake();
                        withinHandbrakeDistance = true;
                        carPresent = true;
                        break;
                    }
                }
            }
            if(!carPresent){
                withinHandbrakeDistance = false;
            }
            // Sometimes locked in place
            m_Car.Move(0.0f, -0.0001f, -0.0001f, 0.0f);
            drawEmergencySensor(range);
        }

        private List<Ray> generateFrontSensors(float range, float width)
        {
            List<Ray> landingRays = new List<Ray>();
            landingRays.Add(new Ray(transform.position + transform.up, transform.forward * range));
            landingRays.Add(new Ray(transform.position + transform.up + (transform.right * width/4.0f), transform.forward * range));
            landingRays.Add(new Ray(transform.position + transform.up - (transform.right * width/4.0f), transform.forward * range));
            landingRays.Add(new Ray(transform.position + transform.up + (transform.right * width/2.0f), transform.forward * range));
            landingRays.Add(new Ray(transform.position + transform.up - (transform.right * width/2.0f), transform.forward * range));
            return landingRays;
        }

        private void activateDistanceSensor() {
            bool carPresent = false;
            RaycastHit[] hits;
            List<Ray> landingRays = generateFrontSensors(sensorRange,3.5f);
            for (int j = 0; j < landingRays.Count; j++) {
                Ray landingRay = landingRays[j]; 
                hits = Physics.RaycastAll(landingRay, sensorRange);
                for (int i = 0; i < hits.Length; i++) {
                    RaycastHit hit = hits[i];
                    //Renderer rend = hit.transform.GetComponent<Renderer>();
                    if (hit.collider.tag == "Car") {
                        // Make sure it corresponds to the closest car
                        // Need to check if cars belong to the same Waypoint path
                        if (!isFollowing) {
                            i_term = 0.0f;
                        }
                        isFollowing = true;
                        targetCar = hit.collider.gameObject;
                        targetRigidBody = targetCar.GetComponentInParent<Rigidbody>();
                        carPresent = true;
                        break;
                    }
                }
            }
            if (!carPresent) {
                if (isFollowing) {
                    i_term = 0.0f;
                }
                isFollowing = false;
                targetCar = null;
                targetRigidBody = null;
            }
            drawDistanceSensor();
        }

        private void activateLaneChange() {
            WaypointProgressTracker customPT = transform.GetComponentInParent<WaypointProgressTracker>();
            ConnectedWaypoints z = customPT.circuit.transform.GetComponentInParent<ConnectedWaypoints>();
            WaypointCircuit nextCircuit = z.GetAdjacentCircuit();
            if (nextCircuit != null) {
                customPT.circuit = nextCircuit;
            }
        }

        public void SetTarget(Transform target) {
            m_Target = target;
        }

        private void drawTargetDistanceSensor(float range) {
            UnityEngine.Debug.DrawRay(transform.position + transform.up, transform.forward * range, Color.yellow);
            UnityEngine.Debug.DrawRay(transform.position + transform.up + (transform.right * 0.55f), transform.forward * range, Color.blue);
            UnityEngine.Debug.DrawRay(transform.position + transform.up - (transform.right * 0.55f), transform.forward * range, Color.blue);
            UnityEngine.Debug.DrawRay(transform.position + transform.up + (transform.right * 1.05f), transform.forward * range, Color.blue);
            UnityEngine.Debug.DrawRay(transform.position + transform.up - (transform.right * 1.05f), transform.forward * range, Color.blue);
            UnityEngine.Debug.DrawRay(transform.position + transform.up + (transform.right * 1.75f), transform.forward * range, Color.blue);
            UnityEngine.Debug.DrawRay(transform.position + transform.up - (transform.right * 1.75f), transform.forward * range, Color.blue);
        }

        private void drawPedestrianSensor(float range) {
            UnityEngine.Debug.DrawRay(transform.position + transform.up, transform.forward * range, Color.red);
            UnityEngine.Debug.DrawRay(transform.position + transform.up + transform.right, transform.forward * range, Color.red);
            UnityEngine.Debug.DrawRay(transform.position + transform.up - transform.right, transform.forward * range, Color.red);
            UnityEngine.Debug.DrawRay(transform.position + transform.up + (transform.right * 1.7f), transform.forward * range, Color.red);
            UnityEngine.Debug.DrawRay(transform.position + transform.up - (transform.right * 1.7f), transform.forward * range, Color.red);
        }

        private void drawEmergencySensor(float range) {
            UnityEngine.Debug.DrawRay(transform.position + transform.up, transform.forward * range, Color.red);
            UnityEngine.Debug.DrawRay(transform.position + transform.up + transform.right, transform.forward * range, Color.red);
            UnityEngine.Debug.DrawRay(transform.position + transform.up - transform.right, transform.forward * range, Color.red);
            UnityEngine.Debug.DrawRay(transform.position + transform.up + (transform.right * 1.7f), transform.forward * range, Color.red);
            UnityEngine.Debug.DrawRay(transform.position + transform.up - (transform.right * 1.7f), transform.forward * range, Color.red);
        }

        private void drawDistanceSensor() {
            UnityEngine.Debug.DrawRay(transform.position + transform.up, transform.forward * sensorRange, Color.yellow);
            UnityEngine.Debug.DrawRay(transform.position + transform.up + (transform.right * 0.5f), transform.forward * sensorRange, Color.yellow);
            UnityEngine.Debug.DrawRay(transform.position + transform.up - (transform.right * 0.5f), transform.forward * sensorRange, Color.yellow);
            UnityEngine.Debug.DrawRay(transform.position + transform.up + (transform.right * 1.0f), transform.forward * sensorRange, Color.yellow);
            UnityEngine.Debug.DrawRay(transform.position + transform.up - (transform.right * 1.0f), transform.forward * sensorRange, Color.yellow);
            UnityEngine.Debug.DrawRay(transform.position + transform.up + (transform.right * 1.7f), transform.forward * sensorRange, Color.yellow);
            UnityEngine.Debug.DrawRay(transform.position + transform.up - (transform.right * 1.7f), transform.forward * sensorRange, Color.yellow);
        }

        private void drawMergeSensor() {
            UnityEngine.Debug.DrawRay(transform.position + transform.up, transform.right * 5, Color.green);
            UnityEngine.Debug.DrawRay(transform.position + transform.up, transform.right * -5, Color.green);
        }

        private void writeUI(string s) {
            if (GUIText != null) {
                if (selfDriving) {
                    GUIText.text = "";
                    if (hasCollided)
                    {
                        GUIText.text = "Collision Detected\n";
                    } else 
                    if (withinHandbrakeDistance) {
                        GUIText.text = GUIText.text + "Handbrake Activated \n" + "Speed Limit: " + currSpeedLimit.ToString() + "\n" + "Current Velocity: " + Mathf.Sqrt(Mathf.Pow(m_RigidBody.velocity[1],2.0f)+Mathf.Pow(m_RigidBody.velocity[0],2.0f)+Mathf.Pow(m_RigidBody.velocity[2],2.0f)) + "\n" + s;
                    } else if (isFollowing) {
                        GUIText.text = GUIText.text + "Distance Control Activated" + "\n" + "Speed Limit: " + currSpeedLimit.ToString() + "\n" + "Current Velocity: " + Mathf.Sqrt(Mathf.Pow(m_RigidBody.velocity[1],2.0f)+Mathf.Pow(m_RigidBody.velocity[0],2.0f)+Mathf.Pow(m_RigidBody.velocity[2],2.0f)) + "\n" + s;
                    } else {
                        GUIText.text = GUIText.text + "Cruise Control Activated" + "\n" + "Speed Limit: " + currSpeedLimit.ToString() + "\n" + "Current Velocity: " + Mathf.Sqrt(Mathf.Pow(m_RigidBody.velocity[1],2.0f)+Mathf.Pow(m_RigidBody.velocity[0],2.0f)+Mathf.Pow(m_RigidBody.velocity[2],2.0f)) + "\n" + s;
                    }
                } else {
                    GUIText.text = "User Control Activated\n";
                }
            }
        }

    }
}