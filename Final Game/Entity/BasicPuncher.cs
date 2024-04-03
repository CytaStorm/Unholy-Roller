﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Entity
{
	public class BasicPuncher : Enemy
	{
		// Constructors
		public BasicPuncher(Game1 gm, Vector2 position) 
			: base(gm, position)
		{
			// Set Enemy Image
			Texture2D puncherSpritesheet = 
				gm.Content.Load<Texture2D>("Sprites/BasicEnemy");

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

			// Position
			WorldPosition = position;

			// Hitbox
			Hitbox = new Rectangle(
				(int)WorldPosition.X + Image.DestinationRect.Width / 2 - 50,
				(int)WorldPosition.Y + Image.DestinationRect.Height - 100,
				100,
				100);

			// Default Speed
			Speed = 5f;

			// Set Vitality
			MaxHealth = 3;
			CurHealth = MaxHealth;
			InvDuration = 0.5;
			InvTimer = InvDuration;

			// Attacking
			_attackForce = 15f;
			_attackDuration = 0.2;
			_attackDurationTimer = 0.0;
			_attackRadius = Game1.TileSize;
			_attackRange = Game1.TileSize;
			_attackWindupDuration = 0.25;
			_attackWindupTimer = _attackWindupDuration;

			_attackCooldown = 0.8;
			_attackCooldownTimer = 0.0;

			Texture2D gloveSpritesheet = 
				gm.Content.Load<Texture2D>("Sprites/PinPunchSpritesheet");

			_gloveFrameWidth = gloveSpritesheet.Width / 3;
			_gloveImages = new Sprite(
				gloveSpritesheet,
				new Rectangle(
					0, 0,
					_gloveFrameWidth, gloveSpritesheet.Height),
				new Rectangle(
					0, 0, 
					(int)_attackRadius, (int)_attackRadius));

			// Set type
			Type = EntityType.Enemy;

			// Set state
			_chaseRange = Game1.TileSize * 5;
			_aggroRange = Game1.TileSize * 3;

			ActionState = EnemyState.Chase;

			// Animation
			_walkAnimSecondsPerFrame = 0.12;
			return;
		}

		// Methods

		public override void Update(GameTime gameTime)
		{
			TickInvincibility(gameTime);

			TickKnockout(gameTime);


			//CollisionChecker.CheckTilemapCollision(this, Game1.TEST_ROOM.Floor);

			Vector2 distanceFromPlayer = Game1.Player.CenterPosition - CenterPosition;
			float playerDist = distanceFromPlayer.Length();

			DetermineState(playerDist);

			switch (ActionState)
			{
				case EnemyState.Idle:
					Velocity = Vector2.Zero;
					break;

				case EnemyState.Chase:
					TargetPlayer();
					break;

				case EnemyState.Attack:
					Velocity = Vector2.Zero;

					// Apply attack cooldown
					if (_attackCooldownTimer > 0)
					{
						_attackCooldownTimer -=
							gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
					}
					else
					{
						// Choose a direction to attack in
						if (!_attackDirChosen)
						{
							// Get direction from self to player
							_attackDirection = Game1.Player.CenterPosition - CenterPosition;

							// Apply attack range
							_attackDirection.Normalize();
							_attackDirection *= _attackRange;

							_attackDirChosen = true;
						}

						// Charge attack
						_attackWindupTimer -=
							gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;

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
					}
					break;
			}

			CollisionOn = !IsKO;

			CheckEnemyCollisions();

			CollisionChecker.CheckTilemapCollision(this, Game1.TestLevel.CurrentRoom.Tileset);

			Move(Velocity * Player.BulletTimeMultiplier);

			// Update animations
			
			UpdateWalkAnimation(gameTime);

			return;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			// Draw Enemy relative to the player
			Vector2 screenPos = WorldPosition + Game1.MainCamera.WorldToScreenOffset;

			if (IsKO)
			{
				Image.TintColor = Color.Blue;
			}
			else if (_hitPlayer)
			{
				Image.TintColor = Color.Red;
				_hitPlayer = false;
			}
			else
			{
				Image.TintColor = Color.White;
			}

			//Image.Draw(spriteBatch, screenPos);

			switch (ActionState)
			{
				case EnemyState.Idle:
					if (IsKO)
					{
						DrawKoed(spriteBatch, screenPos);
						break;
					}
					Image.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					break;

				case EnemyState.Chase:
					DrawWalking(spriteBatch, screenPos);
					break;

				case EnemyState.Attack:
					if (_attackWindupTimer < _attackWindupDuration && _attackDurationTimer <= 0.0)
					{
						Image.TintColor = Color.Orange;
					}

					Image.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					DrawAttacking(spriteBatch, screenPos);
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

		#region Attack Methods

		protected override void TargetPlayer()
		{
			// Get direction from self to player
			Point eMinusP = Game1.Player.Hitbox.Center - Hitbox.Center;
			Vector2 directionToPlayer = new Vector2(eMinusP.X, eMinusP.Y);

			// Aim enemy toward player at their speed
			directionToPlayer.Normalize();
			directionToPlayer *= Speed;

			Vector2 positionAfterMoving = WorldPosition + directionToPlayer;

			// Stop if get too close to another enemy
			bool shouldStop = false;
			float minDistanceFromEnemies = Game1.TileSize * 3;
			foreach (Enemy e in Game1.EManager.Enemies)
			{
				Vector2 distFromEnemyAfterMoving = (e.WorldPosition - positionAfterMoving);
				if (e != this &&
					distFromEnemyAfterMoving.LengthSquared() <= minDistanceFromEnemies * minDistanceFromEnemies)
				{
					shouldStop = true;
					break;
				}
			}

			if (shouldStop)
			{
				Velocity = Vector2.Zero;
				return;
			}

			Velocity = directionToPlayer;
			return;
		}

		protected override void Attack()
		{
			Vector2 directionToPlayer = _attackDirection;

			// Cast the damage box
			Rectangle damageBox = new Rectangle(
				(int)(CenterPosition.X + directionToPlayer.X - _attackRadius / 2),
				(int)(CenterPosition.Y + directionToPlayer.Y - _attackRadius / 2),
				(int)_attackRadius,
				(int)_attackRadius);

			// Store damage box
			_attackHitbox = damageBox;

			// Check if player is in damage box
			bool hitPlayerDir = damageBox.Intersects(Game1.Player.Hitbox);
			//CollisionChecker.CheckEntityCollision(damageBox, Game1.Player);

			if (hitPlayerDir)
			{
				directionToPlayer.Normalize();
				directionToPlayer *= _attackForce;

				Game1.Player.TakeDamage(1);

				// Keep player from ricocheting endlessly
				// while in attack area
				if (!_attackLandedOnce)
				{
					Game1.Player.Ricochet(directionToPlayer);
					_attackLandedOnce = true;
				}
			}

			return;
		}

		protected override void EndAttack(bool stoppedEarly)
		{
			// Reset windup
			_attackWindupTimer = _attackWindupDuration;

			if (stoppedEarly)
				_attackCooldown = 0;
			else
				_attackCooldownTimer = _attackCooldown;

			// Enemy is not attacking
			_attackDurationTimer = 0;

			// No new attack has happened
			_attackLandedOnce = false;
			_attackDirChosen = false;

			return;
		}

		#endregion

		#region Drawing Helper Methods

		private void UpdateWalkAnimation(GameTime gameTime)
		{
			// Add to the time counter (need TOTALSECONDS here)
			_walkAnimTimeCounter += gameTime.ElapsedGameTime.TotalSeconds 
				* Player.BulletTimeMultiplier;

			// Has enough time gone by to actually flip frames?
			if (_walkAnimTimeCounter >= _walkAnimSecondsPerFrame)
			{
				// Update the frame and wrap
				_walkAnimCurrentFrame++;
				if (_walkAnimCurrentFrame > 4) _walkAnimCurrentFrame = 1;

				// Remove one "frame" worth of time
				_walkAnimTimeCounter -= _walkAnimSecondsPerFrame;
			}

			return;
		}

		private void DrawWalking(SpriteBatch sb, Vector2 screenPos)
		{

			switch (_walkAnimCurrentFrame)
			{
				case 1:
					Vector2 origin = new Vector2(
								Image.SourceRect.X,
								Image.SourceRect.Center.Y - 15);


					Image.Draw(sb, screenPos, MathF.PI / 6, origin);
					break;

				case 2:
					Image.Draw(sb, screenPos, 0f, Vector2.Zero);
					break;

				case 3:
						
                    Image.Draw(
						sb, 
						screenPos, 
						-MathF.PI / 6,
                        new Vector2(
                            Image.SourceRect.Center.X,
                            Image.SourceRect.Y));
					break;

				case 4:
					Image.Draw(sb, screenPos, 0f, Vector2.Zero);
					break;
			}
			return;
		}

		private void DrawAttacking(SpriteBatch sb, Vector2 screenPos)
		{

			// Draw Attack Windup
			if (_attackWindupTimer < _attackWindupDuration && _attackDurationTimer <= 0d)
			{
				// Get the center position of the pulled back fist
				// in screen space

				Vector2 correction = new Vector2(
					Image.DestinationRect.Width / 2.5f,
					Image.DestinationRect.Height / 2.5f);

				Vector2 windupScreenPos =
                    CenterPosition + correction - _attackDirection
                    + Game1.MainCamera.WorldToScreenOffset;

				// Shift position to top-left of glove image (for drawing)
				windupScreenPos -= new Vector2(_gloveFrameWidth / 2, _gloveFrameWidth / 2);

                // Select the correct glove image
                _gloveImages.SourceRect = 
					new Rectangle(
						0, 0, 
						_gloveFrameWidth, _gloveImages.SourceRect.Height);

                // Rotate fist so knuckles face away from player
                float dirAngle = MathF.Atan2(_attackDirection.Y, _attackDirection.X);

                // Draw glove
                _gloveImages.Draw(
					sb, 
					windupScreenPos, 
					dirAngle,
                    _gloveImages.SourceRect.Center.ToVector2());
			}

			// Draw actively attacking
			if (_attackDurationTimer > 0)
			{
				// Get position of the extended fist
				// in screen space

				Vector2 attackScreenPos = CenterPosition + _attackDirection * 3f
                    + Game1.MainCamera.WorldToScreenOffset;

				// Rotate fist so knuckles face player
				float dirAngle = MathF.Atan2(_attackDirection.Y, _attackDirection.X);

				// Get proper glove image
				_gloveImages.SourceRect = new Rectangle(
                    _gloveFrameWidth * 2,
                    0,
                    _gloveFrameWidth,
                    _gloveFrameWidth);

				// Draw glove
				_gloveImages.Draw(
					sb,
					attackScreenPos,
					dirAngle,
					_gloveImages.SourceRect.Center.ToVector2());
			}
			return;
		}

		#endregion
	}
}
