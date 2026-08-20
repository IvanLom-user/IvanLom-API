using System.Collections.Generic;
using UnityEngine;

namespace MyAPI.Data
{
    public class AnimationData
    {
        public List<Sprite> spriteData;
        public WaitForSeconds delay;
        public bool reverse;
        public bool loop;

        public AnimationData(List<Sprite> spriteData, float delay = 0.1f, bool reverse = false, bool loop = false)
        {
            this.spriteData = spriteData;
            this.delay = delay > 0 ? new WaitForSeconds(delay) : null;
            this.reverse = reverse;
            this.loop = loop;
        }
    }
}