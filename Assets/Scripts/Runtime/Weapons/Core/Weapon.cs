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
            if (CurrentAmmo <= 0)
            {
                StartReload();
            }
        }

        public bool TryShoot(float currentTime, out int projectilesToSpawn)
        {
            projectilesToSpawn = 0;

            if (IsReloading || currentTime < _nextFireTime || CurrentAmmo <= 0) return false;

            CurrentAmmo--;
            _nextFireTime = currentTime + (1f / _config.FireRate);
            projectilesToSpawn = _config.ProjectilesPerShot;

            OnShotFired?.Invoke();

            if (CurrentAmmo <= 0)
            {
                StartReload();
            }

            return true;
        }
        public void CancelReload()
        {
            if (!IsReloading) return;

            IsReloading = false;
            _reloadTimer = 0f;

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