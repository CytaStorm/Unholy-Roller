using Final_Game.LevelGen;
using Final_Game.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
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
		/// Speed of the player to reload redirects
		/// </summary>
		private float _reloadRedirectsSpeed;

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

		

		private Game1 _gm;

		// Cores
		private List<Core> _cores;
		private int _coreIndex;

		#endregion

		#region Properties
		public PlayerState State { get; private set; }
		public Vector2 ScreenPosition => WorldPosition + Game1.MainCamera.WorldToScreenOffset;

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


		public Room CurrentRoom { get { return Game1.CurrentLevel.CurrentRoom; } }

        /// <summary>
        /// Amount of redirects the player has left to use.
        /// </summary>
        public int NumRedirects { get; private set; }

        /// <summary>
        /// The maxmium amount of redirects player can have
        /// </summary>
        public int MaxRedirects { get; private set; }

        public bool LaunchPrimed { 
			get => Game1.IsMouseButtonPressed(LaunchButton) && NumRedirects > 0; 
		}

		// Curve Core
  
		public Core CurCore { get; private set; }
		

        public override Vector2 Velocity { get => CurCore.Velocity; }
        public float MinRollSpeed { get; private set; }

		public bool Controllable { get; private set; } = true;

        // Keybinds
        public int LaunchButton { get; set; } = 1;
        public int BrakeButton { get; set; } = 2;

        #endregion

        public event EntityDamaged OnPlayerHit;
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
			
			_brakeSpeed = 0.2f;
			_frictionMagnitude = 0.01f;
			MinRollSpeed = 1f;
			_smileSpeed = 48f;
			_reloadRedirectsSpeed = 5f;

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

			//Cloning
			_gm = gm;

			// Cores
			_cores = new List<Core>()
			{
				new Core(_gm.Content),
				new Core_ThreePointCurve(_gm.Content)
			};
			CurCore = _cores[_coreIndex];
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

			if (Controllable) UpdateBulletTime(gameTime);

			TickInvincibility(gameTime);

			HandleCoreSwap();

			switch (State)
			{
				case PlayerState.Walking:

					if (Controllable) CurCore.MoveWithKeyboard(Game1.CurKB);
					//Debug.WriteLine($"Current worldPos {WorldPosition}");
					// Reset Combo if too much time has passed since prev hit.
					break;

				case PlayerState.Rolling:

                    if (Controllable) HandleBraking();

					//CalculateNextCurvePoint(gameTime);
					CurCore.Update(gameTime);

                    float playerSpeed = Velocity.Length();

                    // Quick reload redirects
                    if (playerSpeed < _reloadRedirectsSpeed)
                    {
                        NumRedirects = MaxRedirects;
                    }

                    // Transition to walking
                    if (playerSpeed < 1f)
                    {
                        //State = PlayerState.Walking;

                        //Velocity = Vector2.Zero;

                        NumRedirects = MaxRedirects + 1;
                    }
                    break;
            }

			if (Controllable) HandleLaunch(gameTime);

			CollisionChecker.CheckTilemapCollision(this, CurrentRoom.Tileset);

			CheckEnemyCollisions();

			CheckPickupCollisions();

			if (CurCore.IsCurving && CurCore.UsesCurve) 
				Move(Velocity);
			else 
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
		}

		#region Collision Handling Methods
		public override void OnHitTile(Tile tile, CollisionDirection colDir)
		{
			switch (tile.Type)
			{
				case TileType.Spike:

					TakeDamage(2);
					break;

				case TileType.OpenDoor:
					//Return so no calculating placing
					//player on the open door.
					//Debug.WriteLine("HIT OPEN DOOR");

					TransferRoom(tile);
					NumRedirects = MaxRedirects;

					return;
			}

			// Place self on part of tile that was hit
			PlaceOnHitEdge(tile, colDir);

			if (State == PlayerState.Walking)
			{
				Move(-Velocity * BulletTimeMultiplier);
			}

            CurCore.OnHitTile(colDir, tile);
        }

		public override void OnHitEntity(Entity entity, CollisionDirection colDir)
		{
			switch (entity.Type)
			{
				case EntityType.Enemy:
					Enemy hitEnemy = (Enemy)entity;

					// Exit early if enemy can't take damage
                    if (hitEnemy.IsInvincible) 
						return;

					HandleEnemyCollision(hitEnemy);		
					break;

				case EntityType.Pickup:
					entity.TakeDamage(1);

					break;
			}

			// Movement-related reaction to collision
			CurCore.OnHitEntity(colDir, entity);
		}

		private void CheckEnemyCollisions()
		{
			for (int i = 0; i < Game1.EManager.Enemies.Count; i++)
			{
				Enemy curEnemy = Game1.EManager.Enemies[i];

				CollisionChecker.CheckEntityCollision(this, curEnemy);
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
			if (State == PlayerState.Rolling)
			{
				Managers.SoundManager.PlayHitSound();

				// Get an extra redirect
				if (NumRedirects < MaxRedirects)
				{
					NumRedirects++;
				}

				// Increase Combo
				Combo++;
				_comboResetDuration = 5f;

				TriggerHitStop();

				// Deal damage to enemy
				hitEnemy.TakeDamage(Damage);
				return;
			}

			// Take damage if hit enemy when not rolling
			TakeDamage(1);
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
					Game1.CurrentLevel.LoadRoomUsingOffset(new Point(-1, 0));
					Move(new Vector2(0, (CurrentRoom.Tileset.Rows - 3) * Game1.TileSize));
					break;
				case "B":
					Game1.CurrentLevel.LoadRoomUsingOffset(new Point(1, 0));
					Move(new Vector2(0, -(CurrentRoom.Tileset.Rows - 3) * Game1.TileSize));
					break;
				case "L":
					Game1.CurrentLevel.LoadRoomUsingOffset(new Point(0, -1));
					Move(new Vector2((CurrentRoom.Tileset.Columns - 3) * Game1.TileSize, 0));
					break;
				case "R":
					Game1.CurrentLevel.LoadRoomUsingOffset(new Point(0, 1));
					Move(new Vector2(-(CurrentRoom.Tileset.Columns - 3) * Game1.TileSize, 0));
					break;
			}
		}

        private void HandleCoreSwap()
        {
            if (!Game1.SingleKeyPress(Keys.Q)) return;

			CurCore.StopCurving();

			// Get Next Core
            _coreIndex++;
            if (_coreIndex == _cores.Count)
            {
                _coreIndex = 0;
            }

			// Maintain velocity from one core to the next
			_cores[_coreIndex].Velocity = CurCore.Velocity;

			// Set Current Core
			CurCore = _cores[_coreIndex];
        }

        private void HandleLaunch(GameTime gameTime)
        {
			if (Game1.IsMouseButtonPressed(LaunchButton) && !CurCore.IsCurving) 
				CurCore.CalculateTrajectory();

            // Launch Player in direction of Mouse
            if (NumRedirects > 0 && Game1.IsMouseButtonClicked(LaunchButton))
            {
				//// Get mouse Position
				//Vector2 mousePos = new Vector2(Game1.CurMouse.X, Game1.CurMouse.Y);

				//// Aim from center of the Player
				//Vector2 centerPos = new Vector2(ScreenPosition.X + Image.DestinationRect.Width / 2,
				//    ScreenPosition.Y + Image.DestinationRect.Height / 2);

				//// Aim toward mouse at player speed
				//Vector2 distance = mousePos - centerPos;
				//distance.Normalize();

				////// Speed is less than max
				////if (Velocity.LengthSquared() < Speed * Speed)
				////{
				////	// Launch player at max speed
				////	distance *= Speed;
				////	Velocity = distance;
				////}
				////else
				////{
				////	// Launch player at current speed
				////	distance *= Velocity.Length();
				////	Velocity = distance;
				////}

				CurCore.Launch(gameTime);

                NumRedirects--;

                // Player is now rolling
                State = PlayerState.Rolling;
            }
        }

        private void HandleBraking()
		{
			float lowestBrakableSpeed = 0.1f * 0.1f;
	
			if (Game1.IsMouseButtonPressed(BrakeButton) && 
				CurCore.Velocity.LengthSquared() >= lowestBrakableSpeed)
			{
				Vector2 deceleration = -CurCore.Velocity;
				deceleration.Normalize();
				deceleration *= _brakeSpeed * BulletTimeMultiplier;
	
				CurCore.Velocity += deceleration;
			}
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
			if (Game1.IsMouseButtonPressed(LaunchButton) && NumRedirects > 0 && !CurrentRoom.Cleared)
			{
				// Transition from normal -> bullet time
				_transitionTimeCounter -= gameTime.ElapsedGameTime.TotalSeconds;
	
			}
			else if (Game1.IsMouseButtonReleased(LaunchButton))
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

			Controllable = true;

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

			if(OnPlayerHit != null) OnPlayerHit(0);

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
				// Player can't die if in tutorial
				if (Game1.CurrentLevel == Game1.TutorialLevel)
				{
					CurHealth = MaxHealth;
					return;
				}

				Controllable = false;

				BulletTimeMultiplier = _minTimeMultiplier;

				SoundManager.PlayDeathSound();

				OnPlayerDeath();
			}

			SoundManager.PlayTakeDamageSound();
			return;
		}

		private void TriggerHitStop()
		{
			hitStopTimeRemaining = hitStopDuration;
		}

		public void ToggleLeftHandMouse()
		{
			int temp = LaunchButton;
			LaunchButton = BrakeButton;
			BrakeButton = temp;
		}

		/// <summary>
		/// Returns a clone of the player.
		/// </summary>
		/// <returns>Clone of the player.</returns>
		public Player Clone()
		{
			return new Player(_gm, this.WorldPosition);
		}
    }
	#endregion
}

