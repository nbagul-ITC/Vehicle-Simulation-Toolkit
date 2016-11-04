//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace VehicleSimulation {
    public class PedestrianPlacementAlgorithm : MonoBehaviour {
        public GameObject pedestrian;
        public int numPedestrians;
        public Vector3 startRange;
        public Vector3 endRange;

        void Start () {
            PlaceGroup(numPedestrians,startRange.x,endRange.x,startRange.y,endRange.y,startRange.z,endRange.z);
        }
        
        public void PlaceGroup(int num, float x1, float x2, float y1, float y2, float z1, float z2) {
            for (int i = 0; i < num; i++) {
                RandomlyPlace(x1, x2, y1, y2, z1, z2);
            }
        }

        public void PlacePedestrian(float x, float y, float z) {
            Vector3 pos = new Vector3(x,y,z);
            Instantiate(pedestrian, pos, Quaternion.identity);
        }

        public void RandomlyPlace(float x1, float x2, float y1, float y2, float z1, float z2) {
            PlacePedestrian(Random.Range(x1,x2), Random.Range(y1,y2), Random.Range(z1,z2));
        }
    }
}
