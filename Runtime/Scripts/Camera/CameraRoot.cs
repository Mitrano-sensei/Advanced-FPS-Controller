using UnityEngine;

namespace FPSController
{
    public class CameraRoot : MonoBehaviour
    {
        #region Fields

        [Header("References")]
        [SerializeField] private CapsuleCollider playerBody;

        private Transform _tr;

        [Header("Settings")] 
        [SerializeField] private float eyeRatio = .7f; // The ratio of the eyes height compared to the body height 
        #endregion    
    
        #region MonoBehaviour
        void Start()
        {
            _tr = transform;
        }

        void Update()
        {
            _tr.position = playerBody.transform.position + Vector3.up * (playerBody.height * eyeRatio);
        }
        #endregion
    }
}
