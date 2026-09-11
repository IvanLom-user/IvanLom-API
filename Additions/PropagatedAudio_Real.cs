using UnityEngine;

namespace MyAPI.Additions 
{
    public class PropagatedAudio_Real : MonoBehaviour
    {
        public AudioSource audioDevice;
        public float maxDistance = 12f;
        public float minDistance = 1f;

        void FixedUpdate()
        {
            if (audioDevice == null) return;

            audioDevice.spatialBlend = 0;

            PlayerManager player = Singleton<CoreGameManager>.Instance.GetPlayer(0);
            if (player == null) return;

            float distance = Vector3.Distance(audioDevice.transform.position, player.transform.position);
            float volume = 1f - Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));

            audioDevice.volume = volume;
        }

        void OnDestroy()
        {
            if (audioDevice != null)
            {
                audioDevice.volume = 0f;
            }
            minDistance = 0f;
            maxDistance = 0f;
            audioDevice = null;
        }
    }
}
