using juul_v2_dev_build.Mods;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using static Juul.NotifiLib;
using static Juul.Patches;
using static Mono.Security.X509.X520;

namespace Juul
{
    public class Buttons
    {
        private static bool initialized = false;

        public static Button ThemeButton;
        public static Button GunStyleButton;
        public static Button GunLineSizeButton;
        public static Button GunSphereSizeButton;

        public static Category GetCategory(string name)
        {
            return Modules.FirstOrDefault(c => c.Name == name);
        }

        public static Category[] Modules = null;

        public static bool ghostview = true;

        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            ThemeButton = new Button { Name = $"Theme: {Core.GetCurrentThemeName()}", Toggle = false, Incremental = true, Up = () => Core.ChangeTheme(true), Down = () => Core.ChangeTheme(false) };
            GunStyleButton = new Button { Name = "Gun Style: " + GunLib.currentLineStyle.ToString(), Toggle = false, Incremental = true, Up = () => GunLib.ChangeGunStyle(true), Down = () => GunLib.ChangeGunStyle(false) };
            GunLineSizeButton = new Button { Name = string.Format("Gun Line Size: {0}", GunLib.GunLineWidth), Toggle = false, Incremental = true, Up = () => GunLib.ChangeGunLineSize(true), Down = () => GunLib.ChangeGunLineSize(false) };
            GunSphereSizeButton = new Button { Name = string.Format("Gun Sphere Size: {0}", GunLib.SphereSize), Toggle = false, Incremental = true, Up = () => GunLib.ChangeGunSphereScale(true), Down = () => GunLib.ChangeGunSphereScale(false) };

            Modules = new Category[]
            {
                new Category {
                    Name = "Settings",
                    Buttons = {
                        new Button { Name = "Save Config", Toggle = false, OnEnable = () => Configs.SaveConfig() },
                        new Button { Name = "Load Config", Toggle = false, OnEnable = () => Configs.LoadConfig() }, ThemeButton,
                        new Button { Name = "Page Buttons", Toggle = false, Incremental = true, Up = () => Core.ChangePageButtons(false), Down = () => Core.ChangePageButtons(true) },
                        new Button { Name = "Menu Size", Toggle = false, Incremental = true, Up = () => Core.ChangeMenuScale(true), Down = () => Core.ChangeMenuScale(false) },
                        new Button { Name = "Button Inset", Toggle = false, Incremental = true, Up = () => Core.ChangeButtonInset(true), Down = () => Core.ChangeButtonInset(false) },
                        new Button { Name = "Text Size", Toggle = false, Incremental = true, Up = () => Core.ChangeTextSize(true), Down = () => Core.ChangeTextSize(false) },
                        new Button { Name = "Make Menu Rounded", Toggle = false, OnEnable = () => { Core.IsRounded = !Core.IsRounded; Core.RebuildMenu();} },
                        new Button { Name = "Outline Menu", Toggle = false, OnEnable = () => Core.IsOutlined = !Core.IsOutlined },
                        new Button { Name = "Rotated Sidebar", Toggle = false, OnEnable = () => Core.IsCatRotated = !Core.IsCatRotated },
                        new Button { Name = "Sidebar Position", Toggle = false, OnEnable = () => Core.IsCatLeft = !Core.IsCatLeft }, GunStyleButton, GunLineSizeButton, GunSphereSizeButton,
                        new Button { Name = "Test Gunlib", Toggle = true, OnEnable = () => GunLib.StartPointerSystem(() => {}, false) },
                        new Button { Name = "Advanced Nametags", Toggle = true, OnceEnable = () => Visual.AdvancedNametags = true, OnceDisable = () => Visual.AdvancedNametags = false },
                        new Button { Name = "Disable Notifications", Toggle = true, OnceEnable = () => NotifiLib.Disablenotifcations = true, OnceDisable = () => NotifiLib.Disablenotifcations = false },
                        new Button { Name = "Enable Ghost View", Toggle = true, Enabled = true, OnceEnable = () => ghostview = true, OnceDisable = () => Players.GhostviewClean() },
                        new Button { Name = string.Format("Proj Delay: Normal"), Toggle = false, Incremental = true, Up = () => Projectiles.DelayChangeUp(), Down = () => Projectiles.DelayChangeDown() },
                        //new Button { Name = "CS Projectiles", Toggle = true, OnceEnable = () => Projectiles.CsProjectiles = true, OnceDisable = () => Projectiles.CsProjectiles = false },
                    }
                },
                new Category {
                    Name = "Movement",
                    Buttons = {
                        new Button { Name = "Platforms", Toggle = true, OnEnable = Movement.Platforms },
                        new Button { Name = "WASD Fly", Toggle = true, OnEnable = Movement.WASDFly },
                        new Button { Name = "Flight", Toggle = true, OnEnable = Movement.Fly },
                        new Button { Name = "Noclip Flight", Toggle = true, OnEnable = Movement.NoClipFly },
                        new Button { Name = "Noclip", Toggle = true, OnEnable = Movement.Noclip },
                        new Button { Name = "Speed Boost", Toggle = true, OnEnable = Movement.SpeedBoost },
                        new Button { Name = "Teleport Gun", Toggle = true, OnEnable = Movement.TeleportGun },
                        new Button { Name = "Zero Gravity", Toggle = true, OnEnable = Movement.ZeroGravity },
                        new Button { Name = "Low Gravity", Toggle = true, OnEnable = Movement.LowGravity },
                        new Button { Name = "High Gravity", Toggle = true, OnEnable = Movement.HighGravity },
                        new Button { Name = "Bouncy", Toggle = true, OnceEnable = Movement.Bouncy, OnceDisable = Movement.ResetBouncy },
                        new Button { Name = "Check Point", Toggle = true, OnEnable = Movement.Checkpoint, OnDisable = Movement.DestroyCheckpoint }
                    }
                },
                new Category {
                    Name = "Player",
                    Buttons = {
                        new Button { Name = "Invis Monkey", Toggle = true, OnEnable = Players.InvisibleMonke },
                        new Button { Name = "Ghost Monkey", Toggle = true, OnEnable = Players.GhostMonke },
                        new Button { Name = "Noclip", Toggle = true, OnEnable = Players.Noclip },
                        new Button { Name = "Long Arms", Toggle = true, OnEnable = Players.RArms, OnDisable = Players.DisableLongArms },
                        new Button { Name = "Short Arms", Toggle = true, OnEnable = Players.SArms, OnDisable = Players.DisableLongArms },
                        new Button { Name = "Spin bot", Toggle = true, OnEnable = Players.Spinbot },
                        new Button { Name = "Hellicopter bot", Toggle = true, OnEnable = Players.HeilaCopter },
                    }
                },
                new Category {
                    Name = "Safety",
                    Buttons = {
                        new Button { Name = "Quit Game", Toggle = false, OnEnable = Safety.QuitGame },
                        new Button { Name = "Join Random", Toggle = false, OnEnable = Safety.JoinRandom },
                        new Button { Name = "Anti Ban", Toggle = true, OnEnable = Safety.AntiBan }, // working? detected?
                        new Button { Name = "Anti RPC Kick", Toggle = true, OnEnable = Safety.AntiRPCKick },
                        new Button { Name = "Restart Game", Toggle = false, OnEnable = Safety.RestartGame },
                        new Button { Name = "Bypass Vc Ban", Toggle = true, OnEnable = Safety.BypassVCBan },
                        new Button { Name = "Disable Map Triggers", Toggle = true, OnceEnable = Safety.DisableMapTriggers, OnceDisable = Safety.EnableMapTriggers },
                        new Button { Name = "Disable Net Triggers", Toggle = true, OnceEnable = Safety.DisableNetworkTriggers, OnceDisable = Safety.EnableNetworkTriggers },
                        new Button { Name = "Disable Quit Box", Toggle = true, OnceEnable = Safety.DisableQuitBox, OnceDisable = Safety.EnableQuitBox },
                        new Button { Name = "Disable AFK Kick", Toggle = true, OnceEnable = Safety.DisableAntiAFK, OnceDisable = Safety.EnableAntiAFK },
                        new Button { Name = "Anti Report Disconnect", Toggle = true, OnEnable = Safety.AntiReportDisconnect },
                        new Button { Name = "Anti Report Reconnect", Toggle = true, OnEnable = Safety.AntiReportReconnect }
                    }
                },
                new Category {
                    Name = "Visual",
                    Buttons = {
                        new Button { Name = "Chams", Toggle = true, OnEnable = Visual.Chams, OnceDisable = Visual.CleanupChams },
                        new Button { Name = "Bone ESP", Toggle = true, OnEnable = Visual.BoneESP, OnceDisable = Visual.CleanupBoneESP },
                        new Button { Name = "Tracers", Toggle = true, OnEnable = Visual.Tracers, OnceDisable = Visual.CleanupTracers },
                        new Button { Name = "2D Box ESP", Toggle = true, OnEnable = Visual.Box2DESP, OnceDisable = Visual.CleanupBox2DESP },
                        new Button { Name = "3D Box ESP", Toggle = true, OnEnable = Visual.Box3DESP, OnceDisable = Visual.CleanupBox3DESP },
                        new Button { Name = "Custom HUD", Toggle = true, OnEnable = Visual.PlayerInfo, OnceDisable = Visual.CleanupPlayerInfo },
                        new Button { Name = "Name Tags", Toggle = true, OnEnable = Visual.PlayerNameESP, OnceDisable = Visual.CleanupPlayerNameESP },
                        new Button { Name = "Menu Them Rig [CS]", Toggle = true, OnEnable = Visual.MenuThemeRig, OnceDisable = Visual.RigColorFix },
                        new Button { Name = "Rainbow All [CS]", Toggle = true, OnEnable = Visual.OutcastAll },
                        new Button { Name = "Draw [CS]", Toggle = true, OnEnable = Visual.Draw }
                    }
                },
                new Category {
                    Name = "Fun",
                    Buttons = {
                        new Button { Name = "RGB Monkey [Stump]", Toggle = true, OnEnable = Fun.FadeMonkey },
                        new Button { Name = "Epilepsy Monkey [Stump]", Toggle = true, OnEnable = Fun.FlashMonkey },
                        new Button { Name = "Unlock All [Gadgets]", Toggle = true, OnEnable = Fun.SIUnlockAll },
                        new Button { Name = "Steal All Terminals [Gadgets]", Toggle = true, OnEnable = Fun.YoinkTerms },
                        new Button { Name = "Shoot Hoverboards", Toggle = true, OnEnable = Fun.ShootHoverBoards },
                        new Button { Name = "Play Random Sounds", Toggle = true, OnEnable = Fun.PlayRandomSounds },
                        new Button { Name = "Play Jman Scream", Toggle = false, OnEnable = Fun.PlayJmanYell },
                        new Button { Name = "Spam Jman Scream", Toggle = true, OnEnable = Fun.SpamJmanYell },
                    }
                },
                new Category {
                    Name = "Master",
                    Buttons = {
                        new Button { Name = "Destroy All Entitys", Toggle = true, OnEnable = Master.DestroyAllEntitys, Description = "Ground Breaking" },
                        new Button { Name = "Destroy Entity Gun", Toggle = true, OnEnable = Master.DestroyEntityGun, Description = "Ground Breaking" },
                        new Button { Name = "Lock Room", Toggle = true, OnceEnable = Overpowered.Test, OnceDisable = Overpowered.Test2, Description = "Ground Breaking" },
                        new Button { Name = "Break Room", Toggle = true, OnceEnable = Overpowered.Test3, OnceDisable = Overpowered.Test4, Description = "Ground Breaking" },
                        new Button { Name = "Break Game Mode", Toggle = true, OnceEnable = Overpowered.BreakGameMode, OnceDisable = Overpowered.FixGamemode, Description = "Ground Breaking" },
                        new Button { Name = "Untag All", Toggle = true, OnEnable = Master.UnTagAll, Description = "Ground Breaking" },
                        new Button { Name = "Untag Gun", Toggle = true, OnEnable = Master.UnTagGun, Description = "Ground Breaking" },
                        new Button { Name = "Untag Self", Toggle = true, OnEnable = Master.UnTagSelf, Description = "Ground Breaking" },
                        new Button { Name = "Force Tag Lag", Toggle = true, OnceEnable = Master.CauseTagLag, OnceDisable = Master.FixTagLag, Description = "Ground Breaking" },
                        new Button { Name = "Material Spam All", Toggle = true, OnEnable = Master.MatAll, Description = "Ground Breaking" },
                        new Button { Name = "Material Spam Gun", Toggle = true, OnEnable = Master.MatGun, Description = "Ground Breaking" },
                        new Button { Name = "Slow All", Toggle = true, OnEnable = Master.SlowAll, Description = "Ground Breaking" },
                        new Button { Name = "Slow Gun", Toggle = true, OnEnable = Master.SlowGun, Description = "Ground Breaking" },
                        new Button { Name = "Vibrate All", Toggle = true, OnEnable = Master.VibrateAll, Description = "Ground Breaking" },
                        new Button { Name = "Vibrate Gun", Toggle = true, OnEnable = Master.VibrateGun, Description = " Ground Breaking" },
                        new Button { Name = "Random Block Gun", Toggle = true, OnEnable = Master.RandomBlockGun, Description = "Ground Breaking" }, // block mod working?
                        new Button { Name = "Destroy Block Gun", Toggle = true, OnEnable = Master.DestroyBlockGun, Description = "Ground Breaking" },
                        new Button { Name = "Block Crash All", Toggle = true, OnEnable = Master.BlockCrashAll, Description = "Ground Breaking" },
                        new Button { Name = "Block Crash Gun", Toggle = true, OnEnable = Master.BlockCrashGun, Description = " Ground Breaking" },
                    }
                },
                new Category {
                    Name = "Advantage",
                    Buttons = {
                        new Button { Name = "Tag All", Toggle = true, OnEnable = Advantages.TagAll },
                        new Button { Name = "Tag Gun", Toggle = true, OnEnable = Advantages.TagGun },
                        new Button { Name = "Tag Self", Toggle = true, OnEnable = Advantages.TagSelf },
                        new Button { Name = "Tag Aura", Toggle = true, OnEnable = Advantages.TagArua },
                        new Button { Name = "No Tag On Join", Toggle = true, OnceEnable = Advantages.NoTagOnJoin, OnceDisable = Advantages.TagOnJoin }
                    }
                },
                new Category {
                    Name = "Overpowered",
                    Subcategories = {
                        new SubCategory {
                            Name = "Detected Mods",
                            Buttons = {
                                new Button { Name = "Set Master Client", Toggle = false, OnceEnable = Dectected.SetMaster },
                                new Button { Name = "Unlimit Lobby", Toggle = false, OnceEnable = Dectected.UnlimitLobby },
                            }
                        }
                    },
                    Buttons = {
                        new Button { Name = "Barrel Fling Gun", Toggle = true, OnEnable = Overpowered.ElfFlingGun },// working?
                        new Button { Name = "Barrel Fling On Touch", Toggle = true, OnEnable = Overpowered.FlingOnTouch }, // working?
                        new Button { Name = "Barrel Fling On Your Touch", Toggle = true, OnEnable = Overpowered.FlingOnYourTouch }, // working?
                        new Button { Name = "Desync All", Toggle = true, OnEnable = Overpowered.FreezeRoom },
                        new Button { Name = "Freeze Room", Toggle = true, OnEnable = Overpowered.FreezeRoomV2 },
                        new Button { Name = "Lag All", Toggle = true, OnEnable = Overpowered.LagAll }, // detected?
                        new Button { Name = "Lag Gun", Toggle = true, OnEnable = Overpowered.LagGun },
                        new Button { Name = "Lag Aura", Toggle = true, OnEnable = Overpowered.LagAura },
                        new Button { Name = "Lag On Your Touch", Toggle = true, OnEnable = Overpowered.LagOnTouch },
                        new Button { Name = "Lag On Touch", Toggle = true, OnEnable = Overpowered.LagOnYourTouch },
                        new Button { Name = "Stutter All", Toggle = true, OnEnable = Overpowered.StutterAll },
                        new Button { Name = "Stutter Gun", Toggle = true, OnEnable = Overpowered.StutterGun },
                        new Button { Name = "Stutter Aura", Toggle = true, OnEnable = Overpowered.StutterAura },
                        new Button { Name = "Stutter On Your Touch", Toggle = true, OnEnable = Overpowered.StutterOnTouch },
                        new Button { Name = "Stutter On Touch", Toggle = true, OnEnable = Overpowered.StutterOnYourTouch },
                        new Button { Name = "Big Stutter All", Toggle = true, OnEnable = Overpowered.CrashAll },
                        new Button { Name = "Big Stutter Gun", Toggle = true, OnEnable = Overpowered.CrashGun },
                        new Button { Name = "Big Stutter Aura", Toggle = true, OnEnable = Overpowered.CrashAura },
                        new Button { Name = "Big Stutter On Your Touch", Toggle = true, OnEnable = Overpowered.CrashOnTouch },
                        new Button { Name = "Big Stutter On Touch", Toggle = true, OnEnable = Overpowered.CrashOnYourTouch },
                        new Button { Name = "Break Audio All", Toggle = true, OnEnable = Overpowered.BreakAudioAll },
                        new Button { Name = "Break Audio Gun", Toggle = true, OnEnable = Overpowered.BreakAudioGun },
                        //new Button { Name = "Destroy All", Toggle = true, OnEnable = Overpowered.DestroyAll },
                        //new Button { Name = "Destroy Gun", Toggle = true, OnEnable = Overpowered.DestroyGun },  working?
                    }
                },
                new Category {
                    Name = "Projectiles", //working?
                    Buttons = {
                        new Button { Name = "Shoot Snowballs", Toggle = true, OnEnable = Projectiles.ShootSnowBalls, OnceDisable = Projectiles.ProjectileCleanUp },
                        new Button { Name = "Shoot Big SnowBalls", Toggle = true, OnEnable = Projectiles.ShootBigSnowBalls, OnceDisable = Projectiles.ProjectileCleanUp },
                        new Button { Name = "Shoot Gifs", Toggle = true, OnEnable = Projectiles.ShootGifs, OnceDisable = Projectiles.ProjectileCleanUp },
                        new Button { Name = "Shoot Rocks", Toggle = true, OnEnable = Projectiles.ShootRocks, OnceDisable = Projectiles.ProjectileCleanUp },
                        new Button { Name = "Projectile Fling Gun", Toggle = true, OnEnable = Projectiles.FlingGun, OnceDisable = Projectiles.ProjectileCleanUp },
                        new Button { Name = "Anti Snowball Fing", Toggle = true, OnEnable = () => GameplayPatches.CheckForAOEKnockbackPatch.Fling = false, OnDisable = () => GameplayPatches.CheckForAOEKnockbackPatch.Fling = true },
                    }
                },
                new Category {
                    Name = "Credits",
                    Buttons = {

                        new Button { Name = "g3if: Creator", Toggle = false, Label = true },
                        new Button { Name = "Angel: Contributor", Toggle = false, Label = true },
                        new Button { Name = "Conetic: Contributor", Toggle = false, Label = true },
                        new Button { Name = "made w/ hate >:(", Toggle = false, Label = true }
                    }
                }
            };
        }
    }
}