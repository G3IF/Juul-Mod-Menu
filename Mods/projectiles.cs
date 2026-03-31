using BepInEx;
using ExitGames.Client.Photon;
using Fusion;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag.Cosmetics.Summer;
using GorillaTagScripts;
using HarmonyLib;
using JetBrains.Annotations;
using Juul;
using OVR.OpenVR;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using POpusCodec.Enums;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.XR;
using static Juul.Patches;
using static Mono.Security.X509.X520;
using static OVRColocationSession;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Juul
{
    public class Projectiles
    {
        public static void DelayChangeUp()
        {
            count++;
            Button b = Core.GetButtonByName("Proj Delay");
            if (count > 3)
            {
                count = 0;
            }
            if (count == 0)
            {
                b.Name = "Proj Delay: Normal";
                Projdelay = 0.180f;
            }
            if (count == 1)
            {
                b.Name = "Proj Delay: Fast";
                Projdelay = 0.140f;
            }
            if (count == 2)
            {
                b.Name = "Proj Delay: Faster";
                Projdelay = 0.119f;
            }
            if (count == 3)
            {
                b.Name = "Proj Delay: Extreme";
                NotifiLib.SendNotification("THIS CAN GET YOU BANNED BE CARFUL", NotifiLib.NotifiReason.Error);
                Projdelay = 0.080f;
            }
        }
        public static void DelayChangeDown()
        {
            count--;
            Button b = Core.GetButtonByName("Proj Delay");
            if (count < 0)
            {
                count = 0;
            }
            if (count == 0)
            {
                b.Name = "Proj Delay: Normal";
                Projdelay = 0.180f;
            }
            if (count == 1)
            {
                b.Name = "Proj Delay: Fast";
                Projdelay = 0.140f;
            }
            if (count == 2)
            {
                b.Name = "Proj Delay: Faster";
                Projdelay = 0.119f;
            }
            if (count == 3)
            {
                b.Name = "Proj Delay: Extreme";
                NotifiLib.SendNotification("THIS CAN GET YOU BANNED BE CARFUL", NotifiLib.NotifiReason.Error);
                Projdelay = 0.080f;
            }
        }

        public static float ProjectileDelay;
        public static int count = 0;
        public static bool ProjCreated = false;
        public static bool CsProjectiles = false;
        public static SnowballThrowable projectile = null;
        public static GameObject Projectile = null;
        private static GorillaVelocityEstimator scriptedGorillaVelEst;
        private static GameObject gorillaVelocityEstimatorCustome;
        public static float Projdelay = 0.180f;
        public static void CreateBigProjectile(Vector3 pos, Vector3 vel, int size)
        {
            if (Projectile == null)
            {
                Projectile = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R/TransferrableItemRightHand/GrowingSnowballRightAnchor(Clone)/LMAVR. RIGHT.");
                Projectile.SetActive(true);
            }
            GrowingSnowballThrowable createproj = Projectile.GetComponent<GrowingSnowballThrowable>();
            if (!ProjCreated)
            {
                createproj.SetSnowballActiveLocal(true);
                createproj.gameObject.SetActive(true);
                createproj.IncreaseSize(size);
                ProjCreated = true;
            }
            else if (ProjCreated && Time.time > ProjectileDelay)
            {
                ProjectileDelay = Time.time + Projdelay;

                PhotonNetwork.RaiseEvent(176, new object[]
              {
    createproj.changeSizeEvent._eventId,
    size
              }, new RaiseEventOptions()
              {
                  Receivers = ReceiverGroup.All,
              }, SendOptions.SendUnreliable);

                PhotonNetwork.RaiseEvent(176, new object[]
                {
    createproj.snowballThrowEvent._eventId,
    pos,
    vel,
    ProjIncrment(pos, vel, size)
                }, new RaiseEventOptions()
                {
                    Receivers = ReceiverGroup.All,
                }, SendOptions.SendUnreliable);
            }
        }
        public static int Incrment1;
        public static int Incrment2;
        public static int Incrment3;
        public static int Incrment4;
        public static int ProjIncrment(Vector3 Pos, Vector3 Vel, int Size)
        {
            try
            {
                GameObject slingshot = new GameObject("slingshot");
                SlingshotProjectile projectile = slingshot.AddComponent<SlingshotProjectile>();

                ProjectileTracker.ProjectileInfo info = new ProjectileTracker.ProjectileInfo(PhotonNetwork.Time, Vel, Pos, Size, projectile);
                ProjIncrment2(Pos, Vel, Size);

                int incrment1 = ProjectileTracker.m_localProjectiles.AddAndIncrement(info);
                Incrment1 = incrment1;

                UnityEngine.Object.Destroy(slingshot);
                Incrment1++;
            }
            catch (Exception e) { Debug.LogException(e); }
            return Incrment1;
        }
        public static int ProjIncrment2(Vector3 Pos, Vector3 Vel, int Size)
        {
            try
            {
                GameObject slingshot = new GameObject("slingshot");
                SlingshotProjectile projectile = slingshot.AddComponent<SlingshotProjectile>();

                ProjectileTracker.ProjectileInfo info = new ProjectileTracker.ProjectileInfo(PhotonNetwork.Time, Vel, Pos, Size, projectile);
                ProjIncrment3(Pos, Vel, Size);

                int incrment2 = ProjectileTracker.m_localProjectiles.IncrementAndAdd(info);
                Incrment2 = incrment2;

                UnityEngine.Object.Destroy(slingshot);

                Incrment2++;
            }
            catch (Exception e) { Debug.LogException(e); }
            return Incrment2;
        }
        public static int ProjIncrment3(Vector3 Pos, Vector3 Vel, int Size)
        {
            try
            {
                GameObject slingshot = new GameObject("slingshot");
                SlingshotProjectile projectile = slingshot.AddComponent<SlingshotProjectile>();

                ProjectileTracker.ProjectileInfo info = new ProjectileTracker.ProjectileInfo(PhotonNetwork.Time, Vel, Pos, Size, projectile);
                ProjIncrment4(Pos, Vel, Size);

                int incrment3 = ProjectileTracker.m_projectileInfoPool.maxInstances;
                Incrment3 = incrment3;

                UnityEngine.Object.Destroy(slingshot);

                Incrment3++;
            }
            catch (Exception e) { Debug.LogException(e); }
            return Incrment3;
        }
        public static void ProjIncrment4(Vector3 Pos, Vector3 Vel, int Size)
        {
            try
            {
                GameObject slingshot = new GameObject("slingshot");
                SlingshotProjectile projectile = slingshot.AddComponent<SlingshotProjectile>();

                ProjectileTracker.ProjectileInfo info = new ProjectileTracker.ProjectileInfo(PhotonNetwork.Time, Vel, Pos, Size, projectile);

                ProjectileTracker.m_projectileInfoPool.InitializePool(100, 1000);

                UnityEngine.Object.Destroy(slingshot);
            }
            catch (Exception e) { Debug.LogException(e); }
        }
        public static void FlingGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                GameplayPatches.IsPositionInRangePatch.enabled = true;
                if (Vector3.Distance(GunLib.LockedPlayer.transform.position, VRRig.LocalRig.transform.position) > 4)
                {
                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position = GunLib.LockedPlayer.transform.position + new Vector3(0, -0.5f, 0);

                    CreateBigProjectile(GunLib.LockedPlayer.transform.position + new Vector3(0, 0.45f, 0), Vector3.down * 9999, 5);
                }
                else
                {
                    VRRig.LocalRig.enabled = true;
                    CreateBigProjectile(GunLib.LockedPlayer.transform.position + new Vector3(0, 0.45f, 0), Vector3.down * 9999, 5);
                }
            }, true);
        }

        public static void ProjectileCleanUp()
        {
            VRRig.LocalRig.enabled = true;
            GorillaTagger.Instance.offlineVRRig.enabled = true;
        }
        public static void ShootBigSnowBalls()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                CreateBigProjectile(GorillaTagger.Instance.rightHandTransform.transform.position, GorillaTagger.Instance.rightHandTransform.transform.forward * 21, 5);
            }
            else
            {
                if (Projectile != null)
                {
                    Projectile.SetActive(false);
                }
                Projectile = null;
            }
        }
        public static void ShootSnowBalls()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                CreateProjectile("Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R/TransferrableItemRightHand/SnowballRightAnchor(Clone)", "LMACF. RIGHT.", GorillaTagger.Instance.rightHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.rightHandTransform.transform.forward * 21);
            }
            else
            {
                if (projectile != null)
                {
                    projectile.gameObject.SetActive(false);
                }
                projectile = null;
            }
        }
        public static void ShootGifs()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                CreateProjectile("Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R/TransferrableItemRightHand/BucketGiftFunctionalAnchor_Right(Clone)", "LMAHR. RIGHT.", GorillaTagger.Instance.rightHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.rightHandTransform.transform.forward * 21);
            }
            else
            {
                if (projectile != null)
                {
                    projectile.gameObject.SetActive(false);
                }
                projectile = null;
            }
        }
        public static void ShootRocks()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                CreateProjectile("Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R/TransferrableItemRightHand/VotingRockAnchor_RIGHT(Clone)", "LMAMT. RIGHT.", GorillaTagger.Instance.rightHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.rightHandTransform.transform.forward * 21);
            }
            else
            {
                if (projectile != null)
                {
                    projectile.gameObject.SetActive(false);
                }
                projectile = null;
            }
        }
        public static List<int> hashes = new List<int>()
        {
            -160604350,//Gif Box 1
            2061412059,//candy cane

        };

        public static SnowballThrowable GetThrowable(GameObject obj)
        {
            if (obj == null) return null;

            if (!obj.GetComponent<SnowballThrowable>()) return null;

            return obj.GetComponent<SnowballThrowable>();
        }
        public static int GetHashOfProjectile(GameObject obj)
        {
            if (obj == null) return 0;

            SnowballThrowable ball = GetThrowable(obj);
            if (ball == null) return 0;

            return ball.GetHashCode();
        }
        public static void CreateProjectile(string path, string name, Vector3 pos, Quaternion rot, Vector3 vel)
        {
            projectile = GameObject.Find(path + "/").transform.Find(name).GetComponent<SnowballThrowable>();
            if (!projectile.gameObject.active)
            {
                projectile.SetSnowballActiveLocal(true);
                projectile.velocityEstimator = scriptedGorillaVelEst;
                projectile.transform.position = pos;
                projectile.transform.rotation = rot;
            }
            if (!ProjCreated)
            {
                gorillaVelocityEstimatorCustome = new GameObject("GorillaVelocityEstimator");
                scriptedGorillaVelEst = gorillaVelocityEstimatorCustome.AddComponent<GorillaVelocityEstimator>();
                ProjCreated = true;
            }
            if (ProjCreated && projectile != null && CsProjectiles)
            {
                GameObject projectilePenis = ObjectPools.instance.Instantiate(GetHashOfProjectile(projectile.gameObject), GorillaTagger.Instance.rightHandTransform.transform.position, GorillaTagger.Instance.rightHandTransform.transform.rotation);
                projectilePenis.GetComponent<Rigidbody>().velocity = GorillaTagger.Instance.rightHandTransform.transform.forward * 21;
                UnityEngine.GameObject.Destroy(projectilePenis, 2.5f);
            }
            else if (ProjCreated && projectile != null && !CsProjectiles)
            {
                Rigidbody gorillaRigidbody = GorillaTagger.Instance.GetComponent<Rigidbody>();
                Vector3 originalVelocity = gorillaRigidbody.velocity;
                Vector3 originalPosition = projectile.transform.position;
                gorillaRigidbody.velocity = vel;

                RoomSystem.projectileSendData[0] = pos;
                RoomSystem.projectileSendData[1] = vel;
                RoomSystem.projectileSendData[2] = RoomSystem.ProjectileSource.RightHand;
                RoomSystem.projectileSendData[3] = ProjIncrment(pos, vel, (int)projectile.transform.lossyScale.x);
                RoomSystem.projectileSendData[4] = false;
                RoomSystem.projectileSendData[5] = Color.white.r;
                RoomSystem.projectileSendData[6] = Color.white.g;
                RoomSystem.projectileSendData[7] = Color.white.b;
                RoomSystem.projectileSendData[8] = Color.white.a;

                ProjectileSend = RoomSystem.projectileSendData;

                ProjecileSending[0] = NetworkSystem.Instance.ServerTimestamp;
                ProjecileSending[1] = 0;
                ProjecileSending[2] = ProjectileSend;

                if (Time.time > Projdelay)
                {
                    Projdelay = Time.time + 0.187f;
                    PhotonNetwork.RaiseEvent(3, ProjecileSending, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, SendOptions.SendUnreliable);
                }

                gorillaRigidbody.velocity = originalVelocity;
                projectile.transform.position = originalPosition;
            }
        }
        public static object[] ProjectileSend = new object[9];
        public static object[] ProjecileSending = new object[2];
        public static List<string> projectiles = new List<string>()
        {
            "Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R/TransferrableItemRightHand/BucketGiftFunctionalAnchor_Right(Clone)",
            "Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/palm.01.L/TransferrableItemLeftHand/BucketGiftFunctionalAnchor_Left(Clone)",
            "Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R/TransferrableItemRightHand/SnowballRightAnchor(Clone)",
            "Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/palm.01.L/TransferrableItemLeftHand/SnowballLeftAnchor(Clone)",
            "Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R/TransferrableItemRightHand/GrowingSnowballRightAnchor(Clone)",
            "Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/palm.01.L/TransferrableItemLeftHand/GrowingSnowballLeftAnchor(Clone)",
            "Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R/palm.01.R/TransferrableItemRightHand/VotingRockAnchor_RIGHT(Clone)",
            "Player Objects/Local VRRig/Local Gorilla Player/GorillaPlayerNetworkedRigAnchor/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L/palm.01.L/TransferrableItemLeftHand/VotingRockAnchor_LEFT(Clone)",
        };
        public static List<string> names = new List<string>()
        {
            "LMAHR. RIGHT.",
            "LMAHQ. LEFT.",
            "LMACF. RIGHT.",
            "LMACE. LEFT.",
            "LMAVR. RIGHT.",
            "LMAVQ. LEFT.",
            "LMAMT. RIGHT.",
            "LMAMS. LEFT.",
        };

    }
}