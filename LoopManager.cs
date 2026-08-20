using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyAPI
{
    public class LoopManager : MonoBehaviour
    {
        public GamePlugin plugin;
        public bool stop;

        public void Update()
        {
            if (!SceneManager.GetActiveScene().name.Contains("Game"))
            {
                plugin.ResetAudio();
            }

            if (plugin.LoopAudio == null) return;
            if (stop) return;

            plugin.LoopAudio.volume = Singleton<PlayerFileManager>.Instance.volume[2];
            if (plugin.currentAudioToLoop != null && !plugin.LoopAudio.isPlaying)
            {
                plugin.LoopAudio.clip = plugin.currentAudioToLoop.soundClip;
                plugin.LoopAudio.Play();
            }
            else if (plugin.currentAudioToLoop == null && plugin.LoopAudio.isPlaying)
            {
                plugin.LoopAudio.clip = null;
                plugin.LoopAudio.Stop();
            }
        }
    }
}