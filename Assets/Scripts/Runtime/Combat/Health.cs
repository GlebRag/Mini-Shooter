using System;
using UnityEngine;

namespace Runtime.Combat
{
    public class Health
    {
        // Передаем текущее и максимальное здоровье для удобного обновления UI
        public event Action<float, float> OnChanged;
        public event Action OnDied;

        public float Current { get; private set; }
        public float Max { get; private set; }
        public bool IsDead => Current <= 0;

        public Health(float maxHealth)
        {
            if (maxHealth <= 0)
            {
                Debug.LogError("Максимальное здоровье должно быть больше нуля!");
                maxHealth = 100f;
            }

            Max = maxHealth;
            Current = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            if (IsDead || damage <= 0) return;

            Current -= damage;

            if (Current < 0)
                Current = 0;

            OnChanged?.Invoke(Current, Max);

            if (IsDead)
            {
                OnDied?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0) return;

            Current += amount;

            if (Current > Max)
                Current = Max;

            OnChanged?.Invoke(Current, Max);
        }
    }
}