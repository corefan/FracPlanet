﻿using System;
using UnityEngine;
// class to manage materials for planet / layer types.
// procedually modify the shader properties to give each planet a uniqe look.
public class PlanetMaterial : MonoBehaviour {
    public int seed = 100;
    private System.Random rnd;
    private string txrPostFix = "";
    
    public Material AssignMaterial(string layer, string curPlanetType = "", int NewSeed = 100, bool reassign = false) {
        seed = NewSeed;
        txrPostFix = "";
        if (layer == "terrain") {
            Texture controlTexture = Resources.Load("PlanetTextures/SplatElevation") as Texture;
            Texture[] terrainTextures = new Texture[6];
            Texture[] terrainNormals = new Texture[6];
            Material planetSurfaceMaterial = new Material(Shader.Find("Custom/Splatmap"));
            planetSurfaceMaterial.SetColor("_Color", TerrainColor(curPlanetType, seed));
            planetSurfaceMaterial.SetTexture("_Control", controlTexture);
            planetSurfaceMaterial.SetFloat("_Smoothness", TerrainShininess(curPlanetType, seed));
            planetSurfaceMaterial.SetColor("_Specular", TerrainColor(curPlanetType, seed, true));
            for (int i = 0; i <= terrainTextures.Length - 1; i++ ) {
                txrPostFix = curPlanetType + (i + 1);
                terrainNormals[i] = Resources.Load("PlanetTextures/" + curPlanetType + "/nm" + txrPostFix) as Texture;
                // if we're far away, load the less detailed diffuse textures.
                if (reassign) { txrPostFix = curPlanetType + (i + 1); } else { txrPostFix = curPlanetType + (i + 1) + "f"; }
                terrainTextures[i] = Resources.Load("PlanetTextures/" + curPlanetType + "/txr" + txrPostFix) as Texture;
                planetSurfaceMaterial.SetTexture("_Texture" + (i + 1), terrainTextures[i]);
                planetSurfaceMaterial.SetTexture("_Normal" + (i + 1), terrainNormals[i]);
                planetSurfaceMaterial.SetTextureScale("_Texture" + (i + 1), TerrainTiling(curPlanetType, seed + i));
                planetSurfaceMaterial.SetTextureScale("_Normal" + (i + 1), planetSurfaceMaterial.GetTextureScale("_Texture" + (i + 1)));
            }
            return planetSurfaceMaterial;
        }

        if (layer == "ocean") {
            txrPostFix = "f";
            if (reassign) { txrPostFix = ""; }
            Texture curTypeTexture = Resources.Load("PlanetTextures/" + curPlanetType + "/txrOcean" + curPlanetType + txrPostFix) as Texture;
            Material oceanMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
            oceanMaterial.SetColor("_TintColor", OceanColor(seed));
            oceanMaterial.SetTexture("_MainTex", curTypeTexture);
            oceanMaterial.mainTextureScale = OceanTiling(seed);
            return oceanMaterial;
        }

        if (layer == "atmosphere") {
            Texture curTypeTexture = Resources.Load("PlanetTextures/" + curPlanetType + "/txrAtmosphere" + curPlanetType) as Texture;
            Texture curTypeNormalMap = Resources.Load("PlanetTextures/" + curPlanetType + "/nmAtmosphere" + curPlanetType) as Texture;
            Material atmosphereMaterial = new Material(Shader.Find("Standard"));
            atmosphereMaterial.SetTexture("_MainTex", curTypeTexture);
            atmosphereMaterial.SetTexture("_BumpMap", curTypeNormalMap);
            atmosphereMaterial.SetFloat("_Mode", 2);
            atmosphereMaterial.SetFloat("_Glossiness", .6F);
            atmosphereMaterial.SetFloat("_Metallic", .7F);
            atmosphereMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            atmosphereMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            atmosphereMaterial.SetInt("_ZWrite", 0);
            atmosphereMaterial.DisableKeyword("_ALPHATEST_ON");
            atmosphereMaterial.EnableKeyword("_ALPHABLEND_ON");
            atmosphereMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            atmosphereMaterial.renderQueue = 3000;
            atmosphereMaterial.SetColor("_SpecColor", AtmosphereColor(curPlanetType, seed));
            atmosphereMaterial.SetColor("_Color", AtmosphereColor(curPlanetType, seed));
            return atmosphereMaterial;
        }

        if (layer == "cloud") {
            rnd = new System.Random(seed);
            int cloudTxrN = rnd.Next(1, 4);
            Texture curTypeTexture = Resources.Load("PlanetTextures/txrCloud" + cloudTxrN) as Texture;
            Material cloudMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
            cloudMaterial.SetTexture("_MainTex", curTypeTexture);
            cloudMaterial.SetColor("_TintColor", CloudColor(curPlanetType, seed));
            cloudMaterial.mainTextureScale = CloudTiling(curPlanetType, seed);
            cloudMaterial.renderQueue = 3000;
            // Adjust the render queue if we're on the surface so it looks right.
            if (reassign) {
                cloudMaterial.renderQueue = 2900;
            }
            return cloudMaterial;
        }

        if (layer == "outline") {
            Texture planetOutlineTexture = Resources.Load("PlanetTextures/txrPlanetOutline") as Texture;
            Material planetOutlineMaterial = new Material(Shader.Find("Particles/Alpha Blended"));
            planetOutlineMaterial.SetTexture("_MainTex", planetOutlineTexture);
            return planetOutlineMaterial;
        }
        return new Material(Shader.Find("Standard"));
    }

    private Color32 CloudColor(string curPlanetType, int seed) {
        rnd = new System.Random(seed);
        if (curPlanetType.Contains("Terra")) {
            // more Earthy white/gray kind of clouds.
            // setup RGB and Alpha.
            int R = rnd.Next(150, 255); int B = rnd.Next(150, 255);
            int G = rnd.Next(150, 255); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        if (curPlanetType.Contains("Icy")) {
            // more Titan/yellow kind of clouds.
            int R = rnd.Next(150, 255); int B = rnd.Next(0, 150);
            int G = rnd.Next(150, 255); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        if (curPlanetType.Contains("Molten")) {
            // more Venus/Hellplanet kind of clouds.
            int R = rnd.Next(200, 255); int B = rnd.Next(80, 200);
            int G = rnd.Next(80, 200); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        // more Venus/Hellplanet kind of clouds.
        return new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    }

    private Color32 OceanColor(int seed) {
        rnd = new System.Random(seed);
        // Give the ocean a unique tint.
        int R = rnd.Next(80, 140); int B = rnd.Next(80, 140);
        int G = rnd.Next(80, 140); int A = rnd.Next(140, 180);
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    private Color32 AtmosphereColor(string curPlanetType, int seed) {
        rnd = new System.Random(seed);
        if (curPlanetType.Contains("Terra")) {
            // random earth atmosphere.
            int R = rnd.Next(100, 215); int B = rnd.Next(100, 215);
            int G = rnd.Next(100, 215); int A = rnd.Next(100, 255);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        if (curPlanetType.Contains("Icy")) {
            // more Titan/yellow kind of clouds.
            int R = rnd.Next(150, 255); int B = rnd.Next(0, 150);
            int G = rnd.Next(150, 255); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        if (curPlanetType.Contains("Molten")) {
            // more Venus/Hellplanet kind of clouds.
            int R = rnd.Next(200, 255); int B = rnd.Next(200, 255);
            int G = rnd.Next(200, 255); int A = rnd.Next(5, 100);
            return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
        }
        return new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    }

    private Color32 TerrainColor(string curPlanetType, int seed, bool specular = false) {
        rnd = new System.Random(seed);
        int cMin = 220; int cMax = 255;
        if (!curPlanetType.Contains("Terra")) { cMax = 255; cMin = 150; }
        if (specular) { cMax = 70; cMin = 0; } 
        int R = rnd.Next(cMin, cMax); int B = rnd.Next(cMin, cMax);
        int G = rnd.Next(cMin, cMax); int A = rnd.Next(cMin, cMax);
        return new Color32((byte)R, (byte)G, (byte)B, (byte)A);
    }

    private float TerrainBumpiness(string curPlanetType, int seed) {
        rnd = new System.Random(seed);
        // not a lot of bumpy shadows, it's Earthy.
        if (curPlanetType.Contains("Terra")) {
            return (float)rnd.NextDouble() * (.6F - .2F) + .2F;
        }
        // bumpy shadows
        if (curPlanetType.Contains("Icy")) {
            return (float)rnd.NextDouble() * (1.3F - .6F) + .6F;
        }
        if (curPlanetType.Contains("Molten")) {
            return (float)rnd.NextDouble() * (3F - 1F) + 1F;
        }
        return 1F;
    }

    private float TerrainShininess(string curPlanetType, int seed) {
        rnd = new System.Random(seed);
        // diffuse non-shiny earth and gas.
        if (curPlanetType.Contains("Terra")) {
            return .02F;
        }
        else if (curPlanetType.Contains("Rock")) {
            return (float)rnd.NextDouble() * (.5F);
        }
        else {
            return (float)rnd.NextDouble();
        }
    }

    private Vector2 OceanTiling(int seed) {
        rnd = new System.Random(seed);
        int xTile = rnd.Next(1, 4);
        int yTile = rnd.Next(1, 4);
        return new Vector2(xTile, yTile);
    }

    private Vector2 CloudTiling(string curPlanetType, int seed) {
        rnd = new System.Random(seed);
        // nice clouds..
        if (curPlanetType.Contains("Terra")) {
            int xTile = rnd.Next(1, 5);
            int yTile = rnd.Next(1, 5);
            return new Vector2(xTile, yTile);
        }
        // thin clouds
        if (curPlanetType.Contains("Icy")) {
            int xTile = rnd.Next(1, 3);
            int yTile = rnd.Next(1, 3);
            return new Vector2(xTile, yTile);
        }
        // dense nasty clouds
        if (curPlanetType.Contains("Molten")) {
            int xTile = rnd.Next(1, 4);
            int yTile = rnd.Next(3, 7);
            return new Vector2(xTile, yTile);
        }
        return new Vector2(1F, 1F);
    }
    private Vector2 TerrainTiling(string curPlanetType, int seed) {
        rnd = new System.Random(seed);
        double xTile = rnd.NextDouble() * (3F - 1F) + 1F;
        double yTile = rnd.NextDouble() * (1.04F - 1F) + 1F;
        return new Vector2((int)xTile, (int)yTile);
    }
}
