//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace VehicleSimulation {
    public class FrameRateLog : MonoBehaviour {
        public float deltaTime = 0.0f;
        StreamWriter frameRateLog;
        public int frameNum = 0;
        public string fileName = "FrameRateLog.txt";

        void Start() {
        	frameRateLog = File.CreateText(fileName);
        }

        void FixedUpdate () {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            frameRateLog.WriteLine("Frame " + frameNum.ToString() + ": " + fps.ToString() + " fps");
            frameNum = frameNum + 1;
        }

        void OnGUI() {
            int w = Screen.width;
            int h = Screen.height;
            GUIStyle style = new GUIStyle();
            Rect rect = new Rect(0, 0, w, h*2/100);
            style.alignment = TextAnchor.UpperRight;
            style.fontSize = h * 2 / 150;
            style.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.} FPS", fps);
            GUI.Label(rect,text,style);
        }
    }
}