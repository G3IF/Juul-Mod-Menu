using GorillaGameModes;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        public static Button RoomJoinerButton;

        public static Category GetCategory(string name)
        {
            if (Modules == null) return null;
            return Modules.FirstOrDefault(c => c.Name == name);
        }

        public static Category[] Modules = null;
        public static Category EnabledCategory;

        public static bool ghostview = true;

        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            EnabledCategory = new Category { Name = "Enabled" };

            ThemeButton = new Button { Name = $"Theme: {Core.GetCurrentThemeName()}", Toggle = false, Incremental = true, Up = () => Core.ChangeTheme(true), Down = () => Core.ChangeTheme(false) };
            GunStyleButton = new Button { Name = "Gun Style: " + GunLib.currentLineStyle.ToString(), Toggle = false, Incremental = true, Up = () => GunLib.ChangeGunStyle(true), Down = () => GunLib.ChangeGunStyle(false) };
            GunLineSizeButton = new Button { Name = string.Format("Gun Line Size: {0}", GunLib.GunLineWidth), Toggle = false, Incremental = true, Up = () => GunLib.ChangeGunLineSize(true), Down = () => GunLib.ChangeGunLineSize(false) };
            GunSphereSizeButton = new Button { Name = string.Format("Gun Sphere Size: {0}", GunLib.SphereSize), Toggle = false, Incremental = true, Up = () => GunLib.ChangeGunSphereScale(true), Down = () => GunLib.ChangeGunSphereScale(false) };
            RoomJoinerButton = new Button { Name = "Join Room: ", Toggle = true, OnceEnable = () => KeyboardManager.IsJoiningRoom = true, OnceDisable = () => KeyboardManager.IsJoiningRoom = false };

            Modules = new Category[]
            {
                new Category {
                    Name = "Settings",
                    Buttons = {
                        new Button { Name = "discord.gg/juulgtag", Toggle = false, Label = true },
                    },
                    Subcategories = {
                        new Category {
                            Name = "Menu Settings",
                            Buttons = {
                                new Button { Name = "Save Config", Toggle = false, OnEnable = () => Configs.SaveConfig() },
                                new Button { Name = "Load Config", Toggle = false, OnEnable = () => Configs.LoadConfig() },

                                ThemeButton,

                                new Button { Name = "Page Buttons", Toggle = false, Incremental = true, Up = () => Core.ChangePageButtons(false), Down = () => Core.ChangePageButtons(true) },
                                new Button { Name = "Menu Size", Toggle = false, Incremental = true, Up = () => Core.ChangeMenuScale(true), Down = () => Core.ChangeMenuScale(false) },
                                new Button { Name = "Button Inset", Toggle = false, Incremental = true, Up = () => Core.ChangeButtonInset(true), Down = () => Core.ChangeButtonInset(false) },
                                new Button { Name = "Text Size", Toggle = false, Incremental = true, Up = () => Core.ChangeTextSize(true), Down = () => Core.ChangeTextSize(false) },

                                new Button { Name = "Make Menu Rounded", Toggle = false, OnEnable = () => { Core.IsRounded = !Core.IsRounded; Core.RebuildMenu();} },
                                new Button { Name = "Outline Menu", Toggle = false, OnEnable = () => Core.IsOutlined = !Core.IsOutlined },
                                new Button { Name = "Rotated Sidebar", Toggle = false, OnEnable = () => Core.IsCatRotated = !Core.IsCatRotated },
                                new Button { Name = "Sidebar Position", Toggle = false, OnEnable = () => Core.IsCatLeft = !Core.IsCatLeft },
                                new Button { Name = "Right Handed", Toggle = false, OnEnable = () => Core.IsRightHanded = !Core.IsRightHanded },
                                new Button { Name = "Boards Gradient", Toggle = true, Enabled = true, OnceEnable = () => Core.IsBoardGradientEnabled = true, OnceDisable = () => Core.IsBoardGradientEnabled = false },
                            }
                        },
                        new Category {
                            Name = "GunLib Settings",
                            Buttons = {
                                GunStyleButton,
                                GunLineSizeButton,
                                GunSphereSizeButton,
                                new Button { Name = "Test Gunlib", Toggle = true, OnEnable = () => GunLib.StartPointerSystem(() => {}, false) }, 
                            }
                        },
                        new Category {
                            Name = "Mod Settings",
                            Buttons = {
                                new Button { Name = "Advanced Nametags", Toggle = true, OnceEnable = () => Visual.AdvancedNametags = true, OnceDisable = () => Visual.AdvancedNametags = false },
                                new Button { Name = "Disable Notifications", Toggle = true, OnceEnable = () => NotifiLib.Disablenotifcations = true, OnceDisable = () => NotifiLib.Disablenotifcations = false },
                                new Button { Name = string.Format("Proj Delay: Normal"), Toggle = false, Incremental = true, Up = () => Projectiles.DelayChangeUp(), Down = () => Projectiles.DelayChangeDown() },
                                new Button { Name = "CS Projectiles", Toggle = true, OnceEnable = () => Projectiles.CsProjectiles = true, OnceDisable = () => Projectiles.CsProjectiles = false },
                            }
                        }
                    }
                },
                EnabledCategory,            
                PlayerMenu.GetPlayersCategory(),
                new Category {
                    Name = "Room",
                    Buttons = {
                        RoomJoinerButton,
                        new Button { Name = "Join Random", Toggle = false, OnEnable = Safety.JoinRandom },

                    }
                },
                new Category {
                    Name = "Movement",
                    Buttons = {
                        new Button { Name = "Platforms", Toggle = true, OnEnable = Movement.Platforms },
                        //new Button { Name = "Platform Type", Toggle = false, Incremental = true, Up = () => Movement.ChangePlatformType(true), Down = () => Movement.ChangePlatformType(false) },

                        new Button { Name = "WASD Fly", Toggle = true, OnEnable = Movement.WASDFly },
                        new Button { Name = "Flight", Toggle = true, OnEnable = Movement.Fly },
                        new Button { Name = "Fly Speed", Toggle = false, Incremental = true, Up = () => Movement.ChangeFlySpeed(true), Down = () => Movement.ChangeFlySpeed(false) },
                        new Button { Name = "Noclip Flight", Toggle = true, OnEnable = Movement.NoClipFly },

                        new Button { Name = "Noclip", Toggle = true, OnEnable = Movement.Noclip },

                        new Button { Name = "Speed Boost", Toggle = true, OnEnable = Movement.SpeedBoost },
                        new Button { Name = "Speed Boost Speed", Toggle = false, Incremental = true, Up = () => Movement.ChangeSpeedBoostSpeed(true), Down = () => Movement.ChangeSpeedBoostSpeed(false) },
                        new Button { Name = "Grip Speed Boost", Toggle = true, OnEnable = Movement.GripSpeedBoost },    

                        new Button { Name = "Pull Mod", Toggle = true, OnEnable = Movement.PullMod },

                        new Button { Name = "Wall Assist", Toggle = true, OnEnable = Movement.WallAssist },
                        new Button { Name = "Wall Walk", Toggle = true, OnEnable = Movement.WallWalk },
                        new Button { Name = "Wall Walk Strength", Toggle = false, Incremental = true, Up = () => Movement.AdjustWallWalkStrength(true), Down = () => Movement.AdjustWallWalkStrength(false) },
                        new Button { Name = "Legit Wall Walk", Toggle = true, OnEnable = Movement.LegitimateWallWalk },

                        new Button { Name = "Teleport Gun", Toggle = true, OnEnable = Movement.TeleportGun },

                        new Button { Name = "Zero Gravity", Toggle = true, OnEnable = Movement.ZeroGravity },
                        new Button { Name = "Low Gravity", Toggle = true, OnEnable = Movement.LowGravity },
                        new Button { Name = "High Gravity", Toggle = true, OnEnable = Movement.HighGravity },

                        new Button { Name = "Bouncy", Toggle = true, OnceEnable = Movement.Bouncy, OnceDisable = Movement.ResetBouncy },
                        new Button { Name = "Check Point", Toggle = true, OnEnable = Movement.Checkpoint, OnDisable = Movement.DestroyCheckpoint }
                    }
                },
                new Category {
                    Name = "Client",
                    Buttons = {
                        new Button { Name = "Invis Monkey", Toggle = true, OnEnable = Players.InvisibleMonke },
                        new Button { Name = "Ghost Monkey", Toggle = true, OnEnable = Players.GhostMonke },
                        new Button { Name = "Enable Ghost View", Toggle = true, Enabled = true, OnceEnable = () => ghostview = true, OnceDisable = () => Players.GhostviewClean() },

                        new Button { Name = "Long Arms", Toggle = true, OnEnable = Players.RArms, OnDisable = Players.DisableLongArms },
                        new Button { Name = "Short Arms", Toggle = true, OnEnable = Players.SArms, OnDisable = Players.DisableLongArms },
                        new Button { Name = "Fix Arms", Toggle = true, OnEnable = Players.DisableLongArms },
                        new Button { Name = "Change Arm Lenth", Toggle = true, OnEnable = Players.ChangeArmLenth, OnDisable = Players.DisableLongArms },

                        new Button { Name = "Spin bot", Toggle = true, OnEnable = Players.Spinbot },

                        //new Button { Name = "Uncap FPS", Toggle = false, OnEnable = Safety.UncapFPS },
                       // new Button { Name = "Set 144 FPS", Toggle = false, OnEnable = Safety.SetFPS144 },
                       // new Button { Name = "Set 120 FPS", Toggle = false, OnEnable = Safety.SetFPS120 },
                      //  new Button { Name = "Set 90 FPS", Toggle = false, OnEnable = Safety.SetFPS90 },
                      //  new Button { Name = "Set 80 FPS", Toggle = false, OnEnable = Safety.SetFPS80 },
                      //  new Button { Name = "Set 72 FPS", Toggle = false, OnEnable = Safety.SetFPS72 },
                      //  new Button { Name = "Set 60 FPS", Toggle = false, OnEnable = Safety.SetFPS60 },
                      //  new Button { Name = "Set 45 FPS", Toggle = false, OnEnable = Safety.SetFPS45 },
                      //  new Button { Name = "Set 15 FPS", Toggle = false, OnEnable = Safety.SetFPS15 },
                      //  new Button { Name = "Set 1 FPS", Toggle = false, OnEnable = Safety.SetFPS1 }
                    }
                },
                new Category {
                    Name = "Safety",
                    Buttons = {
                        new Button { Name = "Quit Game", Toggle = false, OnEnable = Safety.QuitGame },

                        new Button { Name = "Semi-Anti Ban", Toggle = true, OnEnable = Safety.AntiBan },
                        new Button { Name = "Spoof Player", Toggle = false, OnEnable = Safety.SpoofPlayer },
                        new Button { Name = "Anti RPC Kick", Toggle = true, OnEnable = Safety.AntiRPCKick },
                        new Button { Name = "Anti Report Disconnect", Toggle = true, OnEnable = Safety.AntiReportDisconnect },
                        new Button { Name = "Anti Report Reconnect", Toggle = true, OnEnable = Safety.AntiReportReconnect },
                        //new Button { Name = "Anti Report Notify", Toggle = true, OnEnable = Safety.AntiReportNotify },

                        new Button { Name = "Restart Game", Toggle = false, OnEnable = Safety.RestartGame },

                        new Button { Name = "Bypass Vc Ban", Toggle = true, OnEnable = Safety.BypassVCBan },

                        new Button { Name = "Disable Map Triggers", Toggle = true, OnceEnable = Safety.DisableMapTriggers, OnceDisable = Safety.EnableMapTriggers },
                        new Button { Name = "Disable Net Triggers", Toggle = true, OnceEnable = Safety.DisableNetworkTriggers, OnceDisable = Safety.EnableNetworkTriggers },
                        new Button { Name = "Disable Quit Box", Toggle = true, OnceEnable = Safety.DisableQuitBox, OnceDisable = Safety.EnableQuitBox },
                        new Button { Name = "Disable AFK Kick", Toggle = true, OnceEnable = Safety.DisableAntiAFK, OnceDisable = Safety.EnableAntiAFK },

                        new Button { Name = "Left Trigger Disconnect", Toggle = true, OnEnable = Safety.DisconnectLT },
                        new Button { Name = "Right Trigger Disconnect", Toggle = true, OnEnable = Safety.DisconnectRT },

                        new Button { Name = "No Finger Movement", Toggle = true, OnEnable = Safety.NoFinger },
                    }
                },
                new Category {
                    Name = "Visual",
                    Buttons = {
                        new Button { Name = "Array List [PC]", Toggle = true, OnEnable = Visual.EnableArrayList, OnceDisable = Visual.DisableArrayList }, // vr coming soon
                        new Button { Name = "Custom HUD", Toggle = true, OnEnable = Visual.PlayerInfo, OnceDisable = Visual.CleanupPlayerInfo },
                        new Button { Name = "Name Tags", Toggle = true, OnEnable = Visual.PlayerNameESP, OnceDisable = Visual.CleanupPlayerNameESP },

                        new Button { Name = "Chams", Toggle = true, OnEnable = Visual.Chams, OnceDisable = Visual.CleanupChams },
                        //new Button { Name = "Infection Chams", Toggle = true, OnEnable = Visual.InfectionChams, OnceDisable = Visual.CleanupInfectionChams },

                        new Button { Name = "Bone ESP", Toggle = true, OnEnable = Visual.BoneESP, OnceDisable = Visual.CleanupBoneESP },
                        //new Button { Name = "Infection Bone ESP", Toggle = true, OnEnable = Visual.InfectionBoneESP, OnceDisable = Visual.CleanupInfectionBoneESP },

                        new Button { Name = "Tracers", Toggle = true, OnEnable = Visual.Tracers, OnceDisable = Visual.CleanupTracers },
                       // new Button { Name = "Infection Tracers", Toggle = true, OnEnable = Visual.InfectionTracers, OnceDisable = Visual.CleanupInfectionTracers },

                        new Button { Name = "2D Box ESP", Toggle = true, OnEnable = Visual.Box2DESP, OnceDisable = Visual.CleanupBox2DESP },
                        new Button { Name = "2D Corner ESP", Toggle = true, OnEnable = Visual.Box2DCornerESP, OnceDisable = Visual.CleanupBox2DCornerESP },
                        new Button { Name = "3D Box ESP", Toggle = true, OnEnable = Visual.Box3DESP, OnceDisable = Visual.CleanupBox3DESP },
                        new Button { Name = "3D Corner ESP", Toggle = true, OnEnable = Visual.Box3DESPV2, OnceDisable = Visual.CleanupBox3DESPV2 },

                        new Button { Name = "Menu Theme Rig [CS]", Toggle = true, OnEnable = Visual.MenuThemeRig, OnceDisable = Visual.RigColorFix },

                        new Button { Name = "Rainbow All [CS]", Toggle = true, OnEnable = Visual.OutcastAll },
                    }
                },
                new Category {
                    Name = "Fun",
                    Buttons = {
                        new Button { Name = "RGB Monkey [Stump]", Toggle = true, OnEnable = Fun.FadeMonkey },
                        new Button { Name = "Hard RGB Monkey [Stump]", Toggle = true, OnEnable = Fun.FadeMonkeyHardRGB },
                        new Button { Name = "Epilepsy Monkey [Stump]", Toggle = true, OnEnable = Fun.FlashMonkey },
                        new Button { Name = "B&W Epilepsy Monkey [Stump]", Toggle = true, OnEnable = Fun.BAWFlashMonkey },
                        new Button { Name = "Copy Color Gun [Stump]", Toggle = true, OnEnable = Fun.CopyColorGun },

                        new Button { Name = "Unlock All [Gadgets]", Toggle = true, OnEnable = Fun.SIUnlockAll },
                        new Button { Name = "Steal All Terminals [Gadgets]", Toggle = true, OnEnable = Fun.YoinkTerms },
                        new Button { Name = "Give All Resources [Gadgets]", Toggle = true, OnEnable = Fun.GiveAllResources },
                        new Button { Name = "Always Own A Terminal [Gadgets]", Toggle = true, OnEnable = Fun.AlwaysOwnTerminals },
                        new Button { Name = "Disable Terminal Timeout [Gadgets]", Toggle = true, OnEnable = Fun.DisableTerminalTimeout },

                        new Button { Name = "Shoot Hoverboards", Toggle = true, OnEnable = Fun.ShootHoverBoards },

                        new Button { Name = "Bass Sound Spam", Toggle = true, OnEnable = Fun.BassSoundSpam },
                        new Button { Name = "Metal Sound Spam", Toggle = true, OnEnable = Fun.MetalSoundSpam },
                        new Button { Name = "Wolf Sound Spam", Toggle = true, OnEnable = Fun.WolfSoundSpam },
                        new Button { Name = "Cat Sound Spam", Toggle = true, OnEnable = Fun.CatSoundSpam },
                        new Button { Name = "Turkey Sound Spam", Toggle = true, OnEnable = Fun.TurkeySoundSpam },
                        new Button { Name = "Frog Sound Spam", Toggle = true, OnEnable = Fun.FrogSoundSpam },
                        new Button { Name = "Bee Sound Spam", Toggle = true, OnEnable = Fun.BeeSoundSpam },
                        new Button { Name = "Squeak Sound Spam", Toggle = true, OnEnable = Fun.SqueakSoundSpam },
                        new Button { Name = "Earrape Sound Spam", Toggle = true, OnEnable = Fun.EarrapeSoundSpam },
                        new Button { Name = "Ding Sound Spam", Toggle = true, OnEnable = Fun.DingSoundSpam },
                        new Button { Name = "Piano Sound Spam", Toggle = true, OnEnable = Fun.PianoSoundSpam },
                        new Button { Name = "Big Crystal Sound Spam", Toggle = true, OnEnable = Fun.BigCrystalSoundSpam },
                        new Button { Name = "Pan Sound Spam", Toggle = true, OnEnable = Fun.PanSoundSpam },
                        new Button { Name = "AK-47 Sound Spam", Toggle = true, OnEnable = Fun.AK47SoundSpam },
                        new Button { Name = "Tick Sound Spam", Toggle = true, OnEnable = Fun.TickSoundSpam },
                        new Button { Name = "Random Sound Spam", Toggle = true, OnEnable = Fun.RandomSoundSpam },
                        new Button { Name = "Crystal Sound Spam", Toggle = true, OnEnable = Fun.CrystalSoundSpam },
                        new Button { Name = "Siren Sound Spam", Toggle = true, OnEnable = Fun.SirenSoundSpam },
                        new Button { Name = "Play Random Sounds", Toggle = true, OnEnable = Fun.PlayRandomSounds },

                        new Button { Name = "Play Jman Scream", Toggle = false, OnEnable = Fun.PlayJmanYell },
                        new Button { Name = "Spam Jman Scream", Toggle = true, OnEnable = Fun.SpamJmanYell },

                        new Button { Name = "Grab Bug", Toggle = true, OnEnable = Fun.GrabBug },
                        new Button { Name = "Orbit Bug", Toggle = true, OnEnable = Fun.OrbitBug },
                        new Button { Name = "Spaz Bug", Toggle = true, OnEnable = Fun.SpazBug },
                        new Button { Name = "Destroy Bug", Toggle = true, OnEnable = Fun.DestroyBug },

                        new Button { Name = "Grab Bat", Toggle = true, OnEnable = Fun.GrabBat },
                        new Button { Name = "Orbit Bat", Toggle = true, OnEnable = Fun.OrbitBat },
                        new Button { Name = "Spaz Bat", Toggle = true, OnEnable = Fun.SpazBat },
                        new Button { Name = "Destroy Bat", Toggle = true, OnEnable = Fun.DestroyBat },

                        new Button { Name = "Grab Firefly", Toggle = true, OnEnable = Fun.GrabFirefly },
                        new Button { Name = "Orbit Firefly", Toggle = true, OnEnable = Fun.OrbitFirefly },
                        new Button { Name = "Spaz Firefly", Toggle = true, OnEnable = Fun.SpazFirefly },
                        new Button { Name = "Destroy Firefly", Toggle = true, OnEnable = Fun.DestroyFirefly },

                        new Button { Name = "Set Name to HIDE", Toggle = false, OnEnable = () => Fun.ChangeNameTo("HIDE") },
                        new Button { Name = "Set Name to SEEK", Toggle = false, OnEnable = () => Fun.ChangeNameTo("SEEK") },
                        new Button { Name = "Set Name to RUN", Toggle = false, OnEnable = () => Fun.ChangeNameTo("RUN") },
                        new Button { Name = "Set Name to HIDDEN", Toggle = false, OnEnable = () => Fun.ChangeNameTo("HIDDEN") },
                        new Button { Name = "Set Name to FOUND", Toggle = false, OnEnable = () => Fun.ChangeNameTo("FOUND") },
                        new Button { Name = "Set Name to BEHINDYOU", Toggle = false, OnEnable = () => Fun.ChangeNameTo("BEHINDYOU") },
                        new Button { Name = "Set Name to STATUE", Toggle = false, OnEnable = () => Fun.ChangeNameTo("STATUE") },
                        new Button { Name = "Set Name to GHOST", Toggle = false, OnEnable = () => Fun.ChangeNameTo("GHOST") },
                        new Button { Name = "Set Name to HAUNT", Toggle = false, OnEnable = () => Fun.ChangeNameTo("HAUNT") },
                        new Button { Name = "Set Name to CREEP", Toggle = false, OnEnable = () => Fun.ChangeNameTo("CREEP") },
                        new Button { Name = "Set Name to STALKER", Toggle = false, OnEnable = () => Fun.ChangeNameTo("STALKER") },
                        new Button { Name = "Set Name to 404", Toggle = false, OnEnable = () => Fun.ChangeNameTo("404") },
                        new Button { Name = "Set Name to 7NV", Toggle = false, OnEnable = () => Fun.ChangeNameTo("7NV") },
                        new Button { Name = "Set Name to JUULONTOP", Toggle = false, OnEnable = () => Fun.ChangeNameTo("JUULONTOP") },

                        new Button { Name = "Unlock All Cosmetic [CS]", Toggle = false, OnEnable = () => Fun.UnlockAllCosmetics() },
                        new Button { Name = "Unlock Subscription [CS]", Toggle = true, OnEnable = Fun.UnlockSubscription },
                        new Button { Name = "Unlock All Shinyrocks [CS]", Toggle = false, OnEnable = () => Fun.GiveUnlimitedShinyRocks() },

                        new Button { Name = "Sticky Holdables In Hand", Toggle = true, OnEnable = () => Fun.StickHoldables() },
                        new Button { Name = "Spin Holdables In Hand", Toggle = true, OnEnable = () => Fun.SpinHoldables() },
                        new Button { Name = "Juggle Holdables", Toggle = true, OnEnable = () => Fun.JuggleHoldables() },
                    }
                },
                new Category {
                    Name = "Master",
                    Buttons = {
                        new Button { Name = "Set Guardian Self", Toggle = false, OnEnable = Master.SetGuardianSelf },
                        new Button { Name = "Set Guardian Gun", Toggle = true, OnEnable = Master.SetGuardianGun },
                        new Button { Name = "Set Guardian Aura", Toggle = true, OnEnable = Master.GuardianAura },
                        new Button { Name = "Set Guardian On Your Touch", Toggle = true, OnEnable = Master.GuardianOnTouch },
                        new Button { Name = "Set Guardian On Touch", Toggle = true, OnEnable = Master.GuardianOnYourTouch },

                        new Button { Name = "UnGuardian Self", Toggle = false, OnEnable = Master.UnGuardianSelf },
                        new Button { Name = "UnGuardian Gun", Toggle = true, OnEnable = Master.UnGuardianGun },
                        new Button { Name = "UnGuardian Aura", Toggle = true, OnEnable = Master.UnGuardianAura },
                        new Button { Name = "UnGuardian On Your Touch", Toggle = true, OnEnable = Master.UnGuardianOnTouch },
                        new Button { Name = "UnGuardian On Touch", Toggle = true, OnEnable = Master.UnGuardianOnYourTouch },

                        new Button { Name = "Open All Elevator Doors", Toggle = false, OnEnable = Master.OpenElevatorDoor },
                        new Button { Name = "Close All Elevator Doors", Toggle = false, OnEnable = Master.CloseElevatorDoor },

                        new Button { Name = "Teleport All Elevators To Stump", Toggle = false, OnEnable = Master.TeleportToStump },
                        new Button { Name = "Teleport All Elevators To City", Toggle = false, OnEnable = Master.TeleportToCity },
                        new Button { Name = "Teleport All Elevators To Ghost Reactor", Toggle = false, OnEnable = Master.TeleportToGhostReactor },
                        new Button { Name = "Teleport All Elevators To Monke Blocks", Toggle = false, OnEnable = Master.TeleportToMonkeBlocks },
                        new Button { Name = "Teleport All Elevators All Players To Me", Toggle = false, OnEnable = Master.TeleportAllPlayersToMe },

                        new Button { Name = "Freeze Elevator Doors Open", Toggle = true, OnEnable = Master.FreezeElevatorDoorsOpen },
                        new Button { Name = "Freeze Elevator Doors Closed", Toggle = true, OnEnable = Master.FreezeElevatorDoorsClosed },

                        new Button { Name = "Break All Elevators", Toggle = true, OnEnable = Master.BreakElevator },

                        new Button { Name = "Open Basement Door", Toggle = true, OnEnable = Master.OpenBasementDoor },
                        new Button { Name = "Close Basement Door", Toggle = true, OnEnable = Master.CloseBasementDoor },

                        new Button { Name = "Break Basement Door", Toggle = true, OnEnable = Master.BreakBasementDoor },

                        new Button { Name = "Purchase All Tools", Toggle = false, OnEnable = Master.PurchaseAllStationTools },
                        new Button { Name = "Kill All Enemies", Toggle = false, OnEnable = Master.KillAllEnemies },
                        new Button { Name = "GhostReactor Kill All", Toggle = false, OnEnable = Master.GhostReactorKillAll },
                        new Button { Name = "GhostReactor Kill Gun V1", Toggle = true, OnEnable = Master.GhostReactorKillGun },
                        new Button { Name = "GhostReactor Kill Gun V2", Toggle = true, OnEnable = Master.GhostReactorKillGun2 },
                        new Button { Name = "GhostReactor Shield All", Toggle = false, OnEnable = Master.GhostReactorSheildAll },
                        new Button { Name = "GhostReactor Shield Gun", Toggle = true, OnEnable = Master.GhostReactorSheildGun },

                        new Button { Name = "Start Shift", Toggle = false, OnEnable = Master.StartShiftNow },
                        new Button { Name = "End Shift", Toggle = false, OnEnable = Master.EndShiftNow },
                        new Button { Name = "Max Difficulty", Toggle = false, OnEnable = Master.SetMaxDifficulty },
                        new Button { Name = "Set Depth Level 1", Toggle = false, OnEnable = () => Master.SetDepthLevel(1) },
                        new Button { Name = "Set Depth Level 5", Toggle = false, OnEnable = () => Master.SetDepthLevel(5) },
                        new Button { Name = "Set Depth Level 10", Toggle = false, OnEnable = () => Master.SetDepthLevel(10) },
                        new Button { Name = "Spawn Core", Toggle = false, OnEnable = Master.GhostSpawnCoreGun },
                        new Button { Name = "Spawn Chaos Seed", Toggle = false, OnEnable = Master.GhostSpawnChaosSeedGun },
                        new Button { Name = "Spawn Super Core", Toggle = false, OnEnable = Master.GhostSpawnSuperCoreGun },

                        new Button { Name = "Destroy All Entitys", Toggle = true, OnEnable = Master.DestroyAllEntitys, Description = "Ground Breaking" },
                        new Button { Name = "Destroy Entity Gun", Toggle = true, OnEnable = Master.DestroyEntityGun, Description = "Ground Breaking" },

                        new Button { Name = "Despawn All Critters", Toggle = false, OnEnable = Master.DespawnAllCritters },
                        new Button { Name = "Make Critters Peaceful", Toggle = false, OnEnable = Master.MakeCrittersPeaceful },
                        new Button { Name = "Make Critters Eat", Toggle = false, OnEnable = Master.MakeCrittersEat },
                        new Button { Name = "Make Critters Run", Toggle = false, OnEnable = Master.MakeCrittersRun },
                        new Button { Name = "Make Critters Sleep", Toggle = false, OnEnable = Master.MakeCrittersSleep },
                        new Button { Name = "Spawn Stun Bomb", Toggle = false, OnEnable = Master.SpawnStunBombAtPlayer },
                        new Button { Name = "Spawn Noise Maker", Toggle = false, OnEnable = Master.SpawnNoiseMakerAtPlayer },
                        new Button { Name = "Spawn Sticky Trap", Toggle = false, OnEnable = Master.SpawnStickyTrapAtPlayer },
                        new Button { Name = "Mass Stun", Toggle = false, OnEnable = Master.TriggerMassStun },
                        new Button { Name = "Giant Critters", Toggle = false, OnEnable = Master.GiantCritters },
                        new Button { Name = "Tiny Critters", Toggle = false, OnEnable = Master.TinyCritters },
                        new Button { Name = "Reset Critter Sizes", Toggle = false, OnEnable = Master.ResetCritterSizes },

                        new Button { Name = "Critters Stun Bomb Gun", Toggle = true, OnEnable = Master.CritterStunBombGun },
                        new Button { Name = "Critters Sticky Trap Gun", Toggle = true, OnEnable = Master.CritterStickyTrapGun },
                        new Button { Name = "Critters Noise Maker Gun", Toggle = true, OnEnable = Master.CritterNoiseMakerGun },
                        new Button { Name = "Critters Cage Gun", Toggle = true, OnEnable = Master.CritterCageGun },
                        new Button { Name = "Critters Food Gun", Toggle = true, OnEnable = Master.CritterFoodGun },
                        new Button { Name = "Critters Sticky Goo Gun", Toggle = true, OnEnable = Master.CritterStickyGooGun },
                        new Button { Name = "Critters Spawn Gun", Toggle = true, OnEnable = Master.CritterSpawnGun },

                        new Button { Name = "Activate Mines Lucy Second Look", Toggle = false, OnEnable = Master.ActivateMinesLucySecondLook },
                        new Button { Name = "Mines Lucy Jumpscare All", Toggle = false, OnEnable = Master.MineLucyJumpscareAll },
                        new Button { Name = "Mines Lucy Jumpscare Gun", Toggle = true, OnEnable = Master.MineLucyJumpscareGun },

                        new Button { Name = "Lock Room", Toggle = true, OnceEnable = Overpowered.Test, OnceDisable = Overpowered.Test2, Description = "Ground Breaking" },
                        new Button { Name = "Break Room", Toggle = true, OnceEnable = Overpowered.Test3, OnceDisable = Overpowered.Test4, Description = "Ground Breaking" },

                        new Button { Name = "Break Game Mode", Toggle = true, OnceEnable = Overpowered.BreakGameMode, OnceDisable = Overpowered.FixGamemode, Description = "Ground Breaking" },

                        new Button { Name = "Untag All", Toggle = true, OnEnable = Master.UnTagAll, Description = "Ground Breaking" },
                        new Button { Name = "Untag Gun", Toggle = true, OnEnable = Master.UnTagGun, Description = "Ground Breaking" },
                        new Button { Name = "Untag Self", Toggle = true, OnEnable = Master.UnTagSelf, Description = "Ground Breaking" },

                        new Button { Name = "Force Tag Lag", Toggle = true, OnceEnable = Master.CauseTagLag, OnceDisable = Master.FixTagLag, Description = "Ground Breaking" },

                        new Button { Name = "Material Spam All", Toggle = true, OnEnable = Master.MatAll, Description = "Ground Breaking" },
                        new Button { Name = "Material Spam Gun", Toggle = true, OnEnable = Master.MatGun, Description = "Ground Breaking" },

                        new Button { Name = "Activate Grey Zone ", Toggle = false, OnEnable = Master.ActivateGreyZone, Description = "Ground Breaking" },
                        new Button { Name = "DeActivate Grey Zone", Toggle = false, OnEnable = Master.DeactivateGreyZone, Description = "Ground Breaking" },
                        new Button { Name = "Flash Grey Zone", Toggle = true, OnEnable = Master.SpazGreyZone, Description = "Ground Breaking" },

                        new Button { Name = "Paintbrawl All Red Team", Toggle = false, OnEnable = Master.AllRedTeam },
                        new Button { Name = "Paintbrawl All Blue Team", Toggle = false, OnEnable = Master.AllBlueTeam },

                        new Button { Name = "Paintbawl Kill All Players", Toggle = false, OnEnable = Master.KillAll },
                        new Button { Name = "Paintbawl Revive All Players", Toggle = false, OnEnable = Master.HealAll },
                        new Button { Name = "Paintbawl Stun All Players", Toggle = false, OnEnable = Master.StunAll },

                        new Button { Name = "Paintbawl Kill Gun", Toggle = true, OnEnable = Master.InstantKillGun },
                        new Button { Name = "Paintbawl Stun Gun", Toggle = true, OnEnable = Master.StunGun },
                        new Button { Name = "Paintbawl Team Changer Gun", Toggle = true, OnEnable = Master.TeamChangerGun },
                        new Button { Name = "Paintbawl Revive Gun", Toggle = true, OnEnable = Master.HealGun },

                        new Button { Name = "Virtual Stump Kick All", Toggle = true, OnEnable = Fun.VirtualStumpKickAll },
                        new Button { Name = "Virtual Stump Kick Gun", Toggle = true, OnEnable = Fun.VirtualStumpKickGun },

                        new Button { Name = "Slow All", Toggle = true, OnEnable = Master.SlowAll, Description = "Ground Breaking" },
                        new Button { Name = "Slow Gun", Toggle = true, OnEnable = Master.SlowGun, Description = "Ground Breaking" },

                        new Button { Name = "Vibrate All", Toggle = true, OnEnable = Master.VibrateAll, Description = "Ground Breaking" },
                        new Button { Name = "Vibrate Gun", Toggle = true, OnEnable = Master.VibrateGun, Description = " Ground Breaking" },

                        new Button { Name = "Random Block Gun", Toggle = true, OnEnable = Master.RandomBlockGun, Description = "Ground Breaking" },

                        new Button { Name = "Destroy Block Gun", Toggle = true, OnEnable = Master.DestroyBlockGun, Description = "Ground Breaking" },
                        new Button { Name = "Destroy All Blocks", Toggle = true, OnEnable = Master.RecycleAllBlocks, Description = "Ground Breaking" },

                        new Button { Name = "Block Crash All", Toggle = true, OnEnable = Master.BlockCrashAll, Description = "Ground Breaking" },
                        new Button { Name = "Block Crash Gun", Toggle = true, OnEnable = Master.BlockCrashGun, Description = "Ground Breaking" },

                        new Button { Name = "Block Freeze Gun", Toggle = true, OnEnable = Master.BlockFreezeGun, Description = "Ground Breaking" },
                        new Button { Name = "Block Freeze All", Toggle = true, OnEnable = Master.BlockFreezeAll, Description = "Ground Breaking" },
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
                    Buttons = {

                        new Button { Name = "Buy Barrel", Toggle = true, OnEnable = Overpowered.BuyBarrel },

                        new Button { Name = "Lock Room", Toggle = true, OnEnable = Overpowered.LockRoom },
                        new Button { Name = "UnLock Room", Toggle = true, OnEnable = Overpowered.UnlockRoom },
                        new Button { Name = "Spaz Room", Toggle = true, OnEnable = Overpowered.SpazRoom },

                        new Button { Name = "Desync All", Toggle = true, OnEnable = Overpowered.FreezeRoomV2 },
                        new Button { Name = "Freeze Room", Toggle = true, OnEnable = Overpowered.FreezeRoom },

                        new Button { Name = "Lag All", Toggle = true, OnEnable = Overpowered.LagAll },
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

                        new Button { Name = "Strong Lag All", Toggle = true, OnEnable = Overpowered.StrongLagAll },
                        new Button { Name = "Strong Lag Gun", Toggle = true, OnEnable = Overpowered.StrongLagGun },
                        new Button { Name = "Strong Lag Aura", Toggle = true, OnEnable = Overpowered.StrongLagAura },
                        new Button { Name = "Strong Lag On Your Touch", Toggle = true, OnEnable = Overpowered.StrongLagTouch },
                        new Button { Name = "Strong Lag On Touch", Toggle = true, OnEnable = Overpowered.StrongLagOnYourTouch },

                    }
                },
                new Category {
                    Name = "Projectiles",
                    Buttons = {
                        new Button { Name = "Anti Snowball Fing", Toggle = true, OnEnable = () => GameplayPatches.CheckForAOEKnockbackPatch.Fling = false, OnDisable = () => GameplayPatches.CheckForAOEKnockbackPatch.Fling = true },
                    }
                },
                new Category {
                    Name = "Soundboard",
                    Buttons = { 

                    }
                },
                new Category {
                    Name = "Credits",
                    Buttons = {
                        new Button { Name = "g3if: Founder", Toggle = false, Label = true },
                        new Button { Name = "PlopYert: Developer", Toggle = false, Label = true },  // wizzy / angel
                        new Button { Name = "Conetic: Contributor", Toggle = false, Label = true },
                        new Button { Name = "Status : Undetected", Toggle = false, Label = true },
                        new Button { Name = "made with love <3", Toggle = false, Label = true },
                    }
                }
            };
        }
        public static void RefreshEnabledCategory()
        {
            if (Modules == null || EnabledCategory == null) return;
            EnabledCategory.Buttons.Clear();
            for (int i = 0; i < Modules.Length; i++)
            {
                if (Modules[i] == EnabledCategory) continue;
                for (int j = 0; j < Modules[i].Buttons.Count; j++)
                {
                    Button btn = Modules[i].Buttons[j];
                    if (btn.Toggle && btn.Enabled)
                    {
                        EnabledCategory.Buttons.Add(btn);
                    }
                }
            }
        }
    }
}
