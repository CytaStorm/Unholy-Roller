using Final_Game.Entity;
using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Final_Game
{
	/// <summary>
	/// Which state the game is in.
	/// </summary>
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


        #region Fields
		/// <summary>
		/// Level the player is currently on.
		/// </summary>
		public static Level TestLevel { get; private set; }

		/// <summary>
		/// Player Object
		/// </summary>
		public static Player Player { get; private set; }

		/// <summary>
		/// Cursor object.
		/// </summary>
		private Texture2D _cursorTexture;

		/// <summary>
		/// UI controller object.
		/// </summary>
		private UI _ui;

		/// <summary>
		/// Mouse controller object for control of cursor in-game.
		/// </summary>
        private MouseCursor _gameplayCursor;
		/// <summary>
		/// Mouse controller object for control of cursor in menus.
		/// </summary>
        private MouseCursor _menuCursor;

		/// <summary>
		/// Screen width, in pixels.
		/// </summary>
        public static int WindowWidth = 1920;
		/// <summary>
		/// Screen height, in pixels.
		/// </summary>
        public static int WindowHeight = 1080;
        #endregion

        #region Properties
		/// <summary>
		/// Rectangle respresenting the bounds of the screen, in pixels.
		/// </summary>
        public static Rectangle ScreenBounds => 
			new Rectangle(0, 0, WindowWidth, WindowHeight);
		/// <summary>
		/// Center of the screen, in pixels.
		/// </summary>
		public static Vector2 ScreenCenter => 
			new Vector2(WindowWidth / 2, WindowHeight / 2);

		/// <summary>
		/// Current state of the mouse.
		/// </summary>
        public static MouseState CurMouse { get; private set; }
		/// <summary>
		/// Previous state of the mouse.
		/// </summary>
        public static MouseState PrevMouse { get; private set; }
		/// <summary>
		/// Is the mouse on screen?
		/// </summary>
        public static bool MouseIsOnScreen => 
			ScreenBounds.Contains(CurMouse.Position);

        // Keyboard
		/// <summary>
		/// Current state of the keyboard.
		/// </summary>
        public static KeyboardState CurKB { get; private set; }
		/// <summary>
		/// Previous state of the keyboard.
		/// </summary>
        public static KeyboardState PrevKB { get; private set; }

		/// <summary>
		/// How large the each map tile is, in pixels.
		/// </summary>
        public static int TileSize { get; private set; } = 100;

		/// <summary>
		/// Game FSM.
		/// </summary>
        public static GameState State { get; private set; }

		/// <summary>
		/// Object that creates tiles.
		/// </summary>
		private static TileMaker tilemaker;

		/// <summary>
		/// Enemy manager.
		/// </summary>
		public static EnemyManager EManager { get; private set; }

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
			Player = new Player(this, new Vector2(
				TestLevel.CurrentRoom.Tileset.Width / 2, 
				TestLevel.CurrentRoom.Tileset.Height / 2));

			// Create Entity Managers
			EManager = new EnemyManager(this);

			// Set default game state
			State = GameState.Menu;

			//Load in first level content.
			TestLevel.LoadRoomUsingOffset(new Point(0, 0));
			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_cursorTexture = Content.Load<Texture2D>("Sprites/CursorSprite");

			// Create custom cursor
			_gameplayCursor = MouseCursor.FromTexture2D(
				_cursorTexture, _cursorTexture.Width / 2, _cursorTexture.Height / 2);
			
			// Create UI Manager
            _ui = new UI(this, _spriteBatch);
			
			// Create default cursor
			_menuCursor = MouseCursor.Arrow;

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
					PlayUpdate(gameTime);

					if (SingleKeyPress(Keys.Escape))
						PauseGame(true);
						break;

				case GameState.Pause:
					if (SingleKeyPress(Keys.Escape))
						PauseGame(false);
						break;
            }

			_ui.Update(gameTime);

			// Store controller states
			PrevMouse = CurMouse;
			PrevKB = CurKB;
			//Debug.WriteLine(TestLevel.CurrentRoom.Cleared);
			//Debug.WriteLine(TestLevel.CurrentRoom.Tileset.EnemyCount);

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

					EManager.Draw(_spriteBatch, gameTime);

					break;
			}

			_ui.Draw(gameTime);
			
			_spriteBatch.End();

			//DrawDebug();

			base.Draw(gameTime);
		}

		#region Update Game FSM Methods
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

		private void PlayUpdate(GameTime gameTime)
		{
			if (IsActive)
			{
				Player.Update(gameTime);
				TestLevel.CurrentRoom.Update(gameTime);

				EManager.Update(gameTime);
			}
		}
		#endregion

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

		private void DrawDebug()
		{
			// Debug Drawing
			ShapeBatch.Begin(GraphicsDevice);

			Player.DrawGizmos();

			EManager.DrawGizmos();

			ShapeBatch.End();
		}
    }
}