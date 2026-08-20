using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MyAPI.Data
{
    public class GameRoomData
    {
        /// <summary>
        /// The room's file path.
        /// </summary>
        public string FilePath;

        /// <summary>
        /// The room's sprite in Level Studio.
        /// </summary>
        public Sprite EditorSprite;

        /// <summary>
        /// The room's Weight. Depending on the weight, the generator decides will it spawn the room or not.
        /// </summary>
        public int Weight = 1000;

        /// <summary>
        /// The room's door materials.
        /// </summary>
        public StandardDoorMats DoorMats;

        /// <summary>
        /// The room's category.
        /// </summary>
        public RoomCategory RoomCat;

        /// <summary>
        /// The room's map color.
        /// </summary>
        public Color MapColor;

        /// <summary>
        /// The room's additions. Currently only one - Randomize Lockers.
        /// </summary>
        public List<RoomAddition> Additions = new List<RoomAddition>() { RoomAddition.None };

        public List<WeightedRoomAsset> Rooms { get; protected set; } = new List<WeightedRoomAsset>();
        public RoomTex RoomTextures { get; protected set; }

        public GamePlugin plugin;

        public GameRoomData(GamePlugin plugin, Sprite editorSprite, string filePath, int weight, StandardDoorMats doorMats, RoomCategory roomCat, Color mapColor, params RoomAddition[] additions)
        {
            this.plugin = plugin;
            EditorSprite = editorSprite;
            FilePath = filePath;
            Weight = weight;
            Additions = additions.ToList();
            DoorMats = doorMats;
            RoomCat = roomCat;
            MapColor = mapColor;
        }

        /// <summary>
        /// Will return all paths that contain a .rblpl file in the room's FilePath.
        /// </summary>
        /// <param name="plugin">The plugin containing the room file(s).</param>
        /// <returns></returns>
        public string[] GetRoomPaths(GamePlugin plugin) => Directory.GetFiles(FilePath, "*.rbpl");

        /// <summary>
        /// Reads the room file from a specific path.
        /// </summary>
        /// <param name="roomPath">The .rbpl file path.</param>
        /// <param name="formatAsset">Read asset. Not created.</param>
        /// <param name="asset">Created asset based of the format asset.</param>
        public void Read(string roomPath, out BaldiRoomAsset formatAsset, out ExtendedRoomAsset asset)
        {
            try
            {
                BinaryReader reader = new BinaryReader(File.OpenRead(roomPath));
                formatAsset = BaldiRoomAsset.Read(reader);
                reader.Close();

                try
                {
                    asset = LevelImporter.CreateRoomAsset(formatAsset);
                }
                catch (Exception ex)
                {
                    asset = null;
                    if (plugin != null)
                    {
                        plugin.Log($"Error creating room asset: {ex.Message}", BepInEx.Logging.LogLevel.Error);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                formatAsset = null;
                asset = null;
                if (plugin != null)
                {
                    plugin.Log($"Error when reading a room file: {ex.Message}!", BepInEx.Logging.LogLevel.Fatal);
                }
                throw;
            }
        }

        /// <summary>
        /// Adds the room(s) to a specific WeightedRoomAsset list.
        /// </summary>
        /// <param name="rooms">The room(s) will be added to this list.</param>
        public void AddToList(RoomTex roomTextures)
        {
            string[] roomPaths = GetRoomPaths(plugin);

            if (roomPaths == null || roomPaths.Length == 0)
            {
                plugin.Log($"No room files found in path: {FilePath}", BepInEx.Logging.LogLevel.Error);
                return;
            }

            RoomTextures = roomTextures;

            for (int i = 0; i < roomPaths.Length; i++)
            {
                plugin.Log($"Loading room: {roomPaths[i]}", BepInEx.Logging.LogLevel.Info);
                Read(roomPaths[i], out var formatAsset, out var asset);
                ApplyAdditions(ref asset, formatAsset);
                asset.doorMats = DoorMats;
                asset.wallTex = roomTextures.wall;
                asset.florTex = roomTextures.floor;
                asset.ceilTex = roomTextures.ceil;
                asset.color = MapColor;
                asset.category = RoomCat;
                Rooms.Add(new()
                {
                    selection = asset,
                    weight = Weight
                });
            }
        }

        /// <summary>
        /// Applies the room's addition effects.
        /// </summary>
        /// <param name="asset">The created asset.</param>
        /// <param name="formatAsset">Read asset that is not created.</param>
        public void ApplyAdditions(ref ExtendedRoomAsset asset, BaldiRoomAsset formatAsset)
        {
            if (Additions.Contains(RoomAddition.RandomizeLockers))
            {
                if (formatAsset.basicObjects.Find(x => x.prefab == "locker") != null)
                {
                    asset.basicSwaps = new()
                    {
                        new()
                        {
                            chance = 0.05f,
                            potentialReplacements =
                            [
                                new()
                                {
                                    weight = 100,
                                    selection = LevelLoaderPlugin.Instance.basicObjects["bluelocker"].transform
                                }
                            ],
                            prefabToSwap = LevelLoaderPlugin.Instance.basicObjects["locker"].transform
                        }
                    };
                }
            }
            asset.lightPre = LevelLoaderPlugin.Instance.lightTransforms["standardhanging"];
        }
    }

    public class RoomDataBuilder
    {
        private string _filePath;
        private int _weight = 1000;
        public Sprite _editorSprite;
        private StandardDoorMats _doorMats;
        private RoomCategory _roomCat = RoomCategory.Null;
        private Color _mapColor = Color.black;
        private List<RoomAddition> _additions = new List<RoomAddition>() { RoomAddition.None };

        public RoomDataBuilder AddEditorSprite(Sprite sprite)
        {
            _editorSprite = sprite;
            return this;
        }

        public RoomDataBuilder AddPath(string path)
        {
            _filePath = path;
            return this;
        }

        public RoomDataBuilder AddWeight(int weight)
        {
            _weight = weight;
            return this;
        }

        public RoomDataBuilder AddDoorMaterials(StandardDoorMats doorMaterials)
        {
            _doorMats = doorMaterials;
            return this;
        }

        public RoomDataBuilder AddRoomCategory(RoomCategory roomCat)
        {
            _roomCat = roomCat;
            return this;
        }

        public RoomDataBuilder AddMapColor(Color mapColor)
        {
            _mapColor = mapColor;
            return this;
        }

        public RoomDataBuilder AddAdditions(params RoomAddition[] additions)
        {
            _additions = additions.ToList();
            return this;
        }

        public GameRoomData Build(GamePlugin plugin)
        {
            return new GameRoomData(plugin, _editorSprite, _filePath, _weight, _doorMats, _roomCat, _mapColor, _additions.ToArray()); ;
        }
    }

    /// <summary>
    /// The additions the room will have. There will be more additions later.
    /// </summary>
    public enum RoomAddition : byte
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        None,
        /// <summary>
        /// Randomizes the blue and not blue lockers.
        /// </summary>
        RandomizeLockers
    }

    public readonly struct RoomTex
    {
        public readonly string wallKey;
        public readonly string floorKey;
        public readonly string ceilKey;
        public readonly Texture2D wall;
        public readonly Texture2D floor;
        public readonly Texture2D ceil;

        public RoomTex(string wallKey, Texture2D wallTex, string floorKey, Texture2D floorTex, string ceilKey, Texture2D ceilTex)
        {
            this.wallKey = wallKey;
            this.wall = wallTex;
            this.floorKey = floorKey;
            this.floor = floorTex;
            this.ceilKey = ceilKey;
            this.ceil = ceilTex;
        }
    }
}