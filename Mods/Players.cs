using BepInEx;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using Application = UnityEngine.Application;
using Image = UnityEngine.UI.Image;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Text = UnityEngine.UI.Text;

namespace Juul
{
    public class Players
    {
        public static int Longinde = 0;

        private static float lastGhostToggleTime;
        private static float lastInvisToggleTime;
        private static float lastRightInput;
        private static float lastRightInput2;
        private static bool ghostEnabled => Time.time - lastGhostToggleTime < 0.1f ? false : GorillaTagger.Instance?.offlineVRRig != null && !VRRig.LocalRig.enabled;
        private static float ghostStateEnabledTime = -1f;
        private static float invisStateEnabledTime = -1f;
        private static float rightInputPressedTime = -1f;

        private static bool GhostState
        {
            get => ghostStateEnabledTime > 0f;
            set
            {
                if (value) ghostStateEnabledTime = Time.time;
                else ghostStateEnabledTime = -1f;
            }
        }

        private static bool InvisState
        {
            get => invisStateEnabledTime > 0f;
            set
            {
                if (value) invisStateEnabledTime = Time.time;
                else invisStateEnabledTime = -1f;
            }
        }

        private static bool LastRightInput
        {
            get => rightInputPressedTime > 0f && Time.time - rightInputPressedTime < Time.deltaTime * 2f;
            set
            {
                if (value) rightInputPressedTime = Time.time;
                else rightInputPressedTime = -1f;
            }
        }

        private static float lastInput2Time = -1f;
        private static bool LastInput2
        {
            get => lastInput2Time > 0f && Time.time - lastInput2Time < Time.deltaTime * 2f;
            set
            {
                if (value) lastInput2Time = Time.time;
                else lastInput2Time = -1f;
            }
        }

        
        public static void InvisibleMonke()
        {
            bool triggerButton = Buttons.ghostview
                ? ControllerInputPoller.instance.leftControllerPrimaryButton
                : ControllerInputPoller.instance.leftControllerSecondaryButton;

            bool inputPressed = triggerButton || UnityInput.Current.GetKey(KeyCode.T);

            if (inputPressed && Time.time >= invisStateEnabledTime + 0.2f)
            {
                invisStateEnabledTime = Time.time;

                if (InvisState)
                {
                    InvisState = false;
                    GorillaTagger.Instance.offlineVRRig.enabled = false;
                    GorillaTagger.Instance.offlineVRRig.transform.position = new Vector3(100f, 0f, 100f);

                    if (Buttons.ghostview)
                    {
                        Renderer rigRenderer = GorillaTagger.Instance.offlineVRRig.mainSkin.GetComponent<Renderer>();
                        rigRenderer.material.shader = Shader.Find("GUI/Text Shader");
                        Color hollowColor = Color.white;
                        hollowColor.a = 0.3f;
                        rigRenderer.material.color = hollowColor;
                    }
                }
                else
                {
                    InvisState = true;
                    Visual.RigColorFix();
                    GorillaTagger.Instance.offlineVRRig.enabled = true;
                }
            }
        }

        public static void GhostMonke()
        {
            bool input = Inputs.RightSecondary;

            if (input && !LastRightInput)
            {
                GhostState = !GhostState;

                if (GhostState)
                {
                    if (Buttons.ghostview)
                    {
                        Renderer rigRenderer = GorillaTagger.Instance.offlineVRRig.mainSkin.GetComponent<Renderer>();
                        rigRenderer.material.shader = Shader.Find("GUI/Text Shader");
                        Color hollowColor = Color.white;
                        hollowColor.a = 0.3f;
                        rigRenderer.material.color = hollowColor;
                    }
                    VRRig.LocalRig.enabled = false;
                }
                else
                {
                    if (Buttons.ghostview)
                    {
                        Visual.RigColorFix();
                    }
                    VRRig.LocalRig.enabled = true;
                }
            }

            LastRightInput = input;
        }

        public static void GhostviewClean()
        {
            Buttons.ghostview = false;
            Visual.RigColorFix();
        }

        public static void SSRgbMonkey()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(NetworkSystem.Instance.LocalPlayer.UserId))
                {
                    Color color = Color.Lerp(Color.red, Color.blue, Mathf.PingPong(Time.time, 1));

                    VRRig.LocalRig.netView.GetView.RPC("RPC_InitializeNoobMaterial", RpcTarget.All, new object[]
                    {
                        color.r,
                        color.g,
                        color.b
                    });
                }
            }
        }

        public static void Noclip()
        {
            if (Inputs.RightPrimary)
            {
                foreach (MeshCollider v in Resources.FindObjectsOfTypeAll<MeshCollider>())
                    v.enabled = false;
            }
            else
            {
                foreach (MeshCollider v in Resources.FindObjectsOfTypeAll<MeshCollider>())
                    v.enabled = true;
            }
        }

        public static void DisableLongArms()
        {
            GTPlayer.Instance.transform.localScale = Vector3.one * VRRig.LocalRig.NativeScale;
        }

        public static void SArms()
        {
            GorillaLocomotion.GTPlayer.Instance.transform.localScale = new Vector3(0.77f, 0.77f, 0.77f);
        }

        public static void RArms()
        {
            GorillaLocomotion.GTPlayer.Instance.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
        }

        public static void Spinbot()
        {
            if (Inputs.RightGrip)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = GTPlayer.Instance.bodyCollider.transform.position + new Vector3(0f, 0.15f, 0f);
                VRRig.LocalRig.transform.Rotate(new Vector3(0f, 10f, 0f));
            }
            else
            {
                VRRig.LocalRig.enabled = true;
            }
        }

        public static void HeilaCopter()
        {
            if (Inputs.RightGrip)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = Vector3.up * 10;
                VRRig.LocalRig.transform.Rotate(new Vector3(0, 10, 0));
            }
            else
            {
                VRRig.LocalRig.enabled = true;
            }
        }
    }
}