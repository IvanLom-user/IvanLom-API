using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyAPI
{
    public class LoopManager : MonoBehaviour
    {
        public GamePlugin plugin;
        public bool stop;
        private bool transitioning;

        public void Update()
        {
            if (!SceneManager.GetActiveScene().name.Contains("Game"))
            {
                plugin.ResetAudio();
            }

            if (plugin.LoopAudio == null) return;
            if (stop) return;

            if (!transitioning)
            {
                plugin.LoopAudio.volume = Singleton<PlayerFileManager>.Instance.volume[2] * 2f;
                if (plugin.currentAudioToLoop != null && !plugin.LoopAudio.isPlaying)
                {
                    plugin.LoopAudio.clip = plugin.currentAudioToLoop.soundClip;
                    plugin.LoopAudio.Play();
                }
                else if (plugin.currentAudioToLoop == null && plugin.LoopAudio.isPlaying)
                {
                    transitioning = true;
                    StartCoroutine(FadeOut());
                }
            }
        }

        private IEnumerator FadeOut()
        {
            transitioning = true;
            while (plugin.LoopAudio.volume > 0f)
            {
                plugin.LoopAudio.volume -= Time.deltaTime * 1.25f;
                yield return null;
            }
            plugin.LoopAudio.clip = null;
            plugin.LoopAudio.Stop();
            transitioning = false;
        }
    }
}