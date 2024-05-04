using Microsoft.Xna.Framework.Graphics;
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
		KO
	}

	public abstract class Enemy : Entity
	{
		#region Fields
		private double _koDuration = 5;
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
		
		protected Sprite _gloveImages;
		protected int _gloveFrameWidth;

		// Animation
		protected double _walkAnimTimeCounter;
		protected double _walkAnimSecondsPerFrame;
		protected int _walkAnimCurrentFrame;

        protected Sprite _knockoutStars;
        protected double _koStarAnimDuration;
        protected double _koStarAnimTimeCounter;

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

			Type = EntityType.Enemy;
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
			if (InvTimer > 0 || IsKO) 
				return;
				
				// Enemy doesn't take damage if they have infinite health
				// Still gets invFrames so collision with other entities
				// isn't messed up
				if (!InfiniteHealth)
					CurHealth -= damage;

				// Temporarily become invincible
				InvTimer = InvDuration;

				// Handle low health
				if (CurHealth <= 0)
				{
					// Enemy is temporarily knocked out
					Managers.SoundManager.PlayKnockSound();
					_koTimer = _koDuration;

					// Enemy doesn't properly die
					CurHealth = 1;

					// Stop moving
					Velocity = Vector2.Zero;
				}
		}

		#endregion 

		#region Attack Methods
		protected virtual void TargetPlayer() { }
		
		protected virtual void DetermineState(float playerDist)
		{
			if (IsKO)
			{
				ActionState = EnemyState.KO;
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

		public override void DrawGizmos()
		{
			// Visualize attack windup with a vertical progress bar
			Vector2 screenPos = Game1.MainCamera.GetPerspectivePosition(
				WorldPosition + Game1.MainCamera.WorldToScreenOffset);

			// Get amount attack has charged
			float attackReadiness = 
				(float)((_attackWindupDuration - _attackWindupTimer) 
				/ _attackWindupDuration);
			
			// Get how full the progress bar should be
			float boxFill = 
				Image.DestinationRect.Height * attackReadiness;
			
			// Make a rectangle representing attack progress
			Rectangle attackProgressBar = new Rectangle(
				(int)screenPos.X,
				(int)(screenPos.Y + 
				(Image.DestinationRect.Height - boxFill) * Game1.MainCamera.Zoom),
				(int)(Image.DestinationRect.Width * Game1.MainCamera.Zoom),
				(int)boxFill);

			// Draw progress bar
			ShapeBatch.Box(attackProgressBar, Color.Yellow * 0.4f);

			// Attacking
			if (_attackDurationTimer > 0)
			{
				// Draw Attack Hitbox
				int atkHitDistX = (int)WorldPosition.X - _attackHitbox.X;
				int atkHitDistY = (int)WorldPosition.Y - _attackHitbox.Y;

				Rectangle drawnAttackHit = new Rectangle(
					(int)(screenPos.X - atkHitDistX * Game1.MainCamera.Zoom),
					(int)(screenPos.Y - atkHitDistY * Game1.MainCamera.Zoom),
					(int)(_attackHitbox.Width * Game1.MainCamera.Zoom),
					(int)(_attackHitbox.Height * Game1.MainCamera.Zoom));

				Color fadedGreen = new Color(0f, 0f, 1f, 0.4f);

				ShapeBatch.Box(drawnAttackHit, fadedGreen);
			}

			base.DrawGizmos();
		}

        protected void UpdateKOAnimation(GameTime gameTime)
        {
            int numKoStars = 4;

            // Current frame is (passedTime / (seconds per frame))
            int curFrame =
                (int)(_koStarAnimTimeCounter /
                (_koStarAnimDuration / (numKoStars - 1)));

            // Get corresponding image
            _knockoutStars.SourceRect = new Rectangle(
                _knockoutStars.SourceRect.Width * curFrame,
                0,
                _knockoutStars.SourceRect.Width,
                _knockoutStars.SourceRect.Height);

            // Move animation forward
            _koStarAnimTimeCounter +=
                gameTime.ElapsedGameTime.TotalSeconds *
                Player.BulletTimeMultiplier;

            // Reset animation once it reaches or exceeds its duration
            if (_koStarAnimTimeCounter >= _koStarAnimDuration)
                _koStarAnimTimeCounter -= _koStarAnimDuration;
        }

        #endregion

    }
}
