using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Services.Input
{
    public class InputSystemInputService : IInputService, System.IDisposable
    {
        private readonly GameInput _gameInput;

        public Vector2 MoveAxis => _gameInput.Player.Move.ReadValue<Vector2>();
        public Vector2 LookAxis => _gameInput.Player.Look.ReadValue<Vector2>();

        // Проверка: кнопка удерживается (подходит для автомата)
        public bool IsShootPressed => Mouse.current != null && Mouse.current.leftButton.isPressed;
        public bool IsReloadPressed => _gameInput.Player.Reload.triggered;
        // Проверка: кнопка только что нажата (подходит для пистолета/дробовика)
        public bool IsShootClicked => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        public int SelectedWeaponSlot
        {
            get
            {
                if (Keyboard.current == null) return -1;

                if (Keyboard.current.digit1Key.wasPressedThisFrame) return 0; // Пистолет
                if (Keyboard.current.digit2Key.wasPressedThisFrame) return 1; // Дробовик
                if (Keyboard.current.digit3Key.wasPressedThisFrame) return 2; // Автомат

                return -1;
            }
        }

        public InputSystemInputService()
        {
            _gameInput = new GameInput();
            _gameInput.Player.Enable();
        }

        public void Dispose()
        {
            _gameInput.Player.Disable();
            _gameInput.Dispose();
        }
    }
}