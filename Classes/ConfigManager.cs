using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Juul
{
    [Serializable]
    public class SettingsConfig
    {
        public int ThemeIndex;
        public int PageButtons;
        public float MenuScale;
        public float ButtonInset;
        public float TextSize;
        public bool IsOutlined;
        public bool IsCatRotated;
        public bool IsCatLeft;
        public int GunStyle;
        public float GunLineWidth;
        public float GunSphereSize;
    }
    public static class Configs
    {
        public static string ConfigPath
        {
            get
            {
                return Path.Combine(Directory.GetParent(Application.dataPath).FullName, Core.Folder, "Configs");
            }
        }
        private static readonly string PathCfg =
            Path.Combine(ConfigPath, "settings.cfg");
        public static bool ConfigExists()
        {
            if (!Directory.Exists(ConfigPath))
                return false;
            return File.Exists(PathCfg);
        }
        public static void SaveConfig()
        {
            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }
            var cfg = new SettingsConfig
            {
                ThemeIndex = Core.ThemeValue,
                PageButtons = Core.PageBtnVer,
                MenuScale = Core.MenuWidth,
                ButtonInset = Core.BtnInset,
                TextSize = Core.TextSize,
                IsOutlined = Core.IsOutlined,
                IsCatRotated = Core.IsCatRotated,
                IsCatLeft = Core.IsCatLeft,
                GunStyle = (int)GunLib.currentLineStyle,
                GunLineWidth = GunLib.GunLineWidth,
                GunSphereSize = GunLib.SphereSize
            };
            File.WriteAllText(PathCfg, JsonUtility.ToJson(cfg, true));
        }
        public static void LoadConfig()
        {
            if (!File.Exists(PathCfg))
                return;
            string json = File.ReadAllText(PathCfg);
            if (string.IsNullOrEmpty(json))
                return;
            SettingsConfig cfg = JsonUtility.FromJson<SettingsConfig>(json);
            if (cfg == null)
                return;
            Core.ThemeValue = cfg.ThemeIndex;
            Core.PageBtnVer = cfg.PageButtons;
            Core.MenuWidth = cfg.MenuScale;
            Core.BtnInset = cfg.ButtonInset;
            Core.TextSize = cfg.TextSize;
            Core.IsOutlined = cfg.IsOutlined;
            Core.IsCatRotated = cfg.IsCatRotated;
            Core.IsCatLeft = cfg.IsCatLeft;
            GunLib.currentLineStyle = (GunLib.GunLineStyle)cfg.GunStyle;
            GunLib.GunLineWidth = cfg.GunLineWidth;
            GunLib.SphereSize = cfg.GunSphereSize;
            if (Buttons.ThemeButton != null)
                Buttons.ThemeButton.Name = $"Theme: {Core.GetCurrentThemeName()}";
            if (Buttons.GunStyleButton != null)
                Buttons.GunStyleButton.Name = "Gun Style: " + GunLib.currentLineStyle.ToString();
            if (Buttons.GunLineSizeButton != null)
                Buttons.GunLineSizeButton.Name = string.Format("Gun Line Size: {0}", GunLib.GunLineWidth);
            if (Buttons.GunSphereSizeButton != null)
                Buttons.GunSphereSizeButton.Name = string.Format("Gun Sphere Size: {0}", GunLib.SphereSize);
        }
    }
}