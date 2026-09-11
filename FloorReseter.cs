using HarmonyLib;
using MyAPI.Core;
using UnityEngine;

namespace Raldi
{
    [HarmonyPatch(typeof(BaseGameManager))]
    public static class FloorUpdater
    {
        [HarmonyPatch(nameof(BaseGameManager.RestartLevel)), HarmonyPrefix]
        private static void RestartLevel_Prefix() => ClearData();

        [HarmonyPatch(nameof(BaseGameManager.BeginPlay)), HarmonyPrefix]
        private static void BeginPlay_Prefix() => ClearData();

        [HarmonyPatch(nameof(BaseGameManager.LoadNextLevel)), HarmonyPrefix]
        private static void LoadNextLevel_Prefix()
        {
            ClearData();
            OnNextLevel();
        }

        public static void ClearData()
        {
            API_Plugin plugin = API_Plugin.Instance;
            if (plugin == null)
            {
                Debug.LogError("The API Plugin is not found!");
                return;
            }
            if (plugin.onFloorUpdate == null) return;

            plugin.onFloorUpdate.Invoke();
        }

        public static void OnNextLevel()
        {
            API_Plugin plugin = API_Plugin.Instance;
            if (plugin == null)
            {
                Debug.LogError("The API Plugin is not found!");
                return;
            }
            if (plugin.onNextLevel == null) return;

            plugin.onNextLevel.Invoke();
        }
    }
}