using Photon.Pun;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Juul
{
    internal class Soundboard
    {
        public static bool IsPlaying = false;
        public static float RecoverTimer = -1f;
        public static bool HearSelf = false;
        public static bool LoopAudio = false;

        private static GameObject _audioObj;
        private static AudioSource _audioSource;
        private static Dictionary<string, AudioClip> _clipCache = new Dictionary<string, AudioClip>();
        private static bool _loaded = false;

        public static string SoundsFolder
        {
            get
            {
                string path = Path.Combine(Core.Folder, "Sounds");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        public static void PopulateSounds()
        {
            Category soundCat = Buttons.GetCategory("Soundboard");
            if (soundCat == null) return;

            soundCat.Buttons.Clear();
            soundCat.Buttons.Add(new Button { Name = "Stop All Sounds", Toggle = false, OnEnable = () => HaltAllAudio() });
            soundCat.Buttons.Add(new Button { Name = "Hear Audios Playing", Toggle = true, Enabled = HearSelf, OnEnable = () => HearSelf = true, OnDisable = () => HearSelf = false });
            soundCat.Buttons.Add(new Button { Name = "Loop Audio", Toggle = true, Enabled = LoopAudio, OnEnable = () => LoopAudio = true, OnDisable = () => LoopAudio = false });
            soundCat.Buttons.Add(new Button { Name = "Open Sound Folder", Toggle = false, OnEnable = () => RevealSoundFolder() });
            soundCat.Buttons.Add(new Button { Name = "Reload Sounds", Toggle = false, OnEnable = () => { LoadSoundboard(false); } });
            soundCat.Buttons.Add(new Button { Name = "↓ Sounds ↓", Toggle = false, Label = true });

            string[] supportedExts = { ".wav", ".ogg", ".mp3" };
            string[] files = new string[0];

            try
            {
                files = Directory.GetFiles(SoundsFolder, "*.*", SearchOption.AllDirectories)
                    .Where(f => supportedExts.Contains(Path.GetExtension(f).ToLower()))
                    .OrderBy(f => Path.GetFileNameWithoutExtension(f))
                    .ToArray();
            }
            catch { }

            foreach (string file in files)
            {
                string displayName = Path.GetFileNameWithoutExtension(file).Replace("_", " ");
                string filePath = file;

                soundCat.Buttons.Add(new Button
                {
                    Name = displayName,
                    Toggle = true,
                    OnceEnable = () => PlayFile(filePath),
                    OnceDisable = () => HaltAudio()
                });
            }

            _loaded = true;
        }

        public static void PlayFile(string filePath)
        {
            HaltAudio();

            string ext = Path.GetExtension(filePath).ToLower();
            AudioType audioType = AudioType.WAV;
            if (ext == ".ogg") audioType = AudioType.OGGVORBIS;
            else if (ext == ".mp3") audioType = AudioType.MPEG;

            Audios.EnsureInstance();
            Audios.instance.StartCoroutine(LoadAndPlay(filePath, audioType));
        }

        private static IEnumerator LoadAndPlay(string filePath, AudioType audioType)
        {
            AudioClip clip = null;

            if (_clipCache.TryGetValue(filePath, out AudioClip cached) && cached != null)
            {
                clip = cached;
            }
            else
            {
                string fileUri = "file:///" + filePath.Replace("\\", "/");
                using (var request = UnityWebRequestMultimedia.GetAudioClip(fileUri, audioType))
                {
                    yield return request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success) yield break;

                    clip = DownloadHandlerAudioClip.GetContent(request);
                    if (clip == null) yield break;

                    _clipCache[filePath] = clip;
                }
            }

            PushToMic(clip);
        }

        private static void PushToMic(AudioClip clip)
        {
            if (clip == null) return;

            if (PhotonNetwork.InRoom)
            {
                if (HearSelf)
                    PlayLocal(clip);

                try
                {
                    var recorder = GorillaTagger.Instance.myRecorder;
                    if (recorder != null)
                    {
                        recorder.SourceType = Recorder.InputSourceType.AudioClip;
                        recorder.AudioClip = clip;
                        recorder.LoopAudioClip = LoopAudio;
                        recorder.IsRecording = false;
                        recorder.RestartRecording(true);
                    }
                }
                catch
                {
                    PlayLocal(clip);
                }
            }
            else
            {
                PlayLocal(clip);
            }

            IsPlaying = true;
            RecoverTimer = Time.time + clip.length + 0.5f;
        }

        private static void PlayLocal(AudioClip clip)
        {
            EnsureAudioObj();
            _audioSource.clip = clip;
            _audioSource.loop = LoopAudio;
            _audioSource.volume = 1f;
            _audioSource.Play();
        }

        public static void HaltAudio()
        {
            if (_audioSource != null)
                _audioSource.Stop();

            if (PhotonNetwork.InRoom)
            {
                try
                {
                    var recorder = GorillaTagger.Instance.myRecorder;
                    if (recorder != null)
                    {
                        recorder.SourceType = Recorder.InputSourceType.Microphone;
                        recorder.AudioClip = null;
                        recorder.IsRecording = false;
                        recorder.RestartRecording(true);
                    }
                }
                catch { }
            }

            Category soundCat = Buttons.GetCategory("Soundboard");
            if (soundCat != null)
            {
                for (int i = 0; i < soundCat.Buttons.Count; i++)
                {
                    if (soundCat.Buttons[i].Toggle && soundCat.Buttons[i].Enabled)
                        soundCat.Buttons[i].Enabled = false;
                }
            }

            IsPlaying = false;
            RecoverTimer = -1f;
        }

        public static void UpdateSoundboard()
        {
            if (Buttons.Modules == null) return;

            if (!_loaded)
                PopulateSounds();

            if (!LoopAudio && IsPlaying && RecoverTimer > 0f && Time.time >= RecoverTimer)
                HaltAudio();
        }

        private static void EnsureAudioObj()
        {
            if (_audioObj == null)
            {
                _audioObj = new GameObject("JuulSoundboard");
                Object.DontDestroyOnLoad(_audioObj);
                _audioSource = _audioObj.AddComponent<AudioSource>();
                _audioSource.spatialBlend = 0f;
                _audioSource.playOnAwake = false;
            }
            else if (_audioSource == null)
            {
                _audioSource = _audioObj.GetComponent<AudioSource>() ?? _audioObj.AddComponent<AudioSource>();
            }
        }

        public static void HaltAllAudio() => HaltAudio();

        public static void RevealSoundFolder()
        {
            string path = SoundsFolder;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch { }
        }

        public static void LoadSoundboard(bool switchCategory = true)
        {
            _clipCache.Clear();
            _loaded = false;
            PopulateSounds();
            if (switchCategory)
            {
                Category soundCat = Buttons.GetCategory("Soundboard");
                if (soundCat != null)
                {
                    Core.ActiveCategory = soundCat;
                    Core.CurrentPage = 0;
                    Core.RebuildMenu();
                }
            }
        }
    }
}


