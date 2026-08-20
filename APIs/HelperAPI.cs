using HarmonyLib;
using TMPro;
using UnityEngine;

namespace MyAPI
{
    /// <summary>
    /// The API with helper methods.
    /// </summary>
    public static class HelperAPI
    {
        public static void SetAudioMan(ref AudioManager aud, bool _2D, bool loop = false)
        {
            GameObject newAudioMan = new GameObject("New_AudioManager");
            Object.DontDestroyOnLoad(newAudioMan);
            aud = newAudioMan.GetComponent<AudioManager>() ?? newAudioMan.AddComponent<AudioManager>();
            aud.audioDevice = newAudioMan.GetComponent<AudioSource>() ?? newAudioMan.AddComponent<AudioSource>();
            aud.SetLoop(loop);

            if (_2D)
            {
                aud.audioDevice.spatialBlend = 0f;
                aud.audioDevice.maxDistance = 999f;
                aud.positional = false;
            }
        }

        public static void SetAudioMan(ref AudioSource aud, bool _2D, bool loop = false)
        {
            GameObject newAudioMan = new GameObject("New_AudioSource");
            Object.DontDestroyOnLoad(newAudioMan);
            aud = newAudioMan.GetComponent<AudioSource>() ?? newAudioMan.AddComponent<AudioSource>();
            aud.loop = loop;

            if (_2D)
            {
                aud.spatialBlend = 0f;
                aud.maxDistance = 999f;
            }
        }

        public static void SetText(ref TextMeshProUGUI text, string textName, Transform hudTransform)
        {
            text = new GameObject(textName).AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(hudTransform, false);
            text.rectTransform.sizeDelta = new Vector2(500f, 80f);
            text.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            text.rectTransform.localPosition = Vector3.zero;
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.localPosition = Vector3.zero;
            text.gameObject.SetActive(true);
        }

        public static void SetValue<T>(object instance, string fieldName, T setVal)
        {
            Traverse.Create(instance).Field(fieldName).SetValue(setVal);
        }

        public static void SetPosAndRotation(this Transform transform, Vector3 pos, Quaternion rotation, bool local)
        {
            if (local)
            {
                transform.localPosition = pos;
                transform.localRotation = rotation;
                return;
            }
            transform.SetPositionAndRotation(pos, rotation);
        }

        public static void SetPosAndRotation(this Transform transform, Vector3 pos, Quaternion rotation) => transform.SetPositionAndRotation(pos, rotation);

        public static void ToCenter(this RectTransform rect)
        {
            rect.anchoredPosition = Vector3.zero;
            rect.anchorMin = Vector2.one * 0.5f;
            rect.anchorMax = Vector2.one * 0.5f;
        }

        public static void ToCenter(this UnityEngine.UI.Image image) => image.rectTransform.ToCenter();
    }
}