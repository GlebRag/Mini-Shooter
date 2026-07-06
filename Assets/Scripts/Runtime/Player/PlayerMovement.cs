using UnityEngine;
using Runtime.Services.Input;

namespace Runtime.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _gravity = -9.81f;

        private CharacterController _characterController;
        private IInputService _inputService;
        private Vector3 _verticalVelocity;

        private void Awake() => _characterController = GetComponent<CharacterController>();

        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }

        private void Update()
        {
            if (_inputService == null) return;

            Move();
            ApplyGravity();
        }

        private void Move()
        {
            Vector2 input = _inputService.MoveAxis;

            Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;

            _characterController.Move(moveDirection * _moveSpeed * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (_characterController.isGrounded && _verticalVelocity.y < 0)
            {
                _verticalVelocity.y = -2f;
            }

            _verticalVelocity.y += _gravity * Time.deltaTime;
            _characterController.Move(_verticalVelocity * Time.deltaTime);
        }
    }
}