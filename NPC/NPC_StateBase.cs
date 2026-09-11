using UnityEngine;

namespace MyAPI
{
    public class NPC_StateBase<T> : NpcState where T : NPC
    {
        public T mainNpc;

        public NPC_StateBase(NPC chara, T main) : base(chara)
        {
            npc = chara;
            mainNpc = main;
        }
    }
}