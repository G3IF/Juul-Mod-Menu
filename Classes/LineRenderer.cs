using System.Collections.Generic;
using UnityEngine;

namespace Juul
{
    public class LineLib : MonoBehaviour
    {
        private static Dictionary<GameObject, Coroutine> activePulseCoroutines = new Dictionary<GameObject, Coroutine>();

        public class Line
        {
            public GameObject gameObject;
            public LineRenderer lineRenderer;

            public void UpdatePosition(Vector3 start, Vector3 end)
            {
                if (lineRenderer != null)
                {
                    lineRenderer.SetPosition(0, start);
                    lineRenderer.SetPosition(1, end);
                }
            }

            public void UpdatePoints(Vector3[] points)
            {
                if (lineRenderer != null)
                {
                    lineRenderer.positionCount = points.Length;
                    lineRenderer.SetPositions(points);
                }
            }

            public void UpdateColor(Color color)
            {
                if (lineRenderer != null)
                {
                    lineRenderer.startColor = color;
                    lineRenderer.endColor = color;
                    if (lineRenderer.material != null)
                        lineRenderer.material.color = color;
                }
            }

            public void UpdateWidth(float width)
            {
                if (lineRenderer != null)
                {
                    lineRenderer.startWidth = width;
                    lineRenderer.endWidth = width;
                }
            }

            public void SetActive(bool active)
            {
                if (gameObject != null)
                    gameObject.SetActive(active);
            }

            public void Destroy()
            {
                if (gameObject != null)
                    GameObject.Destroy(gameObject);
            }
        }

        public static Line CreateLine(Vector3 start, Vector3 end, float width = 0.01f, Color? color = null)
        {
            GameObject lineObject = new GameObject("Line");
            lineObject.layer = 0;

            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

            Shader shader = Core.GuiTextShader ?? Shader.Find("Unlit/Color");
            lineRenderer.material = new Material(shader);

            Color lineColor = color ?? Core.BaseColor;
            lineRenderer.material.color = lineColor;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;

            lineRenderer.numCapVertices = 5;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.useWorldSpace = true;
            lineRenderer.alignment = LineAlignment.View;

            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.allowOcclusionWhenDynamic = false;

            lineRenderer.enabled = true;
            lineObject.SetActive(true);

            return new Line { gameObject = lineObject, lineRenderer = lineRenderer };
        }

        public static Line CreateContinuousLine(Vector3[] points, float width = 0.01f, Color? color = null, bool loop = false)
        {
            GameObject lineObject = new GameObject("ContinuousLine");
            lineObject.layer = 0;

            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

            Shader shader = Core.GuiTextShader ?? Shader.Find("Unlit/Color");
            lineRenderer.material = new Material(shader);

            Color lineColor = color ?? Core.BaseColor;
            lineRenderer.material.color = lineColor;

            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);

            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;

            lineRenderer.numCapVertices = 5;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.useWorldSpace = true;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.loop = loop;

            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.allowOcclusionWhenDynamic = false;

            lineRenderer.enabled = true;
            lineObject.SetActive(true);

            return new Line { gameObject = lineObject, lineRenderer = lineRenderer };
        }

        public static void DeleteLine(Line line)
        {
            if (line != null)
                line.Destroy();
        }

        public static GameObject CreateSphere(Vector3 position, float size, Color color, bool pulse = false)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * size;
            sphere.GetComponent<Renderer>().material.shader = Core.UberShader;
            sphere.GetComponent<Renderer>().material.color = color;
            GameObject.Destroy(sphere.GetComponent<Collider>());

            if (pulse)
            {
                var coroutine = sphere.AddComponent<LineLib>().StartCoroutine(PulseSphere(sphere, size));
                activePulseCoroutines[sphere] = coroutine;
            }

            return sphere;
        }

        public static void DestroySphere(GameObject sphere)
        {
            if (sphere != null)
            {
                activePulseCoroutines.Remove(sphere);
                GameObject.Destroy(sphere);
            }
        }

        private static System.Collections.IEnumerator PulseSphere(GameObject sphere, float baseSize)
        {
            Vector3 originalScale = sphere.transform.localScale;
            while (sphere != null)
            {
                float scaleFactor = 1 + Mathf.Sin(Time.time * 2f) * 0.03f;
                sphere.transform.localScale = originalScale * scaleFactor;
                yield return null;
            }
        }

        public static void DrawLine(Vector3 start, Vector3 end, float width = 0.01f)
        {
            GameObject lineObject = new GameObject("TempLine");
            lineObject.layer = 0;

            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

            Shader shader = Core.GuiTextShader ?? Shader.Find("Unlit/Color");
            lineRenderer.material = new Material(shader);
            lineRenderer.material.color = Core.BaseColor;

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.startColor = Core.BaseColor;
            lineRenderer.endColor = Core.BaseColor;

            lineRenderer.numCapVertices = 5;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.useWorldSpace = true;

            lineRenderer.enabled = true;
            lineObject.SetActive(true);

            GameObject.Destroy(lineObject, Time.deltaTime);
        }
    }
}
