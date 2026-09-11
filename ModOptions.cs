using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MyAPI
{
    public abstract class ModOptions : CustomOptionsCategory
    {
        protected Dictionary<string, MenuToggle> synchronizatedToggles = new Dictionary<string, MenuToggle>();
        protected Dictionary<string, AdjustmentBars> synchronizatedBars = new Dictionary<string, AdjustmentBars>();

        public override void Build()
        {
            BuildMenu();
            CreateApplyButton(OnApply);
        }

        public abstract void BuildMenu();

        protected virtual void OnApply()
        {
            string[] keys = synchronizatedToggles.Keys.ToArray();
            MenuToggle[] toggles = synchronizatedToggles.Values.ToArray();
            for (int i = 0; i < synchronizatedToggles.Count; i++)
            {
                PlayerPrefs.SetInt(keys[i], toggles[i].Value ? 1 : 0);
            }
            string[] barKeys = synchronizatedBars.Keys.ToArray();
            AdjustmentBars[] bars = synchronizatedBars.Values.ToArray();
            for (int i = 0; i < synchronizatedBars.Count; i++)
            {
                PlayerPrefs.SetInt(barKeys[i], Mathf.RoundToInt((int)bars[i].ReflectionGetVariable("val")));
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

        protected AdjustmentBars CreateBarsButtons(string synchronizableName, string nameKey, string descKey, Vector3 pos, int width, int defaultIndex)
        {
            var bars = CreateBars(OnApply, nameKey, pos, width);
            bars.ReflectionSetVariable("val", PlayerPrefs.GetInt(synchronizableName, defaultIndex));
            bars.ReflectionInvoke("UpdateBars", null);

            if (!string.IsNullOrEmpty(descKey)) AddTooltip(bars, descKey);

            if (!string.IsNullOrEmpty(synchronizableName)) synchronizatedBars.Add(synchronizableName, bars);

            return bars;
        }

        protected StandardMenuButton CreateButton(Sprite spr, string name, string descKey, Vector3 pos, Vector2 sizeDelta)
        {
            var img = CreateButton(none, spr, name, pos, sizeDelta);

            if (!string.IsNullOrEmpty(descKey)) AddTooltip(img, descKey);

            return img;
        }

        private void none() { }
    }
}