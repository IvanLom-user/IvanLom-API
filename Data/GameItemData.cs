using MTM101BaldAPI.Registers;
using System.Collections.Generic;
using System.Linq;

namespace MyAPI.Data
{
    public class GameItemData
    {
        /// <summary>
        /// The item's reference.
        /// </summary>
        public ItemObject item;

        /// <summary>
        /// Special pickup sound for this item. Can be left as null if you don't want to override the pickup sound.
        /// </summary>
        public SoundObject customPickupSound;

        /// <summary>
        /// Chance of the item generating in a level except the Johnny's Store.
        /// </summary>
        public int chance = 1000;

        /// <summary>
        /// Chance of the item appearing in the Johnny's Store.
        /// </summary>
        public int shopChance = 1000;

        /// <summary>
        /// The levels the item can spawn in.
        /// </summary>
        public List<PotentialLocations> locations = new List<PotentialLocations>();

        /// <summary>
        /// Item's Flags. Mostly for other mods to work properly with this mod.
        /// </summary>
        public ItemFlags flags;

        /// <summary>
        /// Whatever this item must be generated or it is PitStop™-only.
        /// </summary>
        /// <returns>True if the item is able to generate depending on it's locations. False if the locations do not include Floors or Endless.</returns>
        public bool Generate() => (locations.Contains(PotentialLocations.Floors) || locations.Contains(PotentialLocations.Endless)) && chance > 0;

        public GameItemData(int chance, int shopChance, ItemFlags flags, SoundObject customPickupSound = null, params PotentialLocations[] locations)
        {
            this.chance = chance;
            this.shopChance = shopChance;
            this.locations = locations.ToList();
            this.flags = flags;

            if (customPickupSound != null)
            {
                this.customPickupSound = customPickupSound;
            }
        }
    }

    public class ItemDataBuilder
    {
        private SoundObject _pickSfx;
        private int _chance = 0;
        private int _shopChance = 0;
        private List<PotentialLocations> _locations = new List<PotentialLocations>();
        private ItemFlags _flags = ItemFlags.None;

        public ItemDataBuilder AddCustomPickupSound(SoundObject sound)
        {
            _pickSfx = sound;
            return this;
        }

        public ItemDataBuilder AddChance(int chance)
        {
            _chance = chance;
            return this;
        }

        public ItemDataBuilder AddShopChance(int shopChance)
        {
            _shopChance = shopChance;
            return this;
        }

        public ItemDataBuilder AddLocations(params PotentialLocations[] locations)
        {
            _locations = locations.ToList();
            return this;
        }

        public ItemDataBuilder AddFlags(ItemFlags flags)
        {
            _flags = flags;
            return this;
        }

        public GameItemData Build() => new GameItemData(_chance, _shopChance, _flags, _pickSfx, _locations.ToArray());
    }
}