using UnityEngine;

namespace FPSController
{
    public class CinemachinePlayerCamera : MonoBehaviour
    {
        #region Fields
        [SerializeField] private Transform orientation;

        #endregion    
    
        #region MonoBehaviour
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void FixedUpdate()
        {
            HandleLook();
        }

        #endregion

        private void HandleLook()
        {
            orientation.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        }
    }
}
