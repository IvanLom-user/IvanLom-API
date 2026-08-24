using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using System.Collections.Generic;

namespace MyAPI.Core
{
    [BepInPlugin("il.modded.api", "IvanLom_API", "1.0")]
    [BepInProcess("BALDI.exe")]
    public class API_Plugin : GamePlugin
    {
        public delegate void OnNextLevel();
        public OnFloorReset onNextLevel;
        public delegate void OnFloorReset();
        public OnFloorReset onFloorUpdate;

        public List<string> plugins = new List<string>();

        public override ModInfo GetPluginInfo() => new ModInfo("il.modded.api", "IvanLom_API", "1.0");

        public static API_Plugin Instance;
        protected override void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("il.modded.api");
            harmony.PatchAllConditionals();
        }

        protected override void LoadImportant() { }
    }
}