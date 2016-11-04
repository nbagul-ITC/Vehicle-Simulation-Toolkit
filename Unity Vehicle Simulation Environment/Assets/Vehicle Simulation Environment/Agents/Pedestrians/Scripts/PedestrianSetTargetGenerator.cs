//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.ThirdPerson;

namespace VehicleSimulation {
    public class PedestrianSetTargetGenerator : MonoBehaviour {
        private float targetTimer;
        private float timer;
        private GameObject pedestrianTarget;
        public GameObject[] pedestrianTargets;
        public Vector2 updateInterval;

        void Start () {
            pedestrianTarget = new GameObject("Pedestrian AI Target");      
            AICharacterControl AIControl = gameObject.GetComponent<AICharacterControl>();
            AIControl.target = pedestrianTarget.transform;
            timer = targetTimer;
            targetTimer = Random.Range(updateInterval.x,updateInterval.y);
        }
        
        void Update () {
            timer += Time.deltaTime;
            if (timer >= targetTimer) {
                pedestrianTarget.transform.position = pedestrianTargets[Random.Range(0, pedestrianTargets.Length)].transform.position;
                timer = 0;
            }
        }

        public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask) {
            Vector3 randDirection = Random.insideUnitSphere * dist;
            randDirection += origin;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
            return navHit.position;
        }
    }
}