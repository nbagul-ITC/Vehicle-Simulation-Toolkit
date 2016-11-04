//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace VehicleSimulation {
    public class SimulationLogger : MonoBehaviour {
        public string path = "Assets/Vehicle Simulation Environment/Data Visualization/Data/";
        public float elapsedTime = 0.0f;

        public List<GameObject> agents;
        public int logLevel = 0; // Between 0-1
        // 0 - Logs all position and rotation data for all game objects
        // 1 - Logs events where two objects are close together
        public float proximityDetection = 10.0f;

        public List<List<PositionLog>> positionLogs;
        public Dictionary<string, PositionLog> positionLogDict;

        void Start() {
            positionLogs = new List<List<PositionLog>>();
            switch (logLevel) {
                case 0:
                    for(int i = 0; i < agents.Count; i++) {
                        List<PositionLog> tempLog = new List<PositionLog>();
                        tempLog.Add(new PositionLog());
                        positionLogs.Insert(i, tempLog);
                    }
                    break;
                case 1:
                    UnityEngine.Debug.Log("Case 1");
                    for(int i = 0; i < agents.Count; i++) {

                    }
                    break;
            }
        }

        void Update() {
            switch (logLevel) {
                case 0:
                    for(int i = 0; i < agents.Count; i++) {
                        positionLogs[i][0].AddGameObjectPosition(agents[i], elapsedTime);
                    }
                    break;
                case 1:
                    UnityEngine.Debug.Log("Case 1");
                    break;
            }
            elapsedTime = elapsedTime + Time.deltaTime;
        }

        void OnGUI() {
            if (GUILayout.Button ("Save Logs")) {
                Debug.Log("Saving Logs");
                SaveLogs();
            }
        }

        void SaveLogs() {
            switch (logLevel) {
                case 0:            
                    for(int i = 0; i < agents.Count; i++) {
                        string filename = SceneManager.GetActiveScene().name + "-" + agents[i].transform.name.ToString() + "-";
                        int counter = 0;
                        foreach (PositionLog positionLog in positionLogs[i]){
                            positionLog.ToJSON(path + filename + counter.ToString() + ".json");
                        }
                    }
                    break;
                case 1:
                    UnityEngine.Debug.Log("Case 1");
                    break;
            }
        }
    }
}