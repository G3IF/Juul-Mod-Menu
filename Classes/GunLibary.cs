using BepInEx;
using GorillaLocomotion;
using Oculus.Platform;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Random = UnityEngine.Random;

namespace Juul
{
    public class GunLib : MonoBehaviour
    {
        private void FixedUpdate()
        {
            LineColor = Core.BaseColor;
            PointerColor = Core.BaseColor;
        }

        public static void ChangeGunStyle(bool forward = true)
        {
            if (forward)
                currentLineStyle++;
            else
                currentLineStyle--;

            if (currentLineStyle > GunLineStyle.CoiledLine)
                currentLineStyle = GunLineStyle.Smooth;
            if (currentLineStyle < GunLineStyle.Smooth)
                currentLineStyle = GunLineStyle.CoiledLine;

            if (Buttons.GunStyleButton != null)
                Buttons.GunStyleButton.Name = "Gun Style: " + currentLineStyle.ToString();
        }

        public static void ChangeGunLineSize(bool forward = true)
        {
            if (forward)
                currentLineSize++;
            else
                currentLineSize--;

            if (currentLineSize >= LineScales.Length)
                currentLineSize = 0;
            if (currentLineSize < 0)
                currentLineSize = LineScales.Length - 1;

            GunLineWidth = LineScales[currentLineSize];

            if (Buttons.GunLineSizeButton != null)
                Buttons.GunLineSizeButton.Name = string.Format("Gun Line Size: {0}", GunLineWidth);
        }

        public static void ChangeGunSphereScale(bool forward = true)
        {
            if (forward)
                currentSphereSize++;
            else
                currentSphereSize--;

            if (currentSphereSize >= SphereScales.Length)
                currentSphereSize = 0;
            if (currentSphereSize < 0)
                currentSphereSize = SphereScales.Length - 1;

            SphereSize = SphereScales[currentSphereSize];

            if (spherepointer != null)
                spherepointer.transform.localScale = Vector3.one * SphereSize;

            if (Buttons.GunSphereSizeButton != null)
                Buttons.GunSphereSizeButton.Name = string.Format("Gun Sphere Size: {0}", SphereSize);
        }

        public enum GunLineStyle
        {
            Smooth,
            Straight,
            Wavy,
            DynamicPulse,
            Electric,
            Spiral,
            Throb,
            Helix,
            Flare,
            Zigzag,
            Meteor,
            Burst,
            Spiral1,
            Sparky,
            ElectricZag,
            WaterRipple,
            Webbed,
            BurstPulse,
            CoiledLine,
        }

        public static int LineCurve = 150;
        private const float LineSmoothFactor = 6f;
        private const float DestroyDelay = 0.02f;
        public static float PulseSpeed = 2f;
        public static float PulseAmplitude = 0.03f;
        public static GameObject spherepointer;
        public static VRRig LockedPlayer;
        public static bool isLocked;
        public static Vector3 lr;
        public static Color32 PointerColor = Core.BaseColor;
        public static Color32 LineColor = Core.BaseColor;
        public static GunLineStyle currentLineStyle = GunLineStyle.Smooth;
        public static RaycastHit raycastHit;
        public static float SphereSize = 0.04f;
        public static float GunLineWidth = 0.01f;
        private static int currentLineSize = 0;
        private static readonly float[] LineScales = { 0.001f, 0.002f, 0.003f, 0.004f, 0.005f, 0.006f, 0.007f, 0.008f, 0.009f, 0.01f, 0.011f, 0.012f, 0.013f, 0.014f, 0.015f, 0.016f, 0.017f, 0.018f, 0.019f, 0.02f };
        private static int currentSphereSize = 0;
        private static readonly float[] SphereScales = { 0.01f, 0.02f, 0.03f, 0.04f, 0.05f, 0.06f, 0.07f, 0.08f, 0.09f, 0.1f, 0.11f, 0.12f, 0.13f, 0.14f, 0.15f, 0.16f, 0.17f, 0.18f, 0.19f, 0.2f };

        private static GameObject _gunLineObject;
        private static LineRenderer _gunLineRenderer;
        private static Material _gunLineMaterial;
        private static Coroutine _gunLineCoroutine;
        private static MonoBehaviour _gunLineHost;

        private static LineRenderer GetOrCreateGunLine()
        {
            if (_gunLineRenderer == null || _gunLineObject == null)
            {
                if (_gunLineObject != null) GameObject.Destroy(_gunLineObject);
                _gunLineObject = new GameObject("GunLine_Persistent");
                _gunLineRenderer = _gunLineObject.AddComponent<LineRenderer>();
                _gunLineMaterial = new Material(Core.UberShader);
                _gunLineRenderer.material = _gunLineMaterial;
                _gunLineRenderer.useWorldSpace = true;
                _gunLineHost = _gunLineObject.AddComponent<GunLib>();
                _gunLineCoroutine = null;
            }
            return _gunLineRenderer;
        }

        private static void UpdateGunLine(Vector3 handPos, Vector3 spherePos, Vector3 smoothMid)
        {
            var lineRenderer = GetOrCreateGunLine();
            lineRenderer.startWidth = GunLineWidth;
            lineRenderer.endWidth = GunLineWidth;
            lineRenderer.startColor = LineColor;
            lineRenderer.endColor = LineColor;
            _gunLineMaterial.color = LineColor;
            _gunLineObject.SetActive(true);

            SetLineStyle(lineRenderer, handPos, spherePos, smoothMid);
            lineRenderer.startColor = Color.Lerp(Core.BaseColor, Core.BaseColor, Mathf.PingPong(Time.time * 1, 0.5f));
            lineRenderer.endColor = lineRenderer.startColor;
        }

        private static void HideGunLine()
        {
            if (_gunLineObject != null)
                _gunLineObject.SetActive(false);
        }

        private static int? noInvisLayerMask;
        public static int NoInvisLayerMask()
        {
            noInvisLayerMask ??= ~(
                (1 << LayerMask.NameToLayer("TransparentFX")) |
                (1 << LayerMask.NameToLayer("Ignore Raycast")) |
                (1 << LayerMask.NameToLayer("Zone")) |
                (1 << LayerMask.NameToLayer("Gorilla Trigger")) |
                (1 << LayerMask.NameToLayer("Gorilla Boundary")) |
                (1 << LayerMask.NameToLayer("GorillaCosmetics")) |
                (1 << LayerMask.NameToLayer("GorillaParticle"))
            );

            return noInvisLayerMask ?? GTPlayer.Instance.locomotionEnabledLayers;
        }

        public static void GunPreview()
        {
            StartPointerSystem(() => { }, false);
        }

        public static void GunPreviewLock()
        {
            StartPointerSystem(() => { }, true);
        }

        public static void SetSphereSize(float newSize)
        {
            SphereSize = Mathf.Clamp(newSize, 0.01f, 0.3f);
            if (spherepointer != null)
                spherepointer.transform.localScale = Vector3.one * SphereSize;
        }

        public static void SetGunLineWidth(float newWidth)
        {
            GunLineWidth = Mathf.Clamp(newWidth, 0.001f, 0.1f);
        }

        private static void SetLineStyle(LineRenderer lineRenderer, Vector3 start, Vector3 end, Vector3 smoothMid)
        {
            switch (currentLineStyle)
            {
                case GunLineStyle.Smooth: CurveLineRenderer(lineRenderer, start, smoothMid, end); break;
                case GunLineStyle.Straight: lineRenderer.positionCount = 2; lineRenderer.SetPosition(0, start); lineRenderer.SetPosition(1, end); break;
                case GunLineStyle.Wavy: DrawWavyLine(lineRenderer, start, end, 20, 0.1f, 5f); break;
                case GunLineStyle.DynamicPulse: DrawDynamicPulse(lineRenderer, start, end, 50); break;
                case GunLineStyle.Electric: DrawElectricLine(lineRenderer, start, end, 30, 0.2f); break;
                case GunLineStyle.Spiral: DrawSpiralLine(lineRenderer, start, end, 250, 0.05f, 15); break;
                case GunLineStyle.Throb: DrawThrobLine(lineRenderer, start, end, 30); break;
                case GunLineStyle.Helix: DrawHelixLine(lineRenderer, start, end, 250, 0.05f, 15); break;
                case GunLineStyle.Flare: DrawFlareLine(lineRenderer, start, end, 30); break;
                case GunLineStyle.Zigzag: DrawZigzagLine(lineRenderer, start, end, 20, 0.1f); break;
                case GunLineStyle.Meteor: DrawMeteorLine(lineRenderer, start, end, 30); break;
                case GunLineStyle.Burst: DrawBurstLine(lineRenderer, start, end, 30); break;
                case GunLineStyle.Spiral1: DrawSpiralLine1(lineRenderer, start, end, 250, 0.05f, 15); break;
                case GunLineStyle.Sparky: DrawSparkyLine(lineRenderer, start, end, 50); break;
                case GunLineStyle.ElectricZag: DrawElectricZagLine(lineRenderer, start, end, 50); break;
                case GunLineStyle.WaterRipple: DrawWaterRippleLine(lineRenderer, start, end, 50); break;
                case GunLineStyle.Webbed: DrawWebbedLine(lineRenderer, start, end, 50); break;
                case GunLineStyle.BurstPulse: DrawBurstPulseLine(lineRenderer, start, end, 50); break;
                case GunLineStyle.CoiledLine: DrawCoiledLine(lineRenderer, start, end, 50); break;
            }
        }

        public static void CycleLineStyle()
        {
            currentLineStyle = (GunLineStyle)(((int)currentLineStyle + 1) % Enum.GetNames(typeof(GunLineStyle)).Length);
        }

        private static void DrawThrobLine(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            float pulse = Mathf.Abs(Mathf.Sin(Time.time * 4f)) * 0.05f;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                pos += Vector3.up * Mathf.Sin(t * Mathf.PI * 2 + Time.time * 4f) * pulse;
                lr.SetPosition(i, pos);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawWavyLine(LineRenderer lr, Vector3 start, Vector3 end, int points, float amplitude, float frequency)
        {
            lr.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                pos.y += Mathf.Sin(Time.time * frequency + t * Mathf.PI * 2) * amplitude;
                lr.SetPosition(i, pos);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawDynamicPulse(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                float pulse = Mathf.Abs(Mathf.Sin(Time.time * 5f + t * 10f)) * 0.05f;
                pos += Vector3.up * pulse;
                lr.SetPosition(i, pos);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawElectricLine(LineRenderer lr, Vector3 start, Vector3 end, int segments, float jagAmount)
        {
            lr.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                pos += Random.insideUnitSphere * jagAmount * Mathf.Sin(Time.time * 20f);
                lr.SetPosition(i, pos);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(segments - 1, end);
        }

        private static void DrawSpiralLine(LineRenderer lr, Vector3 start, Vector3 end, int points, float radius, float twists)
        {
            lr.positionCount = points;
            Vector3 direction = end - start;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                float angle = t * twists * Mathf.PI * 2;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                pos += Quaternion.LookRotation(direction) * offset;
                lr.SetPosition(i, pos);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawSpiralLine1(LineRenderer lr, Vector3 start, Vector3 end, int points, float radius, int turns)
        {
            lr.positionCount = points;
            Vector3 direction = (end - start).normalized;
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(direction, up)) > 0.9f) up = Vector3.right;
            Vector3 right = Vector3.Cross(direction, up).normalized;
            up = Vector3.Cross(right, direction).normalized;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t);
                float angle = t * turns * 2f * Mathf.PI + Time.time * 5f;
                Vector3 offset = right * Mathf.Cos(angle) + up * Mathf.Sin(angle);
                Vector3 styled = basePos + offset * radius;
                lr.SetPosition(i, styled);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawHelixLine(LineRenderer lr, Vector3 start, Vector3 end, int points, float radius, float twists)
        {
            lr.positionCount = points;
            Vector3 direction = end - start;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                float angle = t * twists * Mathf.PI * 2 + Time.time * 3f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                pos += Quaternion.LookRotation(direction) * offset;
                lr.SetPosition(i, pos);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawFlareLine(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                lr.startWidth = Mathf.Lerp(0.02f, 0.08f, 1 - t);
                lr.endWidth = 0.01f;
                lr.SetPosition(i, pos);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawZigzagLine(LineRenderer lr, Vector3 start, Vector3 end, int points, float amplitude)
        {
            lr.positionCount = points;
            Vector3 direction = (end - start).normalized;
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                if (i % 2 == 0)
                    pos += right * amplitude;
                else
                    pos -= right * amplitude;
                lr.SetPosition(i, pos);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawBurstLine(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            float burst = Mathf.Abs(Mathf.Sin(Time.time * 6f)) * 0.2f;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                pos += Random.insideUnitSphere * burst;
                lr.SetPosition(i, pos);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawMeteorLine(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 pos = Vector3.Lerp(start, end, t);
                lr.startWidth = Mathf.Lerp(0.1f, 0.02f, t);
                lr.endWidth = 0.01f;
                lr.SetPosition(i, pos);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawSparkyLine(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            Vector3 direction = (end - start).normalized;
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(direction, up)) > 0.9f) up = Vector3.right;
            Vector3 right = Vector3.Cross(direction, up).normalized;
            up = Vector3.Cross(right, direction).normalized;

            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t);
                float hash = Mathf.PerlinNoise(t * 12f, Time.time * 4f) * 2f - 1f;
                float amplitude = 0.1f * Mathf.Pow(1f - t, 0.3f);
                Vector3 styled = basePos + (right * hash + up * (1f - Mathf.Abs(hash))) * amplitude;
                lr.SetPosition(i, styled);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawElectricZagLine(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            Vector3 direction = (end - start).normalized;
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;

            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t);
                float segs = 20f;
                float phase = Mathf.Floor(t * segs) % 2 == 0 ? 1f : -1f;
                float amplitude = 0.06f;
                Vector3 styled = basePos + right * phase * amplitude;
                lr.SetPosition(i, styled);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawWaterRippleLine(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            Vector3 direction = (end - start).normalized;
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(direction, up)) > 0.9f) up = Vector3.right;

            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t);
                float angle = t * 12f * Mathf.PI + Time.time * 2f;
                float amplitude = 0.04f;
                Vector3 styled = basePos + up * Mathf.Sin(angle) * amplitude;
                lr.SetPosition(i, styled);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawWebbedLine(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            Vector3 direction = (end - start).normalized;
            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(direction, up).normalized;
            up = Vector3.Cross(right, direction).normalized;

            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t);
                float angle = t * 20f * Mathf.PI + Time.time * 6f;
                float amplitude = 0.03f;
                Vector3 styled = basePos + (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * amplitude;
                lr.SetPosition(i, styled);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawBurstPulseLine(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            Vector3 right = Vector3.Cross((end - start).normalized, Vector3.up).normalized;

            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t);
                float pulse = Mathf.PingPong(t * 10f - Time.time * 5f, 1f);
                float amplitude = 0.1f * pulse * (1f - t);
                Vector3 styled = basePos + right * amplitude;
                lr.SetPosition(i, styled);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static void DrawCoiledLine(LineRenderer lr, Vector3 start, Vector3 end, int points)
        {
            lr.positionCount = points;
            Vector3 direction = (end - start).normalized;
            Vector3 up = Vector3.up;
            if (Mathf.Abs(Vector3.Dot(direction, up)) > 0.9f) up = Vector3.right;
            Vector3 right = Vector3.Cross(direction, up).normalized;
            up = Vector3.Cross(right, direction).normalized;

            float coilTurns = 8f;
            float coilRadius = 0.025f;

            for (int i = 0; i < points; i++)
            {
                float t = (float)i / (points - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t);
                float coilAngle = t * 2f * Mathf.PI * coilTurns + Time.time * 5f;
                Vector3 coilOffset = right * Mathf.Cos(coilAngle) + up * Mathf.Sin(coilAngle);
                Vector3 styled = basePos + coilOffset * coilRadius;
                lr.SetPosition(i, styled);
            }
            lr.SetPosition(0, start);
            lr.SetPosition(points - 1, end);
        }

        private static Vector3 CalculateBezierPoint(Vector3 start, Vector3 mid, Vector3 end, float t)
        {
            return Mathf.Pow(1 - t, 2) * start + 2 * (1 - t) * t * mid + Mathf.Pow(t, 2) * end;
        }

        private static void CurveLineRenderer(LineRenderer lineRenderer, Vector3 start, Vector3 mid, Vector3 end)
        {
            lineRenderer.positionCount = LineCurve;
            for (int i = 0; i < LineCurve; i++)
            {
                float t = (float)i / (LineCurve - 1);
                lineRenderer.SetPosition(i, CalculateBezierPoint(start, mid, end, t));
            }
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(LineCurve - 1, end);
        }

        private static IEnumerator StartCurvyLineRenderer(LineRenderer lineRenderer, Vector3 start, Vector3 end, Vector3 smoothMid)
        {
            while (lineRenderer != null)
            {
                if (lineRenderer == null) yield break;
                SetLineStyle(lineRenderer, start, end, smoothMid);
                lineRenderer.startColor = Color.Lerp(Core.BaseColor, Core.BaseColor, Mathf.PingPong(Time.time * 1, 0.5f));
                lineRenderer.endColor = lineRenderer.startColor;
                yield return null;
            }
        }

        public static void StartVrGunR(Action action, bool LockOn)
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                isLocked = false;
                Physics.Raycast(GorillaTagger.Instance.rightHandTransform.position, -GorillaTagger.Instance.rightHandTransform.up, out raycastHit, 512f, NoInvisLayerMask());
                if (spherepointer == null)
                {
                    spherepointer = LineLib.CreateSphere(raycastHit.point, SphereSize, LineColor, true);
                    lr = GorillaTagger.Instance.offlineVRRig.rightHandTransform.position;
                }
                if (LockedPlayer == null)
                {
                    spherepointer.transform.position = raycastHit.point;
                    spherepointer.GetComponent<Renderer>().material.color = LineColor;
                    isLocked = false;
                }
                else
                {
                    spherepointer.transform.position = LockedPlayer.transform.position;
                }

                Vector3 handPos = GorillaTagger.Instance.rightHandTransform.position;
                Vector3 spherePos = spherepointer.transform.position;

                lr = Vector3.Lerp(lr, (handPos + spherePos) / 2f, Time.deltaTime * 6f);

                UpdateGunLine(handPos, spherePos, lr);

                if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f)
                {
                    trigger = true;
                    if (LockOn)
                    {
                        if (LockedPlayer == null)
                        {
                            LockedPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                        if (LockedPlayer != null)
                        {
                            spherepointer.transform.position = LockedPlayer.transform.position;
                            action();
                            isLocked = true;
                        }
                        return;
                    }
                    action();
                    return;
                }
                else
                {
                    trigger = false;
                    if (LockedPlayer != null)
                    {
                        LockedPlayer = null;
                        return;
                    }
                }
            }
            else
            {
                HideGunLine();
                if (spherepointer != null)
                {
                    LineLib.DestroySphere(spherepointer);
                    spherepointer = null;
                    LockedPlayer = null;
                }
            }
        }

        public static void StartVrGunL(Action action, bool LockOn)
        {
            if (ControllerInputPoller.instance.leftGrab)
            {
                Physics.Raycast(GorillaTagger.Instance.leftHandTransform.position, -GorillaTagger.Instance.leftHandTransform.up, out raycastHit, 512f, NoInvisLayerMask());
                if (spherepointer == null)
                {
                    spherepointer = LineLib.CreateSphere(raycastHit.point, SphereSize, LineColor, true);
                    lr = GorillaTagger.Instance.offlineVRRig.leftHandTransform.position;
                }
                if (LockedPlayer == null)
                {
                    spherepointer.transform.position = raycastHit.point;
                    spherepointer.GetComponent<Renderer>().material.color = LineColor;
                }
                else
                {
                    spherepointer.transform.position = LockedPlayer.transform.position;
                }

                Vector3 handPos = GorillaTagger.Instance.leftHandTransform.position;
                Vector3 spherePos = spherepointer.transform.position;

                lr = Vector3.Lerp(lr, (handPos + spherePos) / 2f, Time.deltaTime * 6f);

                UpdateGunLine(handPos, spherePos, lr);

                if (ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f)
                {
                    trigger = true;
                    if (LockOn)
                    {
                        if (LockedPlayer == null)
                        {
                            LockedPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                        if (LockedPlayer != null)
                        {
                            spherepointer.transform.position = LockedPlayer.transform.position;
                            action();
                            isLocked = true;
                        }
                        return;
                    }
                    action();
                    return;
                }
                else if (LockedPlayer != null)
                {
                    LockedPlayer = null;
                    return;
                }
            }
            else
            {
                HideGunLine();
                if (spherepointer != null)
                {
                    LineLib.DestroySphere(spherepointer);
                    spherepointer = null;
                    LockedPlayer = null;
                }
            }
        }

        public static void StartPcGun(Action action, bool LockOn)
        {
            Ray ray = GameObject.Find("Shoulder Camera").activeSelf ? GameObject.Find("Shoulder Camera").GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition) : GorillaTagger.Instance.mainCamera.GetComponent<Camera>().ScreenPointToRay(UnityInput.Current.mousePosition);
            if (Mouse.current.rightButton.isPressed)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(ray.origin, ray.direction, out raycastHit, 512f, NoInvisLayerMask()) && spherepointer == null)
                {
                    if (spherepointer == null)
                    {
                        spherepointer = LineLib.CreateSphere(raycastHit.point, SphereSize, LineColor, true);
                        lr = GorillaTagger.Instance.offlineVRRig.rightHandTransform.position;
                    }
                }
                if (LockedPlayer == null)
                {
                    spherepointer.transform.position = raycastHit.point;
                    spherepointer.GetComponent<Renderer>().material.color = LineColor;
                }
                else
                {
                    spherepointer.transform.position = LockedPlayer.transform.position;
                }

                Vector3 handPos = GorillaTagger.Instance.rightHandTransform.position;
                Vector3 spherePos = spherepointer.transform.position;

                lr = Vector3.Lerp(lr, (handPos + spherePos) / 2f, Time.deltaTime * 6f);

                UpdateGunLine(handPos, spherePos, lr);

                if (Mouse.current.leftButton.isPressed)
                {
                    trigger = true;
                    if (LockOn)
                    {
                        if (LockedPlayer == null)
                        {
                            LockedPlayer = raycastHit.collider.GetComponentInParent<VRRig>();
                        }
                        if (LockedPlayer != null)
                        {
                            spherepointer.transform.position = LockedPlayer.transform.position;
                            action();
                            isLocked = true;
                        }
                        return;
                    }
                    action();
                    return;
                }
                else
                {
                    trigger = false;
                    if (LockedPlayer != null)
                    {
                        LockedPlayer = null;
                        return;
                    }
                }
            }
            else
            {
                HideGunLine();
                if (spherepointer != null)
                {
                    LineLib.DestroySphere(spherepointer);
                    spherepointer = null;
                    LockedPlayer = null;
                }
            }
        }

        public static void StartPointerSystem(Action action, bool locko)
        {
            if (XRSettings.isDeviceActive)
            {
                if (gunLeft == true)
                {
                    StartVrGunL(action, locko);
                }
                else
                {
                    StartVrGunR(action, locko);
                }
            }
            if (!XRSettings.isDeviceActive)
            {
                StartPcGun(action, locko);
            }
        }

        public static bool trigger = false;
        public static bool gunLeft = false;
    }
}
