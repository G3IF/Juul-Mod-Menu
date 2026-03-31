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
                    return;
                MonkeAgent.instance.rpcErrorMax = int.MaxValue;
                MonkeAgent.instance.rpcCallLimit = int.MaxValue;
                MonkeAgent.instance.logErrorMax = int.MaxValue;
                PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                PhotonNetwork.QuickResends = int.MaxValue;
                var peer = PhotonNetwork.NetworkingClient?.LoadBalancingPeer;
                if (peer != null)
                {
                    peer.SentCountAllowance = int.MaxValue;
                    peer.QuickResendAttempts = 3;
                    peer.CrcEnabled = false;
                    peer.UseByteArraySlicePoolForEvents = false;
                    peer.TrafficStatsEnabled = false;
                    peer.TrafficStatsReset();
                    peer.SendOutgoingCommands();
                    try
                    {
                        var type = peer.GetType();
                        var queueField = type.GetField("outgoingStreamQueue", BindingFlags.NonPublic | BindingFlags.Instance);
                        var queue = queueField?.GetValue(peer) as System.Collections.IList;
                        queue?.Clear();
                        var commandsField = type.GetField("commandList", BindingFlags.NonPublic | BindingFlags.Instance);
                        var commands = commandsField?.GetValue(peer) as System.Collections.IList;
                        commands?.Clear();
                        var resentField = type.GetField("resentCommandsCount", BindingFlags.NonPublic | BindingFlags.Instance);
                        resentField?.SetValue(peer, 0);
                    }
                    catch { }
                }
                PhotonNetwork.NetworkStatisticsEnabled = false;
                ValueTuple<Type, object, string, bool>[] targets = new ValueTuple<Type, object, string, bool>[]
                {
                    (gorillaNotType, gorillaInstance, "rpcErrorMax", false),
                    (gorillaNotType, gorillaInstance, "rpcCallLimit", false),
                    (gorillaNotType, gorillaInstance, "logErrorMax", false),
                    (gorillaNotType, gorillaInstance, "userRPCCalls", false),
                    (gorillaNotType, gorillaInstance, "_sendReport", false),
                    (typeof(PhotonNetwork), null, "QuickResends", true),
                    (typeof(PhotonNetwork), null, "MaxResendsBeforeDisconnect", true)
                };
                foreach (var entry in targets)
                    TrySetMember(entry.Item1, entry.Item2, entry.Item3, GetDefaultValue(entry.Item3), entry.Item4);
                try
                {
                    var userRPCCallsField = gorillaNotType.GetField("userRPCCalls", BindingFlags.NonPublic | BindingFlags.Instance);
                    var userRPCCalls = userRPCCallsField?.GetValue(gorillaInstance) as System.Collections.IDictionary;
                    userRPCCalls?.Clear();
                }
                catch { }
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
                        PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
                        Traverse yeah = Traverse.Create(typeof(PhotonNetwork));
                        yeah.Property("ResentReliableCommands").SetValue(0);
                        PhotonNetwork.NetworkingClient.Service();
                        PhotonNetwork.NetworkingClient.OpChangeGroups(null, new byte[] { 1, 2, 3, 4 });
                        PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsReset();
                        try
                        {
                            var system = AppDomain.CurrentDomain.GetAssemblies()
                                .First(a => a.GetName().Name == "Assembly-CSharp")
                                .GetType("RoomSystem");

                            system.GetMethod("OnPlayerLeftRoom", BindingFlags.NonPublic | BindingFlags.Instance)
                                .Invoke(null, new object[] { NetworkSystem.Instance.LocalPlayer });
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
                        try
                        {
                            var photonViewList = typeof(PhotonNetwork).GetField("photonViewList",
                                BindingFlags.NonPublic | BindingFlags.Static);
                            var viewDict = photonViewList?.GetValue(null) as System.Collections.IDictionary;
                            if (viewDict != null)
                            {
                                var keysToRemove = new System.Collections.ArrayList();
                                foreach (System.Collections.DictionaryEntry entry in viewDict)
                                {
                                    var view = entry.Value as PhotonView;
                                    if (view != null && view.IsMine && view.isRuntimeInstantiated)
                                        keysToRemove.Add(entry.Key);
                                }
                                foreach (var key in keysToRemove)
                                    viewDict.Remove(key);
                            }
                        }
                        catch { }
                    }
                    catch { }
                }
                MethodInfo refresh = gorillaNotType.GetMethod("RefreshRPCs", BindingFlags.NonPublic | BindingFlags.Instance);
                refresh?.Invoke(gorillaInstance, null);
            }
            catch { }
        }
        private static byte[] cachedSerializedRpc;

        private static void AntiRPCKicker()
        {
            for (int i = 0; i < 1300; i++)
                ResendCachedRpc();
            try
            {
                var peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
                var field = peer.GetType().GetField("outgoingStreamQueue", BindingFlags.Instance | BindingFlags.NonPublic);

                if (field != null)
                {
                    IList list = field.GetValue(peer) as IList;
                    if (list != null && list.Count > 0)
                        cachedSerializedRpc = list[list.Count - 1] as byte[];
                }
            }
            catch
            {
                cachedSerializedRpc = null;
            }
        }

        private static void ResendCachedRpc()
        {
            if (cachedSerializedRpc == null)
                return;
            try
            {
                var peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
                var type = peer.GetType();
                var method = type.GetMethod("SendReliable", BindingFlags.Instance | BindingFlags.NonPublic)
                            ?? type.GetMethod("SendUnreliable", BindingFlags.Instance | BindingFlags.NonPublic);
                method?.Invoke(peer, new object[] { cachedSerializedRpc });
            }
            catch
            {
                SetTick(9999f);
            }
        }

        public static void SetTick(float tickMultiplier)
        {
            var photonMono = GameObject.Find("PhotonMono")?.GetComponent<PhotonHandler>();
            if (photonMono != null)
            {
                Traverse.Create(photonMono).Field("nextSendTickCountOnSerialize").SetValue((int)(Time.realtimeSinceStartup * tickMultiplier));
                PhotonHandler.SendAsap = true;
            }
        }

        private static bool TrySetMember(Type type, object instance, string fieldName, object value, bool isStatic)
        {
            try
            {
                var field = type.GetField(fieldName,
                    (isStatic ? BindingFlags.Static : BindingFlags.Instance) |
                    BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(instance, value);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static object GetDefaultValue(string fieldName)
        {
            if (fieldName.Contains("Max") || fieldName.Contains("Limit") || fieldName.Contains("Count"))
                return int.MaxValue;
            if (fieldName.Contains("userRPCCalls"))
                return null;
            if (fieldName.Contains("_sendReport"))
                return false;
            return null;
        }

        public static void RPCProtection()
        {
            AntiRPCKick();
        }

        public static object RunViewUpdate()
        {
            return typeof(PhotonNetwork).GetMethod("RunViewUpdate", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
        }
        private static DateTime lastAntiBanCall = DateTime.MinValue;
        private static readonly TimeSpan antiBanInterval = TimeSpan.FromSeconds(5);
        private static bool initialized;
        private static FieldInfo authContextField;
        private static FieldInfo photonViewListField;
        private static FieldInfo userRPCCallsField;
        private static FieldInfo reportedPlayersField;
        private static FieldInfo sendReportField;
        private static FieldInfo suspiciousPlayerIdField;
        private static FieldInfo suspiciousReasonField;
        private static FieldInfo suspiciousPlayerNameField;
        private static FieldInfo cachedDataField;
        private static FieldInfo monoRPCMethodsCacheField;
        private static MethodInfo clearAllEventsMethod;
        private static FieldInfo staticPlayerField;
        private static FieldInfo requestTimeoutField;
        private static FieldInfo compressApiDataField;
        private static FieldInfo disableFocusTimeCollectionField;
        private static FieldInfo sentCountAllowanceField;
        private static FieldInfo quickResendAttemptsField;
        private static FieldInfo outgoingStreamQueueField;
        public static void InitializeAntiBanHelper()
        {
            if (initialized) return;
            try
            {
                var playFabHttpType = typeof(PlayFabHttp);
                clearAllEventsMethod = playFabHttpType.GetMethod("ClearAllEvents", BindingFlags.Public | BindingFlags.Static);
                authContextField = typeof(PlayFabAuthenticationAPI).GetField("_authenticationContext",
                    BindingFlags.Static | BindingFlags.NonPublic);
                staticPlayerField = typeof(PlayFabSettings).GetField("staticPlayer",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                requestTimeoutField = typeof(PlayFabSettings).GetField("RequestTimeout",
                    BindingFlags.Static | BindingFlags.Public);
                compressApiDataField = typeof(PlayFabSettings).GetField("CompressApiData",
                    BindingFlags.Static | BindingFlags.Public);
                disableFocusTimeCollectionField = typeof(PlayFabSettings).GetField("DisableFocusTimeCollection",
                    BindingFlags.Static | BindingFlags.Public);
                var monkeAgentType = typeof(MonkeAgent);
                userRPCCallsField = monkeAgentType.GetField("userRPCCalls",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                reportedPlayersField = monkeAgentType.GetField("reportedPlayers",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                sendReportField = monkeAgentType.GetField("_sendReport",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                suspiciousPlayerIdField = monkeAgentType.GetField("_suspiciousPlayerId",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                suspiciousPlayerNameField = monkeAgentType.GetField("_suspiciousPlayerName",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                suspiciousReasonField = monkeAgentType.GetField("_suspiciousReason",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var photonNetworkType = typeof(Photon.Pun.PhotonNetwork);
                photonViewListField = photonNetworkType.GetField("photonViewList",
                    BindingFlags.Static | BindingFlags.NonPublic);
                cachedDataField = photonNetworkType.GetField("cachedData",
                    BindingFlags.Static | BindingFlags.NonPublic);
                monoRPCMethodsCacheField = photonNetworkType.GetField("monoRPCMethodsCache",
                    BindingFlags.Static | BindingFlags.NonPublic);
                var peerType = typeof(LoadBalancingPeer);
                sentCountAllowanceField = peerType.GetField("SentCountAllowance",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                quickResendAttemptsField = peerType.GetField("QuickResendAttempts",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                outgoingStreamQueueField = peerType.GetField("outgoingStreamQueue",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                initialized = true;
            }
            catch { }
        }

        public static void AntiBan()
        {
            InitializeAntiBanHelper();
            if (!PhotonNetwork.InRoom) return;
            try
            {
                var instance = MonkeAgent.instance;
                if (instance != null)
                {
                    instance.rpcErrorMax = int.MaxValue;
                    instance.rpcCallLimit = int.MaxValue;
                    instance.logErrorMax = int.MaxValue;
                    userRPCCallsField?.SetValue(instance, new Dictionary<string, Dictionary<string, object>>());
                    reportedPlayersField?.SetValue(instance, new List<string>());
                    sendReportField?.SetValue(instance, false);
                    suspiciousPlayerIdField?.SetValue(instance, "");
                    suspiciousPlayerNameField?.SetValue(instance, "");
                    suspiciousReasonField?.SetValue(instance, "");
                }
                PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                PhotonNetwork.QuickResends = int.MaxValue;
                PhotonNetwork.NetworkStatisticsEnabled = false;
                var peer = PhotonNetwork.NetworkingClient?.LoadBalancingPeer;
                if (peer != null)
                {
                    sentCountAllowanceField?.SetValue(peer, int.MaxValue);
                    quickResendAttemptsField?.SetValue(peer, (byte)3);
                    var queue = outgoingStreamQueueField?.GetValue(peer) as System.Collections.IList;
                    queue?.Clear();
                    var resentField = peer.GetType().GetField("resentCommandsCount", BindingFlags.NonPublic | BindingFlags.Instance);
                    resentField?.SetValue(peer, 0);
                    peer.SendOutgoingCommands();
                }
                PhotonNetwork.SendAllOutgoingCommands();
                photonViewListField?.SetValue(null, Activator.CreateInstance(photonViewListField.FieldType));
                cachedDataField?.SetValue(null, new Dictionary<int, Dictionary<int, Queue<object[]>>>());
                monoRPCMethodsCacheField?.SetValue(null, new Dictionary<Type, List<MethodInfo>>());
                if (DateTime.UtcNow - lastAntiBanCall < antiBanInterval) return;
                lastAntiBanCall = DateTime.UtcNow;
                if (!PhotonNetwork.IsConnected)
                {
                    authContextField?.SetValue(null, null);
                    clearAllEventsMethod?.Invoke(null, null);
                    return;
                }
                if (!PlayFabAuthenticationAPI.IsEntityLoggedIn()) return;
                try
                {
                    clearAllEventsMethod?.Invoke(null, null);
                    requestTimeoutField?.SetValue(null, 30000);
                    compressApiDataField?.SetValue(null, true);
                    disableFocusTimeCollectionField?.SetValue(null, true);
                    var staticPlayer = staticPlayerField?.GetValue(null) as PlayFabAuthenticationContext;
                    staticPlayer?.ForgetAllCredentials();
                }
                catch { }
            }
            catch { }
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

                foreach (var vrrig in from vrrig in VRRigCache.ActiveRigs where !vrrig.isLocal let D1 = Vector3.Distance(vrrig.rightHandTransform.position, report.position) let D2 = Vector3.Distance(vrrig.leftHandTransform.position, report.position) where D1 < 0.65f || D2 < 0.65f select vrrig)
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
        public static void AntiReportNotify()
        {
            AntiReport((vrrig, position) =>
            {
                NotifiLib.SendNotification("You Have Been Reported");
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
        public static void UncapFPS()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = int.MaxValue;
        }
        public static void SetFPS144()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 144;
        }
        public static void SetFPS120()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 120;
        }

        public static void SetFPS90()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 90;
        }

        public static void SetFPS80()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 80;
        }

        public static void SetFPS72()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 72;
        }

        public static void SetFPS60()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        public static void SetFPS45()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 45;
        }

        public static void SetFPS15()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 15;
        }

        public static void SetFPS1()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 1;
        }

        public static void AntiAFKKick()
        {
            PhotonNetworkController.Instance.disableAFKKick = true;
        }
        private static bool spoofingActive = false;
        private static string spoofedPlayFabId;
        private static string spoofedEntityId;
        private static string spoofedEntityToken;
        private static string spoofedSessionTicket;
        private static FieldInfo nicknameField;
        private static FieldInfo userIdField;
        private static Type networkSystemType;
        private static PropertyInfo networkSystemInstanceProperty;
        private static MethodInfo returnToSinglePlayerMethod;
        private static System.Random random = new System.Random();

        public static void InitializePlayerSpoofHelper()
        {
            if (initialized) return;
            try
            {
                authContextField = typeof(PlayFabAuthenticationAPI).GetField("_authenticationContext",
                    BindingFlags.Static | BindingFlags.NonPublic);
                staticPlayerField = typeof(PlayFabSettings).GetField("staticPlayer",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var photonNetworkType = typeof(PhotonNetwork);
                var playerType = typeof(Player);
                nicknameField = playerType.GetField("nickName",
                    BindingFlags.Instance | BindingFlags.Public);
                userIdField = playerType.GetField("userId",
                    BindingFlags.Instance | BindingFlags.Public);
                networkSystemType = Type.GetType("NetworkSystem, Assembly-CSharp");
                if (networkSystemType != null)
                {
                    networkSystemInstanceProperty = networkSystemType.GetProperty("Instance",
                        BindingFlags.Static | BindingFlags.Public);
                    returnToSinglePlayerMethod = networkSystemType.GetMethod("ReturnToSinglePlayer",
                        BindingFlags.Instance | BindingFlags.Public);
                }
                initialized = true;
            }
            catch { }
        }
        private static void ForgetAllPlayFabCredentials()
        {
            try
            {
                var staticPlayer = staticPlayerField?.GetValue(null) as PlayFabAuthenticationContext;
                staticPlayer?.ForgetAllCredentials();
                authContextField?.SetValue(null, null);
                var clearEventsMethod = typeof(PlayFabHttp).GetMethod("ClearAllEvents",
                    BindingFlags.Public | BindingFlags.Static);
                clearEventsMethod?.Invoke(null, null);
                typeof(PlayFabSettings).GetField("DisableFocusTimeCollection",
                    BindingFlags.Static | BindingFlags.Public)?.SetValue(null, true);
                typeof(PlayFabSettings).GetField("DisableAdvertising",
                    BindingFlags.Static | BindingFlags.Public)?.SetValue(null, true);
                typeof(PlayFabSettings).GetField("DisableDeviceInfo",
                    BindingFlags.Static | BindingFlags.Public)?.SetValue(null, true);
            }
            catch { }
        }
        private static void GenerateSpoofedIdentities()
        {
            spoofedPlayFabId = RandomPlayfabID();
            spoofedEntityId = RandomEntityID();
            spoofedEntityToken = RandomToken();
            spoofedSessionTicket = RandomTicket();
        }
        private static void ApplySpoofedIdentity()
        {
            try
            {
                var spoofedContext = new PlayFabAuthenticationContext
                {
                    PlayFabId = spoofedPlayFabId,
                    EntityId = spoofedEntityId,
                    EntityToken = spoofedEntityToken,
                    ClientSessionTicket = spoofedSessionTicket,
                    EntityType = "_GorillaPlayer"
                };
                authContextField?.SetValue(null, spoofedContext);
                staticPlayerField?.SetValue(null, spoofedContext);
                if (PhotonNetwork.LocalPlayer != null)
                {
                    nicknameField?.SetValue(PhotonNetwork.LocalPlayer, "Player_" + UnityEngine.Random.Range(1000, 9999));
                    userIdField?.SetValue(PhotonNetwork.LocalPlayer, spoofedPlayFabId);
                }
            }
            catch { }
        }
        public static string GetCurrentSpoofedPlayFabId() => spoofedPlayFabId;
        public static bool IsSpoofingActive() => spoofingActive;
        private static string RandomPlayfabID()
        {
            const string chars = "0123456789ABCDEF";
            string id = "";
            for (int i = 0; i < 16; i++)
            {
                id += chars[random.Next(chars.Length)];
            }
            return id;
        }
        private static string RandomEntityID()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
        }
        private static string RandomToken()
        {
            byte[] bytes = new byte[32];
            random.NextBytes(bytes);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
        private static string RandomTicket()
        {
            return "TICKET_" + Guid.NewGuid().ToString("N").ToUpper();
        }
        public static void SpoofPlayer()
        {
            InitializePlayerSpoofHelper();
            try
            {
                if (networkSystemInstanceProperty != null && returnToSinglePlayerMethod != null)
                {
                    var instance = networkSystemInstanceProperty.GetValue(null);
                    returnToSinglePlayerMethod.Invoke(instance, null);
                }
                ForgetAllPlayFabCredentials();
                GenerateSpoofedIdentities();
                ApplySpoofedIdentity();
                spoofingActive = true;
            }
            catch { }
        }

    }
}
