using HarmonyLib;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;
using MyAPI.Additions;
using MyAPI.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MyAPI
{
    /// <summary>
    /// The API with helper methods.
    /// </summary>
    public static class HelperAPI
    {
        public static Color GetColorStr(string colorName)
        {
            if (string.IsNullOrEmpty(colorName)) return Color.white;

            return colorName.ToLower() switch
            {
                "white" => Color.white,
                "black" => Color.black,
                "red" => Color.red,
                "blue" => Color.blue,
                "green" => Color.green,
                "yellow" => Color.yellow,
                "cyan" => Color.cyan,
                "magenta" => Color.magenta,
                "gray" => Color.gray,
                "grey" => Color.gray,
                "clear" => Color.clear,
                _ => Color.white
            };
        }

        public static BaldiFonts GetFontInt(int fontSize) => fontSize switch
        {
            <= 0 => BaldiFonts.ComicSans18,
            < 18 => BaldiFonts.ComicSans12,
            < 24 => BaldiFonts.ComicSans18,
            < 36 => BaldiFonts.ComicSans24,
            _ => BaldiFonts.ComicSans36
        };

        #region Item Randomizer
        private static readonly List<WeightedItemObject> validItems = [];

        public static void PrecomputeValidItems()
        {
            var suitableItems = ItemMetaStorage.Instance.GetAllWithoutFlags(ItemFlags.Unobtainable | ItemFlags.NoUses | ItemFlags.InstantUse);

            validItems.Clear();
            foreach (var meta in suitableItems)
            {
                if (meta.itemObjects != null && !meta.tags.Contains("mrbeast_cant_give") && meta.flags != ItemFlags.InstantUse && meta.flags != ItemFlags.Unobtainable && meta.flags != ItemFlags.NoUses)
                {
                    foreach (var itemObj in meta.itemObjects)
                    {
                        if (itemObj == null) continue;
                        if (itemObj.itemType == Items.None || itemObj.itemType == Items.PentagonKey || itemObj.itemType == Items.SquareKey || itemObj.itemType == Items.TriangleKey || itemObj.itemType == Items.WeirdKey || itemObj.itemType == Items.HexagonKey || itemObj.itemType == Items.CircleKey) continue;

                        validItems.Add(new WeightedItemObject
                        {
                            selection = itemObj,
                            weight = 100
                        });
                    }
                }
            }
        }

        public static ItemObject GetRandomItem(ItemManager itm)
        {
            try
            {
                if (itm == null)
                {
                    throw new NullReferenceException("ItemManager is null!");
                }

                if (validItems == null)
                {
                    throw new NullReferenceException("Valid items array is null!");
                }

                if (validItems.Count <= 0)
                {
                    throw new ArgumentOutOfRangeException("No valid items found!");
                }

                return WeightedItemObject.RandomSelection(validItems.ToArray());
            }
            catch (Exception e)
            {
                Debug.LogError($"Error getting random item: {e.Message}");
                return null;
            }
        }
        #endregion

        public static void RaldifyBillboard(SpriteRenderer billboard)
        {
            billboard.gameObject.AddComponent<ShakingBillboard>();
            billboard.material.DisableKeyword("BOOLEAN_BILLBOARD_ON");
        }

        public static T LoadAsset<T>() where T : Object => Resources.FindObjectsOfTypeAll<T>().First();

        public static T[] LoadAssets<T>() where T : Object => Resources.FindObjectsOfTypeAll<T>();

        public static Type GetTypeByName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            Type type = Type.GetType(typeName);
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in GetTypes(assembly))
                {
                    if (t != null && t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                        return t;
                }
            }

            return null;
        }

        public static IEnumerable<Type> GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
            catch
            {
                return Enumerable.Empty<Type>();
            }
        }

        public static int HaveItems<T, H>(ItemManager itm) where T : Item where H : T
        {
            if (itm == null) return 0;

            int totalAmount = 0;

            if (itm.items != null)
            {
                foreach (var item in itm.items)
                {
                    if (item.item is T && item.item is not H) totalAmount++;
                }
            }

            Item[] allItems = API_Plugin.Instance.usedItems.ToArray();
            for (int i = 0; i < allItems.Length; i++)
            {
                var item = allItems[i];
                if (item != null && item is T && item is not H)
                {
                    totalAmount++;
                }
            }

            return totalAmount;
        }

        public static int HaveItems<T>(ItemManager itm) where T : Item
        {
            if (itm == null) return 0;

            int totalAmount = 0;

            if (itm.items != null)
            {
                foreach (var item in itm.items)
                {
                    if (item.item is T) totalAmount++;
                }
            }

            Item[] allItems = API_Plugin.Instance.usedItems.ToArray();
            for (int i = 0; i < allItems.Length; i++)
            {
                var item = allItems[i];
                if (item != null && item is T)
                {
                    totalAmount++;
                }
            }

            return totalAmount;
        }

        public static void ApplyBillboard(SpriteRenderer rend)
        {
            GameObject[] sources = Resources.FindObjectsOfTypeAll<GameObject>();
            if (sources == null || sources.Length <= 0) return;

            GameObject plant = sources.First((GameObject x) => x.name == "Plant");
            if (plant == null) return;

            rend.material = plant.GetComponentInChildren<SpriteRenderer>().material;
        }

        public static void SetAudioMan(ref AudioManager aud, bool _2D, bool loop = false)
        {
            GameObject newAudioMan = new GameObject($"GamePlugin_{Random.Range(-999999f, 999999f)}_{Random.Range(-999999f, 999999f)}_{Random.Range(-99999f, 99999f)}");
            Object.DontDestroyOnLoad(newAudioMan);
            aud = newAudioMan.GetComponent<AudioManager>() ?? newAudioMan.AddComponent<AudioManager>();
            aud.audioDevice = newAudioMan.GetComponent<AudioSource>() ?? newAudioMan.AddComponent<AudioSource>();
            aud.SetLoop(loop);

            if (_2D)
            {
                aud.audioDevice.spatialBlend = 0f;
                aud.audioDevice.maxDistance = 999f;
                aud.positional = false;
            }
        }

        public static void SetAudioMan(ref AudioSource aud, bool _2D, float distance, bool loop = false)
        {
            GameObject newAudioMan = new GameObject($"GamePlugin_{Random.Range(-999999f, 999999f)}_{Random.Range(-999999f, 999999f)}_{Random.Range(-99999f, 99999f)}");
            Object.DontDestroyOnLoad(newAudioMan);
            aud = newAudioMan.GetComponent<AudioSource>() ?? newAudioMan.AddComponent<AudioSource>();
            aud.loop = loop;

            if (_2D)
            {
                aud.spatialBlend = 0f;
                aud.maxDistance = 999f;
            }
            else
            {
                aud.spatialBlend = 0f;
                PropagatedAudio_Real propagated = newAudioMan.AddComponent<PropagatedAudio_Real>();
                propagated.audioDevice = aud;
                propagated.maxDistance = distance;
            }
        }

        public static void SetText(ref TextMeshProUGUI text, string textName, Transform hudTransform)
        {
            text = new GameObject(textName).AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(hudTransform, false);
            text.rectTransform.sizeDelta = new Vector2(500f, 80f);
            text.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            text.rectTransform.localPosition = Vector3.zero;
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.localPosition = Vector3.zero;
            text.gameObject.SetActive(true);
        }

        public static void SetValue<T>(object instance, string fieldName, T setVal)
        {
            Traverse.Create(instance).Field(fieldName).SetValue(setVal);
        }

        public static void SetPosAndRotation(this Transform transform, Vector3 pos, Quaternion rotation, bool local)
        {
            if (local)
            {
                transform.localPosition = pos;
                transform.localRotation = rotation;
                return;
            }
            transform.SetPositionAndRotation(pos, rotation);
        }

        public static void SetPosAndRotation(this Transform transform, Vector3 pos, Quaternion rotation) => transform.SetPositionAndRotation(pos, rotation);

        public static void ToCenter(this RectTransform rect)
        {
            rect.anchoredPosition = Vector3.zero;
            rect.anchorMin = Vector2.one * 0.5f;
            rect.anchorMax = Vector2.one * 0.5f;
        }

        public static void ToCenter(this UnityEngine.UI.Image image) => image.rectTransform.ToCenter();

        public static void TeleportToOther(this Transform transform, Transform other)
        {
            transform.SetParent(other);
            transform.SetParent(null);
            transform.position = other.position;
        }

        public static bool CreateAudio(out AudioSource audio, bool _3d, float distance, bool loop, AudioClip clip, bool immediatePlay)
        {
            GameObject aud = new GameObject($"GameAudio_{Random.Range(-999999f, 999999f)}_{Random.Range(-99999f, 99999f)}_{Random.Range(-9999f, 9999f)}");
            audio = aud.AddComponent<AudioSource>();
            audio.rolloffMode = AudioRolloffMode.Custom;
            audio.spatialBlend = 0;
            audio.maxDistance = distance;
            audio.loop = loop;
            audio.clip = clip;

            if (_3d)
            {
                var propagated = aud.AddComponent<PropagatedAudio_Real>();
                propagated.audioDevice = audio;
                propagated.maxDistance = distance;
            }

            if (immediatePlay)
            {
                audio.Play();
            }

            return audio != null;
        }

        public static bool CreateSprite(out SpriteRenderer spriteRend, Sprite sprite, bool billboard)
        {
            GameObject spriteObj = new GameObject($"GameSprite_{Random.Range(-999999f, 999999f)}_{Random.Range(-99999f, 99999f)}_{Random.Range(-9999f, 9999f)}");
            spriteObj.layer = LayerMask.NameToLayer("Billboard");
            spriteRend = spriteObj.AddComponent<SpriteRenderer>();
            spriteRend.renderingLayerMask = 1;
            spriteRend.sprite = sprite;

            if (billboard)
            {
                ApplyBillboard(spriteRend);
            }

            return spriteRend != null;
        }

        public static bool AddCollider<T>(GameObject target, float radius = 3.5f, bool trigger = false) where T : Collider
        {
            var collider = target.AddComponent<T>();
            if (collider is SphereCollider sphere)
            {
                sphere.radius = 3.5f;
            }
            else if (collider is CapsuleCollider capsule)
            {
                capsule.radius = 3.5f;
            }
            collider.isTrigger = trigger;
            return collider != null;
        }

        public static bool AddLifetime(SpriteRenderer target, float lifetime, Pickup pickup)
        {
            var lifetimer = target.gameObject.AddComponent<ItemLifetime>();
            lifetimer.lifetime = lifetime;
            lifetimer.pickup = pickup;
            lifetimer.spriteRenderer = target;
            return lifetimer != null;
        }


        /// <summary>
        /// A replacement to T.Instance / GamePlugin.Instance.
        /// </summary>
        /// <typeparam name="T">The Plugin's type.</typeparam>
        /// <param name="sensitive">If sensitive, any fail will lead to a NullReferenceException.</param>
        /// <returns>Plugin with the specified type.</returns>
        /// <exception cref="NullReferenceException"></exception>
        public static T Instance<T>(bool sensitive = true) where T : GamePlugin
        {
            if (API_Plugin.Instance == null)
            {
                API_Plugin.Instance = Object.FindObjectOfType<API_Plugin>(true);
                if (API_Plugin.Instance == null)
                {
                    if (sensitive)
                    {
                        throw new NullReferenceException($"Error when trying to get an Instance of {typeof(T).Name} Plugin: API Plugin is null!");
                    }
                    return null;
                }
            }

            if (API_Plugin.Instance.Plugins.TryGetValue(typeof(T).Name, out var plug) && plug.TryGetComponent<T>(out var typed))
            {
                return typed;
            }
            if (sensitive)
            {
                throw new NullReferenceException($"Could not find {typeof(T).Name} Plugin!");
            }
            return null;
        }

        public static T Instance<T>() where T : GamePlugin => Instance<T>(true);

        public static GamePlugin Instance(string guid)
        {
            foreach (var plugin in API_Plugin.Instance.Plugins.Values)
            {
                if (plugin.GetPluginInfo().guid == guid)
                {
                    return plugin;
                }
            }
            return null;
        }
    }

    public class GameColor
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public static GameColor white => new GameColor(1, 1, 1, 1);
        public static GameColor black => new GameColor(0, 0, 0, 1);
        public static GameColor clear => new GameColor(1, 1, 1, 0);

        public GameColor(float r, float g, float b, float a = 1)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public void Change(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Color ToColor() => new Color(r, g, b, a);
        public Color Inverse() => new Color(1 - r, 1 - g, 1 - b);
        public static GameColor Inverse(GameColor other) => new GameColor(1 - other.r, 1 - other.g, 1 - other.b);
    }
}