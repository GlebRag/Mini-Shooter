using Assets.Scripts.Runtime.Combat.Effects.Config;
using Runtime.Combat.Effects;
using System;
using UnityEngine;

namespace Runtime.Combat
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _speed = 20f;

        private float _damage;
        private float _maxRange;
        private Vector3 _startPosition;
        private Rigidbody _rigidbody;
        private Action<Projectile> _onRelease;

        private StatusEffectConfig[] _effectsOnHit;

        private bool _isReleased;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;

            if (TryGetComponent<Collider>(out var entityCollider))
            {
                entityCollider.isTrigger = true;
            }
        }

        public void Initialize(float damage, float range, StatusEffectConfig[] effectsOnHit, Action<Projectile> onRelease)
        {
            _damage = damage;
            _maxRange = range;
            _effectsOnHit = effectsOnHit;
            _onRelease = onRelease;
            _startPosition = transform.position;

            _isReleased = false;

            _rigidbody.linearVelocity = transform.forward * _speed;
        }

        private void Update()
        {
            if (Vector3.Distance(_startPosition, transform.position) >= _maxRange)
            {
                Release();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isReleased) return;


            if (other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(_damage);
            }

            if (other.TryGetComponent(out EffectsManager effectsManager))
            {
                if (_effectsOnHit != null)
                {
                    foreach (StatusEffectConfig effectConfig in _effectsOnHit)
                    {
                        if (effectConfig != null)
                        {
                            effectsManager.ApplyEffect(effectConfig);
                        }
                    }
                }
            }

            Release();
        }

        private void Release()
        {
            if (_isReleased) return;
            _isReleased = true;

            _rigidbody.linearVelocity = Vector3.zero;
            _onRelease?.Invoke(this);
        }
    }
}