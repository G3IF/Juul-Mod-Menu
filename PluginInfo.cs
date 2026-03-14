using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using UnityEngine;
using HarmonyLib;

namespace Juul
{
    [BepInPlugin(guid, title, version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string guid = "Juul";
        public const string title = "Juul";
        public const string version = "2.0";
        private GameObject coreObject;

        void Awake()
        {
            try
            {
                var harmony = new Harmony(guid);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
               
            }
            coreObject = new GameObject("JuulCore");
            UnityEngine.Object.DontDestroyOnLoad(coreObject);
            StartCoroutine(DelayedInitialize());
        }

        private IEnumerator DelayedInitialize()
        {
            float waitTime = 0f;
            while (GorillaTagger.Instance == null && waitTime < 30f)
            {
                yield return new WaitForSeconds(0.5f);
                waitTime += 0.5f;
            }

            if (GorillaTagger.Instance == null)
            {
                yield break;
            }

            try
            {
                Buttons.Initialize();
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                }
                yield break;
            }
            try
            {
                coreObject.AddComponent<Core>();
            }
            catch (Exception e)
            {

                yield break;
            }
            try
            {
                Audios.Play("Loaded");
            }
            catch (Exception e)
            {
            }

            
        }

        void OnDestroy()
        {
            if (coreObject != null)
            {
                UnityEngine.Object.Destroy(coreObject);
            }
        }
    }
}