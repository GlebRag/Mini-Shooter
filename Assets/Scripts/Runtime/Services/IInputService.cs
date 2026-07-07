using UnityEngine;

namespace Runtime.Services.Input
{
    public interface IInputService
    {
        Vector2 MoveAxis { get; }
        Vector2 LookAxis { get; }
        bool IsShootPressed { get; }
        bool IsShootClicked { get; }
        int SelectedWeaponSlot { get; }
        bool IsReloadPressed { get; }
    }
}