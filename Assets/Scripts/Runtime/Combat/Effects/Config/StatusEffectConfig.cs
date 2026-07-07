using UnityEngine;

namespace Runtime.Combat.Effects
{
    public abstract class StatusEffectConfig : ScriptableObject
    {
        [Header("Base Effect Settings")]
        [SerializeField] private string _effectName;
        [SerializeField] private float _duration;
        [SerializeField] private Sprite _icon;

        public string EffectName => _effectName;
        public float Duration => _duration;
        public Sprite Icon => _icon;

        public abstract StatusEffect CreateInstance(GameObject target);
    }
}