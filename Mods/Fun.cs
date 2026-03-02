using BepInEx;
using GorillaGameModes;
using GorillaTagScripts;
using Photon.Pun;
using Photon.Realtime;
using GorillaExtensions;
using System;
using ExitGames.Client.Photon;
using System.Collections;
using System.Linq;
using System.Reflection;
using GorillaNetworking;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.XR;
using Photon;
using System.Threading;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using JoinType = GorillaNetworking.JoinType;

namespace Juul
{
    internal class Fun
    {

        private static Hashtable rpcFilterByViewId = new Hashtable();
        public static void FlushRPCS()
        {
            rpcFilterByViewId[0] = GorillaTagger.Instance.myVRRig.ViewID;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                CachingOption = EventCaching.RemoveFromRoomCache,
                TargetActors = new int[]
                {
                        PhotonNetwork.LocalPlayer.ActorNumber
                }
            };
            MonkeAgent.instance.rpcErrorMax = int.MaxValue;
            MonkeAgent.instance.rpcCallLimit = int.MaxValue;
            MonkeAgent.instance.logErrorMax = int.MaxValue;
            PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
            PhotonNetwork.QuickResends = int.MaxValue;
            PhotonNetwork.SendAllOutgoingCommands();
            PhotonNetwork.NetworkingClient.OpRaiseEvent(200, rpcFilterByViewId, raiseEventOptions, SendOptions.SendReliable);
        } 
       
        public static void DropHoverBoard(Vector3 pos, Quaternion rot, Vector3 vel)
        {
            if (PhotonNetwork.IsConnected)
            {
                FreeHoverboardManager.instance.SendDropBoardRPC(pos, rot, vel, vel, Color.clear);
                FlushRPCS();
            }
        }
        public static float delay;
        public static void ShootHoverBoards()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.1f;
                    DropHoverBoard(GorillaTagger.Instance.rightHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.rightHandTransform.transform.forward * 12);
                }
            }
            if (ControllerInputPoller.instance.leftGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.1f;
                    DropHoverBoard(GorillaTagger.Instance.leftHandTransform.transform.position, Quaternion.identity, GorillaTagger.Instance.leftHandTransform.transform.forward * 12);
                }
            }
        }
        public static void PlayRandomSounds()
        {
            if (PhotonNetwork.IsConnected)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, UnityEngine.Random.Range(0, GTPlayer.Instance.materialData.Count), false, 999999f);
                FlushRPCS();
            }
        }
        public static void PlayJmanYell()
        {
            if (PhotonNetwork.IsConnected)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 337, false, 1f);
                FlushRPCS();
            }
        }
        public static void SpamJmanYell()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.25f;
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, 337, false, 1f);
                }
                FlushRPCS();
            }
        }
        public static void SIUnlockAll()
        {
            foreach (bool[] gadget in SIProgression.Instance.unlockedTechTreeData)
            {
                Array.Fill(gadget, true);
            }
        }
        public static void YoinkTerms()
        {
            foreach (SICombinedTerminal term in SuperInfectionManager.activeSuperInfectionManager.zoneSuperInfection.siTerminals)
            {
                term.PlayerHandScanned(NetworkSystem.Instance.LocalPlayer.ActorNumber);
            }
        }


        public static void Nroom()
        {
            RoomConfig Room = new RoomConfig
            {
                createIfMissing = true,
                isJoinable = true,
                isPublic = true,
                MaxPlayers = 10,
                CustomProps = new ExitGames.Client.Photon.Hashtable() { { "gameMode", GorillaComputer.instance.GetJoinTriggerForZone("forest").GetFullDesiredGameModeString() }, { "platform", (string)typeof(PhotonNetworkController).GetField("platformTag", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(PhotonNetworkController.Instance) }, { "queueName", GorillaComputer.instance.currentQueue }, }
            };
            NetworkSystem.Instance.ConnectToRoom("NIGGER", Room);
        }

        public static bool nettrigsoff = false, qboff = false;

        public static void nettriggers()
        {
            if (nettrigsoff) { GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab").SetActive(false); }
            else { GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab").SetActive(true); }
        }
        public static void quitbox()
        {
            if (qboff) { GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox").SetActive(false); }
            else { GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox").SetActive(true); }
        }
        public static void FlashMonkey()
        {
            float speed = 15f;
            float t = Mathf.Sin(Time.time * speed);
            Color c = Color.HSVToRGB(Mathf.Abs(t), 1f, 1f);
            if (PhotonNetwork.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, c.r, c.g, c.b);
            }
        }
        public static void FadeMonkey()
        {
            float h = Mathf.Repeat(Time.time * 0.2f, 1f);
            Color c = Color.HSVToRGB(h, 1f, 1f);
            if (PhotonNetwork.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, c.r, c.g, c.b);
            }
        }


    }
}