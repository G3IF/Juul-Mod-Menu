using ExitGames.Client.Photon;
using g3;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Ionic.Zlib;
using Liv.Lck.Tablet;
using Mono.Security.Cryptography;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.EventsModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Technie.PhysicsCreator.Skinned;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static OVRColocationSession;
using static SuperInfectionManager;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.InputSystem.DefaultInputActions;
using Application = UnityEngine.Application;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Text = UnityEngine.UI.Text;

namespace Juul
{
    internal class Overpowered
    {
        


       

        public static void GetOwnerOfSIEntity()
        {
            try
            {
                RequestableOwnershipGuard guard = SuperInfectionManager.activeSuperInfectionManager.gameEntityManager.guard;
                guard.SetCreator(NetworkSystem.Instance.LocalPlayer);
                guard.SetCurrentOwner(NetworkSystem.Instance.LocalPlayer);
                if (SuperInfectionManager.activeSuperInfectionManager.gameEntityManager.IsAuthorityPlayer(PhotonNetwork.LocalPlayer))
                {
                    NotifiLib.SendNotification("WORKED HAVE FUN!!!");
                }
            }
            catch(Exception e) { Debug.LogException(e); }
        }
      
        public static void DebugSpawnedEntitys()
        {
            if (PhotonNetwork.InRoom)
            {
                if (GameMode.IsPlaying(GameModeType.SuperInfect))
                {
                    foreach (GameEntity game in SuperInfectionManager.activeSuperInfectionManager.gameEntityManager.GetGameEntities())
                    {
                        Debug.Log($"name : {game.gameObject.name}\n");
                        Debug.Log($"id : {game.typeId}\n ");
                    }
                }
            }
        }

        public static float delay = 0f;
        public static System.Random rand = new System.Random();
        public static object[] Reportdata = new object[6];

        public static SendOptions STS() => new SendOptions { Encrypt = true, DeliveryMode = DeliveryMode.ReliableUnsequenced, Reliability = true };

        public static DeployedChild GetChild(DeployableObject obj) => Traverse.Create(obj).Field("_child").GetValue<DeployedChild>();

        public static RaiseEventOptions TargetedWCO(int actor, EventCaching cache = EventCaching.AddToRoomCacheGlobal)
            => new RaiseEventOptions { CachingOption = cache, TargetActors = new[] { actor } };

        public static void InvokeOnDeployMultiple(DeployableObject deployable, int times = 5)
        {
            UnityEvent onDeploy = Traverse.Create(deployable).Field("_onDeploy").GetValue<UnityEvent>();
            for (int i = 0; i < times; i++) onDeploy.Invoke();
        }

        public static void AddBarrel()
        {
            CosmeticsController.instance.currentCart.Insert(0, CosmeticsController.instance.GetItemFromDict("LMAPE."));
        }
        
        
   

        public static void SpamDeploythxboowomp(DeployableObject deployable)
        {
            UnityEvent value = Traverse.Create(deployable).Field("_onDeploy").GetValue<UnityEvent>();
            for (int i = 0; i > 2; i++)
            {
                value.Invoke();
            }
        }
        public static void SpamDeploythxboowompCrash(DeployableObject deployable)
        {
            UnityEvent value = Traverse.Create(deployable).Field("_onDeploy").GetValue<UnityEvent>();
            for (int i = 0; i > 1; i++)
            {
                value.Invoke();
            }
        }
        public static void FlingWithElff(VRRig who)
        {
            DeployableObject deployable = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.R/upper_arm.R/forearm.R/Right Arm Item Anchor/DropZoneAnchor/HoldableThrowableBarrelLeprechaun_Anchor(Clone)/LMAPE.").GetComponent<DeployableObject>();
            var signal = Traverse.Create(deployable).Field("_deploySignal").GetValue<PhotonSignal<long, int, long>>();
            var signal2 = deployable._deploySignal;
            DeployedChild child = GetChild(deployable);
            Rigidbody value = Traverse.Create(child).Field("_rigidbody").GetValue<Rigidbody>();
            object[] array = PhotonUtils.FetchScratchArray(5);
            array[0] = deployable._deploySignal._signalID;
            array[1] = PhotonNetwork.ServerTimestamp;
            array[2] = BitPackUtils.PackWorldPosForNetwork(who.bodyTransform.transform.position + new Vector3(0, -0.3f));
            array[3] = 469893376;
            array[4] = BitPackUtils.PackWorldPosForNetwork(who.bodyTransform.transform.up * 9998.99f);
            PhotonNetwork.RaiseEvent(177, array, TargetedWCO(who.Creator.ActorNumber, EventCaching.AddToRoomCacheGlobal), STS());
            child.Deploy(deployable, who.bodyTransform.transform.position + new Vector3(0, -0.3f), Quaternion.identity, who.bodyTransform.transform.up * 9998.99f, false);
            deployable.DeployChild();
            SpamDeploythxboowomp(deployable);
            value.linearDamping = 0f;
            value.angularDamping = 0f;
            value.AddForce(new Vector3(0, 10000, 0), ForceMode.Force);
            value.AddForce(new Vector3(0, 10000, 0), ForceMode.Impulse);
            value.AddForce(new Vector3(0, 10000, 0), ForceMode.VelocityChange);
            value.AddForce(new Vector3(0, 10000, 0), ForceMode.Acceleration);
            value.linearVelocity = who.bodyTransform.transform.up * 9998.99f;
            value.solverVelocityIterations = 9998;
            value.detectCollisions = false;
            value.inertiaTensor = who.bodyTransform.transform.up * 9998.99f;
            value.AddForce(new Vector3(0, 10000, 0), ForceMode.Force);
            value.AddForce(new Vector3(0, 10000, 0), ForceMode.Impulse);
            value.AddForce(new Vector3(0, 10000, 0), ForceMode.VelocityChange);
            value.AddForce(new Vector3(0, 10000, 0), ForceMode.Acceleration);
            child.ReturnToParent(2f);
        }

        public static void ElfFlingGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                FlingWithElff(GunLib.LockedPlayer);
            }, true);
        }

        public static void FlingOnTouch()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig)
                {
                    if (Vector3.Distance(GorillaTagger.Instance.transform.position, vrrig.rightHandTransform.transform.position) < 0.2f || Vector3.Distance(GorillaTagger.Instance.transform.position, vrrig.leftHandTransform.transform.position) < 0.2f)
                    {
                        FlingWithElff(vrrig);
                    }
                }
            }
        }
        public static void FlingOnYourTouch()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && (Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                  || Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                  || Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    FlingWithElff(vrrig);
                }
            }
        }
        public static void DestroyAll()
        {
            if (PhotonNetwork.InRoom)
            {
                foreach(Player p in PhotonNetwork.PlayerListOthers)
                {
                    PhotonNetwork.OpRemoveCompleteCacheOfPlayer(p.ActorNumber);
                }
            }
        }
        public static void DestroyGun() // detceted?
        {
            GunLib.StartPointerSystem(() =>
            {
                PhotonNetwork.OpRemoveCompleteCacheOfPlayer(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber);
            }, true);
        }

        public static void BreakAudioAll()
        {
            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f || Mouse.current.rightButton.isPressed)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.00000000000015f;
                    VRRig.LocalRig.netView.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.Others, new object[]
                    {
                        true,
                        false
                    }, true);
                    VRRig.LocalRig.netView.SendRPC("EnableNonCosmeticHandItemRPC", RpcTarget.Others, new object[]
                    {
                        false,
                        false
                    });
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, 111, false, 999999f);
                    Safety.AntiRPCKick();
                }
            }
        }
        public static void BreakAudioGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.00000000000015f;
                    VRRig.LocalRig.netView.SendRPC("EnableNonCosmeticHandItemRPC", Rigs.GetPlayerFromVRRig(GunLib.LockedPlayer), new object[]
                    {
                       true,
                       false
                     });
                    VRRig.LocalRig.netView.SendRPC("EnableNonCosmeticHandItemRPC", Rigs.GetPlayerFromVRRig(GunLib.LockedPlayer), new object[]
                    {
                       false,
                       false
                    });
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", Rigs.GetPlayerFromVRRig(GunLib.LockedPlayer), 111, false, 999999f);
                    Safety.AntiRPCKick();
                }
            }, true);
        }
        public static void Test()
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;

            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }
        public static void Test2()
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;

            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }

        public static void Test3()
        {
            PhotonNetwork.CurrentRoom.IsVisible = true;

            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }
        public static void Test4()
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;

            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }
        public static void BreakGameMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GorillaTagManager.instance.StopPlaying();
                GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
            }
        }
        public static void FixGamemode()
        {
            GorillaTagManager.instance.StartPlaying();
            GorillaScoreboardTotalUpdater.instance.UpdateActiveScoreboards();
        }
        public static void FreezeRoom()
        {
            foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerListOthers)
            {
                Reportdata[0] = p.UserId;
                Reportdata[1] = int.MaxValue;
                Reportdata[2] = p.NickName;
                Reportdata[3] = p.NickName;
                Reportdata[4] = !NetworkSystem.Instance.SessionIsPrivate;
                Reportdata[5] = NetworkSystem.Instance.RoomStringStripped();

                WebFlags webFlags = new WebFlags(255);

                NetEventOptions netEventOptions = new NetEventOptions() { Flags = webFlags, Reciever = NetEventOptions.RecieverTarget.others };

                if (Time.time > delay)
                {
                    delay = Time.time + 1f;
                    for (int i = 0; i < 11; i++)
                    {
                        NetworkSystemRaiseEvent.RaiseEvent(51, Reportdata, netEventOptions, false);
                    }
                }
            }
        }
        public static void FreezeRoomV2()

        {
            MonkeAgent.instance.suspiciousPlayerId = "☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠";
            MonkeAgent.instance.suspiciousPlayerName = "☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠";
            MonkeAgent  .instance.suspiciousReason = "☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠☠";

            WebFlags flags = new WebFlags(255);
            NetEventOptions options = new NetEventOptions
            {
                Reciever = NetEventOptions.RecieverTarget.master,
                Flags = flags
            };
            string[] array = new string[MonkeAgent.instance.cachedPlayerList.Length];
            int num = 0;
            foreach (NetPlayer netPlayer in MonkeAgent.instance.cachedPlayerList)
            {
                array[num] = netPlayer.UserId;
                num++;
            }
            object[] data = new object[]
            {
            NetworkSystem.Instance.RoomStringStripped(),
            array,
            NetworkSystem.Instance.MasterClient.UserId,
            MonkeAgent.instance.suspiciousPlayerId,
            MonkeAgent.instance.suspiciousPlayerName,
            MonkeAgent.instance.suspiciousReason,
            NetworkSystemConfig.AppVersion
            };

            if (Time.time > delay)
            {
                delay = Time.time + 3f;
                for (int i = 0; i < 200; i++)
                {
                    NetworkSystemRaiseEvent.RaiseEvent(8, data, options, false);
                }
                Safety.AntiRPCKick();
            }
        }
        public static void StutterGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                StutterPlayer(GunLib.LockedPlayer);
            }, true);
        }
        public static void StutterAll()
        {
            foreach (VRRig p in GorillaParent.instance.vrrigs.Where(r => r != VRRig.LocalRig))
            {
                StutterPlayer(p);
            }
        }
        public static void LagGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                LagPlayer(GunLib.LockedPlayer);
            }, true);
        }
        public static void LagAll()
        {
            foreach (VRRig p in GorillaParent.instance.vrrigs.Where(r => r != VRRig.LocalRig))
            {
                LagPlayer(p);
            }
        }
        public static void LagOnYourTouch()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && (Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    LagPlayer(vrrig);
                }
            }
        }
        public static void CrashOnYourTouch()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && ((double)Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    CrashPlayer(vrrig);
                }
            }
        }
        public static void StutterOnYourTouch()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (!vrrig.isMyPlayer && !vrrig.isOfflineVRRig && ((double)Vector3.Distance(vrrig.rightHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.leftHandTransform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5
                   || (double)Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 0.5))
                {
                    StutterPlayer(vrrig);
                }
            }
        }
        public static void LagOnTouch()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f))
                {
                    LagPlayer(vrrig);
                }
            }
        }
        public static void StutterOnTouch()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f))
                {
                    StutterPlayer(vrrig);
                }
            }
        }
        public static void CrashOnTouch()
        {
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if (vrrig != GorillaTagger.Instance.offlineVRRig && (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.headMesh.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f || Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, vrrig.bodyTransform.transform.position) < 0.25f))
                {
                    CrashPlayer(vrrig);
                }
            }
        }
        public static void LagAura()
        {
            List<VRRig> vrriglist = new List<VRRig>();
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if ((Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f && vrrig != GorillaTagger.Instance.offlineVRRig))
                {
                    vrriglist.Add(vrrig);
                }
                foreach (VRRig rigs in vrriglist)
                {
                    LagPlayer(rigs);
                }
            }
        }
        public static void CrashAura()
        {
            List<VRRig> vrriglist = new List<VRRig>();
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if ((Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f && vrrig != GorillaTagger.Instance.offlineVRRig))
                {
                    vrriglist.Add(vrrig);
                }
                foreach (VRRig rigs in vrriglist)
                {
                    CrashPlayer(rigs);
                }
            }
        }
        public static void StutterAura()
        {
            List<VRRig> vrriglist = new List<VRRig>();
            foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
            {
                if ((Vector3.Distance(vrrig.transform.position, GorillaTagger.Instance.offlineVRRig.transform.position) <= 3.54f && vrrig != GorillaTagger.Instance.offlineVRRig))
                {
                    vrriglist.Add(vrrig);
                }
                foreach (VRRig rigs in vrriglist)
                {
                    StutterPlayer(rigs);
                }
            }
        }
        public static void CrashGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                CrashPlayer(GunLib.LockedPlayer);
            }, true);
        }
        public static void CrashAll()
        {
            foreach (VRRig p in GorillaParent.instance.vrrigs.Where(r => r != VRRig.LocalRig))
            {
                CrashPlayer(p);
            }
        }
        
        public static void CrashPlayer(VRRig player) // not actual
        {
            if (Time.time <= delay) return;
            for (int i = 0; i < 1875; i++) PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(3, new ExitGames.Client.Photon.Hashtable(), new RaiseEventOptions { TargetActors = new int[] { player.Creator.ActorNumber } }, SendOptions.SendUnreliable);
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            for (int ii = 0; ii < 10; ii++)
                Safety.AntiRPCKick();
            delay = Time.time + 4.54f;
        }
        public static void LagPlayer(VRRig player)
        {
            if (Time.time <= delay) return;
            for (int i = 0; i < 600; i++) PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(3, new ExitGames.Client.Photon.Hashtable(), new RaiseEventOptions { TargetActors = new int[] { player.Creator.ActorNumber } }, SendOptions.SendUnreliable);
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            Safety.AntiRPCKick();
            delay = Time.time + 1.3f;
        }
        public static void StutterPlayer(VRRig player)
        {
            if (Time.time <= delay) return;
            for (int i = 0; i < 1200; i++) PhotonNetwork.NetworkingClient.LoadBalancingPeer.OpRaiseEvent(3, new ExitGames.Client.Photon.Hashtable(), new RaiseEventOptions { TargetActors = new int[] { player.Creator.ActorNumber } }, SendOptions.SendUnreliable);
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            Safety.AntiRPCKick();
            delay = Time.time + 5f;
        }
        
        //idk if raise event 3 is detected, but it works, if its detected let me know

        public static Hashtable hashtable = new Hashtable();
        
        public static void FixModCheckers()
        {
            hashtable.Clear();
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, null, null);
        }
    }
}