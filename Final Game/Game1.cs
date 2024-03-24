using Final_Game.Entity;
using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
		GameOver,
		Cutscene
	}

	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		public static Room TutorialRoom { get; private set; }

		#region Fields
		/// <summary>
		/// Level the player is currently on.
		/// </summary>
		public static Level TestLevel { get; private set; }

		/// <summary>
		/// Player Object
		/// </summary>
		public static Player Player { get; private set; }

		// Cursor

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

		// Cutscenes
		private CutsceneManager _csManager;
		#endregion

		#region Properties
		// Screen
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

		// Mouse
		/// <summary>
		/// Current state of the mouse.
		/// </summary>
		public static MouseState CurMouse { get; private set; }
		/// <summary>
		/// Previous state of the mouse.
		/// </summary>
		public static MouseState PrevMouse { get; private set; }
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

		// Environment
		/// <summary>
		/// How large the each map tile is, in pixels.
		/// </summary>
		public static int TileSize { get; private set; } = 100;

		/// <summary>
		/// Game FSM.
		/// </summary>
		public GameState State { get; set; }

		/// <summary>
		/// Object that creates tiles.
		/// </summary>
		private static TileMaker tilemaker;

		// Enemy Management
		public static EnemyManager EManager { get; private set; }

		// Pickup Management
		public static PickupManager PManager { get; private set; }
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
			tilemaker = new TileMaker(Content);

			TestLevel = new Level(10, 10, 25);

			TutorialRoom = new Room(new Point(0, 0));

			Player = new Player(this, new Vector2(
				TestLevel.CurrentRoom.Tileset.Width / 2,
				800));
			Player.MoveToRoomCenter(TutorialRoom);

			// Set default game state
			State = GameState.Menu;
      
			//Load in first level content.
			//TestLevel.LoadRoomUsingOffset(new Point(0, 0));
			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			_cursorTexture = Content.Load<Texture2D>("Sprites/CursorSprite");

			// Create player
			Player = new Player(this, new Vector2(300, 300));

			// Create Entity Managers
			EManager = new EnemyManager(this);
			PManager = new PickupManager(this);

			// Create custom cursor
			_gameplayCursor = MouseCursor.FromTexture2D(
				_cursorTexture, _cursorTexture.Width / 2, _cursorTexture.Height / 2);

			// Create default cursor
			_menuCursor = MouseCursor.Arrow;

			// Create UI Manager
			_ui = new UI(this, _spriteBatch);

			// Create Cutscene Manager
			_csManager = new CutsceneManager(this);

			// Hook Up Buttons
			SubscribeToButtons();

			// Make any other subscriptions
			Player.OnPlayerDeath += EnterGameOver;
		}

		protected override void Update(GameTime gameTime)
		{
			// Only Update game if Game Window has focus
			if (!this.IsActive) return;

			// Get controller states
			CurMouse = Mouse.GetState();
			CurKB = Keyboard.GetState();

			// Update game
			switch (State)
			{
				case GameState.Play:
					Player.Update(gameTime);

					if (_csManager.Scene == Cutscene.None)
						TestLevel.CurrentRoom.Update(gameTime);

					EManager.Update(gameTime);

					PManager.Update(gameTime);

					if (SingleKeyPress(Keys.Escape))
						PauseGame(true);

					break;

				case GameState.Pause:
					if (SingleKeyPress(Keys.Escape))
						PauseGame(false);
					break;

				case GameState.Cutscene:
					_csManager.Update(gameTime);

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
			// Only Draw Game if Game Window has focus
			if (!this.IsActive) return;

			GraphicsDevice.Clear(Color.CornflowerBlue);

			_spriteBatch.Begin();

			// Draw game
			switch (State)
			{
				case GameState.Play:

					TestLevel.CurrentRoom.Draw(_spriteBatch);

					Player.Draw(_spriteBatch);

					EManager.Draw(_spriteBatch);

					PManager.Draw(_spriteBatch);

					break;

				case GameState.Cutscene:
					_csManager.Draw(_spriteBatch);
					break;
			}

			_ui.Draw(gameTime);

			_spriteBatch.End();

			// Draw simplified shapes

			ShapeBatch.Begin(GraphicsDevice);

			switch (State)
			{
				case GameState.Play:
					//DrawDebug();

					_ui.DrawMinimap();
					break;
			}

			ShapeBatch.End();

			base.Draw(gameTime);
		}

		public void PauseGame(bool paused)
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
			// Reset Entites
			Player.Reset();
		}

		private void EnterGameOver()
		{
			_csManager.StartCutscene(Cutscene.GameOver);
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
			_ui.MenuButtons[1].OnClicked += StartTutorial;
			_ui.MenuButtons[2].OnClicked += ExitGame;

			_ui.PauseButtons[0].OnClicked += ResumeGame;
			_ui.PauseButtons[1].OnClicked += ReturnToMainMenu;
			_ui.PauseButtons[1].OnClicked += _csManager.EndCurrentScene;

			_ui.GameOverButtons[0].OnClicked += ResetGame;
			_ui.GameOverButtons[0].OnClicked += StartGame;
			_ui.GameOverButtons[1].OnClicked += ReturnToMainMenu;

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

		private void ExitGame()
		{
			Exit();
		}

		public void StartTutorial()
		{
			State = GameState.Cutscene;
			Mouse.SetCursor(_gameplayCursor);

			_csManager.StartCutscene(Cutscene.Tutorial);

			Player.MoveToRoomCenter(TutorialRoom);
		}

		#endregion

		private void DrawDebug()
		{
			// Debug Drawing
			Player.DrawGizmos();

			EManager.DrawGizmos();

			PManager.DrawGizmos();
		}
	}
}
