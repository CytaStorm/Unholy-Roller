using Final_Game;
using Final_Game.Entity;
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

		// Player Health
		//private Sprite _heart;
		//private Sprite _halfHeart;
		//private Sprite _emptyHeart;

		// Player Speed
		private Texture2D _speedometerPin;
		private Texture2D _speedometerCrest;
		private float _maxSpeedometerSpeed;
		#endregion

		#region Properties
		// Button Containers
		public Button[] MenuButtons { get; private set; }
		public Button[] PauseButtons { get; private set; }

        // Fonts
        public static SpriteFont TitleCaseArial { get; private set; }
		public static SpriteFont MediumArial { get; private set; }

        #endregion

        // Constructors
        public UI(Game1 gm, SpriteBatch sb)
		{
			_gm = gm;
			_spriteBatch = sb;

			// Heart image
			//Texture2D heart = _gm.Content.Load<Texture2D>("BowlingHeart");
			//_heart = new Sprite(
			//    heart,
			//    new Rectangle(0, 0, 100, 100),
			//    new Rectangle(0, 0, 100, 100));

			//Texture2D halfHeart = _gm.Content.Load<Texture2D>("BowlingHalfHeart");
			//_halfHeart = new Sprite(
			//    halfHeart,
			//    new Rectangle(0, 0, 100, 100),
			//    new Rectangle(0, 0, 100, 100));

			//Texture2D emptyHeart = _gm.Content.Load<Texture2D>("EmptyHeart");
			//_emptyHeart = new Sprite(
			//    emptyHeart,
			//    new Rectangle(0, 0, 100, 100),
			//    new Rectangle(0, 0, 100, 100));

			// Setup Player Speedometer
			_speedometerPin = _gm.Content.Load<Texture2D>("SpeedometerPin");
			_speedometerCrest = _gm.Content.Load<Texture2D>("SpeedometerCrest");
			_maxSpeedometerSpeed = 60f;


			// Load Fonts
			TitleCaseArial = _gm.Content.Load<SpriteFont>("TitleCaseArial");
			MediumArial = _gm.Content.Load<SpriteFont>("MediumArial");

			CreateButtons();

			CreateSliders();
		}

		// Methods
		public void Update(GameTime gameTime)
		{
			switch (Game1.State)
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
			}
		}
		public void Draw(GameTime gameTime)
		{
			switch (Game1.State)
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
					DrawPlayerHealth();

					DrawPlayerSpeedometer();

					DrawPlayerCombo();

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
			}
		}

		private void DrawPlayerHealth()
		{
			// How many half hearts the player has.
			int currentHealth = Game1.Player.CurHealth;
			int wholeHearts = currentHealth / 2;

			//Draw whole hearts
			for (int i = 0; i < wholeHearts; i++)
			{
				//Draw whole hearts using size of heart sprite * i
			}

			if (currentHealth % 2 == 1)
			{
				//Draw half heart after full heart
			}
			return;
		}

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

        #endregion
    }
}
