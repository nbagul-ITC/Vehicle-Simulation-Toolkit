//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.ThirdPerson;

namespace VehicleSimulation {
    public class PedestrianTargetGenerator : MonoBehaviour {
        private float targetRadius;
        private float targetTimer;
        private float timer;
        private GameObject pedestrianTarget;
        public Vector2 radiusInterval;
        public Vector2 timerInterval;

        void Start () {
            pedestrianTarget = new GameObject("Pedestrian AI Target");      
            AICharacterControl AIControl = gameObject.GetComponent<AICharacterControl>();
            AIControl.target = pedestrianTarget.transform;
            timer = targetTimer;
            targetRadius = Random.Range(radiusInterval.x,radiusInterval.y);
            targetTimer = Random.Range(timerInterval.x,timerInterval.y);
        }
        
        void Update () {
            timer += Time.deltaTime;
            if (timer >= targetTimer) {
                pedestrianTarget.transform.position = RandomNavSphere(transform.position, targetRadius, -1);
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
