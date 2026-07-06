using UnityEngine;

namespace Runtime.Combat
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("Settings")]
        [SerializeField] private float _maxHealth = 100f;

        [Header("Events (Optional)")]
        [SerializeField] private bool _destroyOnDeath = true;

        // Предоставляем доступ к чистой логике для других систем (например, для UI полоски здоровья)
        public Health Health { get; private set; }

        private void Awake()
        {
            // Инициализируем чистую C#-логику здоровья
            Health = new Health(_maxHealth);
        }

        private void OnEnable()
        {
            Health.OnDied += HandleDeath;
        }

        private void OnDisable()
        {
            Health.OnDied -= HandleDeath;
        }

        // Реализация интерфейса IDamageable
        public void TakeDamage(float damage)
        {
            Health.TakeDamage(damage);
        }

        private void HandleDeath()
        {
            Debug.Log($"[{gameObject.name}] погиб.");

            if (_destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}