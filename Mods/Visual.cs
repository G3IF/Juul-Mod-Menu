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
        private static Dictionary<VRRig, LineLib.Line> box2DV2LineCache = new Dictionary<VRRig, LineLib.Line>();
        private static Dictionary<VRRig, LineLib.Line[]> box2DCornerLineCache = new Dictionary<VRRig, LineLib.Line[]>();
        private static Dictionary<VRRig, LineLib.Line[]> box3DLineCache = new Dictionary<VRRig, LineLib.Line[]>();
        private static Dictionary<VRRig, LineLib.Line[]> box3DV2LineCache = new Dictionary<VRRig, LineLib.Line[]>();

        private static Dictionary<VRRig, Canvas> playernamecache = new Dictionary<VRRig, Canvas>();
        private static Dictionary<VRRig, TextMeshPro> playernametextcache = new Dictionary<VRRig, TextMeshPro>();
        private static Dictionary<VRRig, Material> originalMaterials = new Dictionary<VRRig, Material>();
        private static Dictionary<VRRig, Color> originalColors = new Dictionary<VRRig, Color>();

        public static bool AdvancedNametags = false;

        private const float width = 0.015f;
        private const float tracerWidth = 0.01f;
        private const float boxWidth = 0.018f;
        private static GameObject hudInstance;
        private static TextMesh textMesh;
        private static float hudTimer = 0f;
        private static int currentFPS = 0;
        public static void VisualPlayer(VRRig rig, Color color)
        {
            if ( rig == null ) return;

            rig.mainSkin.material.shader = Core.GuiTextShader;
            rig.mainSkin.material.color = color;    
        }
        public static void CleanUpPlayer()
        {
            if (PhotonNetwork.InRoom)
            {
                VRRig player = Rigs.GetVRRigFromPlayer(PhotonNetwork.MasterClient);
                player.mainSkin.material.shader = Core.UberShader;
                player.mainSkin.material.color = player.playerColor;
            }
        }
        public static void BoneESP()
        {
            if (GorillaParent.instance == null || Core.CachedActiveRigs == null || Core.CachedActiveRigs.Length == 0)
                return;

            if (GorillaTagger.Instance == null)
                return;

            VRRig[] currentRigs = Core.CachedActiveRigs;
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
                        float dist = Core.CachedMainCamera != null ? Vector3.Distance(Core.CachedMainCamera.transform.position, boneA.position) : 1f;
                        float scaledWidth = width * Mathf.Clamp(dist * 0.15f, 0.5f, 2.0f);
                        LineLib.Line boneLine = LineLib.CreateLine(boneA.position, boneB.position, scaledWidth, Core.BaseColor);
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
                        float dist = Core.CachedMainCamera != null ? Vector3.Distance(Core.CachedMainCamera.transform.position, boneA.position) : 1f;
                        lines[lineIndex].UpdateWidth(width * Mathf.Clamp(dist * 0.15f, 0.5f, 2.0f));
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
            if (GorillaParent.instance == null || Core.CachedActiveRigs == null || Core.CachedActiveRigs.Length == 0)
                return;

            if (GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null)
                return;

            VRRig localRig = GorillaTagger.Instance.offlineVRRig;

            if (localRig.rightHandTransform == null)
                return;

            Vector3 handPosition = localRig.rightHandTransform.position;
            VRRig[] currentRigs = Core.CachedActiveRigs;
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
                        float dist = Core.CachedMainCamera != null ? Vector3.Distance(Core.CachedMainCamera.transform.position, headPosition) : 1f;
                        float scaledWidth = tracerWidth * Mathf.Clamp(dist * 0.15f, 0.5f, 2.0f);
                        LineLib.Line tracerLine = LineLib.CreateLine(handPosition, headPosition, scaledWidth, Core.BaseColor);
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
                            float dist = Core.CachedMainCamera != null ? Vector3.Distance(Core.CachedMainCamera.transform.position, headPosition) : 1f;
                            tracerLine.UpdateWidth(tracerWidth * Mathf.Clamp(dist * 0.15f, 0.5f, 2.0f));
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
            if (GorillaParent.instance == null || Core.CachedActiveRigs == null || Core.CachedActiveRigs.Length == 0)
                return;

            if (GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null)
                return;

            VRRig localRig = GorillaTagger.Instance.offlineVRRig;
            if (localRig.head == null || localRig.head.rigTarget == null)
                return;

            Camera mainCamera = Core.CachedMainCamera;
            if (mainCamera == null)
                return;

            VRRig[] currentRigs = Core.CachedActiveRigs;
            HashSet<VRRig> activeRigs = new HashSet<VRRig>();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == localRig)
                    continue;

                activeRigs.Add(rig);

                if (rig.head == null || rig.head.rigTarget == null)
                {
                    if (box2DLineCache.ContainsKey(rig))
                    {
                        foreach (var line in box2DLineCache[rig])
                            if (line != null) line.SetActive(false);
                    }
                    continue;
                }

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
            float dist = Vector3.Distance(cam.transform.position, rig.head.rigTarget.position);
            float scaledWidth = boxWidth * Mathf.Clamp(dist * 0.15f, 0.5f, 2.0f);

            lines[0] = LineLib.CreateLine(corners[0], corners[1], scaledWidth, Core.BaseColor);
            lines[1] = LineLib.CreateLine(corners[1], corners[2], scaledWidth, Core.BaseColor);
            lines[2] = LineLib.CreateLine(corners[2], corners[3], scaledWidth, Core.BaseColor);
            lines[3] = LineLib.CreateLine(corners[3], corners[0], scaledWidth, Core.BaseColor);

            box2DLineCache[rig] = lines;
        }

        private static void Update2DBoxForRig(VRRig rig, Camera cam)
        {
            if (!box2DLineCache.ContainsKey(rig))
                return;

            LineLib.Line[] lines = box2DLineCache[rig];
            if (lines == null || lines.Length != 4)
            {
                box2DLineCache.Remove(rig);
                Create2DBoxForRig(rig, cam);
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == null || lines[i].gameObject == null || lines[i].lineRenderer == null)
                {
                    foreach (var l in lines)
                    {
                        if (l != null && l.gameObject != null)
                            LineLib.DeleteLine(l);
                    }
                    box2DLineCache.Remove(rig);
                    Create2DBoxForRig(rig, cam);
                    return;
                }
            }

            Vector3[] corners = Get2DBoxCorners(rig, cam);
            float dist = Vector3.Distance(cam.transform.position, rig.head.rigTarget.position);
            float scaledWidth = boxWidth * Mathf.Clamp(dist * 0.15f, 0.5f, 2.0f);

            lines[0].UpdatePosition(corners[0], corners[1]);
            lines[1].UpdatePosition(corners[1], corners[2]);
            lines[2].UpdatePosition(corners[2], corners[3]);
            lines[3].UpdatePosition(corners[3], corners[0]);

            foreach (LineLib.Line line in lines)
            {
                line.UpdateColor(Core.BaseColor);
                line.UpdateWidth(scaledWidth);
                line.SetActive(true);
            }
        }

        private static Vector3[] Get2DBoxCorners(VRRig rig, Camera cam)
        {
            Vector3 headPos = rig.head.rigTarget.position;
            Vector3 center = headPos;

            float height = 1.2f;
            float width = 0.6f;

            Vector3 camForward = cam.transform.position - center;
            camForward.y = 0;
            camForward.Normalize();
            if (camForward.sqrMagnitude < 0.001f) camForward = Vector3.forward;

            Vector3 camRight = Vector3.Cross(Vector3.up, camForward).normalized;
            Vector3 camUp = Vector3.up;

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
            if (GorillaParent.instance == null || Core.CachedActiveRigs == null || Core.CachedActiveRigs.Length == 0)
                return;

            if (GorillaTagger.Instance == null)
                return;

            VRRig[] currentRigs = Core.CachedActiveRigs;
            HashSet<VRRig> activeRigs = new HashSet<VRRig>();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == GorillaTagger.Instance.offlineVRRig)
                    continue;

                activeRigs.Add(rig);

                if (rig.head == null || rig.head.rigTarget == null)
                {
                    if (box3DLineCache.ContainsKey(rig))
                    {
                        foreach (var line in box3DLineCache[rig])
                            if (line != null) line.SetActive(false);
                    }
                    continue;
                }

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
            float dist = Core.CachedMainCamera != null ? Vector3.Distance(Core.CachedMainCamera.transform.position, rig.head.rigTarget.position) : 1f;
            float scaledWidth = boxWidth * Mathf.Clamp(dist * 0.15f, 0.5f, 2.0f);

            lines[0] = LineLib.CreateLine(corners[0], corners[1], scaledWidth, Core.BaseColor);
            lines[1] = LineLib.CreateLine(corners[1], corners[2], scaledWidth, Core.BaseColor);
            lines[2] = LineLib.CreateLine(corners[2], corners[3], scaledWidth, Core.BaseColor);
            lines[3] = LineLib.CreateLine(corners[3], corners[0], scaledWidth, Core.BaseColor);

            lines[4] = LineLib.CreateLine(corners[4], corners[5], scaledWidth, Core.BaseColor);
            lines[5] = LineLib.CreateLine(corners[5], corners[6], scaledWidth, Core.BaseColor);
            lines[6] = LineLib.CreateLine(corners[6], corners[7], scaledWidth, Core.BaseColor);
            lines[7] = LineLib.CreateLine(corners[7], corners[4], scaledWidth, Core.BaseColor);

            lines[8] = LineLib.CreateLine(corners[0], corners[4], scaledWidth, Core.BaseColor);
            lines[9] = LineLib.CreateLine(corners[1], corners[5], scaledWidth, Core.BaseColor);
            lines[10] = LineLib.CreateLine(corners[2], corners[6], scaledWidth, Core.BaseColor);
            lines[11] = LineLib.CreateLine(corners[3], corners[7], scaledWidth, Core.BaseColor);

            box3DLineCache[rig] = lines;
        }

        private static void Update3DBoxForRig(VRRig rig)
        {
            if (!box3DLineCache.ContainsKey(rig))
                return;

            LineLib.Line[] lines = box3DLineCache[rig];
            if (lines == null || lines.Length != 12)
            {
                box3DLineCache.Remove(rig);
                Create3DBoxForRig(rig);
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == null || lines[i].gameObject == null || lines[i].lineRenderer == null)
                {
                    foreach (var l in lines)
                    {
                        if (l != null && l.gameObject != null)
                            LineLib.DeleteLine(l);
                    }
                    box3DLineCache.Remove(rig);
                    Create3DBoxForRig(rig);
                    return;
                }
            }

            Vector3[] corners = Get3DBoxCorners(rig);
            float dist = Core.CachedMainCamera != null ? Vector3.Distance(Core.CachedMainCamera.transform.position, rig.head.rigTarget.position) : 1f;
            float scaledWidth = boxWidth * Mathf.Clamp(dist * 0.15f, 0.5f, 2.0f);

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
                line.UpdateWidth(scaledWidth);
                line.SetActive(true);
            }
        }

        private static Vector3[] Get3DBoxCorners(VRRig rig)
        {
            Vector3 headPos = rig.head.rigTarget.position;
            Vector3 center = headPos;

            float height = 1.25f;
            float boxHorizWidth = 0.65f;
            float boxDepth = 0.55f;

            Vector3 forward = rig.head.rigTarget.forward;
            forward.y = 0;
            forward.Normalize();
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 up = Vector3.up;

            Vector3[] corners = new Vector3[8];

            corners[0] = center + up * (height / 2) - right * (boxHorizWidth / 2) + forward * (boxDepth / 2);
            corners[1] = center + up * (height / 2) + right * (boxHorizWidth / 2) + forward * (boxDepth / 2);
            corners[2] = center + up * (height / 2) + right * (boxHorizWidth / 2) - forward * (boxDepth / 2);
            corners[3] = center + up * (height / 2) - right * (boxHorizWidth / 2) - forward * (boxDepth / 2);

            corners[4] = center - up * (height / 2) - right * (boxHorizWidth / 2) + forward * (boxDepth / 2);
            corners[5] = center - up * (height / 2) + right * (boxHorizWidth / 2) + forward * (boxDepth / 2);
            corners[6] = center - up * (height / 2) + right * (boxHorizWidth / 2) - forward * (boxDepth / 2);
            corners[7] = center - up * (height / 2) - right * (boxHorizWidth / 2) - forward * (boxDepth / 2);

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
            if (GorillaParent.instance == null || Core.CachedActiveRigs == null || Core.CachedActiveRigs.Length == 0)
                return;

            if (GorillaTagger.Instance == null)
                return;

            VRRig[] currentRigs = Core.CachedActiveRigs;

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

                Shader chamShader = Core.GuiTextShader;
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
                    rigRenderer.material.shader = Core.UberShader;
                    rigRenderer.material.color = rig.playerColor;
                }
            }
            originalMaterials.Clear();
            originalColors.Clear();
        }

        public static void PlayerNameESP()
        {
            if (GorillaParent.instance == null || Core.CachedActiveRigs == null || Core.CachedActiveRigs.Length == 0)
                return;

            if (GorillaTagger.Instance == null)
                return;

            VRRig[] currentRigs = Core.CachedActiveRigs;
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

            Material textMaterial = new Material(Core.GuiTextShader);
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

            UnityEngine.UI.Text text;
            if (!playernametextcache.TryGetValue(rig, out TextMeshPro _))
            {
                text = canvas.GetComponentInChildren<UnityEngine.UI.Text>();
                if (text == null) return;
            }
            else
            {
                text = canvas.GetComponentInChildren<UnityEngine.UI.Text>();
            }

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

            Camera mainCamera = Core.CachedMainCamera;
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
                hudInstance.AddComponent<PlayerInfoBehavior>();
            }
        }

        public static bool Infected(VRRig p)
        {
            return p != null && p.mainSkin != null && p.mainSkin.material != null && (p.mainSkin.material.name.Contains("It") || p.mainSkin.material.name.Contains("fected"));
        }

        public static void MenuThemeRig()
        {
            Renderer rigRenderer = GorillaTagger.Instance.offlineVRRig.mainSkin.GetComponent<Renderer>();
            rigRenderer.material.shader = Core.GuiTextShader;
            Color hollowColor = Core.BaseColor;
            hollowColor.a = 0.3f;
            rigRenderer.material.color = hollowColor;
        }

        public static void RigColorFix()
        {
            Renderer rigRenderer = GorillaTagger.Instance.offlineVRRig.mainSkin.GetComponent<Renderer>();
            rigRenderer.material.shader = Core.UberShader;
            rigRenderer.material.color = GorillaTagger.Instance.offlineVRRig.playerColor;
        }

        public static void CleanupPlayerInfo()
        {
            if (hudInstance != null)
            {
                UnityEngine.Object.Destroy(hudInstance);
                hudInstance = null;
            }
        }

        public class PlayerInfoBehavior : MonoBehaviour
        {
            private TextMesh textMesh;
            private GUIStyle pcStyle;
            private int fps;
            private float timer;
            private string infoText = "";

            private void Start()
            {
                textMesh = gameObject.AddComponent<TextMesh>();
                textMesh.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                textMesh.GetComponent<MeshRenderer>().material = textMesh.font.material;
                textMesh.fontSize = 500;
                textMesh.characterSize = 0.5f;
                textMesh.anchor = TextAnchor.UpperLeft;

                if (Camera.main != null)
                {
                    transform.SetParent(Camera.main.transform, false);
                }
                transform.localPosition = new Vector3(0.15f, 0.25f, 1.0f);
                transform.localRotation = Quaternion.identity;
                transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);
            }

            private void Update()
            {
                bool isVR = UnityEngine.XR.XRSettings.isDeviceActive;
                textMesh.GetComponent<MeshRenderer>().enabled = isVR;

                float frameFPS = 1.0f / Time.unscaledDeltaTime;
                fps = (int)Mathf.Lerp(fps, frameFPS, Time.unscaledDeltaTime * 10f);

                timer += Time.deltaTime;
                if (timer > 0.1f)
                {
                    timer = 0f;
                    var p = PhotonNetwork.LocalPlayer;
                    string name = p != null ? p.NickName : "Unknown";
                    string roomPlayers = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount.ToString() : "0";
                    bool inRoom = PhotonNetwork.InRoom;
                    bool isMaster = PhotonNetwork.IsMasterClient;
                    VRRig localRig = GorillaTagger.Instance.offlineVRRig;
                    bool isInfected = localRig != null && Infected(localRig);

                    infoText = 
                        $"Name: {name}\n" +
                        $"FPS: {fps}\n" +
                        $"In Lobby: {inRoom}\n" +
                        $"Room Players: {roomPlayers}\n" +
                        $"Is Master Client: {isMaster}\n" +
                        $"Is Tagged: {isInfected}";

                    textMesh.text = infoText;
                    textMesh.color = Core.BaseColor;
                }
            }

            private void OnGUI()
            {
                if (Event.current.type != EventType.Repaint) return;
                if (UnityEngine.XR.XRSettings.isDeviceActive) return;

                if (pcStyle == null)
                {
                    pcStyle = new GUIStyle(GUI.skin.label);
                    pcStyle.fontSize = 20;
                    pcStyle.fontStyle = FontStyle.Normal;
                    pcStyle.alignment = TextAnchor.UpperLeft;
                }

                float yOffset = 8f;
                float xOffset = 8f;

                pcStyle.normal.textColor = Color.black;
                GUI.Label(new Rect(xOffset + 1f, yOffset + 1f, 400f, 200f), infoText, pcStyle);
                pcStyle.normal.textColor = Core.BaseColor;
                GUI.Label(new Rect(xOffset, yOffset, 400f, 200f), infoText, pcStyle);
            }
        }

        private static GameObject arrayListInstance;

        public static void EnableArrayList()
        {
            if (arrayListInstance == null)
            {
                arrayListInstance = new GameObject("ArrayList");
                arrayListInstance.AddComponent<ArrayListBehavior>();
            }
        }

        public static void DisableArrayList()
        {
            if (arrayListInstance != null)
            {
                Object.Destroy(arrayListInstance);
                arrayListInstance = null;
            }
        }


        public class ArrayListBehavior : MonoBehaviour
        {
            private GUIStyle textStyle;
            private Texture2D bgTexture;
            private Texture2D lineTexture;
            private List<Button> enabledMods = new List<Button>();
            private const float margin = 8f;
            private const float gap = 3f;

            private void OnGUI()
            {
                if (Event.current.type != EventType.Repaint) return;
                if (textStyle == null)
                {
                    textStyle = new GUIStyle(GUI.skin.label);
                    textStyle.fontSize = 20;
                    textStyle.fontStyle = FontStyle.Normal;
                    textStyle.alignment = TextAnchor.MiddleRight;
                }
                if (bgTexture == null)
                {
                    bgTexture = new Texture2D(1, 1);
                    bgTexture.SetPixel(0, 0, new Color(0.05f, 0.05f, 0.05f, 0.85f));
                    bgTexture.Apply();
                }
                if (lineTexture == null)
                {
                    lineTexture = new Texture2D(1, 1);
                    lineTexture.SetPixel(0, 0, Color.white);
                    lineTexture.Apply();
                }
                if (Buttons.Modules == null) return;
                enabledMods.Clear();
                foreach (var category in Buttons.Modules)
                {
                    if (category == Buttons.EnabledCategory) continue;
                    if (category == PlayerMenu.GetPlayersCategory()) continue;
                    foreach (var btn in category.Buttons)
                    {
                        if (btn != null && btn.Enabled && btn.Toggle && !btn.Incremental && !btn.Label)
                        {
                            if (btn.Name.Contains("Array List") || btn.Name.Contains("Watermark")) continue;
                            enabledMods.Add(btn);
                        }
                    }
                }
                enabledMods.Sort((a, b) => b.Name.Length.CompareTo(a.Name.Length));
                float screenWidth = Screen.width;
                float yOffset = margin;
                for (int i = 0; i < enabledMods.Count; i++)
                {
                    string text = enabledMods[i].Name;
                    Vector2 size = textStyle.CalcSize(new GUIContent(text));
                    float rectWidth = size.x + 14f;
                    float rectHeight = size.y + 2f;
                    float x = screenWidth - rectWidth - margin;
                    float normalizedPos = (enabledMods.Count > 1) ? (float)i / (enabledMods.Count - 1) : 0f;
                    Color modColor = GetSyncedGradientColor(normalizedPos);
                    GUI.DrawTexture(new Rect(x, yOffset, rectWidth, rectHeight), bgTexture);
                    Color oldColor = GUI.color;
                    GUI.color = modColor;
                    GUI.DrawTexture(new Rect(screenWidth - 3f - margin, yOffset, 3f, rectHeight), lineTexture);
                    GUI.color = oldColor;
                    textStyle.normal.textColor = Color.black;
                    GUI.Label(new Rect(x, yOffset + 2f, rectWidth - 8f, rectHeight), text, textStyle);
                    textStyle.normal.textColor = modColor;
                    GUI.Label(new Rect(x - 1f, yOffset + 1f, rectWidth - 8f, rectHeight), text, textStyle);
                    yOffset += rectHeight + gap;
                }
            }


            private Color GetSyncedGradientColor(float normalizedPosition)
            {
                if (Themes.List == null || Core.ThemeValue < 0 || Core.ThemeValue >= Themes.List.Length)
                    return Core.BaseColor;
                Theme currentTheme = Themes.List[Core.ThemeValue];
                if (currentTheme.Colors == null || currentTheme.Colors.Length == 0) return Core.BaseColor;
                if (currentTheme.Colors.Length == 1) return currentTheme.Colors[0];
                float totalRange = currentTheme.Colors.Length - 1;
                float baseT = Mathf.PingPong(Time.time * currentTheme.Speed, totalRange);
                float t = Mathf.PingPong(baseT + normalizedPosition * totalRange * 0.15f, totalRange);
                int indexA = Mathf.FloorToInt(t);
                int indexB = Mathf.Clamp(indexA + 1, 0, currentTheme.Colors.Length - 1);
                float localT = t - indexA;
                float easedT = localT < 0.5f ? 2f * localT * localT : 1f - Mathf.Pow(-2f * localT + 2f, 2f) / 2f;
                return Color.Lerp(currentTheme.Colors[indexA], currentTheme.Colors[indexB], easedT);
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
            if (GorillaParent.instance == null || Core.CachedActiveRigs == null || Core.CachedActiveRigs.Length == 0)
                return;

            if (GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null)
                return;

            VRRig localRig = GorillaTagger.Instance.offlineVRRig;

            if (localRig.rightHandTransform == null)
                return;

            Vector3 handPosition = localRig.rightHandTransform.position;
            VRRig[] currentRigs = Core.CachedActiveRigs;
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
            if (GorillaParent.instance == null || Core.CachedActiveRigs == null || Core.CachedActiveRigs.Length == 0)
                return;

            if (GorillaTagger.Instance == null)
                return;

            VRRig[] currentRigs = Core.CachedActiveRigs;

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

                Shader chamShader = Core.GuiTextShader;
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
                    rigRenderer.material.shader = Core.UberShader;
                    rigRenderer.material.color = rig.playerColor;
                }
            }
            originalMaterials.Clear();
            originalColors.Clear();
        }
        public static void InfectionBoneESP()
        {
            if (GorillaParent.instance == null || Core.CachedActiveRigs == null || Core.CachedActiveRigs.Length == 0)
                return;

            if (GorillaTagger.Instance == null)
                return;

            VRRig[] currentRigs = Core.CachedActiveRigs;
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
     
        

        

        public static void Box2DCornerESP()
        {
            if (GorillaParent.instance == null || Core.CachedActiveRigs == null || Core.CachedActiveRigs.Length == 0) return;
            if (GorillaTagger.Instance == null || GorillaTagger.Instance.offlineVRRig == null) return;

            VRRig localRig = GorillaTagger.Instance.offlineVRRig;
            Camera mainCamera = Core.CachedMainCamera;
            if (mainCamera == null) return;

            VRRig[] currentRigs = Core.CachedActiveRigs;
            HashSet<VRRig> activeRigs = new HashSet<VRRig>();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == localRig) continue;
                activeRigs.Add(rig);

                if (rig.head == null || rig.head.rigTarget == null)
                {
                    if (box2DCornerLineCache.ContainsKey(rig))
                        foreach (var l in box2DCornerLineCache[rig]) if (l != null) l.SetActive(false);
                    continue;
                }

                if (!box2DCornerLineCache.ContainsKey(rig))
                    Create2DCornerBoxForRig(rig, mainCamera);
                else
                    Update2DCornerBoxForRig(rig, mainCamera);
            }
            CleanupDisconnected2DCornerBoxes(activeRigs);
        }

        private static Vector3[][] Get2DCornerLines(VRRig rig, Camera cam)
        {
            Vector3[] c = Get2DBoxCorners(rig, cam);
            float hLen = (c[1] - c[0]).magnitude / 4f; 
            float vLen = (c[0] - c[3]).magnitude / 4f; 

            Vector3 right = (c[1] - c[0]).normalized;
            Vector3 down = (c[3] - c[0]).normalized;
            Vector3 up = -down;

            Vector3[][] lines = new Vector3[8][];
            int idx = 0;

            lines[idx++] = new Vector3[] { c[0], c[0] + right * hLen };
            lines[idx++] = new Vector3[] { c[0], c[0] + down * vLen };

            lines[idx++] = new Vector3[] { c[1], c[1] - right * hLen };
            lines[idx++] = new Vector3[] { c[1], c[1] + down * vLen };

            lines[idx++] = new Vector3[] { c[2], c[2] - right * hLen };
            lines[idx++] = new Vector3[] { c[2], c[2] + up * vLen };

            lines[idx++] = new Vector3[] { c[3], c[3] + right * hLen };
            lines[idx++] = new Vector3[] { c[3], c[3] + up * vLen };

            return lines;
        }

        private static void Create2DCornerBoxForRig(VRRig rig, Camera cam)
        {
            Vector3[][] pointPairs = Get2DCornerLines(rig, cam);
            LineLib.Line[] lines = new LineLib.Line[8];
            float dist = Vector3.Distance(cam.transform.position, rig.head.rigTarget.position);
            float scaledWidth = boxWidth * Mathf.Clamp(dist * 0.15f, 0.5f, 2.0f);

            for (int i = 0; i < 8; i++)
                lines[i] = LineLib.CreateLine(pointPairs[i][0], pointPairs[i][1], scaledWidth, Core.BaseColor);

            box2DCornerLineCache[rig] = lines;
        }

        private static void Update2DCornerBoxForRig(VRRig rig, Camera cam)
        {
            if (!box2DCornerLineCache.ContainsKey(rig)) return;
            LineLib.Line[] lines = box2DCornerLineCache[rig];
            if (lines == null || lines.Length != 8)
            {
                box2DCornerLineCache.Remove(rig);
                Create2DCornerBoxForRig(rig, cam);
                return;
            }

            for (int i = 0; i < 8; i++)
            {
                if (lines[i] == null || lines[i].gameObject == null || lines[i].lineRenderer == null)
                {
                    foreach (var l in lines) if (l != null && l.gameObject != null) LineLib.DeleteLine(l);
                    box2DCornerLineCache.Remove(rig);
                    Create2DCornerBoxForRig(rig, cam);
                    return;
                }
            }

            Vector3[][] pointPairs = Get2DCornerLines(rig, cam);
            float dist = Vector3.Distance(cam.transform.position, rig.head.rigTarget.position);
            float scaledWidth = boxWidth * Mathf.Clamp(dist * 0.15f, 0.5f, 2.0f);

            for (int i = 0; i < 8; i++)
            {
                lines[i].UpdatePosition(pointPairs[i][0], pointPairs[i][1]);
                lines[i].UpdateWidth(scaledWidth);
                lines[i].UpdateColor(Core.BaseColor);
                lines[i].SetActive(true);
            }
        }

        private static void CleanupDisconnected2DCornerBoxes(HashSet<VRRig> activeRigs)
        {
            List<VRRig> rigsToRemove = new List<VRRig>();
            foreach (var kvp in box2DCornerLineCache)
            {
                if (!activeRigs.Contains(kvp.Key) || kvp.Key == null)
                {
                    if (kvp.Value != null)
                        foreach (var l in kvp.Value) if (l != null) LineLib.DeleteLine(l);
                    rigsToRemove.Add(kvp.Key);
                }
            }
            foreach (var rig in rigsToRemove) box2DCornerLineCache.Remove(rig);
        }

        public static void CleanupBox2DCornerESP()
        {
            foreach (var kvp in box2DCornerLineCache)
                if (kvp.Value != null)
                    foreach (var l in kvp.Value) if (l != null) LineLib.DeleteLine(l);
            box2DCornerLineCache.Clear();
        }

        public static void Box3DESPV2()
        {
            if (GorillaParent.instance == null || Core.CachedActiveRigs == null || Core.CachedActiveRigs.Length == 0) return;
            if (GorillaTagger.Instance == null) return;

            VRRig[] currentRigs = Core.CachedActiveRigs;
            HashSet<VRRig> activeRigs = new HashSet<VRRig>();

            foreach (VRRig rig in currentRigs)
            {
                if (rig == null || rig == GorillaTagger.Instance.offlineVRRig) continue;
                activeRigs.Add(rig);

                if (rig.head == null || rig.head.rigTarget == null)
                {
                    if (box3DV2LineCache.ContainsKey(rig))
                    {
                        foreach (var l in box3DV2LineCache[rig]) if (l != null) l.SetActive(false);
                    }
                    continue;
                }

                if (!box3DV2LineCache.ContainsKey(rig))
                    Create3DV2BoxForRig(rig);
                else
                    Update3DV2BoxForRig(rig);
            }
            CleanupDisconnected3DV2Boxes(activeRigs);
        }

        private static Vector3[][] GetCornerBoxLines(VRRig rig)
        {
            Vector3 headPos = rig.head.rigTarget.position;
            Vector3 center = headPos;

            float h = 1.25f;
            float w = 0.65f;
            float d = 0.55f;
            
            float lineLenH = h / 4f;
            float lineLenW = w / 4f;
            float lineLenD = d / 4f;

            Vector3 forward = rig.head.rigTarget.forward;
            forward.y = 0;
            forward.Normalize();
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 up = Vector3.up;

            Vector3[] c = new Vector3[8];
            c[0] = center + up * (h/2) - right * (w/2) + forward * (d/2);
            c[1] = center + up * (h/2) + right * (w/2) + forward * (d/2);
            c[2] = center + up * (h/2) + right * (w/2) - forward * (d/2);
            c[3] = center + up * (h/2) - right * (w/2) - forward * (d/2);
            c[4] = center - up * (h/2) - right * (w/2) + forward * (d/2);
            c[5] = center - up * (h/2) + right * (w/2) + forward * (d/2);
            c[6] = center - up * (h/2) + right * (w/2) - forward * (d/2);
            c[7] = center - up * (h/2) - right * (w/2) - forward * (d/2);

            Vector3[][] lines = new Vector3[24][];
            int idx = 0;
            
            lines[idx++] = new Vector3[] { c[0], c[0] + right * lineLenW };
            lines[idx++] = new Vector3[] { c[0], c[0] - forward * lineLenD };
            lines[idx++] = new Vector3[] { c[0], c[0] - up * lineLenH };

            lines[idx++] = new Vector3[] { c[1], c[1] - right * lineLenW };
            lines[idx++] = new Vector3[] { c[1], c[1] - forward * lineLenD };
            lines[idx++] = new Vector3[] { c[1], c[1] - up * lineLenH };

            lines[idx++] = new Vector3[] { c[2], c[2] + forward * lineLenD };
            lines[idx++] = new Vector3[] { c[2], c[2] - right * lineLenW };
            lines[idx++] = new Vector3[] { c[2], c[2] - up * lineLenH };

            lines[idx++] = new Vector3[] { c[3], c[3] + right * lineLenW };
            lines[idx++] = new Vector3[] { c[3], c[3] + forward * lineLenD };
            lines[idx++] = new Vector3[] { c[3], c[3] - up * lineLenH };

            lines[idx++] = new Vector3[] { c[4], c[4] + right * lineLenW };
            lines[idx++] = new Vector3[] { c[4], c[4] - forward * lineLenD };
            lines[idx++] = new Vector3[] { c[4], c[4] + up * lineLenH };
            
            lines[idx++] = new Vector3[] { c[5], c[5] - right * lineLenW };
            lines[idx++] = new Vector3[] { c[5], c[5] - forward * lineLenD };
            lines[idx++] = new Vector3[] { c[5], c[5] + up * lineLenH };
            
            lines[idx++] = new Vector3[] { c[6], c[6] + forward * lineLenD };
            lines[idx++] = new Vector3[] { c[6], c[6] - right * lineLenW };
            lines[idx++] = new Vector3[] { c[6], c[6] + up * lineLenH };
            
            lines[idx++] = new Vector3[] { c[7], c[7] + right * lineLenW };
            lines[idx++] = new Vector3[] { c[7], c[7] + forward * lineLenD };
            lines[idx++] = new Vector3[] { c[7], c[7] + up * lineLenH };

            return lines;
        }

        private static void Create3DV2BoxForRig(VRRig rig)
        {
            Vector3[][] pointPairs = GetCornerBoxLines(rig);
            LineLib.Line[] lines = new LineLib.Line[24];
            
            float dist = Core.CachedMainCamera != null ? Vector3.Distance(Core.CachedMainCamera.transform.position, rig.head.rigTarget.position) : 1f;
            float scaledWidth = boxWidth * Mathf.Clamp(dist * 0.15f, 0.5f, 2.0f);

            for (int i = 0; i < 24; i++)
                lines[i] = LineLib.CreateLine(pointPairs[i][0], pointPairs[i][1], scaledWidth, Core.BaseColor);
                
            box3DV2LineCache[rig] = lines;
        }

        private static void Update3DV2BoxForRig(VRRig rig)
        {
            if (!box3DV2LineCache.ContainsKey(rig)) return;
            LineLib.Line[] lines = box3DV2LineCache[rig];
            if (lines == null || lines.Length != 24)
            {
                box3DV2LineCache.Remove(rig);
                Create3DV2BoxForRig(rig);
                return;
            }

            for (int i = 0; i < 24; i++)
            {
                if (lines[i] == null || lines[i].gameObject == null || lines[i].lineRenderer == null)
                {
                    foreach (var l in lines) if (l != null && l.gameObject != null) LineLib.DeleteLine(l);
                    box3DV2LineCache.Remove(rig);
                    Create3DV2BoxForRig(rig);
                    return;
                }
            }

            Vector3[][] pointPairs = GetCornerBoxLines(rig);
            float dist = Core.CachedMainCamera != null ? Vector3.Distance(Core.CachedMainCamera.transform.position, rig.head.rigTarget.position) : 1f;
            float scaledWidth = boxWidth * Mathf.Clamp(dist * 0.15f, 0.5f, 2.0f);

            for (int i = 0; i < 24; i++)
            {
                lines[i].UpdatePosition(pointPairs[i][0], pointPairs[i][1]);
                lines[i].UpdateWidth(scaledWidth);
                lines[i].UpdateColor(Core.BaseColor);
                lines[i].SetActive(true);
            }
        }

        private static void CleanupDisconnected3DV2Boxes(HashSet<VRRig> activeRigs)
        {
            List<VRRig> rigsToRemove = new List<VRRig>();
            foreach (var kvp in box3DV2LineCache)
            {
                if (!activeRigs.Contains(kvp.Key) || kvp.Key == null)
                {
                    if (kvp.Value != null)
                        foreach (var l in kvp.Value) if (l != null) LineLib.DeleteLine(l);
                    rigsToRemove.Add(kvp.Key);
                }
            }
            foreach (var rig in rigsToRemove) box3DV2LineCache.Remove(rig);
        }

        public static void CleanupBox3DESPV2()
        {
            foreach (var kvp in box3DV2LineCache)
                if (kvp.Value != null)
                    foreach (var l in kvp.Value) if (l != null) LineLib.DeleteLine(l);
            box3DV2LineCache.Clear();
        }


    }
}
