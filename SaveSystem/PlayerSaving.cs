using MTM101BaldAPI.SaveSystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityCipher;

namespace MyAPI._SaveSystem
{
    public static class PlayerSaving<T> where T : GamePlugin
    {
        public static ModData<T> data;

        public static void Load()
        {
            if (!Directory.Exists(ModData<T>.Path))
            {
                Directory.CreateDirectory(ModData<T>.Path);
            }
            if (!File.Exists(ModData<T>.FullPath))
            {
                data = new ModData<T>();
                Save();
            }
            else
            {
                try
                {
                    data = JsonConvert.DeserializeObject<ModData<T>>(RijndaelEncryption.Decrypt(File.ReadAllText(ModData<T>.FullPath), ModData<T>.FileName));
                }
                catch (Exception ex)
                {
                    HelperAPI.Instance<T>().Log($"Could not load file, creating new one! {ex.Message}", BepInEx.Logging.LogLevel.Warning);
                    data = new ModData<T>();
                    Save();
                }
            }
        }

        public static void Save()
        {
            if (data != null)
            {
                File.WriteAllText(ModData<T>.FullPath, RijndaelEncryption.Encrypt(JsonConvert.SerializeObject(data), ModData<T>.FileName));
            }
        }
    }

    [JsonObject]
    public class ModData<H> where H : GamePlugin
    {
        public static string File => "mod.dat";
        public static string FileName => Singleton<PlayerFileManager>.Instance.fileName;
        public static string Path => ModdedSaveSystem.GetSaveFolder(HelperAPI.Instance<H>(), FileName) + "/";
        public static string FullPath => System.IO.Path.Combine(Path, File);

        public Dictionary<string, int> data = new Dictionary<string, int>();

        public void Set(string id, int item)
        {
            data[id] = item;
            PlayerSaving<H>.Save();
        }

        public void Remove(string id)
        {
            if (data.Remove(id))
                PlayerSaving<H>.Save();
        }

        public int Get(string id, int defaultValue = 0)
        {
            if (!data.TryGetValue(id, out var value))
            {
                Set(id, defaultValue);
                return defaultValue;
            }

            return value;
        }

        public bool TryGet(string id, out int value, int defaultValue = 0)
        {
            if (data.TryGetValue(id, out var val))
            {
                value = val;
                return true;
            }

            value = defaultValue;
            return false;
        }
    }
}