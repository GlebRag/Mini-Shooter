using UnityEngine;
using UnityEngine.Pool;

namespace Runtime.Combat.Pool
{
    public class ProjectileFactory
    {
        private readonly GameObject _prefab;
        private readonly Transform _poolRoot;
        private readonly ObjectPool<Projectile> _pool;

        public ProjectileFactory(GameObject prefab, int initialCapacity = 50, int maxCapacity = 100)
        {
            _prefab = prefab;

            _poolRoot = new GameObject("[Projectile Pool]").transform;

            _pool = new ObjectPool<Projectile>(
                createFunc: CreateInstance,
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: true,
                defaultCapacity: initialCapacity,
                maxSize: maxCapacity
            );

            PreWarm(initialCapacity);
        }

        public void Spawn(Vector3 position, Quaternion rotation, float damage, float range)
        {
            Projectile projectile = _pool.Get();

            projectile.transform.SetPositionAndRotation(position, rotation);
            projectile.Initialize(damage, range, ReturnToPool);
        }

        private void ReturnToPool(Projectile projectile)
        {
            _pool.Release(projectile);
        }


        private Projectile CreateInstance()
        {
            GameObject go = Object.Instantiate(_prefab, _poolRoot);
            return go.GetComponent<Projectile>();
        }

        private void OnGetFromPool(Projectile projectile)
        {
            projectile.gameObject.SetActive(true);
        }

        private void OnReleaseToPool(Projectile projectile)
        {
            projectile.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(Projectile projectile)
        {
            if (projectile != null)
            {
                Object.Destroy(projectile.gameObject);
            }
        }

        private void PreWarm(int count)
        {
            Projectile[] tempArray = new Projectile[count];

            for (int i = 0; i < count; i++)
            {
                tempArray[i] = _pool.Get();
            }

            for (int i = 0; i < count; i++)
            {
                _pool.Release(tempArray[i]);
            }
        }
    }
}