using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using MyAPI.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MyAPI
{
    public static class GeneratorAPI
    {
        /// <summary>
        /// Prepare the custom NPC for the level.
        /// </summary>
        /// <param name="character">The NPC Prefab.</param>
        /// <param name="sprite">The sprite for the NPC.</param>
        /// <param name="music">Music the NPC will play.</param>
        /// <param name="additionalMusic">Additional music the NPC will play. (also can be used as voicelines of the music like for polish cow)</param>
        /// <param name="speed">NPC's Default Speed.</param>
        public static void LoadNPC(this CustomNPC character, GamePlugin plugin, Sprite sprite, SoundObject music, SoundObject additionalMusic, float maxDistance, AudioRolloffMode rolloff, float speed = 12f)
        {
            if (plugin == null)
            {
                Debug.LogError("The given plugin is null!");
                return;
            }

            if (character == null)
            {
                plugin.Log("The custom character is null!", BepInEx.Logging.LogLevel.Fatal);
                return;
            }

            character.audMan = character.gameObject.AddComponent<AudioManager>();
            AudioSource charAudSource = character.audMan.gameObject.AddComponent<AudioSource>();
            charAudSource.minDistance = 1f;
            charAudSource.maxDistance = maxDistance;
            charAudSource.rolloffMode = rolloff;
            charAudSource.spatialBlend = 1;
            character.audMan.audioDevice = charAudSource;
            character.wahahAudMan = character.gameObject.AddComponent<PropagatedAudioManager>();
            if (music != null)
            {
                character.wahahAudMan.ReflectionSetVariable("soundOnStart", new SoundObject[] { music });
                character.wahahAudMan.ReflectionSetVariable("loopOnStart", true);
            }

            if (additionalMusic != null)
            {
                var newAudMan = new GameObject();
                newAudMan.name = "AdditionalAudio";
                newAudMan.transform.SetParent(character.transform);
                var newAudioDevice = newAudMan.gameObject.AddComponent<AudioSource>();
                newAudioDevice.minDistance = 1f;
                newAudioDevice.maxDistance = character.audMan.audioDevice.maxDistance;
                newAudioDevice.rolloffMode = character.audMan.audioDevice.rolloffMode;
                newAudioDevice.spatialBlend = character.audMan.audioDevice.spatialBlend;
                character.additionalWahahAudMan = newAudMan.AddComponent<PropagatedAudioManager>();
                character.additionalWahahAudMan.audioDevice = newAudioDevice;
                character.additionalWahahAudMan.ReflectionSetVariable("soundOnStart", new SoundObject[] { additionalMusic });
                character.additionalWahahAudMan.ReflectionSetVariable("loopOnStart", true);
            }

            if (character.spriteRenderer[0] == null)
            {
                character.spriteRenderer[0] = character.gameObject.GetComponent<SpriteRenderer>() != null ?
                    character.gameObject.GetComponent<SpriteRenderer>() : character.gameObject.AddComponent<SpriteRenderer>();
            }

            character.Navigator?.SetSpeed(speed);
            character.spriteRenderer[0].transform.localScale = Vector3.one * 0.8f;
            character.spriteRenderer[0].transform.position += new Vector3(0.1f, 0.1f, 0f);
            character.spriteRenderer[0].sprite = sprite;

            plugin.Log($"NPC: {character.name};" 
                + music == null ? "No music will be given; " : $"Music: {music.soundClip.name}; "
                + $"The audio is {character.audMan}; "
                + $"Sprite: {character.spriteRenderer[0]}.", BepInEx.Logging.LogLevel.Info);
        }

        /// <summary>
        /// Adds the NPC to the loader. (the NPC will be able to spawn in a specific floor, with a specific chance)
        /// </summary>
        /// <param name="character">NPC Prefab.</param>
        /// <param name="floorName">The floor's name it can spawn in.</param>
        /// <param name="floorNumber">The floor it can spawn in.</param>
        /// <param name="sceneObject">Scene it can spawn in.</param>
        /// <param name="weight">The weight the NPC spawns or not.</param>
        /// <param name="potentialFloorNames">The levels the NPC is able to spawn (endless, "END", "F", etc.)</param>
        public static void SpawnNPC(this CustomNPC character, string floorName, int floorNumber, SceneObject sceneObject, int weight, params PotentialLocations[] locations)
        {
            if (character == null)
            {
                Debug.LogError("NullReferenceException: The custom character is null!");
                return;
            }
            CustomLevelObject[] levelObjects = sceneObject.GetCustomLevelObjects();
            foreach (string availableFloorNames in GetPotentialLevels(locations.ToList()))
            {
                if (floorName.StartsWith(availableFloorNames) || sceneObject.GetMeta().tags.Contains(availableFloorNames))
                {
                    sceneObject.potentialNPCs.Add(new WeightedNPC() { selection = character, weight = weight });
                    sceneObject.MarkAsNeverUnload();
                    break;
                }
            }
        }

        /// <summary>
        /// Adds an item to the shop. (spawns item with a specific chance in the shop)
        /// </summary>
        /// <param name="itm">The item prefab.</param>
        /// <param name="floorName">The floor's name it can spawn.</param>
        /// <param name="floorNumber">The floor it can spawn</param>
        /// <param name="sceneObject">The scene it can spawn</param>
        /// <param name="chance">The weight it can spawn.</param>
        /// <param name="potentialFloorNames">The levels it can spawn.</param>
        public static void AddItemInTheShop(this ItemObject itm, string floorName, int floorNumber, SceneObject sceneObject, int chance, params PotentialLocations[] locations)
        {
            if (itm == null)
            {
                Debug.LogError("NullReferenceException: The custom item is null!");
                return;
            }
            CustomLevelObject[] levelObjects = sceneObject.GetCustomLevelObjects();
            foreach (string availableFloorNames in GetPotentialLevels(locations.ToList()))
            {
                if (floorName.StartsWith(availableFloorNames) || sceneObject.GetMeta().tags.Contains(availableFloorNames))
                {
                    WeightedItemObject[] currentItems = sceneObject.shopItems;
                    int currentLength = currentItems != null ? currentItems.Length : 0;
                    WeightedItemObject[] newArray = new WeightedItemObject[currentLength + 1];

                    if (currentLength > 0)
                        Array.Copy(currentItems, newArray, currentLength);

                    newArray[currentLength] = new WeightedItemObject() { selection = itm, weight = chance };

                    sceneObject.totalShopItems = Math.Min(sceneObject.totalShopItems + 2, 10);
                    sceneObject.shopItems = newArray;
                    sceneObject.MarkAsNeverUnload();
                    break;
                }
            }
        }

        /// <summary>
        /// Adds an item to the loader. (spawns item in a specific floor, level, with specific chance)
        /// </summary>
        /// <param name="itm">The item prefab.</param>
        /// <param name="plugin">The plugin that spawned the item.</param>
        /// <param name="floorName">The floor's name it can spawn.</param>
        /// <param name="floorNumber">The floor it can spawn</param>
        /// <param name="sceneObject">The scene it can spawn</param>
        /// <param name="chance">The weight it can spawn.</param>
        /// <param name="potentialFloorNames">The levels it can spawn.</param>
        public static void GenerateItem(this ItemObject itm, GamePlugin plugin, string floorName, int floorNumber, SceneObject sceneObject, int chance, params PotentialLocations[] locations)
        {
            if (itm == null)
            {
                Debug.LogError("NullReferenceException: The custom item is null!");
                return;
            }

            CustomLevelObject[] levelObjects = sceneObject.GetCustomLevelObjects();
            foreach (string availableFloorNames in GetPotentialLevels(locations.ToList()))
            {
                if ((floorName.StartsWith(availableFloorNames) || sceneObject.GetMeta().tags.Contains(availableFloorNames)))
                {
                    for (int i = 0; i < levelObjects.Length; i++)
                    {
                        if (levelObjects[i].IsModifiedByMod(plugin.Info)) continue;
                        levelObjects[i].potentialItems = levelObjects[i].potentialItems.AddItem(new WeightedItemObject() { selection = itm, weight = chance }).ToArray();
                        levelObjects[i].MarkAsModifiedByMod(plugin.Info);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Converts the potential locations enum array to the string array.
        /// </summary>
        /// <returns>An array of in-game levels.</returns>
        public static string[] GetPotentialLevels(List<PotentialLocations> locations)
        {
            List<string> levels = new List<string>();
            if (locations.Contains(PotentialLocations.Floors))
            {
                levels.Add("F");
            }
            if (locations.Contains(PotentialLocations.Endless))
            {
                levels.Add("endless");
                levels.Add("END");
            }
            if (locations.Contains(PotentialLocations.Pitstop))
            {
                levels.Add("PIT");
            }
            return levels.ToArray();
        }
    }

    /// <summary>
    /// Levels of the game.
    /// </summary>
    public enum PotentialLocations : byte
    {
        None,
        /// <summary>
        /// Any floor except Endless: 1, 2, 3, 4 and 5
        /// </summary>
        Floors,
        /// <summary>
        /// Any Endless floor.
        /// </summary>
        Endless,
        /// <summary>
        /// Any level marked as Johnny's Store.
        /// </summary>
        Pitstop
    }
}