//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using UnityStandardAssets.Utility;

namespace VehicleSimulation {
    public class PlacementAlgorithm : MonoBehaviour {
        public GameObject carTemplate;
        public GameObject[] vehicleModels;

        void Awake() {
            // Build Environment Here
        }

        void Start () {
            PlaceVehicle(1.0f,1.5f,1.0f);
            RandomlyPlaceVehicle(0.0f,50.0f,1.5f,1.5f,0.0f,50.0f);
        }

        public void PlaceVehicle(float x, float y, float z) {
            Vector3 pos = new Vector3(x,y,z);
            GameObject newCar = ((GameObject) Instantiate(carTemplate, pos, Quaternion.identity));
            SetCarModel(newCar);
            RandomizeRigidbodyParams(newCar);
            bool assigned = AssignClosestWaypointCircuit(newCar);
            if (!assigned) {
                newCar.GetComponent<CarAI>().toggleForceStop();
            }
            HideSkyCar(newCar);
        }

        public void PlaceVehicle(float x, float y, float z, WaypointCircuit circuit) {
            Vector3 pos = new Vector3(x,y,z);
            GameObject newCar = ((GameObject) Instantiate(carTemplate, pos, Quaternion.identity));
            AssignWaypointCircuit(newCar, circuit);
            SetCarModel(newCar);
            RandomizeRigidbodyParams(newCar);
            HideSkyCar(newCar);
        }

        public void PlaceGroup(int numVehicles, float x1, float x2, float y1, float y2, float z1, float z2) {
            for (int i = 0; i < numVehicles; i++) {
                RandomlyPlaceVehicle(x1, x2, y1, y2, z1, z2);
            }
        }

        private void SetCarModel(GameObject newCar) {
            GameObject carModel = vehicleModels[Random.Range(0,(vehicleModels.Length-1))];
            GameObject newCarModel = ((GameObject) Instantiate(carModel, newCar.transform.position, Quaternion.identity));
            newCarModel.transform.parent = newCar.transform;
        }

        public void RandomlyPlaceVehicle(float x1, float x2, float y1, float y2, float z1, float z2) {
            PlaceVehicle(Random.Range(x1,x2), Random.Range(y1,y2), Random.Range(z1,z2));
        }

        public void RandomlyPlaceVehicle(float x1, float x2, float y1, float y2, float z1, float z2, WaypointCircuit circuit) {
            PlaceVehicle(Random.Range(x1,x2), Random.Range(y1,y2), Random.Range(z1,z2), circuit);
        }

        void RandomizeRigidbodyParams(GameObject newCar) {
            newCar.GetComponent<Rigidbody>().mass = Random.Range(500.0f, 1500.0f);
            newCar.GetComponent<Rigidbody>().drag = Random.Range(0.01f, 0.001f);
            newCar.GetComponent<Rigidbody>().angularDrag = Random.Range(0.01f, 0.001f);
        }

        public void AssignWaypointCircuit(GameObject newCar, WaypointCircuit circuit) {
            WaypointProgressTracker waypointTracker = newCar.GetComponent<WaypointProgressTracker>();
            waypointTracker.circuit = circuit;
        }

        public bool AssignClosestWaypointCircuit(GameObject newCar) {
            // Should not run in update function - Very Slow!
            Vector3 currentPosition = newCar.transform.position;
            WaypointCircuit[] circuits = Object.FindObjectsOfType<WaypointCircuit>();
            WaypointCircuit closestCircuit = Object.FindObjectOfType<WaypointCircuit>();
            float minimumDistance = Mathf.Infinity;
            foreach (WaypointCircuit circuit in circuits) {
                foreach (Transform child in circuit.transform) {
                    if (Vector3.Distance(currentPosition,child.position) < minimumDistance) {
                        closestCircuit = circuit;
                        minimumDistance = Vector3.Distance(currentPosition,child.position);
                    }
                }
            }
            AssignWaypointCircuit(newCar, closestCircuit);
            if (closestCircuit != null) {
                return true;
            } else {
                return false;
            }
        }

        public void GenerateWaypointCircuit(GameObject spline) {
            // WaypointCircuit circuit = spline.AddComponent<WaypointCircuit>(); 
            // circuit.AssignChildObjects();
        }

        public void HideSkyCar(GameObject newCar) {
            foreach (Transform child in newCar.transform) {
                UnityEngine.Debug.Log(child.gameObject.name);
                if (child.gameObject.name == "SkyCar") {
                    UnityEngine.Debug.Log(child.gameObject.name);
                    foreach (Transform subchild in child) {   
                        MeshRenderer render = subchild.gameObject.GetComponentInChildren<MeshRenderer>();
                        render.enabled = false;
                    }
                }
            }
        }
    }
}
