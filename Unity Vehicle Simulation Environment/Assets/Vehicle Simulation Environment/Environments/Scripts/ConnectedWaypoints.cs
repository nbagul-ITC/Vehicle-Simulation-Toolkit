//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using UnityStandardAssets.Utility;

namespace VehicleSimulation {
    public class ConnectedWaypoints : MonoBehaviour {
        public WaypointCircuit[] nextCircuits;
        public WaypointCircuit leftCircuit;
        public WaypointCircuit rightCircuit;
        // private float[] weights;
        
        public WaypointCircuit GetNextCircuit() {
            if (nextCircuits == null) {
                return null;
            } else if (nextCircuits.Length > 0) {
                return nextCircuits[Random.Range(0,nextCircuits.Length)];
            } else {
                return null;
            }
        }

        public WaypointCircuit GetAdjacentCircuit() {
            int r = Random.Range(0,2);
            if (r==0) {
                return leftCircuit;
            } else {
                return rightCircuit;
            }
        }

        public WaypointCircuit GetLeftCircuit() {
            return leftCircuit;
        }

        public WaypointCircuit GetRightCircuit() {
            return rightCircuit;
        }
    }
}
