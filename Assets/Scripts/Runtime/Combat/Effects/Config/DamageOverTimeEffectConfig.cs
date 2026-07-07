using Runtime.Combat.Effects;
using UnityEngine;

namespace Assets.Scripts.Runtime.Combat.Effects.Config
{
    [CreateAssetMenu(fileName = "DamageOverEffect", menuName = "Combat/Effects/DamageOver")]
    public class DamageOverTimeEffectConfig : StatusEffectConfig
    {
        [SerializeField] private float _damagePerTick;
        [SerializeField] private float _tickInterval;

        public float DamagePerTick => _damagePerTick;
        public float TickInterval => _tickInterval;

        public override StatusEffect CreateInstance(GameObject target)
        {
            return new DamageOverTimeEffect(this, target);
        }
    }
}