using BepInEx;
using ExitGames.Client.Photon;
using g3;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTagScripts;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.XR;
using Valve.Newtonsoft.Json;
using Valve.VR;
using CommonUsages = UnityEngine.XR.CommonUsages;
using JoinType = GorillaNetworking.JoinType;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Juul
{
    [HarmonyPatch(typeof(GTPlayer))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    public partial class Core : MonoBehaviour
    {
        public static GameObject Menu = null;
        public static GameObject Canvas = null;
        public static GameObject Pointer = null;
        public static GameObject Frame = null;
        public static GameObject Sidebar = null;
        public static float SmFl = 0.0035f;
        public static Category ActiveCategory = null;
        public static int BtnIndex = 0;
        public static int CatIndex = 0;
        public static int MaxButtons = 8;
        public static int CurrentPage = 0;
        public static int PageBtnVer = 2;
        public static float ButtonCooldown = 0f;
        public static float IncrementCooldown = 0f;
        public static bool IsOutlined = false;
        public static bool IsRounded = true; 
        public static bool IsCatLeft = true;
        public static bool IsCatRotated = true;
        public static bool MenuStart = false;
        public static bool IsMenuOpen = false;
        public static Font Arial = Font.CreateDynamicFontFromOSFont("Arial", 14);
        public static Font Verdana = Font.CreateDynamicFontFromOSFont("Verdana", 14);
        public static Font SFPro = Font.CreateDynamicFontFromOSFont("SF Pro", 14);
        public static Font Consolas = Font.CreateDynamicFontFromOSFont("Consolas", 14);
        public static Font MenuFont = Verdana;
        public static float OffBrightness = 0.5f;
        public static float OnBrightness = 0.33f;
        public static int ThemeValue = 0;
        public static Color BaseColor = Color.red;

        public static Color GetCurrentThemeColor()
        {
            return Themes.List[ThemeValue].Color;
        }

        public static string GetCurrentThemeName()
        {
            return Themes.List[ThemeValue].Name;
        }

        public static void ChangeTheme(bool forward)
        {
            if (forward && ThemeValue >= (Themes.List.Length - 1)) ThemeValue = 0;
            else if (!forward && ThemeValue <= 0) ThemeValue = (Themes.List.Length - 1);
            else ThemeValue = ThemeValue + (forward ? 1 : -1);
            if (Buttons.ThemeButton != null)
                Buttons.ThemeButton.Name = $"Theme: {GetCurrentThemeName()}";
        }

        public static void SetTheme(int value)
        {
            ThemeValue = value;
            if (Buttons.ThemeButton != null)
                Buttons.ThemeButton.Name = $"Theme: {GetCurrentThemeName()}";
            RebuildMenu();
        }

        public static string Folder
        {
            get
            {
                return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Juul");
            }
        }

        public static void ChangePageButtons(bool forward)
        {
            PageBtnVer = PageBtnVer + (forward ? 1 : -1);
            if (PageBtnVer < 0) PageBtnVer = 3;
            if (PageBtnVer > 3) PageBtnVer = 0;
            RebuildMenu();
        }

        public static Color CycleColors(Color[] colors, float cycleSpeed)
        {
            if (colors == null || colors.Length == 0)
                return Color.white;
            if (colors.Length == 1)
                return colors[0];
            float totalRange = colors.Length - 1;
            float t = Mathf.PingPong(Time.time * cycleSpeed, totalRange);
            int indexA = Mathf.FloorToInt(t);
            int indexB = Mathf.Clamp(indexA + 1, 0, colors.Length - 1);
            float localT = t - indexA;
            float easedT = localT < 0.5f
                ? 2f * localT * localT
                : 1f - Mathf.Pow(-2f * localT + 2f, 2f) / 2f;
            return Color.Lerp(colors[indexA], colors[indexB], easedT);
        }

        public static void OutlineGradient(GameObject toOutline)
        {
            if (IsOutlined && toOutline != null && Menu != null)
            {
                GameObject outline = GameObject.CreatePrimitive(PrimitiveType.Cube);
                outline.transform.parent = Menu.transform;
                outline.transform.rotation = Quaternion.identity;
                outline.transform.localScale = toOutline.transform.localScale - new Vector3(SmFl / 2f, 0f, 0f);
                outline.transform.localPosition = toOutline.transform.localPosition;
                outline.transform.rotation = toOutline.transform.rotation;
                GameObject.Destroy(outline.GetComponent<Rigidbody>());
                GameObject.Destroy(outline.GetComponent<BoxCollider>());
                GradientSetter cs1 = outline.AddComponent<GradientSetter>();
                var sourceGradient = toOutline.GetComponent<GradientSetter>();
                if (sourceGradient != null)
                {
                    cs1.brightness = sourceGradient.brightness - 0.3f;
                    cs1.gradientOffset = sourceGradient.gradientOffset;
                }
                if (IsRounded)
                {
                    var sourceCorners = toOutline.GetComponent<RoundedCorners>();
                    if (sourceCorners != null)
                    {
                        RoundedCorners corners = outline.AddComponent<RoundedCorners>();
                        corners.bevel = sourceCorners.bevel;
                    }
                }
                toOutline.transform.localScale = toOutline.transform.localScale - new Vector3(0f, 0.01f, 0.01f);
            }
        }

        public static Vector3 ServerPos;
        public static Vector3 ServerLeftHandPos;
        public static Vector3 ServerRightHandPos;
        public static Vector3 ServerSyncPos;
        public static Vector3 ServerSyncLeftHandPos;
        public static Vector3 ServerSyncRightHandPos;

        public static void OnSerialize()
        {
            ServerSyncPos = VRRig.LocalRig?.transform.position ?? ServerSyncPos;
            ServerSyncLeftHandPos = VRRig.LocalRig?.leftHand?.rigTarget?.transform.position ?? ServerSyncLeftHandPos;
            ServerSyncRightHandPos = VRRig.LocalRig?.rightHand?.rigTarget?.transform.position ?? ServerSyncRightHandPos;
        }

        public static bool inroomrel = false;

        private static readonly Dictionary<string, GameObject> objectPool = new Dictionary<string, GameObject>();
        private const int MAX_POOL_SIZE = 100;
        private static float poolCleanupTimer = 0f;

        public static GameObject GetObject(string find)
        {
            if (objectPool.TryGetValue(find, out GameObject go))
            {
                if (go != null)
                    return go;
                else
                    objectPool.Remove(find);
            }

            GameObject tgo = GameObject.Find(find);
            if (tgo != null && objectPool.Count < MAX_POOL_SIZE)
                objectPool.Add(find, tgo);

            return tgo;
        }

        private static void CleanupObjectPool()
        {
            poolCleanupTimer += Time.deltaTime;
            if (poolCleanupTimer > 60f)
            {
                poolCleanupTimer = 0f;
                List<string> keysToRemove = new List<string>();
                foreach (var kvp in objectPool)
                {
                    if (kvp.Value == null)
                        keysToRemove.Add(kvp.Key);
                }
                for (int i = 0; i < keysToRemove.Count; i++)
                {
                    objectPool.Remove(keysToRemove[i]);
                }
            }
        }

        public static Camera TPC;
        private static RaycastHit[] raycastHits = new RaycastHit[10];
        private static int uiLayerMask = 1 << 2;

        private static int? noInvisLayerMask;
        public static int NoInvisLayerMask()
        {
            noInvisLayerMask ??= ~(
                1 << LayerMask.NameToLayer("TransparentFX") |
                1 << LayerMask.NameToLayer("Ignore Raycast") |
                1 << LayerMask.NameToLayer("Zone") |
                1 << LayerMask.NameToLayer("Gorilla Trigger") |
                1 << LayerMask.NameToLayer("Gorilla Boundary") |
                1 << LayerMask.NameToLayer("GorillaCosmetics") |
                1 << LayerMask.NameToLayer("GorillaParticle"));

            return noInvisLayerMask ?? GTPlayer.Instance.locomotionEnabledLayers;
        }

        public static void Prefix()
        {
            CleanupObjectPool();

            try
            {
                if (TPC == null)
                {
                    try
                    {
                        TPC = GetObject("Player Objects/Third Person Camera/Shoulder Camera").GetComponent<Camera>();
                    }
                    catch
                    {
                        TPC = GetObject("Shoulder Camera").GetComponent<Camera>();
                    }
                }
            }
            catch { }

            if (VRRig.LocalRig == null) return;
            ServerPos = ServerPos == Vector3.zero ? ServerSyncPos : Vector3.Lerp(ServerPos, VRRig.LocalRig.SanitizeVector3(ServerSyncPos), VRRig.LocalRig.lerpValueBody * 0.66f);
            ServerLeftHandPos = ServerLeftHandPos == Vector3.zero ? ServerSyncLeftHandPos : Vector3.Lerp(ServerLeftHandPos, VRRig.LocalRig.SanitizeVector3(ServerSyncLeftHandPos), VRRig.LocalRig.lerpValueBody);
            ServerRightHandPos = ServerRightHandPos == Vector3.zero ? ServerSyncRightHandPos : Vector3.Lerp(ServerRightHandPos, VRRig.LocalRig.SanitizeVector3(ServerSyncRightHandPos), VRRig.LocalRig.lerpValueBody);

            if (PhotonNetwork.InRoom && !inroomrel)
            {
                inroomrel = true;
            }

            if (!MenuStart)
            {
                MenuStart = true;
                Buttons.Initialize();
                Configs.LoadConfig();
            }

            if (ActiveCategory == null && Buttons.Modules != null && Buttons.Modules.Length > 0)
            {
                ActiveCategory = Buttons.Modules[0];
            }

            if (Themes.List != null)
            {
                for (int i = 0; i < Themes.List.Length; i++)
                {
                    Themes.List[i].Color = CycleColors(Themes.List[i].Colors, Themes.List[i].Speed);
                }
            }

            BaseColor = Color.Lerp(BaseColor, GetCurrentThemeColor(), Time.deltaTime * 5.5f);

            bool tabPressed = false;
            try
            {
                tabPressed = UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.qKey.isPressed;
            }
            catch { }

            bool shouldOpenMenu = Inputs.LeftSecondary || tabPressed;

            if (shouldOpenMenu)
            {
                if (Menu == null)
                {
                    CreateFrame();
                    Audios.Play("https://files.catbox.moe/tt0vjs.mp3", 0.35f);
                    Menu.AddComponent<ScaleInAnimation>();
                    IsMenuOpen = true;

                    if (Pointer == null)
                    {
                        Pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        Pointer.transform.parent = GorillaTagger.Instance.rightHandTransform;
                        Pointer.GetComponent<Renderer>().material.color = Color.white;
                        Pointer.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                        Pointer.transform.localScale = Vector3.one * 0.0075f;
                        Pointer.layer = 2;
                    }
                }

                if (Menu != null)
                {
                    if (tabPressed && TPC != null)
                    {
                        Menu.transform.position = TPC.transform.position + TPC.transform.forward * 0.6f;
                        Menu.transform.rotation = Quaternion.LookRotation(TPC.transform.position - Menu.transform.position) * Quaternion.Euler(-90f, 0f, -90f);

                        if (UnityEngine.InputSystem.Mouse.current != null)
                        {
                            Ray ray = TPC.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());

                            if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
                            {
                                int hitCount = Physics.RaycastNonAlloc(ray, raycastHits, 512f, uiLayerMask);

                                if (hitCount > 0)
                                {
                                    System.Array.Sort(raycastHits, 0, hitCount, Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance)));

                                    for (int i = 0; i < hitCount; i++)
                                    {
                                        IncrementalButtonCollider incrementalCollider = raycastHits[i].collider.GetComponent<IncrementalButtonCollider>();
                                        if (incrementalCollider != null)
                                        {
                                            if (Time.time > IncrementCooldown)
                                            {
                                                IncrementCooldown = Time.time + 0.15f;
                                                incrementalCollider.onClick?.Invoke();
                                                Audios.Play("https://files.catbox.moe/5hoxyg.mp3");
                                                RebuildMenu();
                                            }
                                            break;
                                        }

                                        ButtonCollider buttonCollider = raycastHits[i].collider.GetComponent<ButtonCollider>();
                                        if (buttonCollider != null)
                                        {
                                            if (Time.time > ButtonCooldown)
                                            {
                                                ButtonCooldown = Time.time + 0.2345f;
                                                buttonCollider.onClick?.Invoke();
                                                Audios.Play("https://files.catbox.moe/5hoxyg.mp3");
                                                RebuildMenu();
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Menu.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                        Menu.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                    }
                }
            }
            else
            {
                if (Menu != null)
                {
                    var scaleInAnimation = Menu.AddComponent<ScaleInAnimation>();
                    scaleInAnimation.reverse = true;
                    GameObject.Destroy(Pointer);
                    Pointer = null;
                    IsMenuOpen = false;
                }
            }

            if (Buttons.Modules != null)
            {
                for (int i = 0; i < Buttons.Modules.Length; i++)
                {
                    Category category = Buttons.Modules[i];
                    for (int j = 0; j < category.Buttons.Count; j++)
                    {
                        Button button = category.Buttons[j];
                        if (button.Enabled)
                            button.OnEnable();
                        else
                            button.OnDisable();
                    }
                }
            }
        }

        public static void RebuildMenu()
        {
            if (Menu != null)
            {
                CleanupMenu();
                GameObject.Destroy(Menu);
                Menu = null;
            }
            CreateFrame();
        }

        private static void CleanupMenu()
        {
            if (Menu == null) return;

            GradientSetter[] gradients = Menu.GetComponentsInChildren<GradientSetter>();
            foreach (var gradient in gradients)
            {
                if (gradient != null)
                    GameObject.Destroy(gradient);
            }

            ColorSetter[] colors = Menu.GetComponentsInChildren<ColorSetter>();
            foreach (var color in colors)
            {
                if (color != null)
                    GameObject.Destroy(color);
            }

            Renderer[] renderers = Menu.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.material != null)
                    GameObject.Destroy(renderer.material);
            }
        }

        public static void AddText(string Text, float Size, Vector3 Position, Vector3 Rotation = default(Vector3), bool Bold = false)
        {
            if (Canvas == null) return;

            GameObject gameObject = new GameObject()
            {
                transform =
                {
                    parent = Canvas.transform
                }
            };
            Text text = gameObject.AddComponent<Text>();
            text.font = MenuFont;
            text.fontSize = 1;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 0;
            text.text = Text;
            text.color = Color.white;
            text.fontStyle = Bold ? FontStyle.Bold : FontStyle.Normal;
            text.material.renderQueue = 4000;
            RectTransform component = text.GetComponent<RectTransform>();
            component.localPosition = Vector3.zero;
            component.sizeDelta = new Vector2(0.24f, 0.035f * Size);
            component.localPosition = Position;
            component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f) + Rotation);
        }

        public static float MenuWidth = 0.8f;
        public static float BtnInset = 0.1f;
        public static float BtnUpset = 0.3f;
        public static float BtnHeight = 0.07f;
        public static float BtnSpace = 0.005f;
        public static float TextSize = 0.5f;
        public static float GradVal = 0.066f;

        public static void ChangeMenuScale(bool forward)
        {
            if (forward && MenuWidth >= 2f) MenuWidth = 0.45f;
            if (!forward && MenuWidth <= 0.45f) MenuWidth = 2f;
            MenuWidth = MenuWidth + (forward ? 0.025f : -0.025f);
            RebuildMenu();
        }

        public static void ChangeButtonInset(bool forward)
        {
            if (forward && BtnInset >= 0.5f) BtnInset = 0f;
            if (!forward && BtnInset <= 0f) BtnInset = 0.4f;
            BtnInset = BtnInset + (forward ? 0.025f : -0.025f);
            RebuildMenu();
        }

        public static void ChangeTextSize(bool forward)
        {
            if (forward && TextSize >= 1.1f) TextSize = 0.3f;
            if (!forward && TextSize <= 0.3f) TextSize = 0.9f;
            TextSize = TextSize + (forward ? 0.025f : -0.025f);
            RebuildMenu();
        }

        public static void CreateFrame()
        {
            Menu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Menu.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            GameObject.Destroy(Menu.GetComponent<Rigidbody>());
            GameObject.Destroy(Menu.GetComponent<Collider>());
            GameObject.Destroy(Menu.GetComponent<Renderer>());

            Canvas = new GameObject();
            Canvas.transform.parent = Menu.transform;
            Canvas canvas = Canvas.AddComponent<Canvas>();
            CanvasScaler canvasScaler = Canvas.AddComponent<CanvasScaler>();
            Canvas.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasScaler.dynamicPixelsPerUnit = 10000f;

            Frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Frame.name = "Menu Frame";
            Frame.transform.parent = Menu.transform;
            Frame.transform.rotation = Quaternion.identity;
            Frame.transform.localScale = new Vector3(SmFl, MenuWidth, 0.9f);
            Frame.transform.localPosition = new Vector3(SmFl * 40f, 0f, 0f);
            GameObject.Destroy(Frame.GetComponent<Rigidbody>());
            GameObject.Destroy(Frame.GetComponent<BoxCollider>());
            GradientSetter frameGradient = Frame.AddComponent<GradientSetter>();
            if (IsRounded) Frame.AddComponent<RoundedCorners>();
            OutlineGradient(Frame);

            Sidebar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Sidebar.transform.parent = Menu.transform;
            Sidebar.transform.localScale = new Vector3(SmFl, 0.45f, 0.9f);
            Sidebar.transform.localPosition = new Vector3(SmFl * 40f + (IsCatRotated ? Sidebar.transform.localScale.y / 2f : 0f), (IsCatLeft ? -((Frame.transform.localScale.y / 2f) + (Sidebar.transform.localScale.y / 2f)) : ((Frame.transform.localScale.y / 2f) + (Sidebar.transform.localScale.y / 2f))) + (IsCatRotated ? 0f : (IsCatLeft ? -(SmFl * 20f) : (SmFl * 20f))), 0f);
            Sidebar.transform.localRotation = Quaternion.Euler(0f, 0f, IsCatRotated ? (IsCatLeft ? 45f : (-45f)) : 0f);
            GameObject.Destroy(Sidebar.GetComponent<Rigidbody>());
            GameObject.Destroy(Sidebar.GetComponent<BoxCollider>());
            GradientSetter sidebarGradient = Sidebar.AddComponent<GradientSetter>();
            if (IsRounded) Sidebar.AddComponent<RoundedCorners>();
            OutlineGradient(Sidebar);

            GameObject disconnectButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
            disconnectButton.name = "Disconnect Button";
            disconnectButton.layer = 2;
            disconnectButton.transform.parent = Menu.transform;
            disconnectButton.transform.localScale = new Vector3(SmFl, MenuWidth, 0.075f);
            disconnectButton.transform.localPosition = new Vector3(SmFl * 40f, 0f, 0.5f);
            GameObject.Destroy(disconnectButton.GetComponent<Rigidbody>());
            GradientSetter disconnectColor = disconnectButton.AddComponent<GradientSetter>();
            disconnectColor.gradientOffset = 0f;
            BoxCollider disconnectComponent = disconnectButton.GetComponent<BoxCollider>();
            disconnectComponent.isTrigger = true;
            ButtonCollider disButtonCollider = disconnectButton.AddComponent<ButtonCollider>();
            disButtonCollider.onClick = () => PhotonNetwork.Disconnect();
            if (IsRounded)
            {
                RoundedCorners disconnectCorners = disconnectButton.AddComponent<RoundedCorners>();
                disconnectCorners.bevel = disconnectCorners.bevel / 2f;
            }
            OutlineGradient(disconnectButton);

            AddText("Disconnect", TextSize, disconnectButton.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));
            AddText(Plugin.title, 1f, Frame.transform.position + new Vector3(SmFl, 0f, 0.1625f), default(Vector3), true);

            BtnIndex = 0;
            CatIndex = 0;

            if (Buttons.Modules != null)
            {
                foreach (Category category in Buttons.Modules)
                {
                    AddCategory(category.Name);
                }
            }

            if (ActiveCategory == null && Buttons.Modules != null && Buttons.Modules.Length > 0)
            {
                ActiveCategory = Buttons.Modules[0];
            }

            if (PageBtnVer == 2)
            {
                AddCustomButton("<<<<<<", () => PreviousPage());
                AddCustomButton(">>>>>>", () => NextPage());
            }

            if (PageBtnVer == 3)
            {
                BtnIndex++;
            }

            if (ActiveCategory != null)
            {
                RefreshButtons();
            }

            if (PageBtnVer == 0 || PageBtnVer == 3)
            {
                GameObject prevObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                prevObj.layer = 2;
                prevObj.transform.parent = Menu.transform;
                prevObj.transform.rotation = Quaternion.identity;
                prevObj.transform.localScale = new Vector3(SmFl, ((MenuWidth - BtnInset) / 2f) - 0.005f, BtnHeight);
                prevObj.transform.localPosition = new Vector3(SmFl * 40f + SmFl, (((MenuWidth - BtnInset) / 2f) + 0.005f) / 2f, BtnUpset - (PageBtnVer == 3 ? 0f : ((BtnHeight + BtnSpace) * (MaxButtons + 1))));
                GameObject.Destroy(prevObj.GetComponent<Rigidbody>());
                BoxCollider boxColP = prevObj.GetComponent<BoxCollider>();
                boxColP.isTrigger = true;
                ColorSetter cs1 = prevObj.AddComponent<ColorSetter>();
                cs1.brightness = OffBrightness;
                cs1.colorOffset = -GradVal * 2;
                ButtonCollider buttonCollider = prevObj.AddComponent<ButtonCollider>();
                buttonCollider.onClick = PreviousPage;
                AddText("<", TextSize, prevObj.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));

                GameObject nextObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                nextObj.layer = 2;
                nextObj.transform.parent = Menu.transform;
                nextObj.transform.rotation = Quaternion.identity;
                nextObj.transform.localScale = new Vector3(SmFl, ((MenuWidth - BtnInset) / 2f) - 0.005f, BtnHeight);
                nextObj.transform.localPosition = new Vector3(SmFl * 40f + SmFl, -((((MenuWidth - BtnInset) / 2f) + 0.005f) / 2f), BtnUpset - (PageBtnVer == 3 ? 0f : ((BtnHeight + BtnSpace) * (MaxButtons + 1))));
                GameObject.Destroy(nextObj.GetComponent<Rigidbody>());
                BoxCollider boxColN = nextObj.GetComponent<BoxCollider>();
                boxColN.isTrigger = true;
                ColorSetter cs2 = nextObj.AddComponent<ColorSetter>();
                cs2.brightness = OffBrightness;
                cs2.colorOffset = -GradVal * 2;
                ButtonCollider buttonCollider2 = nextObj.AddComponent<ButtonCollider>();
                buttonCollider2.onClick = NextPage;
                AddText(">", TextSize, nextObj.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));

                if (PageBtnVer == 3) BtnIndex++;
            }
            else if (PageBtnVer == 1)
            {
                BtnIndex = MaxButtons;
                AddCustomButton("<<<<<<", PreviousPage);
                AddCustomButton(">>>>>>", NextPage);
            }
        }

        public static int BtnCount()
        {
            if (PageBtnVer == 1 || PageBtnVer == 2) return MaxButtons;
            else return MaxButtons + 1;
        }

        public static void RefreshButtons()
        {
            if (ActiveCategory == null) return;

            int totalItems = ActiveCategory.Subcategories.Count + ActiveCategory.Buttons.Count;
            int buttonLimit = BtnCount();
            int startIndex = CurrentPage * buttonLimit;
            int endIndex = Mathf.Min(startIndex + buttonLimit, totalItems);

            for (int i = startIndex; i < endIndex; i++)
            {
                if (i < ActiveCategory.Subcategories.Count)
                {
                    Category sub = ActiveCategory.Subcategories[i];
                    AddCustomButton(sub.Name, () => {
                        sub.ParentCategory = ActiveCategory;
                        ActiveCategory = sub;
                        CurrentPage = 0;
                        RebuildMenu();
                    });
                }
                else
                {
                    int buttonIndex = i - ActiveCategory.Subcategories.Count;
                    AddButton(ActiveCategory.Buttons[buttonIndex].Name);
                }
            }
        }

        public static void NextPage()
        {
            if (ActiveCategory == null) return;
            int totalItems = ActiveCategory.Subcategories.Count + ActiveCategory.Buttons.Count;
            int num = Mathf.CeilToInt((float)totalItems / (float)BtnCount());
            if (CurrentPage < num - 1)
            {
                CurrentPage++;
                RebuildMenu();
            }
            else
            {
                CurrentPage = 0;
                RebuildMenu();
            }
        }

        public static void PreviousPage()
        {
            if (ActiveCategory == null) return;

            if (CurrentPage > 0)
            {
                CurrentPage--;
                RebuildMenu();
            }
            else if (ActiveCategory.ParentCategory != null)
            {
                ActiveCategory = ActiveCategory.ParentCategory;
                CurrentPage = 0;
                RebuildMenu();
            }
            else
            {
                int totalItems = ActiveCategory.Subcategories.Count + ActiveCategory.Buttons.Count;
                int num = Mathf.Max(1, Mathf.CeilToInt((float)totalItems / (float)BtnCount()));
                CurrentPage = num - 1;
                RebuildMenu();
            }
        }

        public static Button GetButtonFromCategory(string Category, string Button)
        {
            if (Buttons.Modules == null) return null;

            for (int i = 0; i < Buttons.Modules.Length; i++)
            {
                if (Buttons.Modules[i].Name == Category || Buttons.Modules[i].Name.Contains(Category))
                {
                    List<Button> buttons = Buttons.Modules[i].Buttons;
                    for (int j = 0; j < buttons.Count; j++)
                    {
                        if (buttons[j].Name == Button || buttons[j].Name.Contains(Button))
                        {
                            return buttons[j];
                        }
                    }
                }
            }
            return null;
        }

        public static Button GetButtonByName(string Name)
        {
            if (Buttons.Modules == null) return null;

            for (int i = 0; i < Buttons.Modules.Length; i++)
            {
                List<Button> buttons = Buttons.Modules[i].Buttons;
                for (int j = 0; j < buttons.Count; j++)
                {
                    if (buttons[j].Name == Name || buttons[j].Name.Contains(Name))
                    {
                        return buttons[j];
                    }
                }
            }
            return null;
        }

        public static void AddCategory(string name)
        {
            if (Menu == null) return;

            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.layer = 2;
            gameObject.transform.parent = Menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(SmFl, 0.4f, BtnHeight);
            gameObject.transform.localPosition = new Vector3(SmFl * 40f + (IsCatRotated ? Sidebar.transform.localScale.y / 2f : 0f) + SmFl, (IsCatLeft ? -((Frame.transform.localScale.y / 2f) + (Sidebar.transform.localScale.y / 2f)) : ((Frame.transform.localScale.y / 2f) + (Sidebar.transform.localScale.y / 2f))) + (IsCatRotated ? 0f : (IsCatLeft ? -(SmFl * 20f) : (SmFl * 20f))), 0.36f - ((BtnHeight + BtnSpace) * CatIndex));
            gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, IsCatRotated ? (IsCatLeft ? 45f : (-45f)) : 0f);
            GameObject.Destroy(gameObject.GetComponent<Rigidbody>());
            BoxCollider component = gameObject.GetComponent<BoxCollider>();
            component.isTrigger = true;

            Category category = null;
            if (Buttons.Modules != null)
            {
                for (int i = 0; i < Buttons.Modules.Length; i++)
                {
                    if (Buttons.Modules[i].Name == name)
                    {
                        category = Buttons.Modules[i];
                        break;
                    }
                }
            }

            float brightness = ((ActiveCategory == category) ? OnBrightness : OffBrightness);
            ColorSetter cs1 = gameObject.AddComponent<ColorSetter>();
            cs1.brightness = brightness;
            cs1.colorOffset = (CatIndex * GradVal) - GradVal;

            string categoryName = name;
            ButtonCollider buttonCollider = gameObject.AddComponent<ButtonCollider>();
            buttonCollider.onClick = () =>
            {
                CurrentPage = 0;
                if (Buttons.Modules != null)
                {
                    for (int i = 0; i < Buttons.Modules.Length; i++)
                    {
                        if (Buttons.Modules[i].Name == categoryName)
                        {
                            ActiveCategory = Buttons.Modules[i];
                            break;
                        }
                    }
                }
                RebuildMenu();
            };
            AddText(name, TextSize, gameObject.transform.position + new Vector3(SmFl, 0f, SmFl), new Vector3(IsCatRotated ? (IsCatLeft ? (-45f) : 45f) : 0f, 0f, 0f));
            CatIndex++;
        }

        public static void AddCustomButton(string name, Action callback)
        {
            if (Menu == null) return;

            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.layer = 2;
            gameObject.transform.parent = Menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(SmFl, (MenuWidth - BtnInset), BtnHeight);
            gameObject.transform.localPosition = new Vector3(SmFl * 40f + SmFl, 0f, BtnUpset - ((BtnHeight + BtnSpace) * (float)BtnIndex));
            GameObject.Destroy(gameObject.GetComponent<Rigidbody>());
            BoxCollider component = gameObject.GetComponent<BoxCollider>();
            component.isTrigger = true;
            ColorSetter cs1 = gameObject.AddComponent<ColorSetter>();
            cs1.brightness = OffBrightness;
            cs1.colorOffset = (BtnIndex * GradVal) - GradVal;
            ButtonCollider buttonCollider = gameObject.AddComponent<ButtonCollider>();
            buttonCollider.onClick = callback;
            AddText(name, TextSize, gameObject.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));
            BtnIndex++;
        }

        public static void AddButton(string name)
        {
            if (ActiveCategory == null || Menu == null) return;

            Button button = null;
            for (int i = 0; i < ActiveCategory.Buttons.Count; i++)
            {
                if (ActiveCategory.Buttons[i].Name == name)
                {
                    button = ActiveCategory.Buttons[i];
                    break;
                }
            }
            if (button == null) return;

            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.layer = 2;
            gameObject.transform.parent = Menu.transform;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(SmFl, (MenuWidth - BtnInset) - (button.Incremental ? 0.175f : 0f), BtnHeight);
            gameObject.transform.localPosition = new Vector3(SmFl * 40f + SmFl, 0f, BtnUpset - ((BtnHeight + BtnSpace) * (float)BtnIndex));
            GameObject.Destroy(gameObject.GetComponent<Rigidbody>());
            BoxCollider component = gameObject.GetComponent<BoxCollider>();
            component.isTrigger = true;
            float brightness = (button.Enabled ? OnBrightness : OffBrightness);
            ColorSetter cs1 = gameObject.AddComponent<ColorSetter>();
            cs1.brightness = brightness;
            cs1.colorOffset = (BtnIndex * GradVal) - GradVal;

            if (!button.Label)
            {
                if (button.Incremental)
                {
                    GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gameObject2.layer = 2;
                    gameObject2.transform.parent = Menu.transform;
                    gameObject2.transform.rotation = Quaternion.identity;
                    gameObject2.transform.localScale = new Vector3(SmFl, 0.08f, BtnHeight);
                    gameObject2.transform.localPosition = new Vector3(SmFl * 40f + SmFl, ((MenuWidth - BtnInset) / 2f) - (gameObject2.transform.localScale.y / 2f), BtnUpset - ((BtnHeight + BtnSpace) * (float)BtnIndex));
                    ColorSetter cs2 = gameObject2.AddComponent<ColorSetter>();
                    cs2.brightness = OffBrightness;
                    cs2.colorOffset = (BtnIndex * GradVal) - GradVal;
                    GameObject.Destroy(gameObject2.GetComponent<Rigidbody>());

                    GameObject gameObject3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gameObject3.layer = 2;
                    gameObject3.transform.parent = Menu.transform;
                    gameObject3.transform.rotation = Quaternion.identity;
                    gameObject3.transform.localScale = new Vector3(SmFl, 0.08f, BtnHeight);
                    gameObject3.transform.localPosition = new Vector3(SmFl * 40f + SmFl, -(((MenuWidth - BtnInset) / 2f) - (gameObject2.transform.localScale.y / 2f)), BtnUpset - ((BtnHeight + BtnSpace) * (float)BtnIndex));
                    ColorSetter cs3 = gameObject3.AddComponent<ColorSetter>();
                    cs3.brightness = OffBrightness;
                    cs3.colorOffset = (BtnIndex * GradVal) - GradVal;
                    GameObject.Destroy(gameObject3.GetComponent<Rigidbody>());

                    AddText("-", TextSize, gameObject2.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));
                    AddText("+", TextSize, gameObject3.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));

                    BoxCollider component2 = gameObject2.GetComponent<BoxCollider>();
                    component2.isTrigger = true;
                    BoxCollider component3 = gameObject3.GetComponent<BoxCollider>();
                    component3.isTrigger = true;

                    IncrementalButtonCollider downCol = gameObject2.AddComponent<IncrementalButtonCollider>();
                    downCol.onClick = () =>
                    {
                        button.Down();
                    };
                    IncrementalButtonCollider upCol = gameObject3.AddComponent<IncrementalButtonCollider>();
                    upCol.onClick = () =>
                    {
                        button.Up();
                    };
                }

                var DefaultCallback = () => { };
                if (button.OnEnable != DefaultCallback || button.OnDisable != DefaultCallback)
                {
                    ButtonCollider buttonCollider = gameObject.AddComponent<ButtonCollider>();
                    buttonCollider.onClick = () =>
                    {
                        if (button.Toggle)
                        {
                            button.Enabled = !button.Enabled;
                            if (button.Enabled)
                            {
                                button.OnceEnable();
                                button.OnEnable();
                            }
                            else
                            {
                                button.OnceDisable();
                                button.OnDisable();
                            }
                        }
                        else
                        {
                            button.OnEnable();
                        }
                        RebuildMenu();
                    };
                }
            }
            else
            {
                GameObject.Destroy(gameObject.GetComponent<Renderer>());
            }

            AddText(name, TextSize, gameObject.transform.position + new Vector3(SmFl, 0f, SmFl), default(Vector3));
            BtnIndex++;
        }

        public class ScaleInAnimation : MonoBehaviour
        {
            [Header("Settings")]
            [SerializeField] public bool reverse = false;
            [SerializeField] public float duration = 0.4f;
            [SerializeField] public System.Action onComplete;
            private Vector3 startScale;
            private Vector3 targetScale;
            private float elapsed;
            private bool initialized;
            private void Awake()
            {
                Initialize();
            }
            private void Initialize()
            {
                if (initialized) return;
                if (!reverse)
                {
                    targetScale = transform.localScale;
                    startScale = Vector3.zero;
                    transform.localScale = startScale;
                }
                else
                {
                    startScale = transform.localScale;
                    targetScale = Vector3.zero;
                }
                elapsed = 0f;
                initialized = true;
            }
            private void Update()
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = EaseInOutCubic(t);
                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, easedT);
                if (t >= 1f)
                {
                    transform.localScale = targetScale;
                    onComplete?.Invoke();
                    if (reverse)
                        Destroy(gameObject);
                    else
                        Destroy(this);
                }
            }
            private float EaseInOutCubic(float t)
            {
                return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
            }
        }

        public static Vector3 ParentScale(Vector3 baseVector)
        {
            return new Vector3(
                (baseVector.x * Menu.transform.localScale.x) * (1f / Menu.transform.localScale.x),
                (baseVector.y * Menu.transform.localScale.y) * (1f / Menu.transform.localScale.y),
                (baseVector.z * Menu.transform.localScale.z) * (1f / Menu.transform.localScale.z)
            );
        }

        public class RoundedCorners : MonoBehaviour
        {
            [Range(0f, 0.5f)] public float bevel = 0.04f;
            public bool topLeft = true;
            public bool topRight = true;
            public bool bottomLeft = true;
            public bool bottomRight = true;
            public float multX = 0f;
            public float multY = 0f;
            public float bevelX = 0f;
            public float bevelY = 0f;
            private Renderer sourceRenderer;
            private GradientSetter gradientSetter;
            private ColorSetter colorSetter;
            void Start()
            {
                sourceRenderer = GetComponent<Renderer>();
                if (!sourceRenderer) return;
                gradientSetter = GetComponent<GradientSetter>();
                colorSetter = GetComponent<ColorSetter>();
                float sx = Mathf.Max(transform.localScale.y, 0.001f);
                float sy = Mathf.Max(transform.localScale.z, 0.001f);
                multX = (1f / sx) * (1f + Mathf.Log(sx + 1f));
                multY = (1f / sy) * (1f + Mathf.Log(sy + 1f));
                bevelX = bevel * multX;
                bevelY = bevel * multY;
                CreateGeometry();
                sourceRenderer.enabled = false;
            }
            void CreateGeometry()
            {
                Transform parent = transform;
                CreateCube(parent, Vector3.zero, new Vector3(1f, 1f - bevelX * 2f, 1f), false, -1);
                CreateCube(parent, Vector3.zero, new Vector3(1f, 1f, 1f - bevelY * 2f), false, -1);
                bool[] enabled = { topLeft, bottomLeft, topRight, bottomRight };
                Vector3[] offsets =
                {
                    new Vector3(0f, -0.5f + bevelX, -0.5f + bevelY),
                    new Vector3(0f, 0.5f - bevelX, -0.5f + bevelY),
                    new Vector3(0f, -0.5f + bevelX, 0.5f - bevelY),
                    new Vector3(0f, 0.5f - bevelX, 0.5f - bevelY)
                };
                for (int i = 0; i < 4; i++)
                {
                    bool isTop = (i == 2 || i == 3);
                    if (enabled[i])
                    {
                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        Destroy(c.GetComponent<Collider>());
                        c.transform.SetParent(parent, false);
                        c.transform.localRotation = Quaternion.Euler(0, 0, 90);
                        c.transform.localScale = new Vector3(bevelX * 2f, 0.5f, bevelY * 2f);
                        c.transform.localPosition = offsets[i];
                        ConfigureRenderer(c.GetComponent<Renderer>(), true, isTop ? 0 : 1);
                    }
                    else
                    {
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Destroy(cube.GetComponent<Collider>());
                        cube.transform.SetParent(parent, false);
                        cube.transform.localScale = new Vector3(1f, bevelX * 2f, bevelY * 2f);
                        cube.transform.localPosition = offsets[i];
                        ConfigureRenderer(cube.GetComponent<Renderer>(), true, isTop ? 0 : 1);
                    }
                }
            }
            void CreateCube(Transform parent, Vector3 pos, Vector3 scale, bool isCorner, int cornerType)
            {
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(g.GetComponent<Collider>());
                g.transform.SetParent(parent, false);
                g.transform.localPosition = pos;
                g.transform.localScale = scale;
                ConfigureRenderer(g.GetComponent<Renderer>(), isCorner, cornerType);
            }
            void ConfigureRenderer(Renderer r, bool isCorner, int cornerType)
            {
                Material oldMaterial = r.material;
                if (oldMaterial != null)
                    Destroy(oldMaterial);

                if (gradientSetter != null)
                {
                    GradientSetter gs = r.gameObject.AddComponent<GradientSetter>();
                    gs.brightness = gradientSetter.brightness;
                    gs.isVertical = gradientSetter.isVertical;
                    if (isCorner)
                    {
                        float bevelOffset = bevel * gradientSetter.gradientOffset;
                        if (cornerType == 0)
                        {
                            gs.startOffset = gradientSetter.startOffset;
                            gs.gradientOffset = bevelOffset;
                        }
                        else
                        {
                            gs.startOffset = gradientSetter.startOffset + gradientSetter.gradientOffset - bevelOffset;
                            gs.gradientOffset = bevelOffset;
                        }
                    }
                    else
                    {
                        gs.gradientOffset = gradientSetter.gradientOffset;
                        gs.startOffset = gradientSetter.startOffset;
                    }
                }
                else if (colorSetter != null)
                {
                    ColorSetter cs = r.gameObject.AddComponent<ColorSetter>();
                    cs.brightness = colorSetter.brightness;
                    cs.colorOffset = colorSetter.colorOffset;
                }
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;
                r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            }
        }

        public class GradientSetter : MonoBehaviour
        {
            [Header("Color Settings")]
            [SerializeField, Range(0f, 2f)] public float brightness = 1f;
            [SerializeField] public bool isVertical = false;
            [SerializeField, Range(0f, 10f)] public float gradientOffset = 1f;
            [SerializeField, Range(0f, 10f)] public float startOffset = 0f;
            private Renderer rend;
            private Material cachedMaterial;
            private Texture2D gradientTexture;
            private Color[] pixels;
            private const int width = 64;
            private const int height = 64;
            private Color lastColor1;
            private Color lastColor2;
            private bool needsUpdate = true;
            private float updateTimer = 0f;
            private const float updateInterval = 0.033f;
            private bool initialized = false;
            private bool isCylinder = false;
            private void Start()
            {
                rend = GetComponent<Renderer>();
                if (rend == null) return;
                isCylinder = GetComponent<MeshFilter>()?.sharedMesh.name.Contains("Cylinder") ?? false;
                cachedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                rend.material = cachedMaterial;
                CreateGradientTexture();
                initialized = true;
                lastColor1 = GetOffsetColor(startOffset) * brightness;
                lastColor2 = GetOffsetColor(startOffset + gradientOffset) * brightness;
                UpdateGradientTexture();
            }
            private void Update()
            {
                if (!initialized || !isActiveAndEnabled) return;
                updateTimer += Time.deltaTime;
                if (updateTimer >= updateInterval)
                {
                    updateTimer = 0f;
                    Color color1 = GetOffsetColor(startOffset) * brightness;
                    Color color2 = GetOffsetColor(startOffset + gradientOffset) * brightness;
                    if (Vector4.Distance(lastColor1, color1) > 0.02f || Vector4.Distance(lastColor2, color2) > 0.02f)
                    {
                        lastColor1 = color1;
                        lastColor2 = color2;
                        needsUpdate = true;
                    }
                    if (needsUpdate)
                    {
                        UpdateGradientTexture();
                        needsUpdate = false;
                    }
                }
            }
            private void CreateGradientTexture()
            {
                gradientTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
                gradientTexture.filterMode = FilterMode.Bilinear;
                gradientTexture.wrapMode = isCylinder ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                pixels = new Color[width * height];
                cachedMaterial.color = Color.white;
                cachedMaterial.mainTexture = gradientTexture;
            }
            private Color GetOffsetColor(float timeOffsetSeconds)
            {
                Theme currentTheme = Themes.List[Core.ThemeValue];
                if (currentTheme.Colors == null || currentTheme.Colors.Length == 0)
                    return Color.white;
                if (currentTheme.Colors.Length == 1)
                    return currentTheme.Colors[0];
                float totalRange = currentTheme.Colors.Length - 1;
                float t = Mathf.PingPong((Time.time + timeOffsetSeconds) * currentTheme.Speed, totalRange);
                int indexA = Mathf.FloorToInt(t);
                int indexB = Mathf.Clamp(indexA + 1, 0, currentTheme.Colors.Length - 1);
                float localT = t - indexA;
                float easedT = localT < 0.5f
                    ? 2f * localT * localT
                    : 1f - Mathf.Pow(-2f * localT + 2f, 2f) / 2f;
                return Color.Lerp(currentTheme.Colors[indexA], currentTheme.Colors[indexB], easedT);
            }
            private void UpdateGradientTexture()
            {
                if (gradientTexture == null) return;
                Color color1 = lastColor2;
                Color color2 = lastColor1;
                color1.a = 1f;
                color2.a = 1f;
                int index = 0;
                if (isCylinder)
                {
                    for (int y = 0; y < height; y++)
                    {
                        float t = (float)y / height;
                        Color lineColor = Color.Lerp(color1, color2, t);
                        for (int x = 0; x < width; x++)
                        {
                            pixels[index++] = lineColor;
                        }
                    }
                }
                else if (isVertical)
                {
                    for (int y = 0; y < height; y++)
                    {
                        float t = (float)y / height;
                        Color lineColor = Color.Lerp(color1, color2, t);
                        for (int x = 0; x < width; x++)
                        {
                            pixels[index++] = lineColor;
                        }
                    }
                }
                else
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            float t = (float)x / width;
                            pixels[index++] = Color.Lerp(color1, color2, t);
                        }
                    }
                }
                gradientTexture.SetPixels(pixels);
                gradientTexture.Apply(false);
            }
            public void SetBrightness(float value)
            {
                brightness = Mathf.Max(0f, value);
                needsUpdate = true;
            }
            private void OnDestroy()
            {
                if (gradientTexture != null)
                    Destroy(gradientTexture);
                if (cachedMaterial != null)
                    Destroy(cachedMaterial);
            }
        }

        public class ColorSetter : MonoBehaviour
        {
            [Header("Color Settings")]
            [SerializeField, Range(0f, 1f)] public float brightness = 1f;
            [SerializeField, Range(0f, 10f)] public float colorOffset = 0f;
            private Renderer rend;
            private Material instanceMaterial;
            private Color lastAppliedColor;
            private float updateTimer = 0f;
            private const float updateInterval = 0.033f;
            private void Start()
            {
                rend = GetComponent<Renderer>();
                if (rend == null) return;

                instanceMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                instanceMaterial.SetFloat("_Surface", 1);
                instanceMaterial.SetFloat("_Blend", 0);
                instanceMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                instanceMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                instanceMaterial.SetFloat("_ZWrite", 0);
                instanceMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                instanceMaterial.renderQueue = (int)RenderQueue.Transparent;

                rend.material = instanceMaterial;
                lastAppliedColor = new Color(0f, 0f, 0f, 1f - brightness);
                instanceMaterial.color = lastAppliedColor;
            }
            private void Update()
            {
                if (rend == null || instanceMaterial == null || !isActiveAndEnabled) return;
                updateTimer += Time.deltaTime;
                if (updateTimer >= updateInterval)
                {
                    updateTimer = 0f;
                    Color targetColor = new Color(0f, 0f, 0f, 1f - brightness);
                    if (Mathf.Abs(lastAppliedColor.a - targetColor.a) > 0.02f)
                    {
                        lastAppliedColor = targetColor;
                        instanceMaterial.color = targetColor;
                    }
                }
            }
            public void SetBrightness(float value)
            {
                brightness = Mathf.Clamp01(value);
            }
            private void OnDestroy()
            {
                if (instanceMaterial != null)
                    Destroy(instanceMaterial);
            }
        }
    }
}