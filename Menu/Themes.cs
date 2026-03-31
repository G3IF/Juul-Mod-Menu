using System;
using System.Runtime.CompilerServices;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace Juul
{
    public class Theme
    {
        public string Name = "Placeholder";
        public float Speed = 0.66f;
        public Color[] Colors;
        public Color Color;
    }

    public class Themes
    {
        public static Theme[] List = new Theme[]
        {
            new Theme()
            {
                Name = "Juul",
                Colors = new Color[]
                {
                    Colors.Blend(Colors.Blend(Color.green, Color.cyan), Color.white),
                    Colors.Blend(Color.magenta, Color.white)
                }
            },
            new Theme()
            {
                Name = "Shadow",
                Colors = new Color[]
                {
                    Juul.Colors.Blend(Color.gray, Color.black, Color.black),
                    Juul.Colors.Blend(Color.gray, Color.black, Color.black, Color.black),
                }
            },
            new Theme()
            {
                Name = "Twilight",
                Colors = new Color[]
                {
                    Colors.Blend(Colors.Blend(Color.blue, Color.magenta), Color.white),
                    Colors.Blend(Color.blue, Color.magenta)
                }
            },
            new Theme()
            {
                Name = "Deep Sea",
                Colors = new Color[]
                {
                    Colors.Blend(Color.blue, Color.cyan),
                    Color.blue
                }
            },
            new Theme()
            {
                Name = "Rose",
                Colors = new Color[]
                {
                    Colors.Blend(Colors.Blend(Color.magenta, Color.magenta, Color.red), Color.white),
                    Colors.Blend(Color.magenta, Color.magenta, Color.red)
                }
            },
            new Theme()
            {
                Name = "Inferno",
                Colors = new Color[]
                {
                    Color.red,
                    Colors.Blend(Color.black, Color.red)
                }
            },
            new Theme()
            {
                Name = "Sunset",
                Colors = new Color[]
                {
                    Colors.Blend(Color.red, Color.yellow),
                    Colors.Blend(Color.magenta, Color.red, Color.red)
                }
            },
            new Theme()
            {
                Name = "Forest",
                Colors = new Color[]
                {
                    Color.green,
                    Colors.Blend(Color.green, Color.black)
                }
            },
            new Theme()
            {
                Name = "Frost",
                Colors = new Color[]
                {
                    Colors.Blend(Color.cyan, Color.white),
                    Color.cyan
                }
            },
            new Theme()
            {
                Name = "Midnight",
                Colors = new Color[]
                {
                    Colors.Blend(Color.blue, Color.black),
                    Color.black
                }
            },
            new Theme()
            {
                Name = "Coral",
                Colors = new Color[]
                {
                    Colors.Blend(Color.red, Color.magenta, Color.white),
                    Colors.Blend(Color.red, Color.yellow)
                }
            },
            new Theme()
            {
                Name = "Amber",
                Colors = new Color[]
                {
                    Colors.Blend(Color.yellow, Color.red),
                    Colors.Blend(Color.red, Color.black)
                }
            },
            new Theme()
            {
                Name = "Storm",
                Colors = new Color[]
                {
                    Colors.Blend(Color.gray, Color.blue),
                    Colors.Blend(Color.black, Color.blue)
                }
            },
            new Theme()
            {
                Name = "Neon",
                Colors = new Color[]
                {
                    Colors.Blend(Color.magenta, Color.cyan),
                    Color.magenta
                }
            },
            new Theme()
            {
                Name = "Aurora",
                Colors = new Color[]
                {
                    Colors.Blend(Color.green, Color.cyan, Color.white),
                    Colors.Blend(Color.blue, Color.magenta)
                }
            },
            new Theme()
            {
                Name = "Copper",
                Colors = new Color[]
                {
                    Colors.Blend(Color.red, Color.yellow, Color.black),
                    Colors.Blend(Color.red, Color.black, Color.black)
                }
            },
            new Theme()
            {
                Name = "Sakura",
                Colors = new Color[]
                {
                    Colors.Blend(Color.magenta, Color.white, Color.white),
                    Colors.Blend(Color.magenta, Color.red, Color.white)
                }
            },
            new Theme()
            {
                Name = "Venom",
                Colors = new Color[]
                {
                    Colors.Blend(Color.green, Color.yellow),
                    Colors.Blend(Color.green, Color.black)
                }
            },
            new Theme()
            {
                Name = "RGB",
                Speed = 1.5f,
                Colors = new Color[]
                {
                    Color.red,
                    Juul.Colors.Blend(Color.red, Color.yellow),
                    Color.yellow,
                    Juul.Colors.Blend(Color.yellow, Color.green),
                    Color.green,
                    Juul.Colors.Blend(Color.green, Color.blue),
                    Color.blue,
                    Juul.Colors.Blend(Color.blue, Color.magenta),
                    Color.magenta,
                    Juul.Colors.Blend(Color.magenta, Color.red),
                }
            },
            new Theme()
            {
                Name = "Ocean Breeze",
                Speed = 0.5f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.cyan, Color.white, Color.white),
                    Colors.Blend(Color.cyan, Color.blue),
                    Colors.Blend(Color.blue, Color.blue, Color.cyan)
                }
            },
            new Theme()
            {
                Name = "Lava",
                Speed = 0.8f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.red, Color.yellow, Color.yellow),
                    Colors.Blend(Color.red, Color.red, Color.yellow),
                    Colors.Blend(Color.red, Color.black),
                    Colors.Blend(Color.red, Color.red, Color.black)
                }
            },
            new Theme()
            {
                Name = "Purple Haze",
                Speed = 0.55f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.magenta, Color.blue, Color.white),
                    Colors.Blend(Color.magenta, Color.magenta, Color.blue),
                    Colors.Blend(Color.blue, Color.magenta)
                }
            },
            new Theme()
            {
                Name = "Cotton Candy",
                Speed = 0.6f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.cyan, Color.white, Color.white),
                    Colors.Blend(Color.magenta, Color.white, Color.white),
                    Colors.Blend(Color.cyan, Color.magenta, Color.white)
                }
            },
            new Theme()
            {
                Name = "Toxic",
                Speed = 0.9f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.green, Color.yellow, Color.yellow),
                    Colors.Blend(Color.green, Color.green, Color.yellow),
                    Colors.Blend(Color.green, Color.black)
                }
            },
            new Theme()
            {
                Name = "Galaxy",
                Speed = 0.4f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.blue, Color.black, Color.black),
                    Colors.Blend(Color.magenta, Color.blue, Color.black),
                    Colors.Blend(Color.cyan, Color.blue),
                    Colors.Blend(Color.magenta, Color.black)
                }
            },
            new Theme()
            {
                Name = "Mint",
                Speed = 0.5f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.cyan, Color.green, Color.white),
                    Colors.Blend(Color.cyan, Color.white, Color.white),
                    Colors.Blend(Color.green, Color.cyan)
                }
            },
            new Theme()
            {
                Name = "Crimson",
                Speed = 0.7f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.red, Color.red, Color.black),
                    Colors.Blend(Color.red, Color.magenta),
                    Color.red
                }
            },
            new Theme()
            {
                Name = "Electric",
                Speed = 1.2f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.cyan, Color.white),
                    Colors.Blend(Color.blue, Color.cyan, Color.white),
                    Colors.Blend(Color.blue, Color.magenta),
                    Colors.Blend(Color.cyan, Color.blue)
                }
            },
            new Theme()
            {
                Name = "Golden Hour",
                Speed = 0.45f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.yellow, Color.white, Color.white),
                    Colors.Blend(Color.yellow, Color.red),
                    Colors.Blend(Color.red, Color.magenta, Color.yellow)
                }
            },
            new Theme()
            {
                Name = "Nebula",
                Speed = 0.6f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.magenta, Color.blue, Color.blue),
                    Colors.Blend(Color.blue, Color.cyan),
                    Colors.Blend(Color.magenta, Color.red),
                    Colors.Blend(Color.blue, Color.black)
                }
            },
            new Theme()
            {
                Name = "Synthwave",
                Speed = 1.0f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.magenta, Color.magenta, Color.blue),
                    Colors.Blend(Color.cyan, Color.blue),
                    Colors.Blend(Color.magenta, Color.red, Color.red),
                    Colors.Blend(Color.cyan, Color.magenta)
                }
            },
            new Theme()
            {
                Name = "Blood Moon",
                Speed = 0.5f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.red, Color.black, Color.black),
                    Colors.Blend(Color.red, Color.red, Color.black),
                    Colors.Blend(Color.red, Color.magenta, Color.black)
                }
            },
            new Theme()
            {
                Name = "Arctic",
                Speed = 0.4f,
                Colors = new Color[]
                {
                    Color.white,
                    Colors.Blend(Color.cyan, Color.white, Color.white),
                    Colors.Blend(Color.blue, Color.cyan, Color.white)
                }
            },
            new Theme()
            {
                Name = "Emerald",
                Speed = 0.6f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.green, Color.cyan),
                    Colors.Blend(Color.green, Color.green, Color.cyan),
                    Colors.Blend(Color.green, Color.black)
                }
            },
            new Theme()
            {
                Name = "Hyper",
                Speed = 1.8f,
                Colors = new Color[]
                {
                    Color.red,
                    Colors.Blend(Color.magenta, Color.red),
                    Color.magenta,
                    Colors.Blend(Color.blue, Color.magenta),
                    Color.blue,
                    Colors.Blend(Color.cyan, Color.blue),
                    Color.cyan,
                    Colors.Blend(Color.green, Color.cyan),
                    Color.green,
                    Colors.Blend(Color.yellow, Color.green),
                    Color.yellow,
                    Colors.Blend(Color.red, Color.yellow)
                }
            },
            new Theme()
            {
                Name = "Volcano",
                Speed = 0.75f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.red, Color.yellow),
                    Colors.Blend(Color.red, Color.red, Color.black),
                    Colors.Blend(Color.black, Color.red),
                    Color.black
                }
            },
            new Theme()
            {
                Name = "Peacock",
                Speed = 0.65f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.cyan, Color.blue, Color.white),
                    Colors.Blend(Color.green, Color.cyan),
                    Colors.Blend(Color.blue, Color.magenta),
                    Colors.Blend(Color.cyan, Color.green)
                }
            },
            new Theme()
            {
                Name = "Phantom",
                Speed = 0.5f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.gray, Color.white),
                    Colors.Blend(Color.gray, Color.gray, Color.black),
                    Colors.Blend(Color.black, Color.gray),
                    Colors.Blend(Color.gray, Color.white, Color.white)
                }
            },
            new Theme()
            {
                Name = "Cherry Blossom",
                Speed = 0.5f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.magenta, Color.white, Color.white, Color.white),
                    Colors.Blend(Color.red, Color.magenta, Color.white, Color.white),
                    Colors.Blend(Color.magenta, Color.white, Color.white)
                }
            },
            new Theme()
            {
                Name = "Abyssal",
                Speed = 0.4f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.blue, Color.black, Color.black),
                    Colors.Blend(Color.cyan, Color.blue, Color.black),
                    Color.black,
                    Colors.Blend(Color.blue, Color.black)
                }
            },
            new Theme()
            {
                Name = "Wildfire",
                Speed = 1.1f,
                Colors = new Color[]
                {
                    Color.yellow,
                    Colors.Blend(Color.yellow, Color.red),
                    Color.red,
                    Colors.Blend(Color.red, Color.black),
                    Colors.Blend(Color.red, Color.yellow, Color.yellow)
                }
            },
            new Theme()
            {
                Name = "Dusk",
                Speed = 0.45f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.red, Color.magenta, Color.black),
                    Colors.Blend(Color.magenta, Color.blue, Color.black),
                    Colors.Blend(Color.blue, Color.black, Color.black),
                    Colors.Blend(Color.red, Color.yellow, Color.black)
                }
            },
            new Theme()
            {
                Name = "Bioluminescent",
                Speed = 0.55f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.cyan, Color.green, Color.white),
                    Colors.Blend(Color.cyan, Color.blue, Color.black),
                    Colors.Blend(Color.green, Color.cyan, Color.black),
                    Colors.Blend(Color.blue, Color.black)
                }
            },
            new Theme()
            {
                Name = "Candy",
                Speed = 0.7f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.red, Color.white, Color.white),
                    Colors.Blend(Color.magenta, Color.white, Color.white),
                    Colors.Blend(Color.cyan, Color.white, Color.white),
                    Colors.Blend(Color.yellow, Color.white, Color.white)
                }
            },
            new Theme()
            {
                Name = "Solar",
                Speed = 0.8f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.white, Color.yellow, Color.white),
                    Colors.Blend(Color.yellow, Color.yellow, Color.red),
                    Colors.Blend(Color.red, Color.yellow),
                    Colors.Blend(Color.red, Color.red, Color.black)
                }
            },
            new Theme()
            {
                Name = "Holographic",
                Speed = 1.3f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.cyan, Color.white),
                    Colors.Blend(Color.magenta, Color.white),
                    Colors.Blend(Color.cyan, Color.magenta, Color.white),
                    Colors.Blend(Color.blue, Color.cyan, Color.white),
                    Colors.Blend(Color.magenta, Color.red, Color.white)
                }
            },
            new Theme()
            {
                Name = "Swamp",
                Speed = 0.5f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.green, Color.black, Color.black),
                    Colors.Blend(Color.green, Color.yellow, Color.black),
                    Colors.Blend(Color.black, Color.green, Color.black),
                    Colors.Blend(Color.green, Color.cyan, Color.black)
                }
            },
            new Theme()
            {
                Name = "Prism",
                Speed = 1.0f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.red, Color.white),
                    Colors.Blend(Color.yellow, Color.white),
                    Colors.Blend(Color.green, Color.white),
                    Colors.Blend(Color.cyan, Color.white),
                    Colors.Blend(Color.blue, Color.white),
                    Colors.Blend(Color.magenta, Color.white)
                }
            },
            new Theme()
            {
                Name = "Obsidian",
                Speed = 0.35f,
                Colors = new Color[]
                {
                    Color.black,
                    Colors.Blend(Color.black, Color.gray),
                    Colors.Blend(Color.gray, Color.blue, Color.black),
                    Colors.Blend(Color.black, Color.black, Color.gray)
                }
            },
            new Theme()
            {
                Name = "Flamingo",
                Speed = 0.55f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.red, Color.magenta, Color.white, Color.white),
                    Colors.Blend(Color.magenta, Color.white, Color.white),
                    Colors.Blend(Color.red, Color.white, Color.white),
                    Colors.Blend(Color.magenta, Color.red, Color.white)
                }
            },
            new Theme()
            {
                Name = "Glacier",
                Speed = 0.3f,
                Colors = new Color[]
                {
                    Color.white,
                    Colors.Blend(Color.cyan, Color.white, Color.white, Color.white),
                    Colors.Blend(Color.blue, Color.white, Color.white),
                    Colors.Blend(Color.cyan, Color.blue, Color.white)
                }
            },
            new Theme()
            {
                Name = "Magma",
                Speed = 0.9f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.red, Color.yellow),
                    Colors.Blend(Color.red, Color.black),
                    Colors.Blend(Color.red, Color.red, Color.yellow),
                    Color.black,
                    Colors.Blend(Color.red, Color.yellow, Color.black)
                }
            },
            new Theme()
            {
                Name = "Ultraviolet",
                Speed = 0.85f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.blue, Color.magenta, Color.magenta),
                    Colors.Blend(Color.magenta, Color.blue, Color.blue),
                    Colors.Blend(Color.blue, Color.black),
                    Colors.Blend(Color.magenta, Color.black)
                }
            },
            new Theme()
            {
                Name = "Spring",
                Speed = 0.5f,
                Colors = new Color[]
                {
                    Colors.Blend(Color.green, Color.yellow, Color.white),
                    Colors.Blend(Color.cyan, Color.green, Color.white),
                    Colors.Blend(Color.magenta, Color.white, Color.white),
                    Colors.Blend(Color.yellow, Color.green, Color.white)
                }
            },
        };
    }
}
