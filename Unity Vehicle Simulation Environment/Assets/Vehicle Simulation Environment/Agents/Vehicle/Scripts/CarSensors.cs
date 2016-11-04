//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace VehicleSimulation {
    public class CarSensors : MonoBehaviour {

        public int lidarDegree = 6;
        public float lidarRange = 10.0f;
        public List<float> lidarData;
        public List<Quaternion> lidarRotations;

        public List<Camera> cameras;
        private List<string> cameraImages;
        public int resWidth = 1288;
        public int resHeight = 964;

        public int ultrasonicRange = 10;
        public int ultrasonicDegree = 3;
        public float ultrasonicData;
        public List<Quaternion> ultrasonicRotations;

        void Start () {
            lidarData = new List<float>();
            cameraImages = new List<string>();
        }
        
        void Update () {
            UpdateLidar();
            UpdateCamera();
            UpdateUltrasonic();
        }

        void UpdateCamera() {
            cameraImages.Clear();
            if (cameras != null) {
                foreach (Camera cam in cameras) {
                    RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
                    cam.targetTexture = rt;
                    Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
                    cam.Render();
                    RenderTexture.active = rt;
                    screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                    cam.targetTexture = null;
                    RenderTexture.active = null;
                    Destroy(rt);
                    byte[] bytes = screenShot.EncodeToPNG();
                    cameraImages.Add(Convert.ToBase64String(bytes));
                    //UnityEngine.Debug.Log(Convert.ToBase64String(bytes));
                    //string filename = "ScreenShot";
                    //System.IO.File.WriteAllBytes(filename, bytes);
                }
            }
        }

        public string getCameraImage(int index){
            return cameraImages[index];
        }

        void UpdateLidar() {
            RaycastHit[] hits;
            List<Ray> landingRays = generateAngularSensors(-135, 135, lidarDegree, lidarRange);
            lidarData.Clear();
            for (int j = 0; j < landingRays.Count; j++) {
                Ray landingRay = landingRays[j]; 
                hits = Physics.RaycastAll(landingRay, lidarRange);
                if(hits.Length > 0) {
                    if(!(hits.Length == 1 && hits[0].transform == gameObject.transform)) {
                        float min = hits[0].distance;
                        //int minIndex = 0;
                        for(int i = 1; i < hits.Length; ++i) {
                            if(hits[i].transform != gameObject.transform && hits[i].distance < min) {
                                min = hits[i].distance;
                                //minIndex = i;
                            }
                        }
                        lidarData.Add(min);
                        UnityEngine.Debug.DrawRay(transform.position + transform.up, lidarRotations[j] * transform.forward * min, Color.green);
                   }
                } else {
                    lidarData.Add(lidarRange);
                    UnityEngine.Debug.DrawRay(transform.position + transform.up, lidarRotations[j] * transform.forward * lidarRange, Color.green);
                    //Gizmos.DrawRay(transform.position + transform.up, lidarRotations[j] * transform.forward * lidarRange, Color.red);
                }
            }
        }

        private List<Ray> generateAngularSensors(int rayInitial, int rayFinal, int rayDegree, float range) {
            List<Ray> landingRays = new List<Ray>();
            lidarRotations.Clear();
            for (int i = rayInitial; i <= rayFinal; i = i + rayDegree) {
                Quaternion rot = Quaternion.Euler(0,-1*i,0);
                lidarRotations.Add(rot);
                landingRays.Add(new Ray(transform.position + transform.up, rot * transform.forward * range));
            }
            return landingRays;
        }

        public string getLidarData() {
            string newListString = "(";
            foreach(float f in lidarData) {
                newListString = newListString + f.ToString() + ",";
            }
            return newListString.Substring(1,newListString.Length-2);
        }

        void UpdateUltrasonic(){
            RaycastHit[] hits;
            List<Ray> landingRays = generateAngularSensors(150, 210, ultrasonicDegree, ultrasonicRange);
            lidarData.Clear();
            ultrasonicData = ultrasonicRange;
            for (int j = 0; j < landingRays.Count; j++) {
                Ray landingRay = landingRays[j]; 
                hits = Physics.RaycastAll(landingRay, lidarRange);
                if(hits.Length > 0) {
                    if(!(hits.Length == 1 && hits[0].transform == gameObject.transform)) {
                        float min = hits[0].distance;
                        //int minIndex = 0;
                        for(int i = 1; i < hits.Length; ++i) {
                            if(hits[i].transform != gameObject.transform && hits[i].distance < min) {
                                min = hits[i].distance;
                                //minIndex = i;
                            }
                        }
                        if (ultrasonicData > min){
                            ultrasonicData = min;
                        }
                        UnityEngine.Debug.DrawRay(transform.position + transform.up, lidarRotations[j] * transform.forward * min, Color.white);
                   }
                } else {
                    UnityEngine.Debug.DrawRay(transform.position + transform.up, lidarRotations[j] * transform.forward * lidarRange, Color.white);
                    Gizmos.DrawRay(transform.position + transform.up, lidarRotations[j] * transform.forward * lidarRange);
                }
            }
        }
    }
}
