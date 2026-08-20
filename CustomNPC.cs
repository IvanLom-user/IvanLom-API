using MyAPI.Additions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyAPI.NPCs
{
    public class CustomNPC : NPC
    {
        protected List<ItemLifetime> items = new List<ItemLifetime>();
        public AudioManager audMan;
        public AudioManager wahahAudMan;
        public AudioManager additionalWahahAudMan;
        public PlayerManager pm;
        private CoreGameManager cgm;
        public bool overrideSpeed;
        public float timerTime;

        public delegate void OnInitialize();
        public delegate void WhenDestroyedNPC();

        public void Do(params Action[] actions)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                var action = actions[i];
                if (action == null) continue;
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        public CoreGameManager GetCGM()
        {
            if (cgm == null)
            {
                cgm = Singleton<CoreGameManager>.Instance;
            }
            return cgm;
        }

        public IEnumerator SingleTimerCoroutine(float time, Action action)
        {
            yield return new WaitForSeconds(time);
            action?.Invoke();
        }

        public IEnumerator TimerCoroutine(float time, Action action)
        {
            while (true)
            {
                yield return new WaitForSeconds(time);
                action?.Invoke();
            }
        }

        public IEnumerator TimerCoroutine(float minRandom, float maxRandom, Action action)
        {
            while (true)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(minRandom, maxRandom));
                action?.Invoke();
            }
        }

        public virtual bool ChangeState(NpcState newState)
        {
            if (behaviorStateMachine == null) return false;
            if (behaviorStateMachine.CurrentState == newState) return false;

            behaviorStateMachine.CurrentState?.Exit();
            behaviorStateMachine.ChangeState(newState);
            return true;
        }

        public override void Initialize()
        {
            base.Initialize();

            behaviorStateMachine.ChangeState(new CustomNPC_Wander(this));
        }

#pragma warning disable CS0108
        protected virtual void Update() => base.Update();
#pragma warning restore CS0108

        protected virtual void OnDestroy() => StopAllCoroutines();
    }

    public class CustomNPC_StateBase : NpcState
    {
        protected NPC character;

        public CustomNPC_StateBase(NPC chara) : base(chara)
        {
            character = chara;
        }
    }

    public class CustomNPC_Wander : CustomNPC_StateBase
    {
        public CustomNPC_Wander(NPC chara) : base(chara)
        {
        }

        public override void Enter()
        {
            base.Enter();
            if (!npc.Navigator.HasDestination)
            {
                ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
            }
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }
    }
}