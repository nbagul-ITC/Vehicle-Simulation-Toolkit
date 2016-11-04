//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace VehicleSimulation {
    public class SpeedLimit : MonoBehaviour {
        public int speedLimit = 0;
        public string roadType = "Local"; // Either Highway or Local
    }
}
