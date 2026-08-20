using BepInEx.Logging;
using MyAPI.Core;
using PlusLevelStudio;
using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.Tools;
using PlusStudioLevelFormat;
using UnityEngine;

namespace MyAPI
{
    public class GamePluginEditor
    {
        public GamePlugin plugin;

        public void AddEditorStuff()
        {
            if (plugin == null)
            {
                Debug.LogError("Plugin is null! - GamePluginEditorSupport");
                return;
            }

            if (plugin.storage.rooms != null && plugin.storage.rooms.Count > 0)
            {
                foreach (var room in plugin.storage.rooms)
                {
                    if (room.Value != null && room.Key != string.Empty)
                    {
                        LoadRoomTextures(room.Key, [room.Value.RoomTextures.floorKey, room.Value.RoomTextures.wallKey, room.Value.RoomTextures.ceilKey]);
                    }
                }
            }
            if (plugin.storage.npcs != null && plugin.storage.npcs.Count > 0)
            {
                foreach (var npc in plugin.storage.npcs)
                {
                    if (npc.Value != null && npc.Key != string.Empty)
                    {
                        EditorInterface.AddNPCVisual(npc.Key, npc.Value.Npc);
                    }
                }
            }
            if (plugin.storage.prefabs != null && plugin.storage.prefabs.Count > 0)
            {
                foreach (var prefab in plugin.storage.prefabs)
                {
                    if (prefab.Value != null && prefab.Key != string.Empty)
                    {
                        EditorInterface.AddObjectVisualWithCustomCapsuleCollider(prefab.Key, prefab.Value, 3.5f, 5f, 0, Vector3.zero);
                    }
                }
            }
            EditorInterfaceModes.AddModeCallback(AddContent);
        }

        public void AddContent(EditorMode mode, bool vanilla)
        {
            if (plugin == null)
            {
                Debug.LogError("Plugin is null! - GamePluginEditorSupport");
                return;
            }

            if (plugin.storage.rooms != null && plugin.storage.rooms.Count > 0)
            {
                foreach (var room in plugin.storage.rooms)
                {
                    if (room.Value != null && room.Key != string.Empty)
                    {
                        AddRoom(mode, room.Key, room.Value.EditorSprite);
                    }
                }
            }
            if (plugin.storage.npcs != null && plugin.storage.npcs.Count > 0)
            {
                foreach (var npc in plugin.storage.npcs)
                {
                    if (npc.Value != null && npc.Key != string.Empty)
                    {
                        AddNPC(mode, npc.Key, npc.Value.EditorSprite);
                        AddPoster(mode, npc.Key + "_Poster");
                    }
                }
            }
            if (plugin.storage.items != null && plugin.storage.items.Count > 0)
            {
                foreach (var item in plugin.storage.items)
                {
                    if (item.Value != null && item.Key != string.Empty)
                    {
                        AddItem(mode, item.Key, item.Value.item.itemSpriteSmall);
                    }
                }
            }
            if (plugin.storage.posters != null && plugin.storage.posters.Count > 0)
            {
                foreach (var poster in plugin.storage.posters)
                {
                    if (poster.Value != null && poster.Key != string.Empty)
                    {
                        AddPoster(mode, poster.Key);
                    }
                }
            }
            if (plugin.storage.prefabs != null && plugin.storage.prefabs.Count > 0)
            {
                foreach (var prefab in plugin.storage.prefabs)
                {
                    if (prefab.Value != null && prefab.Key != string.Empty)
                    {
                        AddObject(mode, prefab.Key, prefab.Value.GetComponentInChildren<SpriteRenderer>().sprite);
                    }
                }
            }
        }

        public void LoadRoomTextures(string roomId, string[] roomTextures)
        {
            LevelStudioPlugin loaderPlugin = LevelStudioPlugin.Instance;

            if (!loaderPlugin.selectableTextures.Contains(roomTextures[0]))
                loaderPlugin.selectableTextures.Add(roomTextures[0]);
            if (!loaderPlugin.selectableTextures.Contains(roomTextures[1]))
                loaderPlugin.selectableTextures.Add(roomTextures[1]);
            if (!loaderPlugin.selectableTextures.Contains(roomTextures[2]))
                loaderPlugin.selectableTextures.Add(roomTextures[2]);

            if (!loaderPlugin.defaultRoomTextures.ContainsKey(roomId))
            {
                loaderPlugin.defaultRoomTextures.Add(roomId, new TextureContainer(roomTextures[0], roomTextures[1], roomTextures[2]));
            }
            else
            {
                loaderPlugin.defaultRoomTextures[roomId] = new TextureContainer(roomTextures[0], roomTextures[1], roomTextures[2]);
            }
        }

        public void AddTool<T>(EditorMode mode, T tool, string cat) where T : EditorTool => EditorInterfaceModes.AddToolToCategory(mode, cat, tool);

        public void AddRoom(EditorMode mode, string id, Sprite editorSprite) => AddTool(mode, new RoomTool(id, editorSprite), "rooms");
        public void AddItem(EditorMode mode, string id, Sprite editorSprite) => AddTool(mode, new ItemTool(id, editorSprite), "items");
        public void AddNPC(EditorMode mode, string id, Sprite editorSprite) => AddTool(mode, new NPCTool(id, editorSprite), "npcs");
        public void AddPoster(EditorMode mode, string id) => AddTool(mode, new PosterTool(id), "posters");
        public void AddPoster(EditorMode mode, string id, Sprite editorSprite) => AddTool(mode, new PosterTool(id, editorSprite), "posters");
        public void AddObject(EditorMode mode, string id, Sprite editorSprite) => AddTool(mode, new ObjectToolSubtileNoRotation(id, editorSprite), "objects");
    }
}