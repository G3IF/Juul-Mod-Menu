using BepInEx;
using ExitGames.Client.Photon;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTagScripts;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.XR;
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
        public static void GiveAllResources()
        {
            var prog = SIProgression.Instance;
            foreach (SIResource.ResourceType type in Enum.GetValues(typeof(SIResource.ResourceType)))
            {
                prog.resourceDict[type] = 999999;
            }
            SIPlayer.SetAndBroadcastProgression();
        }
        public static void CompleteAllQuests()
        {
            var prog = SIProgression.Instance;
            for (int i = 0; i < prog.ActiveQuestIds.Length; i++)
            {
                if (prog.ActiveQuestIds[i] != -1)
                {
                    prog.AttemptRedeemCompletedQuest(i);
                }
            }
        }
        public static void AlwaysOwnTerminals()
        {
            var manager = SuperInfectionManager.activeSuperInfectionManager;
            foreach (var term in manager.zoneSuperInfection.siTerminals)
            {
                term.activePlayer = SIPlayer.LocalPlayer;
                term.isOccupiedByLocalPlayer = true;
            }
        }
        public static void DisableTerminalTimeout()
        {
            var manager = SuperInfectionManager.activeSuperInfectionManager;
            foreach (var term in manager.zoneSuperInfection.siTerminals)
            {
                term.foldupDelay = float.MaxValue;
            }
        }
       










        public static void SoundSpammer(int num)
        {
            if (ControllerInputPoller.instance.rightGrab || ControllerInputPoller.instance.leftGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.1f;
                    if (PhotonNetwork.InRoom)
                    {
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, num, false, 999999f);
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(num, false, 999999f);
                    }
                }
            }
            FlushRPCS();
        }
        public static void BassSoundSpam() => SoundSpammer(68);
        public static void MetalSoundSpam() => SoundSpammer(18);
        public static void WolfSoundSpam() => SoundSpammer(195);
        public static void CatSoundSpam() => SoundSpammer(236);
        public static void TurkeySoundSpam() => SoundSpammer(83);
        public static void FrogSoundSpam() => SoundSpammer(91);
        public static void BeeSoundSpam() => SoundSpammer(191);
        public static void SqueakSoundSpam() => SoundSpammer(215);
        public static void EarrapeSoundSpam() => SoundSpammer(215);
        public static void DingSoundSpam() => SoundSpammer(244);
        public static void BigCrystalSoundSpam() => SoundSpammer(213);
        public static void PanSoundSpam() => SoundSpammer(248);
        public static void AK47SoundSpam() => SoundSpammer(203);
        public static void TickSoundSpam() => SoundSpammer(148);
        public static void PianoSoundSpam()
        {
            if (ControllerInputPoller.instance.rightGrab || ControllerInputPoller.instance.leftGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.1f;
                    int randomPiano = UnityEngine.Random.Range(295, 308);
                    if (PhotonNetwork.InRoom)
                    {
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, randomPiano, false, 999999f);
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(randomPiano, false, 999999f);
                    }
                }
            }
            FlushRPCS();
        }

        public static void RandomSoundSpam()
        {
            if (ControllerInputPoller.instance.rightGrab || ControllerInputPoller.instance.leftGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.1f;
                    int randomSound = UnityEngine.Random.Range(0, 350);
                    if (PhotonNetwork.InRoom)
                    {
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, randomSound, false, 999999f);
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(randomSound, false, 999999f);
                    }
                }
            }
            FlushRPCS();
        }

        public static void CrystalSoundSpam()
        {
            int[] crystalSounds = new int[] { 213, 214, 215, 216, 217 };
            if (ControllerInputPoller.instance.rightGrab || ControllerInputPoller.instance.leftGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.15f;
                    int crystalSound = crystalSounds[UnityEngine.Random.Range(0, crystalSounds.Length)];
                    if (PhotonNetwork.InRoom)
                    {
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, crystalSound, false, 999999f);
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(crystalSound, false, 999999f);
                    }
                }
            }
            FlushRPCS();
        }

        public static void SirenSoundSpam()
        {
            int[] sirenSounds = new int[] { 250, 251, 252, 253 };
            if (ControllerInputPoller.instance.rightGrab || ControllerInputPoller.instance.leftGrab)
            {
                if (Time.time > delay)
                {
                    delay = Time.time + 0.2f;
                    int sirenSound = sirenSounds[UnityEngine.Random.Range(0, sirenSounds.Length)];
                    if (PhotonNetwork.InRoom)
                    {
                        GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, sirenSound, false, 999999f);
                    }
                    else
                    {
                        GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(sirenSound, false, 999999f);
                    }
                }
            }
            FlushRPCS();
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
        public static void FadeMonkeyHardRGB()
        {
            float t = Time.time * 0.5f;
            float r = Mathf.PingPong(t * 2f, 1f);
            float g = Mathf.PingPong(t * 2f + 0.66f, 1f);
            float b = Mathf.PingPong(t * 2f + 1.33f, 1f);
            r = Mathf.Clamp01(r);
            g = Mathf.Clamp01(g);
            b = Mathf.Clamp01(b);

            if (PhotonNetwork.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, r, g, b);
            }
        }
        public static void BAWFlashMonkey()
        {
            float t = Time.time * 5f;
            float val = Mathf.PingPong(t, 1f) > 0.5f ? 1f : 0f;

            if (PhotonNetwork.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, val, val, val);
            }
        }
        public static void UnlockSubscription()
        {
            SubscriptionManager.SetSubscriptionSettingValue(
                SubscriptionManager.SubscriptionFeatures.IOBT, 1);
            SubscriptionManager.SetSubscriptionSettingValue(
                SubscriptionManager.SubscriptionFeatures.HandTracking, 1);
            SubscriptionManager.SetSubscriptionSettingValue(
                SubscriptionManager.SubscriptionFeatures.GoldenName, 1);
        }
        public static void ForgetAllCredentials()
        {
            PlayFabSettings.staticPlayer.ForgetAllCredentials();
        }
        public static void GrabBug()
        {
            if (Inputs.RightGrip)
            {
                GameObject targetObject = GameObject.Find("Floating Bug Holdable");
                Vector3 handPos = GTPlayer.Instance.rightHand.controllerTransform.position;
                targetObject.transform.position = handPos;
                targetObject.transform.SetParent(GTPlayer.Instance.rightHand.controllerTransform);
            }
            if (Inputs.LeftGrip)
            {
                GameObject targetObject2 = GameObject.Find("Floating Bug Holdable");
                Vector3 handPos2 = GTPlayer.Instance.leftHand.controllerTransform.position;
                targetObject2.transform.position = handPos2;
                targetObject2.transform.SetParent(GTPlayer.Instance.leftHand.controllerTransform);
            }
        }
        public static void GrabBat()
        {
            if (Inputs.RightGrip)
            {
                GameObject targetObject = GameObject.Find("Cave Bat Holdable");
                Vector3 handPos = GTPlayer.Instance.rightHand.controllerTransform.position;
                targetObject.transform.position = handPos;
                targetObject.transform.SetParent(GTPlayer.Instance.rightHand.controllerTransform);
            }
            if (Inputs.LeftGrip)
            {
                GameObject targetObject2 = GameObject.Find("Cave Bat Holdable");
                Vector3 handPos2 = GTPlayer.Instance.leftHand.controllerTransform.position;
                targetObject2.transform.position = handPos2;
                targetObject2.transform.SetParent(GTPlayer.Instance.leftHand.controllerTransform);
            }
        }
        public static void GrabFirefly()
        {
            if (Inputs.RightGrip)
            {
                GameObject targetObject = GameObject.Find("Firefly");
                Vector3 handPos = GTPlayer.Instance.rightHand.controllerTransform.position;
                targetObject.transform.position = handPos;
                targetObject.transform.SetParent(GTPlayer.Instance.rightHand.controllerTransform);
            }
            if (Inputs.LeftGrip)
            {
                GameObject targetObject2 = GameObject.Find("Firefly");
                Vector3 handPos2 = GTPlayer.Instance.leftHand.controllerTransform.position;
                targetObject2.transform.position = handPos2;
                targetObject2.transform.SetParent(GTPlayer.Instance.leftHand.controllerTransform);
            }
        }
        public static void GrabCamera()
        {
            if (Inputs.RightGrip)
            {
                LckSocialCamera rightCamera = LckSocialCameraManager.Instance._socialCameraCococamInstance;
                rightCamera.visible = true;
                rightCamera.recording = true;
                rightCamera.m_CameraVisuals.SetNetworkedVisualsActive(true);
                rightCamera.m_CameraVisuals.SetRecordingState(true);
                rightCamera.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                rightCamera.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                rightCamera.transform.SetParent(GorillaTagger.Instance.rightHandTransform);
            }
            if (Inputs.LeftGrip)
            {
                LckSocialCamera leftCamera = LckSocialCameraManager.Instance._socialCameraCococamInstance;
                leftCamera.visible = true;
                leftCamera.recording = true;
                leftCamera.m_CameraVisuals.SetNetworkedVisualsActive(true);
                leftCamera.m_CameraVisuals.SetRecordingState(true);
                leftCamera.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                leftCamera.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                leftCamera.transform.SetParent(GorillaTagger.Instance.leftHandTransform);
            }
        }
        public static void OrbitBug()
        {
            GameObject targetObject = GameObject.Find("Floating Bug Holdable");
            float angle = Time.time * 15f;
            Vector3 orbitPos = GTPlayer.Instance.transform.position +
            new Vector3(Mathf.Cos(angle) * 1.5f, 0.5f, Mathf.Sin(angle) * 1.5f);
            targetObject.transform.position = orbitPos;
            targetObject.transform.Rotate(Vector3.up, 15f * 50f * Time.deltaTime);
        }
        public static void OrbitBat()
        {
            GameObject targetObject = GameObject.Find("Cave Bat Holdable");
            float angle = Time.time * 15f;
            Vector3 orbitPos = GTPlayer.Instance.transform.position +
            new Vector3(Mathf.Cos(angle) * 1.5f, 0.5f, Mathf.Sin(angle) * 1.5f);
            targetObject.transform.position = orbitPos;
            targetObject.transform.Rotate(Vector3.up, 15f * 50f * Time.deltaTime);
        }
        public static void OrbitFirefly()
        {
            GameObject targetObject = GameObject.Find("Firefly");
            float angle = Time.time * 15f;
            Vector3 orbitPos = GTPlayer.Instance.transform.position +
            new Vector3(Mathf.Cos(angle) * 1.5f, 0.5f, Mathf.Sin(angle) * 1.5f);
            targetObject.transform.position = orbitPos;
            targetObject.transform.Rotate(Vector3.up, 15f * 50f * Time.deltaTime);
        }
        public static void OrbitCamera()
        {
            LckSocialCamera camera = LckSocialCameraManager.Instance._socialCameraCococamInstance;
            camera.visible = true;
            camera.recording = true;
            camera.m_CameraVisuals.SetNetworkedVisualsActive(true);
            camera.m_CameraVisuals.SetRecordingState(true);
            float angle = Time.time * 10f;
            Vector3 orbitPos = GTPlayer.Instance.transform.position +
            new Vector3(Mathf.Cos(angle) * 2f, 1f, Mathf.Sin(angle) * 2f);
            camera.transform.position = orbitPos;
            camera.transform.LookAt(GTPlayer.Instance.transform.position);
        }
        public static void SpazBug()
        {
            GameObject targetObject = GameObject.Find("Floating Bug Holdable");
            Vector3 spazPos = GTPlayer.Instance.transform.position + Vector3.up +
            new Vector3(
                Mathf.Sin(Time.time * 20f) * 1f,
                Mathf.Cos(Time.time * 25f) * 1f,
                Mathf.Sin(Time.time * 15f) * 1f
            );
            targetObject.transform.position = spazPos;
            targetObject.transform.rotation = Quaternion.Euler(
                Time.time * 200f,
                Time.time * 300f,
                Time.time * 100f
            );
        }
        public static void SpazBat()
        {
            GameObject targetObject = GameObject.Find("Cave Bat Holdable");
            Vector3 spazPos = GTPlayer.Instance.transform.position + Vector3.up +
            new Vector3(
                Mathf.Sin(Time.time * 20f) * 1f,
                Mathf.Cos(Time.time * 25f) * 1f,
                Mathf.Sin(Time.time * 15f) * 1f
            );
            targetObject.transform.position = spazPos;
            targetObject.transform.rotation = Quaternion.Euler(
                Time.time * 200f,
                Time.time * 300f,
                Time.time * 100f
            );
        }
        public static void SpazFirefly()
        {
            GameObject targetObject = GameObject.Find("Firefly");
            Vector3 spazPos = GTPlayer.Instance.transform.position + Vector3.up +
            new Vector3(
                Mathf.Sin(Time.time * 20f) * 1f,
                Mathf.Cos(Time.time * 25f) * 1f,
                Mathf.Sin(Time.time * 15f) * 1f
            );
            targetObject.transform.position = spazPos;
            targetObject.transform.rotation = Quaternion.Euler(
                Time.time * 200f,
                Time.time * 300f,
                Time.time * 100f
            );
        }
        public static void SpazCamera()
        {
            LckSocialCamera camera = LckSocialCameraManager.Instance._socialCameraCococamInstance;
            camera.visible = true;
            camera.recording = true;
            camera.m_CameraVisuals.SetNetworkedVisualsActive(true);
            camera.m_CameraVisuals.SetRecordingState(true);
            Vector3 spazPos = GTPlayer.Instance.transform.position + Vector3.up +
            new Vector3(
                Mathf.Sin(Time.time * 30f) * 2f,
                Mathf.Cos(Time.time * 40f) * 1.5f,
                Mathf.Sin(Time.time * 25f) * 2f
            );
            camera.transform.position = spazPos;
            camera.transform.rotation = Quaternion.Euler(
                Mathf.Sin(Time.time * 20f) * 360f,
                Mathf.Cos(Time.time * 15f) * 360f,
                Mathf.Sin(Time.time * 10f) * 360f
            );
        }
        public static void DestroyBug()
        {
            GameObject targetObject = GameObject.Find("Floating Bug Holdable");
            targetObject.transform.position = new Vector3(999f, 999f, 999f);
        }
        public static void DestroyBat()
        {
            GameObject targetObject = GameObject.Find("Cave Bat Holdable");
            targetObject.transform.position = new Vector3(999f, 999f, 999f);
        }
        public static void DestroyFirefly()
        {
            GameObject targetObject = GameObject.Find("Firefly");
            targetObject.transform.position = new Vector3(999f, 999f, 999f);
        }
        public static void DestroyCamera()
        {
            LckSocialCamera camera = LckSocialCameraManager.Instance._socialCameraCococamInstance;
            camera.visible = false;
            camera.recording = false;
            camera.m_CameraVisuals.SetNetworkedVisualsActive(false);
            camera.m_CameraVisuals.SetRecordingState(false);
            camera.transform.position = new Vector3(999f, 999f, 999f);
        }
        public static void ChangeNameTo(string newName)
        {
            var computer = GorillaComputer.instance;
            if (computer == null) return;
            computer.currentName = newName;
            computer.savedName = newName;
            NetworkSystem.Instance.SetMyNickName(newName);
            PlayerPrefs.SetString("playerName", newName);
            PlayerPrefs.Save();
            VRRig.LocalRig.SetNameTagText(newName);
        }
        public static void ChangeColor(float r, float g, float b)
        {
            var computer = GorillaComputer.instance;
            if (computer == null) return;
            computer.redValue = Mathf.Clamp(r, 0f, 1f);
            computer.greenValue = Mathf.Clamp(g, 0f, 1f);
            computer.blueValue = Mathf.Clamp(b, 0f, 1f);
            PlayerPrefs.SetFloat("redValue", computer.redValue);
            PlayerPrefs.SetFloat("greenValue", computer.greenValue);
            PlayerPrefs.SetFloat("blueValue", computer.blueValue);
            PlayerPrefs.Save();
            GorillaTagger.Instance.UpdateColor(computer.redValue, computer.greenValue, computer.blueValue);
            if (NetworkSystem.Instance.InRoom)
            {
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All,
                    computer.redValue, computer.greenValue, computer.blueValue);
            }
        }
        public static void CopyColorGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (GunLib.LockedPlayer == null) return;
                VRRig targetRig = GunLib.LockedPlayer;
                if (targetRig == null) return;
                float r = targetRig.playerColor.r;
                float g = targetRig.playerColor.g;
                float b = targetRig.playerColor.b;
                var computer = GorillaComputer.instance;
                if (computer != null)
                {
                    computer.redValue = r;
                    computer.greenValue = g;
                    computer.blueValue = b;
                    PlayerPrefs.SetFloat("redValue", r);
                    PlayerPrefs.SetFloat("greenValue", g);
                    PlayerPrefs.SetFloat("blueValue", b);
                    PlayerPrefs.Save();
                }
                if (PhotonNetwork.InRoom)
                {
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, r, g, b);
                }

            }, true);
        }
        public static CosmeticsController GetCosmetics()
        {
            return CosmeticsController.instance;
        }
        public static void UnlockAllCosmetics()
        {
            var cosmetics = GetCosmetics();
            if (cosmetics == null) return;
            foreach (var item in cosmetics.allCosmetics)
            {
                if (!item.isNullItem && !cosmetics.unlockedCosmetics.Contains(item))
                {
                    cosmetics.unlockedCosmetics.Add(item);
                    switch (item.itemCategory)
                    {
                        case CosmeticsController.CosmeticCategory.Hat:
                            if (!cosmetics.unlockedHats.Contains(item))
                                cosmetics.unlockedHats.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Face:
                            if (!cosmetics.unlockedFaces.Contains(item))
                                cosmetics.unlockedFaces.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Badge:
                            if (!cosmetics.unlockedBadges.Contains(item))
                                cosmetics.unlockedBadges.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Paw:
                            if (!item.isThrowable)
                            {
                                if (!cosmetics.unlockedPaws.Contains(item))
                                    cosmetics.unlockedPaws.Add(item);
                            }
                            else
                            {
                                if (!cosmetics.unlockedThrowables.Contains(item))
                                    cosmetics.unlockedThrowables.Add(item);
                            }
                            break;
                        case CosmeticsController.CosmeticCategory.Fur:
                            if (!cosmetics.unlockedFurs.Contains(item))
                                cosmetics.unlockedFurs.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Shirt:
                            if (!cosmetics.unlockedShirts.Contains(item))
                                cosmetics.unlockedShirts.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Back:
                            if (!cosmetics.unlockedBacks.Contains(item))
                                cosmetics.unlockedBacks.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Arms:
                            if (!cosmetics.unlockedArms.Contains(item))
                                cosmetics.unlockedArms.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Chest:
                            if (!cosmetics.unlockedChests.Contains(item))
                                cosmetics.unlockedChests.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.Pants:
                            if (!cosmetics.unlockedPants.Contains(item))
                                cosmetics.unlockedPants.Add(item);
                            break;
                        case CosmeticsController.CosmeticCategory.TagEffect:
                            if (!cosmetics.unlockedTagFX.Contains(item))
                                cosmetics.unlockedTagFX.Add(item);
                            break;
                    }
                }
            }
            cosmetics.UpdateWardrobeModelsAndButtons();
            cosmetics.OnCosmeticsUpdated?.Invoke();
        }
        public static void GiveUnlimitedShinyRocks()
        {
            var cosmetics = GetCosmetics();
            if (cosmetics == null) return;
            cosmetics.currencyBalance = 999999;
            cosmetics.UpdateCurrencyBoards();
        }
        private static List<TransferrableObject> cachedHoldables = new List<TransferrableObject>();
        private static float lastCacheTime = 0f;
        private static float cacheInterval = 0.5f; 

        private static void RefreshHoldablesCache()
        {
            if (Time.time - lastCacheTime > cacheInterval)
            {
                cachedHoldables.Clear();
                var found = Resources.FindObjectsOfTypeAll<TransferrableObject>();
                foreach (var obj in found)
                {
                    if (obj != null)
                    {
                        cachedHoldables.Add(obj);
                    }
                }
                lastCacheTime = Time.time;
            }
        }

        public static void StickHoldables()
        {
            try
            {
                RefreshHoldablesCache();
                foreach (var holdable in cachedHoldables)
                {
                    try
                    {
                        if (holdable == null || holdable.gameObject == null || !holdable.gameObject.activeInHierarchy)
                            continue;
                        if (holdable.currentState == TransferrableObject.PositionState.InLeftHand ||
                            holdable.currentState == TransferrableObject.PositionState.InRightHand)
                        {
                            Transform handTransform = holdable.currentState == TransferrableObject.PositionState.InLeftHand ?
                                GTPlayer.Instance.leftHand.controllerTransform :
                                GTPlayer.Instance.rightHand.controllerTransform;

                            holdable.transform.position = handTransform.position;
                            holdable.transform.rotation = handTransform.rotation;
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static void SpinHoldables()
        {
            try
            {
                RefreshHoldablesCache();
                foreach (var holdable in cachedHoldables)
                {
                    try
                    {
                        if (holdable == null || holdable.transform == null)
                            continue;

                        if (holdable.currentState == TransferrableObject.PositionState.InLeftHand ||
                            holdable.currentState == TransferrableObject.PositionState.InRightHand)
                        {
                            holdable.transform.Rotate(Vector3.up, 360f * Time.deltaTime);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static float nextJuggleTime = 0f;
        private static float juggleInterval = 0.3f;
        private static int positionIndex = 0;
        private static TransferrableObject.PositionState[] allPositions = new TransferrableObject.PositionState[]
        {
            TransferrableObject.PositionState.InLeftHand,
            TransferrableObject.PositionState.InRightHand,
            TransferrableObject.PositionState.OnLeftArm,
            TransferrableObject.PositionState.OnRightArm,
            TransferrableObject.PositionState.OnLeftShoulder,
            TransferrableObject.PositionState.OnRightShoulder,
            TransferrableObject.PositionState.OnChest,
            TransferrableObject.PositionState.Dropped
        };

        public static void JuggleHoldables()
        {
            try
            {
                if (Time.time < nextJuggleTime) return;
                nextJuggleTime = Time.time + juggleInterval;
                RefreshHoldablesCache();
                positionIndex = (positionIndex + 1) % allPositions.Length;
                foreach (var holdable in cachedHoldables)
                {
                    try
                    {
                        if (holdable == null || !holdable.gameObject.activeInHierarchy) continue;
                        if (holdable.currentState != TransferrableObject.PositionState.None)
                        {
                            holdable.currentState = allPositions[positionIndex];
                            if (allPositions[positionIndex] == TransferrableObject.PositionState.InLeftHand && holdable.canAutoGrabLeft)
                            {
                                holdable.OnGrab(holdable.gripInteractor, EquipmentInteractor.instance.leftHand);
                            }
                            else if (allPositions[positionIndex] == TransferrableObject.PositionState.InRightHand && holdable.canAutoGrabRight)
                            {
                                holdable.OnGrab(holdable.gripInteractor, EquipmentInteractor.instance.rightHand);
                            }
                            else if (allPositions[positionIndex] == TransferrableObject.PositionState.Dropped)
                            {
                                holdable.DropItem();
                            }
                        }
                    }
                    catch {}
                }
            }
            catch {}
        }
        public static VirtualStumpSerializer GetTerminalNetwork()
        {
            if (CustomMapsTerminal.instance == null) return null;
            return CustomMapsTerminal.instance.mapTerminalNetworkObject;
        }
        public static void VirtualStumpKickGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!PhotonNetwork.IsMasterClient) return;
                var network = GetTerminalNetwork();
                VRRig targetRig = GunLib.LockedPlayer;
                NetPlayer netPlayer = targetRig.Creator ?? NetworkSystem.Instance.GetPlayer(NetworkSystem.Instance.GetOwningPlayerID(targetRig.rigSerializer.gameObject));
                Photon.Realtime.Player targetPlayer = netPlayer.GetPlayerRef();
                network.photonView.RPC("SetRoomMap_RPC", targetPlayer, -1);
            }, true);
        }
        public static void VirtualStumpKickAll()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            var network = GetTerminalNetwork();
            foreach (var rig in VRRigCache.ActiveRigs)
            {
                if (rig.isOfflineVRRig) continue;
                NetPlayer netPlayer = rig.Creator ?? NetworkSystem.Instance.GetPlayer(NetworkSystem.Instance.GetOwningPlayerID(rig.rigSerializer.gameObject));
                Photon.Realtime.Player targetPlayer = netPlayer.GetPlayerRef();
                network.photonView.RPC("SetRoomMap_RPC", targetPlayer, -1);
            }
        }




    }
}