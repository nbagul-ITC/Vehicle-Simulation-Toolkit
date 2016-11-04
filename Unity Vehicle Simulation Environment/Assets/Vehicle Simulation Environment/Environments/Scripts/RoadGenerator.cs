//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Utility;

namespace VehicleSimulation {
    public class RoadGenerator : MonoBehaviour {
        public int numLeftLanes = 1;
        public int numRightLanes = 1;

        // Road Properties
        public float laneWidth = 3.7f;
        public float shoulderWidth = 1.2f;
        // if total lanes > 6, shoulderWidth = 3.0f
        // private float medianWidth = 9.1f;
        public float medianWidth = 0.0f;

        // Splines
        private GameObject leftRoadEdge;
        private GameObject rightRoadEdge;
        private GameObject leftShoulderEdge;
        private GameObject rightShoulderEdge;
        private GameObject leftMedianEdge;
        private GameObject rightMedianEdge;

        // Initialization
        private GameObject originalNodes;
        private GameObject debug;
        private List<GameObject> leftCircuits;
        private List<GameObject> rightCircuits;

        private Transform[] path;
        private bool loop = false;
        private int betweenNodeCount = 25;
        private IEnumerable<Vector3> nodes;
        private int count = 0;
        private List<Vector3> nodeList = new List<Vector3>();

        [InspectorButton("OnButtonClicked")]
        public bool generate;

        [InspectorButton("OnResetClicked")]
        public bool resetRoad;

        [InspectorButton("OnTestClicked")]
        public bool testRoad;

        private void OnTestClicked() {
            List<Vector3> testCenterSpline = new List<Vector3>();
            testCenterSpline.Add(new Vector3(0.0f,0.0f,0.0f));
            testCenterSpline.Add(new Vector3(20.0f,0.0f,10.0f));
            testCenterSpline.Add(new Vector3(50.0f,0.0f,0.0f)); 
            List<List<List<Vector3>>> testRoadList = generateRoad(testCenterSpline, 3.7f, 2.0f, 1.2f, 2, 0, false, 10);

            foreach (List<List<Vector3>> a in testRoadList) {
                foreach (List<Vector3> b in a) {
                    foreach (Vector3 c in b) {
                        GameObject d = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        d.transform.position = c;
                    }
                }
            }
        }

        private void OnResetClicked() {
            bool clicked = false;
            foreach (Transform child in gameObject.transform) {
                if (child.name.ToString() == "Debug") {
                    clicked = true;
                }
            }
            if (clicked) {
                List<GameObject> children = new List<GameObject>();
                foreach (Transform child in gameObject.transform) {
                    if (child.name.ToString() != "Debug") {
                        children.Add(child.gameObject);
                    } else {
                        debug = child.gameObject;
                        foreach (Transform subChild in debug.transform) {
                            if (subChild.name.ToString() == "Original Nodes") {
                                originalNodes = subChild.gameObject;
                            }
                        }
                    }
                }
                foreach (GameObject child in children) {
                    DestroyImmediate(child);
                }
                children = new List<GameObject>();
                foreach (Transform child in originalNodes.transform) {
                    children.Add(child.gameObject);
                }
                foreach (GameObject child in children) {
                    child.transform.parent = gameObject.transform;
                    if (child.gameObject.GetComponent<Renderer>() != null) {
                        child.gameObject.GetComponent<Renderer>().enabled = true;
                    }
                }
                DestroyImmediate(debug);
            }

        }

        public void OnButtonClicked() {
            bool clicked = false;
            foreach (Transform child in gameObject.transform) {
                if (child.name.ToString() == "Debug") {
                    clicked = true;
                }
            }

            if (clicked == false) {
                leftCircuits = new List<GameObject>();
                rightCircuits = new List<GameObject>();
                originalNodes = new GameObject("Original Nodes");
                foreach (Transform child in gameObject.transform) {
                    if (child.gameObject.GetComponent<Renderer>() != null) {
                        child.gameObject.GetComponent<Renderer>().enabled = false;
                    }
                }

                GameObject centerSpline = GenerateCenterSpline();
                debug = new GameObject("Debug");
                debug.transform.parent = gameObject.transform;
                centerSpline.transform.parent = debug.transform;
                originalNodes.transform.parent = debug.transform;
                
                if (numLeftLanes + numRightLanes > 6) {
                    shoulderWidth = 3.0f;
                }

                // Create Median
                leftMedianEdge = GenerateLeftSpline(centerSpline, medianWidth/2);
                leftMedianEdge.transform.parent = debug.transform;
                rightMedianEdge = GenerateRightSpline(centerSpline, medianWidth/2);
                rightMedianEdge.transform.parent = debug.transform;

                for (float j = 0.0f; j < numLeftLanes; j++) {
                    GameObject leftCenterline = GenerateLeftSpline(centerSpline, (j+1)*laneWidth - laneWidth/2 + medianWidth/2);
                    leftCenterline = ReverseSpline(leftCenterline);
                    addWaypointCircuit(leftCenterline);
                    leftCircuits.Add(leftCenterline);
                    // GameObject left = GenerateLeftSpline(centerSpline, (j+1)*laneWidth);
                    // addWaypointCircuit(left);
                }

                for (float j = 0.0f; j < numRightLanes; j++) {
                    GameObject rightCenterline = GenerateRightSpline(centerSpline, (j+1)*laneWidth - laneWidth/2 + medianWidth/2);
                    addWaypointCircuit(rightCenterline);
                    rightCircuits.Add(rightCenterline);
                    // GameObject right = GenerateRightSpline(centerSpline, (j+1)*laneWidth);
                    // addWaypointCircuit(right);
                }

                leftRoadEdge = GenerateLeftSpline(centerSpline, (numLeftLanes)*laneWidth + medianWidth/2);
                leftRoadEdge.transform.parent = debug.transform;
                rightRoadEdge = GenerateRightSpline(centerSpline, (numRightLanes)*laneWidth + medianWidth/2);
                rightRoadEdge.transform.parent = debug.transform;
                MeshSplines(leftRoadEdge, leftMedianEdge, Color.black);
                MeshSplines(rightRoadEdge, rightMedianEdge, Color.black);

                leftShoulderEdge = GenerateLeftSpline(centerSpline, (numLeftLanes)*laneWidth + shoulderWidth + medianWidth/2);
                leftShoulderEdge.transform.parent = debug.transform;
                MeshSplines(leftRoadEdge, leftShoulderEdge, Color.white);
                rightShoulderEdge = GenerateRightSpline(centerSpline, (numRightLanes)*laneWidth + shoulderWidth + medianWidth/2);
                rightShoulderEdge.transform.parent = debug.transform;
                MeshSplines(rightRoadEdge, rightShoulderEdge, Color.white);

                // MeshSplines(left, centerSpline);
                // AssignWaypointCircuit();
                // GenerateParallelSpine();
                ConnectCircuits();
            }
        }

        GameObject GenerateCenterSpline(){
            path = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++) {
                path[i] = transform.GetChild(i);
            }
            GameObject spline = new GameObject("Center Spline");
            spline.transform.position = new Vector3(0,0,0);
            spline.transform.parent = gameObject.transform;
            nodes = Interpolate.NewCatmullRom(path, betweenNodeCount, loop);
            count = 0;
            foreach (Vector3 node in nodes) {
                GameObject point = new GameObject("Node " + count.ToString());
                point.transform.position = node;
                point.transform.parent = spline.transform;
                count++;
            }
            foreach (Transform node in path) {
                node.parent = originalNodes.transform;
                // Destroy(node.gameObject);
            }
            return spline;
        }

        GameObject ReverseSpline(GameObject spline) {
            // Reorder spline children, save positions in a list and reverse
            GameObject newSpline = new GameObject(spline.transform.name);
            newSpline.transform.parent = gameObject.transform;
            List<Vector3> childLocations = getChildrenLocation(spline);
            childLocations.Reverse();
            count = 0;
            foreach (Vector3 pos in childLocations) {
                GameObject loc = new GameObject("Node " + count.ToString());
                loc.transform.position = pos;
                loc.transform.parent = newSpline.transform;
                count = count + 1;
            }
            DestroyImmediate(spline);
            return newSpline;
        }

        void addWaypointCircuit(GameObject spline) {
            // WaypointCircuit circuit = spline.AddComponent<WaypointCircuit>();
            // circuit.AssignChildObjects();
            // spline.AddComponent<ConnectedWaypoints>();
        }

        void setConnectedWaypoints(GameObject spline, WaypointCircuit leftCircuit, WaypointCircuit rightCircuit) {
            ConnectedWaypoints connection = spline.GetComponent<ConnectedWaypoints>();
            if (leftCircuit != null) {
                connection.leftCircuit = leftCircuit;
            }
            if (rightCircuit != null) {        
                connection.rightCircuit = rightCircuit;
            }
        }

        void ConnectCircuits() {
            for (int i= 1; i < leftCircuits.Count; i++) {
                setConnectedWaypoints(leftCircuits[i-1], null, leftCircuits[i].GetComponent<WaypointCircuit>());
                setConnectedWaypoints(leftCircuits[i], leftCircuits[i-1].GetComponent<WaypointCircuit>(), null);
            }
            for (int i= 1; i < rightCircuits.Count; i++) {
                setConnectedWaypoints(rightCircuits[i-1], null, rightCircuits[i].GetComponent<WaypointCircuit>());
                setConnectedWaypoints(rightCircuits[i], rightCircuits[i-1].GetComponent<WaypointCircuit>(), null);
            }
        }

        List<Vector3> getChildrenLocation(GameObject parentObject) {
            List<Vector3> locations = new List<Vector3>();
            foreach (Transform child in parentObject.transform) {
                locations.Add(child.position);
            }
            return locations;
        }

        GameObject GenerateLeftSpline(GameObject centerSpline, float dist) {
            List<Vector3> centerSplineNodes = getChildrenLocation(centerSpline);
            GameObject spline = new GameObject("Left Spline " + dist.ToString());
            spline.transform.position = new Vector3(0,0,0);
            spline.transform.parent = gameObject.transform;
            // gameObject.AddComponent<MeshFilter>();
            // Mesh mesh = GetComponent<MeshFilter>().mesh;
            //List<Vector3> outsideVertices = new List<Vector3>();
            List<Vector3> leftVertices = new List<Vector3>();
            count = 0;
            for (int i= 0; i < centerSplineNodes.Count-1; i++) {
                Vector3 midpoint = Vector3.Lerp(centerSplineNodes[i+1],centerSplineNodes[i],0.5f);
                Vector3 derivative = centerSplineNodes[i+1] - centerSplineNodes[i];
                Vector3 norm = Vector3.Normalize(derivative)*dist;
                Quaternion rot1 = Quaternion.AngleAxis(-90,Vector3.up);
                Vector3 ortho1 = rot1*norm;
                Vector3 pos1 = midpoint + ortho1;
                pos1.y = (centerSplineNodes[i].y+centerSplineNodes[i+1].y)/2;
                GameObject point = new GameObject("Node " + count.ToString());
                point.transform.position = pos1;
                point.transform.parent = spline.transform;
                count++;
                leftVertices.Add(pos1);
            }
            return spline;
        }

        GameObject GenerateRightSpline(GameObject centerSpline, float dist) {
            List<Vector3> centerSplineNodes = getChildrenLocation(centerSpline);
            GameObject spline = new GameObject("Right Spline " + dist.ToString());
            spline.transform.position = new Vector3(0,0,0);
            spline.transform.parent = gameObject.transform;
            // gameObject.AddComponent<MeshFilter>();
            // Mesh mesh = GetComponent<MeshFilter>().mesh;
            //List<Vector3> outsideVertices = new List<Vector3>();
            List<Vector3> leftVertices = new List<Vector3>();
            count = 0;
            for (int i= 0; i < centerSplineNodes.Count-1; i++) {
                Vector3 midpoint = Vector3.Lerp(centerSplineNodes[i+1],centerSplineNodes[i],0.5f);
                Vector3 derivative = centerSplineNodes[i+1] - centerSplineNodes[i];
                Vector3 norm = Vector3.Normalize(derivative)*dist;
                Quaternion rot1 = Quaternion.AngleAxis(90,Vector3.up);
                Vector3 ortho1 = rot1*norm;
                Vector3 pos1 = midpoint + ortho1;
                pos1.y = (centerSplineNodes[i].y+centerSplineNodes[i+1].y)/2;
                GameObject point = new GameObject("Node " + count.ToString());
                point.transform.position = pos1;
                point.transform.parent = spline.transform;
                count++;
                leftVertices.Add(pos1);
            }
            return spline;
        }

        void MeshSplines(GameObject spline1, GameObject spline2, Color material) {
            // Add a new gameobject for visualization
            GameObject splineMesh = new GameObject("SplineMesh");
            splineMesh.transform.position = gameObject.transform.position;
            splineMesh.transform.parent = debug.transform;
            splineMesh.AddComponent<MeshRenderer>();
            splineMesh.GetComponent<MeshRenderer>().material.color = material;
            splineMesh.AddComponent<MeshFilter>();
            Mesh mesh = splineMesh.GetComponent<MeshFilter>().mesh;
            List<Vector3> outsideVertices = new List<Vector3>();
            List<Vector3> leftVertices = getChildrenLocation(spline1);
            List<Vector3> rightVertices = getChildrenLocation(spline2);
            for (int i= 0; i < leftVertices.Count-1; i++) {
                outsideVertices.Add(leftVertices[i] - gameObject.transform.position);
                outsideVertices.Add(rightVertices[i] - gameObject.transform.position);
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
        }

        public List<List<List<Vector3>>> generateRoad(List<Vector3> initialSpline, float laneWidth, float medianWidth, float shoulderWidth, int numLeftLanes, int numRightLanes, bool loop, int betweenNodeCount) {
            // Returns list of 4 lists of nodes that represents a lane
            // [1] = Lists of left center lanes (Starting from the lanes closest to the initial Spline)
            // [2] = Lists of left boundary lanes
            // [3] = Lists of right center lanes
            // [4] = Lists of right boundary lanes 
            List<List<Vector3>> leftCenterLanes = new List<List<Vector3>>();
            List<List<Vector3>> leftBoundaryLanes = new List<List<Vector3>>();
            List<List<Vector3>> rightCenterLanes = new List<List<Vector3>>();
            List<List<Vector3>> rightBoundaryLanes = new List<List<Vector3>>();

            List<Vector3> centerLane = new List<Vector3>();

            IEnumerable<Vector3> nodes = Interpolate.NewCatmullRom(initialSpline.ToArray(), betweenNodeCount, loop);
            foreach (Vector3 node in nodes) {
                centerLane.Add(node);
            }

            if (numLeftLanes != 0) {
                leftBoundaryLanes.Add(GenerateLeftSpline(centerLane, medianWidth/2.0f));
            }
            for (float j = 0.0f; j < numLeftLanes; j = j + 1.0f) {
                    leftBoundaryLanes.Add(GenerateLeftSpline(centerLane, (j+1.0f)*laneWidth - laneWidth/2.0f + medianWidth/2.0f));
                    leftCenterLanes.Add(GenerateLeftSpline(centerLane, (j+1.0f)*laneWidth + medianWidth/2.0f));
            }

            if (numRightLanes != 0) {
                rightBoundaryLanes.Add(GenerateRightSpline(centerLane, medianWidth/2.0f));
            }
            for (float j = 0.0f; j < numRightLanes; j = j + 1.0f) {
                    rightBoundaryLanes.Add(GenerateRightSpline(centerLane, (j+1.0f)*laneWidth - laneWidth/2.0f + medianWidth/2.0f));
                    rightCenterLanes.Add(GenerateRightSpline(centerLane, (j+1.0f)*laneWidth + medianWidth/2.0f));
            }

            List<List<List<Vector3>>> completeList = new List<List<List<Vector3>>>();
            completeList.Add(leftCenterLanes);
            completeList.Add(leftBoundaryLanes);
            completeList.Add(rightCenterLanes);
            completeList.Add(rightBoundaryLanes);
            return completeList;
        }

        List<Vector3> GenerateLeftSpline(List<Vector3> centerSpline, float dist) {
            List<Vector3> leftVertices = new List<Vector3>();
            for (int i= 0; i < centerSpline.Count-1; i++) {
                Vector3 midpoint = Vector3.Lerp(centerSpline[i+1], centerSpline[i],0.5f);
                Vector3 derivative = centerSpline[i+1] - centerSpline[i];
                Vector3 norm = Vector3.Normalize(derivative)*dist;
                Quaternion rot1 = Quaternion.AngleAxis(-90, Vector3.up);
                Vector3 ortho1 = rot1*norm;
                Vector3 pos1 = midpoint + ortho1;
                pos1.y = (centerSpline[i].y+centerSpline[i+1].y)/2;
                leftVertices.Add(pos1);
            }
            leftVertices.Reverse();
            return leftVertices;
        }

        List<Vector3> GenerateRightSpline(List<Vector3> centerSpline, float dist) {
            List<Vector3> rightVertices = new List<Vector3>();
            for (int i= 0; i < centerSpline.Count-1; i++) {
                Vector3 midpoint = Vector3.Lerp(centerSpline[i+1], centerSpline[i],0.5f);
                Vector3 derivative = centerSpline[i+1] - centerSpline[i];
                Vector3 norm = Vector3.Normalize(derivative)*dist;
                Quaternion rot1 = Quaternion.AngleAxis(90, Vector3.up);
                Vector3 ortho1 = rot1*norm;
                Vector3 pos1 = midpoint + ortho1;
                pos1.y = (centerSpline[i].y+centerSpline[i+1].y)/2;
                rightVertices.Add(pos1);
            }
            return rightVertices;
        }

        void GenerateParallelSpine(){
            foreach (Vector3 node in nodes) {
                nodeList.Add(node);
                // UnityEngine.Debug.Log(node.ToString(), gameObject);
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
                // UnityEngine.Debug.Log("Derivative: " + derivative.ToString(),gameObject);
                Vector3 norm = Vector3.Normalize(derivative)*laneWidth;
                // UnityEngine.Debug.Log("Scaled Norm: " + norm.ToString(),gameObject);
                Quaternion rot1 = Quaternion.AngleAxis(90,Vector3.up);
                Quaternion rot2 = Quaternion.AngleAxis(-90,Vector3.up);
                Vector3 ortho1 = rot1*norm;
                Vector3 ortho2 = rot2*norm;
                // GameObject image = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // UnityEngine.Debug.Log("Midpoint: " + midpoint.ToString());
                Vector3 pos1 = midpoint + ortho1;
                pos1.y = (nodeList[i].y+nodeList[i+1].y)/2;
                // image.transform.position =  pos1;
                // image.GetComponent<Renderer>().material.color = Color.blue;
                // Destroy(image.GetComponent<BoxCollider>());

                // GameObject image2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Vector3 pos2 = midpoint + ortho2;
                pos2.y = (nodeList[i].y+nodeList[i+1].y)/2;
                // image2.transform.position =  pos2;             
                // image2.GetComponent<Renderer>().material.color = Color.red;
                // Destroy(image2.GetComponent<BoxCollider>());

                outsideVertices.Add(pos1 - gameObject.transform.position);
                outsideVertices.Add(pos2 - gameObject.transform.position);
                leftVertices.Add(pos1);
                rightVertices.Add(pos2);
            }
            // UnityEngine.Debug.Log((2*(count-1)).ToString(),gameObject);
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

            // GenerateRoadSides(leftVertices);
            // GenerateRoadSides2(rightVertices);

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
