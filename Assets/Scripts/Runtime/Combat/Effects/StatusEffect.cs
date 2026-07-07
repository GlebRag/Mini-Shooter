using Assets.Scripts.Runtime.Combat.Effects.Config;
using UnityEngine;

namespace Runtime.Combat.Effects
{
    public abstract class StatusEffect
    {
        public StatusEffectConfig Config { get; }
        protected GameObject Target { get; }

        public float RemainingDuration { get; private set; }
        public bool IsFinished => RemainingDuration <= 0f;

        protected StatusEffect(StatusEffectConfig config, GameObject target)
        {
            Config = config;
            Target = target;
            RemainingDuration = config.Duration;
        }

        public virtual void OnApply() { }
        public virtual void OnRemove() { }

        public void Tick(float deltaTime)
        {
            RemainingDuration -= deltaTime;
            OnTick(deltaTime);
        }

        protected abstract void OnTick(float deltaTime);

        public void RefreshDuration()
        {
            RemainingDuration = Config.Duration;
        }
    }
}