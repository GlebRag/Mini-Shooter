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

        // Предохранитель от двойного возврата в пул
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

        public void Initialize(float damage, float range, Action<Projectile> onRelease)
        {
            _damage = damage;
            _maxRange = range;
            _onRelease = onRelease;
            _startPosition = transform.position;

            // Сбрасываем флаг, когда снаряд достают из пула для нового выстрела
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
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(_damage);
            }

            Release();
        }

        private void Release()
        {
            // Если снаряд УЖЕ был возвращен в пул в этом кадре — игнорируем повторный вызов
            if (_isReleased) return;
            _isReleased = true;

            _rigidbody.linearVelocity = Vector3.zero;
            _onRelease?.Invoke(this);
        }
    }
}