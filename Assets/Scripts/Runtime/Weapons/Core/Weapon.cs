using System;
using Runtime.Weapons.Configs;

namespace Runtime.Weapons.Core
{
    public class Weapon
    {
        public event Action OnShotFired;
        public event Action OnReloadStarted;
        public event Action OnReloadFinished;

        private readonly WeaponConfig _config;
        private float _nextFireTime;
        private float _reloadTimer;

        public WeaponConfig Config => _config;
        public int CurrentAmmo { get; private set; }
        public bool IsReloading { get; private set; }

        public float RemainingReloadTime => IsReloading ? (_reloadTimer > 0f ? _reloadTimer : 0f) : 0f;

        public Weapon(WeaponConfig config)
        {
            _config = config;
            CurrentAmmo = config.MagazineSize;
        }

        public void Tick(float deltaTime, float currentTime)
        {
            if (!IsReloading) return;

            _reloadTimer -= deltaTime;
            if (_reloadTimer <= 0)
            {
                CompleteReload();
            }
        }
        public void OnEquip()
        {
            // Если оружие достали, а в магазине круглый ноль — 
            // принудительно и автоматически запускаем перезарядку заново
            if (CurrentAmmo <= 0)
            {
                StartReload();
            }
        }

        public bool TryShoot(float currentTime, out int projectilesToSpawn)
        {
            projectilesToSpawn = 0;

            // ИСПРАВЛЕНО: Теперь просто выходим, если стрелять нельзя (идёт перезарядка, КД между выстрелами или патроны на нуле)
            if (IsReloading || currentTime < _nextFireTime || CurrentAmmo <= 0) return false;

            // Производим выстрел
            CurrentAmmo--;
            _nextFireTime = currentTime + (1f / _config.FireRate);
            projectilesToSpawn = _config.ProjectilesPerShot;

            OnShotFired?.Invoke();

            // ИСПРАВЛЕНО: Автоматически запускаем перезарядку сразу же, как только ушёл последний патрон!
            if (CurrentAmmo <= 0)
            {
                StartReload();
            }

            return true;
        }
        public void CancelReload()
        {
            // Если оружие и так не перезаряжается — ничего не делаем
            if (!IsReloading) return;

            IsReloading = false;
            _reloadTimer = 0f;

            // Оповещаем UI, чтобы он убрал красный фон и таймер, 
            // но патроны останутся пустыми, так как CompleteReload() не вызывался
            OnReloadFinished?.Invoke();
        }

        public void StartReload()
        {
            if (IsReloading || CurrentAmmo == _config.MagazineSize) return;

            IsReloading = true;
            _reloadTimer = _config.ReloadTime;
            OnReloadStarted?.Invoke();
        }

        private void CompleteReload()
        {
            IsReloading = false;
            CurrentAmmo = _config.MagazineSize;
            OnReloadFinished?.Invoke();
        }
    }
}