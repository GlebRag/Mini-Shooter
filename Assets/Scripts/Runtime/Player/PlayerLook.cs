using UnityEngine;
using Runtime.Services.Input;

namespace Runtime.Player
{
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _sensitivity = 1.5f;
        [SerializeField] private float _minVerticalAngle = -85f;
        [SerializeField] private float _maxVerticalAngle = 85f;

        private IInputService _inputService;
        private float _verticalRotation;

        public void Construct(IInputService inputService)
        {
            _inputService = inputService;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            if (_inputService == null) return;

            Vector2 lookInput = _inputService.LookAxis * _sensitivity;

            _verticalRotation -= lookInput.y;
            _verticalRotation = Mathf.Clamp(_verticalRotation, _minVerticalAngle, _maxVerticalAngle);
            _cameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);

            transform.Rotate(Vector3.up * lookInput.x);
        }
    }
}