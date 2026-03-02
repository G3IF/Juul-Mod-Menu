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
        public static float FlySpeed = 20f;
        public static float NoclipFlySpeed = 20f;
        public static int Speedinde = 0;
        public static GameObject platL, platR;
        public static string[] platModeNames = { "Invisible", "Transparent", "Solid" };
        public static int platMode = 1;

        public static string[] platInputNames = { "Grip", "Trigger" };
        public static int platInput = 0;





        public static void NoClipFly()
        {
            if (Inputs.RightPrimary)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * NoclipFlySpeed);
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

        public static void Fly()
        {
            if (Inputs.RightPrimary)
            {
                GTPlayer.Instance.transform.position += GorillaTagger.Instance.headCollider.transform.forward * (Time.deltaTime * FlySpeed);
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

        public static void SpeedBoost()
        {
            GTPlayer.Instance.maxJumpSpeed = 20;
            GTPlayer.Instance.jumpMultiplier = 20;
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
                Renderer rendL = platL.GetComponent<Renderer>();
                if (platMode == 0) rendL.enabled = false;
                else
                {
                    rendL.enabled = true;
                    rendL.material.color = new Color(Core.BaseColor.r, Core.BaseColor.g, Core.BaseColor.b, (platMode == 1) ? 0.5f : 1f);
                }
            }
            else if (platL != null) { UnityEngine.Object.Destroy(platL); platL = null; }
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
                Renderer rendR = platR.GetComponent<Renderer>();
                if (platMode == 0) rendR.enabled = false;
                else
                {
                    rendR.enabled = true;
                    rendR.material.color = new Color(Core.BaseColor.r, Core.BaseColor.g, Core.BaseColor.b, (platMode == 1) ? 0.5f : 1f);
                }
            }
            else if (platR != null) { UnityEngine.Object.Destroy(platR); platR = null; }
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

      
    }
}