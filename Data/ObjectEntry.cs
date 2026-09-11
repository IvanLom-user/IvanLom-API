namespace MyAPI.Data
{
    public class ObjectEntry
    {
        public string Name { get; set; }
        public string AssetManName { get; set; }
        public float SpriteY { get; set; }
        public float[] SpriteSize { get; set; } = [1, 1, 1];
        public string SprName { get; set; }
        public string Type { get; set; }
    }
}