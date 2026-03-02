using System;
using GorillaLocomotion;
using HarmonyLib;
using UnityEngine;

namespace Juul
{
    [HarmonyPatch(typeof(GTPlayer))]
    [HarmonyPatch("LateUpdate", MethodType.Normal)]
    public class Inputs : MonoBehaviour
    {
        public static void Prefix()
        {
            Inputs.LeftPrimary = ControllerInputPoller.instance.leftControllerPrimaryButton;
            Inputs.RightPrimary = ControllerInputPoller.instance.rightControllerPrimaryButton;

            Inputs.LeftSecondary = ControllerInputPoller.instance.leftControllerSecondaryButton;
            Inputs.RightSecondary = ControllerInputPoller.instance.rightControllerSecondaryButton;

            Inputs.LeftGrip = ControllerInputPoller.instance.leftGrab;
            Inputs.RightGrip = ControllerInputPoller.instance.rightGrab;

            Inputs.LeftTrigger = ControllerInputPoller.instance.leftControllerIndexFloat >= 0.5f;
            Inputs.RightTrigger = ControllerInputPoller.instance.rightControllerIndexFloat >= 0.5f;
        }

        public static bool LeftPrimary;
        public static bool RightPrimary;

        public static bool LeftSecondary;
        public static bool RightSecondary;

        public static bool LeftGrip;
        public static bool RightGrip;

        public static bool LeftTrigger;
        public static bool RightTrigger;
    }
}
