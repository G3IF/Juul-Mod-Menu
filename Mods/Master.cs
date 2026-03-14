using BepInEx;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaLocomotion.Gameplay;
using GorillaNetworking;
using GorillaTag;
using GorillaTagScripts;
using HarmonyLib;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using TMPro;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Valve.VR.InteractionSystem;
using static BuilderMaterialOptions;
using static Mono.Security.X509.X520;
using static NetEventOptions;
using static UnityEngine.Rendering.GPUSort;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using JoinType = GorillaNetworking.JoinType;
using Player = Photon.Realtime.Player;
using Random = UnityEngine.Random;

namespace Juul
{
    internal class Master
    {
        public static void UnTagAll()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                foreach (NetPlayer p in NetworkSystem.Instance.AllNetPlayers)
                {
                    foreach (GorillaTagManager gorillaTagManager in GameObject.FindObjectsByType<GorillaTagManager>(FindObjectsSortMode.None))
                    {
                        gorillaTagManager.currentInfected.Remove(p);
                    }
                }
            }
        }

       
        public static void DestroyAllEntitys()
        {
            if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                foreach (GameEntity game in SuperInfectionManager.activeSuperInfectionManager.gameEntityManager.GetGameEntities())
                {
                    SuperInfectionManager.activeSuperInfectionManager.gameEntityManager.RequestDestroyItem(game.id);
                }
            }
        }
        public static void DestroyEntityGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.raycastHit.collider.GetComponentInParent<GameEntity>())
                {
                    GameEntity entity = GunLib.raycastHit.collider.GetComponentInParent<GameEntity>();
                    SuperInfectionManager.activeSuperInfectionManager.gameEntityManager.RequestDestroyItem(entity.id);
                }
            }, false);
        }
       


        public static float delay;
        public static void UnTagGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (PhotonNetwork.LocalPlayer.IsMasterClient)
                {
                    foreach (GorillaTagManager gorillaTagManager in GameObject.FindObjectsByType<GorillaTagManager>(FindObjectsSortMode.None))
                    {
                        gorillaTagManager.currentInfected.Remove(GunLib.LockedPlayer.OwningNetPlayer);
                    }
                }
            }, true);
        }
        public static void UnTagSelf()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                foreach (GorillaTagManager gorillaTagManager in GameObject.FindObjectsByType<GorillaTagManager>(FindObjectsSortMode.None))
                {
                    gorillaTagManager.currentInfected.Remove(NetworkSystem.Instance.LocalPlayer);
                }
            }
        }
        public static void CauseTagLag()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                foreach (GorillaTagManager gorillaTagManager in GameObject.FindObjectsByType<GorillaTagManager>(FindObjectsSortMode.None))
                {
                    gorillaTagManager.tagCoolDown = 999;
                }
            }
        }
        public static void MasterTag(NetPlayer plr)
        {
            GorillaTagManager tagManager = (GorillaTagManager)GorillaGameManager.instance;
            if (Time.time > delay)
            {
                tagManager.currentInfected.Add(plr);
                delay = Time.time + 0.25f;
                tagManager.currentInfected.Remove(plr);
            }
        }
        public static void MatAll()
        {
            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f)
            {
                foreach (NetPlayer player in NetworkSystem.Instance.AllNetPlayers)
                {
                    MasterTag(player);
                    PhotonNetwork.RunViewUpdate();
                }
            }
        }
        public static void MatGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                MasterTag(GunLib.LockedPlayer.OwningNetPlayer);
                PhotonNetwork.RunViewUpdate();
            }, true);
        }
        public static void FixTagLag()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                foreach (GorillaTagManager gorillaTagManager in GameObject.FindObjectsByType<GorillaTagManager>(FindObjectsSortMode.None))
                {
                    gorillaTagManager.tagCoolDown = 5f;
                }
            }
        }
        public static void SlowPlayer(VRRig Player)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                byte Code = 2;
                RoomSystem.statusSendData[0] = RoomSystem.StatusEffects.TaggedTime;
                object[] SendData = RoomSystem.statusSendData;
                RoomSystem.SendEvent(Code, SendData, new NetEventOptions()
                {
                    TargetActors = new int[]
                    {
              Player.OwningNetPlayer.ActorNumber
                    },
                }, false);
            }
        }
        public static void SlowPlayers()
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                byte Code = 2;
                RoomSystem.statusSendData[0] = RoomSystem.StatusEffects.TaggedTime;
                object[] SendData = RoomSystem.statusSendData;
                RoomSystem.SendEvent(Code, SendData, new NetEventOptions()
                {
                    Reciever = RecieverTarget.others
                }, false);
            }
        }
        public static void VibratePlayer(VRRig Player)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                byte Code = 2;
                RoomSystem.statusSendData[0] = RoomSystem.StatusEffects.JoinedTaggedTime;
                object[] SendData = RoomSystem.statusSendData;
                RoomSystem.SendEvent(Code, SendData, new NetEventOptions()
                {
                    TargetActors = new int[]
                    {
              Player.OwningNetPlayer.ActorNumber
                    },
                }, false);
            }
        }
        public static void VibratePlayers(NetEventOptions Player)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                byte Code = 2;
                RoomSystem.statusSendData[0] = RoomSystem.StatusEffects.JoinedTaggedTime;
                object[] SendData = RoomSystem.statusSendData;
                RoomSystem.SendEvent(Code, SendData, new NetEventOptions()
                {
                    Reciever = RecieverTarget.others
                }, false);
            }
        }
        public static void SlowAll()
        {
            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f)
            {
                SlowPlayers();
            }
        }
        public static void SlowGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                SlowPlayer(GunLib.LockedPlayer);
            }, true);
        }
        public static void VibrateAll()
        {
            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f)
            {
                VibratePlayers(new NetEventOptions() { Reciever = RecieverTarget.others });
            }
        }
        public static void VibrateGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                VibratePlayer(GunLib.LockedPlayer);
            }, true);
        }

        public static void SpawnBlock(int id, Vector3 position, Quaternion rotation)
        {
            if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                BuilderTable table = GameObject.Find("Environment Objects/MonkeBlocksRoomPersistent/BuilderTable").GetComponent<BuilderTable>();
                BuilderTableNetworking network = GameObject.Find("Environment Objects/MonkeBlocksRoomPersistent/BuilderNetworking").GetComponent<BuilderTableNetworking>();
                network.photonView.RPC("PieceCreatedByShelfRPC", RpcTarget.All, new object[]
                {
                id,
                table.CreatePieceId(),
                BitPackUtils.PackWorldPosForNetwork(position),
                BitPackUtils.PackQuaternionForNetwork(rotation),
                0,
                (byte)4,
                1,
                PhotonNetwork.LocalPlayer
                });
            }
        }
        public static void DestroyBlock(int id, Vector3 position, Quaternion rotation, bool PlaySfx)
        {
            if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                BuilderTableNetworking network = GameObject.Find("Environment Objects/MonkeBlocksRoomPersistent/BuilderNetworking").GetComponent<BuilderTableNetworking>();
                network.photonView.RPC("PieceDestroyedRPC", RpcTarget.All, new object[]
                {
                id,
                BitPackUtils.PackWorldPosForNetwork(position),
                BitPackUtils.PackQuaternionForNetwork(rotation),
                PlaySfx,
                (short)1
                });
            }
        }
        public static BuilderPiece piece = null;
        public static void BlockCrashAll()
        {
            SpawnBlock(-1447051713, VRRig.LocalRig.transform.position, Quaternion.identity);
            if (piece == null)
            {
                piece = GameObject.FindObjectOfType<BuilderPiece>();
            }
            if (piece.pieceType == -1447051713 || piece.pieceId == -1447051713)
            {
                piece.gameObject.SetActive(false);
            }
        }
        public static void BlockCrashGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                SpawnBlock(-1447051713, GunLib.LockedPlayer.transform.position, Quaternion.identity);
                if (piece == null)
                {
                    piece = GameObject.FindObjectOfType<BuilderPiece>();
                }
                if (piece.pieceType == -1447051713 || piece.pieceId == -1447051713)
                {
                    piece.gameObject.SetActive(false);
                }
            }, true);
        }
        public static List<int> blockIds = new List<int>
{
    857098599, -2063561053, 1510110959, 1848143946, 866161220,
    -604999206, 1844542113, -1514335082, 868696147, -460092905,
    1000122295, 757678001, 1513669651, -1537067750, 944837962,
    -1499829961, -604288536, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -709037470, -1724151965, -350300979, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
    -1, -1, -1, 1858470402, -1, 1794855203, -1326806786, 724396559,
    1755661147, 1459635109, -566818631, 1925587737, -1441835191,
    -1324502924, 111152940, 798264081, -1821684029, 1895524638,
    1961336659, -1059201160, 1051576141, 539529939, -1535427925,
    1210710592, 1228919111, 252298128, -648273975, 1120512569,
    532163265, -845420418, 1834228748, 1063967233, 1700948013,
    2059548340, -1447051713, 1134055607, 1700655257, -1724819324,
    -1218055069, 251444537, -1446121736, -1927069002, -385891195,
    -196038879, -993249117, 1145900217, 1859614656, 1821589092,
    661312857, 1701825380, -1621444201, 1924370326, -1193326485,
    -1194390666, -751675075, -933358727, 24270440, -1
};
        public static System.Random random = new System.Random();
        public static int GetRandomId()
        {
            int index = random.Next(blockIds.Count);
            return blockIds[index];
        }
        public static void RandomBlockGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                SpawnBlock(GetRandomId(), GunLib.spherepointer.transform.position, Quaternion.identity);
            }, false);
        }
        public static void DestroyBlockGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    BuilderTableNetworking network = GameObject.Find("Environment Objects/MonkeBlocksRoomPersistent/BuilderNetworking").GetComponent<BuilderTableNetworking>();
                    BuilderPiece builderPiece;
                    if (GunLib.raycastHit.collider.GetComponentInParent<BuilderPiece>())
                    {
                        builderPiece = GunLib.raycastHit.collider.GetComponentInParent<BuilderPiece>();
                        if (builderPiece != null)
                        {
                            DestroyBlock(builderPiece.pieceId, GunLib.spherepointer.transform.position, Quaternion.identity, true);
                        }
                    }
                }
            }, false);
        }
        public static void RecycleAllBlocks()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            foreach (BuilderPiece piece in GameObject.FindObjectsOfType<BuilderPiece>())
            {
                if (piece != null)
                {
                    Master.DestroyBlock(piece.pieceId, piece.transform.position, piece.transform.rotation, false);
                }
            }
        }
        public static void BlockBringGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer != null)
                {
                    BringPlayer(GunLib.LockedPlayer);
                }
            }, true);
        }

        public static void BlockBringAll()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig != VRRig.LocalRig)
                {
                    BringPlayer(rig);
                }
            }
        }

        private static void BringPlayer(VRRig player)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Vector3 targetPos = VRRig.LocalRig.transform.position;
            Vector3 playerPos = player.transform.position;
            Vector3 direction = (targetPos - playerPos).normalized;
            float distance = Vector3.Distance(playerPos, targetPos);
            int steps = Mathf.FloorToInt(distance / 0.5f);
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector3 bridgePos = Vector3.Lerp(playerPos, targetPos, t);
                bridgePos.y = targetPos.y;
                SpawnBlock(1858470402, bridgePos, Quaternion.identity);
                if (i % 2 == 0)
                {
                    Vector3 offsetPos = bridgePos + Vector3.up * 0.2f;
                    SpawnBlock(-1447051713, offsetPos, Quaternion.identity);
                }
            }
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                Vector3 surroundPos = playerPos + new Vector3(Mathf.Cos(angle) * 0.5f, 1f, Mathf.Sin(angle) * 0.5f);
                SpawnBlock(GetRandomId(), surroundPos, Quaternion.identity);
            }
            for (int i = 0; i < 3; i++)
            {
                Vector3 targetOffset = targetPos + Random.insideUnitSphere * 0.5f;
                SpawnBlock(GetRandomId(), targetOffset, Quaternion.identity);
            }
        }
        public static void BlockFlingGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer != null)
                {
                    FlingPlayer(GunLib.LockedPlayer);
                }
            }, true);
        }

        public static void BlockFlingAll()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig != VRRig.LocalRig)
                {
                    FlingPlayer(rig);
                }
            }
        }

        private static void FlingPlayer(VRRig player)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            Vector3 flingDirection = (player.transform.position - GunLib.spherepointer.transform.position).normalized;
            if (flingDirection == Vector3.zero)
            {
                flingDirection = Random.onUnitSphere;
            }
            for (int i = 0; i < 12; i++)
            {
                float force = i * 0.3f;
                Vector3 spawnPos = player.transform.position + flingDirection * force;
                Quaternion spinRot = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
                SpawnBlock(GetRandomId(), spawnPos, spinRot);
            }
            for (int i = 1; i <= 5; i++)
            {
                Vector3 linePos = player.transform.position + flingDirection * i;
                SpawnBlock(-1447051713, linePos, Quaternion.identity);
            }
        }
        public static void BlockFloatGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer != null)
                {
                    FloatPlayer(GunLib.LockedPlayer);
                }
            }, true);
        }

        public static void BlockFloatAll()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig != VRRig.LocalRig)
                {
                    FloatPlayer(rig);
                }
            }
        }

        private static void FloatPlayer(VRRig player)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3 offset = new Vector3(x * 0.3f, -0.5f, z * 0.3f);
                    Vector3 spawnPos = player.transform.position + offset;
                    int platformBlockId = 1858470402;
                    SpawnBlock(platformBlockId, spawnPos, Quaternion.identity);
                }
            }
            Vector3 liftPos = player.transform.position + Vector3.up * 0.2f;
            SpawnBlock(-1447051713, liftPos, Quaternion.identity);
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90f * Mathf.Deg2Rad;
                Vector3 orbitPos = player.transform.position + new Vector3(Mathf.Cos(angle) * 0.8f, 1f, Mathf.Sin(angle) * 0.8f);
                SpawnBlock(GetRandomId(), orbitPos, Quaternion.identity);
            }
        }
        public static List<GameObject> frozenBlocks = new List<GameObject>();
        public static List<VRRig> frozenPlayers = new List<VRRig>();
        public static void BlockFreezeGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer != null)
                {
                    FreezePlayer(GunLib.LockedPlayer);
                }
            }, true);
        }

        public static void BlockFreezeAll()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig != VRRig.LocalRig)
                {
                    FreezePlayer(rig);
                }
            }
        }

        private static void FreezePlayer(VRRig player)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            float sphereRadius = 1f;
            float spacing = 0.001f; 
            int numLayers = Mathf.RoundToInt((sphereRadius * 1.5f) / spacing);
            for (int layer = 0; layer < numLayers; layer++)
            {
                float y = Mathf.Lerp(-sphereRadius, sphereRadius, (float)layer / (numLayers - 1));
                float layerRadius = Mathf.Sqrt(Mathf.Max(0, sphereRadius * sphereRadius - y * y));
                if (layerRadius < spacing * 0.5f) continue;
                float circumference = 1.5f * Mathf.PI * layerRadius;
                int blocksThisLayer = Mathf.Max(4, Mathf.RoundToInt(circumference / spacing));
                for (int i = 0; i < blocksThisLayer; i++)
                {
                    float angle = (i * 360f / blocksThisLayer) * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(
                        Mathf.Cos(angle) * layerRadius,
                        y,
                        Mathf.Sin(angle) * layerRadius
                    );
                    Vector3 spawnPos = player.transform.position + offset;
                    Quaternion rotation = Quaternion.LookRotation(offset.normalized);
                    SpawnBlock(857098599, spawnPos, rotation);
                    BuilderPiece[] pieces = GameObject.FindObjectsOfType<BuilderPiece>();
                    if (pieces.Length > 0)
                    {
                        frozenBlocks.Add(pieces[pieces.Length - 1].gameObject);
                    }
                }
            }
            if (!frozenPlayers.Contains(player))
            {
                frozenPlayers.Add(player);
            }
        }
        public static void BlockDrawGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.spherepointer != null && GunLib.spherepointer.activeInHierarchy)
                {
                    SpawnBlock(GetRandomId(), GunLib.spherepointer.transform.position, Quaternion.identity);
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 offset = Random.insideUnitSphere * 0.1f;
                        SpawnBlock(GetRandomId(), GunLib.spherepointer.transform.position + offset, Random.rotation);
                    }
                }
            }, true); 
        }


        public static List<T> GetAllType<T>() where T : UnityEngine.Object
        {
            return Resources.FindObjectsOfTypeAll<T>().ToList();
        }
        public static void SetGuardianSelf()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            GetAllType<TappableGuardianIdol>()
                .Where(tgi => tgi?.manager?.photonView != null && !tgi.isChangingPositions)
                .Where(tgi => tgi.zoneManager?.IsZoneValid() == true && tgi.zoneManager.CurrentGuardian == null)
                .FirstOrDefault()?
                .zoneManager?
                .SetGuardian(PhotonNetwork.LocalPlayer);
        }
        public static void SetGuardianAll()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var allPlayers = PhotonNetwork.PlayerList;
            var idols = GetAllType<TappableGuardianIdol>()
                .Where(tgi => tgi?.manager?.photonView != null && !tgi.isChangingPositions)
                .Where(tgi => tgi.zoneManager?.IsZoneValid() == true)
                .ToList();
            if (idols.Count == 0) return;
            int playerIndex = 0;
            foreach (var idol in idols)
            {
                if (playerIndex >= allPlayers.Length) playerIndex = 0;
                var player = NetworkSystem.Instance.GetPlayer(allPlayers[playerIndex]);
                if (player != null)
                {
                    idol.zoneManager?.SetGuardian(player);
                    playerIndex++;
                }
            }
        }
        public static void SetGuardianGun()
        {
            try
            {
                GunLib.StartPointerSystem(() =>
                {
                    if (!PhotonNetwork.IsMasterClient) return;
                    if (GunLib.LockedPlayer == null) return;
                    try
                    {
                        var idol = GetAllType<TappableGuardianIdol>()
                            .Where(tgi => tgi != null && tgi.manager?.photonView != null && !tgi.isChangingPositions)
                            .Where(tgi => tgi.zoneManager != null && tgi.zoneManager.IsZoneValid() && tgi.zoneManager.CurrentGuardian == null)
                            .FirstOrDefault();

                        idol?.zoneManager?.SetGuardian(GunLib.LockedPlayer.OwningNetPlayer);
                    }
                    catch
                    {
                    }
                }, true);
            }
            catch
            {
            }
        }
        public static void GuardianAura()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var myPos = GTPlayer.Instance.transform.position;
            var idols = GetAllType<TappableGuardianIdol>()
                .Where(tgi => tgi?.manager?.photonView != null && !tgi.isChangingPositions)
                .Where(tgi => tgi.zoneManager?.IsZoneValid() == true)
                .ToList();
            if (idols.Count == 0) return;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player == PhotonNetwork.LocalPlayer) continue;

                if (VRRigCache.Instance.TryGetVrrig(player, out var rig))
                {
                    float distance = Vector3.Distance(rig.Rig.transform.position, myPos);
                    if (distance <= 10f)
                    {
                        foreach (var idol in idols)
                        {
                            if (idol.zoneManager.CurrentGuardian == null)
                            {
                                idol.zoneManager.SetGuardian(NetworkSystem.Instance.GetPlayer(player));
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static void GuardianOnTouch()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var idols = GetAllType<TappableGuardianIdol>()
                .Where(tgi => tgi?.manager?.photonView != null && !tgi.isChangingPositions)
                .Where(tgi => tgi.zoneManager?.IsZoneValid() == true)
                .ToList();
            if (idols.Count == 0) return;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player == PhotonNetwork.LocalPlayer) continue;
                if (VRRigCache.Instance.TryGetVrrig(player, out var rig))
                {
                    if (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, rig.Rig.transform.position) < 0.5f ||
                        Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.Rig.transform.position) < 0.5f)
                    {
                        foreach (var idol in idols)
                        {
                            if (idol.zoneManager.CurrentGuardian == null)
                            {
                                idol.zoneManager.SetGuardian(NetworkSystem.Instance.GetPlayer(player));
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static void GuardianOnYourTouch()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var idols = GetAllType<TappableGuardianIdol>()
                .Where(tgi => tgi?.manager?.photonView != null && !tgi.isChangingPositions)
                .Where(tgi => tgi.zoneManager?.IsZoneValid() == true)
                .ToList();
            if (idols.Count == 0) return;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player == PhotonNetwork.LocalPlayer) continue;
                if (VRRigCache.Instance.TryGetVrrig(player, out var rig))
                {
                    if (Vector3.Distance(rig.Rig.leftHandTransform.position, GTPlayer.Instance.transform.position) < 0.5f ||
                        Vector3.Distance(rig.Rig.rightHandTransform.position, GTPlayer.Instance.transform.position) < 0.5f)
                    {
                        foreach (var idol in idols)
                        {
                            if (idol.zoneManager.CurrentGuardian == null)
                            {
                                idol.zoneManager.SetGuardian(PhotonNetwork.LocalPlayer);
                                break;
                            }
                        }
                    }
                }
            }
        }
        public static void UnGuardianAll()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            NetPlayer localPlayer = PhotonNetwork.LocalPlayer;
            foreach (var idol in GetAllType<TappableGuardianIdol>())
            {
                if (idol?.zoneManager != null)
                {
                    var field = idol.zoneManager.GetType().GetField("guardianPlayer",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    field?.SetValue(idol.zoneManager, null);
                }
            }
        }
        public static void UnGuardianGun()
        {
            try
            {
                GunLib.StartPointerSystem(() =>
                {
                    if (!PhotonNetwork.IsMasterClient) return;
                    if (GunLib.LockedPlayer == null) return;

                    try
                    {
                        NetPlayer targetPlayer = GunLib.LockedPlayer.OwningNetPlayer;

                        var idol = GetAllType<TappableGuardianIdol>()
                            .Where(tgi => tgi?.zoneManager != null)
                            .Where(tgi => tgi.zoneManager.CurrentGuardian == targetPlayer)
                            .FirstOrDefault();

                        if (idol?.zoneManager != null)
                        {
                            var field = idol.zoneManager.GetType().GetField("guardianPlayer",
                                BindingFlags.NonPublic | BindingFlags.Instance);

                            if (field != null)
                            {
                                field.SetValue(idol.zoneManager, null);
                            }
                        }
                    }
                    catch
                    {
                    }
                }, true);
            }
            catch
            {
            }
        }
        public static void UnGuardianAura()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var myPos = GTPlayer.Instance.transform.position;
            float auraRadius = 10f;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player == PhotonNetwork.LocalPlayer) continue;
                if (VRRigCache.Instance.TryGetVrrig(player, out var rig))
                {
                    float distance = Vector3.Distance(rig.Rig.transform.position, myPos);
                    if (distance <= auraRadius)
                    {
                        NetPlayer targetPlayer = NetworkSystem.Instance.GetPlayer(player);
                        var idol = GetAllType<TappableGuardianIdol>()
                            .Where(tgi => tgi?.zoneManager != null)
                            .Where(tgi => tgi.zoneManager.CurrentGuardian == targetPlayer)
                            .FirstOrDefault();
                        if (idol?.zoneManager != null)
                        {
                            var field = idol.zoneManager.GetType().GetField("guardianPlayer",
                                BindingFlags.NonPublic | BindingFlags.Instance);
                            field?.SetValue(idol.zoneManager, null);
                        }
                    }
                }
            }
        }
        public static void UnGuardianOnTouch()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player == PhotonNetwork.LocalPlayer) continue;
                if (VRRigCache.Instance.TryGetVrrig(player, out var rig))
                {
                    if (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, rig.Rig.transform.position) < 0.5f ||
                        Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.Rig.transform.position) < 0.5f)
                    {
                        NetPlayer targetPlayer = NetworkSystem.Instance.GetPlayer(player);
                        var idol = GetAllType<TappableGuardianIdol>()
                            .Where(tgi => tgi?.zoneManager != null)
                            .Where(tgi => tgi.zoneManager.CurrentGuardian == targetPlayer)
                            .FirstOrDefault();
                        if (idol?.zoneManager != null)
                        {
                            var field = idol.zoneManager.GetType().GetField("guardianPlayer",
                                BindingFlags.NonPublic | BindingFlags.Instance);
                            field?.SetValue(idol.zoneManager, null);
                        }
                    }
                }
            }
        }
        public static void UnGuardianOnYourTouch()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            NetPlayer localPlayer = NetworkSystem.Instance.LocalPlayer;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player == PhotonNetwork.LocalPlayer) continue;
                if (VRRigCache.Instance.TryGetVrrig(player, out var rig))
                {
                    if (Vector3.Distance(rig.Rig.leftHandTransform.position, GTPlayer.Instance.transform.position) < 0.5f ||
                        Vector3.Distance(rig.Rig.rightHandTransform.position, GTPlayer.Instance.transform.position) < 0.5f)
                    {
                        var idol = GetAllType<TappableGuardianIdol>()
                            .Where(tgi => tgi?.zoneManager != null)
                            .Where(tgi => tgi.zoneManager.CurrentGuardian == localPlayer)
                            .FirstOrDefault();
                        if (idol?.zoneManager != null)
                        {
                            var field = idol.zoneManager.GetType().GetField("guardianPlayer",
                                BindingFlags.NonPublic | BindingFlags.Instance);
                            field?.SetValue(idol.zoneManager, null);
                        }
                    }
                }
            }
        }
        public static void UnGuardianSelf()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            NetPlayer localPlayer = PhotonNetwork.LocalPlayer;
            var idol = GetAllType<TappableGuardianIdol>()
                .FirstOrDefault(tgi => tgi?.zoneManager?.CurrentGuardian == localPlayer);
            if (idol?.zoneManager != null)
            {
                var field = idol.zoneManager.GetType().GetField("guardianPlayer",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                field?.SetValue(idol.zoneManager, null);
            }
        }

        public static void ForceGameMode(GameModeType mode)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GameMode.ChangeGameMode((int)mode);
            }
        }
        public static void ForceGamemodeInfection()
        {
            ForceGameMode(GameModeType.Infection);
        }

        public static void ForceGamemodeCasual()
        {
            ForceGameMode(GameModeType.Casual);
        }

        public static void ForceGamemodeHunt()
        {
            ForceGameMode(GameModeType.HuntDown);
        }

        public static void ForceGamemodePaintbrawl()
        {
            ForceGameMode(GameModeType.Paintbrawl);
        }

        public static void ForceGamemodeAmbush()
        {
            ForceGameMode(GameModeType.Ambush);
        }

        public static void ForceGamemodeFreezeTag()
        {
            ForceGameMode(GameModeType.FreezeTag);
        }

        public static void ForceGamemodeGhost()
        {
            ForceGameMode(GameModeType.Ghost);
        }

        public static void ForceGamemodeCustom()
        {
            ForceGameMode(GameModeType.Custom);
        }

        public static void ForceGamemodeGuardian()
        {
            ForceGameMode(GameModeType.Guardian);
        }

        public static void ForceGamemodePropHunt()
        {
            ForceGameMode(GameModeType.PropHunt);
        }

        public static void ForceGamemodeCompInfection()
        {
            ForceGameMode(GameModeType.InfectionCompetitive);
        }

        public static void ForceGamemodeSuperInfection()
        {
            ForceGameMode(GameModeType.SuperInfect);
        }

        public static void ForceGamemodeSuperCasual()
        {
            ForceGameMode(GameModeType.SuperCasual);
        }
        public static void ForceOptOutAll()
        {
            foreach (var player in RoomSystem.PlayersInRoom)
            {
                GameMode.OptOut(player);
            }
        }
        public static void OpenElevatorDoor()
        {
            foreach (var elevator in GRElevatorManager._instance.allElevators)
            {
                elevator.UpdateLocalState(GRElevator.ElevatorState.DoorBeginOpening);
            }
        }
        public static void CloseElevatorDoor()
        {
            foreach (var elevator in GRElevatorManager._instance.allElevators)
            {
                elevator.UpdateLocalState(GRElevator.ElevatorState.DoorBeginClosing);
            }
        }
        public static void TeleportToStump()
        {
            if (GRElevatorManager._instance != null)
            {
                GRElevatorManager._instance.destination = GRElevatorManager.ElevatorLocation.Stump;
                GRElevatorManager._instance.ActivateElevating();
            }
        }

        public static void TeleportToCity()
        {
            if (GRElevatorManager._instance != null)
            {
                GRElevatorManager._instance.destination = GRElevatorManager.ElevatorLocation.City;
                GRElevatorManager._instance.ActivateElevating();
            }
        }

        public static void TeleportToGhostReactor()
        {
            if (GRElevatorManager._instance != null)
            {
                GRElevatorManager._instance.destination = GRElevatorManager.ElevatorLocation.GhostReactor;
                GRElevatorManager._instance.ActivateElevating();
            }
        }

        public static void TeleportToMonkeBlocks()
        {
            if (GRElevatorManager._instance != null)
            {
                GRElevatorManager._instance.destination = GRElevatorManager.ElevatorLocation.MonkeBlocks;
                GRElevatorManager._instance.ActivateElevating();
            }
        }
        public static void TeleportAllPlayersToMe()
        {
            if (PhotonNetwork.IsMasterClient && GRElevatorManager._instance != null)
            {
                var currentLoc = GRElevatorManager._instance.currentLocation;
                GRElevatorManager._instance.destination = currentLoc;
                GRElevatorManager._instance.ActivateElevating();
            }
        }
        public static void FreezeElevatorDoorsOpen()
        {
            if (PhotonNetwork.IsMasterClient && GRElevatorManager._instance != null)
            {
                foreach (var elevator in GRElevatorManager._instance.allElevators)
                {
                    elevator.UpdateLocalState(GRElevator.ElevatorState.DoorOpen);
                    elevator.upperDoor.position = elevator.openTargetTop.position;
                    elevator.lowerDoor.position = elevator.openTargetBottom.position;
                }
            }
        }

        public static void FreezeElevatorDoorsClosed()
        {
            if (PhotonNetwork.IsMasterClient && GRElevatorManager._instance != null)
            {
                foreach (var elevator in GRElevatorManager._instance.allElevators)
                {
                    elevator.UpdateLocalState(GRElevator.ElevatorState.DoorClosed);
                    elevator.upperDoor.position = elevator.closedTargetTop.position;
                    elevator.lowerDoor.position = elevator.closedTargetBottom.position;
                }
            }
        }
        public static void BreakElevator()
        {
            if (GRElevatorManager._instance == null) return;

            foreach (var elevator in GRElevatorManager._instance.allElevators)
            {
                elevator.UpdateLocalState(GRElevator.ElevatorState.DoorBeginOpening);
                elevator.UpdateLocalState(GRElevator.ElevatorState.DoorMovingOpening);
                elevator.UpdateLocalState(GRElevator.ElevatorState.DoorBeginClosing);
                elevator.UpdateLocalState(GRElevator.ElevatorState.DoorMovingClosing);
                elevator.UpdateLocalState(GRElevator.ElevatorState.DoorBeginOpening);
                elevator.UpdateLocalState(GRElevator.ElevatorState.DoorMovingOpening);
                elevator.UpdateLocalState(GRElevator.ElevatorState.DoorBeginClosing);
                elevator.UpdateLocalState(GRElevator.ElevatorState.DoorMovingClosing);
            }
        }
        public static void OpenBasementDoor()
        {
            GameObject.Find("Environment Objects/LocalObjects_Prefab/CityToBasement/DungeonEntrance/DungeonDoor_Prefab").GetComponent<PhotonView>().RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Opening);
        }
        public static void CloseBasementDoor()
        {
            GameObject.Find("Environment Objects/LocalObjects_Prefab/CityToBasement/DungeonEntrance/DungeonDoor_Prefab").GetComponent<PhotonView>().RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Closing);
        }
        public static void BreakBasementDoor()
        {
            GameObject door = GameObject.Find("Environment Objects/LocalObjects_Prefab/CityToBasement/DungeonEntrance/DungeonDoor_Prefab");
            if (door != null)
            {
                PhotonView doorView = door.GetComponent<PhotonView>();
                doorView.RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Opening);
                doorView.RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Closing);
                doorView.RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Opening);
                doorView.RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Closing);
                doorView.RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Opening);
                doorView.RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Closing);
                doorView.RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Opening);
                doorView.RPC("ChangeDoorState", RpcTarget.AllViaServer, GTDoor.DoorState.Closing);
            }
        }

        public static GhostReactorManager GetGhostReactorManager()
        {
            return GameObject.FindObjectOfType<GhostReactorManager>();
        }
        public static GhostReactor GetGhostReactor()
        {
            return GhostReactor.instance;
        }
        public static void PurchaseAllStationTools()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetGhostReactorManager();
            if (manager == null || manager.reactor == null) return;
            var stations = manager.reactor.toolPurchasingStations;
            if (stations != null)
            {
                for (int i = 0; i < stations.Count; i++)
                {
                    manager.photonView.RPC("ToolPurchaseStationRequestRPC",
                        RpcTarget.All,
                        i,
                        GhostReactorManager.ToolPurchaseStationAction.TryPurchase);
                }
            }
        }
        public static void KillAllEnemies()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetGhostReactorManager();
            if (manager == null) return;
            manager.InstantDeathForCurrentEnemies();
        }
        public static void StartShiftNow()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetGhostReactorManager();
            if (manager == null) return;
            manager.RequestShiftStartAuthority(true);
        }
        public static void EndShiftNow()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetGhostReactorManager();
            if (manager == null) return;
            manager.SendRequestShiftEndRPC();
        }
        public static void GiveAllPlayersShield()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetGhostReactorManager();
            if (manager == null) return;
            foreach (var player in NetworkSystem.Instance.AllNetPlayers)
            {
                var grPlayer = GRPlayer.Get(player.ActorNumber);
                if (grPlayer != null)
                {
                    manager.RequestGrantPlayerShield(grPlayer, 100, 0);
                }
            }
        }
        public static void SetMaxDifficulty()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetGhostReactorManager();
            if (manager == null || manager.reactor == null) return;
            manager.reactor.difficultyScalingForCurrentFloor = 10f;
        }
        public static void SetDepthLevel(int level)
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetGhostReactorManager();
            if (manager == null || manager.reactor == null) return;
            manager.reactor.depthLevel = level;
            manager.reactor.depthConfigIndex = manager.reactor.PickLevelConfigForDepth(level);
            manager.reactor.DelveToNextDepth();
        }
        public static void GhostReactorKillAll()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetGhostReactorManager();
            if (manager == null || manager.reactor == null) return;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player == PhotonNetwork.LocalPlayer) continue;
                var grPlayer = GRPlayer.Get(player.ActorNumber);
                if (grPlayer != null)
                {
                    manager.RequestPlayerStateChange(grPlayer, GRPlayer.GRPlayerState.Ghost);
                    manager.ReportPlayerDeath(grPlayer);
                }
            }
        }
        public static void GhostReactorKillGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!PhotonNetwork.IsMasterClient) return;
                if (GunLib.LockedPlayer == null) return;
                var manager = GetGhostReactorManager();
                if (manager == null) return;
                var grPlayer = GRPlayer.Get(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber);
                if (grPlayer != null)
                {
                    manager.RequestPlayerStateChange(grPlayer, GRPlayer.GRPlayerState.Ghost);
                    manager.ReportPlayerDeath(grPlayer);
                }
            }, true);
        }
        public static void GhostReactorSheildAll()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetGhostReactorManager();
            if (manager == null) return;
            foreach (var player in PhotonNetwork.PlayerList)
            {
                var grPlayer = GRPlayer.Get(player.ActorNumber);
                if (grPlayer != null)
                {
                    manager.RequestGrantPlayerShield(grPlayer, 100, 0);
                }
            }
        }
        public static void GhostReactorSheildGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!PhotonNetwork.IsMasterClient) return;
                if (GunLib.LockedPlayer == null) return;
                var manager = GetGhostReactorManager();
                if (manager == null) return;
                var grPlayer = GRPlayer.Get(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber);
                if (grPlayer != null)
                {
                    manager.RequestGrantPlayerShield(grPlayer, 100, 0);
                }
            }, true);
        }
        public static void GhostReactorKillGun2()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!PhotonNetwork.IsMasterClient) return;
                if (GunLib.LockedPlayer == null) return;
                var manager = GetGhostReactorManager();
                if (manager == null) return;
                var grPlayer = GRPlayer.Get(GunLib.LockedPlayer.OwningNetPlayer.ActorNumber);
                if (grPlayer != null)
                {
                    manager.photonView.RPC("PlayerStateChangeRPC", RpcTarget.All,
                        PhotonNetwork.LocalPlayer.ActorNumber,
                        grPlayer.gamePlayer.rig.OwningNetPlayer.ActorNumber,
                        1);
                    manager.ReportPlayerDeath(grPlayer);
                }
            }, true);
        }

        public static CrittersManager GetCrittersManager()
        {
            return CrittersManager.instance;
        }
        public static void DespawnAllCritters()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager != null)
            {
                manager.QueueDespawnAllCritters();
            }
        }
        public static void MakeCrittersPeaceful()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            foreach (var actor in manager.crittersActors)
            {
                if (actor is CrittersPawn pawn)
                {
                    pawn.currentState = CrittersPawn.CreatureState.Idle;
                }
            }
        }
        public static void MakeCrittersEat()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            foreach (var actor in manager.crittersActors)
            {
                if (actor is CrittersPawn pawn)
                {
                    pawn.currentState = CrittersPawn.CreatureState.Eating;
                }
            }
        }
        public static void MakeCrittersRun()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            foreach (var actor in manager.crittersActors)
            {
                if (actor is CrittersPawn pawn)
                {
                    pawn.currentState = CrittersPawn.CreatureState.Running;
                }
            }
        }
        public static void MakeCrittersSleep()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            foreach (var actor in manager.crittersActors)
            {
                if (actor is CrittersPawn pawn)
                {
                    pawn.currentState = CrittersPawn.CreatureState.Sleeping;
                }
            }
        }
        public static void SpawnFoodAtPlayer()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            var pos = GTPlayer.Instance.transform.position + Vector3.up * 2f;
            var food = manager.SpawnActor(CrittersActor.CrittersActorType.Food);
            if (food != null)
            {
                food.transform.position = pos;
            }
        }
        public static void SpawnCageAtPlayer()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            var pos = GTPlayer.Instance.transform.position + Vector3.up * 2f;
            var cage = manager.SpawnActor(CrittersActor.CrittersActorType.Cage);
            if (cage != null)
            {
                cage.transform.position = pos;
            }
        }
        public static void SpawnStunBombAtPlayer()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            var pos = GTPlayer.Instance.transform.position + Vector3.up * 2f;
            var bomb = manager.SpawnActor(CrittersActor.CrittersActorType.StunBomb);
            if (bomb != null)
            {
                bomb.transform.position = pos;
            }
        }
        public static void SpawnNoiseMakerAtPlayer()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            var pos = GTPlayer.Instance.transform.position + Vector3.up * 2f;
            var noise = manager.SpawnActor(CrittersActor.CrittersActorType.NoiseMaker);
            if (noise != null)
            {
                noise.transform.position = pos;
            }
        }
        public static void SpawnStickyTrapAtPlayer()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            var pos = GTPlayer.Instance.transform.position + Vector3.up * 2f;
            var trap = manager.SpawnActor(CrittersActor.CrittersActorType.StickyTrap);
            if (trap != null)
            {
                trap.transform.position = pos;
            }
        }
        public static void TriggerMassStun()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            var pos = GTPlayer.Instance.transform.position;
            manager.TriggerEvent(CrittersManager.CritterEvent.StunExplosion, -1, pos, Quaternion.identity);
        }
        public static void DespawnAllFood()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            foreach (var actor in manager.allActors)
            {
                if (actor.crittersActorType == CrittersActor.CrittersActorType.Food && actor.gameObject.activeSelf)
                {
                    manager.DespawnActor(actor);
                }
            }
        }
        public static void DespawnAllTraps()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            foreach (var actor in manager.allActors)
            {
                if ((actor.crittersActorType == CrittersActor.CrittersActorType.StickyTrap ||
                     actor.crittersActorType == CrittersActor.CrittersActorType.Cage) &&
                    actor.gameObject.activeSelf)
                {
                    manager.DespawnActor(actor);
                }
            }
        }
        public static void BringAllCritters()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            var playerPos = GTPlayer.Instance.transform.position;
            foreach (var actor in manager.crittersActors)
            {
                if (actor is CrittersPawn)
                {
                    actor.transform.position = playerPos + Vector3.up * 2f + UnityEngine.Random.insideUnitSphere * 3f;
                }
            }
        }
        public static void GiantCritters()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            foreach (var actor in manager.crittersActors)
            {
                if (actor is CrittersPawn)
                {
                    actor.transform.localScale = Vector3.one * 5f;
                }
            }
        }
        public static void TinyCritters()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            foreach (var actor in manager.crittersActors)
            {
                if (actor is CrittersPawn)
                {
                    actor.transform.localScale = Vector3.one * 0.2f;
                }
            }
        }
        public static void ResetCritterSizes()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var manager = GetCrittersManager();
            if (manager == null) return;
            foreach (var actor in manager.crittersActors)
            {
                if (actor is CrittersPawn)
                {
                    actor.transform.localScale = Vector3.one;
                }
            }
        }
        public static void CritterStunBombGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                var manager = GetCrittersManager();
                if (manager == null || !manager.LocalAuthority()) return;
                Vector3 spawnPos = GunLib.spherepointer.transform.position;
                var bomb = manager.SpawnActor(CrittersActor.CrittersActorType.StunBomb);
                if (bomb != null)
                {
                    bomb.transform.position = spawnPos;
                    manager.TriggerEvent(CrittersManager.CritterEvent.StunExplosion, bomb.actorId, spawnPos, Quaternion.identity);
                }
            }, false);
        }
        public static void CritterStickyTrapGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                var manager = GetCrittersManager();
                if (manager == null || !manager.LocalAuthority()) return;
                Vector3 spawnPos = GunLib.spherepointer.transform.position;
                var trap = manager.SpawnActor(CrittersActor.CrittersActorType.StickyTrap);
                if (trap != null)
                {
                    trap.transform.position = spawnPos;
                }
            }, false);
        }
        public static void CritterNoiseMakerGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                var manager = GetCrittersManager();
                if (manager == null || !manager.LocalAuthority()) return;
                Vector3 spawnPos = GunLib.spherepointer.transform.position;
                var noise = manager.SpawnActor(CrittersActor.CrittersActorType.NoiseMaker);
                if (noise != null)
                {
                    noise.transform.position = spawnPos;
                    manager.TriggerEvent(CrittersManager.CritterEvent.NoiseMakerTriggered, noise.actorId, spawnPos, Quaternion.identity);
                }
            }, false);
        }
        public static void CritterCageGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                var manager = GetCrittersManager();
                if (manager == null || !manager.LocalAuthority()) return;
                Vector3 spawnPos = GunLib.spherepointer.transform.position;
                var cage = manager.SpawnActor(CrittersActor.CrittersActorType.Cage);
                if (cage != null)
                {
                    cage.transform.position = spawnPos;
                }
            }, false);
        }
        public static void CritterFoodGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                var manager = GetCrittersManager();
                if (manager == null || !manager.LocalAuthority()) return;
                Vector3 spawnPos = GunLib.spherepointer.transform.position;
                var food = manager.SpawnActor(CrittersActor.CrittersActorType.Food);
                if (food != null)
                {
                    food.transform.position = spawnPos;
                }
            }, false);
        }
        public static void CritterStickyGooGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                var manager = GetCrittersManager();
                if (manager == null || !manager.LocalAuthority()) return;
                Vector3 spawnPos = GunLib.spherepointer.transform.position;
                var goo = manager.SpawnActor(CrittersActor.CrittersActorType.StickyGoo);
                if (goo != null)
                {
                    goo.transform.position = spawnPos;
                    manager.TriggerEvent(CrittersManager.CritterEvent.StickyDeployed, goo.actorId, spawnPos, Quaternion.identity);
                }
            }, false);
        }
        public static void CritterSpawnGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                var manager = GetCrittersManager();
                if (manager == null || !manager.LocalAuthority()) return;
                Vector3 spawnPos = GunLib.spherepointer.transform.position;
                var critter = manager.SpawnCritter();
                if (critter != null)
                {
                    critter.transform.position = spawnPos;
                }
            }, false);
        }
        public static void CritterDespawnGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                var manager = GetCrittersManager();
                if (manager == null || !manager.LocalAuthority()) return;
                if (GunLib.raycastHit.collider != null)
                {
                    var critter = GunLib.raycastHit.collider.GetComponentInParent<CrittersPawn>();
                    if (critter != null)
                    {
                        manager.DespawnActor(critter);
                        return;
                    }
                    var actor = GunLib.raycastHit.collider.GetComponentInParent<CrittersActor>();
                    if (actor != null)
                    {
                        manager.DespawnActor(actor);
                    }
                }
            }, false);
        }
        public static void GhostSpawnCoreGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                var manager = GetGhostReactorManager();
                if (manager == null || !manager.IsAuthority()) return;
                Vector3 spawnPos = GunLib.spherepointer.transform.position;
                foreach (var player in NetworkSystem.Instance.AllNetPlayers)
                {
                    var grPlayer = GRPlayer.Get(player.ActorNumber);
                    if (grPlayer != null)
                    {
                        manager.ReportCoreCollection(grPlayer, ProgressionManager.CoreType.Core);
                    }
                }
            }, false);
        }
        public static void GhostSpawnChaosSeedGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                var manager = GetGhostReactorManager();
                if (manager == null || !manager.IsAuthority()) return;
                Vector3 spawnPos = GunLib.spherepointer.transform.position;
                foreach (var player in NetworkSystem.Instance.AllNetPlayers)
                {
                    var grPlayer = GRPlayer.Get(player.ActorNumber);
                    if (grPlayer != null)
                    {
                        manager.ReportCoreCollection(grPlayer, ProgressionManager.CoreType.ChaosSeed);
                    }
                }
            }, false);
        }
        public static void GhostSpawnSuperCoreGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                var manager = GetGhostReactorManager();
                if (manager == null || !manager.IsAuthority()) return;
                Vector3 spawnPos = GunLib.spherepointer.transform.position;
                foreach (var player in NetworkSystem.Instance.AllNetPlayers)
                {
                    var grPlayer = GRPlayer.Get(player.ActorNumber);
                    if (grPlayer != null)
                    {
                        manager.ReportCoreCollection(grPlayer, ProgressionManager.CoreType.SuperCore);
                    }
                }
            }, false);
        }
        public static GreyZoneManager GetGreyZone()
        {
            return GreyZoneManager.Instance;
        }
        public static void ActivateGreyZone()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var greyZone = GetGreyZone();
            if (greyZone == null) return;
            greyZone.ActivateGreyZoneAuthority();
        }
        public static void DeactivateGreyZone()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var greyZone = GetGreyZone();
            if (greyZone == null) return;
            greyZone.DeactivateGreyZoneAuthority();
        }
        private static float spazGreyTimer = 0f;
        private static float spazGreyInterval = 0f;
        public static void SpazGreyZone()
        {
            var greyZone = GetGreyZone();
            if (greyZone == null) return;
            if (!PhotonNetwork.IsMasterClient) return;
            spazGreyTimer += Time.deltaTime;
            if (spazGreyTimer >= spazGreyInterval)
            {
                spazGreyTimer = 0f;
                if (greyZone.GreyZoneActive)
                {
                    greyZone.DeactivateGreyZoneAuthority();
                }
                else
                {
                    greyZone.ActivateGreyZoneAuthority();
                }
            }
        }
        public static GorillaPaintbrawlManager GetPaintbrawl()
        {
            return GorillaGameManager.instance as GorillaPaintbrawlManager;
        }
        public static void AllRedTeam()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var pb = GetPaintbrawl();
            if (pb == null) return;
            foreach (var player in NetworkSystem.Instance.AllNetPlayers)
            {
                pb.playerStatusDict[player.ActorNumber] = GorillaPaintbrawlManager.PaintbrawlStatus.RedTeam;
            }
        }
        public static void AllBlueTeam()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var pb = GetPaintbrawl();
            if (pb == null) return;
            foreach (var player in NetworkSystem.Instance.AllNetPlayers)
            {
                pb.playerStatusDict[player.ActorNumber] = GorillaPaintbrawlManager.PaintbrawlStatus.BlueTeam;
            }
        }
        public static void InstantKillGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!PhotonNetwork.IsMasterClient) return;
                if (GunLib.LockedPlayer == null) return;
                var pb = GetPaintbrawl();
                if (pb == null) return;
                int targetActor = GunLib.LockedPlayer.OwningNetPlayer.ActorNumber;
                pb.playerLives[targetActor] = 0;

            }, true);
        }
        public static void StunGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!PhotonNetwork.IsMasterClient) return;
                if (GunLib.LockedPlayer == null) return;
                var pb = GetPaintbrawl();
                if (pb == null) return;
                int targetActor = GunLib.LockedPlayer.OwningNetPlayer.ActorNumber;
                pb.playerStunTimes[targetActor] = Time.time;
            }, true);
        }
        public static void TeamChangerGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!PhotonNetwork.IsMasterClient) return;
                if (GunLib.LockedPlayer == null) return;
                var pb = GetPaintbrawl();
                if (pb == null) return;
                int targetActor = GunLib.LockedPlayer.OwningNetPlayer.ActorNumber;
                if (pb.OnRedTeam(GunLib.LockedPlayer.OwningNetPlayer))
                {
                    pb.playerStatusDict[targetActor] = GorillaPaintbrawlManager.PaintbrawlStatus.BlueTeam;
                }
                else
                {
                    pb.playerStatusDict[targetActor] = GorillaPaintbrawlManager.PaintbrawlStatus.RedTeam;
                }
            }, true);
        }
        public static void HealGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!PhotonNetwork.IsMasterClient) return;
                if (GunLib.LockedPlayer == null) return;
                var pb = GetPaintbrawl();
                if (pb == null) return;
                int targetActor = GunLib.LockedPlayer.OwningNetPlayer.ActorNumber;
                pb.playerLives[targetActor] = 3;
            }, true);
        }
        public static void KillAll()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var pb = GetPaintbrawl();
            if (pb == null) return;
            foreach (var player in NetworkSystem.Instance.AllNetPlayers)
            {
                pb.playerLives[player.ActorNumber] = 0;
            }
        }
        public static void HealAll()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var pb = GetPaintbrawl();
            if (pb == null) return;
            foreach (var player in NetworkSystem.Instance.AllNetPlayers)
            {
                pb.playerLives[player.ActorNumber] = 3;
            }
        }
        public static void StunAll()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var pb = GetPaintbrawl();
            if (pb == null) return;
            float stunTime = Time.time;
            foreach (var player in NetworkSystem.Instance.AllNetPlayers)
            {
                pb.playerStunTimes[player.ActorNumber] = stunTime;
            }
        }
        public static void ActivateMinesLucySecondLook()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            GameObject ghostObject = GameObject.Find("Environment Objects/05Maze_PersistentObjects/MinesSecondLookSkeleton");
            if (ghostObject == null) return;
            SecondLookSkeleton skeleton = ghostObject.GetComponent<SecondLookSkeleton>();
            if (skeleton == null) return;
            SecondLookSkeletonSynchValues synchValues = ghostObject.GetComponent<SecondLookSkeletonSynchValues>();
            if (synchValues == null) return;
            skeleton.tapped = true;
            if (skeleton.currentState == SecondLookSkeleton.GhostState.Unactivated)
            {
                var view = synchValues.GetComponent<PhotonView>();
                if (view != null)
                {
                    view.RPC("RemoteActivateGhost", RpcTarget.All, new object[0]);
                }
            }
        }
        public static void MineLucyJumpscareAll()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            GameObject ghostObject = GameObject.Find("Environment Objects/05Maze_PersistentObjects/MinesSecondLookSkeleton");
            if (ghostObject == null) return;
            SecondLookSkeleton skeleton = ghostObject.GetComponent<SecondLookSkeleton>();
            if (skeleton == null) return;
            SecondLookSkeletonSynchValues synchValues = ghostObject.GetComponent<SecondLookSkeletonSynchValues>();
            if (synchValues == null) return;
            skeleton.tapped = true;
            var view = synchValues.GetComponent<PhotonView>();
            if (view == null) return;
            if (skeleton.currentState == SecondLookSkeleton.GhostState.Unactivated)
            {
                view.RPC("RemoteActivateGhost", RpcTarget.All, new object[0]);
            }
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player != PhotonNetwork.LocalPlayer)
                {
                    view.RPC("RemotePlayerSeen", RpcTarget.All, new object[0]);
                }
            }
            foreach (var player in PhotonNetwork.PlayerList)
            {
                if (player != PhotonNetwork.LocalPlayer)
                {
                    view.RPC("RemotePlayerCaught", RpcTarget.All, new object[0]);
                }
            }
        }
        public static void MineLucyJumpscareGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!PhotonNetwork.IsMasterClient) return;
                if (GunLib.LockedPlayer == null) return;
                GameObject ghostObject = GameObject.Find("Environment Objects/05Maze_PersistentObjects/MinesSecondLookSkeleton");
                if (ghostObject == null) return;
                SecondLookSkeleton skeleton = ghostObject.GetComponent<SecondLookSkeleton>();
                if (skeleton == null) return;
                SecondLookSkeletonSynchValues synchValues = ghostObject.GetComponent<SecondLookSkeletonSynchValues>();
                if (synchValues == null) return;
                skeleton.tapped = true;
                var view = synchValues.GetComponent<PhotonView>();
                if (view == null) return;
                if (skeleton.currentState == SecondLookSkeleton.GhostState.Unactivated)
                {
                    view.RPC("RemoteActivateGhost", RpcTarget.All, new object[0]);
                }
                view.RPC("RemotePlayerSeen", RpcTarget.All, new object[0]);
                view.RPC("RemotePlayerCaught", RpcTarget.All, new object[0]);
            }, true);
        }









    }
}