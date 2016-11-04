//------------------------------------------------------------------------------------------------
// Vehicle Simulation Environment
// Jonathan Shum - Mountain View, CA
// Toyota InfoTechnology Center USA
//------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace VehicleSimulation {
    public class TerrainTextureGenerator : MonoBehaviour {
        Terrain terr;
        int hmWidth;
        int hmHeight;
        float timer = 1.0f;
        float counter = 0.0f;

        void Start () {
            terr = Terrain.activeTerrain;
            hmWidth = terr.terrainData.heightmapWidth;
            hmHeight = terr.terrainData.heightmapHeight;
            Terrain.activeTerrain.heightmapMaximumLOD = 0;
        }

        void LateUpdate () {
            if (counter > timer) {
                float[,] heights = terr.terrainData.GetHeights(0,0,hmWidth,hmHeight);
                for (int i=0; i<hmWidth; i++) {
                    for (int j=0; j<hmHeight; j++) {
                        heights[i,j] = Random.Range(0.0f,0.002f);
                    }
                }
                terr.terrainData.SetHeights(0,0,heights);
                counter = 0.0f;
            }
            counter = counter + Time.deltaTime;
        }
    }
}
