//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VehicleSimulation {
    public class VisualizeJSON : MonoBehaviour {
        public string filename;
        public List<float> time = new List<float>();
        public List<float> x = new List<float>();
        public List<float> y = new List<float>();
        public List<float> z = new List<float>();
        public List<float> phi = new List<float>();
        public List<float> theta = new List<float>();
        public List<float> psi = new List<float>();

        public bool paused;
        public bool completed;
        public float elapsedTime = 0.0f;

        void Start () {
            JSONObject positionData;
            if (filename != "") {
                var r = new StreamReader(filename, Encoding.Default);
                positionData = new JSONObject(r.ReadToEnd());
                if (positionData["time"] != null) {
                    foreach (var t in positionData["time"].ToString().Replace("\"","").Split(',')) {
                        time.Add((float)Double.Parse(t.ToString()));
                    }
                }
                if (positionData["x"] != null) {
                    foreach (var t in positionData["x"].ToString().Replace("\"","").Split(',')) {
                        x.Add((float)Double.Parse(t.ToString()));
                    }
                }
                if (positionData["y"] != null) {
                    foreach (var t in positionData["y"].ToString().Replace("\"","").Split(',')) {
                        y.Add((float)Double.Parse(t.ToString()));
                    }
                }
                if (positionData["z"] != null) {
                    foreach (var t in positionData["z"].ToString().Replace("\"","").Split(',')) {
                        z.Add((float)Double.Parse(t.ToString()));
                    }
                }
                if (positionData["phi"] != null) {
                    foreach (var t in positionData["phi"].ToString().Replace("\"","").Split(',')) {
                        phi.Add((float)Double.Parse(t.ToString()));
                    }
                }
                if (positionData["theta"] != null) {
                    foreach (var t in positionData["theta"].ToString().Replace("\"","").Split(',')) {
                        theta.Add((float)Double.Parse(t.ToString()));
                    }
                }
                if (positionData["psi"] != null) {
                    foreach (var t in positionData["psi"].ToString().Replace("\"","").Split(',')) {
                        psi.Add((float)Double.Parse(t.ToString()));
                    }
                }
            }
            paused = false;
        }
        
        void Update () {
            if (!paused) {
                elapsedTime = elapsedTime + Time.deltaTime;
            }
            if (filename != "") {
                float closest = time.Aggregate((xt,yt) => Mathf.Abs(xt-elapsedTime) < Mathf.Abs(yt-elapsedTime) ? xt : yt);
                int index = time.IndexOf(closest);
                if (index == time.Count) {
                    completed = true;
                } else {
                    Vector3 temp_pos = new Vector3(getFromArray(x,index),getFromArray(z,index),getFromArray(y,index));
                    gameObject.transform.position = temp_pos;
                    Vector3 temp_rot = new Vector3(getFromArray(phi,index),getFromArray(psi,index),getFromArray(theta,index));
                    gameObject.transform.eulerAngles = temp_rot;
                    completed = false;
                }
            }
        }

        public float getFromArray(List<float> a,int i) {
            if (a == null) {
                return 0.0f;
            } else if (a.Count == 0) {
                return 0.0f;
            } else {
                return a[i];
            }
        }

        public void pause() {
            paused = true;
        }

        public void reset() {
            elapsedTime = 0.0f;
        }

        public void play() {
            paused = false;
        }
    }
}
