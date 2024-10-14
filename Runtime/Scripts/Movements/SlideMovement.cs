using UnityEngine;

namespace FPSController
{
    [RequireComponent(typeof(PlayerController))]
    public class SlideMovement : MonoBehaviour
    {
        #region Fields
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform orientation;

        

        #endregion

        #region MonoBehaviour
        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            if (rb == null)
                rb = GetComponent<Rigidbody>();

            if (orientation == null )
            {
                Debug.LogError("Missing Orientation :c");
                enabled = false;
            }
        }

        void Start()
        {
        
        }

        void Update()
        {
        
        }
        #endregion
    }
}
