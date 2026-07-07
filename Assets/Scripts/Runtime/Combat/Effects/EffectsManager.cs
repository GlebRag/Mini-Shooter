using Assets.Scripts.Runtime.Combat.Effects.Config;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Combat.Effects
{
    public class EffectsManager : MonoBehaviour
    {
        private readonly List<StatusEffect> _activeEffects = new List<StatusEffect>();

        public event Action<StatusEffect> OnEffectAdded;
        public event Action<StatusEffect> OnEffectRemoved;

        public IReadOnlyList<StatusEffect> ActiveEffects => _activeEffects;

        public void ApplyEffect(StatusEffectConfig config)
        {
            if (config == null) return;

            StatusEffect existingEffect = _activeEffects.Find(e => e.Config == config);

            if (existingEffect != null)
            {
                existingEffect.RefreshDuration();
            }
            else
            {
                StatusEffect newEffect = config.CreateInstance(gameObject);
                _activeEffects.Add(newEffect);

                newEffect.OnApply();
                OnEffectAdded?.Invoke(newEffect);
            }
        }

        private void Update()
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = _activeEffects[i];
                effect.Tick(Time.deltaTime);

                if (effect.IsFinished)
                {
                    effect.OnRemove();
                    OnEffectRemoved?.Invoke(effect);
                    _activeEffects.RemoveAt(i);
                }
            }
        }
    }
}