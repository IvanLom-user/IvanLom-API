using UnityEngine;

namespace MyAPI.Additions
{
    public class ShakingBillboard : MonoBehaviour
    {
        private Transform camTransform;

        private void OnWillRenderObject()
        {
            if (camTransform == null)
            {
                camTransform = Singleton<CoreGameManager>.Instance.GetCamera(0)?.transform;
            }
            transform.localRotation = camTransform.rotation * GetRandomShakeRotation();
        }

        private Quaternion GetRandomShakeRotation()
        {
            Vector3 randomEuler = Vector3.zero;
            randomEuler.x = Random.Range(-48f, 48f);
            randomEuler.y = Random.Range(-48f, 48f);
            randomEuler.z = Random.Range(-48f, 48f);

            return Quaternion.Euler(randomEuler);
        }
    }
}