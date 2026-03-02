using System;
using UnityEngine;

namespace Juul
{
    public class Handler
    {
        public static int Speedinde = 0;
        public static int Longinde = 0;
        public static void Change(string type, bool forward)
        {
            if (type == "Flight")
            {
                var Button = Buttons.GetCategory("Movement").GetButton($"Flight: {Movement.FlySpeed}");
                Movement.FlySpeed = Math.Max(0f, Movement.FlySpeed + (forward ? 1f : -1f));
                Button.SetText($"Flight: {Movement.FlySpeed}");
            }
            else if (type == "Noclip")
            {
                var Button = Buttons.GetCategory("Movement").GetButton($"Noclip: {Movement.NoclipFlySpeed}");
                Movement.NoclipFlySpeed = Math.Max(0f, Movement.NoclipFlySpeed + (forward ? 1f : -1f));
                Button.SetText($"Noclip: {Movement.NoclipFlySpeed}");
            }
            /*else if (type == "Speed")
            {
                var Button = Buttons.GetCategory("Movement").GetButton($"Speed: {Movement.Speed()}");
                if (forward) Movement.Speedinde = (Movement.Speedinde + 1 > 2) ? 0 : Movement.Speedinde + 1;
                else Movement.Speedinde = (Movement.Speedinde - 1 < 0) ? 2 : Movement.Speedinde - 1;
                Button.SetText($"Speed: {Movement.Speed()}");
            }
            else if (type == "LongArms")
            {
                var Button = Buttons.GetCategory("Player").GetButton($"Arms: {Players.LongArms()}");
                if (forward) Players.Longinde = (Players.Longinde + 1 > 3) ? 0 : Players.Longinde + 1;
                else Players.Longinde = (Players.Longinde - 1 < 0) ? 3 : Players.Longinde - 1;
                Button.SetText($"Arms: {Players.LongArms()}");
            } caused mem leaks*/
        }
    }
}