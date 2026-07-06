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

            // Создаем пустой объект-контейнер, чтобы снаряды в иерархии Unity не мозолили глаза
            _poolRoot = new GameObject("[Projectile Pool]").transform;

            // Инициализируем стандартный пул Unity
            _pool = new ObjectPool<Projectile>(
                createFunc: CreateInstance,
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: true,
                defaultCapacity: initialCapacity,
                maxSize: maxCapacity
            );

            // Прогрев пула (Pre-warm): принудительно создаем стартовые 50 снарядов
            PreWarm(initialCapacity);
        }

        public void Spawn(Vector3 position, Quaternion rotation, float damage, float range)
        {
            // Берем готовый снаряд из пула
            Projectile projectile = _pool.Get();

            // Настраиваем его позицию и физику
            projectile.transform.SetPositionAndRotation(position, rotation);
            projectile.Initialize(damage, range, ReturnToPool);
        }

        private void ReturnToPool(Projectile projectile)
        {
            _pool.Release(projectile);
        }

        // --- Колбэки управления элементами пула ---

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

            // Достаем 50 штук (они создаются в памяти)
            for (int i = 0; i < count; i++)
            {
                tempArray[i] = _pool.Get();
            }

            // Тут же возвращаем их обратно спать
            for (int i = 0; i < count; i++)
            {
                _pool.Release(tempArray[i]);
            }
        }
    }
}