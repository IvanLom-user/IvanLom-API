using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MyAPI
{
    /// <summary>
    /// The API with the loading-related methods. (GetSound, GetSprite, GetTexture, LoadSpriteSheet, etc.)
    /// </summary>
    public static class BaseAPI
    {
        public static SoundObject GetSound(this string soundName, GamePlugin plugin, string subtitle, Color? color = null, string secondFolder = "", string format = ".ogg", SoundType sfxType = SoundType.Effect, string folder = "Sounds")
        {
            SoundObject sound;

            if (color == null)
            {
                color = Color.white;
            }

            try
            {
                string fileName = soundName + format;
                if (!fileName.Contains(format))
                {
                    fileName += format;
                }
                if (!plugin.assetMan.ContainsKey(soundName))
                {
                    plugin.assetMan.Add(soundName, ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(plugin, folder, secondFolder, fileName), subtitle, sfxType, color.Value, subtitle.Length > 0 ? -1f : 0f));
                }

                sound = plugin.assetMan.Get<SoundObject>(soundName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error appeared when loading sound {soundName}: {ex.Message}");
                return null;
            }
            return sound;
        }

        public static Sprite GetSprite(this string spriteName, GamePlugin plugin, string folder = "Sprites", string format = ".png", string secondFolder = "", float pixelsPerUnit = 50f)
        {
            Sprite spr;
            try
            {
                if (spriteName.StartsWith("spr_"))
                {
                    spriteName = spriteName.Replace("spr_", "");
                }
                string fileName = spriteName + format;
                if (!fileName.Contains(format))
                {
                    fileName += format;
                }

                if (!plugin.assetMan.ContainsKey(spriteName))
                {
                    Texture2D texture;
                    if (secondFolder != "")
                    {
                        texture = AssetLoader.TextureFromMod(plugin, folder, secondFolder, fileName);
                    }
                    else
                    {
                        texture = AssetLoader.TextureFromMod(plugin, folder, fileName);
                    }

                    Sprite sprite = AssetLoader.SpriteFromTexture2D(texture, pixelsPerUnit);
                    plugin.assetMan.Add(spriteName, sprite);
                }

                spr = plugin.assetMan.Get<Sprite>(spriteName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error appeared when loading sprite {spriteName}: {ex.Message}");
                return null;
            }
            return spr;
        }

        public static Texture2D GetTexture(this string spriteName, GamePlugin plugin, string folder = "Sprites", string format = ".png")
        {
            Texture2D t;
            try
            {
                if (spriteName.StartsWith("spr_"))
                {
                    spriteName = spriteName.Replace("spr_", "");
                }
                string fn = spriteName + format;
                if (!fn.Contains(format))
                {
                    fn += format;
                }
                if (!plugin.assetMan.ContainsKey(spriteName))
                {
                    plugin.assetMan.Add(spriteName, AssetLoader.TextureFromMod(plugin, folder, fn));
                }
                t = plugin.assetMan.Get<Texture2D>(spriteName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error appeared when loading texture {spriteName}: {ex.Message}");
                return null;
            }
            return t;
        }

        public static SoundObject GetRandomSound(this string subtitle, GamePlugin plugin, Color color, string format = ".ogg", params string[] soundName)
        {
            int sr = UnityEngine.Random.Range(0, soundName.Length);
            return GetSound(soundName[sr], plugin, subtitle, color, format);
        }

        public static Material GetDefaultSpriteMaterial()
        {
            Shader spriteShader = Shader.Find("Sprites/Default");

            if (spriteShader == null)
            {
                Debug.LogError("The shader wasn't found!");
                return null;
            }

            Material defaultSpriteMaterial = new Material(spriteShader);
            return defaultSpriteMaterial;
        }

        public static List<Sprite> LoadSpriteSheet(int[] layout, GamePlugin plugin, string spriteSheetTextureName = "PolishCow", int dimensionX = 220, int dimensionZ = 184, float spriteSize = 100f)
        {
            var sprites = new List<Sprite>();

            Texture2D texture = spriteSheetTextureName.GetTexture(plugin);
            if (texture == null)
            {
                Debug.LogError("Failed to load the cow texture");
                return sprites;
            }

            Debug.Log($"Loaded texture: {texture.name}, size: {texture.width}x{texture.height}");

            for (int row = 0; row < layout.Length; row++)
            {
                float y = texture.height - (row + 1) * dimensionZ;

                if (y < 0 || y + dimensionZ > texture.height)
                {
                    Debug.LogWarning($"Row {row}: y={y} is outside texture bounds, height:{texture.height}!");
                    continue;
                }

                for (int col = 0; col < layout[row]; col++)
                {
                    float x = col * dimensionX;

                    if (x + dimensionX > texture.width)
                    {
                        Debug.LogWarning($"Row {row}, Col {col}: x={x} is outside texture bounds (width={texture.width})");
                        continue;
                    }

                    Rect rect = new Rect(x, y, dimensionX, dimensionZ);
                    Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), spriteSize);
                    sprites.Add(sprite);
                    Debug.Log($"Created sprite {sprites.Count - 1} at row {row}, col {col}");
                }
            }

            Debug.Log($"Total sprites created: {sprites.Count}");
            return sprites;
        }
    }
}