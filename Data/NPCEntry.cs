using System.Collections.Generic;

namespace MyAPI.Data
{
    public class NPCDataEntry
    {
        public string Id { get; set; }
        public string AssetName { get; set; }
        public string NameKey { get; set; }
        public string PosterNameKey { get; set; }
        public string PosterDescKey { get; set; }
        public string NpcType { get; set; } = "CustomNPC";
        public string SpriteName { get; set; }
        public string ThemeMusic { get; set; }
        public string AdditionalMusic { get; set; }
        public float MaxAudDist { get; set; } = 20f;
        public float Speed { get; set; } = 3f;
        public string Rolloff { get; set; } = "Linear";
        public NPCDataExtra Data { get; set; } = new NPCDataExtra();
    }

    public class NPCDataExtra
    {
        public int Weight { get; set; } = 250;
        public string RoomCategory { get; set; } = "Hall";
        public string RoomAssetName { get; set; }
        public string PosterTexture { get; set; }
        public List<string> Locations { get; set; } = new List<string> { "Floors", "Endless" };
    }
}