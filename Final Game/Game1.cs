using Final_Game.Entity;
using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Final_Game
{
	public enum GameState
	{
		Menu,
		Play,
		Pause,
		GameOver
	}

	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		public static Level TestLevel { get; private set; }

        #region Fields
		// Player
		public static Player Player { get; private set; }

		// Cursor
		private Texture2D _cursorTexture;

		// UI
		private UI _ui;

		// Mouse
        private MouseCursor _gameplayCursor;
        private MouseCursor _menuCursor;

		// Screen
        public static int WindowWidth = 1920;
        public static int WindowHeight = 1080;
        #endregion

        #region Properties
		// Screen
        public static Rectangle ScreenBounds => 
			new Rectangle(0, 0, WindowWidth, WindowHeight);
		public static Vector2 ScreenCenter => 
			new Vector2(WindowWidth / 2, WindowHeight / 2);

        // Mouse
        public static MouseState CurMouse { get; private set; }
        public static MouseState PrevMouse { get; private set; }
        public static bool MouseIsOnScreen => 
			ScreenBounds.Contains(CurMouse.Position);

        // Keyboard
        public static KeyboardState CurKB { get; private set; }
        public static KeyboardState PrevKB { get; private set; }

		// Environment
        public static int TileSize { get; private set; } = 100;

		// Game FSM
        public static GameState State { get; private set; }

		private static TileMaker tilemaker;

        #endregion

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			// Set window size
			_graphics.PreferredBackBufferWidth = WindowWidth;
			_graphics.PreferredBackBufferHeight = WindowHeight;
			_graphics.ApplyChanges();
		} 

		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			tilemaker = new TileMaker(Content);
			TestLevel = new Level(10, 10, 25);
			Player = new Player(this, new Vector2(300, 300));


			// Set default game state
			State = GameState.Menu;

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_cursorTexture = Content.Load<Texture2D>("Sprites/CursorSprite");

			// Create player
			Player = new Player(this, new Vector2(300, 300));
			
			// Create custom cursor
			_gameplayCursor = MouseCursor.FromTexture2D(
				_cursorTexture, _cursorTexture.Width / 2, _cursorTexture.Height / 2);

			// Create default cursor
			_menuCursor = MouseCursor.Arrow;

			// Create UI Manager
            _ui = new UI(this, _spriteBatch);

			// Hook Up Buttons
			SubscribeToButtons();
        }

		protected override void Update(GameTime gameTime)
		{
			// Get controller states
			CurMouse = Mouse.GetState();
			CurKB = Keyboard.GetState();

			// Update game
			switch (State)
			{
				case GameState.Play:
					if (this.IsActive)
					{
						Player.Update(gameTime);
						TestLevel.CurrentRoom.Update(gameTime);
					}
						
					if (SingleKeyPress(Keys.Escape))
					{
						PauseGame(true);
					}
                    break;

				case GameState.Pause:
					if (SingleKeyPress(Keys.Escape))
					{
						PauseGame(false);
					}
					break;
            }

			_ui.Update(gameTime);

			// Store controller states
			PrevMouse = CurMouse;
			PrevKB = CurKB;

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			_spriteBatch.Begin();
			TestLevel.CurrentRoom.Draw(_spriteBatch);

			// Draw game
			switch (State)
			{
				case GameState.Play:
					Player.Draw(_spriteBatch);

					break;
			}

			_ui.Draw(gameTime);
			
			_spriteBatch.End();

			ShapeBatch.Begin(GraphicsDevice);

			// Debug Drawing
			// ShapeBatch.Box(Player.Hitbox, Color.White);

			ShapeBatch.End();

			base.Draw(gameTime);
		}

		private void PauseGame(bool paused)
		{
			if (paused)
			{
				State = GameState.Pause;
				Mouse.SetCursor(_menuCursor);
			}
			else
			{
				State = GameState.Play;
				Mouse.SetCursor(_gameplayCursor);
			}
		}
        private void ResetGame()
        {
            Player.Reset();
        }

        #region Mouse Wrapper Methods
        public static bool IsMouseLeftClicked()
		{
			return
				MouseIsOnScreen && 
				CurMouse.LeftButton == ButtonState.Released &&
				PrevMouse.LeftButton == ButtonState.Pressed;
		}

		public static bool IsMouseButtonPressed(int buttonNum)
		{
			if (!MouseIsOnScreen) return false;

			switch (buttonNum)
			{
				case 1:
					return CurMouse.LeftButton == ButtonState.Pressed;

				case 2:
					return CurMouse.RightButton == ButtonState.Pressed;

				case 3:
					return CurMouse.MiddleButton == ButtonState.Pressed;

				default:
					return false;
			}
		}
		public static bool IsMouseButtonReleased(int buttonNum)
		{
			if (!MouseIsOnScreen) return false;

			switch (buttonNum)
			{
				case 1:
					return CurMouse.LeftButton == ButtonState.Released;

				case 2:
					return CurMouse.RightButton == ButtonState.Released;

				case 3:
					return CurMouse.MiddleButton == ButtonState.Released;

				default:
					return false;
			}
		}
        #endregion

        #region Keyboard Wrapper Methods

		public static bool SingleKeyPress(Keys k)
		{
			return CurKB.IsKeyDown(k) && PrevKB.IsKeyUp(k);
		}

        #endregion

        #region Button Methods
		
		public void SubscribeToButtons()
		{
			_ui.MenuButtons[0].OnClicked += StartGame;
			_ui.MenuButtons[2].OnClicked += EndGame;

			_ui.PauseButtons[0].OnClicked += ResumeGame;
			_ui.PauseButtons[1].OnClicked += ReturnToMainMenu;
		}

		private void StartGame()
		{
			State = GameState.Play;
			Mouse.SetCursor(_gameplayCursor);
		}

		private void ResumeGame()
		{
			State = GameState.Play;
            Mouse.SetCursor(_gameplayCursor);
        }

		private void ReturnToMainMenu()
		{
			State = GameState.Menu;
			Mouse.SetCursor(_menuCursor);

			ResetGame();
		}

		private void EndGame()
		{
			Exit();
		}

        #endregion
    }
}