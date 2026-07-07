using UnityEngine;

namespace Runtime.Weapons.Configs
{
    [CreateAssetMenu(fileName = "NewWeaponConfig", menuName = "Configs/Weapon Config")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("Base Parameters")]
        [SerializeField] private string _weaponName;
        [SerializeField] private float _damage = 10f;
        [SerializeField] private float _fireRate = 2f;
        [SerializeField] private float _range = 50f;
        [SerializeField] private float _spread = 2f;
        [SerializeField] private float _reloadTime = 1.5f;
        [SerializeField] private int _magazineSize = 30;

        [Header("Mechanics")]
        [SerializeField] private bool _isAutomatic;
        [SerializeField] private int _projectilesPerShot = 1;
        [SerializeField] private GameObject _projectilePrefab;

        public string WeaponName => _weaponName;
        public float Damage => _damage;
        public float FireRate => _fireRate;
        public float Range => _range;
        public float Spread => _spread;
        public float ReloadTime => _reloadTime;
        public int MagazineSize => _magazineSize;
        public bool IsAutomatic => _isAutomatic;
        public int ProjectilesPerShot => _projectilesPerShot;
        public GameObject ProjectilePrefab => _projectilePrefab;
    }
}