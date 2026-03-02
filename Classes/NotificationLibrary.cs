using BepInEx;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;

namespace Juul
{
    [BepInPlugin("org.Juul.gorillatag.NotifiLib", "NotificationLib", "2.0.1")]
    public class NotifiLib : BaseUnityPlugin
    {
        private static NotifiLib instance;
        private GameObject HUDParent;
        private GameObject HUDCanvas;
        private Camera mainCamera;
        private bool hasInit;

        private static Texture2D bgTexture;
        private static Texture2D progressBarBg;
        private static Texture2D progressBarFill;
        private static Texture2D glowTexture;

        private static Color[] bgPixels;
        private static Color[] pfPixels;
        private const int BG_W = 380;
        private const int BG_H = 90;
        private const int BG_R = 18;
        private const int PF_W = 350;
        private const int PF_H = 4;

        private static readonly Color textWhite = new Color(1f, 1f, 1f, 1f);
        private static readonly Color textGrey = new Color(0.85f, 0.85f, 0.85f, 1f);
        private static readonly List<NotificationUI> activeNotifications = new List<NotificationUI>();
        private static readonly List<ScreenNotification> screenNotifications = new List<ScreenNotification>();
        private const int MAX_NOTIFICATIONS = 2;
        private const int MAX_SCREEN_NOTIFICATIONS = 5;
        private const float NOTIFICATION_WIDTH = 380f;
        private const float NOTIFICATION_HEIGHT = 90f;
        private const float NOTIFICATION_SPACING = 100f;
        public static bool IsEnabled = true;
        public static bool Disablenotifcations = false;
        private static GUIStyle titleStyle;
        private static GUIStyle messageStyle;

        private static float gradientUpdateTimer = 0f;
        private const float gradientUpdateInterval = 0.033f;
        private static Color lastGradColor1;
        private static Color lastGradColor2;

        private void Awake() { instance = this; }

        private void Init()
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
            InitializeTextures();
            if (XRSettings.isDeviceActive) CreateVRHUD();
            hasInit = true;
        }

        private void InitializeTextures()
        {
            var c1 = GetThemeColor(0f);
            var c2 = GetThemeColor(0.5f);

            bgPixels = BuildGradientRoundedRectPixels(BG_W, BG_H, c1, c2, BG_R);
            bgTexture = new Texture2D(BG_W, BG_H, TextureFormat.RGBA32, false);
            bgTexture.filterMode = FilterMode.Bilinear;
            bgTexture.wrapMode = TextureWrapMode.Clamp;
            bgTexture.SetPixels(bgPixels);
            bgTexture.Apply(false);

            progressBarBg = CreateRoundedRect(PF_W, PF_H, new Color(0.08f, 0.08f, 0.08f, 1f), 2);

            pfPixels = BuildSolidRoundedRectPixels(PF_W, PF_H, c1, 2);
            progressBarFill = new Texture2D(PF_W, PF_H, TextureFormat.RGBA32, false);
            progressBarFill.filterMode = FilterMode.Bilinear;
            progressBarFill.wrapMode = TextureWrapMode.Clamp;
            progressBarFill.SetPixels(pfPixels);
            progressBarFill.Apply(false);

            glowTexture = CreateGlow(BG_W, BG_H, new Color(0.7f, 0.7f, 0.7f, 1f));

            lastGradColor1 = c1;
            lastGradColor2 = c2;
        }

        private void CreateVRHUD()
        {
            HUDParent = new GameObject("NOTIFICATIONLIB_VR_PARENT");
            HUDParent.transform.SetParent(mainCamera.transform, false);
            HUDParent.transform.localPosition = new Vector3(-0.15f, 0f, 0.8f);
            HUDParent.transform.localRotation = Quaternion.Euler(0f, -20f, 0f);

            HUDCanvas = new GameObject("NOTIFICATIONLIB_CANVAS");
            HUDCanvas.transform.SetParent(HUDParent.transform, false);
            Canvas canvas = HUDCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;
            CanvasScaler scaler = HUDCanvas.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;
            HUDCanvas.AddComponent<GraphicRaycaster>();
            RectTransform rect = HUDCanvas.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(500f, 500f);
            rect.localScale = Vector3.one * 0.001f;
            rect.localPosition = Vector3.zero;
        }

        private void Update()
        {
            if (!hasInit && Camera.main != null && GorillaTagger.Instance != null) Init();
            if (!hasInit) return;

            gradientUpdateTimer += Time.deltaTime;
            if (gradientUpdateTimer >= gradientUpdateInterval)
            {
                gradientUpdateTimer = 0f;
                var c1 = GetThemeColor(0f);
                var c2 = GetThemeColor(0.5f);
                if (Vector4.Distance(c1, lastGradColor1) > 0.02f || Vector4.Distance(c2, lastGradColor2) > 0.02f)
                {
                    lastGradColor1 = c1;
                    lastGradColor2 = c2;
                    RebuildGradientTextures(c1, c2);
                }
            }

            if (XRSettings.isDeviceActive)
            {
                for (int i = activeNotifications.Count - 1; i >= 0; i--)
                {
                    if (activeNotifications[i] != null) activeNotifications[i].Update();
                    else activeNotifications.RemoveAt(i);
                }
            }

            for (int i = screenNotifications.Count - 1; i >= 0; i--)
            {
                screenNotifications[i].Update();
                if (screenNotifications[i].State == NotificationState.Completed)
                {
                    screenNotifications.RemoveAt(i);
                    UpdateScreenNotificationPositions();
                }
            }
        }

        private static void RebuildGradientTextures(Color c1, Color c2)
        {
            if (bgTexture == null || progressBarFill == null) return;

            bgPixels = BuildGradientRoundedRectPixels(BG_W, BG_H, c1, c2, BG_R);
            bgTexture.SetPixels(bgPixels);
            bgTexture.Apply(false);

            pfPixels = BuildSolidRoundedRectPixels(PF_W, PF_H, c1, 2);
            progressBarFill.SetPixels(pfPixels);
            progressBarFill.Apply(false);
        }

        private void OnGUI()
        {
            if (XRSettings.isDeviceActive) return;
            if (screenNotifications.Count == 0) return;
            InitializeStyles();
            float x = 15f;
            float baseY = Screen.height - 15f - NOTIFICATION_HEIGHT;
            for (int i = screenNotifications.Count - 1; i >= 0; i--)
            {
                ScreenNotification notif = screenNotifications[i];
                int displayIndex = screenNotifications.Count - 1 - i;
                notif.TargetY = baseY - (displayIndex * (NOTIFICATION_HEIGHT + 10f));
                notif.CurrentY = Mathf.Lerp(notif.CurrentY, notif.TargetY, Time.deltaTime * 8f);
                DrawScreenNotification(notif, x + notif.OffsetX, notif.CurrentY);
            }
        }

        private void DrawScreenNotification(ScreenNotification notif, float x, float y)
        {
            float scale = notif.Scale;
            if (scale <= 0.01f) return;

            float scaledWidth = NOTIFICATION_WIDTH * scale;
            float scaledHeight = NOTIFICATION_HEIGHT * scale;
            float offsetX = (NOTIFICATION_WIDTH - scaledWidth) * 0.5f;
            float offsetY = (NOTIFICATION_HEIGHT - scaledHeight) * 0.5f;
            Rect rect = new Rect(x + offsetX, y + offsetY, scaledWidth, scaledHeight);

            Color orig = GUI.color;

            if (notif.State == NotificationState.Displaying)
            {
                float pulse = 0.3f + Mathf.Sin(Time.time * 3f) * 0.2f;
                GUI.color = new Color(1f, 1f, 1f, pulse * notif.Alpha * 0.3f);
                GUI.DrawTexture(new Rect(x + offsetX - 3f * scale, y + offsetY - 3f * scale,
                    (NOTIFICATION_WIDTH + 6f) * scale, (NOTIFICATION_HEIGHT + 6f) * scale), glowTexture);
            }

            GUI.color = new Color(1f, 1f, 1f, notif.Alpha);
            GUI.DrawTexture(rect, bgTexture);

            GUI.color = new Color(textWhite.r, textWhite.g, textWhite.b, notif.Alpha);
            GUI.Label(new Rect(x + offsetX + 20f * scale, y + offsetY + 15f * scale, 340f * scale, 25f * scale), notif.Title, titleStyle);

            GUI.color = new Color(textGrey.r, textGrey.g, textGrey.b, notif.Alpha);
            GUI.Label(new Rect(x + offsetX + 20f * scale, y + offsetY + 38f * scale, 340f * scale, 30f * scale), notif.Message, messageStyle);

            GUI.color = new Color(1f, 1f, 1f, notif.Alpha * 0.5f);
            GUI.DrawTexture(new Rect(x + offsetX + 15f * scale, y + offsetY + (NOTIFICATION_HEIGHT - 12f) * scale, 350f * scale, 4f * scale), progressBarBg);

            float progress = Mathf.Clamp01(1f - ((Time.time - notif.StartTime) / notif.Duration));
            float fillWidth = 350f * progress * scale;
            if (fillWidth > 0f)
            {
                GUI.color = new Color(1f, 1f, 1f, notif.Alpha);
                GUI.DrawTexture(new Rect(x + offsetX + 15f * scale, y + offsetY + (NOTIFICATION_HEIGHT - 12f) * scale, fillWidth, 4f * scale), progressBarFill);
            }

            GUI.color = orig;
        }

        private void InitializeStyles()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = textWhite },
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = false,
                    richText = true
                };
            }
            if (messageStyle == null)
            {
                messageStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    normal = { textColor = textGrey },
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = true,
                    richText = true
                };
            }
        }

        private static Color GetThemeColor(float timeOffset)
        {
            if (Themes.List == null || Themes.List.Length == 0) return Color.white;
            var theme = Themes.List[Core.ThemeValue];
            if (theme.Colors == null || theme.Colors.Length == 0) return Color.white;
            if (theme.Colors.Length == 1) return theme.Colors[0];
            float totalRange = theme.Colors.Length - 1;
            float t = Mathf.PingPong((Time.time + timeOffset) * theme.Speed, totalRange);
            int indexA = Mathf.FloorToInt(t);
            int indexB = Mathf.Clamp(indexA + 1, 0, theme.Colors.Length - 1);
            float localT = t - indexA;
            float easedT = localT < 0.5f ? 2f * localT * localT : 1f - Mathf.Pow(-2f * localT + 2f, 2f) / 2f;
            return Color.Lerp(theme.Colors[indexA], theme.Colors[indexB], easedT);
        }

        public enum NotifiReason
        {
            Info, Button, RoomJoined, RoomLeft, MasterClientChange, Error, Success, Warning
        }

        public static void SendNotification(string message, NotifiReason reason = NotifiReason.Info)
        {
            SendNotification(GetReasonTitle(reason), message, 3f, reason);
        }

        public static void SendNotification(string title, string message, float duration = 3f, NotifiReason reason = NotifiReason.Info)
        {
            if (Disablenotifcations || !IsEnabled || instance == null || !instance.hasInit) return;
            try
            {
                if (XRSettings.isDeviceActive)
                {
                    activeNotifications.RemoveAll(n => n == null || n.notifObject == null);
                    while (activeNotifications.Count >= MAX_NOTIFICATIONS)
                    {
                        var oldest = activeNotifications[0];
                        if (oldest != null && oldest.notifObject != null)
                            instance.StartCoroutine(oldest.FadeOut(true));
                        else
                            activeNotifications.RemoveAt(0);
                        activeNotifications.RemoveAt(0);
                    }
                    var notif = new NotificationUI(title, message, duration, reason);
                    activeNotifications.Add(notif);
                    instance.StartCoroutine(notif.Show());
                    instance.StartCoroutine(notif.LifeCycle());
                    UpdateNotificationPositions();
                }

                if (!XRSettings.isDeviceActive)
                {
                    while (screenNotifications.Count >= MAX_SCREEN_NOTIFICATIONS)
                        screenNotifications.RemoveAt(0);
                    screenNotifications.Add(new ScreenNotification
                    {
                        Title = title,
                        Message = message,
                        Duration = duration,
                        StartTime = Time.time,
                        State = NotificationState.FadingIn,
                        Alpha = 0f,
                        Scale = 0f,
                        OffsetX = 0f,
                        TargetY = 0f,
                        CurrentY = Screen.height - 15f - NOTIFICATION_HEIGHT
                    });
                    UpdateScreenNotificationPositions();
                }
            }
            catch (Exception) { }
        }

        private static void UpdateNotificationPositions()
        {
            for (int i = 0; i < activeNotifications.Count; i++)
            {
                if (activeNotifications[i] != null)
                    activeNotifications[i].targetY = (activeNotifications.Count - 1 - i) * NOTIFICATION_SPACING;
            }
        }

        private static void UpdateScreenNotificationPositions()
        {
            float baseY = Screen.height - 15f - NOTIFICATION_HEIGHT;
            for (int i = screenNotifications.Count - 1; i >= 0; i--)
                screenNotifications[i].TargetY = baseY - ((screenNotifications.Count - 1 - i) * (NOTIFICATION_HEIGHT + 10f));
        }

        private static string GetReasonTitle(NotifiReason reason)
        {
            return reason switch
            {
                NotifiReason.Error => "<color=#ff4444>ERROR</color>",
                NotifiReason.Success => "<color=#44ff44>SUCCESS</color>",
                NotifiReason.Warning => "<color=#ffaa44>WARNING</color>",
                NotifiReason.RoomJoined => "<color=#44aaff>JOINED</color>",
                NotifiReason.RoomLeft => "<color=#ff8844>LEFT</color>",
                _ => "<color=#ffffff>INFO</color>",
            };
        }

        public static void ClearAllNotifications()
        {
            if (instance == null) return;
            foreach (var n in activeNotifications)
                if (n != null && n.notifObject != null) n.ForceDestroy();
            activeNotifications.Clear();
            screenNotifications.Clear();
        }

        public static void ClearPastNotifications(int amount)
        {
            if (instance == null) return;
            amount = Math.Min(amount, activeNotifications.Count);
            for (int i = 0; i < amount; i++)
                if (activeNotifications[i] != null && activeNotifications[i].notifObject != null)
                    activeNotifications[i].ForceDestroy();
            activeNotifications.RemoveRange(0, amount);
            UpdateNotificationPositions();
            screenNotifications.RemoveRange(0, Math.Min(amount, screenNotifications.Count));
        }

        private void OnDestroy()
        {
            ClearAllNotifications();
            if (bgTexture != null) Destroy(bgTexture);
            if (progressBarBg != null) Destroy(progressBarBg);
            if (progressBarFill != null) Destroy(progressBarFill);
            if (glowTexture != null) Destroy(glowTexture);
        }
        private static float RoundedRectSDF(float px, float py, float w, float h, float r)
        {
            float cx = w * 0.5f, cy = h * 0.5f;
            float qx = Mathf.Abs(px - cx) - (cx - r);
            float qy = Mathf.Abs(py - cy) - (cy - r);
            float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
            float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
            return outside + inside - r;
        }

        private static Color[] BuildGradientRoundedRectPixels(int width, int height, Color c1, Color c2, int radius)
        {
            var pixels = new Color[width * height];
            float r = radius;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = RoundedRectSDF(x + 0.5f, y + 0.5f, width, height, r);
                    float alpha = Mathf.Clamp01(-(dist - 0.5f) / 1.5f);
                    if (alpha <= 0f)
                    {
                        pixels[x + y * width] = Color.clear;
                    }
                    else
                    {
                        Color g = Color.Lerp(c1, c2, (float)x / width);
                        g.a = alpha * 0.95f;
                        pixels[x + y * width] = g;
                    }
                }
            }
            return pixels;
        }

        private static Color[] BuildSolidRoundedRectPixels(int width, int height, Color color, int radius)
        {
            var pixels = new Color[width * height];
            float r = radius;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = RoundedRectSDF(x + 0.5f, y + 0.5f, width, height, r);
                    float alpha = Mathf.Clamp01(-(dist - 0.5f) / 1.5f);
                    if (alpha <= 0f)
                        pixels[x + y * width] = Color.clear;
                    else
                    {
                        var c = color;
                        c.a *= alpha;
                        pixels[x + y * width] = c;
                    }
                }
            }
            return pixels;
        }

        private static Texture2D CreateRoundedRect(int width, int height, Color color, int radius)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.SetPixels(BuildSolidRoundedRectPixels(width, height, color, radius));
            tex.Apply(false);
            return tex;
        }

        private static Texture2D CreateGlow(int width, int height, Color color)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
            float maxDist = Mathf.Sqrt(center.x * center.x + center.y * center.y);
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = x - center.x, dy = y - center.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = 1f - Mathf.Clamp01(dist / maxDist);
                    alpha = alpha * alpha * 0.5f;
                    pixels[x + y * width] = new Color(color.r, color.g, color.b, alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply(false);
            return tex;
        }

        private enum NotificationState { FadingIn, Displaying, FadingOut, Completed }

        private class ScreenNotification
        {
            public string Title, Message;
            public float Duration, StartTime, Alpha, Scale, OffsetX, TargetY, CurrentY;
            public NotificationState State;

            public void Update()
            {
                float elapsed = Time.time - StartTime;
                switch (State)
                {
                    case NotificationState.FadingIn:
                        Alpha = Mathf.Lerp(Alpha, 1f, Time.deltaTime * 10f);
                        Scale = Mathf.Lerp(Scale, 1f, Time.deltaTime * 12f);
                        if (Alpha > 0.98f && Scale > 0.98f) { State = NotificationState.Displaying; Alpha = 1f; Scale = 1f; }
                        break;
                    case NotificationState.Displaying:
                        if (elapsed >= Duration - 0.5f) State = NotificationState.FadingOut;
                        break;
                    case NotificationState.FadingOut:
                        Alpha = Mathf.Lerp(Alpha, 0f, Time.deltaTime * 8f);
                        Scale = Mathf.Lerp(Scale, 0f, Time.deltaTime * 10f);
                        if (Alpha < 0.02f && Scale < 0.02f) State = NotificationState.Completed;
                        break;
                }
            }
        }

        private class NotificationUI
        {
            public GameObject notifObject;
            public string title, message;
            public float duration, startTime, targetY, currentAlpha;
            public NotifiReason reason;
            private Image background, glow, progressBg, progressFill;
            private TextMeshProUGUI titleText, messageText;
            private RectTransform rectTransform;
            private enum State { FadingIn, Displaying, FadingOut, Completed }
            private State currentState = State.FadingIn;
            private float lastUpdateTime;
            private const float UPDATE_INTERVAL = 0.016f;

            public NotificationUI(string title, string message, float duration, NotifiReason reason)
            {
                this.title = title; this.message = message;
                this.duration = duration; this.reason = reason;
                this.startTime = Time.time; this.lastUpdateTime = Time.time;
                CreateUI();
            }

            private void CreateUI()
            {
                notifObject = new GameObject("Notification");
                notifObject.transform.SetParent(instance.HUDCanvas.transform, false);
                rectTransform = notifObject.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(NOTIFICATION_WIDTH, NOTIFICATION_HEIGHT);
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = Vector3.zero;

                var glowObj = new GameObject("Glow");
                glowObj.transform.SetParent(notifObject.transform, false);
                glow = glowObj.AddComponent<Image>();
                glow.sprite = Sprite.Create(glowTexture, new Rect(0, 0, glowTexture.width, glowTexture.height), new Vector2(0.5f, 0.5f));
                glow.raycastTarget = false;
                var glowRect = glowObj.GetComponent<RectTransform>();
                glowRect.sizeDelta = new Vector2(NOTIFICATION_WIDTH + 6, NOTIFICATION_HEIGHT + 6);
                glowRect.anchoredPosition = Vector2.zero;
                glow.color = new Color(1f, 1f, 1f, 0f);

                var bgObj = new GameObject("Background");
                bgObj.transform.SetParent(notifObject.transform, false);
                background = bgObj.AddComponent<Image>();
                background.sprite = Sprite.Create(bgTexture, new Rect(0, 0, bgTexture.width, bgTexture.height), new Vector2(0.5f, 0.5f));
                background.raycastTarget = false;
                var bgRect = bgObj.GetComponent<RectTransform>();
                bgRect.sizeDelta = new Vector2(NOTIFICATION_WIDTH, NOTIFICATION_HEIGHT);
                bgRect.anchoredPosition = Vector2.zero;

                var progressBgObj = new GameObject("ProgressBg");
                progressBgObj.transform.SetParent(notifObject.transform, false);
                progressBg = progressBgObj.AddComponent<Image>();
                progressBg.sprite = Sprite.Create(progressBarBg, new Rect(0, 0, progressBarBg.width, progressBarBg.height), new Vector2(0.5f, 0.5f));
                progressBg.raycastTarget = false;
                var progressBgRect = progressBgObj.GetComponent<RectTransform>();
                progressBgRect.sizeDelta = new Vector2(350f, 4f);
                progressBgRect.anchoredPosition = new Vector2(0, -40);

                var progressFillObj = new GameObject("ProgressFill");
                progressFillObj.transform.SetParent(notifObject.transform, false);
                progressFill = progressFillObj.AddComponent<Image>();
                progressFill.sprite = Sprite.Create(progressBarFill, new Rect(0, 0, progressBarFill.width, progressBarFill.height), new Vector2(0f, 0.5f));
                progressFill.type = Image.Type.Filled;
                progressFill.fillMethod = Image.FillMethod.Horizontal;
                progressFill.fillAmount = 1f;
                progressFill.raycastTarget = false;
                var progressFillRect = progressFillObj.GetComponent<RectTransform>();
                progressFillRect.sizeDelta = new Vector2(350f, 4f);
                progressFillRect.pivot = new Vector2(0f, 0.5f);
                progressFillRect.anchoredPosition = new Vector2(-175, -40);

                var titleObj = new GameObject("Title");
                titleObj.transform.SetParent(notifObject.transform, false);
                var tc = titleObj.AddComponent<Canvas>(); tc.overrideSorting = true; tc.sortingOrder = 10;
                titleText = titleObj.AddComponent<TextMeshProUGUI>();
                titleText.text = title; titleText.fontSize = 18; titleText.fontStyle = FontStyles.Bold;
                titleText.color = textWhite; titleText.alignment = TextAlignmentOptions.Left;
                titleText.enableWordWrapping = false; titleText.richText = true; titleText.raycastTarget = false;
                var titleRect = titleObj.GetComponent<RectTransform>();
                titleRect.sizeDelta = new Vector2(340f, 30f); titleRect.anchoredPosition = new Vector2(0, 15);

                var msgObj = new GameObject("Message");
                msgObj.transform.SetParent(notifObject.transform, false);
                var mc = msgObj.AddComponent<Canvas>(); mc.overrideSorting = true; mc.sortingOrder = 10;
                messageText = msgObj.AddComponent<TextMeshProUGUI>();
                messageText.text = message; messageText.fontSize = 14;
                messageText.color = textGrey; messageText.alignment = TextAlignmentOptions.Left;
                messageText.enableWordWrapping = true; messageText.richText = true; messageText.raycastTarget = false;
                var msgRect = msgObj.GetComponent<RectTransform>();
                msgRect.sizeDelta = new Vector2(340f, 35f); msgRect.anchoredPosition = new Vector2(0, -10);
            }

            public IEnumerator Show()
            {
                float elapsed = 0f, fadeTime = 0.3f;
                while (elapsed < fadeTime)
                {
                    if (notifObject == null || rectTransform == null) { currentState = State.Completed; yield break; }
                    float t = elapsed / fadeTime;
                    rectTransform.localScale = Vector3.one * EaseOutBack(t);
                    UpdateAlpha(t);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                if (rectTransform != null) rectTransform.localScale = Vector3.one;
                UpdateAlpha(1f);
                currentState = State.Displaying;
            }

            private float EaseOutBack(float t)
            {
                float c1 = 1.70158f, c3 = c1 + 1f;
                return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
            }

            public IEnumerator LifeCycle()
            {
                yield return new WaitForSeconds(duration - 0.5f);
                yield return FadeOut(false);
            }

            public IEnumerator FadeOut(bool immediate)
            {
                currentState = State.FadingOut;
                float fadeTime = immediate ? 0.15f : 0.25f, elapsed = 0f;
                while (elapsed < fadeTime)
                {
                    if (notifObject == null || rectTransform == null) { currentState = State.Completed; yield break; }
                    float t = 1f - (elapsed / fadeTime);
                    rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                    UpdateAlpha(t);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                ForceDestroy();
                currentState = State.Completed;
            }

            public void ForceDestroy()
            {
                if (notifObject != null) { UnityEngine.Object.Destroy(notifObject); notifObject = null; }
            }

            private void UpdateAlpha(float alpha)
            {
                if (notifObject == null) return;
                currentAlpha = alpha;
                if (background != null) background.color = new Color(1f, 1f, 1f, alpha);
                if (titleText != null) titleText.color = new Color(textWhite.r, textWhite.g, textWhite.b, alpha);
                if (messageText != null) messageText.color = new Color(textGrey.r, textGrey.g, textGrey.b, alpha * 0.85f);
                if (progressBg != null) progressBg.color = new Color(1f, 1f, 1f, alpha * 0.5f);
                if (progressFill != null) progressFill.color = new Color(1f, 1f, 1f, alpha);
                if (glow != null && currentState == State.Displaying)
                {
                    float pulse = 0.3f + Mathf.Sin(Time.time * 3f) * 0.2f;
                    glow.color = new Color(1f, 1f, 1f, pulse * alpha * 0.3f);
                }
            }

            public void Update()
            {
                if (notifObject == null || rectTransform == null) return;
                if (Time.time - lastUpdateTime < UPDATE_INTERVAL) return;
                lastUpdateTime = Time.time;

                var cur = rectTransform.anchoredPosition;
                var tar = new Vector2(0, targetY);
                if (Vector2.Distance(cur, tar) > 0.1f)
                    rectTransform.anchoredPosition = Vector2.Lerp(cur, tar, Time.deltaTime * 8f);

                if (currentState == State.Displaying && progressFill != null)
                    progressFill.fillAmount = Mathf.Clamp01(1f - ((Time.time - startTime) / duration));
            }
        }
    }
}