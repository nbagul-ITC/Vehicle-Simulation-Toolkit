//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VehicleSimulation {
    public class SplineGenerator : MonoBehaviour {
        private Transform[] path;
        bool loop = false;
        int betweenNodeCount = 10;
        IEnumerable<Vector3> nodes;

        void Awake() {
            GenerateSpline();
            AssignWaypointCircuit();
        }

        void GenerateSpline() {
            path = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++) {
                path[i] = transform.GetChild(i);
            }
            nodes = Interpolate.NewCatmullRom(path, betweenNodeCount, loop);
            int count = 0;
            foreach (Vector3 node in nodes) {
                GameObject point = new GameObject("Node" + count.ToString());
                point.transform.position = node;
                point.transform.parent = gameObject.transform;
                count++;
            }
            foreach (Transform node in path) {
                node.parent = null;
                Destroy(node.gameObject);
            }
        }

        void AssignWaypointCircuit() {
            // WaypointCircuit circuit = gameObject.GetComponent<WaypointCircuit>();
            // circuit.AssignChildObjects();
        }
    }
}
