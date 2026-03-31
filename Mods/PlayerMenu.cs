using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Juul
{
    public class PlayerMenu
    {
        public static VRRig SelectedPlayerRig = null;
        public static bool IsInPlayerInfo = false;
        public static bool IsSpectating = false;
        public static GameObject SpectatePanel = null;
        public static GameObject SpectateOutline = null;
        public static Camera SpectateCamera = null;
        public static RenderTexture SpectateTex = null;
        private static float playerListRefreshTimer = 0f;
        private static Category playersCategory = null;
        private static bool needsInitialPopulate = true;

        public static Category GetPlayersCategory()
        {
            if (playersCategory == null)
            {
                playersCategory = new Category { Name = "Players" };
                RefreshPlayerList();
            }
            return playersCategory;
        }

        public static void Tick()
        {
            if (playersCategory == null) return;
            if (Core.ActiveCategory != playersCategory) return;

            if (IsInPlayerInfo) return;

            if (needsInitialPopulate || playersCategory.Buttons.Count == 0)
            {
                needsInitialPopulate = false;
                playerListRefreshTimer = 0f;
                RefreshPlayerList();
                Core.RebuildMenu();
                return;
            }

            playerListRefreshTimer += Time.deltaTime;
            if (playerListRefreshTimer < 2.0f) return;
            playerListRefreshTimer = 0f;

            int oldCount = playersCategory.Buttons.Count;
            RefreshPlayerList();
            if (playersCategory.Buttons.Count != oldCount)
                Core.RebuildMenu();
        }

        public static void RefreshPlayerList()
        {
            if (playersCategory == null) return;

            playersCategory.Buttons.Clear();
            playersCategory.Subcategories.Clear();

            if (!PhotonNetwork.InRoom)
            {
                playersCategory.Buttons.Add(new Button
                {
                    Name = "Not in a room",
                    Toggle = false,
                    Label = true
                });
                return;
            }

            var otherPlayers = PhotonNetwork.PlayerListOthers;
            if (otherPlayers == null || otherPlayers.Length == 0)
            {
                playersCategory.Buttons.Add(new Button
                {
                    Name = "No other players",
                    Toggle = false,
                    Label = true
                });
                return;
            }

            foreach (Player player in otherPlayers)
            {
                string displayName = string.IsNullOrEmpty(player.NickName) ? "Unknown" : player.NickName;
                Player capturedPlayer = player;

                playersCategory.Buttons.Add(new Button
                {
                    Name = displayName,
                    Toggle = false,
                    OnEnable = () => OpenPlayerInfo(capturedPlayer)
                });
            }
        }

        public static void OpenPlayerInfo(Player player)
        {
            if (player == null) return;

            VRRig rig = Rigs.GetVRRigFromPlayer(player);
            SelectedPlayerRig = rig;
            IsInPlayerInfo = true;

            Category infoCategory = new Category
            {
                Name = player.NickName ?? "Player",
                ParentCategory = playersCategory
            };

            infoCategory.Buttons.Add(new Button
            {
                Name = "<< Back",
                Toggle = false,
                OnEnable = () => GoBackToPlayerList()
            });

            infoCategory.Buttons.Add(new Button
            {
                Name = $"Name: {player.NickName ?? "Unknown"}",
                Toggle = false,
                Label = true
            });

            string colorCode = "N/A";
            if (rig != null)
            {
                Color c = rig.playerColor;
                colorCode = GetColorCode(c);
            }
            infoCategory.Buttons.Add(new Button
            {
                Name = $"Color: {colorCode}",
                Toggle = false,
                Label = true
            });

            string pfid = player.UserId ?? "N/A";
            infoCategory.Buttons.Add(new Button
            {
                Name = $"ID: {pfid}",
                Toggle = false,
                Label = true
            });

            infoCategory.Buttons.Add(new Button
            {
                Name = $"Teleport To {player.NickName ?? "Player"}",
                Toggle = false,
                OnEnable = () => TeleportToPlayer(rig)
            });

            infoCategory.Buttons.Add(new Button
            {
                Name = "Spectate",
                Toggle = true,
                OnceEnable = () => StartSpectating(rig),
                OnceDisable = () => StopSpectating()
            });

            infoCategory.Buttons.Add(new Button
            {
                Name = "Lag Player",
                Toggle = true,
                OnEnable = () =>
                {
                    if (rig != null) Overpowered.LagPlayer(rig);
                }
            });

            Core.ActiveCategory = infoCategory;
            Core.CurrentPage = 0;
            Core.RebuildMenu();
        }

        public static void GoBackToPlayerList()
        {
            IsInPlayerInfo = false;
            SelectedPlayerRig = null;
            StopSpectating();
            needsInitialPopulate = true;
            Core.ActiveCategory = playersCategory;
            Core.CurrentPage = 0;
            RefreshPlayerList();
            Core.RebuildMenu();
        }

        public static void TeleportToPlayer(VRRig rig)
        {
            if (rig == null || rig.transform == null) return;
            try
            {
                Movement.TeleportPlayer(rig.transform.position);
            }
            catch { }
        }

        public static void StartSpectating(VRRig rig)
        {
            if (rig == null) return;
            IsSpectating = true;
            SelectedPlayerRig = rig;

            if (SpectateTex == null)
                SpectateTex = new RenderTexture(512, 512, 16);

            if (SpectateCamera == null)
            {
                GameObject camObj = new GameObject("JuulSpectateCamera");
                SpectateCamera = camObj.AddComponent<Camera>();
                SpectateCamera.targetTexture = SpectateTex;
                SpectateCamera.fieldOfView = 60f;
                SpectateCamera.nearClipPlane = 0.1f;
                SpectateCamera.farClipPlane = 1000f;
                SpectateCamera.depth = -5;
                SpectateCamera.enabled = true;
            }
        }

        public static void StopSpectating()
        {
            IsSpectating = false;

            if (SpectateCamera != null)
            {
                UnityEngine.Object.Destroy(SpectateCamera.gameObject);
                SpectateCamera = null;
            }
            if (SpectateTex != null)
            {
                SpectateTex.Release();
                UnityEngine.Object.Destroy(SpectateTex);
                SpectateTex = null;
            }
            if (SpectatePanel != null)
            {
                UnityEngine.Object.Destroy(SpectatePanel);
                SpectatePanel = null;
            }
            if (SpectateOutline != null)
            {
                UnityEngine.Object.Destroy(SpectateOutline);
                SpectateOutline = null;
            }
        }

        public static void UpdateSpectate()
        {
            if (!IsSpectating || SpectateCamera == null || SelectedPlayerRig == null) return;

            Vector3 targetPos = SelectedPlayerRig.transform.position;
            Vector3 targetForward = SelectedPlayerRig.head != null && SelectedPlayerRig.head.rigTarget != null
                ? SelectedPlayerRig.head.rigTarget.forward
                : SelectedPlayerRig.transform.forward;
            targetForward.y = 0;
            targetForward.Normalize();
            if (targetForward.sqrMagnitude < 0.001f) targetForward = Vector3.forward;

            Vector3 camPos = targetPos - targetForward * 2.5f + Vector3.up * 1.5f;
            SpectateCamera.transform.position = Vector3.Lerp(SpectateCamera.transform.position, camPos, Time.deltaTime * 6f);
            SpectateCamera.transform.LookAt(targetPos + Vector3.up * 0.5f);

            if (Core.Menu != null)
            {
                if (SpectatePanel == null)
                {
                    SpectatePanel = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    SpectatePanel.name = "SpectateView";
                    SpectatePanel.transform.parent = Core.Menu.transform;
                    UnityEngine.Object.Destroy(SpectatePanel.GetComponent<Collider>());

                    Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                    mat.mainTexture = SpectateTex;
                    mat.color = Color.white;
                    SpectatePanel.GetComponent<Renderer>().material = mat;
                }

                if (SpectateOutline == null)
                {
                    SpectateOutline = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    SpectateOutline.name = "SpectateOutline";
                    SpectateOutline.transform.parent = Core.Menu.transform;
                    UnityEngine.Object.Destroy(SpectateOutline.GetComponent<Collider>());
                    var gs = SpectateOutline.AddComponent<Core.GradientSetter>();
                    if (Core.Frame != null)
                    {
                        var frameGs = Core.Frame.GetComponent<Core.GradientSetter>();
                        if (frameGs != null)
                        {
                            gs.gradientOffset = frameGs.gradientOffset;
                            gs.startOffset = frameGs.startOffset;
                        }
                    }
                }

                float sideOffset = Core.IsCatLeft
                    ? ((Core.Frame.transform.localScale.y / 2f) + 0.25f)
                    : -((Core.Frame.transform.localScale.y / 2f) + 0.25f);

                SpectatePanel.transform.localScale = new Vector3(0.45f, 0.45f, 0.001f);
                SpectatePanel.transform.localPosition = new Vector3(
                    Core.SmFl * 41f,
                    sideOffset,
                    0f
                );
                SpectatePanel.transform.localRotation = Quaternion.Euler(180f, 90f, 90f);

                SpectateOutline.transform.localScale = new Vector3(0.47f, 0.47f, 0.001f);
                SpectateOutline.transform.localPosition = new Vector3(
                    Core.SmFl * 40.5f,
                    sideOffset,
                    0f
                );
                SpectateOutline.transform.localRotation = Quaternion.Euler(180f, 90f, 90f);
            }
        }

        private static string GetColorCode(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 9f);
            int g = Mathf.RoundToInt(color.g * 9f);
            int b = Mathf.RoundToInt(color.b * 9f);
            return $"{r} {g} {b}";
        }
    }
}
