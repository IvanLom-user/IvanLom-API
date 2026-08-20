using MyAPI.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MyAPI.Data
{
    public class GameNPCData
    {
        public CustomNPC Npc;
        public Sprite EditorSprite;
        public string NameKey = "Test";
        public float MaxAudDist = 900f;
        public float Speed = 12f;
        public int Weight = 1000;
        public AudioRolloffMode Rolloff;
        public List<PotentialLocations> Locations = new List<PotentialLocations>();
        public Texture2D PosterTexture;
        public Sprite NpcSprite;
        public SoundObject AdditionalMusic;
        public SoundObject ThemeMusic;
        public RoomCategory RoomCat;
        public List<WeightedRoomAsset> PotentialRooms = new List<WeightedRoomAsset>();

        /// <summary>
        /// Whatever this NPC must be generated or not.
        /// </summary>
        /// <returns>True if the item is able to generate depending on it's locations. False if the locations do not include Floors or Endless.</returns>
        public bool Generate() => (Locations.Contains(PotentialLocations.Floors) || Locations.Contains(PotentialLocations.Endless)) && Weight > 0;

        public GameNPCData(Sprite editorSprite, string nameKey, float maxAudDist, float speed, int weight, AudioRolloffMode rolloff, Texture2D posterTexture, Sprite npcSprite, SoundObject additionalMusic, SoundObject themeMusic, RoomCategory roomCat, List<WeightedRoomAsset> potentialRooms, params PotentialLocations[] locations)
        {
            EditorSprite = editorSprite;
            NameKey = nameKey;
            MaxAudDist = maxAudDist;
            Speed = speed;
            Weight = weight;
            Rolloff = rolloff;
            Locations = locations.ToList();
            PosterTexture = posterTexture;
            NpcSprite = npcSprite;
            AdditionalMusic = additionalMusic;
            ThemeMusic = themeMusic;
            RoomCat = roomCat;
            PotentialRooms = potentialRooms;
        }
    }

    public class NPCDataBuilder
    {
        private string _nameKey = "Test";
        private float _maxAudDist = 900f;
        private float _speed = 12f;
        private int _weight = 1000;
        public PotentialLocations[] _locations = { PotentialLocations.Floors, PotentialLocations.Endless };
        private AudioRolloffMode _rolloff = AudioRolloffMode.Linear;
        private Texture2D _posterTexture;
        public Sprite _editorSprite;
        private Sprite _npcSprite;
        private SoundObject _additionalMusic;
        private SoundObject _themeMusic;
        private RoomCategory _roomCat = RoomCategory.Hall;
        private List<WeightedRoomAsset> _potentialRooms;

        public NPCDataBuilder AddName(string name)
        {
            _nameKey = name;
            return this;
        }

        public NPCDataBuilder AddMaxAudDistance(float dist)
        {
            _maxAudDist = dist;
            return this;
        }

        public NPCDataBuilder AddSpeed(float speed)
        {
            _speed = speed;
            return this;
        }

        public NPCDataBuilder AddWeight(int weight)
        {
            _weight = weight;
            return this;
        }

        public NPCDataBuilder AddLocations(params PotentialLocations[] locations)
        {
            _locations = locations;
            return this;
        }

        public NPCDataBuilder AddRolloff(AudioRolloffMode rolloff)
        {
            _rolloff = rolloff;
            return this;
        }

        public NPCDataBuilder AddPSTTexture(Texture2D texture)
        {
            _posterTexture = texture;
            return this;
        }

        public NPCDataBuilder AddSprite(Sprite sprite)
        {
            _npcSprite = sprite;
            return this;
        }

        public NPCDataBuilder AddEditorSprite(Sprite sprite)
        {
            _editorSprite = sprite;
            return this;
        }

        public NPCDataBuilder AddAdditionalMusic(SoundObject music)
        {
            _additionalMusic = music;
            return this;
        }

        public NPCDataBuilder AddMusic(SoundObject music)
        {
            _themeMusic = music;
            return this;
        }

        public NPCDataBuilder AddRoomCategory(RoomCategory cat)
        {
            _roomCat = cat;
            return this;
        }

        public NPCDataBuilder AddPotentialRooms(List<WeightedRoomAsset> rooms)
        {
            _potentialRooms = rooms;
            return this;
        }

        public GameNPCData Build()
        {
            if (_npcSprite == null)
            {
                throw new NullReferenceException($"NPC's Sprite is null! {_nameKey}!");
            }
            if (_roomCat == RoomCategory.Null)
            {
                throw new NullReferenceException($"NPC's Room Category is null! {_nameKey}!");
            }
            if (_posterTexture == null)
            {
                throw new NullReferenceException($"NPC's Poster Texture is null! {_nameKey}!");
            }
            if (_editorSprite == null)
            {
                _editorSprite = _npcSprite;
            }

            return new GameNPCData(_editorSprite, _nameKey, _maxAudDist, _speed, _weight, _rolloff, _posterTexture, _npcSprite, _additionalMusic, _themeMusic, _roomCat, _potentialRooms, _locations);
        }
    }
}