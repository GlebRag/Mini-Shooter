using Assets.Scripts.Runtime.Combat.Effects.Config;
using UnityEngine;

namespace Runtime.Combat.Effects
{
    public class DamageOverTimeEffect : StatusEffect
    {
        private readonly DamageOverTimeEffectConfig _dotConfig;
        private readonly HealthComponent _healthComponent;
        private float _tickTimer;

        public DamageOverTimeEffect(DamageOverTimeEffectConfig config, GameObject target) : base(config, target)
        {
            _dotConfig = config;

            _healthComponent = target.GetComponentInChildren<HealthComponent>() ?? target.GetComponentInParent<HealthComponent>();

            if (_healthComponent == null)
            {
                Debug.LogError($"[Effects] На объекте {target.name} или его родителях не найден HealthComponent");
            }
        }

        public override void OnApply()
        {
            ApplyTickDamage();
        }

        protected override void OnTick(float deltaTime)
        {
            if (_healthComponent == null || _healthComponent.Health.IsDead) return;

            _tickTimer += deltaTime;
            if (_tickTimer >= _dotConfig.TickInterval)
            {
                _tickTimer -= _dotConfig.TickInterval;
                ApplyTickDamage();
            }
        }

        private void ApplyTickDamage()
        {
            if (_healthComponent != null && !_healthComponent.Health.IsDead)
            {
                _healthComponent.TakeDamage(_dotConfig.DamagePerTick);
            }
        }
    }
}