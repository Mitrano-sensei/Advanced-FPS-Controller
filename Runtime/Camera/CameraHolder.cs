using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSController
{
    public class CameraHolder : MonoBehaviour
    {
        [SerializeField] private Transform cameraRoot;

        void Update()
        {
            transform.position = cameraRoot.position;
            transform.rotation = cameraRoot.rotation;
        }
    }
}
