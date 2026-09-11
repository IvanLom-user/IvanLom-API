using System.Collections.Generic;

namespace MyAPI.Data
{
    public class ItemEntry
    {
        public string Id { get; set; }
        public string AssetName { get; set; }
        public string SpriteName { get; set; }
        public string LocalizedName { get; set; }
        public int ShopPrice { get; set; } = 0;
        public int GenCost { get; set; } = 0;
        public List<string> Tags { get; set; } = new List<string>();
        public string ItemType { get; set; } = "ITM_Quarter";
        public ItemDataEntry Data { get; set; } = new ItemDataEntry();
    }

    public class ItemDataEntry
    {
        public int Chance { get; set; } = 1000;
        public int ShopChance { get; set; } = 1000;
        public List<string> Locations { get; set; } = new List<string>();
        public List<string> Flags { get; set; } = new List<string>();
        public string CustomPickupSound { get; set; }
    }
}