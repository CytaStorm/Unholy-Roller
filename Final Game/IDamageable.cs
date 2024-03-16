using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
	public interface IDamageable
	{
		// Properties
		int MaxHealth { get; }
		int CurHealth { get; }
		double InvDuration { get; }
		double InvTimer { get; }

		// Methods
		void TakeDamage(int amount);
		void Die();
	}
}
