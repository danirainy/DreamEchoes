﻿using DreamEchoesCore.Misc;
using RingLib;
using UnityEngine;

namespace DreamEchoesCore.Scenes;

internal class DreamEchoesSeer
{
    private class ClearSceneBorders : MonoBehaviour
    {
        private void Update()
        {
            var cleared = false;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var obj in scene.GetRootGameObjects())
            {
                if (obj.name.StartsWith("SceneBorder"))
                {
                    obj.SetActive(false);
                    cleared = true;
                }
            }
            if (cleared)
            {
                Log.LogInfo(typeof(DreamEchoesSeer).Name, "Cleared Scene Borders");
                Object.Destroy(this);
            }
        }
    }

    public static void Initialize(string sceneName)
    {
        if (sceneName != "DreamEchoesSeer")
        {
            return;
        }

        Log.LogInfo(typeof(DreamEchoesSeer).Name, "Initializing DreamEchoesSeer");

        GameObject prefab;
        GameObject instance;
        foreach (var (key, value) in Preload.Names)
        {
            if (key != "Dream_Backer_Shrine")
            {
                continue;
            }
            prefab = DreamEchoesCore.GetPreloaded(key, value);
            instance = Object.Instantiate(prefab);
            foreach (var spriteRender in instance.GetComponentsInChildren<SpriteRenderer>(true))
            {
                var color = spriteRender.color;
                Color.RGBToHSV(color, out float H, out float S, out float V);
                H = (H * 360 + 45) % 360 / 360;
                color = Color.HSVToRGB(H, S, V);
                spriteRender.color = color;
            }
            if (value.StartsWith("CameraLockArea"))
            {
                var boxCollider = instance.GetComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(120.5565f, 24.5745f);
            }
            if (value.StartsWith("dream_scene pieces"))
            {
                var clouds = instance.transform.Find("wp_clouds");
                var cloud = clouds.Find("wp_clouds_0002_1 (80)").gameObject;
                GameObject.Destroy(cloud);
            }
            if (value.StartsWith("water_fog(Clone)"))
            {
                var spriteRender = instance.GetComponent<SpriteRenderer>();
                spriteRender.color = new Color(0.6792f, 0.4118f, 1, 0.75f);
            }
            if (value.StartsWith("dream_fog"))
            {
                var spriteRender = instance.GetComponent<SpriteRenderer>();
                spriteRender.color = new Color(0.6792f, 0.4118f, 0.75f, 0.75f);
            }
            instance.SetActive(true);
        }

        prefab = DreamEchoesCore.GetPreloaded("Mines_34", "mine_summit_statue");
        instance = Object.Instantiate(prefab);
        instance.transform.position = new Vector3(90, 31, 26);
        instance.transform.localScale = new Vector3(1.5f, 1.5f, 1);
        instance.SetActive(true);

        prefab = DreamEchoesCore.GetPreloaded("RestingGrounds_04", "dreamer_statue/Dreamer_statue_horn_left");
        instance = Object.Instantiate(prefab);
        instance.transform.position = new Vector3(75, 30, 26);
        instance.transform.localScale = new Vector3(2, 2, 1);
        instance.SetActive(true);

        prefab = DreamEchoesCore.GetPreloaded("RestingGrounds_04", "dreamer_statue/Dreamer_statue_horn_right");
        instance = Object.Instantiate(prefab);
        instance.transform.position = new Vector3(105, 30, 26);
        instance.transform.localScale = new Vector3(2, 2, 1);
        instance.SetActive(true);

        instance = new GameObject("ClearSceneBorders");
        instance.AddComponent<ClearSceneBorders>();

        Log.LogInfo(typeof(DreamEchoesSeer).Name, "Initialized DreamEchoesSeer");
    }
}
