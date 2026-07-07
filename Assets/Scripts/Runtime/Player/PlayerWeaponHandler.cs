using Runtime.Combat.Pool;
using Runtime.Services.Input;
using Runtime.Weapons.Configs;
using Runtime.Weapons.Core;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace Runtime.Player
{
    public class PlayerWeaponHandler : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private Camera _mainCamera; 
        [SerializeField] private Transform _firePoint;
        [SerializeField] private WeaponConfig[] _initialWeapons;

        private IInputService _inputService;
        private WeaponInventory _inventory;
        private ProjectileFactory _projectileFactory;

        public WeaponInventory Inventory => _inventory;

        public void Construct(IInputService inputService, ProjectileFactory projectileFactory)
        {
            _inputService = inputService;
            _projectileFactory = projectileFactory;

            _inventory = new WeaponInventory();

            foreach (var config in _initialWeapons)
            {
                if (config != null) _inventory.AddWeapon(config);
            }
        }

        private void Update()
        {
            if (_inputService == null || _inventory.ActiveWeapon == null) return;

            Weapon activeWeapon = _inventory.ActiveWeapon;
            activeWeapon.Tick(Time.deltaTime, Time.time);

            int slotInput = _inputService.SelectedWeaponSlot;
            if (slotInput != -1)
            {
                _inventory.ActiveWeapon?.CancelReload();

                _inventory.SelectSlot(slotInput);

                _inventory.ActiveWeapon?.OnEquip();
            }

            if (_inputService.IsReloadPressed)
            {
                activeWeapon.StartReload();
            }

            bool wantsToShoot = activeWeapon.Config.IsAutomatic
                ? _inputService.IsShootPressed
                : _inputService.IsShootClicked;

            if (wantsToShoot && activeWeapon.TryShoot(Time.time, out int count))
            {
                SpawnProjectiles(activeWeapon, count);
            }
        }

        private void SpawnProjectiles(Weapon weapon, int count)
        {

            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, weapon.Config.Range))
            {
                targetPoint = hit.point; 
            }
            else
            {
                targetPoint = ray.GetPoint(weapon.Config.Range); 
            }

            Vector3 fireDirection = (targetPoint - _firePoint.position).normalized;
            Quaternion baseRotation = Quaternion.LookRotation(fireDirection);

            for (int i = 0; i < count; i++)
            {
                Quaternion spreadRotation = Quaternion.Euler(
                    Random.Range(-weapon.Config.Spread, weapon.Config.Spread),
                    Random.Range(-weapon.Config.Spread, weapon.Config.Spread),
                    0f
                );

                Quaternion finalRotation = baseRotation * spreadRotation;

                _projectileFactory.Spawn(_firePoint.position, finalRotation, weapon.Config.Damage, weapon.Config.Range);
            }
        }
    }
}