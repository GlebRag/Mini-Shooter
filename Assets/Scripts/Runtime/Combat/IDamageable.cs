using System;
using UnityEngine;

namespace Runtime.Combat
{
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}