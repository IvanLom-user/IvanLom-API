using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MyAPI.Core
{
    [BepInPlugin("il.modded.api", "IvanLom_API", "1.2")]
    [BepInProcess("BALDI.exe")]
    public class API_Plugin : GamePlugin
    {
        public delegate void OnNextLevel();
        public OnFloorReset onNextLevel;
        public delegate void OnFloorReset();
        public OnFloorReset onFloorUpdate;

        public List<string> plugins = new List<string>();
        public List<string> items = new List<string>();

        public List<Item> usedItems = new List<Item>();

        public HashSet<Action> frameWaits = new HashSet<Action>();

        public override ModInfo GetPluginInfo() => new ModInfo("il.modded.api", "IvanLom_API");

        public static API_Plugin Instance;
        protected override void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("il.modded.api");
            harmony.PatchAllConditionals();
        }

        public IEnumerator FrameWait(Action action)
        {
            frameWaits.Add(action);
            WaitForSecondsEnvironmentTimescale wait = new WaitForSecondsEnvironmentTimescale(Singleton<BaseGameManager>.Instance.Ec, 3f);
            while (wait.keepWaiting)
            {
                yield return null;
            }
            action.Invoke();
            frameWaits.Remove(action);
        }

        private void Update()
        {
            if (usedItems == null || usedItems.Count <= 0) return;

            for (int i = usedItems.Count - 1; i >= 0; i--)
            {
                Item used = usedItems[i];
                if (used == null)
                {
                    usedItems.RemoveAt(i);
                    continue;
                }
            }
        }

        protected override void LoadImportant() { }
    }
}