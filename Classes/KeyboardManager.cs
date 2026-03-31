using System;
using UnityEngine;

namespace Juul
{
    public class KeyboardManager
    {
        public static GameObject KeyboardObj = null;
        public static bool IsJoiningRoom = false;
        public static bool WasJoiningRoomLastFrame = false;
        public static string JoinRoomQuery = "";

        private static readonly string[][] KeyLayout = new string[][]
        {
            new string[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
            new string[] { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
            new string[] { "Z", "X", "C", "V", "B", "N", "M" },
            new string[] { "Space", "\b", "Enter" }
        };

        public static void ToggleKeyboard(bool show)
        {
            if (!show)
            {
                if (KeyboardObj != null)
                {
                    if (!KeyboardObj.Equals(null))
                    {
                        GameObject.Destroy(KeyboardObj);
                    }
                    KeyboardObj = null;
                }
                return;
            }

            if (KeyboardObj != null)
            {
                if (KeyboardObj.Equals(null))
                {
                    KeyboardObj = null;
                }
                else
                {
                    return;
                }
            }

            KeyboardObj = new GameObject("SearchKeyboard");

            if (Core.Menu != null)
            {
                KeyboardObj.transform.parent = Core.Menu.transform;
            }
            Canvas kbCanvas = KeyboardObj.AddComponent<Canvas>();
            kbCanvas.renderMode = RenderMode.WorldSpace;
            KeyboardObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.transform.parent = KeyboardObj.transform;
            GameObject.Destroy(bg.GetComponent<Rigidbody>());
            GameObject.Destroy(bg.GetComponent<BoxCollider>());
            float keySize = 0.035f;
            float spacing = 0.005f;
            float kbWidth = (10 * keySize) + (11 * spacing) + 0.04f;
            float totalKeysHeight = (4 * keySize) + (3 * spacing);
            float kbHeight = totalKeysHeight + 0.035f;
            bg.transform.localScale = new Vector3(Core.SmFl, kbWidth, kbHeight);
            bg.transform.localPosition = Vector3.zero;
            bg.transform.localRotation = Quaternion.identity;
            Core.GradientSetter bgGrad = bg.AddComponent<Core.GradientSetter>();
            bgGrad.startOffset = 1.0f;
            bgGrad.gradientOffset = 0.2f;
            if (Core.IsRounded)
            {
                Core.RoundedCorners corners = bg.AddComponent<Core.RoundedCorners>();
                corners.bevel = 0.015f;
            }
            Core.OutlineGradient(bg);
            float startY = (totalKeysHeight / 2f) - (keySize / 2f);            
            for (int r = 0; r < KeyLayout.Length; r++)
            {
                string[] row = KeyLayout[r];
                
                float rowWidth = 0f;
                float[] keyWidths = new float[row.Length];
                for (int c = 0; c < row.Length; c++)
                {
                    float width = keySize;
                    if (row[c] == "Space") width = keySize * 6f;
                    else if (row[c] == "\b") width = keySize * 1.5f;
                    else if (row[c] == "Enter") width = keySize * 2.5f;
                    keyWidths[c] = width;
                    rowWidth += width;
                }
                rowWidth += (row.Length - 1) * spacing;
                float currentX = rowWidth / 2f;
                for (int c = 0; c < row.Length; c++)
                {
                    string key = row[c];
                    float kWidth = keyWidths[c];
                    float xPos = currentX - (kWidth / 2f); 
                    float yPos = startY - (r * (keySize + spacing));
                    
                    Vector3 pos = new Vector3(Core.SmFl + 0.002f, xPos, yPos);
                    CreateKey(KeyboardObj.transform, key, pos, keySize, kWidth);
                    
                    currentX -= (kWidth + spacing);
                }
            }
            float targetX = (Core.SmFl * 40f) + 0.14f;
            float targetZ = -0.45f - (kbHeight / 2f) - 0.13f; 
            KeyboardObj.transform.localPosition = new Vector3(targetX, 0f, targetZ);
            KeyboardObj.transform.localRotation = Quaternion.Euler(0f, -40f, 0f);
        }
        private static void CreateKey(Transform parent, string keyChar, Vector3 localPos, float height, float width)
        {
            GameObject keyObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            keyObj.layer = 2;
            keyObj.transform.parent = parent;            
            keyObj.transform.localScale = new Vector3(Core.SmFl, width, height);
            keyObj.transform.localPosition = localPos;
            keyObj.transform.localRotation = Quaternion.identity;            
            GameObject.Destroy(keyObj.GetComponent<Rigidbody>());
            BoxCollider col = keyObj.GetComponent<BoxCollider>();
            col.isTrigger = true;            
            Core.ColorSetter cs = keyObj.AddComponent<Core.ColorSetter>();
            cs.brightness = Core.OffBrightness;            
            string displayText = keyChar;
            if (keyChar == "\b") displayText = "Back";
            if (keyChar == "Space") displayText = "Space";
            AddTextObj(KeyboardObj.transform, () => displayText, localPos + new Vector3((Core.SmFl / 2f) + 0.001f, 0f, 0f), height * 0.8f);
            ButtonCollider buttonCol = keyObj.AddComponent<ButtonCollider>();
            buttonCol.onClick = () => 
            {
                if (keyChar == "Enter")
                {
                    if (IsJoiningRoom)
                    {
                        GorillaNetworking.PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(JoinRoomQuery, GorillaNetworking.JoinType.Solo);
                        IsJoiningRoom = false; 
                        ToggleKeyboard(false);
                        try { Buttons.RoomJoinerButton.Enabled = false; } catch { }
                    }
                    else
                    {
                        SearchManager.PerformSearch();
                        SearchManager.IsSearching = false; 
                        ToggleKeyboard(false);
                        try { Buttons.GetCategory("Settings").Buttons.Find(b => b.Name == "Configure Search").Enabled = false; } catch { }
                    }
                    Core.RebuildMenu();
                    return;
                }
                if (IsJoiningRoom)
                {
                    if (keyChar == "\b")
                    {
                        if (JoinRoomQuery.Length > 0)
                            JoinRoomQuery = JoinRoomQuery.Substring(0, JoinRoomQuery.Length - 1);
                    }
                    else if (keyChar == "Space") JoinRoomQuery += " ";
                    else JoinRoomQuery += keyChar.ToUpper();

                    if (Buttons.RoomJoinerButton != null)
                        Buttons.RoomJoinerButton.Name = "Join Room: " + JoinRoomQuery;
                }
                else
                {
                    if (keyChar == "\b")
                    {
                        if (SearchManager.SearchQuery.Length > 0)
                            SearchManager.SearchQuery = SearchManager.SearchQuery.Substring(0, SearchManager.SearchQuery.Length - 1);
                    }
                    else if (keyChar == "Space") SearchManager.SearchQuery += " ";
                    else SearchManager.SearchQuery += keyChar.ToLower();                    
                    SearchManager.PerformSearch();
                }
                Core.RebuildMenu();
            };
        }

        private static void AddTextObj(Transform parent, Func<string> textGetter, Vector3 localPos, float size)
        {
            GameObject textObj = new GameObject("Keyboardtext");
            textObj.transform.parent = parent;            
            UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
            text.font = Core.MenuFont;
            text.fontSize = 40;
            text.alignment = TextAnchor.MiddleCenter;
            text.resizeTextForBestFit = false;
            text.color = Color.white;
            text.horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
            text.verticalOverflow = UnityEngine.VerticalWrapMode.Overflow;
            text.material.renderQueue = 4000;            
            float scaleTweak = 0.0005f;
            RectTransform component = text.GetComponent<RectTransform>();
            component.sizeDelta = new Vector2(0.35f / scaleTweak, (0.035f * size * 20f) / scaleTweak); 
            component.transform.localScale = Vector3.one * scaleTweak;
            component.localPosition = localPos;
            component.localRotation = Quaternion.Euler(180f, 90f, 90f);            
            Updater updater = textObj.AddComponent<Updater>();
            updater.getter = textGetter;
            updater.textComponent = text;
        }

        public class Updater : MonoBehaviour
        {
            public Func<string> getter;
            public UnityEngine.UI.Text textComponent;
            void Update()
            {
                if (textComponent != null && getter != null)
                {
                    string newText = getter();
                    if (textComponent.text != newText)
                    {
                        textComponent.text = newText;
                    }
                }
            }
        }
    }
}
