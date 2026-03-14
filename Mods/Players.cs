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

        private static bool LRI;
        private static bool GS;
        private static bool IS;
        private static bool LLI;
        private static bool WRE;

        public static void InvisibleMonke()
        {
            bool inputPressed = Buttons.ghostview
                ? ControllerInputPoller.instance.leftControllerPrimaryButton
                : ControllerInputPoller.instance.leftControllerSecondaryButton;

            inputPressed = inputPressed || UnityInput.Current.GetKey(KeyCode.T);

            if (inputPressed && !LLI)
            {
                IS = !IS;

                if (IS)
                {
                    WRE = VRRig.LocalRig.enabled;
                    VRRig.LocalRig.enabled = false;
                    VRRig.LocalRig.transform.position =
                        GorillaTagger.Instance.bodyCollider.transform.position - Vector3.up * 99999f;
                }
                else
                {
                    VRRig.LocalRig.enabled = WRE;
                    Visual.RigColorFix();
                }
            }

            if (IS)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position =
                    GorillaTagger.Instance.bodyCollider.transform.position - Vector3.up * 99999f;
            }

            LLI = inputPressed;
        }

        public static void GhostMonke()
        {
            bool input = Inputs.RightSecondary;

            if (input && !LRI)
            {
                GS = !GS;

                if (GS)
                {
                    if (Buttons.ghostview)
                    {
                        Renderer rigRenderer = VRRig.LocalRig.mainSkin.GetComponent<Renderer>();
                        rigRenderer.material.shader = Shader.Find("GUI/Text Shader");
                        Color hollowColor = Color.white;
                        hollowColor.a = 0.3f;
                        rigRenderer.material.color = hollowColor;
                    }
                    VRRig.LocalRig.enabled = false;
                }
                else
                {
                    Visual.RigColorFix();
                    VRRig.LocalRig.enabled = true;
                }
            }
            LRI = input;
        }

        public static void GhostviewClean()
        {
            Buttons.ghostview = false;
            GS = false;
            Visual.RigColorFix();
            VRRig.LocalRig.enabled = true;
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
        public static float armLength = 1f;
        public static float minArmLength = 0.5f;
        public static float maxArmLength = 3f;
        public static void ChangeArmLenth()
        {
            if (Inputs.RightGrip)
            {
                armLength += Time.deltaTime * 2f;
                if (armLength > maxArmLength) armLength = maxArmLength;
                GorillaLocomotion.GTPlayer.Instance.transform.localScale = new Vector3(1f, 1f, 1f) * armLength;
                if (GorillaTagger.Instance.offlineVRRig != null)
                {
                    GorillaTagger.Instance.offlineVRRig.transform.localScale = new Vector3(1f, 1f, 1f) * armLength;
                }
            }
            if (Inputs.LeftGrip)
            {
                armLength -= Time.deltaTime * 2f;
                if (armLength < minArmLength) armLength = minArmLength;

                GorillaLocomotion.GTPlayer.Instance.transform.localScale = new Vector3(1f, 1f, 1f) * armLength;

                if (GorillaTagger.Instance.offlineVRRig != null)
                {
                    GorillaTagger.Instance.offlineVRRig.transform.localScale = new Vector3(1f, 1f, 1f) * armLength;
                }
            }
            if (Inputs.RightPrimary)
            {
                armLength = 1f;
                GorillaLocomotion.GTPlayer.Instance.transform.localScale = Vector3.one;

                if (GorillaTagger.Instance.offlineVRRig != null)
                {
                    GorillaTagger.Instance.offlineVRRig.transform.localScale = Vector3.one;
                }
            }
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
        public static void Helicopter()
        {
            if (Inputs.RightGrip)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = Vector3.Lerp(VRRig.LocalRig.transform.position, Vector3.up * 10, Time.deltaTime * 5f);
                VRRig.LocalRig.transform.Rotate(new Vector3(0, 10, 0));
                VRRig.LocalRig.leftHandTransform.localPosition = new Vector3(-0.5f, 0, 0);
                VRRig.LocalRig.leftHandTransform.localRotation = Quaternion.Euler(0, 0, 90);
                VRRig.LocalRig.rightHandTransform.localPosition = new Vector3(0.5f, 0, 0);
                VRRig.LocalRig.rightHandTransform.localRotation = Quaternion.Euler(0, 0, -90);
                GTPlayer.Instance.transform.Rotate(new Vector3(0, 20, 0));
            }
            else
            {
                VRRig.LocalRig.enabled = true;
                VRRig.LocalRig.leftHandTransform.localPosition = Vector3.zero;
                VRRig.LocalRig.leftHandTransform.localRotation = Quaternion.identity;
                VRRig.LocalRig.rightHandTransform.localPosition = Vector3.zero;
                VRRig.LocalRig.rightHandTransform.localRotation = Quaternion.identity;
            }
        }
        public static void Bayblade()
        {
            if (Inputs.RightGrip)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.Rotate(new Vector3(0, 10, 0));
                VRRig.LocalRig.leftHandTransform.localPosition = new Vector3(-0.5f, 0, 0);
                VRRig.LocalRig.leftHandTransform.localRotation = Quaternion.Euler(0, 0, 90);
                VRRig.LocalRig.rightHandTransform.localPosition = new Vector3(0.5f, 0, 0);
                VRRig.LocalRig.rightHandTransform.localRotation = Quaternion.Euler(0, 0, -90);
                GTPlayer.Instance.transform.Rotate(new Vector3(0, 20, 0));
            }
            else
            {
                VRRig.LocalRig.enabled = true;
                VRRig.LocalRig.leftHandTransform.localPosition = Vector3.zero;
                VRRig.LocalRig.leftHandTransform.localRotation = Quaternion.identity;
                VRRig.LocalRig.rightHandTransform.localPosition = Vector3.zero;
                VRRig.LocalRig.rightHandTransform.localRotation = Quaternion.identity;
            }
        }
        public static void Tpose()
        {
            if (Inputs.RightGrip)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.leftHandTransform.localPosition = new Vector3(-1f, 0.5f, 0f);
                VRRig.LocalRig.leftHandTransform.localRotation = Quaternion.Euler(0, 0, 0);
                VRRig.LocalRig.rightHandTransform.localPosition = new Vector3(1f, 0.5f, 0f);
                VRRig.LocalRig.rightHandTransform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                VRRig.LocalRig.enabled = true;
                VRRig.LocalRig.leftHandTransform.localPosition = Vector3.zero;
                VRRig.LocalRig.leftHandTransform.localRotation = Quaternion.identity;
                VRRig.LocalRig.rightHandTransform.localPosition = Vector3.zero;
                VRRig.LocalRig.rightHandTransform.localRotation = Quaternion.identity;
            }
        }
        public static float walkCycle = 0f;

        public static void MinecraftAnimations()
        {
            if (Inputs.RightGrip)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.leftHandTransform.localPosition = new Vector3(-0.3f, -0.5f, 0.2f);
                VRRig.LocalRig.leftHandTransform.localRotation = Quaternion.Euler(0, 0, 0);
                VRRig.LocalRig.rightHandTransform.localPosition = new Vector3(0.3f, -0.5f, 0.2f);
                VRRig.LocalRig.rightHandTransform.localRotation = Quaternion.Euler(0, 0, 0);
                walkCycle += Time.deltaTime * 3f; 
                float swing = Mathf.Sin(walkCycle) * 0.3f;
                VRRig.LocalRig.leftHandTransform.localPosition = new Vector3(-0.3f, -0.5f, 0.2f + swing);
                VRRig.LocalRig.rightHandTransform.localPosition = new Vector3(0.3f, -0.5f, 0.2f - swing);
            }
            else
            {
                VRRig.LocalRig.enabled = true;
                VRRig.LocalRig.leftHandTransform.localPosition = Vector3.zero;
                VRRig.LocalRig.leftHandTransform.localRotation = Quaternion.identity;
                VRRig.LocalRig.rightHandTransform.localPosition = Vector3.zero;
                VRRig.LocalRig.rightHandTransform.localRotation = Quaternion.identity;
            }
        }
        
        

    }
}