//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VehicleSimulation {
    public class SplineMeshGenerator : MonoBehaviour {
        private Transform[] path;
        bool loop = false;
        int betweenNodeCount = 20;
        IEnumerable<Vector3> nodes;
        int count = 0;
        List<Vector3> nodeList;
        public float stdLaneWidth = 3.7f;

        void Start() {
            nodeList = new List<Vector3>();
            GenerateSpline();
            AssignWaypointCircuit();
            GenerateParallelSpine();
        }

        void GenerateSpline(){
            path = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++) {
                path[i] = transform.GetChild(i);
            }
            nodes = Interpolate.NewCatmullRom(path, betweenNodeCount, loop);
            count = 0;
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
            // CustomWaypointCircuit circuit = gameObject.GetComponent<CustomWaypointCircuit>();
            // circuit.AssignChildObjects();
        }

        void GenerateParallelSpine(){
            foreach (Vector3 node in nodes) {
                nodeList.Add(node);
            }
            gameObject.AddComponent<MeshFilter>();
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            List<Vector3> outsideVertices = new List<Vector3>();
            List<Vector3> leftVertices = new List<Vector3>();
            List<Vector3> rightVertices = new List<Vector3>();
            for (int i= 0; i < count-1; i++) {
                // Compare path[i] and path[i+1]
                Vector3 midpoint = Vector3.Lerp(nodeList[i+1],nodeList[i],0.5f);
                Vector3 derivative = nodeList[i+1] - nodeList[i];
                Vector3 norm = Vector3.Normalize(derivative)*stdLaneWidth;
                Quaternion rot1 = Quaternion.AngleAxis(90,Vector3.up);
                Quaternion rot2 = Quaternion.AngleAxis(-90,Vector3.up);
                Vector3 ortho1 = rot1*norm;
                Vector3 ortho2 = rot2*norm;
                Vector3 pos1 = midpoint + ortho1;
                pos1.y = (nodeList[i].y+nodeList[i+1].y)/2;
                Vector3 pos2 = midpoint + ortho2;
                pos2.y = (nodeList[i].y+nodeList[i+1].y)/2;
                outsideVertices.Add(pos1 - gameObject.transform.position);
                outsideVertices.Add(pos2 - gameObject.transform.position);
                leftVertices.Add(pos1);
                rightVertices.Add(pos2);
            }
            mesh.vertices = outsideVertices.ToArray();
            List<int> meshTriangles = new List<int>();
            for (int j = 0; j < 2*(count-1)-3; j++) {
                meshTriangles.Add(j);
                meshTriangles.Add(j+1);
                meshTriangles.Add(j+2);

                meshTriangles.Add(j+3);
                meshTriangles.Add(j+2);
                meshTriangles.Add(j+1);
            }
            mesh.triangles = meshTriangles.ToArray();

            GenerateRoadMesh("Left Side Lane", leftVertices, 0.25f, 0.0f);
            GenerateRoadMesh("Right Side Lane", rightVertices, 0.0f, 0.25f);
        }

        void GenerateRoadMesh(string name, List<Vector3> centerNodeList, float leftWidth, float rightWidth) {
        	GameObject road = new GameObject(name);
            road.transform.parent = gameObject.transform;
            road.AddComponent<MeshFilter>();
            road.AddComponent<MeshRenderer>();
            Mesh mesh = road.GetComponent<MeshFilter>().mesh;
            List<Vector3> outsideVertices = new List<Vector3>();
            for (int i= 0; i < centerNodeList.Count -1 ; i++) {
                Vector3 midpoint = Vector3.Lerp(centerNodeList[i+1],centerNodeList[i],0.5f);
                Vector3 derivative = centerNodeList[i+1] - centerNodeList[i];
                Vector3 norm = Vector3.Normalize(derivative);
                Quaternion rot1 = Quaternion.AngleAxis(90,Vector3.up);
                Quaternion rot2 = Quaternion.AngleAxis(-90,Vector3.up);
                Vector3 ortho1 = rot1*(norm*leftWidth);
                Vector3 ortho2 = rot2*(norm*rightWidth);
                Vector3 pos1 = midpoint + ortho1;
                pos1.y = (centerNodeList[i].y+centerNodeList[i+1].y)/2;
                Vector3 pos2 = midpoint + ortho2;
                pos2.y = (centerNodeList[i].y+centerNodeList[i+1].y)/2;
                outsideVertices.Add(pos1);
                outsideVertices.Add(pos2);
            }
            mesh.vertices = outsideVertices.ToArray();
            List<int> meshTriangles = new List<int>();
            for (int j = 0; j < 2*(centerNodeList.Count-1)-3; j++) {
                meshTriangles.Add(j);
                meshTriangles.Add(j+1);
                meshTriangles.Add(j+2);
                meshTriangles.Add(j+3);
                meshTriangles.Add(j+2);
                meshTriangles.Add(j+1);
            }
            mesh.triangles = meshTriangles.ToArray();
            road.GetComponent<MeshRenderer>().material.color = Color.white;
        }

    }
}
