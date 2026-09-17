using UnityEngine;

namespace MyAPI.Additions
{
    public class ShakingBillboard : MonoBehaviour
    {
        private Transform camTransform;
        private float shakeTimer = 0f;
        private Quaternion currentShakeRotation = Quaternion.identity;
        private const float SHAKE_INTERVAL = 1f / 100f;

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            if (camTransform == null)
            {
                camTransform = Singleton<CoreGameManager>.Instance.GetCamera(0)?.transform;
                return;
            }

            shakeTimer += Time.deltaTime;

            while (shakeTimer >= SHAKE_INTERVAL)
            {
                shakeTimer -= SHAKE_INTERVAL;
                currentShakeRotation = GetRandomShakeRotation();
            }

            transform.localRotation = camTransform.rotation * currentShakeRotation;
        }

        private Quaternion GetRandomShakeRotation()
        {
            Vector3 randomEuler = Vector3.zero;
            randomEuler.x = Random.Range(-36, 36);
            randomEuler.y = Random.Range(-36, 36);
            randomEuler.z = Random.Range(-36, 36);

            return Quaternion.Euler(randomEuler);
        }
    }
}