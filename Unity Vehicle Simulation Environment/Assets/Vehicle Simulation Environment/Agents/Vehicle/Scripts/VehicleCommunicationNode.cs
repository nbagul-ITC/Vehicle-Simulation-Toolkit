//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System;

namespace VehicleSimulation {
    public class VehicleCommunicationNode : MonoBehaviour {
        private GameObject[] allVehicles;
        public List<GameObject> nearbyVehicles = new List<GameObject>();
        public string nearbyVehicleNames = "";
        public float communicationDistance = 20.0f;
        private float communicationUpdateTime = 0.01f;
        private float currTime = 0.0f;
        private float currentTime = 0.0f;
        public string ipAddress;
        public string broadcast;
        public string received;
        public Dictionary<string, List<string>> receivedMessages = new Dictionary<string, List<string>>();

        public string personalMessage;
        public string defaultVehicleData;

        // Circle Params
        float theta_scale = 0.01f; // Set lower to add more points
        int size; // Total number of points in circle
        LineRenderer lineRenderer;

        void Awake() {
            float sizeValue = (2.0f * Mathf.PI) / theta_scale; 
            size = (int)sizeValue;
            size++;
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.SetWidth(0.2f, 0.2f); //thickness of line
            lineRenderer.SetVertexCount(size);     
        }

        void Start () {
            allVehicles = GameObject.FindGameObjectsWithTag("Car");
        }
        
        void LateUpdate () {
            currentTime = currentTime + Time.deltaTime;

            defaultVehicleData = getVehicleData();

            if (currTime > communicationUpdateTime) {
                broadcastMessage(broadcast,communicationDistance);
                currTime = 0.0f;
            } else {
                currTime = currTime + Time.deltaTime;
            }
            drawConcentricCircle(communicationDistance);
        }

        void drawConcentricCircle(float radius) {
            Vector3 pos;
            float theta = 0f;
            for(int i = 0; i < size; i++) {          
              theta += (2.0f * Mathf.PI * theta_scale);         
              float x = radius * Mathf.Cos(theta);
              float z = radius * Mathf.Sin(theta);          
              x += gameObject.transform.position.x;
              z += gameObject.transform.position.z;
              pos = new Vector3(x, 1.0f, z);
              lineRenderer.SetPosition(i, pos);
            }
        }

        string getVehicleData() {
            string pos = gameObject.transform.position.ToString();
            string rot = gameObject.transform.rotation.ToString();
            return ipAddress + ": {" + "Position: " + pos + ", " + "Rotation: " + rot + "}";
        }

        void broadcastMessage(string message, float radius) {
            nearbyVehicles.Clear();
            nearbyVehicleNames = "";
            foreach (GameObject vehicle in allVehicles) {
                float distance = Vector3.Distance(gameObject.transform.position, vehicle.transform.position);
                if ((distance < communicationDistance) && (vehicle.GetComponent<VehicleCommunicationNode>()) && (vehicle != gameObject)) {
                    nearbyVehicles.Add(vehicle);
                    nearbyVehicleNames = nearbyVehicleNames + vehicle.GetComponent<VehicleCommunicationNode>().ipAddress + ", ";
                    vehicle.GetComponent<VehicleCommunicationNode>().receivedMessages[ipAddress] = new List<string>{currentTime.ToString(), currentTime.ToString(), broadcast};
                    foreach (string messageKey in receivedMessages.Keys) {
                        if (vehicle.GetComponent<VehicleCommunicationNode>().receivedMessages.ContainsKey(messageKey)) {
                            List<string> messageValue = vehicle.GetComponent<VehicleCommunicationNode>().receivedMessages[messageKey];
                            if (float.Parse(messageValue[1]) < float.Parse(receivedMessages[messageKey][1])) {
                                vehicle.GetComponent<VehicleCommunicationNode>().receivedMessages[messageKey] = new List<string>{currentTime.ToString(), receivedMessages[messageKey][1], receivedMessages[messageKey][2]};
                            }
                        } else {
                            vehicle.GetComponent<VehicleCommunicationNode>().receivedMessages[messageKey] = new List<string>{currentTime.ToString(), receivedMessages[messageKey][1], receivedMessages[messageKey][2]};
                        }
                    }
                }
            }
            nearbyVehicleNames = nearbyVehicleNames.Substring(0, nearbyVehicleNames.Length-2);
        }
    }
}
