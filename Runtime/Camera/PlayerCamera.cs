using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSController
{
    /**
     * This script is responsible for handling the camera movement.
     * Place this Script on the Camera Root GameObject. Note that the Camera Root GameObject should be a child of the Player GameObject, and the Camera GameObject should NOT be a child of the Camera Root GameObject.
     * Instead, the Camera GameObject should be a child of a Camera Holder GameObject, with the CameraHolder Script.
     */
    public class PlayerCamera : MonoBehaviour
    {
        #region Fields
        [SerializeField] private InputReader inputReader;

        [SerializeField] private float sensX;
        [SerializeField] private float sensY;

        [SerializeField] private Transform orientation;

        float _xRotation, _yRotation;
        #endregion

        #region MonoBehaviour
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (inputReader == null) Debug.LogError("InputReader is not assigned in PlayerCamera");

            inputReader.Look += HandleLook;
        }

        #endregion

        #region Private Methods

        private void HandleLook(Vector2 look, bool isMouse)
        {
            if (!isMouse) Debug.LogError("Controller input not implemented yet");

            _xRotation -= look.y * sensY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            _yRotation += look.x * sensX;

            transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
            orientation.rotation = Quaternion.Euler(0f, _yRotation, 0f);
        }

        #endregion
    }
}
