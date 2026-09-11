using HarmonyLib;
using MTM101BaldAPI.Reflection;
using MyAPI.Core;
using UnityEngine;

namespace MyAPI.Patches
{
    [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.UseItem))]
    public class Inventory_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(ItemManager __instance)
        {
            if (!(bool)__instance.ReflectionGetVariable("disabled") || __instance.items[__instance.selectedItem].overrideDisabled && __instance.maxItem >= 0)
            {
                Item item = Object.Instantiate(__instance.items[__instance.selectedItem].item);

                if (item.Use(__instance.pm))
                {
                    API_Plugin.Instance.usedItems.Add(item);

                    if (Singleton<CoreGameManager>.Instance.inventoryChallenge)
                    {
                        __instance.ReduceTargetInventorySize();
                    }

                    __instance.RemoveItem(__instance.selectedItem);
                    item?.PostUse(__instance.pm);
                }
            }

            return false;
        }
    }
}