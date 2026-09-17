using MyAPI.Core;
using MyAPI.NPCs;
using System;
using System.Collections;
using UnityEngine;

namespace MyAPI._Items
{
    public abstract class GameItem : Item
    {
        #region Variables
        protected RaycastHit hit;
        protected Collider[] hits = new Collider[32];
        protected HudGauge gauge;
        public new PlayerManager pm;
        #endregion

        #region Main
        protected virtual void OnDestroy() => DeactivateGauge();
        #endregion

        #region Helpers
        protected virtual bool Try(Action action)
        {
            bool success = true;
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                success = false;
            }
            return success;
        }

        protected CoreGameManager CGM => Singleton<CoreGameManager>.Instance;
 
        protected void AddStamina(float value, bool limited = true) => pm.plm.AddStamina(value, limited);

        protected bool IsLocked(Door door) => door != null && door.locked;

        protected bool NotBroken(Window window) => window != null && !window.IsOpen;

        protected void BreakRule(string rule, float timer) => pm.RuleBreak(rule, timer);

        protected bool IsNear<T>(out T result) where T : MonoBehaviour
        {
            result = null;
            int num = Physics.OverlapSphereNonAlloc(pm.transform.position, pm.pc.Reach + 6f, hits, pm.pc.ClickLayers, QueryTriggerInteraction.Collide);
            for (int i = 0; i < num; i++)
            {
                var found = hits[i];
                if (found != null)
                {
                    if (found.TryGetComponent<T>(out var res))
                    {
                        result = res;
                    }
                }
            }
            return result != null;
        }

        protected bool Raycast => Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.Reach, pm.pc.ClickLayers);

        protected void ActivateGauge(Sprite icon, float total)
        {
            if (CGM == null) return;

            HudManager hud = CGM.GetHud(pm.playerNumber);
            if (gauge == null && hud != null && hud.gaugeManager != null)
            {
                gauge = hud.gaugeManager.ActivateNewGauge(icon, total);
            }
        }

        protected void DeactivateGauge()
        {
            if (gauge != null)
            {
                gauge.Deactivate();
                gauge = null;
            }
        }

        protected void AddPoints(int points, bool animation = true)
        {
            if (pm != null && CGM != null)
            {
                CGM.AddPoints(points, pm.playerNumber, animation);
            }
        }

        protected void AlertNPCs(GameObject source, int priority, params CustomNPC[] npcs)
        {
            if (npcs == null || npcs.Length <= 0) return;

            for (int i = 0; i < npcs.Length; i++)
            {
                var npc = npcs[i];
                if (npc == null) continue;

                npc.Hear(source.gameObject, source.transform.position, priority);
                npc.Alert(source.transform.position);
            }
        }

        protected void AlertNPCs(GameObject source, int priority, params NPC[] npcs)
        {
            if (npcs == null || npcs.Length <= 0) return;

            for (int i = 0; i < npcs.Length; i++)
            {
                var npc = npcs[i];
                if (npc == null) continue;

                npc.Hear(source.gameObject, source.transform.position, priority);
                npc.Navigator.FindPath(source.transform.position);
            }
        }

        protected IEnumerator ActivateTimer(float total, Action after)
        {
            float timer = total;
            while (timer > 0f)
            {
                timer -= Time.deltaTime * Singleton<BaseGameManager>.Instance.Ec.EnvironmentTimeScale;
                yield return null;
            }

            after?.Invoke();
        }

        protected IEnumerator ActivateGaugedTimer(float total, Action after)
        {
            float timer = total;
            while (timer > 0f)
            {
                timer -= Time.deltaTime * Singleton<BaseGameManager>.Instance.Ec.EnvironmentTimeScale;
                if (gauge != null)
                {
                    gauge.SetValue(total, timer);
                }
                yield return null;
            }

            DeactivateGauge();
            after?.Invoke();
        }
        #endregion
    }
}