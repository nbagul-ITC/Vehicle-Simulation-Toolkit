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
    public class PositionLog {
        public List<float> time;
        public List<float> x;
        public List<float> y;
        public List<float> z;
        public List<float> phi;
        public List<float> theta;
        public List<float> psi;

        public PositionLog(){
            time = new List<float>();
            x = new List<float>();
            y = new List<float>();
            z = new List<float>();
            phi = new List<float>();
            theta = new List<float>();
            psi = new List<float>();
        }

        public void Test() {
            time.Add(2.3f);
            time.Add(3.4f);
            x.Add(2.3f);
            x.Add(3.4f);
            y.Add(2.3f);
            y.Add(3.4f);
            z.Add(2.3f);
            z.Add(3.4f);
            phi.Add(2.3f);
            phi.Add(3.4f);
            theta.Add(2.3f);
            theta.Add(3.4f);
            psi.Add(2.3f);
            psi.Add(3.4f);

            ToJSON("Test.json");
        }

        public void ToJSON(string filename) {
            JSONObject positionLog = new JSONObject();
            positionLog.AddField("time",floatListToString(time));
            positionLog.AddField("x",floatListToString(x));
            positionLog.AddField("y",floatListToString(y));
            positionLog.AddField("z",floatListToString(z));
            positionLog.AddField("phi",floatListToString(phi));
            positionLog.AddField("theta",floatListToString(theta));
            positionLog.AddField("psi",floatListToString(psi));
            File.WriteAllText(filename, positionLog.print());
        }

        private string floatListToString(List<float> floatList) {
            string s = "";
            foreach(float f in floatList) {
                s = s + f.ToString() + ",";
            }
            s = s.Substring(0,s.Length-1);
            return s;
        }

        public void AddGameObjectPosition(GameObject obj, float timeStamp) {
            time.Add(timeStamp);
            x.Add(obj.transform.position.x);
            y.Add(obj.transform.position.y);
            z.Add(obj.transform.position.z);
            phi.Add(obj.transform.rotation.eulerAngles.x);
            theta.Add(obj.transform.rotation.eulerAngles.y);
            psi.Add(obj.transform.rotation.eulerAngles.z);
        }
    }
}