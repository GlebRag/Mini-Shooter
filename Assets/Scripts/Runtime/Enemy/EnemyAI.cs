using UnityEngine;
using UnityEngine.AI;
using Runtime.Weapons.Core;
using Runtime.Weapons.Configs;
using Runtime.Combat.Pool;
using Runtime.Combat;

namespace Runtime.Enemy
{
    [RequireComponent(typeof(NavMeshAgent), typeof(HealthComponent))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Vision (Зрение обнаружения)")]
        [SerializeField] private float _viewDistance = 15f;
        [SerializeField] private float _viewAngle = 45f;
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private float _shootAngleThreshold = 8f; 

        [Header("Combat Vision (Боевое зрение)")]
        [SerializeField] private float _loseSightDistance = 22f; 
        [SerializeField] private float _forgetTime = 2f;         

        [Header("Combat Settings (Боевые настройки)")]
        [SerializeField] private float _shootDelay = 1f;         

        [Header("Patrol (Патрулирование)")]
        [SerializeField] private float _patrolRadius = 12f;    
        [SerializeField] private float _patrolWaitTime = 3f; 

        [Header("Vision Visualization")]
        [SerializeField] private LineRenderer _visionLineRenderer; 

        [Header("Weapon Settings")]
        [SerializeField] private WeaponConfig[] _availableWeapons;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private MeshRenderer _meshRenderer;

        private Transform _playerTarget;
        private ProjectileFactory _projectileFactory;

        private NavMeshAgent _agent;
        private HealthComponent _healthComponent;
        private Weapon _myWeapon;
        private bool _isPlayerDetected;


        private Vector3 _spawnPoint;
        private float _patrolTimer;
        private float _forgetTimer;
        private float _shootDelayTimer;
        private float _combatStoppingDistance;

        private Color _baseColor;
        private float _flashTimer;
        private float _lastHealth;

        public void Construct(Transform playerTarget, ProjectileFactory projectileFactory)
        {
            _playerTarget = playerTarget;
            _projectileFactory = projectileFactory;
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _healthComponent = GetComponent<HealthComponent>();
        }

        private void Start()
        {
            InitializeRandomWeapon();

            _lastHealth = _healthComponent.Health.Current;
            _healthComponent.Health.OnChanged += HandleHealthChanged;

            SetupVisionRenderer();

            _spawnPoint = transform.position;
            _agent.stoppingDistance = 0.3f;
        }

        private void Update()
        {
            if (_playerTarget == null || _myWeapon == null) return;

            _myWeapon.Tick(Time.deltaTime, Time.time);

            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_flashTimer <= 0f) _meshRenderer.material.color = _baseColor;
            }

            if (!_isPlayerDetected)
            {
                // Режим 1: Мирное патрулирование
                if (_visionLineRenderer != null && !_visionLineRenderer.enabled)
                    _visionLineRenderer.enabled = true;

                HandlePatrol();
                CheckForPlayer();
                DrawVisionCone();
            }
            else
            {
                // Режим 2: Бой
                if (_visionLineRenderer != null && _visionLineRenderer.enabled)
                    _visionLineRenderer.enabled = false;

                HandleCombat();
            }
        }

        private void OnDestroy()
        {
            if (_healthComponent != null && _healthComponent.Health != null)
                _healthComponent.Health.OnChanged -= HandleHealthChanged;
        }

        private void InitializeRandomWeapon()
        {
            if (_availableWeapons == null || _availableWeapons.Length == 0) return;

            int randomIndex = Random.Range(0, _availableWeapons.Length);
            WeaponConfig chosenConfig = _availableWeapons[randomIndex];
            _myWeapon = new Weapon(chosenConfig);

            ApplyColorByWeapon(chosenConfig);
            _combatStoppingDistance = chosenConfig.Range * 0.8f;
        }

        private void ApplyColorByWeapon(WeaponConfig config)
        {
            if (_meshRenderer == null) return;

            if (config.ProjectilesPerShot > 1) _baseColor = Color.red;
            else if (config.IsAutomatic) _baseColor = Color.green;
            else _baseColor = Color.blue;

            _meshRenderer.material.color = _baseColor;
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (current < _lastHealth)
            {
                _meshRenderer.material.color = Color.white;
                _flashTimer = 0.1f;

                if (!_isPlayerDetected)
                {
                    _isPlayerDetected = true;
                    _forgetTimer = _forgetTime;
                }
            }
            _lastHealth = current;
        }

        private void HandlePatrol()
        {
            if (_agent.stoppingDistance != 0.3f)
            {
                _agent.stoppingDistance = 0.3f;
            }

            if (!_agent.hasPath || _agent.remainingDistance <= _agent.stoppingDistance)
            {
                _patrolTimer += Time.deltaTime;

                if (_patrolTimer >= _patrolWaitTime)
                {
                    Vector3 randomPoint = GetRandomNavMeshPoint(_spawnPoint, _patrolRadius);
                    _agent.SetDestination(randomPoint);
                    _patrolTimer = 0f;
                }
            }
        }

        private Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += center;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return center;
        }

        private void CheckForPlayer()
        {
            Vector3 rayStart = transform.position + Vector3.up * 1.2f;
            Vector3 rayEnd = _playerTarget.position + Vector3.up * 1.2f;
            Vector3 directionToPlayer = (rayEnd - rayStart).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, _playerTarget.position);

            if (distanceToPlayer <= _viewDistance)
            {
                Vector3 horizontalDir = (_playerTarget.position - transform.position).normalized;
                horizontalDir.y = 0;

                if (Vector3.Angle(transform.forward, horizontalDir) <= _viewAngle)
                {
                    float maxRayDistance = Vector3.Distance(rayStart, rayEnd);
                    if (!Physics.Raycast(rayStart, directionToPlayer, maxRayDistance, _obstacleMask))
                    {
                        _isPlayerDetected = true;
                        _forgetTimer = _forgetTime;
                        _shootDelayTimer = 0f;
                        _patrolTimer = 0f;
                    }
                }
            }
        }

        private void HandleCombat()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _playerTarget.position);

            Vector3 rayStart = transform.position + Vector3.up * 1.2f;
            Vector3 rayEnd = _playerTarget.position + Vector3.up * 1.2f;
            Vector3 rayDirection = (rayEnd - rayStart).normalized;
            float maxRayDistance = Vector3.Distance(rayStart, rayEnd);

            bool hasLineOfSight = false;

            if (distanceToPlayer <= _loseSightDistance)
            {
                if (!Physics.Raycast(rayStart, rayDirection, maxRayDistance, _obstacleMask))
                {
                    hasLineOfSight = true;
                }
            }

            if (hasLineOfSight)
            {
                _forgetTimer = _forgetTime;
            }
            else
            {
                _forgetTimer -= Time.deltaTime;
                if (_forgetTimer <= 0f)
                {
                    _isPlayerDetected = false;
                    _shootDelayTimer = 0f;
                    _agent.ResetPath();
                    return;
                }
            }

            if (_agent.stoppingDistance != _combatStoppingDistance)
            {
                _agent.stoppingDistance = _combatStoppingDistance;
            }

            Vector3 lookDirection = (_playerTarget.position - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 12f);
            }

            _agent.SetDestination(_playerTarget.position);

            if (hasLineOfSight && distanceToPlayer <= _myWeapon.Config.Range)
            {
                _shootDelayTimer += Time.deltaTime;

                if (_shootDelayTimer >= _shootDelay)
                {
                    float angleToPlayer = Vector3.Angle(transform.forward, lookDirection);

                    if (angleToPlayer <= _shootAngleThreshold)
                    {
                        if (_myWeapon.TryShoot(Time.time, out int count))
                        {
                            SpawnProjectiles(count);
                        }
                    }
                }
            }
            else
            {
                _shootDelayTimer = 0f;
            }
        }

        private void SpawnProjectiles(int count)
        {
            Vector3 targetPoint = _playerTarget.position;
            float heightOffset = _firePoint.position.y - transform.position.y;
            targetPoint.y = _playerTarget.position.y + heightOffset;

            Vector3 aimDirection = (targetPoint - _firePoint.position).normalized;
            Quaternion baseAimRotation = Quaternion.LookRotation(aimDirection);

            for (int i = 0; i < count; i++)
            {
                Quaternion spreadRotation = Quaternion.Euler(
                    Random.Range(-_myWeapon.Config.Spread, _myWeapon.Config.Spread),
                    Random.Range(-_myWeapon.Config.Spread, _myWeapon.Config.Spread),
                    0f
                );

                Quaternion finalRotation = baseAimRotation * spreadRotation;
                _projectileFactory.Spawn(_firePoint.position, finalRotation, _myWeapon.Config.Damage, _myWeapon.Config.Range);
            }
        }

        private void SetupVisionRenderer()
        {
            if (_visionLineRenderer == null) return;
            _visionLineRenderer.useWorldSpace = false;
            _visionLineRenderer.positionCount = 22;
            _visionLineRenderer.startWidth = 0.05f;
            _visionLineRenderer.endWidth = 0.05f;

            _visionLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            Color green = new Color(0f, 1f, 0f, 0.3f);
            _visionLineRenderer.startColor = green;
            _visionLineRenderer.endColor = green;
        }

        private void DrawVisionCone()
        {
            if (_visionLineRenderer == null || !_visionLineRenderer.enabled) return;

            int segments = 20;
            _visionLineRenderer.SetPosition(0, Vector3.zero);

            float startAngle = -_viewAngle;
            float angleStep = (_viewAngle * 2f) / segments;

            for (int i = 0; i <= segments; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector3 localDir = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward;
                Vector3 worldDir = transform.TransformDirection(localDir);

                float distance = _viewDistance;
                if (Physics.Raycast(transform.position + Vector3.up, worldDir, out RaycastHit hit, _viewDistance, _obstacleMask))
                {
                    distance = hit.distance;
                }

                Vector3 localPoint = localDir * distance;
                localPoint.y = -0.9f;

                _visionLineRenderer.SetPosition(i + 1, localPoint);
            }
            _visionLineRenderer.SetPosition(segments + 1, Vector3.zero);
        }
    }
}