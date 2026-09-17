using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using MTM101BaldAPI.UI;
using MyAPI.Core;
using MyAPI.Data;
using MyAPI.NPCs;
using PlusStudioLevelLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
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
        protected ConfigEntry<bool> activePlugin;
        protected GamePluginEditor editor;

        protected AudioSource loopAudio;

        public AudioSource LoopAudio
        {
            get
            {
                if (loopAudio == null)
                {
                    HelperAPI.SetAudioMan(ref loopAudio, true, 999f, true);
                }
                return loopAudio;
            }
        }

        public SoundObject currentAudioToLoop;

        public void ResetAudio()
        {
            currentAudioToLoop = null;
            LoopAudio.Stop();
        }
        #endregion

        #region Plugin Loading
        public abstract ModInfo GetPluginInfo();

        protected virtual void Awake()
        {
            activePlugin = Config.Bind(
    "Active",
    "Is Active",
    true,
    "Activates the mod."
);
            API_Plugin api = FindObjectOfType<API_Plugin>();
            if (api != null && activePlugin.Value)
            {
                api.AddPlugin(GetPluginInfo().type, this);
            }
            if (api != null && !api.Plugins.TryGetValue(GetPluginInfo().type, out _))
            {
                enabled = false;
                return;
            }

            assetMan = new AssetManager();
            storage = new PluginStorage();
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio"))
            {
                editor = new GamePluginEditor();
                editor.plugin = this;
            }

            LoadingEvents.RegisterOnAssetsLoaded(Info, LoadImportant, LoadingEventOrder.Pre);
            LoadingEvents.RegisterOnAssetsLoaded(Info, PreLoad(), LoadingEventOrder.Pre);
            GeneratorManagement.Register(this, GenerationModType.Addend, TryAddStuff);

            ModdedSaveGame.AddSaveHandler(Info);

            GameObject loopManobj = new GameObject();
            loopManobj.name = $"PluginLoopManager_{GetPluginInfo().name}";
            DontDestroyOnLoad(loopManobj);
            LoopMan = loopManobj.AddComponent<LoopManager>();
            LoopMan.plugin = this;
        }

        protected void LoadOptions<T, H>(string localizationKey) where T : ModOptions<H> where H : GamePlugin
        {
            if (!activePlugin.Value) return;

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
                Log($"Nothing was loaded for a plugin with (GUID: {GetPluginInfo().guid}; Name: {GetPluginInfo().name}; Additions: {GetPluginInfo().additions})!", LogLevel.Warning);
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

        protected virtual void AddNPCs() { LoadNPCsFromJSON(); }

        protected virtual void AddItems() { LoadItemsFromJSON(); }

        protected virtual void AddPosters() { }

        protected virtual void AddRooms(out string roomPath) { roomPath = Path.Combine(AssetLoader.GetModPath(this), "Rooms"); }

        protected virtual void AddObjects() { LoadObjectsFromJSON(); }

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
                    if (storage.posters != null && storage.posters.Count > 0)
                    {
                        foreach (var poster in storage.posters.Values)
                        {
                            if (poster != null && poster.poster != null && poster.weight > 0 && !poster.categoryOnly)
                            {
                                poster.poster.GeneratePoster(this, name, num, scnObj, poster.weight);
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

        public virtual ItemObject LoadItem<T>(GameItemData data, int shopPrice, int genCost, string AssetManName, string spriteName, string localizedItemName, params string[] tags) where T : Item
        {
            if (storage?.items?.ContainsKey(AssetManName) == true)
            {
                Log($"Item '{AssetManName}' already exists!", LogLevel.Warning);
                return storage.items[AssetManName].item;
            }

            var newItem = LoadItem_NoSave<T>(data, shopPrice, genCost, spriteName, localizedItemName, tags);

            data.item = newItem;
            assetMan.Add(AssetManName, newItem);
            assetMan.Add(AssetManName + "_DATA", data);
            storage?.items?.Add(AssetManName, data);
            storage?.Add(AssetManName, data);

            API_Plugin.Instance.items?.Add(AssetManName);

            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudioloader"))
            {
                LevelLoaderPlugin.Instance.itemObjects.Add(AssetManName, newItem);
            }

            return newItem;
        }

        public virtual ItemObject LoadItem_NoSave<T>(GameItemData data, int shopPrice, int genCost, string spriteName, string localizedItemName, params string[] tags) where T : Item
        {
            string localizedItemDesc = localizedItemName.Replace("ITM_", "Desc_");

            if (!localizedItemDesc.Contains("Desc"))
            {
                localizedItemDesc = "Desc_" + localizedItemName;
            }

            var newItem = new ItemBuilder(Info)
.SetEnum(localizedItemName).SetNameAndDescription(localizedItemName, localizedItemDesc).SetPickupSound(data.customPickupSound)
.SetShopPrice(shopPrice).SetGeneratorCost(genCost).SetItemComponent<T>().SetSprites($"{spriteName}_small".GetSprite(this, secondFolder: "Items"), $"{spriteName}_big".GetSprite(this, secondFolder: "Items"))
.SetMeta(data.flags, tags).Build();
            return newItem;
        }

        #region Item JSON Loading

        /// <summary>
        /// Loads items from JSON configuration in the plugin's folder.
        /// Override this to provide custom item loading logic.
        /// </summary>
        protected virtual void LoadItemsFromJSON()
        {
            BaseAPI.GetJson<ItemEntry>(this, "Items", out var config);
            if (config?.Data == null || config.Data.Count == 0)
            {
                Log($"{GetPluginInfo().guid}: No items found in Items.json!", LogLevel.Warning);
                return;
            }

            foreach (var entry in config.Data)
                LoadSingleItem(entry);
        }

        /// <summary>
        /// Loads a single item from an ItemEntry configuration.
        /// </summary>
        protected virtual void LoadSingleItem(ItemEntry entry)
        {
            try
            {
                var data = CreateITMDataFromEntry(entry);

                Type itemType = HelperAPI.GetTypeByName(entry.ItemType);
                if (itemType == null)
                {
                    Log($"Item type '{entry.ItemType}' not found for item {entry.Id}!", LogLevel.Fatal);
                    return;
                }

                var method = typeof(GamePlugin).GetMethod("LoadItem").MakeGenericMethod(itemType);
                var parameters = new object[] { data, entry.ShopPrice, entry.GenCost, entry.AssetName, entry.SpriteName, entry.LocalizedName, entry.Tags?.ToArray() ?? [] };

                method.Invoke(this, parameters);

                Log($"Loaded item: {entry.AssetName} (Type: {itemType.Name})", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log($"Error loading item {entry.Id}: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Creates GameItemData from an ItemEntry.
        /// </summary>
        protected virtual GameItemData CreateITMDataFromEntry(ItemEntry entry)
        {
            var d = entry.Data;
            var builder = new ItemDataBuilder().AddChance(d.Chance).AddShopChance(d.ShopChance).AddLocations(d.Locations?.Select(l => Enum.TryParse<PotentialLocations>(l, true, out var p) ? p : PotentialLocations.None).Where(p => p != PotentialLocations.None).ToArray() ?? Array.Empty<PotentialLocations>());

            if (d.Flags?.Any() == true)
            {
                var flags = d.Flags.Aggregate(ItemFlags.None, (current, flag) =>
                    Enum.TryParse<ItemFlags>(flag, true, out var f) ? current | f : current);
                builder.AddFlags(flags);
            }

            if (!string.IsNullOrEmpty(d.CustomPickupSound))
                builder.AddCustomPickupSound(d.CustomPickupSound.GetSound(this, "", Color.white, format: ".ogg", sfxType: SoundType.Effect, folder: "Sounds"));

            return builder.Build();
        }
        #endregion

        public virtual void LoadPoster(out PosterObject newPoster, GamePosterData data, Texture2D posterTexture, string AssetManName)
        {
            if (storage?.posters?.ContainsKey(AssetManName) == true)
            {
                Log($"Already contains poster key: {AssetManName}!", LogLevel.Fatal);
                newPoster = null;
                return;
            }

            newPoster = ObjectCreators.CreatePosterObject(posterTexture, data.Convert());
            data.poster = newPoster;
            assetMan.Add($"{AssetManName}", newPoster);
            assetMan.Add($"{AssetManName}_DATA", data);
            storage?.posters?.Add(AssetManName, data);
            storage?.Add(AssetManName, data);

            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudioloader"))
            {
                LevelLoaderPlugin.Instance.posterAliases.Add(AssetManName, newPoster);
            }
        }

        #region Poster JSON Loading

        public enum PosterType : byte { Random, RoomExclusive }

        /// <summary>
        /// Loads posters from JSON configuration in the plugin's folder.
        /// Override this to provide custom poster loading logic.
        /// </summary>
        protected virtual void LoadPostersFromJSON(PosterType type, RoomCategory exclusiveRooms, string modPrefix)
        {
            List<WeightedPosterObject> roomExclusivePosters = new List<WeightedPosterObject>();
            string AssetName = $"{exclusiveRooms}_Poster0";

            try
            {
                BaseAPI.GetJson<PosterEntry>(this, $"Posters_{exclusiveRooms}", out var config);

                if (config == null) return;

                if (type == PosterType.Random)
                {
                    if (config.Data != null)
                    {
                        foreach (var entry in config.Data)
                        {
                            LoadSinglePoster(entry, type, AssetName, modPrefix);
                        }
                    }
                }
                if (type == PosterType.RoomExclusive)
                {
                    if (config.Data != null)
                    {
                        foreach (var entry in config.Data)
                        {
                            LoadSinglePoster(entry, type, AssetName, modPrefix);

                            if (storage.posters.TryGetValue($"{AssetName}{entry.Id}", out var posterData))
                            {
                                roomExclusivePosters.Add(new WeightedPosterObject()
                                {
                                    weight = entry.Weight,
                                    selection = posterData.poster
                                });
                            }
                        }
                    }

                    RoomAsset[] rooms = Array.FindAll(HelperAPI.LoadAssets<RoomAsset>(), x => x.category == exclusiveRooms);

                    for (int i = 0; i < rooms.Length; i++)
                    {
                        rooms[i].posters.AddRange(roomExclusivePosters);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"{GetPluginInfo().guid}: Error loading objects from JSON: {ex.Message}", LogLevel.Fatal);
            }
        }

        /// <summary>
        /// Loads a single poster from a PosterEntry configuration.
        /// </summary>
        protected virtual void LoadSinglePoster(PosterEntry entry, PosterType type, string assetPrefix, string modPrefix)
        {
            try
            {
                Color textColor = HelperAPI.GetColorStr(entry.TextColor);
                BaldiFonts font = HelperAPI.GetFontInt(entry.FontSize);
                int fontSize = entry.FontSize;
                IntVector2 pos = new IntVector2(entry.Position[0], entry.Position[1]);
                string textKey = entry.TextKey ?? $"{assetPrefix}{entry.Id}";

                var posterData = new PosterDataBuilder().MakeRoomOnly(type == PosterType.RoomExclusive).AddWeight(entry.Weight).AddText(textKey, textColor, pos, font, FontStyles.Normal, fontSize).Build();

                string textureName = $"{assetPrefix}{entry.Id}";
                string textureKey = $"{modPrefix}_{assetPrefix}_Tex_{entry.Id}";

                storage.Add(textureKey, textureName.GetTexture(this, Path.Combine("Sprites", "Posters"), ".png"));

                Texture2D texture = storage.Get<Texture2D>(textureKey);

                if (texture == null)
                {
                    Log($"Texture not found for poster: {textureKey}", LogLevel.Warning);
                    return;
                }

                LoadPoster(out _, posterData, texture, $"{modPrefix}_{assetPrefix}{entry.Id}");
            }
            catch (Exception ex)
            {
                Log($"Error loading poster {entry.Id}: {ex.Message}", LogLevel.Error);
            }
        }
        #endregion

        #region Object JSON Loading
        public virtual void LoadObjectsFromJSON()
        {
            try
            {
                BaseAPI.GetJson<ObjectEntry>(this, "Objects", out var config);

                if (config == null) return;

                foreach (ObjectEntry data in config.Data)
                {
                    Sprite sprite = data.SprName.GetSprite(this, "Sprites", ".png", "Objects");
                    if (sprite == null)
                    {
                        Debug.LogWarning($"Sprite not found: {data.SprName} for object {data.Name}");
                        continue;
                    }

                    Vector3 spriteSize = data.SpriteSize == null ? Vector3.one : new Vector3(
                        data.SpriteSize.Length > 0 ? data.SpriteSize[0] : 1,
                        data.SpriteSize.Length > 1 ? data.SpriteSize[1] : 1,
                        data.SpriteSize.Length > 2 ? data.SpriteSize[2] : 1
                    );

                    LoadObjectWithType(data.Name, data.AssetManName, data.SpriteY, spriteSize, sprite, data.Type);
                }
            }
            catch (Exception ex)
            {
                Log($"{GetPluginInfo().guid}: Error loading objects from JSON: {ex.Message}", LogLevel.Fatal);
            }
        }

        public virtual void LoadObjectWithType(string name, string AssetManName, float spriteY, Vector3 spriteSize, Sprite sprite, string typeName)
        {
            CreateObject(out var newObj, name, spriteY, spriteSize, sprite);

            if (!string.IsNullOrEmpty(typeName))
            {
                Type componentType = HelperAPI.GetTypeByName(typeName);
                if (componentType != null && componentType.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    newObj.AddComponent(componentType);
                }
                else
                {
                    Log($"Component type '{typeName}' not found or is not a MonoBehaviour for object {name}!", LogLevel.Fatal);
                }
            }

            newObj.ConvertToPrefab(true);
            AddObjectToStorage(AssetManName, newObj);
        }
        #endregion

        #region Object Loading
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
        #endregion

        #region NPC Loading
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

                NPCBuilder<T> builder = new NPCBuilder<T>(Info)
        .SetName(data.NameKey).SetEnum(data.NameKey)
        .SetMinMaxAudioDistance(1, data.MaxAudDist).IgnorePlayerOnSpawn()
        .AddLooker().AddTrigger()
        .AddSpawnableRoomCategories(data.RoomCat)
        .SetPoster(data.PosterTexture, PosterNameKey, PosterDescKey);

                if (data.PotentialRooms != null && data.PotentialRooms.Count > 0)
                {
                    builder.AddPotentialRoomAssets(data.PotentialRooms.ToArray());
                }

                npc = builder.Build();

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
        #region NPC JSON Loading

        private static readonly Dictionary<string, System.Type> npcTypes = new Dictionary<string, System.Type>();
        private static readonly Dictionary<string, AudioRolloffMode> _rolloffCache = new Dictionary<string, AudioRolloffMode>
{
    { "Linear", AudioRolloffMode.Linear },
    { "Logarithmic", AudioRolloffMode.Logarithmic },
    { "Custom", AudioRolloffMode.Custom }
};

        /// <summary>
        /// Loads NPCs from JSON configuration in the plugin's folder.
        /// </summary>
        protected virtual void LoadNPCsFromJSON()
        {
            try
            {
                BaseAPI.GetJson<NPCDataEntry>(this, "NPCs", out var config);
                if (config?.Data == null || config.Data.Count == 0)
                {
                    Log($"{GetPluginInfo().guid}: No NPCs found in NPCs.json!", LogLevel.Warning);
                    return;
                }

                foreach (var entry in config.Data)
                    LoadSingleNPC(entry);
            }
            catch (Exception ex)
            {
                Log($"{GetPluginInfo().guid}: Error loading NPCs from JSON: {ex.Message}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Loads a single NPC from an NPCDataEntry configuration.
        /// </summary>
        protected virtual void LoadSingleNPC(NPCDataEntry entry)
        {
            try
            {
                Texture2D posterTexture = null;
                if (!string.IsNullOrEmpty(entry.Data.PosterTexture))
                {
                    posterTexture = entry.Data.PosterTexture.GetTexture(this, folder: Path.Combine("Sprites", "NPCs"), format: ".png");
                }

                Sprite npcSprite = null;
                if (!string.IsNullOrEmpty(entry.SpriteName))
                {
                    npcSprite = entry.SpriteName.GetSprite(this, secondFolder: "NPCs");
                }

                Sprite editorSprite = null;
                if (!string.IsNullOrEmpty(entry.AssetName))
                {
                    string editorSpriteName = $"Editor_{entry.AssetName}";
                    editorSprite = editorSpriteName.GetSprite(this, secondFolder: "NPCs");
                    if (editorSprite == null)
                        editorSprite = npcSprite;
                }

                var rolloff = AudioRolloffMode.Linear;
                if (!string.IsNullOrEmpty(entry.Rolloff) && _rolloffCache.TryGetValue(entry.Rolloff, out var parsedRolloff))
                    rolloff = parsedRolloff;

                var roomCat = RoomCategory.Null;
                if (!string.IsNullOrEmpty(entry.Data.RoomCategory))
                {
                    if (Enum.TryParse(entry.Data.RoomCategory, true, out RoomCategory roomCat2))
                    {
                        roomCat = roomCat2;
                    }
                    if (roomCat == RoomCategory.Null)
                    {
                        roomCat = EnumExtensions.ExtendEnum<RoomCategory>(entry.Data.RoomCategory);
                    }
                }

                var potentialRooms = new List<WeightedRoomAsset>();
                if (!string.IsNullOrEmpty(entry.Data.RoomAssetName))
                {
                    if (storage.rooms.ContainsKey(entry.Data.RoomAssetName))
                    {
                        var roomData = storage.rooms[entry.Data.RoomAssetName];
                        if (roomData.Rooms != null && roomData.Rooms.Count > 0)
                        {
                            potentialRooms = roomData.Rooms;
                        }
                    }
                    else
                    {
                        Log($"Room asset '{entry.Data.RoomAssetName}' not found in storage for NPC {entry.Id}!", LogLevel.Warning);
                    }
                }

                var locations = entry.Data.Locations.Select(l => Enum.TryParse<PotentialLocations>(l, true, out var loc) ? loc : PotentialLocations.None).Where(l => l != PotentialLocations.None).ToArray();

                SoundObject themeMusic = null;
                if (!string.IsNullOrEmpty(entry.ThemeMusic))
                {
                    themeMusic = entry.ThemeMusic.GetSound(this, "", Color.white, format: ".ogg", sfxType: SoundType.Music, folder: "Music");
                }

                SoundObject additionalMusic = null;
                if (!string.IsNullOrEmpty(entry.AdditionalMusic))
                {
                    additionalMusic = entry.AdditionalMusic.GetSound(this, "", Color.white, format: ".ogg", sfxType: SoundType.Music, folder: "Music");
                }

                Log($"Loading NPC: {entry.Id}", LogLevel.Info);
                Log($"  - Sprite: {(npcSprite != null ? npcSprite.name : "NULL")}", LogLevel.Info);
                Log($"  - Poster Texture: {(posterTexture != null ? posterTexture.name : "NULL")}", LogLevel.Info);
                Log($"  - Theme Music: {(themeMusic != null ? themeMusic.name : "NULL")}", LogLevel.Info);
                Log($"  - Room Category: {roomCat}", LogLevel.Info);
                Log($"  - Potential Rooms: {potentialRooms.Count}", LogLevel.Info);

                var npcBuilder = new NPCDataBuilder().AddName(entry.NameKey).AddMaxAudDistance(entry.MaxAudDist).AddSpeed(entry.Speed).AddWeight(entry.Data.Weight).AddRolloff(rolloff).AddPSTTexture(posterTexture).AddSprite(npcSprite).AddEditorSprite(editorSprite).AddMusic(themeMusic).AddAdditionalMusic(additionalMusic).AddRoomCategory(roomCat).AddLocations(locations);
                if (potentialRooms != null && potentialRooms.Count > 0)
                {
                    npcBuilder.AddPotentialRooms(potentialRooms);
                }
                var npcData = npcBuilder.Build();

                if (!npcTypes.TryGetValue(entry.NpcType, out var npcType))
                {
                    npcType = HelperAPI.GetTypeByName(entry.NpcType);
                    if (npcType == null)
                    {
                        Log($"NPC type '{entry.NpcType}' not found for {entry.Id}!", LogLevel.Fatal);
                        return;
                    }
                    npcTypes[entry.NpcType] = npcType;
                }

                var method = typeof(GamePlugin).GetMethod("LoadNPC").MakeGenericMethod(npcType);
                method.Invoke(this, [
            npcData,
            entry.AssetName,
            entry.PosterNameKey,
            entry.PosterDescKey
        ]);

                Log($"Loaded NPC: {entry.AssetName} (Type: {npcType.Name})", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Log($"Error loading NPC {entry.Id}: {ex.Message}", LogLevel.Error);
            }
        }

        #endregion
        #endregion

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
        /// Anything special that needs to be seen by other mods.
        /// </summary>
        public virtual bool special => false;

        /// <summary>
        /// The storage contains the plugin's saved things.
        /// </summary>
        public PluginStorage storage;

        /// <summary>
        /// Mod's Information, like GUID, the Plugin's name, or it's version. Also contains information of it's additions.
        /// </summary>
        public readonly struct ModInfo
        {
            public readonly string guid;
            public readonly string name;
            public readonly string type;
            public readonly HashSet<PluginAddition> additions;

            public ModInfo(string guid, string name, string type, params PluginAddition[] additions)
            {
                this.guid = guid;
                this.name = name;
                this.type = type;
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