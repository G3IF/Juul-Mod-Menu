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
    }
}