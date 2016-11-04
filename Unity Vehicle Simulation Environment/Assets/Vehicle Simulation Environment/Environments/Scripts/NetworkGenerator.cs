//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Utility;

namespace VehicleSimulation {
    public class NetworkGenerator : MonoBehaviour {
        public AdjacencyList<RoadSpline> network;
        private int counter = 0;

        void Start () {
            network = new AdjacencyList<RoadSpline>();

            // Block 1
            Vector3[] nodes1 = new Vector3[2];
            nodes1[0] = new Vector3(0.0f,0.0f,0.0f);            
            nodes1[1] = new Vector3(100f,0.0f,0.0f);
            GameObject spline1 = PlaceSpline("Spline1", nodes1);
            RoadSpline roadSpline1 = new RoadSpline(spline1, null, null);
            network.AddVertex(roadSpline1);

            Vector3[] nodes2 = new Vector3[2];
            nodes2[0] = new Vector3(100f,0.0f,0.0f);            
            nodes2[1] = new Vector3(100f,0.0f,100f);
            GameObject spline2 = PlaceSpline("Spline2", nodes2);
            RoadSpline roadSpline2 = new RoadSpline(spline2, null, null);
            network.AddVertex(roadSpline2);
            network.AddEdge(roadSpline1, roadSpline2);

            Vector3[] nodes3 = new Vector3[2];
            nodes3[0] = new Vector3(100f,0.0f,100f);            
            nodes3[1] = new Vector3(0f,0.0f,100f);
            GameObject spline3 = PlaceSpline("Spline3", nodes3);
            RoadSpline roadSpline3 = new RoadSpline(spline3, null, null);
            network.AddVertex(roadSpline3);
            network.AddEdge(roadSpline2, roadSpline3);

            Vector3[] nodes4 = new Vector3[2];
            nodes4[0] = new Vector3(0f,0.0f,100f);            
            nodes4[1] = new Vector3(0f,0.0f,0f);
            GameObject spline4 = PlaceSpline("Spline4", nodes4);
            RoadSpline roadSpline4 = new RoadSpline(spline4, null, null);
            network.AddVertex(roadSpline4);
            network.AddEdge(roadSpline3, roadSpline4);
            network.AddEdge(roadSpline4, roadSpline1);

            // Block 2
            Vector3[] nodes5 = new Vector3[2];
            nodes5[0] = new Vector3(0f,0.0f,-100f);            
            nodes5[1] = new Vector3(0f,0.0f,0f);
            GameObject spline5 = PlaceSpline("Spline5", nodes5);
            RoadSpline roadSpline5 = new RoadSpline(spline5, null, null);
            network.AddVertex(roadSpline5);
            network.AddEdge(roadSpline5, roadSpline1);

            Vector3[] nodes6 = new Vector3[2];
            nodes6[0] = new Vector3(100f,0.0f,0f);            
            nodes6[1] = new Vector3(100f,0.0f,-100f);
            GameObject spline6 = PlaceSpline("Spline6", nodes6);
            RoadSpline roadSpline6 = new RoadSpline(spline6, null, null);
            network.AddVertex(roadSpline6);
            network.AddEdge(roadSpline1, roadSpline6);

            Vector3[] nodes7 = new Vector3[2];
            nodes7[0] = new Vector3(100f,0.0f,-100f);            
            nodes7[1] = new Vector3(0f,0.0f,-100f);
            GameObject spline7 = PlaceSpline("Spline7", nodes7);
            RoadSpline roadSpline7 = new RoadSpline(spline7, null, null);
            network.AddVertex(roadSpline7);
            network.AddEdge(roadSpline6, roadSpline7);
            network.AddEdge(roadSpline7, roadSpline5);

            ConnectWaypoints();
        }

        void ConnectWaypoints() {
            foreach (KeyValuePair<RoadSpline, List<RoadSpline>> connectionPair in network.vertexDict) {
                ConnectedWaypoints paths = connectionPair.Key.circuit.GetComponent<ConnectedWaypoints>();
                paths.nextCircuits = new WaypointCircuit[connectionPair.Value.Count];
                for (int i = 0; i < connectionPair.Value.Count; i++) {
                    paths.nextCircuits[i] = connectionPair.Value[i].circuit.GetComponent<WaypointCircuit>();
                }
                if (connectionPair.Key.rightCircuit != null) {
                    paths.rightCircuit = connectionPair.Key.rightCircuit.GetComponent<WaypointCircuit>();
                }
                if (connectionPair.Key.leftCircuit != null) {
                    paths.leftCircuit = connectionPair.Key.leftCircuit.GetComponent<WaypointCircuit>();
                }
            }
        }

        void PlaceNode(GameObject parent, Vector3 location) {
            GameObject node = new GameObject("Node" + counter.ToString());
            node.transform.position = location;
            node.transform.parent = parent.transform;
            counter = counter + 1;
        }

        GameObject PlaceSpline(string name, Vector3[] nodes) {
            GameObject newSpline = new GameObject(name);
            foreach (Vector3 node in nodes) {
                PlaceNode(newSpline, node);
            }
            newSpline.AddComponent<WaypointCircuit>();
            newSpline.AddComponent<SplineGenerator>();
            newSpline.AddComponent<ConnectedWaypoints>();
            return newSpline;
        }
    }

    public class RoadSpline { 
        public GameObject circuit;
        public GameObject leftCircuit;
        public GameObject rightCircuit; 

        public RoadSpline(GameObject c, GameObject left, GameObject right) {
            circuit = c;
        }
    }

    public class AdjacencyList<K> {
        private List<List<K>> vertexList = new List<List<K>>();
        public Dictionary<K, List<K>> vertexDict = new Dictionary<K, List<K>>();
     
        public AdjacencyList(){}
       
        public List<K> AddVertex(K key) {
            List<K> vertex = new List<K>();
            vertexList.Add(vertex);
            vertexDict.Add(key, vertex);
            return vertex;
        }
       
        public void AddEdge(K startKey, K endKey) {      
            List<K> startVertex = vertexDict.ContainsKey(startKey) ? vertexDict[startKey] : null;
            List<K> endVertex = vertexDict.ContainsKey(endKey) ? vertexDict[endKey] : null;
            if (startVertex == null)
                throw new ArgumentException("Cannot create edge from a non-existent start vertex.");
            if (endVertex == null)
                endVertex = AddVertex(endKey);
            startVertex.Add(endKey);
        }
       
        public void RemoveVertex(K key) {
            List<K> vertex = vertexDict[key];
            int vertexNumAdjacent = vertex.Count;
            for (int i = 0; i < vertexNumAdjacent; i++) {  
                K neighbourVertexKey = vertex[i];
                RemoveEdge(key, neighbourVertexKey);
            }
            vertexList.Remove(vertex);
            vertexDict.Remove(key);
        }
       
        public void RemoveEdge(K startKey, K endKey) {
            ((List<K>)vertexDict[startKey]).Remove(endKey);
            ((List<K>)vertexDict[endKey]).Remove(startKey);
        }
       
        public bool Contains(K key) {
            return vertexDict.ContainsKey(key);
        }
       
        public int VertexDegree(K key) {
            return vertexDict[key].Count;
        }
    }
}
