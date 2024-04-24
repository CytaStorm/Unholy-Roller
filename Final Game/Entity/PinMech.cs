using Final_Game.LevelGen;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;
using Final_Game.Managers;
using System.Diagnostics;

namespace Final_Game.Entity
{
    public enum BossState
	{
		PinThrow,
		PinBombs,
		Overheated,
		HandSwipe,
		Idle

	}
	public class PinMech : Enemy
	{

		public event EntityDying OnBossDeath;

		private BossState BossActionState;
		private int pinBombsChanceModifier;
		private double pinBombsDuration;
		private double pinBombsDurationTimer;
		private double pinBombsDelay;
		private IndicatorManager indicators;
		private Texture2D circle;
		private Game1 gm;
		private double _pinThrowWindUp;
		private bool aboutToThrow;
		private int savedYdirection;
		private int savedXdirection;
		private double overheatTimer;
		private string debugState;
		private double stunTimer;

		//
		private Sprite _regularArmLeft;
		private Sprite _regularArmRight;
		private Sprite _punchArm;

		private Vector2 _rightArmOffset;

		// Constructors
		public PinMech(Game1 gm, Vector2 position)
			: base(gm, position)
		{
			// Set Enemy Image
			//Load sprites
			Texture2D bossSprite =
				gm.Content.Load<Texture2D>("Sprites/boss");
			Texture2D regArms =
				gm.Content.Load<Texture2D>("Sprites/bossregulararm");
			Texture2D punchArm = 
				gm.Content.Load<Texture2D>("Sprites/bossarmattack");
			circle = gm.Content.Load<Texture2D>("RedCircle");

			this.gm = gm;

			//Main body
			Image = new Sprite(
				bossSprite,
				new Rectangle(
					0, 0,
					150, 250),
				new Rectangle(
					(int)position.X,
					(int)position.Y,
					(int)(Game1.TileSize * (1.5/2.5f) * 3f),
					(int)(Game1.TileSize * (1.5/2.5f) * 4f)));

			//Left arm
			_regularArmLeft = new Sprite(
				regArms,
				new Rectangle(
					0, 0,
					regArms.Width / 2, regArms.Height),
				new Rectangle(
					(int)position.X,
					(int)position.Y,
					(int)(Game1.TileSize * (.75/2.5f) * 3f),
					(int)(Game1.TileSize * (.75/2.5f) * 4f)));

			//Right arm
			_regularArmRight = new Sprite(
				regArms,
				new Rectangle(
					regArms.Width / 2, 0,
					regArms.Width / 2, regArms.Height),
				new Rectangle(
					(int)position.X + regArms.Width / 2,
					(int)position.Y,
					(int)(Game1.TileSize * (.75/2.5f) * 3f),
					(int)(Game1.TileSize * (.75/2.5f) * 4f)));

			//Set offset for right arm
			_rightArmOffset = new Vector2(
				Image.DestinationRect.Width - _regularArmRight.DestinationRect.Width,
				0);

			// Position
			WorldPosition = position;

			// Hitbox
			Hitbox = new Rectangle(
				(int)WorldPosition.X - 30,
				(int)WorldPosition.Y,
				150,
				250);

			// Default Speed
			Speed = 5f;

			// Set Vitality
			MaxHealth = 15;
			CurHealth = MaxHealth;
			InvDuration = 0.7;
			InvTimer = InvDuration;
			debugState = "idle";

			// Attacking
			_attackForce = 15f;
			_attackDuration = 0.2;
			_attackDurationTimer = 0.0;
			_attackRadius = Game1.TileSize * 2f;
			_attackRange = Game1.TileSize;
			_attackWindupDuration = 0.25;
			_attackWindupTimer = _attackWindupDuration;
			overheatTimer = 0.0;
			_attackCooldown = 1;
			_attackCooldownTimer = 0.0;
			pinBombsDuration = 10;
			pinBombsDurationTimer = pinBombsDuration;
			pinBombsDelay = 0;

			// Create punch arm sprites
			Texture2D gloveSpritesheet =
				gm.Content.Load<Texture2D>("Sprites/bossarmattack");

			_gloveFrameWidth = gloveSpritesheet.Width / 3;
			_gloveImages = new Sprite(
				gloveSpritesheet,
				new Rectangle(
					0, 0,
					_gloveFrameWidth, gloveSpritesheet.Height),
				new Rectangle(
					0, 0,
					(int)_attackRadius, (int)_attackRadius));

			_pinThrowWindUp = 1;
			aboutToThrow = false;
			stunTimer = 0.0;

			// Set type
			Type = EntityType.Enemy;
			
			// Set state
			_chaseRange = Game1.TileSize * 5;
			_aggroRange = Game1.TileSize * 3;

			ActionState = EnemyState.Idle;
			savedYdirection = 0;
			savedXdirection = 0;

			// Animation
			_walkAnimSecondsPerFrame = 0.12;
			pinBombsChanceModifier = 0;
			BossActionState = BossState.Idle;

			// Prep Post Mortem
			OnBossDeath += Game1.IManager.Clear;
		}

        // Methods

        public override void Update(GameTime gameTime)
		{
			TickInvincibility(gameTime);

			TickKnockout(gameTime);
			// DEBUG 

			//CollisionChecker.CheckTilemapCollision(this, Game1.TEST_ROOM.Floor);
			Random rng = new Random();
			Vector2 distanceFromPlayer = Game1.Player.CenterPosition - CenterPosition;
			float playerDist = distanceFromPlayer.Length();
			int roomWidth = Game1.Player.CurrentRoom.Tileset.Width;
			int roomHeight = Game1.Player.CurrentRoom.Tileset.Height;
			int originX = Game1.Player.CurrentRoom.Origin.X;
			int originY = Game1.Player.CurrentRoom.Origin.Y;

			DetermineState(playerDist);
			//BossActionState = BossState.HandSwipe;

			stunTimer -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;

			switch (BossActionState)
			{
				case BossState.Idle:
					Velocity = Vector2.Zero;
					_attackCooldownTimer -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
					debugState = "Idle";
					break;
				case BossState.Overheated:
					Velocity = Vector2.Zero;
					debugState = "Overheat";
					overheatTimer -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
					break;
				case BossState.PinBombs:
					Velocity = Vector2.Zero;
					debugState = "PinBombs";
					pinBombsDurationTimer -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
					if (pinBombsDurationTimer > 0)
					{
						pinBombsDurationTimer -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
						if(pinBombsDelay > 0)
						{
							pinBombsDelay -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
						}
						else
						{
							Game1.IManager.Add(new Indicator(Game1.Player.WorldPosition, BossActionState, 0)); 
							Game1.IManager.Add(new Indicator(new Vector2(rng.Next(originX + Game1.TileSize, originX + roomWidth - Game1.TileSize), rng.Next(originY + Game1.TileSize, originY + roomWidth - Game1.TileSize)), BossActionState, 0));
							pinBombsDelay = 0.2;
						}
					}
					break;
				case BossState.PinThrow:
					debugState = "Pinthrow";
					Velocity = Vector2.Zero;
					if (Math.Abs(distanceFromPlayer.X) > Math.Abs(distanceFromPlayer.Y))
					{
						if (distanceFromPlayer.X < 0)
						{
							savedXdirection = 1;
							Game1.IManager.Add(new Indicator(WorldPosition, BossActionState, 2));
						}
						else
						{
							savedXdirection = -1;
							Game1.IManager.Add(new Indicator(WorldPosition, BossActionState, 0));
						}
					}
					else
					{
						if (distanceFromPlayer.Y > 0)
						{
							savedYdirection = -1;
							Game1.IManager.Add(new Indicator(WorldPosition, BossActionState, 3));
						}
						else
						{
							savedYdirection = 1;
							Game1.IManager.Add(new Indicator(WorldPosition, BossActionState, 1));
						}
					}
					_pinThrowWindUp = 1;
					aboutToThrow = true;
					_attackCooldownTimer = 3;
					
					break;
				case BossState.HandSwipe:
					Velocity = Vector2.Zero;
					debugState = "HandSwipe";
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

			CheckEnemyCollisions();

			CollisionChecker.CheckTilemapCollision(this, Game1.TestLevel.CurrentRoom.Tileset);

			Move(Velocity * Player.BulletTimeMultiplier);

			// Update animations

			UpdateWalkAnimation(gameTime);
			if(aboutToThrow && _pinThrowWindUp < 0)
			{
				Game1.EManager.Enemies.Add(new PinAttack(savedXdirection, savedYdirection, new Vector2(WorldPosition.X, WorldPosition.Y), gm));
				aboutToThrow = false;
				savedXdirection = 0;
				savedYdirection = 0;
			}
			else if (aboutToThrow)
			{
				_pinThrowWindUp -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
			}
			
			return;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			
			// Draw Enemy relative to the player
			Vector2 distFromPlayer = WorldPosition - Game1.Player.WorldPosition;
			Vector2 screenPos = Game1.Player.ScreenPosition + distFromPlayer;

			// Only draw enemy if they are on screen
			Rectangle screenHit = new Rectangle(
				(int)screenPos.X,
				(int)screenPos.Y,
				Image.DestinationRect.Width,
				Image.DestinationRect.Height);

			bool onScreen = screenHit.Intersects(Game1.ScreenBounds);
			if (!onScreen) return;

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

			switch (BossActionState)
			{
				case BossState.Idle:

					Image.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					_regularArmLeft.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					_regularArmRight.Draw(spriteBatch, screenPos + _rightArmOffset, 0f, Vector2.Zero);
					break;

				case BossState.PinBombs:
					Image.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					_regularArmLeft.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					_regularArmRight.Draw(spriteBatch, screenPos + _rightArmOffset, 0f, Vector2.Zero);
					break;
				case BossState.Overheated:
					Image.TintColor = Color.Orange;
					Image.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					_regularArmLeft.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					_regularArmRight.Draw(spriteBatch, screenPos + _rightArmOffset, 0f, Vector2.Zero);
					break;
				case BossState.PinThrow:
					Image.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					_regularArmLeft.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					_regularArmRight.Draw(spriteBatch, screenPos + _rightArmOffset, 0f, Vector2.Zero);
					break;
				case BossState.HandSwipe:
					if (_attackWindupTimer < _attackWindupDuration && _attackDurationTimer <= 0.0)
					{
						Image.TintColor = Color.Orange;
					}

					Image.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					DrawAttacking(spriteBatch, screenPos);
					//_regularArmLeft.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					//_regularArmRight.Draw(spriteBatch, screenPos + _rightArmOffset, 0f, Vector2.Zero);
					break;
			}

			// Display current health
			//spriteBatch.DrawString(Game1.ARIAL32, $"Hp: {CurHealth}", screenPos, Color.White);

			// Display attack delay
		   /* spriteBatch.DrawString(
				gm.ARIAL32,
				$"Current State " + debugState,
				screenPos,
			   Color.White);*/
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
					Game1.Player.CurCore.Ricochet(directionToPlayer);
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
		protected override void DetermineState(float playerDist)
		{
			Random rng = new Random();
			if (BossActionState == BossState.Overheated)
			{
				if (overheatTimer > 0)
				{
					BossActionState = BossState.Overheated;
					
				}
				else
				{
				   BossActionState = BossState.Idle;
				}
			}
			else if (BossActionState != BossState.PinBombs)
			{
				if (_attackCooldownTimer <= 0 && stunTimer <= 0)
				{
					if (playerDist < _aggroRange)
					{
						BossActionState = BossState.HandSwipe;
						return;
					}
					else
					{

						if (rng.Next(1, 101) > pinBombsChanceModifier)
						{
							BossActionState = BossState.PinThrow;
							pinBombsChanceModifier += 5;
						}
						else
						{
							pinBombsDurationTimer = 10;
							BossActionState = BossState.PinBombs;
							pinBombsChanceModifier = 0;
						}
						EndAttack(true);
						return;
					}
				}
				else
				{
					BossActionState = BossState.Idle;
					return;
				}
			}
			else
			{
				if (pinBombsDurationTimer <= 0)
				{
					BossActionState = BossState.Overheated;
					overheatTimer = 10;
					pinBombsDurationTimer = 10;

				}
				else
				{
					BossActionState = BossState.PinBombs;
				}

				return;
			}

			return;
		}
		public override void TakeDamage(int damage)
		{
			// Take damage if not invincible
			if (IsInvincible) return;

			int damageToReceive = damage;

			if (BossActionState == BossState.Overheated)
			{
				damageToReceive *= 3;

				// Handle low health
				BossActionState = BossState.Idle;
				overheatTimer = 0;
				stunTimer = 1;
			}
			
			// Take damage
			CurHealth -= damageToReceive;

			if (CurHealth <= 0)
			{
				OnBossDeath();

				// Drop a new core
				Game1.PManager.CreatePickup(CenterPosition, PickupType.CurveCore);
				return;
			}

			// Become temporarily invincible
            InvTimer = InvDuration;

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



		private void DrawAttacking(SpriteBatch sb, Vector2 screenPos)
		{
			//This helps draw the non attacking arm.
			Vector2 attackDirectionOffset;
			SpriteEffects flipFX;
			//Determine Left/Right
			//Player is to the left
			if (_attackDirection.X < 0)
			{
				attackDirectionOffset = new Vector2(0, 0);
				flipFX = SpriteEffects.FlipVertically;
			}
			else
			{
				attackDirectionOffset = _rightArmOffset;
				flipFX = SpriteEffects.None;
			}

			_gloveImages.TintColor = Color.White;

			if (_attackWindupTimer < _attackWindupDuration && _attackDurationTimer <= 0d)
			{
				DrawSwingWindup(sb, screenPos, attackDirectionOffset, flipFX);
			}

			if (_attackDurationTimer > 0)
			{
				DrawSwingActive(sb, screenPos, attackDirectionOffset, flipFX);
			}
			
			return;
		}

		/// <summary>
		/// Draws the winding up fist.
		/// </summary>
		/// <param name="sb">Spritebatch for drawing.</param>
		/// <param name="screenPos">Position of boss relative to player.</param>
		/// <param name="attackDirectionOffset">Offset used for drawing not swinging arm.</param>
		/// <param name="flipFX">SpriteFX for flip, use SpriteEffects.None if no flip needed.</param>
		private void DrawSwingWindup(SpriteBatch sb, Vector2 screenPos,
			Vector2 attackDirectionOffset, SpriteEffects flipFX)
		{
			Vector2 windupScreenPos =
				CenterPosition - _attackDirection
				+ Game1.MainCamera.WorldToScreenOffset;

			// Shift position to top-left of glove image (for drawing)
			windupScreenPos -= new Vector2(_gloveFrameWidth / 2, _gloveFrameWidth / 2);

			// Select the correct glove image
			_gloveImages.SourceRect =
				new Rectangle(
					0, 0,
					_gloveFrameWidth, _gloveImages.SourceRect.Height);

			// Rotate fist so knuckles face away from player
			float windupDirAngle = MathF.Atan2(_attackDirection.Y, _attackDirection.X);

			// Glove changes colors right before attacking
			if (_attackWindupTimer < _attackWindupDuration * .3)
				_gloveImages.TintColor = Color.Red;


			//Draw regular hand- if spritefx is flipvertical that means right hand is attacking,
			//so draw inactive left hand
			if (flipFX == SpriteEffects.FlipVertically)
			{
				_regularArmLeft.Draw(sb, screenPos + attackDirectionOffset, 0f, Vector2.Zero);
			}
			else
			{
				_regularArmRight.Draw(sb, screenPos + attackDirectionOffset, 0f, Vector2.Zero);
			}

			//Draw attacking hand
			_gloveImages.Draw(
				sb,
				windupScreenPos,
				//0f,
				windupDirAngle,
				_gloveImages.SourceRect.Center.ToVector2(),
				flipFX);
		}

		/// <summary>
		/// Draws active swinging attack.
		/// </summary>
		/// <param name="sb">Spritebatch for drawing.</param>
		/// <param name="screenPos">Position of boss relative to player.</param>
		/// <param name="attackDirectionOffset">Offset used for drawing not swinging arm.</param>
		/// <param name="flipFX">SpriteFX for flip, use SpriteEffects.None if no flip needed.</param>
		private void DrawSwingActive(SpriteBatch sb, Vector2 screenPos,
			Vector2 attackDirectionOffset, SpriteEffects flipFX)
		{

			// Filter out attack not active
			// Draw actively attacking
			Vector2 attackScreenPos = CenterPosition + _attackDirection * 5f
				+ Game1.MainCamera.WorldToScreenOffset;

			// Rotate fist so knuckles face player
			float dirAngle = MathF.Atan2(_attackDirection.Y, _attackDirection.X);

			// Get proper glove image
			_gloveImages.SourceRect = new Rectangle(
				_gloveFrameWidth * 2,
				0,
				_gloveFrameWidth,
				_gloveFrameWidth);

			//Draw regular hand- if spritefx is flipvertical that means right hand is attacking,
			//so draw inactive left hand
			if (flipFX == SpriteEffects.FlipVertically)
			{
				_regularArmLeft.Draw(sb, screenPos + attackDirectionOffset, 0f, Vector2.Zero);
			}
			else
			{
				_regularArmRight.Draw(sb, screenPos + attackDirectionOffset, 0f, Vector2.Zero);
			}

			// Draw attacking hand.
			_gloveImages.Draw(
				sb,
				attackScreenPos,
				//0f,
				dirAngle,
				_gloveImages.SourceRect.Center.ToVector2(),
				flipFX);
			
		}
		#endregion
	}

}

