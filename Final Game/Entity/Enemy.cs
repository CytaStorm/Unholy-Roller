﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Final_Game.LevelGen;

namespace Final_Game.Entity
{
	public enum EnemyState
	{
		Idle,
		Chase,
		Attack,
	}

	public abstract class Enemy : Entity
	{
		#region Fields
		private double _koDuration = 3;
		private double _koTimer;

		protected float _attackForce;
		protected double _attackDuration;
		protected double _attackDurationTimer;
		protected float _attackRadius;
		protected float _attackRange;
		protected double _attackWindupDuration;
		protected double _attackWindupTimer;
		protected double _attackCooldown;
		protected double _attackCooldownTimer;
		
		protected bool _attackLandedOnce;
		protected Rectangle _attackHitbox;
		protected Vector2 _attackDirection;
		protected bool _attackDirChosen;
		
		protected float _chaseRange;
		protected float _aggroRange;
		
		protected Texture2D _gloveSpriteSheet;
		protected int _gloveFrameWidth;

		// Animation
		protected double _walkAnimTimeCounter;
		protected double _walkAnimSecondsPerFrame;
		protected int _walkAnimCurrentFrame;

		protected bool _hitPlayer;

		#endregion

		#region Properties
		
		public bool IsKO { get => _koTimer > 0; }
		public EnemyState ActionState { get; protected set; }

		#endregion

		// Constructors
		public Enemy(Game1 gm, Vector2 position) 
		{
			WorldPosition = position;
		}

		// Methods

		#region Collision Handling Methods
		public override void OnHitEntity(Entity entity, CollisionDirection collision)
		{
			switch (entity.Type)
			{
				case EntityType.Player:
					_hitPlayer = true;

					// Cancel out attack charge
					_attackWindupTimer = _attackWindupDuration;

					break;

				case EntityType.Enemy:
					Velocity = Vector2.Zero;
					break;
			}

			base.OnHitEntity(entity, collision);
		}

		public override void OnHitTile(Tile tile, CollisionDirection colType)
		{
			switch (tile.Type)
			{
				case TileType.Wall:

					Move(-Velocity);

					break;
			}
		}
		
		protected virtual void CheckEnemyCollisions()
		{
			for (int i = 0; i < Game1.EManager.Enemies.Count; i++)
			{
				Enemy curEnemy = Game1.EManager.Enemies[i];
				if (curEnemy == this) continue;

				CollisionChecker.CheckEntityCollision(this, curEnemy);
			}
			return;
		}

		#endregion

		#region Health Methods
		protected virtual void TickKnockout(GameTime gameTime)
		{
			if (_koTimer > 0)
			{
				//Debug.WriteLine("here");
				_koTimer -= gameTime.ElapsedGameTime.TotalSeconds 
					* Player.BulletTimeMultiplier;
			}
		}

		public override void TakeDamage(int damage)
		{
			// Take damage if not invincible
			if (InvTimer <= 0 && !IsKO)
			{
				CurHealth -= damage;

				// Temporarily become invincible
				InvTimer = InvDuration;

				// Handle low health
				if (CurHealth <= 0)
				{
					// Enemy is temporarily knocked out
					_koTimer = _koDuration;
					CurHealth = 1;
				}
			}
		}

		#endregion 

		#region Attack Methods
		protected virtual void TargetPlayer() { }
		
		protected virtual void DetermineState(float playerDist)
		{
			if (IsKO)
			{
				ActionState = EnemyState.Idle;
				return;
			}
			if (playerDist < _aggroRange)
			{
				ActionState = EnemyState.Attack;
				return;
			}
			if (playerDist < _chaseRange)
			{
				ActionState = EnemyState.Chase;

				EndAttack(true);
				return;
			}

			ActionState = EnemyState.Idle;
			return;

		}

		protected abstract void Attack();

		protected abstract void EndAttack(bool stoppedEarly);
		#endregion

		#region Drawing Helpers
		protected virtual void DrawKoed(SpriteBatch sb, Vector2 screenPos)
		{
			//if (InvTimer >= .2 * InvDuration)
			//{
			//    // Flash a certain color
			//}

			sb.Draw(
				Image.Texture,
				screenPos,
				Image.SourceRect,
				Image.TintColor,
				MathHelper.PiOver2,
				new Vector2(
					Image.SourceRect.Center.X,
					Image.SourceRect.Center.Y),
				(float)Image.DestinationRect.Width / Image.SourceRect.Width,
				SpriteEffects.None,
				0f);
			return;
		}
		public override void DrawGizmos()
		{
			// Draw Enemy relative to the player
			Vector2 distFromPlayer = WorldPosition - Game1.Player.WorldPosition;
			Vector2 screenPos = Game1.Player.ScreenPosition + distFromPlayer;

			// Show attack windup 
			float attackReadiness = (float)((_attackWindupDuration - _attackWindupTimer) / _attackWindupDuration);

			float boxFill = Image.DestinationRect.Height * attackReadiness;
			int boxFillInt = (int)MathF.Round(boxFill);

			Rectangle readyBox = new Rectangle(
				(int)screenPos.X,
				(int)screenPos.Y + Image.DestinationRect.Height - boxFillInt,
				Image.DestinationRect.Width,
				boxFillInt);

			Color fadedYellow = new Color(0.5f, 0.5f, 0.5f, 0.4f);

			ShapeBatch.Box(readyBox, fadedYellow);

			// Draw Hitbox
			int hitDistX = (int)WorldPosition.X - Hitbox.X;
			int hitDistY = (int)WorldPosition.Y - Hitbox.Y;

			Rectangle drawnHit = new Rectangle(
				(int)(screenPos.X - hitDistX),
				(int)(screenPos.Y - hitDistY),
				Hitbox.Width,
				Hitbox.Height);

			Color fadedRed = new Color(1f, 0f, 0f, 0.4f);

			ShapeBatch.Box(drawnHit, fadedRed);

			// Attacking
			if (_attackDurationTimer > 0)
			{
				// Draw Hitbox
				int atkHitDistX = (int)WorldPosition.X - _attackHitbox.X;
				int atkHitDistY = (int)WorldPosition.Y - _attackHitbox.Y;

				Rectangle drawnAttackHit = new Rectangle(
					(int)(screenPos.X - atkHitDistX),
					(int)(screenPos.Y - atkHitDistY),
					_attackHitbox.Width,
					_attackHitbox.Height);

				Color fadedGreen = new Color(0f, 0f, 1f, 0.4f);

				ShapeBatch.Box(drawnAttackHit, fadedGreen);
			}
		}

		#endregion

	}
}
