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
        [SerializeField] private Camera _mainCamera; // Ссылка на главную камеру для расчета прицела
        [SerializeField] private Transform _firePoint;
        [SerializeField] private WeaponConfig[] _initialWeapons;

        private IInputService _inputService;
        private WeaponInventory _inventory;
        private ProjectileFactory _projectileFactory;

        // Экспонируем инвентарь наружу, чтобы корень композиции мог передать его в UI
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
                // 1. Старой пушке отменяем перезарядку
                _inventory.ActiveWeapon?.CancelReload();

                // 2. Переключаем слот на новую пушку
                _inventory.SelectSlot(slotInput);

                // 3. НОВАЯ СТРОЧКА: Говорим новой пушке, что её достали.
                // Если она пустая, она сама мгновенно включит перезарядку!
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
            // 1. Пускаем луч из центра экрана (координаты 0.5, 0.5)
            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 targetPoint;

            // Кидаем луч на максимальную дальность оружия
            if (Physics.Raycast(ray, out RaycastHit hit, weapon.Config.Range))
            {
                targetPoint = hit.point; // Точка физического столкновения с препятствием/врагом
            }
            else
            {
                targetPoint = ray.GetPoint(weapon.Config.Range); // Точка в воздухе на максимальной дистанции
            }

            // 2. Рассчитываем базовое направление от дула оружия до точки прицела
            Vector3 fireDirection = (targetPoint - _firePoint.position).normalized;
            Quaternion baseRotation = Quaternion.LookRotation(fireDirection);

            // 3. Спавним пули с учетом разброса относительно этого направления
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