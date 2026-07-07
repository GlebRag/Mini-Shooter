using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
        [SerializeField] private TMP_Text _reloadTimerText;

        [Header("Effects & Death Panel")]
        [SerializeField] private Image _damageVignette; 
        [SerializeField] private GameObject _deathPanel;
        [SerializeField] private Button _restartButton;

        [Header("Visual Settings")]
        [SerializeField] private Color _readyColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color _reloadColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);

        private Health _health;
        private WeaponInventory _inventory;
        private Weapon _currentActiveWeapon;
        private float _lastHealth;

        public void Initialize(Health health, WeaponInventory inventory)
        {
            _health = health;
            _inventory = inventory;
            _lastHealth = _health.Current;

            _health.OnChanged += UpdateHealthUI;
            _health.OnDied += HandlePlayerDeath;
            UpdateHealthUI(_health.Current, _health.Max);

            _inventory.OnWeaponChanged += HandleWeaponChanged;

            if (_reloadTimerText != null) _reloadTimerText.gameObject.SetActive(false);
            if (_deathPanel != null) _deathPanel.SetActive(false);
            if (_damageVignette != null) _damageVignette.color = new Color(1f, 0f, 0f, 0f);

            if (_restartButton != null) _restartButton.onClick.AddListener(RestartLevel);

            if (_inventory.ActiveWeapon != null)
            {
                HandleWeaponChanged(_inventory.ActiveWeapon);
            }
        }

        private void Update()
        {
            if (_currentActiveWeapon != null && _currentActiveWeapon.IsReloading)
            {
                if (!_reloadTimerText.gameObject.activeSelf) _reloadTimerText.gameObject.SetActive(true);
                _reloadTimerText.text = $"{_currentActiveWeapon.RemainingReloadTime:F1}s";
            }

            if (_damageVignette != null && _damageVignette.color.a > 0f)
            {
                Color c = _damageVignette.color;
                c.a -= Time.deltaTime * 2.5f; 
                _damageVignette.color = c;
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnChanged -= UpdateHealthUI;
                _health.OnDied -= HandlePlayerDeath;
            }
            if (_inventory != null) _inventory.OnWeaponChanged -= HandleWeaponChanged;
            if (_restartButton != null) _restartButton.onClick.RemoveListener(RestartLevel);
            UnsubscribeFromCurrentWeapon();
        }

        private void UpdateHealthUI(float current, float max)
        {
            _healthText.text = $"HP: {Mathf.CeilToInt(current)} / {max}";

            if (current < _lastHealth && current > 0)
            {
                if (_damageVignette != null) _damageVignette.color = new Color(1f, 0f, 0f, 0.4f);
            }
            _lastHealth = current;
        }

        private void HandlePlayerDeath()
        {
            _healthText.text = "ВЫ ПОГИБЛИ";
            _healthText.color = Color.red;

            if (_damageVignette != null) _damageVignette.color = new Color(0.6f, 0f, 0f, 0.6f);


            if (_deathPanel != null) _deathPanel.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Time.timeScale = 0f;
        }

        private void RestartLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            _reloadTimerText.gameObject.SetActive(true);
        }

        private void HandleReloadFinished()
        {
            _weaponPanelBackground.color = _readyColor;
            _reloadTimerText.gameObject.SetActive(false);
            UpdateAmmoUI();
        }
    }
}