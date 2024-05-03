using Final_Game.LevelGen;
using Final_Game.Managers;
using Final_Game.Pickups;
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
		/// Speed of the player to reload redirects
		/// </summary>
		private float _reloadRedirectsSpeed;

		/// <summary>
		/// The speed at which the player loses speed
		/// when holding Mouse2.
		/// </summary>
		private float _brakeSpeed;
		
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
		private int _curCoreIndex;
		private const int _maxCoreNum = 2;

		// Combo
		private Sprite _comboUsedSprite;

		#endregion

		#region Properties
		public PlayerState State { get; set; }
		public Vector2 ScreenPosition => WorldPosition + Game1.MainCamera.WorldToScreenOffset;

		/// <summary>
		/// Multiply any expression that uses Elapsed Seconds by this coefficient
		/// to scale it to bullet time
		/// </summary>
		public static float BulletTimeMultiplier { get; private set; } = 1f;

		public int Combo;
		public bool IsSmiling => Combo > 9;
		public bool ComboReward => Combo > 9;

		private float _hitStopDuration = 0.2f;
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

		// Cores
  
		public Core CurCore { get; private set; }

		/// <summary>
		/// The number of cores the player is currently holding
		/// </summary>
		public int NumCores => _cores.Count;

        public override Vector2 Velocity { get => CurCore.Velocity; }
        public float MinRollSpeed { get; private set; }

		public bool Controllable { get; private set; } = true;

		// Combo
		public bool ComboUseVisualizationOn { get; private set; }

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
            //Cloning
            _gm = gm;

            // Set images
            Texture2D playerSpritesheet = 
				gm.Content.Load<Texture2D>("Sprites/RollingBallSpritesheet2");
			
			// main sprite
			Image = new Sprite(playerSpritesheet,
				new Rectangle(0, 0, 120, 120),
				new Rectangle(0, 0, Game1.TileSize, Game1.TileSize));

			_numRollFrames = 7;
			_rollFrameWidth = 120;

			// Set position in world
			WorldPosition = worldPosition;

			// Set hitbox
			Hitbox = new Rectangle(
				worldPosition.ToPoint() + new Point(3, 3),
				new Point(Image.DestinationRect.Width - 6, Image.DestinationRect.Height - 7));

			// Set movement vars
			Speed = 20f;
			
			_brakeSpeed = 0.2f;
			MinRollSpeed = 1f;
			_reloadRedirectsSpeed = 5f;

			// Set default state
			State = PlayerState.Walking;

			//Set combo
			Combo = 0;
			Texture2D comboUsedTexture = gm.Content.Load<Texture2D>("Sprites/ComboUsedBall");
			_comboUsedSprite = new Sprite(
				comboUsedTexture,
				comboUsedTexture.Bounds,
				Image.DestinationRect);

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

			// Player starts with only the default core
			_cores = new List<Core>()
			{
				new Core(_gm.Content)
			};
			_curCoreIndex = 0;
			CurCore = _cores[_curCoreIndex];
		}
		#endregion
		
		#region Methods
		public override void Update(GameTime gameTime)
		{
			if (hitStopTimeRemaining > 0f)
			{
				hitStopTimeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds;

				if (hitStopTimeRemaining <= 0) ComboUseVisualizationOn = false;
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
                    if (playerSpeed < 1f && !(CurCore.UsesCurve && CurCore.IsCurving))
                    {
                        State = PlayerState.Walking;

                        Velocity = Vector2.Zero;

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

            if (ComboUseVisualizationOn)
            {
                _comboUsedSprite.Draw(
                    sb,
                    screenPos + new Vector2(_rollFrameWidth, _rollFrameWidth) / 2.5f,
                    _directionToFace,
                    new Vector2(120f, 120f) / 2f);
				_comboUsedSprite.AlphaMultiplier = 0.8f;
            }

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
				//Move(-Velocity * BulletTimeMultiplier);
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
					//Remove the pickup.
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

				TriggerHitStop(_hitStopDuration);

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
            _curCoreIndex++;

			if (_curCoreIndex == _cores.Count)
			{
				// Wrap to beginning of storage
				_curCoreIndex = 0;
			}

			// Maintain velocity from one core to the next
			_cores[_curCoreIndex].Velocity = CurCore.Velocity;

			// Set Current Core
			CurCore = _cores[_curCoreIndex];
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

			// Start with one core
			_cores.Clear();
			_curCoreIndex = 0;
			_cores.Add(new Core(_gm.Content));
			CurCore = _cores[_curCoreIndex];
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

		/// <summary>
		/// Replaces the current core the player is using with the
		/// specified core
		/// </summary>
		/// <param name="additionalCore"> new core to use </param>
		public bool AddCore(Core additionalCore)
		{
			// Cannot have two of the same core
			if (!CanAddCore(additionalCore.Name)) return false;

            // Stop any special motion
            CurCore.StopCurving();

            // Maintain velocity from one core to the next
            additionalCore.Velocity = CurCore.Velocity;

			// Replace current core if at max capacity
            if (_cores.Count == _maxCoreNum)
			{
                _cores[_curCoreIndex] = additionalCore;
            }
			else
			{
				// Add core to storage
				_cores.Add(additionalCore);
				_curCoreIndex++;
			}

			// Start using new core
			CurCore = _cores[_curCoreIndex];

			// Get all redirects back
			NumRedirects = MaxRedirects;

			// Visually Notify Player that they
			// received a new core
			_gm.UIManager.StartCoreFlash();

			return true;
		}

		/// <summary>
		/// Gets whether or not player is already holding
		/// a core with the specified name
		/// </summary>
		/// <param name="coreName"></param>
		/// <returns></returns>
		private bool CanAddCore(string coreName)
		{
			foreach (Core c in _cores)
			{
				if (c != null && c.ToString() == coreName)
				{
					return false;
				}
			}

			return true;
		}

		public override void TakeDamage(int amount)
		{

			if(OnPlayerHit != null) OnPlayerHit(0);

			// Early Exit Conditions
			if (ComboReward)
			{
				Combo = 0;

				_gm.UIManager.StartActiveComboIcon();

				ComboUseVisualizationOn = true;

				TriggerHitStop(_hitStopDuration * 2);
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

		private void TriggerHitStop(float duration)
		{
			hitStopTimeRemaining = duration;
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

