using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Entity
{
	public class BombPin : Enemy
	{
		private Sprite _explosionCloud;

		// Constructors
		public BombPin(Game1 gm, Vector2 position) 
			: base(gm, position)
		{
			// Set Enemy Image
			Texture2D puncherSpritesheet = 
				gm.Content.Load<Texture2D>("Sprites/BombPinSpritesheet");

			Image = new Sprite(
				puncherSpritesheet,
				new Rectangle(
					0, 0,
					120, 140),
				new Rectangle(
					(int)position.X,
					(int)position.Y,
					(int)(Game1.TileSize * 1.5f),
					(int)(Game1.TileSize * 1.5f *
					140 / 120)));
			Image.FrameBounds = Image.SourceRect;
			Image.Columns = 3;

			// Position
			WorldPosition = position;

			// Hitbox
			Hitbox = new Rectangle(
				(int)WorldPosition.X + Image.DestinationRect.Width / 2 - 50,
				(int)WorldPosition.Y + Image.DestinationRect.Height - 100,
				100,
				100);


			// Set Vitality
			MaxHealth = 3;
			CurHealth = MaxHealth;
			InvDuration = 0.5;

			// Attacking
			_attackForce = 20f;
			_attackDuration = 1.2;
			_attackDurationTimer = 0.0;
			_attackRadius = Game1.TileSize * 4;
			_attackRange = Game1.TileSize * 3;
			_attackWindupDuration = 0.6;
			_attackWindupTimer = 0;

			_attackCooldown = 0.8;
			_attackCooldownTimer = 0.0;

			// Set type
			Type = EntityType.Enemy;

			// Set state
			_chaseRange = Game1.TileSize * 5;
			_aggroRange = Game1.TileSize * 2;

			ActionState = EnemyState.Idle;

			// Extra aesthetics
			Texture2D _koStarSpritesheet = 
				gm.Content.Load<Texture2D>("Sprites/KO_StarsSpritesheet");
            _knockoutStars = new Sprite(
				_koStarSpritesheet,
				new Rectangle(
					0, 0,
					_koStarSpritesheet.Bounds.Width / 4,
					_koStarSpritesheet.Bounds.Height),
				new Rectangle(
					0, 0,
					(int)(Game1.TileSize * 1.5), 
					(int)(Game1.TileSize * 1.5)));

			_koStarAnimDuration = 0.5;

			Texture2D explosion = gm.Content.Load<Texture2D>("BoomCloud");
			_explosionCloud = new Sprite(
				explosion,
				explosion.Bounds,
				new Rectangle(
					0, 0,
					(int)(_attackRadius),
					(int)(_attackRadius)));

			return;
		}

		// Methods

		public override void Update(GameTime gameTime)
		{

			TickInvincibility(gameTime);

			Vector2 distanceFromPlayer = Game1.Player.CenterPosition - CenterPosition;
			float playerDist = distanceFromPlayer.Length();

			DetermineState(playerDist);

			switch (ActionState)
			{
				case EnemyState.KO:
                    UpdateKOAnimation(gameTime);

                    break;

				case EnemyState.Idle:
					Velocity = Vector2.Zero;
					break;

				case EnemyState.Attack:
					Velocity = Vector2.Zero;

					// Charge attack
					if (_attackWindupTimer > 0)
					{
						_attackWindupTimer -=
							gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
					}

					// If not currently attacking or winding up
					// Use attack 
					if (_attackDurationTimer <= 0 && _attackWindupTimer <= 0)
						_attackDurationTimer = _attackDuration;

					// Currently Attacking
					if (_attackDurationTimer > 0)
					{
						Attack();
						_attackDurationTimer -=
							gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;

						if (_attackDurationTimer <= 0)
						{
							EndAttack(false);
						}
					}
					break;
			}

			CollisionOn = !IsKO && _attackDurationTimer < _attackDuration;

			CheckEnemyCollisions();

			CollisionChecker.CheckTilemapCollision(this, Game1.CurrentLevel.CurrentRoom.Tileset);

			// Update animations
			
			UpdateEnemySprite();

			return;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			// Draw Enemy relative to the player
			Vector2 screenPos = WorldPosition + Game1.MainCamera.WorldToScreenOffset;

			switch (ActionState)
			{
				case EnemyState.KO:

                    Vector2 drawPos =
                        screenPos +
                        new Vector2(Image.DestinationRect.Width, 0f);

                    Image.Draw(
                        spriteBatch,
                        drawPos,
                        MathHelper.PiOver2,
                        Vector2.Zero);

                    Vector2 starDrawPos =
                        screenPos -
                        new Vector2(0f, _knockoutStars.DestinationRect.Height / 2);

                    _knockoutStars.Draw(
                        spriteBatch,
                        starDrawPos,
                        0f,
                        Vector2.Zero);

                    break;

				case EnemyState.Idle:

					Image.Draw(
						spriteBatch, 
						screenPos, 
						0f, 
						Vector2.Zero);
					break;

				case EnemyState.Attack:

					if (_attackWindupTimer > 0)
					{
						Image.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
						return;
					}

                    Vector2 explosionScreenPos =
						_attackHitbox.Location.ToVector2() + Game1.MainCamera.WorldToScreenOffset;

					_explosionCloud.Draw(
						spriteBatch,
						explosionScreenPos,
						0f,
						Vector2.Zero);

					break;
			}

			// Display current health
			//spriteBatch.DrawString(Game1.ARIAL32, $"Hp: {CurHealth}", screenPos, Color.White);

			// Display attack delay
			//spriteBatch.DrawString(
			//    Game1.ARIAL32,
			//    $"ATK_D: {_attackDelayTimer:0.00}",
			//    screenPos,
			//    Color.White);
			return;
		}

        #region Collision Handling

        public override void OnHitEntity(Entity entity, CollisionDirection collision)
        {
            switch (entity.Type)
            {
                case EntityType.Player:
					Game1.Player.CurCore.Ricochet(Vector2.Zero);

                    break;
            }

            base.OnHitEntity(entity, collision);
        }

        #endregion

        #region Attack Methods


        /// <summary>
        /// Aims the enemy toward the player at it's
        /// normal speed. Stops if gets within its attack
        /// range of other enemies
        /// </summary>
        protected override void TargetPlayer()
		{
			// Get direction from self to player
			Vector2 playerDirection = 
				Game1.Player.CenterPosition - CenterPosition;

			// Aim enemy toward player at their speed
			playerDirection.Normalize();
			playerDirection *= Speed;

			// Get future position
			Vector2 positionAfterMoving = WorldPosition + playerDirection;

            if (CheckTilemapCollisionAhead(this, playerDirection, Game1.CurrentLevel.CurrentRoom.Tileset))
            {
                Vector2 newDirection = new Vector2(playerDirection.Y, -playerDirection.X);
                newDirection.Normalize();
                newDirection *= Speed;
                positionAfterMoving = WorldPosition + newDirection;
            }
           
            // Stop if get too close to another enemy
			foreach (Enemy e in Game1.EManager.Enemies)
			{
				// Skip self
				if (e == this) continue;

				Vector2 distFromEnemy = e.WorldPosition - positionAfterMoving;

				// Enemy is too close,
				// MOVE IN THE OPPOSITE DIRECTION
				if (distFromEnemy.Length() <= _attackRange * 2)
				{
					distFromEnemy.Normalize();
					Velocity = distFromEnemy * -Speed;
					return;
				}
			}

			// Move in direction of player
			Velocity = playerDirection;
			return;
		}

        public static bool CheckTilemapCollisionAhead(Entity e, Vector2 direction, Tileset tileset)
        {
            Vector2 nextPosition = e.WorldPosition + direction * e.Speed;
            
            Rectangle predictedHitbox = new Rectangle(
				(int)nextPosition.X, 
				(int)nextPosition.Y,
				e.Hitbox.Width, 
				e.Hitbox.Height);         
			
            for (int row = 0; row < tileset.Layout.GetLength(0); row++)
            {
                for (int col = 0; col < tileset.Layout.GetLength(1); col++)
                {
                    Tile tile = tileset.Layout[row, col];
                    if (tile.CollisionOn && predictedHitbox.Intersects(tile.Hitbox))
                    {                       
                        return true;
                    }
                }
            }
            return false;
        }
      
        protected override void Attack()
		{
			// Cast the damage box
			Point center = CenterPosition.ToPoint();
			Rectangle damageBox = new Rectangle(
				center.X - (int)_attackRadius / 2,
				center.Y - (int)_attackRadius / 2,
				(int)_attackRadius,
				(int)_attackRadius);

			// Store damage box
			_attackHitbox = damageBox;

			// Check if player is in damage box
			bool hitPlayerDir = damageBox.Intersects(Game1.Player.Hitbox);
			
			// Enemies in proximity also take damage
			foreach(Enemy e in Game1.EManager.Enemies)
			{
				if (e.Hitbox.Intersects(damageBox))
				{
					if (e == this)
					{
						e.TakeDamage(100);
						continue;
					}

					e.TakeDamage(2);
				}
			}

			if (hitPlayerDir)
			{
				Vector2 explosionForce = Game1.Player.CenterPosition - CenterPosition;
				explosionForce.Normalize();
				explosionForce *= _attackForce;

				Game1.Player.TakeDamage(2);

				Game1.Player.CurCore.StopCurving();
				Game1.Player.CurCore.Ricochet(explosionForce);
			}

			return;
		}

		protected override void EndAttack(bool stoppedEarly)
		{
			TakeDamage(100);

			ActionState = EnemyState.KO;

			return;
		}

        protected override void DetermineState(float playerDist)
        {
			if (playerDist < _attackRange
				&& _attackWindupTimer == 0
				&& ActionState != EnemyState.Attack)
			{
				_attackWindupTimer = _attackWindupDuration;

				ActionState = EnemyState.Attack;
			}
        }

        #endregion

        #region Drawing Helper Methods

		private void UpdateEnemySprite()
		{
			// Reset Image Color
			Image.TintColor = Color.White;

            if (IsInvincible)
			{
				Image.TintColor *= 0.8f;

				//Image.SourceRect = new Rectangle(
    //                Image.SourceRect.Width * 4,
				//	Image.SourceRect.Y,
				//	Image.SourceRect.Width,
				//	Image.SourceRect.Height);
				return;
			}


			if (ActionState == EnemyState.Attack) Image.TintColor = Color.Orange;

			if (_attackDurationTimer <= 0)
			{
				int spriteNum = (int)
					((_attackWindupDuration - _attackWindupTimer) /
					(_attackWindupDuration / 3));

				Image.SetSourceToFrame(spriteNum + 1);
			}

		}

        #endregion

       
    }
}
