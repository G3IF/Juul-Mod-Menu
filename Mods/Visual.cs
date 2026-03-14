using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static Mono.Security.X509.X520;
using static Unity.Burst.Intrinsics.X86.Avx;
using Application = UnityEngine.Application;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;
using Text = UnityEngine.UI.Text;

namespace Juul
{
    public class Visual
    {
        private static readonly int[] bones = new int[]
        {
            4,3,5,4,19,18,20,19,3,
            18,21,20,22,21,25,21,29,
            21,31,29,27,25,24,22,6,5,
            7,6,10,6,14,6,16,14,12,10,9,7
        };

        private static Dictionary<VRRig, LineLib.Line[]> rigLineCache = new Dictionary<VRRig, LineLib.Line[]>();
        private static Dictionary<VRRig, LineLib.Line> tracerLineCache = new Dictionary<VRRig, LineLib.Line>();
        private static Dictionary<VRRig, LineLib.Line[]> box2DLineCache = new Dictionary<VRRig, LineLib.Line[]>();
        private static Dictionary<VRRig, LineLib.Line[]> box3DLineCache = new Dictionary<VRRig, LineLib.Line[]>();
        private static Dictionary<VRRig, Canvas> playernamecache = new Dictionary<VRRig, Canvas>();
        private static Dictionary<VRRig, TextMeshPro> playernametextcache = new Dictionary<VRRig, TextMeshPro>();
        private static Dictionary<VRRig, Material> originalMaterials = new Dictionary<VRRig, Material>();
        private static Dictionary<VRRig, Color> originalColors = new Dictionary<VRRig, Color>();

        public static bool AdvancedNametags = false;

        private const float width = 0.015f;
        private const float tracerWidth = 0.01f;
        private const float boxWidth = 0.012f;
        private static GameObject hudInstance;
        private static TextMesh textMesh;
        private static float hudTimer = 0f;
        private static int currentFPS = 0;
        public static void VisualPlayer(VRRig rig, Color color)
        {
            if ( rig == null ) return;

            rig.mainSkin.material.shader = Shader.Find("GUI/Text Shader");
            rig.mainSkin.material.color = color;    
        }
        public static void CleanUpPlayer()
        {
            if (PhotonNetwork.InRoom)
            {
                VRRig player = Rigs.GetVRRigFromPlayer(PhotonNetwork.MasterClient);
                player.mainSkin.material.shader = Shader.Find("GorillaTag/UberShader");
                player.mainSkin.material.color = player.playerColor;
            }
        }
        public static void BoneESP()
        {
            if (GorillaParent.instance == null || VRRigCache.ActiveRigs == null)
                return;

            if (GorillaTagger.Instance == null)
                return;

            VRRig[] currentRigs = VRRigCache.ActiveRigs.ToArray();
            HashSet<VRRig> activeRigs = new HashSet<VRRig>();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == GorillaTagger.Instance.offlineVRRig)
                    continue;

                if (rig.head == null || rig.head.rigTarget == null)
                    continue;

                if (rig.mainSkin == null || rig.mainSkin.bones == null || rig.mainSkin.bones.Length == 0)
                    continue;

                activeRigs.Add(rig);

                if (!rigLineCache.ContainsKey(rig))
                {
                    CreateLinesForRig(rig);
                }
                else
                {
                    UpdateLinesForRig(rig);
                }
            }

            CleanupDisconnectedBones(activeRigs);
        }

        private static void CreateLinesForRig(VRRig rig)
        {
            int numBoneConnections = bones.Length / 2;
            LineLib.Line[] lines = new LineLib.Line[numBoneConnections];

            for (int i = 0; i < bones.Length; i += 2)
            {
                try
                {
                    int boneIndexA = bones[i];
                    int boneIndexB = bones[i + 1];

                    if (boneIndexA >= rig.mainSkin.bones.Length || boneIndexB >= rig.mainSkin.bones.Length)
                        continue;

                    Transform boneA = rig.mainSkin.bones[boneIndexA];
                    Transform boneB = rig.mainSkin.bones[boneIndexB];

                    if (boneA != null && boneB != null)
                    {
                        LineLib.Line boneLine = LineLib.CreateLine(boneA.position, boneB.position, width, Core.BaseColor);
                        lines[i / 2] = boneLine;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to create bone line at index {i}: {e.Message}");
                }
            }

            rigLineCache[rig] = lines;
        }

        private static void UpdateLinesForRig(VRRig rig)
        {
            if (!rigLineCache.ContainsKey(rig))
                return;

            LineLib.Line[] lines = rigLineCache[rig];

            if (lines == null || lines.Length == 0)
                return;

            for (int i = 0; i < bones.Length; i += 2)
            {
                try
                {
                    int boneIndexA = bones[i];
                    int boneIndexB = bones[i + 1];
                    int lineIndex = i / 2;

                    if (boneIndexA >= rig.mainSkin.bones.Length || boneIndexB >= rig.mainSkin.bones.Length)
                        continue;

                    if (lineIndex >= lines.Length || lines[lineIndex] == null)
                        continue;

                    Transform boneA = rig.mainSkin.bones[boneIndexA];
                    Transform boneB = rig.mainSkin.bones[boneIndexB];

                    if (boneA != null && boneB != null)
                    {
                        lines[lineIndex].UpdatePosition(boneA.position, boneB.position);
                        lines[lineIndex].UpdateColor(Core.BaseColor);
                        lines[lineIndex].SetActive(true);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to update bone line at index {i}: {e.Message}");
                }
            }
        }

        private static void CleanupDisconnectedBones(HashSet<VRRig> activeRigs)
        {
            List<VRRig> rigsToRemove = new List<VRRig>();

            foreach (var kvp in rigLineCache)
            {
                if (!activeRigs.Contains(kvp.Key) || kvp.Key == null)
                {
                    if (kvp.Value != null)
                    {
                        foreach (LineLib.Line line in kvp.Value)
                        {
                            if (line != null)
                            {
                                LineLib.DeleteLine(line);
                            }
                        }
                    }
                    rigsToRemove.Add(kvp.Key);
                }
            }

            foreach (VRRig rig in rigsToRemove)
            {
                rigLineCache.Remove(rig);
            }
        }

        public static void CleanupBoneESP()
        {

            foreach (var kvp in rigLineCache)
            {
                if (kvp.Value != null)
                {
                    foreach (LineLib.Line line in kvp.Value)
                    {
                        if (line != null)
                        {
                            LineLib.DeleteLine(line);
                        }
                    }
                }
            }
            rigLineCache.Clear();
        }

        public static void Tracers()
        {
            if (GorillaParent.instance == null || VRRigCache.ActiveRigs == null)
                return;

            if (GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null)
                return;

            VRRig localRig = GorillaTagger.Instance.offlineVRRig;

            if (localRig.rightHandTransform == null)
                return;

            Vector3 handPosition = localRig.rightHandTransform.position;
            VRRig[] currentRigs = VRRigCache.ActiveRigs.ToArray();
            HashSet<VRRig> activeRigs = new HashSet<VRRig>();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == localRig)
                    continue;

                if (rig.head == null || rig.head.rigTarget == null)
                    continue;

                activeRigs.Add(rig);
                Vector3 headPosition = rig.head.rigTarget.position;

                if (!tracerLineCache.ContainsKey(rig))
                {
                    try
                    {
                        LineLib.Line tracerLine = LineLib.CreateLine(handPosition, headPosition, tracerWidth, Core.BaseColor);
                        tracerLineCache[rig] = tracerLine;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to create tracer line: {e.Message}");
                    }
                }
                else
                {
                    try
                    {
                        LineLib.Line tracerLine = tracerLineCache[rig];
                        if (tracerLine != null)
                        {
                            tracerLine.UpdatePosition(handPosition, headPosition);
                            tracerLine.UpdateColor(Core.BaseColor);
                            tracerLine.SetActive(true);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to update tracer line: {e.Message}");
                    }
                }
            }

            CleanupDisconnectedTracers(activeRigs);
        }

        private static void CleanupDisconnectedTracers(HashSet<VRRig> activeRigs)
        {
            List<VRRig> rigsToRemove = new List<VRRig>();

            foreach (var kvp in tracerLineCache)
            {
                if (!activeRigs.Contains(kvp.Key) || kvp.Key == null)
                {
                    if (kvp.Value != null)
                    {
                        LineLib.DeleteLine(kvp.Value);
                    }
                    rigsToRemove.Add(kvp.Key);
                }
            }

            foreach (VRRig rig in rigsToRemove)
            {
                tracerLineCache.Remove(rig);
            }
        }

        public static void CleanupTracers()
        {
            foreach (var kvp in tracerLineCache)
            {
                if (kvp.Value != null)
                {
                    LineLib.DeleteLine(kvp.Value);
                }
            }
            tracerLineCache.Clear();
        }

        public static void Box2DESP()
        {
            if (GorillaParent.instance == null || VRRigCache.ActiveRigs == null)
                return;

            if (GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null)
                return;

            VRRig localRig = GorillaTagger.Instance.offlineVRRig;
            if (localRig.head == null || localRig.head.rigTarget == null)
                return;

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            VRRig[] currentRigs = VRRigCache.ActiveRigs.ToArray();
            HashSet<VRRig> activeRigs = new HashSet<VRRig>();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == localRig)
                    continue;

                if (rig.head == null || rig.head.rigTarget == null)
                    continue;

                activeRigs.Add(rig);

                if (!box2DLineCache.ContainsKey(rig))
                {
                    Create2DBoxForRig(rig, mainCamera);
                }
                else
                {
                    Update2DBoxForRig(rig, mainCamera);
                }
            }

            CleanupDisconnected2DBoxes(activeRigs);
        }

        private static void Create2DBoxForRig(VRRig rig, Camera cam)
        {
            LineLib.Line[] lines = new LineLib.Line[4];

            Vector3[] corners = Get2DBoxCorners(rig, cam);

            lines[0] = LineLib.CreateLine(corners[0], corners[1], boxWidth, Core.BaseColor);
            lines[1] = LineLib.CreateLine(corners[1], corners[2], boxWidth, Core.BaseColor);
            lines[2] = LineLib.CreateLine(corners[2], corners[3], boxWidth, Core.BaseColor);
            lines[3] = LineLib.CreateLine(corners[3], corners[0], boxWidth, Core.BaseColor);

            box2DLineCache[rig] = lines;
        }

        private static void Update2DBoxForRig(VRRig rig, Camera cam)
        {
            if (!box2DLineCache.ContainsKey(rig))
                return;

            LineLib.Line[] lines = box2DLineCache[rig];
            if (lines == null || lines.Length != 4)
                return;

            Vector3[] corners = Get2DBoxCorners(rig, cam);

            lines[0].UpdatePosition(corners[0], corners[1]);
            lines[1].UpdatePosition(corners[1], corners[2]);
            lines[2].UpdatePosition(corners[2], corners[3]);
            lines[3].UpdatePosition(corners[3], corners[0]);

            foreach (LineLib.Line line in lines)
            {
                line.UpdateColor(Core.BaseColor);
                line.SetActive(true);
            }
        }

        private static Vector3[] Get2DBoxCorners(VRRig rig, Camera cam)
        {
            Vector3 headPos = rig.head.rigTarget.position;
            Vector3 center = headPos;

            float height = 1.2f;
            float width = 0.6f;

            Vector3 camRight = cam.transform.right;
            Vector3 camUp = cam.transform.up;

            Vector3[] corners = new Vector3[4];
            corners[0] = center + camUp * (height / 2) - camRight * (width / 2);
            corners[1] = center + camUp * (height / 2) + camRight * (width / 2);
            corners[2] = center - camUp * (height / 2) + camRight * (width / 2);
            corners[3] = center - camUp * (height / 2) - camRight * (width / 2);

            return corners;
        }

        private static void CleanupDisconnected2DBoxes(HashSet<VRRig> activeRigs)
        {
            List<VRRig> rigsToRemove = new List<VRRig>();

            foreach (var kvp in box2DLineCache)
            {
                if (!activeRigs.Contains(kvp.Key) || kvp.Key == null)
                {
                    if (kvp.Value != null)
                    {
                        foreach (LineLib.Line line in kvp.Value)
                        {
                            if (line != null)
                                LineLib.DeleteLine(line);
                        }
                    }
                    rigsToRemove.Add(kvp.Key);
                }
            }

            foreach (VRRig rig in rigsToRemove)
            {
                box2DLineCache.Remove(rig);
            }
        }

        public static void CleanupBox2DESP()
        {
            foreach (var kvp in box2DLineCache)
            {
                if (kvp.Value != null)
                {
                    foreach (LineLib.Line line in kvp.Value)
                    {
                        if (line != null)
                            LineLib.DeleteLine(line);
                    }
                }
            }
            box2DLineCache.Clear();
        }

        public static void Box3DESP()
        {
            if (GorillaParent.instance == null || VRRigCache.ActiveRigs == null)
                return;

            if (GorillaTagger.Instance == null)
                return;

            VRRig[] currentRigs = VRRigCache.ActiveRigs.ToArray();
            HashSet<VRRig> activeRigs = new HashSet<VRRig>();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == GorillaTagger.Instance.offlineVRRig)
                    continue;

                if (rig.head == null || rig.head.rigTarget == null)
                    continue;

                activeRigs.Add(rig);

                if (!box3DLineCache.ContainsKey(rig))
                {
                    Create3DBoxForRig(rig);
                }
                else
                {
                    Update3DBoxForRig(rig);
                }
            }

            CleanupDisconnected3DBoxes(activeRigs);
        }

        private static void Create3DBoxForRig(VRRig rig)
        {
            LineLib.Line[] lines = new LineLib.Line[12];

            Vector3[] corners = Get3DBoxCorners(rig);

            lines[0] = LineLib.CreateLine(corners[0], corners[1], boxWidth, Core.BaseColor);
            lines[1] = LineLib.CreateLine(corners[1], corners[2], boxWidth, Core.BaseColor);
            lines[2] = LineLib.CreateLine(corners[2], corners[3], boxWidth, Core.BaseColor);
            lines[3] = LineLib.CreateLine(corners[3], corners[0], boxWidth, Core.BaseColor);

            lines[4] = LineLib.CreateLine(corners[4], corners[5], boxWidth, Core.BaseColor);
            lines[5] = LineLib.CreateLine(corners[5], corners[6], boxWidth, Core.BaseColor);
            lines[6] = LineLib.CreateLine(corners[6], corners[7], boxWidth, Core.BaseColor);
            lines[7] = LineLib.CreateLine(corners[7], corners[4], boxWidth, Core.BaseColor);

            lines[8] = LineLib.CreateLine(corners[0], corners[4], boxWidth, Core.BaseColor);
            lines[9] = LineLib.CreateLine(corners[1], corners[5], boxWidth, Core.BaseColor);
            lines[10] = LineLib.CreateLine(corners[2], corners[6], boxWidth, Core.BaseColor);
            lines[11] = LineLib.CreateLine(corners[3], corners[7], boxWidth, Core.BaseColor);

            box3DLineCache[rig] = lines;
        }

        private static void Update3DBoxForRig(VRRig rig)
        {
            if (!box3DLineCache.ContainsKey(rig))
                return;

            LineLib.Line[] lines = box3DLineCache[rig];
            if (lines == null || lines.Length != 12)
                return;

            Vector3[] corners = Get3DBoxCorners(rig);

            lines[0].UpdatePosition(corners[0], corners[1]);
            lines[1].UpdatePosition(corners[1], corners[2]);
            lines[2].UpdatePosition(corners[2], corners[3]);
            lines[3].UpdatePosition(corners[3], corners[0]);

            lines[4].UpdatePosition(corners[4], corners[5]);
            lines[5].UpdatePosition(corners[5], corners[6]);
            lines[6].UpdatePosition(corners[6], corners[7]);
            lines[7].UpdatePosition(corners[7], corners[4]);

            lines[8].UpdatePosition(corners[0], corners[4]);
            lines[9].UpdatePosition(corners[1], corners[5]);
            lines[10].UpdatePosition(corners[2], corners[6]);
            lines[11].UpdatePosition(corners[3], corners[7]);

            foreach (LineLib.Line line in lines)
            {
                line.UpdateColor(Core.BaseColor);
                line.SetActive(true);
            }
        }

        private static Vector3[] Get3DBoxCorners(VRRig rig)
        {
            Vector3 headPos = rig.head.rigTarget.position;
            Vector3 center = headPos;

            Quaternion rotation = rig.head.rigTarget.rotation;

            float height = 1.2f;
            float width = 0.6f;
            float depth = 0.4f;

            Vector3 forward = rotation * Vector3.forward;
            Vector3 right = rotation * Vector3.right;
            Vector3 up = Vector3.up;

            Vector3[] corners = new Vector3[8];

            corners[0] = center + up * (height / 2) - right * (width / 2) + forward * (depth / 2);
            corners[1] = center + up * (height / 2) + right * (width / 2) + forward * (depth / 2);
            corners[2] = center + up * (height / 2) + right * (width / 2) - forward * (depth / 2);
            corners[3] = center + up * (height / 2) - right * (width / 2) - forward * (depth / 2);

            corners[4] = center - up * (height / 2) - right * (width / 2) + forward * (depth / 2);
            corners[5] = center - up * (height / 2) + right * (width / 2) + forward * (depth / 2);
            corners[6] = center - up * (height / 2) + right * (width / 2) - forward * (depth / 2);
            corners[7] = center - up * (height / 2) - right * (width / 2) - forward * (depth / 2);

            return corners;
        }

        private static void CleanupDisconnected3DBoxes(HashSet<VRRig> activeRigs)
        {
            List<VRRig> rigsToRemove = new List<VRRig>();

            foreach (var kvp in box3DLineCache)
            {
                if (!activeRigs.Contains(kvp.Key) || kvp.Key == null)
                {
                    if (kvp.Value != null)
                    {
                        foreach (LineLib.Line line in kvp.Value)
                        {
                            if (line != null)
                                LineLib.DeleteLine(line);
                        }
                    }
                    rigsToRemove.Add(kvp.Key);
                }
            }

            foreach (VRRig rig in rigsToRemove)
            {
                box3DLineCache.Remove(rig);
            }
        }

        public static void CleanupBox3DESP()
        {
            foreach (var kvp in box3DLineCache)
            {
                if (kvp.Value != null)
                {
                    foreach (LineLib.Line line in kvp.Value)
                    {
                        if (line != null)
                            LineLib.DeleteLine(line);
                    }
                }
            }
            box3DLineCache.Clear();
        }

        public static void Chams()
        {
            if (GorillaParent.instance == null || VRRigCache.ActiveRigs == null)
                return;

            if (GorillaTagger.Instance == null)
                return;

            VRRig[] currentRigs = VRRigCache.ActiveRigs.ToArray();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == GorillaTagger.Instance.offlineVRRig)
                    continue;

                if (rig.mainSkin == null)
                    continue;

                if (!originalMaterials.ContainsKey(rig))
                {
                    originalMaterials[rig] = rig.mainSkin.material;
                    originalColors[rig] = rig.mainSkin.material.color;
                }

                Shader chamShader = Shader.Find("GUI/Text Shader");
                if (chamShader != null)
                {
                    rig.mainSkin.material.shader = chamShader;
                    rig.mainSkin.material.color = Core.BaseColor;
                }
            }
        }

        public static void CleanupChams()
        {
            foreach (var kvp in originalMaterials)
            {
                VRRig rig = kvp.Key;
                if (rig != null && rig.mainSkin != null)
                {
                    Renderer rigRenderer = rig.mainSkin.GetComponent<Renderer>();
                    rigRenderer.material.shader = Shader.Find("GorillaTag/UberShader");
                    rigRenderer.material.color = rig.playerColor;
                }
            }
            originalMaterials.Clear();
            originalColors.Clear();
        }

        public static void PlayerNameESP()
        {
            if (GorillaParent.instance == null || VRRigCache.ActiveRigs == null)
                return;

            if (GorillaTagger.Instance == null)
                return;

            VRRig[] currentRigs = VRRigCache.ActiveRigs.ToArray();
            HashSet<VRRig> activeRigs = new HashSet<VRRig>();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == GorillaTagger.Instance.offlineVRRig)
                    continue;

                if (rig.head == null || rig.head.rigTarget == null)
                    continue;

                activeRigs.Add(rig);

                if (!playernamecache.ContainsKey(rig))
                {
                    CreateNameCanvas(rig);
                }
                else
                {
                    UpdateNameCanvas(rig);
                }
            }

            CleanupNameCanvases(activeRigs);
        }

        private static void CreateNameCanvas(VRRig rig)
        {
            GameObject canvasObj = new GameObject($"{rig.gameObject.name}_NameCanvas");
            canvasObj.transform.SetParent(rig.head.rigTarget);
            canvasObj.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            canvasObj.transform.localRotation = Quaternion.identity;

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            GameObject textObj = new GameObject("PlayerNameText");
            textObj.transform.SetParent(canvasObj.transform);
            textObj.transform.localPosition = Vector3.zero;
            textObj.transform.localRotation = Quaternion.identity;
            textObj.transform.localScale = Vector3.one;

            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.font = Core.MenuFont;
            text.fontSize = 24;
            text.color = Core.BaseColor;
            text.alignment = TextAnchor.MiddleCenter;
            text.text = rig.Creator.NickName;
            text.resizeTextForBestFit = false;
            text.fontStyle = FontStyle.Bold;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            Material textMaterial = new Material(Shader.Find("GUI/Text Shader"));
            textMaterial.color = Core.BaseColor;
            text.material = textMaterial;

            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(300f, 100f);

            canvasObj.transform.localScale = Vector3.one * 0.005f;

            playernamecache[rig] = canvas;
        }
        static Dictionary<string, string> datePool = new Dictionary<string, string> { };
        static Dictionary<string, string> AndriodPool = new Dictionary<string, string> { };
        private static string CreationDate(VRRig rig)
        {
            string UserId = rig.Creator.UserId;

            if (datePool.ContainsKey(UserId))
                return datePool[UserId];
            else
            {
                datePool.Add(UserId, "LOADING");
                PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { PlayFabId = UserId }, delegate (GetAccountInfoResult result)
                {
                    string date = result.AccountInfo.Created.ToString("MMM dd, yyyy HH:mm").ToUpper();
                    datePool[UserId] = date;
                    rig.UpdateName();
                }, delegate { datePool[UserId] = "ERROR"; rig.UpdateName(); }, null, null);
                return "LOADING";
            }
        }

        private static void UpdateNameCanvas(VRRig rig)
        {
            if (!playernamecache.ContainsKey(rig))
                return;

            Canvas canvas = playernamecache[rig];

            if (canvas == null)
                return;

            UnityEngine.UI.Text text = canvas.GetComponentInChildren<UnityEngine.UI.Text>();

            if (text == null)
                return;

            if (AdvancedNametags)
            {
                var fps = rig.fps;
                var fpsstring = fps.ToString();
                var creationDate = CreationDate(rig);
                if (fps < 30)
                {
                    fpsstring = $"<color=red>{fps.ToString()}</color>";
                }
                if (fps > 30 && fps < 60)
                {
                    fpsstring = $"<color=yellow>{fps.ToString()}</color>";
                }
                if (fps > 60)
                {
                    fpsstring = $"<color=green>{fps.ToString()}</color>";
                }
                if (fps > 100)
                {
                    fpsstring = $"<color=cyan>{fps.ToString()}</color>";
                }
                text.text = $"USERNAME : {rig.Creator.NickName}\nID : {rig.Creator.UserId}\nPLATFORM : unkown\nFPS : {fpsstring}\nCREATION : {creationDate}";
            }
            else
            {
                text.text = rig.Creator.NickName;
            }

            text.color = Core.BaseColor;
            if (text.material != null)
            {
                text.material.color = Core.BaseColor;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                canvas.transform.LookAt(canvas.transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
            }
        }

        private static void CleanupNameCanvases(HashSet<VRRig> activeRigs)
        {
            List<VRRig> rigsToRemove = new List<VRRig>();

            foreach (var kvp in playernamecache)
            {
                if (!activeRigs.Contains(kvp.Key) || kvp.Key == null)
                {
                    if (kvp.Value != null)
                    {
                        GameObject.Destroy(kvp.Value.gameObject);
                    }
                    rigsToRemove.Add(kvp.Key);
                }
            }

            foreach (VRRig rig in rigsToRemove)
            {
                playernamecache.Remove(rig);
                playernametextcache.Remove(rig);
            }
        }

        public static void CleanupPlayerNameESP()
        {
            foreach (var kvp in playernamecache)
            {
                if (kvp.Value != null)
                {
                    GameObject.Destroy(kvp.Value.gameObject);
                }
            }
            playernamecache.Clear();
            playernametextcache.Clear();
        }
        public static void PlayerInfo()
        {
            if (hudInstance == null)
            {
                hudInstance = new GameObject("Juul_Info");
                if (Camera.main != null)
                {
                    hudInstance.transform.SetParent(Camera.main.transform, false);
                }

                hudInstance.transform.localPosition = new Vector3(0.15f, 0.25f, 1.0f);
                hudInstance.transform.localRotation = Quaternion.identity;
                hudInstance.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

                textMesh = hudInstance.AddComponent<TextMesh>();
                textMesh.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                textMesh.GetComponent<MeshRenderer>().material = textMesh.font.material;

                textMesh.fontSize = 500;
                textMesh.characterSize = 0.5f;

                textMesh.anchor = TextAnchor.UpperLeft;
                textMesh.color = Core.BaseColor;
            }

            float frameFPS = 1.0f / Time.unscaledDeltaTime;
            currentFPS = (int)Mathf.Lerp(currentFPS, frameFPS, Time.unscaledDeltaTime * 10f);

            hudTimer += Time.deltaTime;
            if (hudTimer > 0.1f)
            {
                hudTimer = 0f;

                var p = PhotonNetwork.LocalPlayer;
                string name = p != null ? p.NickName : "Unknown";

                bool inRoom = PhotonNetwork.InRoom;
                bool isMaster = PhotonNetwork.IsMasterClient;
                VRRig localRig = GorillaTagger.Instance.offlineVRRig;
                bool isInfected = localRig != null && Infected(localRig);

                textMesh.text =
                    $"Name: {name}\n" +
                    $"FPS: {currentFPS}\n" +
                    $"In Lobby: {inRoom}\n" +
                    $"Is Master Client: {isMaster}\n" +
                    $"Is Tagged: {isInfected}";

                textMesh.color = Core.BaseColor;
            }
        }

        public static bool Infected(VRRig p)
        {
            return p != null && p.mainSkin != null && p.mainSkin.material != null && (p.mainSkin.material.name.Contains("It") || p.mainSkin.material.name.Contains("fected"));
        }


        public static void MenuThemeRig()
        {
            Renderer rigRenderer = GorillaTagger.Instance.offlineVRRig.mainSkin.GetComponent<Renderer>();
            rigRenderer.material.shader = Shader.Find("GUI/Text Shader");
            Color hollowColor = Core.BaseColor;
            hollowColor.a = 0.3f;
            rigRenderer.material.color = hollowColor;
        }



        public static void RigColorFix()
        {
            Renderer rigRenderer = GorillaTagger.Instance.offlineVRRig.mainSkin.GetComponent<Renderer>();
            rigRenderer.material.shader = Shader.Find("GorillaTag/UberShader");
            rigRenderer.material.color = GorillaTagger.Instance.offlineVRRig.playerColor;
        }

        public static void CleanupPlayerInfo()
        {
            if (hudInstance != null)
            {
                Object.Destroy(hudInstance);
                hudInstance = null;
                textMesh = null;
            }
        }
        public static void OutcastAll()
        {
            Color c = Color.HSVToRGB(Time.time % 1, 1, 1);
            VRRigCache.ActiveRigs.ForEach(v => { if (v && v != VRRig.LocalRig) v.mainSkin.material.color = c; });
        }

        public static void PlayerInfo2()
        {
            if (hudInstance == null)
            {
                hudInstance = new GameObject("Juul_Info");
                if (Camera.main != null)
                {
                    hudInstance.transform.SetParent(Camera.main.transform, false);
                }

                hudInstance.transform.localPosition = new Vector3(0.15f, 0.25f, 1.0f);
                hudInstance.transform.localRotation = Quaternion.identity;
                hudInstance.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);

                textMesh = hudInstance.AddComponent<TextMesh>();
                textMesh.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                textMesh.GetComponent<MeshRenderer>().material = textMesh.font.material;

                textMesh.fontSize = 500;
                textMesh.characterSize = 0.5f;

                textMesh.anchor = TextAnchor.UpperLeft;
                textMesh.color = Core.BaseColor;
            }

        }
        public static void NightTime()
        {
            BetterDayNightManager.instance.SetTimeOfDay(0);
        }
        public static void DayTime()
        {
            BetterDayNightManager.instance.SetTimeOfDay(3);
        }
        public static void Draw()
        {
            Transform hand = ControllerInputPoller.instance.rightGrab ? GorillaTagger.Instance.rightHandTransform : ControllerInputPoller.instance.leftGrab ? GorillaTagger.Instance.leftHandTransform : null;
            if (hand != null)
            {
                var line = new GameObject("Trail").AddComponent<LineRenderer>();
                line.material = new Material(Shader.Find("Sprites/Default"));
                line.startColor = line.endColor = Color.purple;
                line.startWidth = line.endWidth = 0.02f;
                line.positionCount = 2;
                line.SetPositions(new Vector3[] { hand.position, hand.position });
                GameObject.Destroy(line.gameObject, 0.5f);
            }
        }
        public static void InfectionTracers()
        {
            if (GorillaParent.instance == null || VRRigCache.ActiveRigs == null)
                return;

            if (GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null)
                return;

            VRRig localRig = GorillaTagger.Instance.offlineVRRig;

            if (localRig.rightHandTransform == null)
                return;

            Vector3 handPosition = localRig.rightHandTransform.position;
            VRRig[] currentRigs = VRRigCache.ActiveRigs.ToArray();
            HashSet<VRRig> activeRigs = new HashSet<VRRig>();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == localRig)
                    continue;

                if (rig.head == null || rig.head.rigTarget == null)
                    continue;

                activeRigs.Add(rig);
                Vector3 headPosition = rig.head.rigTarget.position;

                Color tracerColor = Infected(rig) ? Color.red : Core.BaseColor;

                if (!tracerLineCache.ContainsKey(rig))
                {
                    try
                    {
                        LineLib.Line tracerLine = LineLib.CreateLine(handPosition, headPosition, tracerWidth, tracerColor);
                        tracerLineCache[rig] = tracerLine;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to create tracer line: {e.Message}");
                    }
                }
                else
                {
                    try
                    {
                        LineLib.Line tracerLine = tracerLineCache[rig];
                        if (tracerLine != null)
                        {
                            tracerLine.UpdatePosition(handPosition, headPosition);
                            tracerLine.UpdateColor(tracerColor);
                            tracerLine.SetActive(true);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to update tracer line: {e.Message}");
                    }
                }
            }

            CleanupDisconnectedInfectionTracers(activeRigs);
        }

        private static void CleanupDisconnectedInfectionTracers(HashSet<VRRig> activeRigs)
        {
            List<VRRig> rigsToRemove = new List<VRRig>();

            foreach (var kvp in tracerLineCache)
            {
                if (!activeRigs.Contains(kvp.Key) || kvp.Key == null)
                {
                    if (kvp.Value != null)
                    {
                        LineLib.DeleteLine(kvp.Value);
                    }
                    rigsToRemove.Add(kvp.Key);
                }
            }

            foreach (VRRig rig in rigsToRemove)
            {
                tracerLineCache.Remove(rig);
            }
        }

        public static void CleanupInfectionTracers()
        {
            foreach (var kvp in tracerLineCache)
            {
                if (kvp.Value != null)
                {
                    LineLib.DeleteLine(kvp.Value);
                }
            }
            tracerLineCache.Clear();
        }

        public static void InfectionChams()
        {
            if (GorillaParent.instance == null || VRRigCache.ActiveRigs == null)
                return;

            if (GorillaTagger.Instance == null)
                return;

            VRRig[] currentRigs = VRRigCache.ActiveRigs.ToArray();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == GorillaTagger.Instance.offlineVRRig)
                    continue;

                if (rig.mainSkin == null)
                    continue;

                if (!originalMaterials.ContainsKey(rig))
                {
                    originalMaterials[rig] = rig.mainSkin.material;
                    originalColors[rig] = rig.mainSkin.material.color;
                }

                Shader chamShader = Shader.Find("GUI/Text Shader");
                if (chamShader != null)
                {
                    rig.mainSkin.material.shader = chamShader;
                    rig.mainSkin.material.color = Infected(rig) ? Color.red : Core.BaseColor;
                }
            }
        }

        public static void CleanupInfectionChams()
        {
            foreach (var kvp in originalMaterials)
            {
                VRRig rig = kvp.Key;
                if (rig != null && rig.mainSkin != null)
                {
                    Renderer rigRenderer = rig.mainSkin.GetComponent<Renderer>();
                    rigRenderer.material.shader = Shader.Find("GorillaTag/UberShader");
                    rigRenderer.material.color = rig.playerColor;
                }
            }
            originalMaterials.Clear();
            originalColors.Clear();
        }
        public static void InfectionBoneESP()
        {
            if (GorillaParent.instance == null || VRRigCache.ActiveRigs == null)
                return;

            if (GorillaTagger.Instance == null)
                return;

            VRRig[] currentRigs = VRRigCache.ActiveRigs.ToArray();
            HashSet<VRRig> activeRigs = new HashSet<VRRig>();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == GorillaTagger.Instance.offlineVRRig)
                    continue;

                if (rig.head == null || rig.head.rigTarget == null)
                    continue;

                if (rig.mainSkin == null || rig.mainSkin.bones == null || rig.mainSkin.bones.Length == 0)
                    continue;

                activeRigs.Add(rig);

                if (!rigLineCache.ContainsKey(rig))
                {
                    CreateInfectionLinesForRig(rig);
                }
                else
                {
                    UpdateInfectionLinesForRig(rig);
                }
            }

            CleanupDisconnectedInfectionBones(activeRigs);
        }

        private static void CreateInfectionLinesForRig(VRRig rig)
        {
            int numBoneConnections = bones.Length / 2;
            LineLib.Line[] lines = new LineLib.Line[numBoneConnections];

            Color boneColor = Infected(rig) ? Color.red : Core.BaseColor;

            for (int i = 0; i < bones.Length; i += 2)
            {
                try
                {
                    int boneIndexA = bones[i];
                    int boneIndexB = bones[i + 1];

                    if (boneIndexA >= rig.mainSkin.bones.Length || boneIndexB >= rig.mainSkin.bones.Length)
                        continue;

                    Transform boneA = rig.mainSkin.bones[boneIndexA];
                    Transform boneB = rig.mainSkin.bones[boneIndexB];

                    if (boneA != null && boneB != null)
                    {
                        LineLib.Line boneLine = LineLib.CreateLine(boneA.position, boneB.position, width, boneColor);
                        lines[i / 2] = boneLine;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to create bone line at index {i}: {e.Message}");
                }
            }

            rigLineCache[rig] = lines;
        }

        private static void UpdateInfectionLinesForRig(VRRig rig)
        {
            if (!rigLineCache.ContainsKey(rig))
                return;

            LineLib.Line[] lines = rigLineCache[rig];

            if (lines == null || lines.Length == 0)
                return;

            Color boneColor = Infected(rig) ? Color.red : Core.BaseColor;

            for (int i = 0; i < bones.Length; i += 2)
            {
                try
                {
                    int boneIndexA = bones[i];
                    int boneIndexB = bones[i + 1];
                    int lineIndex = i / 2;

                    if (boneIndexA >= rig.mainSkin.bones.Length || boneIndexB >= rig.mainSkin.bones.Length)
                        continue;

                    if (lineIndex >= lines.Length || lines[lineIndex] == null)
                        continue;

                    Transform boneA = rig.mainSkin.bones[boneIndexA];
                    Transform boneB = rig.mainSkin.bones[boneIndexB];

                    if (boneA != null && boneB != null)
                    {
                        lines[lineIndex].UpdatePosition(boneA.position, boneB.position);
                        lines[lineIndex].UpdateColor(boneColor);
                        lines[lineIndex].SetActive(true);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to update bone line at index {i}: {e.Message}");
                }
            }
        }

        private static void CleanupDisconnectedInfectionBones(HashSet<VRRig> activeRigs)
        {
            List<VRRig> rigsToRemove = new List<VRRig>();

            foreach (var kvp in rigLineCache)
            {
                if (!activeRigs.Contains(kvp.Key) || kvp.Key == null)
                {
                    if (kvp.Value != null)
                    {
                        foreach (LineLib.Line line in kvp.Value)
                        {
                            if (line != null)
                            {
                                LineLib.DeleteLine(line);
                            }
                        }
                    }
                    rigsToRemove.Add(kvp.Key);
                }
            }

            foreach (VRRig rig in rigsToRemove)
            {
                rigLineCache.Remove(rig);
            }
        }

        public static void CleanupInfectionBoneESP()
        {
            foreach (var kvp in rigLineCache)
            {
                if (kvp.Value != null)
                {
                    foreach (LineLib.Line line in kvp.Value)
                    {
                        if (line != null)
                        {
                            LineLib.DeleteLine(line);
                        }
                    }
                }
            }
            rigLineCache.Clear();
        }
     






    }
}