//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace VehicleSimulation {
    public class IntersectionCoordination : MonoBehaviour {
        public SpeedLimit[] direction1;
        public SpeedLimit[] direction2;
        public int direction1SpeedLimit;
        public int direction2SpeedLimit;
        public float duration;
        private float timeLeft;

        void Start () {
            foreach (SpeedLimit s in direction1){
                s.speedLimit = direction1SpeedLimit;
            }
            foreach (SpeedLimit s in direction2){
                s.speedLimit = direction2SpeedLimit;;
            }
            timeLeft = duration;
        }
        
        void Update () {
            timeLeft = timeLeft - Time.deltaTime;
            if (timeLeft < 0) {
                swapSpeedLimits();
                timeLeft = duration;
            }
        }

        void swapSpeedLimits(){
            int tempSpeed = direction1SpeedLimit;
            direction1SpeedLimit = direction2SpeedLimit;
            direction2SpeedLimit = tempSpeed;
            foreach (SpeedLimit s in direction1){
                s.speedLimit = direction2SpeedLimit;
            }
            foreach (SpeedLimit s in direction2){
                s.speedLimit = direction1SpeedLimit;
            }
        }
    }
}
