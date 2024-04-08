using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace Final_Game.Entity
{
	public enum PlayerState
	{
		Rolling,
		Walking
	}
	public class Player : Entity
	{
		#region Fields
		/// <summary>
		/// Texture for the arrow displayed when you
		/// hold down Mouse1.
		/// </summary>
		private Texture2D _launchArrowsTexture;
		/// <summary>
		/// Width of the Launch arrow sprite, in pixels.
		/// </summary>
		private int _launchArrowSpriteWidth;

		/// <summary>
		/// Speed of the player when walking.
		/// </summary>
		private float _walkSpeed;

		/// <summary>
		/// The speed at which the player loses speed
		/// when holding Mouse2.
		/// </summary>
		private float _brakeSpeed;
		/// <summary>
		/// How much friction there is applied to player
		/// as they freely roll around.
		/// </summary>
		private float _frictionMagnitude;

		private Sprite _smileSprite;
		/// <summary>
		/// TBD
		/// </summary>
		private float _smileSpeed;

		private float _transitionToWalkingSpeed;

		private bool _controllable = true;

		/// <summary>
		/// TIme left before combo resets.
		/// </summary>
		private double _comboResetDuration;

		// Time dilation
		double _timeTransitionDuration = 0.2;
		double _transitionTimeCounter = 0.2;
		float _normalTimeMultiplier = 1f;
		float _minTimeMultiplier = 0.3f;

		// Animation
		private double _rollFrameDuration = 1;
		private double _rollFrameTimeCounter;
		private int _curRollFrame = 1;
		private int _numRollFrames;
		private int _rollFrameWidth;
		private float _directionToFace;

		#endregion

		#region Properties
		public PlayerState State { get; private set; }
		public Vector2 ScreenPosition { get; private set; }

		/// <summary>
		/// Multiply any expression that uses Elapsed Seconds by this coefficient
		/// to scale it to bullet time
		/// </summary>
		public static float BulletTimeMultiplier { get; private set; } = 1f;

		public int Combo;

		public bool IsSmiling => Combo > 9;
		public bool ComboReward => Combo > 9;

		private float hitStopDuration = 0.2f;
		private float hitStopTimeRemaining = 0f;
		public bool canBeTriggered = true;
		private Enemy lastContactedEnemy = null;
		private Room CurrentRoom { get { return Game1.CurrentLevel.CurrentRoom; } }

        /// <summary>
        /// Amount of redirects the player has left to use.
        /// </summary>
        public int NumRedirects { get; private set; }

        /// <summary>
        /// The maxmium amount of redirects player can have
        /// </summary>
        public int MaxRedirects { get; private set; }

        public bool LaunchPrimed { 
			get => Game1.IsMouseButtonPressed(1) && NumRedirects > 0; 
		}

        #endregion

        public event EntityDamaged OnPlayerDamaged;
		public event EntityDying OnPlayerDeath;


		#region Constructor(s)

		public Player(Game1 gm, Vector2 worldPosition)
		{
			// Set images
			Texture2D playerSpritesheet = 
				gm.Content.Load<Texture2D>("Sprites/RollingBallSpritesheet2");
			
			// main sprite
			Image = new Sprite(playerSpritesheet,
				new Rectangle(0, 0, 120, 120),
				new Rectangle(0, 0, Game1.TileSize, Game1.TileSize));

			_numRollFrames = 7;
			_rollFrameWidth = 120;
			
			// smiling sprite
			_smileSprite = new Sprite(
				gm.Content.Load<Texture2D>("Sprites/SpookyBlueTeeth"),
				new Rectangle(0, 0, 120, 120),
				Image.DestinationRect);

			// launch arrow image
			_launchArrowsTexture = gm.Content.Load<Texture2D>("Sprites/LaunchArrowSpritesheet");

			// Set position on screen
			ScreenPosition = new Vector2(
				Game1.ScreenCenter.X - Image.DestinationRect.Width / 2,
				Game1.ScreenCenter.Y - Image.DestinationRect.Height / 2);

			int numLaunchArrows = 4;
			_launchArrowSpriteWidth = _launchArrowsTexture.Width / numLaunchArrows;

			// Set position in world
			WorldPosition = worldPosition;

			// Set hitbox
			Hitbox = new Rectangle(
				worldPosition.ToPoint() + new Point(3, 3),
				new Point(Image.DestinationRect.Width - 6, Image.DestinationRect.Height - 7));

			// Set movement vars
			Speed = 20f;
			_walkSpeed = 10f;
			_brakeSpeed = 0.2f;
			_frictionMagnitude = 0.01f;
			_transitionToWalkingSpeed = 1f;
			_smileSpeed = 48f;

			// Set default state
			State = PlayerState.Walking;

			//Set combo
			Combo = 0;
			// Give launches
			MaxRedirects = 3;
			NumRedirects = MaxRedirects + 1;

			// Health
			MaxHealth = 6;
			CurHealth = MaxHealth;
			InvDuration = 0.5;

			// Set attack vars
			Damage = 1;

			//Set Health
			MaxHealth = 6;
			CurHealth = MaxHealth;
		}
		#endregion
		
		#region Methods
		public override void Update(GameTime gameTime)
		{
			if (hitStopTimeRemaining > 0f)
			{
				hitStopTimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;
				return;
			}

			UpdateCombo(gameTime);

			if (_controllable) UpdateBulletTime(gameTime);

			TickInvincibility(gameTime);

			switch (State)
			{
				case PlayerState.Walking:

					if (_controllable) MoveWithKeyboard(Game1.CurKB);
					//Debug.WriteLine($"Current worldPos {WorldPosition}");
					// Reset Combo if too much time has passed since prev hit.
					break;

				case PlayerState.Rolling:
					ApplyFriction();

					if (_controllable) HandleBraking();

					// Transition to walking
					if (Velocity.Length() < 1f)
					{
						State = PlayerState.Walking;

						NumRedirects = MaxRedirects + 1;
					}
					break;
			}

			if (_controllable) HandleLaunch();

			//ApplyScreenBoundRicochet();

			CollisionChecker.CheckTilemapCollision(this, CurrentRoom.Tileset);

			if (Game1.CSManager.Scene != Cutscene.Tutorial)
				CheckEnemyCollisions();

			CheckPickupCollisions();

			Move(Velocity * BulletTimeMultiplier);

			UpdateRollAnimation(gameTime);
        }

		public override void Draw(SpriteBatch sb)
		{
			Vector2 screenPos = WorldPosition + Game1.MainCamera.WorldToScreenOffset;

			// Draw player image
			Image.Draw(
				sb, 
				screenPos + new Vector2(_rollFrameWidth, _rollFrameWidth) / 2.5f, 
				_directionToFace, 
				new Vector2(_rollFrameWidth, _rollFrameWidth) / 2f);

			// Draw player launch arrow
			if (_controllable && LaunchPrimed)
			{
				DrawLaunchArrow(sb, screenPos);
			}
		}

		#region Collision Handling Methods
		public override void OnHitTile(Tile tile, CollisionDirection colDir)
		{
			switch (tile.Type)
			{
				case TileType.Spike:

					TakeDamage(2);

					Image.TintColor = Color.LightGoldenrodYellow;
					break;

				case TileType.OpenDoor:
					//Return so no calculating placing
					//player on the open door.
					//Debug.WriteLine("HIT OPEN DOOR");

					TransferRoom(tile);
					NumRedirects = MaxRedirects;
					return;

				case TileType.Wall:
					break;
			}

			// Place self on part of tile that was hit
			PlaceOnHitEdge(tile, colDir);

			if (State == PlayerState.Rolling)
			{
				//Move(-Velocity);

				Ricochet(colDir);
			}
			else if (State == PlayerState.Walking)
			{
				Move(-Velocity * BulletTimeMultiplier);
			}

			base.OnHitTile(tile, colDir);
		}

		public override void OnHitEntity(Entity entity, CollisionDirection colDir)
		{
			switch (entity.Type)
			{
				case EntityType.Enemy:
					Enemy hitEnemy = (Enemy)entity;

					HandleEnemyCollision(hitEnemy);		
					break;

				case EntityType.Pickup:
					entity.TakeDamage(1);

					break;
			}
		}

		private void CheckEnemyCollisions()
		{
			for (int i = 0; i < Game1.EManager.Enemies.Count; i++)
			{
				Enemy curEnemy = Game1.EManager.Enemies[i];

				//Test if Hitstop needs to be applied.
				canBeTriggered = lastContactedEnemy != curEnemy;
				if (CollisionChecker.CheckEntityCollision(this, curEnemy) &&
					canBeTriggered &&
					State == PlayerState.Rolling)
				{
					TriggerHitStop();
					Game1.SManager.PlayHitSound();
					lastContactedEnemy = curEnemy;
					continue;
				}

				//Otherwise, hitstop doesn't need to be applied
				//and will be available to apply to the next enemy.
				canBeTriggered = true;
			}
			return;
		}

		/// <summary>
		/// Checks if pickups have been collided with.
		/// </summary>
		private void CheckPickupCollisions()
		{
			for (int i = 0; i < Game1.PManager.Pickups.Count; i++)
			{
				Entity currentPickup = Game1.PManager.Pickups[i];

				//Clear pickup.
				if (CollisionChecker.CheckEntityCollision(
					this, currentPickup))
				{
					Game1.PManager.PlayerCollided(currentPickup);
					//Decrement to correct for list shortening.
					i--;
				}
			}
			return;
		}

		private void HandleEnemyCollision(Enemy hitEnemy)
		{
			if (hitEnemy.IsInvincible)
			{
				return;
			}

			if (State == PlayerState.Rolling)
			{
				// Speed up
				Vector2 acc = Velocity;
				acc.Normalize();
				acc *= 0.25f;
				Accelerate(acc);

				// Get an extra redirect
				if (NumRedirects < MaxRedirects)
				{
					NumRedirects++; 
				}
				Combo++;
				_comboResetDuration = 5f;
				hitEnemy.TakeDamage(Damage);
				return;
			}

			// Player gets hit.
			// Check for high enough combo.
			if (ComboReward)
			{
				Combo = 0;
				return;
			}

			// Player gets knocked back if standing on top of enemy
			Vector2 distToEnemy = hitEnemy.CenterPosition - CenterPosition;
			distToEnemy.Normalize();
			distToEnemy *= -5;
			this.TakeDamage(1);               
		    Velocity = distToEnemy;	
			State = PlayerState.Rolling;
			return;
		}
	
		#endregion
	
		#region Movement Helper Methods
	
		/// <summary>
		/// Moves the player to the next room.
		/// </summary>
		/// <param name="tile">Door tile that the player touched.</param>
		private void TransferRoom(Tile tile)
		{
			switch (tile.DoorOrientation)
			{
				case "U":
					Game1.TestLevel.LoadRoomUsingOffset(new Point(-1, 0));
					Move(new Vector2(0, (CurrentRoom.Tileset.Rows - 3) * Game1.TileSize));
					break;
				case "B":
					Game1.TestLevel.LoadRoomUsingOffset(new Point(1, 0));
					Move(new Vector2(0, -(CurrentRoom.Tileset.Rows - 3) * Game1.TileSize));
					break;
				case "L":
					Game1.TestLevel.LoadRoomUsingOffset(new Point(0, -1));
					Move(new Vector2((CurrentRoom.Tileset.Columns - 3) * Game1.TileSize, 0));
					break;
				case "R":
					Game1.TestLevel.LoadRoomUsingOffset(new Point(0, 1));
					Move(new Vector2(-(CurrentRoom.Tileset.Columns - 3) * Game1.TileSize, 0));
					break;
			}
		}
	
		public void MoveWithKeyboard(KeyboardState kb)
		{
			Velocity = Vector2.Zero;
	
			// Move up
			if (kb.IsKeyDown(Keys.W))
			{
				Velocity = new Vector2(Velocity.X, Velocity.Y - _walkSpeed);
			}
			// Move down
			if (kb.IsKeyDown(Keys.S))
			{
				Velocity = new Vector2(Velocity.X, Velocity.Y + _walkSpeed);
			}
			// Move left
			if (kb.IsKeyDown(Keys.A))
			{
				Velocity = new Vector2(Velocity.X - _walkSpeed, Velocity.Y);
			}
			// Move right
			if (kb.IsKeyDown(Keys.D))
			{
				Velocity = new Vector2(Velocity.X + _walkSpeed, Velocity.Y);
			}
	
			// Max Velocity is _walkSpeed
			if (Velocity.LengthSquared() > _walkSpeed * _walkSpeed)
			{
				Velocity = Velocity * _walkSpeed / Velocity.Length();
			}
		}
	
		private void HandleLaunch()
		{
			if (Game1.IsMouseButtonPressed(1))
			{
				// Todo: Slow time
			}
	
			// Launch Player in direction of Mouse
			if (NumRedirects > 0 && Game1.IsMouseLeftClicked())
			{
				// Get mouse Position
				Vector2 mousePos = new Vector2(Game1.CurMouse.X, Game1.CurMouse.Y);
	
				// Aim from center of the Player
				Vector2 centerPos = new Vector2(ScreenPosition.X + Image.DestinationRect.Width / 2,
					ScreenPosition.Y + Image.DestinationRect.Height / 2);
	
				// Aim toward mouse at player speed
				Vector2 distance = mousePos - centerPos;
				distance.Normalize();
	
				// Speed is less than max
				if (Velocity.LengthSquared() < Speed * Speed)
				{
					// Launch player at max speed
					distance *= Speed;
					Velocity = distance;
				}
				else
				{
					// Launch player at current speed
					distance *= Velocity.Length();
					Velocity = distance;
				}
	
				// Launch Player at max speed
				// Redirect Player at cur speed
				//if (_numRedirects > _maxRedirects)
				//{
				//    // Launch player at default speed
				//    distance *= _speed;
				//    Velocity = distance;
				//}
				//else
				//{
				//    // Launch player at current speed
				//    distance *= Velocity.Length();
				//    Velocity = distance;
				//}
	
				NumRedirects--;
	
				// Player is now rolling
				State = PlayerState.Rolling;
			}
		}
	
		private void HandleBraking()
		{
			float lowestBrakableSpeed = 0.1f * 0.1f;
	
			if (Game1.IsMouseButtonPressed(2) && 
				Velocity.LengthSquared() >= lowestBrakableSpeed)
			{
				Vector2 deceleration = -Velocity;
				deceleration.Normalize();
				deceleration *= _brakeSpeed * BulletTimeMultiplier;
	
				Velocity += deceleration;
			}
		}
	
		public void Accelerate(Vector2 force)
		{
			Velocity += force;
		}
	
		private void ApplyFriction()
		{
			if (Velocity.LengthSquared() > MathF.Pow(_transitionToWalkingSpeed, 2))
			{
				// Naturally decelerate over time
				Vector2 natDeceleration = -Velocity;
				natDeceleration.Normalize();
				Velocity += natDeceleration * _frictionMagnitude * BulletTimeMultiplier;
			}
		}
		public void Ricochet(CollisionDirection hitDirection)
		{
			if (hitDirection == CollisionDirection.Vertical)
			{
				Velocity = new Vector2(Velocity.X, -Velocity.Y);
			}
			else if (hitDirection == CollisionDirection.Horizontal)
			{
				Velocity = new Vector2(-Velocity.X, Velocity.Y);
			}
	
			State = PlayerState.Rolling;
		}
	
		public void Ricochet(Vector2 newDirection)
		{
			Velocity = newDirection;
	
			State = PlayerState.Rolling;
		}
	
		private void ApplyScreenBoundRicochet()
		{
			if (WorldPosition.X + Image.DestinationRect.Width > Game1.WindowWidth ||
				WorldPosition.X <= 0)
			{
				Velocity = new Vector2(-Velocity.X, Velocity.Y);
			}
			if (WorldPosition.Y + Image.DestinationRect.Height > Game1.WindowHeight ||
				WorldPosition.Y <= 0)
			{
				Velocity = new Vector2(Velocity.X, -Velocity.Y);
			}
		}
	
		private void UpdateBulletTime(GameTime gameTime)
		{
			if (Game1.IsMouseButtonPressed(1) && NumRedirects > 0 && !CurrentRoom.Cleared)
			{
				// Transition from normal -> bullet time
				_transitionTimeCounter -= gameTime.ElapsedGameTime.TotalSeconds;
	
			}
			else if (Game1.IsMouseButtonReleased(1))
			{
				// Transition from bullet -> normal time
				_transitionTimeCounter += gameTime.ElapsedGameTime.TotalSeconds;
			}
	
			// Enforce time counter bounds
			_transitionTimeCounter = 
				MathHelper.Clamp((float)_transitionTimeCounter, 0, (float)_timeTransitionDuration);
	
			// Set time multiplier
			// Interpolates between minTimeMultiplier and normalTimeMultiplier
			// based on whether the transitionTimeCounter is counting up or down
			BulletTimeMultiplier =
				(float)(_minTimeMultiplier + _transitionTimeCounter / _timeTransitionDuration *
				(_normalTimeMultiplier - _minTimeMultiplier));
		}
	
		#endregion
	
		#region Drawing Helper Methods
		private void DrawLaunchArrow(SpriteBatch sb, Vector2 screenPos)
		{
			// Get angle between arrow and mouse
			Vector2 mousePos = new Vector2(Game1.CurMouse.X, Game1.CurMouse.Y);
	
			Vector2 centerScreenPos = new Vector2(
				screenPos.X + Image.DestinationRect.Width / 2,
				screenPos.Y + Image.DestinationRect.Height / 2);
	
			Vector2 playerToMouseDistance = mousePos - centerScreenPos;
	
			float angleBetweenArrowAndMouse = MathF.Atan2(
				playerToMouseDistance.X,
				playerToMouseDistance.Y);
	
			// Scale distance from player to mouse for drawing
			Vector2 directionFromPlayerToMouse = playerToMouseDistance;
			directionFromPlayerToMouse.Normalize();
			directionFromPlayerToMouse *= 120; // Radius
	
			Rectangle arrowSourceRect = new Rectangle();

			// Get correct launch arrow image from spritesheet
			int arrowNumber = MathHelper.Clamp(NumRedirects - 1, 0, 3);

            arrowSourceRect = new Rectangle(
				_launchArrowSpriteWidth * arrowNumber, 0,
				_launchArrowSpriteWidth, _launchArrowSpriteWidth);
	
			// Draw aiming arrow
			sb.Draw(
				_launchArrowsTexture,
				centerScreenPos + directionFromPlayerToMouse,
				arrowSourceRect,
				Color.White,
				-angleBetweenArrowAndMouse,
				new Vector2(
					_launchArrowSpriteWidth / 2,
					_launchArrowSpriteWidth / 2),
				1f,
				SpriteEffects.None,
				0f
				);
	
			//// Display remaining redirects
			//if (_numRedirects <= _maxRedirects)
			//{
			//	Vector2 redirectStringDimensions =
			//		UI.MediumArial.MeasureString(_numRedirects.ToString());
	
			//	Vector2 textPos = centerScreenPos + directionFromPlayerToMouse;
			//	textPos = new Vector2(
			//		textPos.X - redirectStringDimensions.X / 2,
			//		textPos.Y - redirectStringDimensions.Y / 2);
	
			//	sb.DrawString(
			//		UI.MediumArial,
			//		_numRedirects.ToString(),
			//		textPos,
			//		Color.White);
			//}
		}
	
	
		#endregion
	
		/// <summary>
		/// Resets all key variables
		/// </summary>
		public void Reset()
		{
			// Set animation to first frame
			_curRollFrame = 1;
			_rollFrameTimeCounter = 0;

			// Reset sprite orientation
            Image.SourceRect = new Rectangle(
                    0, 0, _rollFrameWidth, _rollFrameWidth);
			_directionToFace = 0f;

            // Restore Health
            CurHealth = MaxHealth;
	
			// Restore Redirects
			NumRedirects = MaxRedirects + 1;
	
			// Stop Moving
			Velocity = Vector2.Zero;
	
			// Set Default State 
			State = PlayerState.Walking;

			_controllable = true;

			// Reset combo
			Combo = 0;
		}
	
		private void UpdateCombo(GameTime gameTime)
		{
			if (_comboResetDuration > 0) 
			{
				//Debug.WriteLine("here");
				_comboResetDuration -= gameTime.ElapsedGameTime.TotalSeconds;
			}
			if (_comboResetDuration <= 0)
			{
				Combo = 0;
			}
			return;
		}

		private void UpdateRollAnimation(GameTime gameTime)
		{
			if (Velocity.LengthSquared() == 0)
			{
				_curRollFrame = 1;
				return;
			}

			double baseDuration = 0.2;
			_rollFrameDuration = 
				baseDuration * 
				Speed / (2 * Velocity.Length());

			// Count up time until next frame
			if (_rollFrameTimeCounter < _rollFrameDuration)
			{
				_rollFrameTimeCounter += 
					gameTime.ElapsedGameTime.TotalSeconds *
					BulletTimeMultiplier;
			}
			else
			{
				// Move to next frame and wrap
				// around when end of animation is reached
				if (_curRollFrame == _numRollFrames)
					_curRollFrame = 1;
				else
					_curRollFrame++;

				// Get correct frame

				int rollFrameY = 0;
				if (IsSmiling) rollFrameY = _rollFrameWidth;

                Image.SourceRect = new Rectangle(
                    _rollFrameWidth * (_curRollFrame - 1),
                    rollFrameY,
                    _rollFrameWidth,
                    _rollFrameWidth);

				// Face player toward their velocity
                _directionToFace =
                    MathF.Atan2(Velocity.Y, Velocity.X) + 3 * MathHelper.PiOver2;

                // Reset time counter
                _rollFrameTimeCounter -=
					gameTime.ElapsedGameTime.TotalSeconds;
			}
		}

		public override void TakeDamage(int amount)
		{

			// Early Exit Conditions
			if (ComboReward)
			{
				Combo = 0;
				return;
			}

			if (InvTimer > 0 || CurHealth <= 0 || InfiniteHealth) return;
				
			CurHealth -= amount;

			// Notify subscribers
			OnPlayerDamaged(amount);

			// Temporarily become invincible
			InvTimer = InvDuration;

			// Handle low health
			if (CurHealth <= 0)
			{
				_controllable = false;

				BulletTimeMultiplier = _minTimeMultiplier;

				OnPlayerDeath();
			}
			return;
		}

		public void TriggerHitStop()
		{
			hitStopTimeRemaining = hitStopDuration;
		}
	}
	#endregion
}

