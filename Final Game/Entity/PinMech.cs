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
		// Constructors
		public PinMech(Game1 gm, Vector2 position)
			: base(gm, position)
		{
			// Set Enemy Image
			Texture2D bossSprite =
				gm.Content.Load<Texture2D>("Sprites/Boss");
			circle = gm.Content.Load<Texture2D>("RedCircle");
			this.gm = gm;
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
			MaxHealth = 3;
			CurHealth = MaxHealth;
			InvDuration = 0.5;
			InvTimer = InvDuration;
			debugState = "idle";

			// Attacking
			_attackForce = 15f;
			_attackDuration = 0.2;
			_attackDurationTimer = 0.0;
			_attackRadius = Game1.TileSize;
			_attackRange = Game1.TileSize;
			_attackWindupDuration = 0.25;
			_attackWindupTimer = _attackWindupDuration;
			overheatTimer = 0.0;
			_attackCooldown = 1;
			_attackCooldownTimer = 0.0;
			pinBombsDuration = 10;
			pinBombsDurationTimer = pinBombsDuration;
			pinBombsDelay = 0;
			Texture2D gloveSpritesheet =
				gm.Content.Load<Texture2D>("Sprites/PinPunch2");

			_gloveFrameWidth = gloveSpritesheet.Width / 5;
			_gloveImages = new Sprite(
				gloveSpritesheet,
				new Rectangle(
					0, 0,
					_gloveFrameWidth, gloveSpritesheet.Height),
				new Rectangle(
					0, 0,
					(int)_attackRadius, (int)_attackRadius));
			indicators = new IndicatorManager(gm);
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
			return;
		}

		// Methods

		public override void Update(GameTime gameTime)
		{
			TickInvincibility(gameTime);

			TickKnockout(gameTime);

			//CollisionChecker.CheckTilemapCollision(this, Game1.TEST_ROOM.Floor);
			Random rng = new Random();
			Vector2 distanceFromPlayer = Game1.Player.CenterPosition - CenterPosition;
			float playerDist = distanceFromPlayer.Length();
			int roomWidth = Game1.Player.CurrentRoom.Tileset.Width;
			int roomHeight = Game1.Player.CurrentRoom.Tileset.Height;
			int originX = Game1.Player.CurrentRoom.Origin.X;
			int originY = Game1.Player.CurrentRoom.Origin.Y;

				DetermineState(playerDist);

			stunTimer -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
			Game1.IManager.Update(gameTime);

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
					break;

				case BossState.PinBombs:

					Image.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					break;
				case BossState.Overheated:
					Image.TintColor = Color.Orange;
					Image.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					break;
				case BossState.PinThrow:
					Image.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);
					break;
				case BossState.HandSwipe:
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
			if (InvTimer <= 0 && BossActionState == BossState.Overheated)
			{
				CurHealth -= damage;

				// Temporarily become invincible
				InvTimer = InvDuration;

				// Handle low health
				BossActionState = BossState.Idle;
				overheatTimer = 0;
				stunTimer = 1;
			}
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

			// Draw Attack Windup
			if (_attackWindupTimer < _attackWindupDuration && _attackDurationTimer <= 0d)
			{
				_gloveImages.TintColor = Color.White;

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

				// Glove changes colors right before attacking
				if (_attackWindupTimer < _attackWindupDuration * .3)
					_gloveImages.TintColor = Color.Red;

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

				Vector2 attackScreenPos = CenterPosition + _attackDirection * 5f
					+ Game1.MainCamera.WorldToScreenOffset;

				// Rotate fist so knuckles face player
				float dirAngle = MathF.Atan2(_attackDirection.Y, _attackDirection.X);

				// Get proper glove image
				_gloveImages.SourceRect = new Rectangle(
					_gloveFrameWidth * 4,
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

