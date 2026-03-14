using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace Juul
{
    internal class Audios : MonoBehaviour
    {
        public static Audios instance;
        private static Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
        private static string tempDir;

        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            tempDir = Path.Combine(Path.GetTempPath(), "JuulAudio");
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);
        }

        private static void EnsureInstance()
        {
            if (instance != null) return;
            var obj = new GameObject("AudioManager");
            instance = obj.AddComponent<Audios>();
            DontDestroyOnLoad(obj);
        }

        public static void Play(string name, float volume = 0.5f)
        {
            EnsureInstance();
            instance.StartCoroutine(instance.LoadAndPlay(name, volume));
        }

        private IEnumerator LoadAndPlay(string name, float volume)
        {
            if (clipCache.TryGetValue(name, out AudioClip cached) && cached != null)
            {
                SpawnAndPlay(cached, volume);
                yield break;
            }
            string tempPath = Path.Combine(tempDir, name + ".mp3");
            if (!File.Exists(tempPath))
            {
                byte[] data = GetEmbeddedResource(name + ".mp3");
                if (data == null) yield break;
                File.WriteAllBytes(tempPath, data);
            }
            string fileUri = "file:///" + tempPath.Replace("\\", "/");
            using (var req = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.MPEG))
            {
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success) yield break;

                var clip = DownloadHandlerAudioClip.GetContent(req);
                if (clip == null) yield break;

                clipCache[name] = clip;
                SpawnAndPlay(clip, volume);
            }
        }

        private static byte[] GetEmbeddedResource(string resourceFileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = null;
            foreach (string name in assembly.GetManifestResourceNames())
            {
                if (name.EndsWith(resourceFileName, StringComparison.OrdinalIgnoreCase))
                {
                    resourceName = name;
                    break;
                }
            }
            if (resourceName == null) return null;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                return data;
            }
        }

        private void SpawnAndPlay(AudioClip clip, float volume)
        {
            var obj = new GameObject("AudioSource");
            DontDestroyOnLoad(obj);
            var src = obj.AddComponent<AudioSource>();
            src.clip = clip;
            src.volume = volume;
            src.Play();
            Destroy(obj, clip.length + 0.5f);
        }
    }
}