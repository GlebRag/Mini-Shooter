using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Services.Input
{
    public class InputSystemInputService : IInputService, System.IDisposable
    {
        private readonly GameInput _gameInput;

        public Vector2 MoveAxis => _gameInput.Player.Move.ReadValue<Vector2>();
        public Vector2 LookAxis => _gameInput.Player.Look.ReadValue<Vector2>();

        public bool IsShootPressed => Mouse.current != null && Mouse.current.leftButton.isPressed;
        public bool IsReloadPressed => _gameInput.Player.Reload.triggered;
        public bool IsShootClicked => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        public int SelectedWeaponSlot
        {
            get
            {
                if (Keyboard.current == null) return -1;

                for (int i = 0; i < 9; i++)
                {
                    Key targetKey = (Key)((int)Key.Digit1 + i);

                    if (Keyboard.current[targetKey].wasPressedThisFrame)
                        return i;
                }

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