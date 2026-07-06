using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Runtime.Combat;
using Runtime.Weapons.Core;

namespace Runtime.UI
{
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Health UI")]
        [SerializeField] private TMP_Text _healthText;

        [Header("Weapon UI")]
        [SerializeField] private Image _weaponPanelBackground;
        [SerializeField] private TMP_Text _weaponNameText;
        [SerializeField] private TMP_Text _ammoText;
        [SerializeField] private TMP_Text _reloadTimerText; // Ссылка на наш новый текст таймера

        [Header("Visual Settings")]
        [SerializeField] private Color _readyColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color _reloadColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);

        private Health _health;
        private WeaponInventory _inventory;
        private Weapon _currentActiveWeapon;

        public void Initialize(Health health, WeaponInventory inventory)
        {
            _health = health;
            _inventory = inventory;

            _health.OnChanged += UpdateHealthUI;
            _health.OnDied += HandlePlayerDeath;
            UpdateHealthUI(_health.Current, _health.Max);

            _inventory.OnWeaponChanged += HandleWeaponChanged;

            // Сразу прячем таймер при старте игры
            if (_reloadTimerText != null) _reloadTimerText.gameObject.SetActive(false);

            if (_inventory.ActiveWeapon != null)
            {
                HandleWeaponChanged(_inventory.ActiveWeapon);
            }
        }

        private void Update()
        {
            // Каждого кадра проверяем: если пушка сейчас перезаряжается — обновляем тикающие цифры
            if (_currentActiveWeapon != null && _currentActiveWeapon.IsReloading)
            {
                // Включаем текст, если он был выключен (например, при переключении на уже перезаряжаемую пушку)
                if (!_reloadTimerText.gameObject.activeSelf)
                {
                    _reloadTimerText.gameObject.SetActive(true);
                }

                // Берём оставшееся время из пушки и форматируем до одного знака после запятой (например, 2.4s)
                _reloadTimerText.text = $"{_currentActiveWeapon.RemainingReloadTime:F1}s";
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnChanged -= UpdateHealthUI;
                _health.OnDied -= HandlePlayerDeath;
            }

            if (_inventory != null)
            {
                _inventory.OnWeaponChanged -= HandleWeaponChanged;
            }

            UnsubscribeFromCurrentWeapon();
        }

        private void UpdateHealthUI(float current, float max)
        {
            _healthText.text = $"HP: {Mathf.CeilToInt(current)} / {max}";
        }

        private void HandlePlayerDeath()
        {
            _healthText.text = "ВЫ ПОГИБЛИ";
            _healthText.color = Color.red;
        }

        private void HandleWeaponChanged(Weapon newWeapon)
        {
            UnsubscribeFromCurrentWeapon();
            _currentActiveWeapon = newWeapon;

            if (_currentActiveWeapon != null)
            {
                _currentActiveWeapon.OnShotFired += UpdateAmmoUI;
                _currentActiveWeapon.OnReloadStarted += HandleReloadStarted;
                _currentActiveWeapon.OnReloadFinished += HandleReloadFinished;

                _weaponNameText.text = _currentActiveWeapon.Config.WeaponName;
                _weaponPanelBackground.color = _currentActiveWeapon.IsReloading ? _reloadColor : _readyColor;

                // Проверяем состояние таймера при смене оружия
                _reloadTimerText.gameObject.SetActive(_currentActiveWeapon.IsReloading);

                UpdateAmmoUI();
            }
        }

        private void UnsubscribeFromCurrentWeapon()
        {
            if (_currentActiveWeapon == null) return;

            _currentActiveWeapon.OnShotFired -= UpdateAmmoUI;
            _currentActiveWeapon.OnReloadStarted -= HandleReloadStarted;
            _currentActiveWeapon.OnReloadFinished -= HandleReloadFinished;
        }

        private void UpdateAmmoUI()
        {
            if (_currentActiveWeapon == null) return;
            _ammoText.text = $"{_currentActiveWeapon.CurrentAmmo} / {_currentActiveWeapon.Config.MagazineSize}";
        }

        private void HandleReloadStarted()
        {
            _weaponPanelBackground.color = _reloadColor;
            _reloadTimerText.gameObject.SetActive(true); // Показываем текст обратного отсчета
        }

        private void HandleReloadFinished()
        {
            _weaponPanelBackground.color = _readyColor;
            _reloadTimerText.gameObject.SetActive(false); // Прячем текст, когда перезарядка окончена
            UpdateAmmoUI();
        }
    }
}