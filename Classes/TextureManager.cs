using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Juul
{
    internal class Textures : MonoBehaviour
    {
        public static string MenuTextures
        {
            get
            {
                return Path.Combine(Directory.GetParent(Application.dataPath).FullName, Core.Folder, "Textures");
            }
        }

        public static Dictionary<string, Texture2D> textureFilePool = new Dictionary<string, Texture2D>();

        public static Texture2D LoadTextureFromFile(string filePath)
        {
            if (textureFilePool.TryGetValue(filePath, out var cachedTexture))
            {
                if (cachedTexture != null)
                {
                    return cachedTexture;
                }
                else
                {
                    textureFilePool.Remove(filePath);
                }
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);

                if (texture.LoadImage(fileData))
                {
                    textureFilePool.Add(filePath, texture);
                    return texture;
                }
                else
                {
                    UnityEngine.Object.Destroy(texture);
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static Texture2D LoadTextureFromURL(string resourcePath, string fileName)
        {
            string filePath = Path.Combine(MenuTextures, fileName);

            if (textureFilePool.TryGetValue(filePath, out var cachedTexture))
            {
                if (cachedTexture != null)
                {
                    return cachedTexture;
                }
                else
                {
                    textureFilePool.Remove(filePath);
                }
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(filePath))
            {
                WebClient stream = new WebClient();
                stream.DownloadFile(resourcePath, filePath);
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);

                if (texture.LoadImage(bytes))
                {
                    textureFilePool.Add(filePath, texture);
                    return texture;
                }
                else
                {
                    Object.Destroy(texture);
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static Dictionary<string, Material> materialFilePool = new Dictionary<string, Material>();
        public static Material LoadMaterialFromFile(string filePath)
        {
            if (materialFilePool.TryGetValue(filePath, out var cachedMaterial))
            {
                if (cachedMaterial != null)
                {
                    return cachedMaterial;
                }
                else
                {
                    materialFilePool.Remove(filePath);
                }
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                byte[] imageData = File.ReadAllBytes(filePath);
                var material = new Material(Shader.Find("GorillaTag/UberShader"))
                {
                    shaderKeywords = new[] { "_USE_TEXTURE" }
                };
                var texture = new Texture2D(4096, 4096);
                ImageConversion.LoadImage(texture, imageData);
                texture.Apply();
                material.mainTexture = texture;

                materialFilePool.Add(filePath, material);
                return material;
            }
            catch
            {
                return null;
            }
        }

        

        public static Material LoadMaterialFromURL(string resourcePath, string fileName)
        {
            string filePath = Path.Combine(MenuTextures, fileName);

            if (materialFilePool.TryGetValue(filePath, out var cachedMaterial))
            {
                if (cachedMaterial != null)
                {
                    return cachedMaterial;
                }
                else
                {
                    materialFilePool.Remove(filePath);
                }
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(filePath))
            {
                using (WebClient stream = new WebClient())
                {
                    stream.DownloadFile(resourcePath, filePath);
                }
            }

            try
            {
                byte[] imageData = File.ReadAllBytes(filePath);
                var material = new Material(Shader.Find("GorillaTag/UberShader"))
                {
                    shaderKeywords = new[] { "_USE_TEXTURE" }
                };
                var texture = new Texture2D(4096, 4096);
                ImageConversion.LoadImage(texture, imageData);
                texture.Apply();
                material.mainTexture = texture;

                materialFilePool.Add(filePath, material);
                return material;
            }
            catch
            {
                return null;
            }
        }
    }
}