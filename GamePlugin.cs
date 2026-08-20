using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using MyAPI.Data;
using MyAPI.NPCs;
using PlusStudioLevelLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MyAPI
{
    [BepInDependency("mtm101.rulerp.baldiplus.levelstudio", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("mtm101.rulerp.baldiplus.levelstudioloader", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    [BepInProcess("BALDI.exe")]
    public abstract class GamePlugin : BaseUnityPlugin
    {
        #region Audio
        public LoopManager LoopMan { get; protected set; }
        protected GamePluginEditor editor;

        protected AudioManager audMan;
        protected AudioSource loopAudio;

        public AudioSource LoopAudio
        {
            get
            {
                if (loopAudio == null)
                {
                    HelperAPI.SetAudioMan(ref loopAudio, true, true);
                }
                return loopAudio;
            }
        }

        public AudioManager AudMan
        {
            get
            {
                if (audMan == null)
                {
                    HelperAPI.SetAudioMan(ref audMan, true, false);
                }
                return audMan;
            }
        }

        public SoundObject currentAudioToLoop;

        public void ResetAudio()
        {
            currentAudioToLoop = null;
            AudMan.audioDevice.Stop();
            LoopAudio.Stop();
        }
        #endregion

        #region Plugin Loading
        public abstract ModInfo GetPluginInfo();

        protected virtual void Awake()
        {
            assetMan = new AssetManager();
            storage = new PluginStorage();
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio"))
            {
                editor = new GamePluginEditor();
                editor.plugin = this;
            }
            Harmony harmony = new Harmony(GetPluginInfo().guid);
            harmony.PatchAllConditionals();

            LoadingEvents.RegisterOnAssetsLoaded(Info, LoadImportant, LoadingEventOrder.Pre);
            LoadingEvents.RegisterOnAssetsLoaded(Info, PreLoad(), LoadingEventOrder.Pre);
            GeneratorManagement.Register(this, GenerationModType.Addend, TryAddStuff);

            ModdedSaveGame.AddSaveHandler(Info);

            GameObject loopManobj = new GameObject();
            loopManobj.name = $"PluginLoopManager_{GetPluginInfo().name}_{GetPluginInfo().version}";
            DontDestroyOnLoad(loopManobj);
            LoopMan = loopManobj.AddComponent<LoopManager>();
            LoopMan.plugin = this;
        }

        protected void LoadOptions<T>(string localizationKey) where T : ModOptions
        {
            CustomOptionsCore.OnMenuInitialize += delegate (OptionsMenu menu, CustomOptionsHandler handler)
            {
                handler.AddCategory<T>(localizationKey);
            };
        }

        protected IEnumerator PreLoad()
        {
            AssetLoader.LoadLocalizationFolder(Path.Combine(AssetLoader.GetModPath(this), "Language"), Language.English);
            HashSet<PluginAddition> additions = GetPluginInfo().additions;
            if (additions == null || additions.Count <= 0)
            {
                Log($"Nothing was loaded for a plugin with (GUID: {GetPluginInfo().guid}; Name: {GetPluginInfo().name}; Version: {GetPluginInfo().version}; Additions: {GetPluginInfo().additions})!", LogLevel.Warning);
                yield break;
            }

            yield return additions.Count;
            yield return "Adding Posters..";
            HandleAddition(PluginAddition.Posters);
            yield return "Adding Objects..";
            HandleAddition(PluginAddition.Objects);
            yield return "Adding Items..";
            HandleAddition(PluginAddition.Items);
            yield return "Adding Rooms.."; 
            HandleAddition(PluginAddition.Rooms);
            yield return "Adding NPCs..";
            HandleAddition(PluginAddition.NPCs);
            yield return "Doing custom instructions..";
            HandleAddition(PluginAddition.Custom);
            yield return "Adding Editor Support..";
            HandleAddition(PluginAddition.EditorSupport);
        }

        protected void HandleAddition(PluginAddition addition)
        {
            if (GetPluginInfo().additions.Contains(addition))
            {
                try
                {
                    switch (addition)
                    {
                        case PluginAddition.None:
                            break;
                        case PluginAddition.NPCs:
                            AddNPCs();
                            break;
                        case PluginAddition.Items:
                            AddItems();
                            break;
                        case PluginAddition.Rooms:
                            AddRooms(out string roomPath);
                            break;
                        case PluginAddition.Posters:
                            AddPosters();
                            break;
                        case PluginAddition.Objects:
                            AddObjects();
                            break;
                        case PluginAddition.EditorSupport:
                            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio"))
                            {
                                InternalEditorSupport();
                                AddEditorSupport();
                            }
                            break;
                        case PluginAddition.Custom:
                            CustomPreLoadInstructions();
                            break;
                        default:
                            Log("Error handling an addition. (called from the default case of the addition switch in the HandleAddition IEnumerator)", LogLevel.Error);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error handling an addition of {addition}: {ex.Message}!", LogLevel.Fatal);
                }
            }
        }

        protected void InternalEditorSupport() => editor.AddEditorStuff();

        protected virtual void AddNPCs() { }

        protected virtual void AddItems() { }

        protected virtual void AddRooms(out string roomPath) { roomPath = Path.Combine(AssetLoader.GetModPath(this), "Rooms"); }

        protected virtual void AddPosters() { }

        protected virtual void AddObjects() { }

        /// <summary>
        /// Executes only if Level Studio is installed.
        /// Everything that was added by mod's additions (load item, load npc, load poster), is automatically added.
        /// You may add editor support for anything else here.
        /// </summary>
        protected virtual void AddEditorSupport() { }

        /// <summary>
        /// Your own PreLoad instructions.
        /// </summary>
        protected virtual void CustomPreLoadInstructions() { }

        /// <summary>
        /// <b>"Aren't you forgetting something important?"</b> -
        /// Load anything else here. Localization is loaded automatically (in Language folder of the mod).
        /// </summary>
        protected abstract void LoadImportant();

        /// <summary>
        /// Required to add stuff for the loader. Items and NPCs are loaded automatically. Crashes on fail due to stability. Please make sure there are no errors that could appear here.
        /// </summary>
        /// <param name="floorName">Floor's Name.</param>
        /// <param name="floorNumber">Floor's Number.</param>
        /// <param name="sceneObject">Floor's Scene.</param>
        protected virtual void AddStuff(string floorName, int floorNumber, SceneObject sceneObject) { }

        /// <summary>
        /// Tries to add things to the loader. I made crash on fail due to stability. Please make sure there are no errors that could appear while adding stuff to the loader.
        /// </summary>
        /// <param name="name">Floor's Name.</param>
        /// <param name="num">Floor's Number.</param>
        /// <param name="scnObj">Floor's Scene.</param>
        protected void TryAddStuff(string name, int num, SceneObject scnObj)
        {
            Try(() => 
            {
                if (storage != null)
                {
                    if (storage.items != null && storage.items.Count > 0)
                    {
                        foreach (var item in storage.items.Values)
                        {
                            if (item != null && item.item != null)
                            {
                                if (item.locations.Contains(PotentialLocations.Pitstop) && item.shopChance > 0)
                                {
                                    item.item.AddItemInTheShop(name, num, scnObj, item.shopChance, PotentialLocations.Floors, PotentialLocations.Endless);
                                }
                                if (item.Generate())
                                {
                                    item.item.GenerateItem(this, name, num, scnObj, item.chance, item.locations.ToArray());
                                }
                            }
                        }
                    }
                    if (storage.npcs != null && storage.npcs.Count > 0)
                    {
                        foreach (var npc in storage.npcs.Values)
                        {
                            if (npc != null && npc.Npc != null && npc.Generate())
                            {
                                npc.Npc.SpawnNPC(name, num, scnObj, npc.Weight, npc.Locations.ToArray());
                            }
                        }
                    }
                }
                AddStuff(name, num, scnObj);
            });
        }

        /// <summary>
        /// Tries to do a certain function. If fails, then the game will crash. It will also log the error in BepInEx's Log file and the Unity's Debug Log.
        /// </summary>
        /// <param name="action">The function that will be called.</param>
        protected void Try(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Log(ex.Message, LogLevel.Fatal);
            }
        }

        /// <summary>
        /// Logs a message into the BepInEx's Log file and the Unity's Debug Log.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public void Log(string message, LogLevel type)
        {
            Logger.Log(type, message);
            switch (type)
            {
                case LogLevel.Error:
                case LogLevel.Fatal:
                    Debug.LogError(message);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
            if (type == LogLevel.Fatal)
            {
                MTM101BaldiDevAPI.CauseCrash(Info, new Exception($"ERROR: {message};\n PLUGIN: {Info}!"));
            }
        }

        public virtual T LoadItem<T>(out ItemObject newItem, GameItemData data, int shopPrice, int genCost, string AssetManName, string spriteName, string localizedItemName, params string[] tags) where T : Item
        {
            if (storage?.items?.ContainsKey(AssetManName) == true)
            {
                Log($"Item '{AssetManName}' already exists in storage. Skipping duplicate.", LogLevel.Warning);
                newItem = storage.items[AssetManName].item;
                return newItem.item as T;
            }

            string localizedItemDesc = localizedItemName.Replace("ITM_", "Desc_");

            if (!localizedItemDesc.Contains("Desc"))
            {
                localizedItemDesc = "Desc_" + localizedItemName;
            }

            newItem = new ItemBuilder(Info)
.SetEnum(localizedItemName).SetNameAndDescription(localizedItemName, localizedItemDesc).SetPickupSound(data.customPickupSound)
.SetShopPrice(shopPrice).SetGeneratorCost(genCost).SetItemComponent<T>().SetSprites($"{spriteName}_small".GetSprite(this, secondFolder: "Items"), $"{spriteName}_big".GetSprite(this, secondFolder: "Items"))
.SetMeta(data.flags, tags).Build();

            data.item = newItem;
            assetMan.Add(AssetManName, newItem);
            assetMan.Add(AssetManName + "_DATA", data);
            storage?.items?.Add(AssetManName, data);
            storage?.Add(AssetManName, data);

            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudioloader"))
            {
                LevelLoaderPlugin.Instance.itemObjects.Add(AssetManName, newItem);
            }
            return newItem.item as T;
        }

        public virtual void LoadPoster(out PosterObject newPoster, GamePosterData data, Texture2D posterTexture, string AssetManName)
        {
            newPoster = ObjectCreators.CreatePosterObject(posterTexture, data.Convert());
            data.poster = newPoster;
            assetMan.Add(AssetManName, newPoster);
            assetMan.Add(AssetManName + "_DATA", data);
            storage?.posters?.Add(AssetManName, data);
            storage?.Add(AssetManName, data);

            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudioloader"))
            {
                LevelLoaderPlugin.Instance.posterAliases.Add(AssetManName, newPoster);
            }
        }

        public virtual void LoadObject<T>(string name, string AssetManName, float spriteY, Vector3 spriteSize, Sprite sprite) where T : MonoBehaviour
        {
            CreateObject(out var newObj, name, spriteY, spriteSize, sprite);

            newObj.AddComponent<T>();
            newObj.ConvertToPrefab(true);

            AddObjectToStorage(AssetManName, newObj);
        }

        public virtual void LoadObject(string name, string AssetManName, float spriteY, Vector3 spriteSize, Sprite sprite)
        {
            CreateObject(out var newObj, name, spriteY, spriteSize, sprite);

            newObj.ConvertToPrefab(true);

            AddObjectToStorage(AssetManName, newObj);
        }

        protected void AddObjectToStorage(string AssetManName, GameObject obj)
        {
            assetMan.Add(AssetManName, obj);
            storage?.prefabs?.Add(AssetManName, obj);
            storage?.Add(AssetManName, obj);
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudioloader"))
            {
                LevelLoaderPlugin.Instance.basicObjects.Add(AssetManName, obj);
            }
        }

        protected void CreateObject(out GameObject newObj, string name, float spriteY, Vector3 spriteSize, Sprite sprite)
        {
            newObj = null;
            GameObject[] sources = Resources.FindObjectsOfTypeAll<GameObject>();
            if (sources == null || sources.Length <= 0) return;

            GameObject plant = sources.First((GameObject x) => x.name == "Plant");
            if (plant == null) return;

            newObj = Instantiate(plant);
            newObj.layer = 0;
            newObj.name = name;
            var rend = newObj.GetComponentInChildren<SpriteRenderer>();
            rend.transform.position = new Vector3(rend.transform.position.x, spriteY, rend.transform.position.z);
            rend.transform.localScale = spriteSize;
            rend.sprite = sprite;
            var collider = newObj.AddComponent<SphereCollider>();
            collider.radius = 6f;
            collider.center = Vector3.zero;
            collider.isTrigger = true;
        }

        /// <summary>
        /// Required for loading NPCs. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="npc">The NPC that will be created.</param>
        /// <param name="data">The NPC's Data.</param>
        /// <param name="AssetManName">The name that will be saved in the asset manager and the plugin's storage.</param>
        /// <param name="PosterNameKey">The NPC's poster name. (localization key)</param>
        /// <param name="PosterDescKey">The NPC's poster description. (localization key)</param>
        /// <returns></returns>
        public virtual T LoadNPC<T>(GameNPCData data, string AssetManName, string PosterNameKey, string PosterDescKey) where T : CustomNPC
        {
            try
            {
                Log("Building the NPC..", LogLevel.Info);
                T npc;

                if (data.PotentialRooms != null && data.PotentialRooms.Count > 0)
                {
                    npc = new NPCBuilder<T>(Info)
        .SetName(data.NameKey).SetEnum(data.NameKey)
        .SetMinMaxAudioDistance(1, data.MaxAudDist).IgnorePlayerOnSpawn()
        .AddLooker().AddTrigger()
        .AddSpawnableRoomCategories(data.RoomCat)
        .AddPotentialRoomAssets(data.PotentialRooms.ToArray())
        .SetPoster(data.PosterTexture, PosterNameKey, PosterDescKey).Build();
                }
                else
                {
                    npc = new NPCBuilder<T>(Info)
        .SetName(data.NameKey).SetEnum(data.NameKey)
        .SetMinMaxAudioDistance(1, data.MaxAudDist).IgnorePlayerOnSpawn()
        .AddLooker().AddTrigger()
        .AddSpawnableRoomCategories(data.RoomCat)
        .SetPoster(data.PosterTexture, PosterNameKey, PosterDescKey).Build();
                }

                Log("Loading the NPC's sprite and music..", LogLevel.Info);
                npc.LoadNPC(this, data.NpcSprite, data.ThemeMusic, data.AdditionalMusic, data.MaxAudDist, data.Rolloff, data.Speed);
                data.Npc = npc;

                Log("Adding NPC to the asset managers..", LogLevel.Info);
                assetMan.Add(AssetManName, npc);
                assetMan.Add(AssetManName + "_DATA", data);
                storage?.npcs?.Add(AssetManName, data);
                storage?.Add(AssetManName, data);
                if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudioloader"))
                {
                    LevelLoaderPlugin.Instance.npcAliases.Add(AssetManName, npc);
                    LevelLoaderPlugin.Instance.posterAliases.Add(AssetManName + "_Poster", npc.Poster);
                }
                return npc;
            }
            catch (Exception ex)
            {
                Log($"Loading NPC Error: {ex.Message};\n{ex.TargetSite}", LogLevel.Error);
                return null;
            }
        }

        /// <summary>
        /// Required for loading rooms. Please note that any error caused by incorrect data will cause crash.
        /// </summary>
        /// <param name="data">The room's data. Must be created using it's constructor.</param>'
        /// <param name="AssetManName">The asset's name that will be saved as the key for the data in the mod's storage and asset manager.</param>
        /// <returns></returns>
        public virtual void LoadRoom(GameRoomData data, string AssetManName)
        {
            if (!Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudioloader"))
            {
                Log("No Level Studio Loader found!", LogLevel.Fatal);
                return;
            }

            try
            {
                assetMan.Add(AssetManName + "_DoorMats", data.DoorMats);

                LevelLoaderPlugin levelLoader = LevelLoaderPlugin.Instance;

                string roomPath = Path.Combine(AssetLoader.GetModPath(this), data.FilePath);

                string floorKey = $"{AssetManName}_Floor";
                string wallKey = $"{AssetManName}_Wall";
                string ceilKey = $"{AssetManName}_Ceil";
                Texture2D floorTex = floorKey.GetTexture(this, folder: "Rooms");
                Texture2D wallTex = wallKey.GetTexture(this, folder: "Rooms");
                Texture2D ceilTex = ceilKey.GetTexture(this, folder: "Rooms");

                RoomTex roomTextures = new RoomTex(wallKey, wallTex, floorKey, floorTex, ceilKey, ceilTex);

                levelLoader.roomTextureAliases.Add(floorKey, floorTex);
                levelLoader.roomTextureAliases.Add(wallKey, wallTex);
                levelLoader.roomTextureAliases.Add(ceilKey, ceilTex);

                RoomSettings roomSettings = new RoomSettings(data.RoomCat, RoomType.Room, data.MapColor, assetMan.Get<StandardDoorMats>(AssetManName + "_DoorMats"), null);

                if (!levelLoader.roomSettings.ContainsKey(AssetManName))
                    levelLoader.roomSettings.Add(AssetManName, roomSettings);
                else
                    levelLoader.roomSettings[AssetManName] = roomSettings;

                data.AddToList(roomTextures);
                assetMan.Add(AssetManName, data);
                storage?.rooms?.Add(AssetManName, data);
                storage?.Add(AssetManName, data);
            }
            catch (Exception ex)
            {
                Log($"Failed to load room: {ex.Message} | {ex.TargetSite}", LogLevel.Fatal);
            }
        }
        #endregion

        #region Needed Plugin Features
        public AssetManager assetMan;

        /// <summary>
        /// The storage contains the plugin's saved things.
        /// </summary>
        public PluginStorage storage;

        /// <summary>
        /// Mod's Information, like GUID, the Plugin's name, or it's version. Also contains information of it's additions.
        /// </summary>
        public struct ModInfo
        {
            public string guid;
            public string name;
            public string version;
            public HashSet<PluginAddition> additions;

            public ModInfo(string guid, string name, string version, params PluginAddition[] additions)
            {
                this.guid = guid;
                this.name = name;
                this.version = version;
                this.additions = additions != null ? [.. additions.ToList()] : [PluginAddition.None];
            }
        }

        /// <summary>
        /// A plugin addition. Things which will get loaded for this mod.
        /// </summary>
        public enum PluginAddition : byte
        {
            /// <summary>
            /// No additions. Not recommended to use.
            /// </summary>
            None,
            /// <summary>
            /// Addition for adding NPCs. This addition will execute the AddNPCs method.
            /// </summary>
            NPCs,
            /// <summary>
            /// Addition for adding items. This addition will execute the AddItems method.
            /// </summary>
            Items,
            /// <summary>
            /// Addition for adding rooms. This addition will execute the AddRooms method.
            /// </summary>
            Rooms,
            /// <summary>
            /// Addition for adding posters. This addition will execute the AddPosters method.
            /// </summary>
            Posters,
            /// <summary>
            /// Addition for adding objects. This addition will execute the AddObjects method.
            /// </summary>
            Objects,
            /// <summary>
            /// Editor Support. Will be executed only if the Level Studio is installed.
            /// </summary>
            EditorSupport,
            /// <summary>
            /// The custom instructions not related to NPCs, Items, Rooms or Editor Support.
            /// </summary>
            Custom
        }
        #endregion
    }

    /// <summary>
    /// The plugin's storage containing some data.
    /// </summary>
    public class PluginStorage
    {
        public Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        public Dictionary<string, GamePosterData> posters = new Dictionary<string, GamePosterData>();
        public Dictionary<string, GameItemData> items = new Dictionary<string, GameItemData>();
        public Dictionary<string, GameNPCData> npcs = new Dictionary<string, GameNPCData>();
        public Dictionary<string, GameRoomData> rooms = new Dictionary<string, GameRoomData>();

        public Dictionary<string, object> data = new Dictionary<string, object>();

        public void Add<T>(string id, T item) where T : class
        {
            if (data.ContainsKey(id))
            {
                throw new Exception($"Already contains key: {id}");
            }

            data.Add(id, item);
        }

        public void Remove(string id)
        {
            if (!data.ContainsKey(id))
            {
                throw new KeyNotFoundException($"PluginStorage: Could not find {id} to remove!");
            }

            data.Remove(id);
        }

        public T Get<T>(string id) where T : class
        {
            if (!data.TryGetValue(id, out var value))
            {
                throw new KeyNotFoundException($"PluginStorage: Could not find {id} to return it!");
            }

            return value as T;
        }

        public bool TryGet<T>(string id, out T value) where T : class
        {
            if (data.TryGetValue(id, out var val) && val is T typedVal)
            {
                value = typedVal;
                return true;
            }

            value = null;
            return false;
        }
    }
}