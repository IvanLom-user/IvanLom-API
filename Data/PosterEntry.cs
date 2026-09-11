namespace MyAPI.Data
{
    public class PosterEntry
    {
        public int Id { get; set; }
        public string TextKey { get; set; }
        public string TextColor { get; set; } = "white";
        public int FontSize { get; set; } = 18;
        public int[] Position { get; set; } = [32, 32];
        public int Weight { get; set; } = 250;
    }
}