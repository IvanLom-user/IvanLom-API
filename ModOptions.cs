using MTM101BaldAPI.OptionsAPI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MyAPI
{
    public abstract class ModOptions : CustomOptionsCategory
    {
        protected Dictionary<string, MenuToggle> synchronizatedToggles = new Dictionary<string, MenuToggle>();

        public override void Build()
        {
            BuildMenu();
            CreateApplyButton(OnApply);
        }

        public abstract void BuildMenu();

        protected void OnApply()
        {
            string[] keys = synchronizatedToggles.Keys.ToArray();
            MenuToggle[] toggles = synchronizatedToggles.Values.ToArray();
            for (int i = 0; i < synchronizatedToggles.Count; i++)
            {
                PlayerPrefs.SetInt(keys[i], toggles[i].Value ? 1 : 0);
            }
            PlayerPrefs.Save();
        }

        protected MenuToggle CreateToggleButton(string synchronizableName, string nameKey, string descKey, Vector3 pos, float width, bool defaultValue = false)
        {
            MenuToggle toggle = CreateToggle(synchronizableName, nameKey, PlayerPrefs.GetInt(synchronizableName, defaultValue ? 1 : 0) == 1, pos, width);

            if (!string.IsNullOrEmpty(descKey)) AddTooltip(toggle, descKey);

            if (!string.IsNullOrEmpty(synchronizableName)) synchronizatedToggles.Add(synchronizableName, toggle);

            return toggle;
        }
    }
}