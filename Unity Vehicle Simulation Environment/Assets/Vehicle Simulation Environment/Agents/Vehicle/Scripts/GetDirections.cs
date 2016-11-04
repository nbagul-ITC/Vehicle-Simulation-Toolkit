//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VehicleSimulation {
    public class GetDirections : MonoBehaviour {
        public string api_key = "";
        JSONObject directionData;
        public Vector2 startingLocation = new Vector2(37.3890019f,-122.0518973f);
        public Vector2 endingLocation = new Vector2(37.3719489f,-122.0413131f);

        void Start () {
            StartCoroutine(PullDirections());
        }
            
        public IEnumerator PullDirections() {
            string url = @"http://valhalla.mapzen.com/route?json=" + WWW.EscapeURL(@"{""locations"":[{""lat"":" + startingLocation.x.ToString() + @",""lon"":" + startingLocation.y.ToString() + @"},{""lat"":" + endingLocation.x.ToString() + @",""lon"":" + endingLocation.y.ToString() + @"}],""costing"":""auto"",""costing_options"":{""auto"":{""country_crossing_penalty"":2000.0}},""directions_options"":{""units"":""miles""}}") + "&id=my_work_route&api_key=" + api_key;
            string direction_file = "TestNav";
            // Limit number of API calls
            if (File.Exists(direction_file)) {
                var r = new StreamReader(direction_file, Encoding.Default);
                directionData = new JSONObject(r.ReadToEnd());
            } else {
                var www = new WWW(url);
                yield return www;
                Debug.Log(url);
                var sr = File.CreateText(direction_file);
                Debug.Log(www.text);
                sr.Write(www.text);
                sr.Close();
                directionData = new JSONObject(www.text);
            }
            PrintDirections(directionData);
        }

        private void PrintDirections(JSONObject directionData) {
            foreach (var direction in directionData["trip"]["legs"][0]["maneuvers"].list) {
                Debug.Log(direction.ToString(),gameObject);
            }
        }
    }
}
