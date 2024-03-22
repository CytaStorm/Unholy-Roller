using Final_Game;
using Final_Game.Entity;
using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
		// Management
		private Game1 _gm;
		private SpriteBatch _spriteBatch;

		// Sliders
		private Slider _testSlider;

		// Backgrounds
		private Texture2D _blankPanel;

		// Player Health
		private Texture2D _blueBallSpritesheet;
		private int _brokenBallSpriteWidth;

		// Player Speed
		private Texture2D _speedometerPin;
		private Texture2D _speedometerCrest;
		private float _maxSpeedometerSpeed;

		// Shake Effect
		private double _shakeDuration;
		private double _shakeTimer;
		private Vector2 _maxShakeOffset;
		private float _maxShakeMagnitude;
		private float _maxShakeMultiplier;

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

		#region Properties
		// Button Containers
		public Button[] MenuButtons { get; private set; }
		public Button[] PauseButtons { get; private set; }
		public Button[] GameOverButtons { get; private set; }

		// Fonts
		public static SpriteFont TitleCaseArial { get; private set; }
		public static SpriteFont MediumArial { get; private set; }

		#endregion

		// Constructors
		public UI(Game1 gm, SpriteBatch sb)
		{
			_gm = gm;
			_spriteBatch = sb;

			// Minimap
			_defaultRoomSize = 20;
			_minimapScale = 0.1f;
			_minimapPos = new Point(
				Game1.ScreenBounds.Right - Level.Map.GetLength(0) * _defaultRoomSize - 150,
				50);


			// Load Backgrounds
			_blankPanel = _gm.Content.Load<Texture2D>("BlankPanel");

			// Load Health Images
			_blueBallSpritesheet = _gm.Content.Load<Texture2D>("Sprites/BlueBallSpritesheet");
			_brokenBallSpriteWidth = 360;

			// Setup Player Speedometer
			_speedometerPin = _gm.Content.Load<Texture2D>("SpeedometerPin");
			_speedometerCrest = _gm.Content.Load<Texture2D>("SpeedometerCrest");
			_maxSpeedometerSpeed = 60f;

			// Gameover Assets
			Texture2D deadBallTexture = _gm.Content.Load<Texture2D>("Sprites/DeadBall");
			_deadBall = new Sprite(
				deadBallTexture,
				deadBallTexture.Bounds,
				new Rectangle(
					0, 0, 
					deadBallTexture.Width * 3 / 4, deadBallTexture.Width * 3 / 4));
			_maxHoverOffset = 50f;
			_hoverDuration = 2;

			// Load Fonts
			TitleCaseArial = _gm.Content.Load<SpriteFont>("TitleCaseArial");
			MediumArial = _gm.Content.Load<SpriteFont>("MediumArial");

			// Setup effects
			_maxShakeMagnitude = 15f;
			_maxShakeMultiplier = 3f;
			_maxShakeOffset = new Vector2(_maxShakeMagnitude, _maxShakeMagnitude);
			_shakeDuration = 0.5;

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
					_testSlider.Update(gameTime);
					break;

				case GameState.Pause:

					// Update pause buttons
					foreach (Button b in PauseButtons)
					{
						b.Update(gameTime);
					}
					break;

				case GameState.GameOver:

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
		public void Draw(GameTime gameTime)
		{
			switch (_gm.State)
			{
				case GameState.Menu:
					DrawMainMenu();

					// Draw demo slider
					_testSlider.Draw(_spriteBatch);

					// Draw slider value
					_spriteBatch.DrawString(
						TitleCaseArial,
						$"{_testSlider.CurValue:0.000}",
						new Vector2(50f, 50f),
						Color.White);
					break;

				case GameState.Play:
					DrawPlayerSpeedometer();

					DrawPlayerCombo();

					// Display Bullet Time multiplier
					//_spriteBatch.DrawString(
					//    Game1.ARIAL32,
					//    $"Time Multiplier: {Player.BulletTimeMultiplier:0.00}",
					//    new Vector2(0f, 200f),
					//    Color.White);
					if (_shakeTimer > 0)
					{
						_shakeTimer -=
							gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
					}
					break;

				case GameState.Pause:
					DrawPauseMenu();

					break;

				case GameState.GameOver:
					DrawGameOverMenu();
					break;
			}
		}

		#region HUD Drawing Methods

		private void DrawPlayerCombo()
		{		
			_spriteBatch.DrawString(
				TitleCaseArial,
				Game1.Player.Combo.ToString(),
				new Vector2(
				100, 400),
				Color.White);
			return;
		}

		private void DrawPlayerSpeedometer()
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
			// Draw Title
			string titleText = "UnHoly Roller";
			Vector2 titleMeasure = TitleCaseArial.MeasureString(titleText);

			_spriteBatch.DrawString(
				TitleCaseArial,
				titleText,
				new Vector2(
					Game1.ScreenCenter.X - titleMeasure.X / 2f,
					150f),
				Color.White);

			// Draw Menu Buttons
			foreach (Button b in MenuButtons)
			{
				b.Draw(_spriteBatch);
			}
			return;
		}

		private void DrawPauseMenu()
		{
			// Draw Heading
			string pauseHeadingText = "Paused";
			Vector2 pauseHeadingDimensions = TitleCaseArial.MeasureString(pauseHeadingText);

			_spriteBatch.DrawString(
				TitleCaseArial,
				pauseHeadingText,
				new Vector2(
					Game1.ScreenCenter.X - pauseHeadingDimensions.X / 2,
					Game1.TileSize * 2),
				Color.White);

			// Draw pause buttons
			foreach (Button b in PauseButtons)
			{
				b.Draw(_spriteBatch);
			}
			return;
		}

		private void DrawGameOverMenu()
		{
			// Draw Game Over Heading
			string gameOverText = "You Died :P";

			Vector2 textPos =
				new Vector2(
				GetCenteredTextPos(gameOverText, TitleCaseArial, Game1.ScreenCenter).X,
				100f);

			_spriteBatch.DrawString(
				TitleCaseArial,
				gameOverText,
				textPos,
				Color.Black);

			// Draw dead ball
			Vector2 drawPos = new Vector2(
				Game1.ScreenCenter.X - _deadBall.DestinationRect.Width / 2,
				Game1.ScreenCenter.Y - _deadBall.DestinationRect.Height / 2 - 150);

			drawPos = new Vector2(drawPos.X, drawPos.Y + _hoverOffset);

			_deadBall.Draw(_spriteBatch, drawPos);

			// Draw game over buttons
			foreach (Button b in GameOverButtons)
			{
				b.Draw(_spriteBatch);
			}
		}

		#endregion

		#region Component Creation Methods
		private void CreateButtons()
		{
			Texture2D emptyButton = _gm.Content.Load<Texture2D>("EmptyButton");

			// Make menu buttons
			Rectangle buttonBounds = new Rectangle(
				Game1.WindowWidth / 2 - emptyButton.Bounds.Width / 2,
				400,
				emptyButton.Bounds.Width,
				emptyButton.Bounds.Height);

			MenuButtons = new Button[3];
			MenuButtons[0] = new Button(buttonBounds, 
				_gm.Content.Load<Texture2D>("CoolButtonStatic"), 
				_gm.Content.Load<Texture2D>("CoolButtonHover"), 
				_gm.Content.Load<Texture2D>("CoolButtonPressed"));
			MenuButtons[0].TintColor = Color.White;
			//MenuButtons[0].SetText("Play", _titleCaseArial);
			buttonBounds.Y += emptyButton.Height;

			MenuButtons[1] = new Button(buttonBounds, emptyButton, emptyButton, emptyButton);
			MenuButtons[1].TextColor = Color.Coral;
			MenuButtons[1].SetText("Tutorial", TitleCaseArial);

			buttonBounds.Y += emptyButton.Height;
			MenuButtons[2] = new Button(buttonBounds, emptyButton, emptyButton, emptyButton);
			MenuButtons[2].TintColor = Color.Orange;
			MenuButtons[2].TextColor = Color.Purple;
			MenuButtons[2].SetText("Quit", TitleCaseArial);

			// Make pause buttons
			buttonBounds.Y = 400;

			PauseButtons = new Button[2];
			PauseButtons[0] = new Button(buttonBounds, emptyButton, emptyButton, emptyButton);
			PauseButtons[0].TintColor = Color.Blue;
			PauseButtons[0].SetText("Resume", TitleCaseArial);
			buttonBounds.Y += emptyButton.Height;

			PauseButtons[1] = new Button(buttonBounds, emptyButton, emptyButton, emptyButton);
			PauseButtons[1].TextColor = Color.Coral;
			PauseButtons[1].SetText("Main Menu", TitleCaseArial);

			// Make Game Over Buttons
			GameOverButtons = new Button[2];

			buttonBounds.Y = Game1.ScreenCenter.ToPoint().Y + 100;
			GameOverButtons[0] = new Button(buttonBounds, emptyButton, emptyButton, emptyButton);
			GameOverButtons[0].TintColor = Color.Black;
			GameOverButtons[0].TextColor = Color.Orange;
			GameOverButtons[0].SetText("Retry", TitleCaseArial);

			buttonBounds.Y += emptyButton.Height;
			GameOverButtons[1] = new Button(buttonBounds, emptyButton, emptyButton, emptyButton);
			GameOverButtons[1].TextColor = Color.Coral;
			GameOverButtons[1].SetText("Main Menu", TitleCaseArial);
			return;
		}
		private void CreateSliders()
		{
			Texture2D sliderBarImage = _gm.Content.Load<Texture2D>("BasicSliderBar");
			Texture2D sliderKnobImage = _gm.Content.Load<Texture2D>("BasicSliderKnob");
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
				else
				{
					// Replace space with a newline
					result = result.Substring(0, spaceIndex) + "\n";
					if (text.Length > result.Length)
					{
						result += text.Substring(result.Length, text.Length - result.Length);
					}
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
			if (amount <= 0)
				return;

			if (Game1.Player.CurHealth > 0)
			{
				// Vibrate health UI
				// Vibration magnitude increases as health decreases
				float shakeMaxMag = _maxShakeMagnitude;
				if (Game1.Player.CurHealth > 0)
				{
					shakeMaxMag =
						_maxShakeMagnitude * 
						(1 - (float)Game1.Player.CurHealth / Game1.Player.MaxHealth) *
						_maxShakeMultiplier;
				}

				_maxShakeOffset = new Vector2(shakeMaxMag, shakeMaxMag);
				_shakeTimer = _shakeDuration;
			}
		}

		#endregion

		/// <summary>
		/// Draws a simplified representation of the map,
		/// highlighting the room the player is in and 
		/// the boss room
		/// </summary>
		public void DrawMinimap()
		{
			// Draw each room in current level relative to each other
			for (int y = 0; y < Level.Map.GetLength(0); y++)
			{
				for (int x = 0; x < Level.Map.GetLength(1); x++)
				{
					Room curRoom = Level.Map[y, x];

					if (curRoom != null)
					{
						Rectangle roomBounds = new Rectangle(
							_minimapPos.X + x * _defaultRoomSize,
							_minimapPos.Y + y * _defaultRoomSize,
							_defaultRoomSize,
							_defaultRoomSize);

						Color boxColor = Color.Black;
						if (curRoom == Game1.TestLevel.CurrentRoom)
							boxColor = Color.White;
						else if (curRoom.IsBossRoom)
							boxColor = Color.Gold;

						// Draw box representing room
						ShapeBatch.Box(roomBounds, boxColor * 0.6f);
					}
				}
			}
		}
	}
}
