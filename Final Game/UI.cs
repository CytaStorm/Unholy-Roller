using Final_Game;
using Final_Game.Entity;
using Final_Game.LevelGen;
using Final_Game.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Final_Game
{
	public class UI
	{
		#region Fields
		// Backgrounds
		private Texture2D _titleBackground;
		private Texture2D _pauseBackground;
		private Texture2D _gameOverBackground;

		// Management
		private Game1 _gm;
		private SpriteBatch _spriteBatch;

		// Sliders
		public Slider _testSlider;

		// Backgrounds
		private Texture2D _blankPanel;

		// Player Health
		private Texture2D _blueBallSpritesheet;
		private int _brokenBallSpriteWidth;

		// Player Speed
		private Texture2D _speedometerPin;
		private Texture2D _speedometerCrest;
		private float _maxSpeedometerSpeed;

		// Combo
		private Texture2D _comboIcon;
		private Texture2D _shieldIcon;

		// Shake Effect
		private double _shakeDuration;
		private double _shakeTimer;
		private Vector2 _maxShakeOffset;
		private float _maxShakeMagnitude;
		private float _maxShakeMultiplier;

		// Core Flash Effect
		private double _flashDuration = 0.5;
		private double _flashTimeCounter;
		private int _flashesToDo = 4;
		private int _completedFlashes;


		// Game Over Assets
		private Sprite _deadBall;
		private float _maxHoverOffset;
		private float _hoverOffset;
		private double _hoverDuration;
		private double _hoverTimeCounter;
		private bool hoverUp = true;

		// Minimap
		private int _defaultRoomSize = 16;
		private float _minimapScale = 0.1f;
		private Point _minimapPos;
		#endregion

		// Button Cursor
		private Texture2D _buttonCursor;

		#region Properties
		// Button Containers
		public Button[] MenuButtons { get; private set; }
		public Button[] PauseButtons { get; private set; }
		public Button[] GameOverButtons { get; private set; }

		// Fonts
		public static SpriteFont TitleCaseCarter { get; private set; }
		public static SpriteFont MediumCarter { get; private set; }
		public static SpriteFont MediumArial { get; private set; }

		#endregion

		// Constructors
		public UI(Game1 gm, SpriteBatch sb)
        {
            _gm = gm;
            _spriteBatch = sb;

			// Load Backgrounds
			_blankPanel = _gm.Content.Load<Texture2D>("BlankPanel");
			_titleBackground = _gm.Content.Load<Texture2D>("TitleBackground");
			_pauseBackground = _gm.Content.Load<Texture2D>("PauseBackground");
			_gameOverBackground = _gm.Content.Load<Texture2D>("DeathBackground");

			// Load Icons
			_comboIcon = _gm.Content.Load<Texture2D>("Sprites/ComboIcon");
			_shieldIcon = _gm.Content.Load<Texture2D>("UI Images/ShieldIcon");

			// Load Health Images
			_blueBallSpritesheet = _gm.Content.Load<Texture2D>("Sprites/BlueBallSpritesheet");
			_brokenBallSpriteWidth = 360;

			// Setup Player Speedometer
			_speedometerPin = _gm.Content.Load<Texture2D>("UI Images/SpeedometerPin");
			_speedometerCrest = _gm.Content.Load<Texture2D>("UI Images/SpeedometerCrest");
			_maxSpeedometerSpeed = 60f;

			// Gameover Assets
			Texture2D deadBallTexture = _gm.Content.Load<Texture2D>("Sprites/DeadBall");
			_deadBall = new Sprite(
				deadBallTexture,
				deadBallTexture.Bounds,
				new Rectangle(
					0, 0, 
					deadBallTexture.Width * 3 / 4, deadBallTexture.Width * 3 / 4));
			_deadBall.ObeyCamera = false;
			_maxHoverOffset = 50f;
			_hoverDuration = 2;

			// Load Fonts
			TitleCaseCarter = _gm.Content.Load<SpriteFont>("TitleCaseArial");
			MediumCarter = _gm.Content.Load<SpriteFont>("MediumCarter");
			MediumArial = _gm.Content.Load<SpriteFont>("MediumArial");

			// Setup effects
			_maxShakeMagnitude = 15f;
			_maxShakeMultiplier = 3f;
			_maxShakeOffset = new Vector2(_maxShakeMagnitude, _maxShakeMagnitude);
			_shakeDuration = 0.5;

			_buttonCursor = _gm.Content.Load<Texture2D>("UI Images/PinheadButtonCursor");

			CreateButtons();

			CreateSliders();

			SubscribeToEntities();
		}

		// Methods
		public void Update(GameTime gameTime)
		{
			switch (_gm.State)
			{
				case GameState.Menu:

					// Update menu buttons
					foreach (Button b in MenuButtons)
					{
						b.Update(gameTime);
					}

					// Update demo slider
					//_testSlider.Update(gameTime);
					break;

				case GameState.Play:
					UpdateSpeedometerShake(gameTime);

					UpdateCoreFlash(gameTime);

					break;

                case GameState.Pause:

					// Update pause buttons
					foreach (Button b in PauseButtons)
					{
						b.Update(gameTime);
					}
					break;

				case GameState.GameOver:
					MediaPlayer.Stop();

					// Oscillate dead bowling ball
					if (hoverUp)
					{
						_hoverTimeCounter += gameTime.ElapsedGameTime.TotalSeconds;

						if (_hoverTimeCounter > _hoverDuration)
							hoverUp = false;
					}
					else
					{
						_hoverTimeCounter -= gameTime.ElapsedGameTime.TotalSeconds;
                        if (_hoverTimeCounter < 0) 
							hoverUp = true;
                    }

					_hoverOffset = 
						(float)(_hoverTimeCounter / _hoverDuration) * _maxHoverOffset;

					// Update game over buttons
					foreach (Button b in GameOverButtons)
					{
						b.Update(gameTime);
					}
					break;
			}
		}
		public void Draw()
		{
			switch (_gm.State)
			{
				case GameState.Menu:
					DrawMainMenu();

					// Draw demo slider
					//_testSlider.Draw(_spriteBatch);

					// Draw slider value
					//_spriteBatch.DrawString(
					//	TitleCaseArial,
					//	$"{_testSlider.CurValue:0.000}",
					//	new Vector2(50f, 50f),
					//	Color.White);
					break;

				case GameState.Play:
					DrawPlayerHealth();

					DrawPlayerSpeedometer();
					
					DrawPlayerCombo();

					DrawCurrentCore();

					// Display Bullet Time multiplier
					//_spriteBatch.DrawString(
					//    Game1.ARIAL32,
					//    $"Time Multiplier: {Player.BulletTimeMultiplier:0.00}",
					//    new Vector2(0f, 200f),
					//    Color.White);
					
					break;

				case GameState.Pause:
					DrawPauseMenu();

					break;

				case GameState.GameOver:
					DrawGameOverMenu();
					break;
			}

			if (Game1.DebugOn)
			{
				_spriteBatch.DrawString(
					MediumArial,
					"Dev Cheats:\n" +
					$"D4 - Toggle Debug: {Game1.DebugOn}\n" +
					$"D5 - Infinite Health: {Game1.Player.InfiniteHealth}\n" +
					$"D6 - Infinite E_Health: {Game1.EManager.EnemiesInvincible}\n" +
					$"D7 - Give Player Curve Core\n" +
					$"Up/Down Change Next Map Dims: {_gm.MapDims}\n" +
					$"Left/Right Change Next Map Num Rooms: {_gm.NumRoomsInMap}",
					new Vector2(50f, 500f),
					Color.White);
			}
		}

        

        #region HUD Drawing Methods

        public void DrawPlayerCombo()
		{
			if (Game1.Player.Combo <= 0) return;

            string curCombo = Game1.Player.Combo.ToString();


            Vector2 comboDrawPos = new Vector2(100f, 410f);
			if (curCombo.Length > 1)
				comboDrawPos.X += 20;

            Vector2 comboStringDimensions =
                TitleCaseCarter.MeasureString(curCombo);

            Color textColor = Color.White;

            // Get Icon to draw
            Texture2D iconToDraw = _comboIcon;

            if (Game1.Player.ComboReward)
			{
                iconToDraw = _shieldIcon;
				textColor = Color.LightGreen;
			}

            // Draw combo number
            _spriteBatch.DrawString(
				TitleCaseCarter,
				curCombo,
				comboDrawPos,
				textColor);

			// Draw Combo Icon after text
			_spriteBatch.Draw(
				iconToDraw,
				new Rectangle(
					(int)(comboDrawPos.X + comboStringDimensions.X),
					430,
					80,
					80),
				Color.White);

			return;
		}

        private void DrawCurrentCore()
        {
            // Draw Core player is currently using
            Game1.Player.CurCore.Icon.Draw(
                _spriteBatch,
                new Vector2(100f, 950f),
                0f,
                Vector2.Zero);

            // Draw Switch Core Hint
            // If player has more than one core
            if (Game1.Player.NumCores > 1)
            {
                _spriteBatch.DrawString(
                    MediumCarter,
                    $"Q",
                    new Vector2(200f, 975f),
                    Color.White * 0.5f);
            }
        }

		private void UpdateCoreFlash(GameTime gameTime)
		{
			if (_completedFlashes == _flashesToDo) return;

			// ON
			if (_flashTimeCounter < _flashDuration / 2)
			{
				Game1.Player.CurCore.Icon.AlphaMultiplier = 1f;
			}
			// OFF
			else
			{
				Game1.Player.CurCore.Icon.AlphaMultiplier = 0.5f;
			}

			// Progress through animation
            _flashTimeCounter += gameTime.ElapsedGameTime.TotalSeconds;

			// Keep track of on/off cycles
			// Reset animation
            if (_flashTimeCounter >= _flashDuration)
            {
                _completedFlashes++;

				_flashTimeCounter -= _flashDuration;
            }
        }

		public void StartCoreFlash()
		{
			_completedFlashes = 0;
		}

        public void DrawPlayerHealth()
        {
            Color tint = Color.White;
            Rectangle source =
                new Rectangle(0, 0, _brokenBallSpriteWidth, _brokenBallSpriteWidth);

            // Get damage aesthetics
            if (Game1.Player.CurHealth > Game1.Player.MaxHealth * 3 / 4)
            {
                // Unscathed
                tint = Color.White;

            }
            else if (Game1.Player.CurHealth > Game1.Player.MaxHealth * 2 / 4)
            {
                // Light Damage
                tint = Color.LightPink;
                source.X = _brokenBallSpriteWidth;
            }
            else if (Game1.Player.CurHealth > Game1.Player.MaxHealth * 1 / 4)
            {
                // Medium Damage
                tint = Color.Pink;
                source.X = _brokenBallSpriteWidth * 2;
            }
            else
            {
                // Heavy Damage
                tint = Color.Red;
                source.X = _brokenBallSpriteWidth * 3;
            }

            // Get smiling aesthetic
            if (Game1.Player.IsSmiling)
                source.Y = _brokenBallSpriteWidth;

            Vector2 drawPos = new Vector2(60f, 40f);

            // Vibrate the image
            // Max magnitude of shake progressively decreases
            if (_shakeTimer > 0)
            {
                float remainingShakeProgress = (float)(_shakeTimer / _shakeDuration);
                float xBound = _maxShakeOffset.X * remainingShakeProgress;
                float yBound = _maxShakeOffset.Y * remainingShakeProgress;

                Random rand = new Random();
                float xOffset = (rand.NextSingle() * xBound * 2) - xBound;
                float yOffset = (rand.NextSingle() * yBound * 2) - yBound;

                Vector2 offset = new Vector2(xOffset, yOffset);

                drawPos += offset;
            }

            // Draw image
            _spriteBatch.Draw(
                _blueBallSpritesheet,
                drawPos,
                source,
                tint);
        }

		public void UpdateSpeedometerShake(GameTime gameTime)
		{
            if (_shakeTimer > 0)
            {
                _shakeTimer -=
                    gameTime.ElapsedGameTime.TotalSeconds *
                    Player.BulletTimeMultiplier;
            }
        }
        public void DrawPlayerSpeedometer()
		{
			// Draw Speed dial
			_spriteBatch.Draw(
				_speedometerCrest,
				new Vector2(60f, 40f),
				Color.White);

			// Draw Pin
			float minAngle = 5 * MathF.PI / 4;
			float maxAngle = 11 * MathF.PI / 6;

			float maxAngularDisplacement = MathHelper.TwoPi - (maxAngle - minAngle);

			float curPlayerSpeed = Game1.Player.Velocity.Length();

			float playerSpeedPercent = curPlayerSpeed / _maxSpeedometerSpeed;

			// Interpolate between min and max angle using
			// player speed
			_spriteBatch.Draw(
				_speedometerPin,
				new Vector2(
					60f + _speedometerCrest.Bounds.Center.X, 
					40f + _speedometerCrest.Bounds.Center.Y),
				_speedometerPin.Bounds,
				Color.White,
				playerSpeedPercent * maxAngularDisplacement - minAngle,
				_speedometerPin.Bounds.Center.ToVector2(),
				1f,
				SpriteEffects.None,
				0f);
		}

		#endregion

		#region Menu Drawing Methods
		private void DrawMainMenu()
		{
			_spriteBatch.Draw(
				_titleBackground,
				Game1.ScreenBounds,
				Color.White);

			string mouseSetting = "";
			if (Game1.Player.LaunchButton == 1)
			{
				mouseSetting = "Right-Handed";
			}
			else
				mouseSetting = "Left-Handed";


			_spriteBatch.DrawString(
				MediumArial,
				$"Mouse Setting:\n{mouseSetting}",
				new Vector2(1600, 950),
				Color.Black);

			// Draw Title
			//string titleText = "UnHoly Roller";
			//Vector2 titleMeasure = TitleCaseArial.MeasureString(titleText);

			//_spriteBatch.DrawString(
			//	TitleCaseArial,
			//	titleText,
			//	new Vector2(
			//		Game1.ScreenCenter.X - titleMeasure.X / 2f,
			//		150f),
			//	Color.White);

			// Draw Menu Buttons
			foreach (Button b in MenuButtons)
			{
				b.Draw(_spriteBatch);

				if (b.ContainsCursor)
				{
					_spriteBatch.Draw(
						_buttonCursor,
						new Rectangle(
							new Point(
								b.Bounds.Left - 120,
								b.Bounds.Center.Y - 60),
							new Point(120, 120)),
						Color.White);
				}
			}
			return;
		}

		private void DrawPauseMenu()
		{
			// Draw Background
			_spriteBatch.Draw(
				_pauseBackground,
				Game1.ScreenBounds,
				Color.White);

			// Draw Heading
			string pauseHeadingText = "Paused";
			Vector2 pauseHeadingDimensions = TitleCaseCarter.MeasureString(pauseHeadingText);

			_spriteBatch.DrawString(
				TitleCaseCarter,
				pauseHeadingText,
				new Vector2(
					Game1.ScreenCenter.X - pauseHeadingDimensions.X / 2,
					Game1.TileSize * 2),
				Color.White);

			// Draw pause buttons
			foreach (Button b in PauseButtons)
			{
				b.Draw(_spriteBatch);

                if (b.ContainsCursor)
                {
                    _spriteBatch.Draw(
                        _buttonCursor,
                        new Rectangle(
                            new Point(
                                b.Bounds.Left - 120,
                                b.Bounds.Center.Y - 60),
                            new Point(120, 120)),
                        Color.White);
                }
            }
			return;
		}

		private void DrawGameOverMenu()
		{
            // Draw Background
            _spriteBatch.Draw(
                _gameOverBackground,
                Game1.ScreenBounds,
                Color.White);

			// Draw dead ball
			Vector2 drawPos = new Vector2(
				Game1.ScreenCenter.X - _deadBall.DestinationRect.Width / 2,
				Game1.ScreenCenter.Y - _deadBall.DestinationRect.Height / 2 - 125);

			drawPos = new Vector2(drawPos.X, drawPos.Y + _hoverOffset);

			_deadBall.Draw(_spriteBatch, drawPos, 0f, Vector2.Zero);

            // Draw Game Over Heading
            string gameOverText = "You Died :P";

            Vector2 textPos =
                new Vector2(
                GetCenteredTextPos(gameOverText, TitleCaseCarter, Game1.ScreenCenter).X,
                100f);

            _spriteBatch.DrawString(
                TitleCaseCarter,
                gameOverText,
                textPos,
                Color.Black);

            // Draw game over buttons
            foreach (Button b in GameOverButtons)
			{
				b.Draw(_spriteBatch);

                if (b.ContainsCursor)
                {
                    _spriteBatch.Draw(
                        _buttonCursor,
                        new Rectangle(
                            new Point(
                                b.Bounds.Left - 120,
                                b.Bounds.Center.Y - 60),
                            new Point(120, 120)),
                        Color.White);
                }
            }
		}

		#endregion

		#region Component Creation Methods
		private void CreateButtons()
		{
			Texture2D emptyButton = _gm.Content.Load<Texture2D>("UI Images/EmptyButton");
			
            // Make menu buttons
            Rectangle buttonBounds = new Rectangle(
				1300,
				400,
				emptyButton.Bounds.Width,
				emptyButton.Bounds.Height);
			MenuButtons = new Button[3];

			// Play

			Vector2 wordDims = TitleCaseCarter.MeasureString("Play");
			int widthBuffer = 60;

			MenuButtons[0] = new Button(
				new Rectangle(
					1300, 
					400, 
					(int)wordDims.X + widthBuffer, 
					(int)wordDims.Y + widthBuffer), 
				null, null, emptyButton);
			MenuButtons[0].PressTint = Color.Blue * 0.3f;
			MenuButtons[0].SetText("Play", TitleCaseCarter);
			MenuButtons[0].TextColor = Color.Black;

			// Tutorial
			wordDims = TitleCaseCarter.MeasureString("Tutorial");
			MenuButtons[1] = new Button(
				new Rectangle(
					1400,
					MenuButtons[0].Bounds.Bottom, 
					(int)wordDims.X + widthBuffer, 
					(int)wordDims.Y + widthBuffer),
				null, null, emptyButton);
            MenuButtons[1].PressTint = Color.Blue * 0.3f;
            MenuButtons[1].SetText("Tutorial", TitleCaseCarter);
            MenuButtons[1].TextColor = Color.Black;

			// Quit
			wordDims = TitleCaseCarter.MeasureString("Quit");
			MenuButtons[2] = new Button(
				new Rectangle(
					1500,
                    MenuButtons[1].Bounds.Bottom,
					(int)wordDims.X + widthBuffer, 
					(int)wordDims.Y + widthBuffer),
				null, null, emptyButton);
            MenuButtons[2].PressTint = Color.Blue * 0.3f;
            MenuButtons[2].SetText("Quit", TitleCaseCarter);
            MenuButtons[2].TextColor = Color.Black;

			// Make pause buttons
			buttonBounds.Location = new Point(
				Game1.WindowWidth / 2 - buttonBounds.Width / 2,
				400);

			// Resume
			PauseButtons = new Button[2];

			wordDims = TitleCaseCarter.MeasureString("Resume");

			PauseButtons[0] = new Button(
				new Rectangle(
					(int)(Game1.ScreenCenter.X - (wordDims.X + widthBuffer) / 2),
					500,
					(int)wordDims.X + widthBuffer,
					(int)wordDims.Y + widthBuffer),
				null, null, null);
			PauseButtons[0].SetText("Resume", TitleCaseCarter);
			PauseButtons[0].TextColor = Color.White;
			buttonBounds.Y += emptyButton.Height;

            // Main Menu
            wordDims = TitleCaseCarter.MeasureString("Main Menu");

            PauseButtons[1] = new Button(
                new Rectangle(
                    (int)(Game1.ScreenCenter.X - (wordDims.X + widthBuffer) / 2),
					PauseButtons[0].Bounds.Bottom,
                    (int)wordDims.X + widthBuffer,
                    (int)wordDims.Y + widthBuffer), 
				null, null, null);
			PauseButtons[1].TextColor = Color.Coral;
			PauseButtons[1].SetText("Main Menu", TitleCaseCarter);

			// Make Game Over Buttons
			GameOverButtons = new Button[2];

			// Retry
			wordDims = TitleCaseCarter.MeasureString("Retry");

			GameOverButtons[0] = new Button(
				new Rectangle(
					(int)(Game1.ScreenCenter.X - (wordDims.X + widthBuffer) / 2),
					700,
					(int)wordDims.X + widthBuffer,
					(int)wordDims.Y + 20),
				null, null, null);
			GameOverButtons[0].TextColor = Color.Blue;
			GameOverButtons[0].SetText("Retry", TitleCaseCarter);

			// Main Menu
			wordDims = TitleCaseCarter.MeasureString("Main Menu");

			GameOverButtons[1] = new Button(
				new Rectangle(
					(int)(Game1.ScreenCenter.X - (wordDims.X + widthBuffer) / 2),
					GameOverButtons[0].Bounds.Bottom,
					(int)wordDims.X + widthBuffer,
					(int)wordDims.Y + 20),
				null, null, null);
			GameOverButtons[1].TextColor = Color.White;
			GameOverButtons[1].SetText("Main Menu", TitleCaseCarter);
			return;
		}
		private void CreateSliders()
		{
			Texture2D sliderBarImage = _gm.Content.Load<Texture2D>("UI Images/BasicSliderBar");
			Texture2D sliderKnobImage = _gm.Content.Load<Texture2D>("UI Images/BasicSliderKnob");
			_testSlider = new Slider(new Point(50, 200), sliderBarImage, sliderKnobImage);
			return;
		}
		#endregion

		#region Global Helper Methods
		/// <summary>
		/// Adds a newline character to the closest space in text 
		/// after a specified number of characters
		/// number of characters 
		/// </summary>
		/// <param name="text"> text to wrap </param>
		/// <param name="numChars"> max number of chars before line wrap </param>
		/// <returns> wrapped text </returns>
		/// <exception cref="Exception"> Number of characters cannot be less than 1 </exception>
		public static string GetWrappedText(string text, int numChars)
		{
			string result = text;
			
			// Early exit conditions
			if (numChars <= 0)
				throw new Exception("Number of characters cannot be less than 1");

			float numOverflows = (float)result.Length / numChars;
			if (numOverflows <= 1f)
				return result;

			// Loop through text, starting at first wrap index
			// Loop until text has been fully wrapped
			for (int i = numChars - 1; i < (int)numOverflows * numChars; i += numChars)
			{
				// Loop backward until found a space
				int spaceIndex = i;
				while(spaceIndex > 0 && text[spaceIndex] != ' ')
				{
					spaceIndex--;
				}

				// Exit early if no space was found
				if (spaceIndex == 0)
				{
					Debug.WriteLine($"Failed to wrap text: {text}");
					return result;
				}

				// Replace space with a newline
				result = result.Substring(0, spaceIndex) + "\n";
				if (text.Length > result.Length)
				{
					result += text.Substring(result.Length, text.Length - result.Length);
				}

				// Start wrapping from index after newline character
				i = spaceIndex + 1;
			}
			return result;
		}

		/// <summary>
		/// Gets the position necessary to draw the specified text
		/// centered on the specified center position
		/// </summary>
		/// <param name="text"> text to center </param>
		/// <param name="font"> font text will be drawn in </param>
		/// <param name="centerPos"> position to center text on </param>
		/// <returns></returns>
		public static Vector2 GetCenteredTextPos(string text, SpriteFont font, Vector2 centerPos)
		{
			Vector2 textDimensions = font.MeasureString(text);

			return centerPos - textDimensions / 2;
		}

		#endregion

		#region Subscription Methods

		/// <summary>
		/// Links UI methods to the events of game entities
		/// </summary>
		private void SubscribeToEntities()
		{
			Game1.Player.OnPlayerDamaged += PlayerWasDamaged;

			return;
		}

		/// <summary>
		/// Creates a UI response when the player's health decreases
		/// </summary>
		/// <param name="amount"> damage dealt to player </param>
		private void PlayerWasDamaged(int amount)
		{
			// Respond if damage was actually dealt to player
			// and player is not dead.
			if (amount <= 0 || Game1.Player.CurHealth <= 0)
				return;

			// Vibrate health UI
			// Vibration magnitude increases as health decreases
   			float shakeMaxMag =
   				_maxShakeMagnitude *
   				(1 - (float)Game1.Player.CurHealth / Game1.Player.MaxHealth) *
   				_maxShakeMultiplier;

			_maxShakeOffset = new Vector2(shakeMaxMag, shakeMaxMag);
			_shakeTimer = _shakeDuration;
			return;
		}

        #endregion

        #region Minimap Methods
        /// <summary>
        /// Loads minimap.
        /// </summary>
        public void LoadMinimap()
        {
            // Minimap
            _defaultRoomSize = 20;
            _minimapScale = 0.1f;
            _minimapPos = new Point(
                Game1.ScreenBounds.Right - Game1.CurrentLevel.Map.GetLength(0)
                * _defaultRoomSize - 150,
                50);
        }

		/// <summary>
		/// Draws a simplified representation of the map,
		/// highlighting the room the player is in and 
		/// the boss room
		/// </summary>
		public void DrawMinimap()
		{
			// Draw each room in current level relative to each other
			for (int y = 0; y < Game1.CurrentLevel.Map.GetLength(0); y++)
			{
				for (int x = 0; x < Game1.CurrentLevel.Map.GetLength(1); x++)
				{
					DrawRoomOnMiniMap(y, x);
				}
			}
			return;
		}

		/// <summary>
		/// Draws room in the minimap.
		/// </summary>
		/// <param name="y">Row of the tileset that the room is on.</param>
		/// <param name="x">Column of the tileset that the room is on.</param>
		private void DrawRoomOnMiniMap(int y, int x)
		{
			Room curRoom = Game1.CurrentLevel.Map[y, x];

			if (curRoom == null)
			{
				return;
			}

			if (curRoom.Discovered)
			{
				Rectangle roomBounds = new Rectangle(
					_minimapPos.X + x * _defaultRoomSize + 4,
					_minimapPos.Y + y * _defaultRoomSize + 4,
					_defaultRoomSize - 8,
					_defaultRoomSize - 8);
				Rectangle border = new Rectangle(
					_minimapPos.X + x * _defaultRoomSize,
					_minimapPos.Y + y * _defaultRoomSize,
					_defaultRoomSize,
					_defaultRoomSize);

				Color boxColor = Color.Black;
				if (curRoom.Entered)
					boxColor = Color.Gray;
				if (curRoom == Game1.CurrentLevel.CurrentRoom)
					boxColor = Color.White;
				else if (curRoom.IsBossRoom)
					boxColor = Color.Gold;

				//Draw border
				ShapeBatch.Box(border, Color.Black);
				// Draw box representing room
				ShapeBatch.Box(roomBounds, boxColor * 0.6f);
			}
			return;
		}
		#endregion

		public void DisplayRedirectsInCursor()
		{
            //string numRedirects = Game1.Player.NumRedirects.ToString();

            //Vector2 stringDims = TitleCaseArial.MeasureString(numRedirects);

            //_spriteBatch.DrawString(
            //	TitleCaseArial,
            //	numRedirects,
            //	Game1.CurMouse.Position.ToVector2() - stringDims / 2,
            //	Color.White);

            float maxRadius = 60f;
            if (Game1.Player.LaunchPrimed)
			{
				// Circular fill
				ShapeBatch.Circle(
					Game1.CurMouse.Position.ToVector2(),
					maxRadius * ((float)Game1.Player.NumRedirects / Game1.Player.MaxRedirects),
					Color.LightBlue * 0.6f);
			}
			else if (Game1.Player.NumRedirects == 0)
			{
                ShapeBatch.Circle(
                    Game1.CurMouse.Position.ToVector2(),
                    maxRadius,
                    Color.Red * 0.8f);
            }
			return;
        }
	}
}
