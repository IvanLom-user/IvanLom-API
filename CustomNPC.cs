using MTM101BaldAPI;
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
        public GamePlugin plugin;
        private CoreGameManager cgm;
        public bool overrideSpeed;
        public float timerTime;
        public virtual float runSpeed => -1f;

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

        public CoreGameManager CGM
        {
            get
            {
                if (cgm == null)
                {
                    cgm = Singleton<CoreGameManager>.Instance;
                }
                return cgm;
            }
        }

        public IEnumerator SingleTimerCoroutine(float time, Action action)
        {
            WaitForSecondsNPCTimescale wait = new WaitForSecondsNPCTimescale(this, time);
            while (wait.keepWaiting)
            {
                yield return null;
            }
            action?.Invoke();
        }

        public IEnumerator TimerCoroutine(float time, Action action)
        {
            while (true)
            {
                WaitForSecondsNPCTimescale wait = new WaitForSecondsNPCTimescale(this, time);
                while (wait.keepWaiting)
                {
                    yield return null;
                }
                action?.Invoke();
            }
        }

        public IEnumerator TimerCoroutine(float minRandom, float maxRandom, Action action)
        {
            while (true)
            {
                WaitForSecondsNPCTimescale wait = new WaitForSecondsNPCTimescale(this, UnityEngine.Random.Range(minRandom, maxRandom));
                while (wait.keepWaiting)
                {
                    yield return null;
                }
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

            behaviorStateMachine.ChangeState(new CustomNPC_Wander(this, this));
        }

        public virtual void Alert(Vector3 pos)
        {
            navigationStateMachine.ChangeState(new NavigationState_TargetPosition(this, 0, pos));

            if (runSpeed == -1f) return;
            Navigator.SetSpeed(runSpeed);
        }

#pragma warning disable CS0108
        protected virtual void Update() => base.Update();
#pragma warning restore CS0108

        protected virtual void OnDestroy() => StopAllCoroutines();
    }

    public class CustomNPC_Wander : NPC_StateBase<CustomNPC>
    {
        public CustomNPC_Wander(NPC chara, CustomNPC custom) : base(chara, custom)
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