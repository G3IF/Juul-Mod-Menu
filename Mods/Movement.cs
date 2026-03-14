using BepInEx;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using static Unity.Burst.Intrinsics.X86.Avx;
using Application = UnityEngine.Application;
using Image = UnityEngine.UI.Image;
using Text = UnityEngine.UI.Text;

namespace Juul
{
    public class Movement
    {
        public static int Speedinde = 0;
        public static string[] platInputNames = { "Grip", "Trigger" };





        public static void NoClipFly()
        {
            if (Inputs.RightPrimary)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * flyspeed);
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
                foreach (MeshCollider v in Resources.FindObjectsOfTypeAll<MeshCollider>())
                    v.enabled = false;
            }
            else
            {
                foreach (MeshCollider v in Resources.FindObjectsOfTypeAll<MeshCollider>())
                    v.enabled = true;
            }
        }
        public static float flyspeed = 5f; 
        private static int speedIndex = 1; 
        private static float[] speedOptions = new float[] { 2f, 5f, 10f, 20f, 50f };
        private static string[] speedNames = new string[] { "Slow", "Normal", "Fast", "Very Fast", "Extreme" };

        public static void Fly()
        {
            if (Inputs.RightPrimary)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * flyspeed);
                GorillaTagger.Instance.rigidbody.linearVelocity = Vector3.zero;
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

        public static void ZeroGravity()
        {
            GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.up * 9.81f, ForceMode.Acceleration);
        }

        public static void LowGravity()
        {
            GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.up * 6.66f, ForceMode.Acceleration);
        }

        public static void HighGravity()
        {
            GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.down * 7.77f, ForceMode.Acceleration);
        }

        public static void Bouncy()
        {
            GorillaTagger.Instance.bodyCollider.material.bounciness = 1f;
            GorillaTagger.Instance.bodyCollider.material.bounceCombine = (PhysicsMaterialCombine)3;
            GorillaTagger.Instance.bodyCollider.material.dynamicFriction = 0f;
        }

        public static void ResetBouncy()
        {
            GorillaTagger.Instance.bodyCollider.material.bounciness = 0f;
            GorillaTagger.Instance.bodyCollider.material.bounceCombine = (PhysicsMaterialCombine)3;
            GorillaTagger.Instance.bodyCollider.material.dynamicFriction = 0f;
        }

        public static bool isTped = false;

        public static void TeleportGun()
        {
            GunLib.StartPointerSystem(() =>
            {
                if (!isTped)
                {
                    isTped = true;
                    TeleportPlayer(GunLib.spherepointer.transform.position);
                }
            }, false);

            if (!GunLib.trigger)
            {
                isTped = false;
            }
        }

        public static void TeleportPlayer(Vector3 pos)
        {
            GTPlayer.Instance.TeleportTo(World2Player(pos), GTPlayer.Instance.transform.rotation, false);
        }

        public static Vector3 World2Player(Vector3 world)
        {
            return world - GorillaTagger.Instance.bodyCollider.transform.position + GorillaTagger.Instance.transform.position;
        }
        public static Vector3 pos;

        public static float jumpspeed = 20f;
        public static float jumpmultiplier = 20f;

        private static float[] speedOptions2 = new float[] { 0.5f, 7f, 10f, 25f };
        private static float[] multiplierOptions = new float[] { 0.5f, 2.1f, 5f, 15f };
        private static string[] speedNames2 = new string[] { "Slow Boost", "Mosa Boost", "Speed Boost", "Insane Boost" };
        private static int speedIndex2 = 2; 

        public static void SpeedBoost()
        {
            GTPlayer.Instance.maxJumpSpeed = jumpspeed;
            GTPlayer.Instance.jumpMultiplier = jumpmultiplier;
        }
        public static void GripSpeedBoost()
        {
            if (Inputs.RightGrip)
            {
                GTPlayer.Instance.maxJumpSpeed = jumpspeed;
                GTPlayer.Instance.jumpMultiplier = jumpmultiplier;
            }
        }
      

        public static void WASDFly()
        {
            float sped = 5f;
            float multipl = 2.5f;
            float sens = 0.3f;
            Transform playerCamera = Camera.main.transform;
            Rigidbody playerRigidBody = GorillaTagger.Instance.rigidbody;
            playerRigidBody.useGravity = false;
            playerRigidBody.linearVelocity = Vector3.zero;
            float actualSpeed = UnityInput.Current.GetKey(KeyCode.LeftShift)
                ? sped * multipl
                : sped;
            float deltaMovement = actualSpeed * Time.deltaTime;
            Vector3 movementVector = Vector3.zero;
            if (UnityInput.Current.GetKey(KeyCode.W) || UnityInput.Current.GetKey(KeyCode.UpArrow))
                movementVector += playerCamera.forward;
            if (UnityInput.Current.GetKey(KeyCode.S) || UnityInput.Current.GetKey(KeyCode.DownArrow))
                movementVector -= playerCamera.forward;
            if (UnityInput.Current.GetKey(KeyCode.D) || UnityInput.Current.GetKey(KeyCode.RightArrow))
                movementVector += playerCamera.right;
            if (UnityInput.Current.GetKey(KeyCode.A) || UnityInput.Current.GetKey(KeyCode.LeftArrow))
                movementVector -= playerCamera.right;
            if (UnityInput.Current.GetKey(KeyCode.Space))
                movementVector += playerCamera.up;
            if (UnityInput.Current.GetKey(KeyCode.LeftControl))
                movementVector -= playerCamera.up;
            playerCamera.position += movementVector * deltaMovement;
            if (UnityInput.Current.GetMouseButton(1))
            {
                Vector3 mouseDelta = UnityInput.Current.mousePosition - pos;
                float pitchRotation = playerCamera.localEulerAngles.x - (mouseDelta.y * sens);
                float yawRotation = playerCamera.localEulerAngles.y + (mouseDelta.x * sens);
                playerCamera.localEulerAngles = new Vector3(pitchRotation, yawRotation, 0f);
            }
            pos = UnityInput.Current.mousePosition;
        }
        public static GameObject platL = null;
        public static GameObject platR = null;
        public static int platMode = 0; 
        public static int platInput = 0; 
        public static float rainbowHue = 0f;

        public static void Platforms()
        {
            bool leftInput = (platInput == 0) ? Inputs.LeftGrip : Inputs.LeftTrigger;
            bool rightInput = (platInput == 0) ? Inputs.RightGrip : Inputs.RightTrigger;
            if (leftInput)
            {
                if (platL == null)
                {
                    platL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    platL.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                    platL.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                    platL.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                    platL.GetComponent<Renderer>().material.shader = Shader.Find("Sprites/Default");
                }
                platL.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                platL.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                Renderer rendL = platL.GetComponent<Renderer>();
                switch (platMode)
                {
                    case 0: 
                        rendL.enabled = true;
                        rendL.material.color = new Color(Core.BaseColor.r, Core.BaseColor.g, Core.BaseColor.b, 1f);
                        break;
                    case 1: 
                        rendL.enabled = false;
                        break;
                    case 2: 
                        rendL.enabled = true;
                        rainbowHue += Time.deltaTime * 0.5f;
                        if (rainbowHue > 1f) rainbowHue -= 1f;
                        rendL.material.color = Color.HSVToRGB(rainbowHue, 1f, 1f);
                        break;
                    case 3: 
                        rendL.enabled = true;
                        rendL.material.color = new Color(Core.BaseColor.r, Core.BaseColor.g, Core.BaseColor.b, 0.3f); 
                                                                                                                     
                        break;
                }
            }
            else if (platL != null)
            {
                UnityEngine.Object.Destroy(platL);
                platL = null;
            }
            if (rightInput)
            {
                if (platR == null)
                {
                    platR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    platR.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                    platR.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                    platR.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                    platR.GetComponent<Renderer>().material.shader = Shader.Find("Sprites/Default");
                }
                platR.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                platR.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                Renderer rendR = platR.GetComponent<Renderer>();
                switch (platMode)
                {
                    case 0: 
                        rendR.enabled = true;
                        rendR.material.color = new Color(Core.BaseColor.r, Core.BaseColor.g, Core.BaseColor.b, 1f);
                        break;
                    case 1: 
                        rendR.enabled = false;
                        break;
                    case 2: 
                        rendR.enabled = true;
                        rendR.material.color = Color.HSVToRGB(rainbowHue, 1f, 1f);
                        break;
                    case 3: 
                        rendR.enabled = true;
                        rendR.material.color = new Color(Core.BaseColor.r, Core.BaseColor.g, Core.BaseColor.b, 0.3f);
                        break;
                }
            }
            else if (platR != null)
            {
                UnityEngine.Object.Destroy(platR);
                platR = null;
            }
            if (platMode == 3)
            {
                if (leftInput || rightInput)
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
        }
        private static string[] modeNames = new string[] { "Normal", "Invisible", "Rainbow", "Noclip" };
   
        public static void ChangeFlySpeed(bool forward)
        {
            if (forward)
                speedIndex = (speedIndex + 1) % speedOptions.Length;
            else
                speedIndex = (speedIndex - 1 + speedOptions.Length) % speedOptions.Length;

            flyspeed = speedOptions[speedIndex];
            string message = $"Fly Speed Changed To: {speedNames[speedIndex]} ({flyspeed})";
            NotifiLib.SendNotification("", message, 2.5f, NotifiLib.NotifiReason.Success);
        }

        public static void ChangeSpeedBoostSpeed(bool forward)
        {
            if (forward)
                speedIndex = (speedIndex + 1) % speedOptions.Length;
            else
                speedIndex = (speedIndex - 1 + speedOptions.Length) % speedOptions.Length;

            jumpspeed = speedOptions[speedIndex];
            jumpmultiplier = multiplierOptions[speedIndex];
            GTPlayer.Instance.maxJumpSpeed = jumpspeed;
            GTPlayer.Instance.jumpMultiplier = jumpmultiplier;
            string message = $"{speedNames[speedIndex]} (Jump: {jumpspeed}, Multi: {jumpmultiplier})";
            NotifiLib.SendNotification("", message, 2.5f, NotifiLib.NotifiReason.Success);
        }

        public static void ChangePlatformType(bool forward)
        {
            if (forward)
                platMode = (platMode + 1) % modeNames.Length;
            else
                platMode = (platMode - 1 + modeNames.Length) % modeNames.Length;

            string message = $"Platform Type Changed To: {modeNames[platMode]}";
            NotifiLib.SendNotification("", message, 2.5f, NotifiLib.NotifiReason.Success);
        }





        public static Vector3? checkpointPos = null;
        public static GameObject orb = null;
        public static void Checkpoint()
        {
            if (Inputs.RightPrimary)
            {
                checkpointPos = GorillaTagger.Instance.rightHandTransform.transform.position;
                if (orb == null)
                {
                    orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    orb.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    orb.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                    Object.Destroy(orb.GetComponent<SphereCollider>());
                }
                orb.transform.position = checkpointPos.Value;
            }

            if (orb != null)
                orb.GetComponent<Renderer>().material.color = Core.BaseColor;

            if (Inputs.RightSecondary && checkpointPos != null)
                Movement.TeleportPlayer(checkpointPos.Value);
        }

        public static void DestroyCheckpoint()
        {
            Object.Destroy(orb);
            orb = null;
            checkpointPos = null;
        }
        private static bool wasLeftHandColliding = false;
        private static bool wasRightHandColliding = false;
        private static float pullStrength = 0.2f;

        public static void PullMod()
        {
            GTPlayer player = GTPlayer.Instance;
            if (player == null) return;
            bool leftColliding = player.leftHand.wasColliding;
            bool rightColliding = player.rightHand.wasColliding;
            if (Inputs.RightGrip && ((leftColliding && !wasLeftHandColliding) || (rightColliding && !wasRightHandColliding)))
            {
                Vector3 velocity = GorillaTagger.Instance.rigidbody.linearVelocity;
                velocity.x *= pullStrength;
                velocity.y = 0f;
                velocity.z *= pullStrength;
                Vector3 newPos = player.transform.position + velocity;
                player.transform.position = newPos;
            }
            wasLeftHandColliding = leftColliding;
            wasRightHandColliding = rightColliding;
        }

        public static void ChangePullStrength()
        {
            pullStrength += 2f;
            if (pullStrength > 20f) pullStrength = 2f;
            NotifiLib.SendNotification("", $"Pull Strength: {pullStrength}", 1.5f, NotifiLib.NotifiReason.Info);
        }

        public static float wallWalkAmount = 0.56f;

        public static void WallAssist()
        {
            if (Inputs.RightGrip)
            {
                if (GTPlayer.Instance.rightHand.wasColliding)
                {
                    GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity += -GTPlayer.Instance.rightHand.controllerTransform.up * 0.56f;
                    RaycastHit raycastHit;
                    Physics.Raycast(GTPlayer.Instance.rightHand.controllerTransform.position, -GTPlayer.Instance.rightHand.controllerTransform.up, out raycastHit);
                }
            }

            if (Inputs.LeftGrip)
            {
                if (GTPlayer.Instance.leftHand.wasColliding)
                {
                    GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity += -GTPlayer.Instance.leftHand.controllerTransform.up * 0.56f;
                    RaycastHit raycastHit2;
                    Physics.Raycast(GTPlayer.Instance.leftHand.controllerTransform.position, -GTPlayer.Instance.leftHand.controllerTransform.up, out raycastHit2);
                }
            }
        }





    }
}