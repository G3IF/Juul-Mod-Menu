using ExitGames.Client.Photon;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.Events;
using PlayFab.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unity.XR.CoreUtils;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Object = UnityEngine.Object;

namespace Juul
{
    internal class Safety
    {
        public static void BypassVCBan()
        {
            GorillaTagger.moderationMutedTime = -1f;
            GorillaTelemetry.PostNotificationEvent("Unmute");
            GorillaTagger.Instance.myRecorder.TransmitEnabled = true;
            if (KIDManager.Instance != null)
            {
                GameObject.Destroy(KIDManager.Instance);
            }
        }
        public static void AntiRPCKick()
        {
            try
            {
                AntiRPCKicker();

                Type gorillaNotType = typeof(MonkeAgent);
                MonkeAgent gorillaInstance = MonkeAgent.instance;

                if (gorillaInstance == null)
                {
                    Debug.LogWarning("MonkeAgent.instance is null — cannot clear RPCs.");
                    return;
                }

                MonkeAgent.instance.rpcErrorMax = int.MaxValue;
                MonkeAgent.instance.rpcCallLimit = int.MaxValue;
                MonkeAgent.instance.logErrorMax = int.MaxValue;
                PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                PhotonNetwork.QuickResends = int.MaxValue;

                ValueTuple<Type, object, string, bool>[] targets = new ValueTuple<Type, object, string, bool>[]
                {
                    (gorillaNotType, gorillaInstance, "rpcErrorMax", false),
                    (gorillaNotType, gorillaInstance, "rpcCallLimit", false),
                    (gorillaNotType, gorillaInstance, "logErrorMax", false),
                    (typeof(PhotonNetwork), null, "QuickResends", true),
                    (typeof(PhotonNetwork), null, "MaxResendsBeforeDisconnect", true)
                };

                foreach (ValueTuple<Type, object, string, bool> entry in targets)
                {
                    if (!TrySetIntMember(entry.Item1, entry.Item2, entry.Item3, int.MaxValue, entry.Item4))
                    {
                        Debug.LogWarning(string.Concat(new string[]
                        {
                            "Could not set '",
                            entry.Item3,
                            "' on ",
                            entry.Item1.FullName,
                            "."
                        }));
                    }
                }

                PhotonNetwork.NetworkingClient.OpRaiseEvent(200, new Hashtable()
                {
                    { 0, GorillaTagger.Instance.myVRRig.ViewID }
                }, new RaiseEventOptions
                {
                    CachingOption = (EventCaching)6,
                    TargetActors = new int[] { PhotonNetwork.LocalPlayer.ActorNumber }
                }, SendOptions.SendReliable);

                if (Time.time > rpcDel)
                {
                    try
                    {
                        rpcDel = Time.time + 0.47f;
                        PhotonNetwork.RemoveBufferedRPCs(int.MaxValue, null, null);
                        PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
                        PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
                        PhotonNetwork.OpCleanRpcBuffer(GorillaTagger.Instance.myVRRig.GetView);
                        MonkeAgent.instance.OnPlayerLeftRoom(PhotonNetwork.LocalPlayer);
                        PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();

                        Traverse yeah = Traverse.Create(typeof(PhotonNetwork));
                        yeah.Property("ResentReliableCommands").SetValue(0);

                        PhotonNetwork.NetworkingClient.Service();
                        PhotonNetwork.NetworkingClient.OpChangeGroups(null, new byte[] { 1, 2, 3, 4 });
                        PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsReset();

                        try
                        {
                            var system = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Assembly-CSharp").GetType("RoomSystem");
                            system.GetMethod("OnPlayerLeftRoom", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(null, new object[] { NetworkSystem.Instance.LocalPlayer });
                        }
                        catch { }

                        try
                        {
                            NetSystemState state = new NetSystemState();
                            PeerStateValue val = new PeerStateValue();
                            state.Equals(NetSystemState.Connecting);
                            val.Equals(PeerStateValue.Connected);
                            RunViewUpdate();
                        }
                        catch { }

                        PhotonNetwork.SendAllOutgoingCommands();
                    }
                    catch { }
                }

                MethodInfo refresh = gorillaNotType.GetMethod("RefreshRPCs", BindingFlags.NonPublic | BindingFlags.Instance);
                if (refresh != null)
                {
                    refresh.Invoke(gorillaInstance, null);
                }
            }
            catch { }
        }

        private static void AntiRPCKicker()
        {
            for (int i = 0; i < 1300; i++)
            {
                ResendCachedRpc();
            }

            try
            {
                if (PhotonNetwork.NetworkingClient.LoadBalancingPeer.GetType().GetField("outgoingStreamQueue", BindingFlags.Instance | BindingFlags.NonPublic) != null)
                {
                    IList list = PhotonNetwork.NetworkingClient.LoadBalancingPeer.GetType().GetField("outgoingStreamQueue", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(PhotonNetwork.NetworkingClient.LoadBalancingPeer) as IList;
                    if (PhotonNetwork.NetworkingClient.LoadBalancingPeer.GetType().GetField("outgoingStreamQueue", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(PhotonNetwork.NetworkingClient.LoadBalancingPeer) as IList != null && list.Count > 0)
                    {
                        Debug.Log(list[list.Count - 1]);
                        cachedSerializedRpc = list[list.Count - 1] as byte[];
                    }
                }
            }
            catch
            {
                cachedSerializedRpc = null;
            }
        }

        private static void ResendCachedRpc()
        {
            if (cachedSerializedRpc != null)
            {
                try
                {
                    if (PhotonNetwork.NetworkingClient.LoadBalancingPeer.GetType().GetMethod("SendReliable", BindingFlags.Instance | BindingFlags.NonPublic) != null)
                    {
                        PhotonNetwork.NetworkingClient.LoadBalancingPeer.GetType().GetMethod("SendReliable", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(PhotonNetwork.NetworkingClient.LoadBalancingPeer, new object[] { cachedSerializedRpc });
                    }
                    else if (PhotonNetwork.NetworkingClient.LoadBalancingPeer.GetType().GetMethod("SendUnreliable", BindingFlags.Instance | BindingFlags.NonPublic) != null)
                    {
                        PhotonNetwork.NetworkingClient.LoadBalancingPeer.GetType().GetMethod("SendUnreliable", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(PhotonNetwork.NetworkingClient.LoadBalancingPeer, new object[] { cachedSerializedRpc });
                    }
                }
                catch
                {
                    Console.WriteLine("if u managed to get here then u broke the code or u retard");
                    SetTick(9999f);
                }
            }
        }

        public static void SetTick(float tickMultiplier)
        {
            if (GameObject.Find("PhotonMono") != null ? GameObject.Find("PhotonMono").GetComponent<PhotonHandler>() : null != null)
            {
                Traverse.Create(GameObject.Find("PhotonMono") != null ? GameObject.Find("PhotonMono").GetComponent<PhotonHandler>() : null).Field("nextSendTickCountOnSerialize").SetValue((int)(Time.realtimeSinceStartup * tickMultiplier));
                PhotonHandler.SendAsap = true;
            }
        }

        private static bool TrySetIntMember(Type type, object targetInstance, string name, int value, bool isStatic)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            FieldInfo fi = type.GetField(name, flags);
            if (fi != null && fi.FieldType == typeof(int))
            {
                fi.SetValue(isStatic ? null : targetInstance, value);
                return true;
            }
            else
            {
                PropertyInfo pi = type.GetProperty(name, flags);
                if (pi != null && pi.PropertyType == typeof(int) && pi.CanWrite)
                {
                    pi.SetValue(isStatic ? null : targetInstance, value, null);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static void RPCProtection()
        {
            AntiRPCKick();
        }

        public static object RunViewUpdate()
        {
            return typeof(PhotonNetwork).GetMethod("RunViewUpdate", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
        }

        public static void AntiBan() // might not work, last tested in decemeber
        {
            if (PhotonNetwork.InRoom)
            {
                if (!Safety.IsRPCPatched)
                {
                    MonkeAgent.instance.rpcErrorMax = int.MaxValue;
                    MonkeAgent.instance.rpcCallLimit = int.MaxValue;
                    MonkeAgent.instance.logErrorMax = int.MaxValue;
                    PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                    PhotonNetwork.QuickResends = int.MaxValue;
                    PhotonNetwork.SendAllOutgoingCommands();
                    Safety.IsRPCPatched = true;
                }
                else
                {
                    LoadBalancingPeer peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
                    Type type = peer.GetType();
                    FieldInfo field = type.GetField("ResentReliableCommands", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field != null)
                    {
                        field.SetValue(peer, 0);
                    }
                    MethodInfo methodClearQueue = type.GetMethod("ClearReliableChannel", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (methodClearQueue != null)
                    {
                        methodClearQueue.Invoke(peer, null);
                    }
                    peer = null;
                    type = null;
                    field = null;
                    methodClearQueue = null;

                    if (DateTime.UtcNow - lastAntiBanCall < antiBanInterval) return;
                    lastAntiBanCall = DateTime.UtcNow;
                    if (!PhotonNetwork.IsConnected)
                    {
                        mockContext = null;
                        typeof(PlayFabAuthenticationAPI).GetField("_authenticationContext", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, null);
                        PlayFabHttp.ClearAllEvents();
                        return;
                    }
                    if (!PlayFabAuthenticationAPI.IsEntityLoggedIn()) return;
                    if (mockContext == null)
                    {
                        mockContext = new PlayFabAuthenticationContext
                        {
                            ClientSessionTicket = Guid.NewGuid().ToString(),
                            EntityId = "MOCK_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                            PlayFabId = "MOCK_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                            EntityToken = "MOCK_" + Guid.NewGuid().ToString(),
                            EntityType = "_GorillaPlayer"
                        };
                    }
                    try
                    {
                        PlayFabAuthenticator.instance.mothershipAuthenticator.MaxMetaLoginAttempts = int.MaxValue;
                        PlayFabAuthenticator.instance.mothershipAuthenticator.BeginLoginFlow();
                        PlayFabAuthenticatorSettings.TitleId = "A-X^0";
                        PlayFabClientAPI.ExecuteCloudScript(new PlayFab.ClientModels.ExecuteCloudScriptRequest
                        {
                            FunctionName = "A-x^0",
                            GeneratePlayStreamEvent = false,
                            AuthenticationContext = mockContext,
                            FunctionParameter = new Dictionary<string, object>
                        {
                            { "AuthenticateWithPlayFab", true },
                            { "OnSerialize", false },
                            { "OnEnable", false }
                        }

                        }, result => { }, error => { });
                        PlayFabHttp.CreateInstance();
                        PlayFabHttp.ClearAllEvents();
                        typeof(PlayFabEvents).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
                        typeof(PlayFabAuthenticationAPI).GetField("_authenticationContext", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, mockContext);
                        PlayFabSettings.RequestTimeout = 30000;
                        PlayFabSettings.CompressApiData = true;
                    }
                    catch { }
                }
            }
        }

        public static void NoFinger()
        {
            ControllerInputPoller.instance.leftControllerGripFloat = 0f;
            ControllerInputPoller.instance.rightControllerGripFloat = 0f;
            ControllerInputPoller.instance.leftControllerIndexFloat = 0f;
            ControllerInputPoller.instance.rightControllerIndexFloat = 0f;
        }

        public static void RestartGame()
        {
            Process.Start("steam://rungameid/1533390");
            Application.Quit();
        }

        public static void QuitGame()
        {
            Application.Quit();
        }

        public static void DisconnectLT()
        {
            if (PhotonNetwork.InRoom)
            {
                if (ControllerInputPoller.instance.leftControllerIndexFloat > 1f)
                {
                    PhotonNetwork.Disconnect();
                }
            }
        }

        public static void DisconnectRT()
        {
            if (PhotonNetwork.InRoom)
            {
                if (ControllerInputPoller.instance.rightControllerIndexFloat > 1f)
                {
                    PhotonNetwork.Disconnect();
                }
            }
        }

        public static void DisableNetworkTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab").SetActive(false);
        }

        public static void EnableNetworkTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab").SetActive(true);
        }

        public static void DisableMapTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab").SetActive(false);
        }

        public static void EnableMapTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab").SetActive(true);
        }

        public static void DisableQuitBox()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox").SetActive(false);
        }

        public static void EnableQuitBox()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox").SetActive(true);
        }

        public static void EnableAntiAFK()
        {
            PhotonNetworkController.Instance.disableAFKKick = true;
        }

        public static void DisableAntiAFK()
        {
            PhotonNetworkController.Instance.disableAFKKick = false;
        }

        public static void JoinRandom()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.Disconnect();
            }
            else
            {
                string text = PhotonNetworkController.Instance.currentJoinTrigger == null ? "forest" : PhotonNetworkController.Instance.currentJoinTrigger.networkZone;
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.GetJoinTriggerForZone(text), 0);
            }
        }

        public static void JoinRoom(string RoomCode)
        {
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(RoomCode, 0);
        }

        public static VRRig reportRig;
        public static void AntiReport(System.Action<VRRig, Vector3> onReport)
        {
            if (!NetworkSystem.Instance.InRoom) return;

            if (reportRig != null)
            {
                onReport?.Invoke(reportRig, reportRig.transform.position);
                reportRig = null;
                return;
            }

            foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer != NetworkSystem.Instance.LocalPlayer) continue;
                Transform report = line.reportButton.gameObject.transform;

                foreach (var vrrig in from vrrig in GorillaParent.instance.vrrigs where !vrrig.isLocal let D1 = Vector3.Distance(vrrig.rightHandTransform.position, report.position) let D2 = Vector3.Distance(vrrig.leftHandTransform.position, report.position) where D1 < 0.65f || D2 < 0.65f select vrrig)
                    onReport?.Invoke(vrrig, report.transform.position);
            }
        }

        public static float antiReportDelay;
        public static void AntiReportDisconnect()
        {
            AntiReport((vrrig, position) =>
            {
                NetworkSystem.Instance.ReturnToSinglePlayer();
            });
        }

        public static void AntiReportReconnect()
        {
            AntiReport((vrrig, position) =>
            {
                string name = PhotonNetwork.CurrentRoom.Name;
                NetworkSystem.Instance.ReturnToSinglePlayer();
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(name, GorillaNetworking.JoinType.Solo);
            });
        }

        static float rpcDel;
        public static bool IsRPCPatched = false;
        public static bool visAntiReport = false;
        private static byte[] cachedSerializedRpc;
        private static DateTime lastAntiBanCall = DateTime.MinValue;
        private static readonly TimeSpan antiBanInterval = TimeSpan.FromSeconds(5);
        private static PlayFabAuthenticationContext mockContext;
    }
}