using System;
using System.Collections.Generic;
using Runtime.Weapons.Configs;

namespace Runtime.Weapons.Core
{
    public class WeaponInventory
    {
        public event Action<Weapon> OnWeaponChanged;

        private readonly List<Weapon> _weapons = new();
        private int _currentSlotIndex = -1;

        public Weapon ActiveWeapon => _currentSlotIndex >= 0 ? _weapons[_currentSlotIndex] : null;

        public void AddWeapon(WeaponConfig config)
        {
            _weapons.Add(new Weapon(config));

            // Автоматически выбираем первую добавленную пушку
            if (_currentSlotIndex == -1)
            {
                SelectSlot(0);
            }
        }

        public void SelectSlot(int index)
        {
            if (index < 0 || index >= _weapons.Count || index == _currentSlotIndex) return;

            _currentSlotIndex = index;
            OnWeaponChanged?.Invoke(ActiveWeapon);
        }
    }
}