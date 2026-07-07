using Runtime.Combat; // Добавили
using Runtime.Combat.Pool;
using Runtime.Enemy;
using Runtime.Player;
using Runtime.Services.Input;
using Runtime.UI;     // Добавили
using UnityEngine;

namespace Runtime.Infrastructure
{
    public class LevelInitializer : MonoBehaviour
    {
        [Header("Global Assets")]
        [SerializeField] private GameObject _projectilePrefab;

        [Header("UI References")]
        [SerializeField] private PlayerHUD _playerHUD; // Ссылка на наш HUD на Canvas

        [Header("Player References")]
        [SerializeField] private HealthComponent _playerHealthComponent; // Нужна ссылка на компонент здоровья игрока
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerLook _playerLook;
        [SerializeField] private PlayerWeaponHandler _playerWeaponHandler;

        private InputSystemInputService _inputService;
        private ProjectileFactory _projectileFactory;

        private void Awake()
        {
            _inputService = new InputSystemInputService();
            _projectileFactory = new ProjectileFactory(_projectilePrefab, initialCapacity: 50, maxCapacity: 150);

            _playerMovement.Construct(_inputService);
            _playerLook.Construct(_inputService);
            _playerWeaponHandler.Construct(_inputService, _projectileFactory);

            _playerHUD.Initialize(_playerHealthComponent.Health, _playerWeaponHandler.Inventory);

            //Плохо, но быстро
            EnemyAI[] enemiesOnScene = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            foreach (var enemy in enemiesOnScene)
            {
                enemy.Construct(_playerMovement.transform, _projectileFactory);
            }
        }

        private void OnDestroy()
        {
            _inputService?.Dispose();
        }
    }
}