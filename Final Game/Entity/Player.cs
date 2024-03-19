using Final_Game.LevelGen;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


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
		private Texture2D _launchArrowsTexture;
		private int _launchArrowSpriteWidth;

		private int _numRedirects;
		private int _maxRedirects;

		private float _walkSpeed;

		private float _brakeSpeed;
		private float _frictionMagnitude;

		private Sprite _smileSprite;
		private float _smileSpeed;

		private float _transitionToWalkingSpeed;

		// Time dilation
		double _timeTransitionDuration = 0.2;
		double _transitionTimeCounter = 0.2;
		float _normalTimeMultiplier = 1f;
		float _minTimeMultiplier = 0.3f;

		#endregion

		#region Properties
		public PlayerState State { get; private set; }
		public Vector2 ScreenPosition { get; private set; }

		/// <summary>
		/// Multiply any expression that uses Elapsed Seconds by this coefficient
		/// to scale it to bullet time
		/// </summary>
		public static float BulletTimeMultiplier { get; private set; } = 1f;

		private Room CurrentRoom { get { return Game1.TestLevel.CurrentRoom; } }

		/// <summary>
		/// Gets whether or not player is moving fast enough for
		/// their sprite to change to smiling
		/// </summary>
		public bool IsSmiling => 
			Velocity.LengthSquared() >= _smileSpeed * _smileSpeed;

		#endregion

		public event EntityDamaged OnPlayerDamaged;

		// Constructors
		public Player(Game1 gm, Vector2 worldPosition)
		{
			// Set images
			Texture2D playerSprite = gm.Content.Load<Texture2D>("Sprites/BasicBlueClean");
			
			// main sprite
			Image = new Sprite(playerSprite,
				new Rectangle(0, 0, 120, 120),
				new Rectangle(0, 0, Game1.TileSize, Game1.TileSize));
			
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

			int numLaunchArrows = 2;
			_launchArrowSpriteWidth = _launchArrowsTexture.Width / numLaunchArrows;

			// Set position in world
			WorldPosition = worldPosition;

			// Set hitbox

			Hitbox = new Rectangle(worldPosition.ToPoint(), new Point(Image.DestinationRect.Width, Image.DestinationRect.Height));

			// Set movement vars
			Speed = 20f;
			_walkSpeed = 10f;
			_brakeSpeed = 0.2f;
			_frictionMagnitude = 0.01f;
			_transitionToWalkingSpeed = 1f;
			_smileSpeed = 48f;

			// Set default state
			State = PlayerState.Walking;

			// Give launches
			_maxRedirects = 3;
			_numRedirects = _maxRedirects + 1;

			// Health
			MaxHealth = 6;
			CurHealth = MaxHealth;
			InvDuration = 0.5;

			// Set attack vars
			Damage = 1;
		}

		// Methods
		public override void Update(GameTime gameTime)
		{
			UpdateBulletTime(gameTime);

			TickInvincibility(gameTime);

			switch (State)
			{
				case PlayerState.Walking:
					MoveWithKeyboard(Game1.CurKB);
					//Debug.WriteLine($"Current worldPos {WorldPosition}");
					break;

				case PlayerState.Rolling:
					ApplyFriction();

					HandleBraking();

					// Transition to walking
					if (Velocity.Length() < 1f)
					{
						State = PlayerState.Walking;

						_numRedirects = _maxRedirects + 1;
					}
					break;
			}

			HandleLaunch();

			//ApplyScreenBoundRicochet();

			CollisionChecker.CheckTilemapCollision(this, CurrentRoom.Tileset);

			HandleEnemyCollisions();

			Move(Velocity * BulletTimeMultiplier);
		}
		public override void Draw(SpriteBatch sb)
		{
			// Draw player image
			if (!IsSmiling)
				Image.Draw(sb, ScreenPosition);
			else
				_smileSprite.Draw(sb, ScreenPosition);

			// Draw player launch arrow
			if (_numRedirects > 0 && 
				Game1.IsMouseButtonPressed(1))
			{
				DrawLaunchArrow(sb);
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
					Debug.WriteLine("HIT OPEN DOOR");
					TransferRoom(tile);
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
				Move(-Velocity);
			}

			base.OnHitTile(tile, colDir);
		}

		public override void OnHitEntity(Entity entity, CollisionDirection colDir)
		{
			switch (entity.Type)
			{
				case EntityType.Enemy:
					if (State != PlayerState.Walking)
					{
						if (!entity.IsInvincible)
						{
							// Speed up
							Vector2 acc = Velocity;
							acc.Normalize();
							acc *= 0.05f;
							Accelerate(acc);

							// Get an extra redirect
							if (_numRedirects < _maxRedirects)
								_numRedirects++;
						}

						entity.TakeDamage(Damage);
					}
					else
					{
						// Player gets knocked back if standing on top of enemy
						Vector2 distToEnemy = entity.CenterPosition - CenterPosition;
						distToEnemy.Normalize();
						distToEnemy *= -5;

						this.TakeDamage(1);

						Velocity = distToEnemy;
						State = PlayerState.Rolling;
					}
					break;
			}
		}

		private void HandleEnemyCollisions()
		{
			for (int i = 0; i < Game1.EManager.Enemies.Count; i++)
			{
				Enemy curEnemy = Game1.EManager.Enemies[i];

				CollisionChecker.CheckEntityCollision(this, curEnemy);
			}
		}

	#endregion

		#region Movement Helper Methods

		private void TransferRoom(Tile tile)
		{
			switch (tile.DoorOrientation)
			{
				case "U":
					Game1.TestLevel.CurrentPoint += new Point(-1, 0);
					Move(new Vector2(0, (CurrentRoom.Tileset.Rows - 3) * Game1.TileSize));
					break;
				case "B":
					Game1.TestLevel.CurrentPoint += new Point(1, 0);
					Move(new Vector2(0, -(CurrentRoom.Tileset.Rows - 3) * Game1.TileSize));
					break;
				case "L":
					Game1.TestLevel.CurrentPoint += new Point(0, -1);
					Move(new Vector2((CurrentRoom.Tileset.Columns - 3) * Game1.TileSize, 0));
					break;
				case "R":
					Game1.TestLevel.CurrentPoint += new Point(0, 1);
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
			if (_numRedirects > 0 && Game1.IsMouseLeftClicked())
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
				if (Velocity.LengthSquared() <= Speed * Speed)
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

				_numRedirects--;

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
            if (Game1.IsMouseButtonPressed(1) && _numRedirects > 0)
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
        private void DrawLaunchArrow(SpriteBatch sb)
		{
			// Get angle between arrow and mouse
			Vector2 mousePos = new Vector2(Game1.CurMouse.X, Game1.CurMouse.Y);

			Vector2 centerScreenPos = new Vector2(
				ScreenPosition.X + Image.DestinationRect.Width / 2,
				ScreenPosition.Y + Image.DestinationRect.Height / 2);

			Vector2 playerToMouseDistance = mousePos - centerScreenPos;

			float angleBetweenArrowAndMouse = MathF.Atan2(
				playerToMouseDistance.X,
				playerToMouseDistance.Y);

			// Scale distance from player to mouse for drawing
			Vector2 directionFromPlayerToMouse = playerToMouseDistance;
			directionFromPlayerToMouse.Normalize();
			directionFromPlayerToMouse *= 120; // Radius

			Rectangle arrowSourceRect = new Rectangle();

			if (_numRedirects > _maxRedirects)
			{
				// Use Launch Arrow Source Rect
				arrowSourceRect = new Rectangle(
					_launchArrowSpriteWidth, 0,
					_launchArrowSpriteWidth, _launchArrowSpriteWidth);
			}
			else
			{
				// Use Redirect Arrow Source Rect
				arrowSourceRect = new Rectangle(
					0, 0,
					_launchArrowSpriteWidth, _launchArrowSpriteWidth);
			}

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

			// Display remaining redirects
			if (_numRedirects <= _maxRedirects)
			{
				Vector2 redirectStringDimensions =
					UI.MediumArial.MeasureString(_numRedirects.ToString());

				Vector2 textPos = centerScreenPos + directionFromPlayerToMouse;
				textPos = new Vector2(
					textPos.X - redirectStringDimensions.X / 2,
					textPos.Y - redirectStringDimensions.Y / 2);

				sb.DrawString(
					UI.MediumArial,
					_numRedirects.ToString(),
					textPos,
					Color.White);
			}
		}

		public override void DrawGizmos()
		{
			// Draw hitbox
			Color fadedRed = new Color(1f, 0f, 0f, 0.4f);

			Vector2 hitboxDistFromPlayer =
				new Vector2(
					WorldPosition.X - Hitbox.X,
					WorldPosition.Y - Hitbox.Y);

			Rectangle hitboxInScreenSpace =
				new Rectangle(
					(int)(ScreenPosition.X + hitboxDistFromPlayer.X),
					(int)(ScreenPosition.Y + hitboxDistFromPlayer.Y),
					Hitbox.Width,
					Hitbox.Height);

			ShapeBatch.Box(hitboxInScreenSpace, fadedRed);
		}

		#endregion

		/// <summary>
		/// Resets all key variables
		/// </summary>
		public void Reset()
		{
			// Restore Health
			CurHealth = MaxHealth;

			// Restore Redirects
			_numRedirects = _maxRedirects + 1;

			// Stop Moving
			Velocity = Vector2.Zero;

			// Set Default State 
			State = PlayerState.Walking;

			// Set player at the center of the current level
			WorldPosition = new Vector2(
				Game1.TestLevel.CurrentRoom.Tileset.Width / 2,
				Game1.TestLevel.CurrentRoom.Tileset.Height / 2);
		}

        public override void TakeDamage(int amount)
        {
            // Take damage if not invincible
            if (InvTimer <= 0)
            {
                CurHealth -= amount;

				OnPlayerDamaged(amount);

                // Temporarily become invincible
                InvTimer = InvDuration;

                // Handle low health
                if (CurHealth <= 0)
                {
					Die();
                }
            }
        }
    }
}
