using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Entity
{
	public abstract class Entity : IMovable, IDamageable, ICollidable
	{
		#region Move Properties
		public Vector2 WorldPosition { get; protected set; }
		public Vector2 Velocity { get; protected set; } = Vector2.Zero;
		public Vector2 Acceleration { get; protected set; } = Vector2.Zero;
		public float Speed { get; protected set; }
		#endregion

		#region Damage Properties
		public int MaxHealth { get; protected set; }
		public int CurHealth { get; protected set; }
		public double InvDuration { get; protected set; }
		public double InvTimer { get; protected set; }
		#endregion

		#region Collision Properties
		public Rectangle Hitbox { get; protected set; }
		public bool CollisionOn { get; protected set; }
		#endregion



		public virtual void Update(GameTime gameTime)
		{

		}

		public virtual void Move(Vector2 distance)
		{
			WorldPosition += distance;

			// Todo: Adjust hitbox to world position
		}

		public virtual void OnHitSomething(ICollidable other, CollisionType collision)
		{

		}

		public virtual void TakeDamage(int amount)
		{

		}

		public virtual void Die()
		{

		}

		public virtual void Draw(SpriteBatch sb)
		{

		}

	}
}
