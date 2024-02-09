using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    public interface IDamageable
    {
        // Properties
        int MaxHealth { get; protected set; }
        int CurHealth { get; protected set; }

        // Methods
        void TakeDamage(int damage);
        void Die();


    }
}
