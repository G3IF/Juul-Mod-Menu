using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Juul
{
    internal class Audios : MonoBehaviour
    {
        public static Audios instance;
        private static Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private static void EnsureInstance()
        {
            if (instance != null) return;
            var obj = new GameObject("AudioManager");
            instance = obj.AddComponent<Audios>();
            DontDestroyOnLoad(obj);
        }

        public static void Play(string url, float volume = 0.5f)
        {
            EnsureInstance();
            instance.StartCoroutine(instance.LoadAndPlay(url, volume));
        }

        private IEnumerator LoadAndPlay(string url, float volume)
        {
            if (clipCache.TryGetValue(url, out AudioClip cached) && cached != null)
            {
                SpawnAndPlay(cached, volume);
                yield break;
            }

            var ext = Path.GetExtension(url).TrimStart('.').ToLower();
            var audioType = GetAudioType(ext);
            if (audioType == AudioType.UNKNOWN) yield break;

            using (var req = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                ((DownloadHandlerAudioClip)req.downloadHandler).streamAudio = true;
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success) yield break;

                var clip = DownloadHandlerAudioClip.GetContent(req);
                if (clip == null) yield break;

                clipCache[url] = clip;
                SpawnAndPlay(clip, volume);
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

        private static AudioType GetAudioType(string ext)
        {
            switch (ext)
            {
                case "mp3": return AudioType.MPEG;
                case "wav": return AudioType.WAV;
                case "ogg": return AudioType.OGGVORBIS;
                case "aiff": case "aif": return AudioType.AIFF;
                default: return AudioType.UNKNOWN;
            }
        }
    }
}