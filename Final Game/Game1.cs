using Final_Game.Entity;
using Final_Game.LevelGen;
using Final_Game.Managers;
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
		public GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		#region Fields
		/// <summary>
		/// Level the player is currently on.
		/// </summary>
		public static Level TestLevel { get; private set; }

		/// <summary>
		/// The level in which the tutorial takes place
		/// </summary>
		public static Level TutorialLevel { get; private set; }

		/// <summary>
		/// The level the player is currently within
		/// </summary>
		public static Level CurrentLevel { get; private set; }

		/// <summary>
		/// Player Object
		/// </summary>
		public static Player Player { get; private set; }

		// Cursor

		/// <summary>
		/// Cursor object.
		/// </summary>
		private Texture2D _cursorTexture;

		private Texture2D _noRedirectCursorTexture;

		/// <summary>
		/// UI controller object.
		/// </summary>
		private UI _ui;

		/// <summary>
		/// Mouse controller object for control of cursor in-game.
		/// </summary>
		private MouseCursor _gameplayCursor;

		private MouseCursor _noRedirectCursor;

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
		public static CutsceneManager CSManager { get; private set; }

		// Backgrounds
		private Texture2D _playBackground;

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

		//Sound Manager 
		public static SoundManager SManager { get; private set; }

		public static ParticleTrailManager FXManager { get; private set; }

		public static Camera MainCamera { get; private set; }

		public static bool DebugOn { get; private set; }
		//Attack Indicator Manager for the Boss
        public static IndicatorManager IManager { get; private set; }
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

			TutorialLevel = new Level(1, 1, 1);

			//Ensures that first room goes through room loading 

			//Player = new Player(this, new Vector2(
			//	TestLevel.CurrentRoom.Tileset.Width / 2,
			//	800));
			//Player.MoveToRoomCenter(TutorialRoom);

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
			_noRedirectCursorTexture = Content.Load<Texture2D>("Sprites/X_CursorSprite");

			// Load Backgrounds
			_playBackground = Content.Load<Texture2D>("PlayBackground4");

			// Make Camera
			MainCamera = new Camera(new Vector2(300, 300), 1f);

			// Create player
			Player = new Player(this, new Vector2(300, 300));

			// Create Entity Managers
			EManager = new EnemyManager(this);
			PManager = new PickupManager(this);

			//Create FX Manager
			FXManager = new ParticleTrailManager(0.3f);

			//Setup sound manager.
			SoundManager.LoadSoundFiles(Content);

			// Create custom cursors
			_gameplayCursor = MouseCursor.FromTexture2D(
				_cursorTexture, _cursorTexture.Width / 2, _cursorTexture.Height / 2);

			_noRedirectCursor = MouseCursor.FromTexture2D(
				_noRedirectCursorTexture,
				_noRedirectCursorTexture.Width / 2,
				_noRedirectCursorTexture.Height / 2);

			// Create default cursor
			_menuCursor = MouseCursor.Arrow;
            
			// Create UI Manager
			_ui = new UI(this, _spriteBatch);

			// Create Cutscene Manager
			CSManager = new CutsceneManager(this);

			//Indicator
			IManager = new IndicatorManager(this);

			// Hook Up Buttons
			SubscribeToButtons();

			// Make any other subscriptions
			Player.OnPlayerDeath += EnterGameOver;
			EManager.OnLastEnemyKilled += SoundManager.PlayOutOfCombatSong;

			SoundManager.PlayBGM();
		}

		protected override void Update(GameTime gameTime)
		{
			// Only Update game if Game Window has focus
			if (!this.IsActive) return;

			// Get controller states
			CurMouse = Mouse.GetState();
			CurKB = Keyboard.GetState();

			HandleDevToggle();


			// Update game
			switch (State)
			{
				case GameState.Menu:
					if (SingleKeyPress(Keys.D0))
						Player.ToggleLeftHandMouse();

					break;

				case GameState.Play:
					Player.Update(gameTime);

					// Update cursor
					if (Player.NumRedirects > 0)
						Mouse.SetCursor(_gameplayCursor);
					else 
						Mouse.SetCursor(_noRedirectCursor);

					MainCamera.Update(gameTime);

					if (CSManager.Scene == Cutscene.None)
						CurrentLevel.CurrentRoom.Update(gameTime);

					EManager.Update(gameTime);

					IManager.Update(gameTime);

					PManager.Update(gameTime);

					FXManager.Update(gameTime);

					if (SingleKeyPress(Keys.Escape))
						PauseGame(true);

					break;

				case GameState.Pause:
					if (SingleKeyPress(Keys.Escape))
						PauseGame(false);
					break;

				case GameState.Cutscene:
					CSManager.Update(gameTime);

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

			GraphicsDevice.Clear(Color.Black);

			_spriteBatch.Begin();
            

            // Draw game
            switch (State)
			{
				case GameState.Play:

					_spriteBatch.Draw(
						_playBackground,
						ScreenBounds,
						Color.White);
					
					CurrentLevel.CurrentRoom.Draw(_spriteBatch);

					Player.Draw(_spriteBatch);

					FXManager.Draw(_spriteBatch);

                    EManager.Draw(_spriteBatch);

                    PManager.Draw(_spriteBatch);

					break;

				case GameState.Cutscene:
					CSManager.Draw(_spriteBatch);
					break;
			}

			_ui.Draw(gameTime);
            
            _spriteBatch.End();

			// Draw simplified shapes

			ShapeBatch.Begin(GraphicsDevice);

			switch (State)
			{
				case GameState.Play:

					if (DebugOn) DrawDebug();
                    IManager.Draw();
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

			Debug.WriteLine("reset");
			EManager.Clear();
			SoundManager.PlayOutOfCombatSong();
		}

		private void EnterGameOver()
		{
			CSManager.StartCutscene(Cutscene.GameOver);
		}

		#region Mouse Wrapper Methods
		public static bool IsMouseButtonClicked(int buttonNum)
		{

			ButtonState pressedInCurFrame = ButtonState.Released;
			ButtonState releasedInPrevFrame = ButtonState.Released;

			switch (buttonNum)
			{
				case 1:
					pressedInCurFrame = CurMouse.LeftButton;
					releasedInPrevFrame = PrevMouse.LeftButton;
					break;

				case 2:
                    pressedInCurFrame = CurMouse.RightButton;
                    releasedInPrevFrame = PrevMouse.RightButton;
                    break;

				case 3:
                    pressedInCurFrame = CurMouse.MiddleButton;
                    releasedInPrevFrame = PrevMouse.MiddleButton;
                    break;

				default:
					return false;
			}

			return
				MouseIsOnScreen &&
				pressedInCurFrame == ButtonState.Released &&
				releasedInPrevFrame == ButtonState.Pressed;
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
			_ui.PauseButtons[1].OnClicked += CSManager.EndCurrentScene;

			_ui.GameOverButtons[0].OnClicked += ResetGame;
			_ui.GameOverButtons[0].OnClicked += StartGame;
			_ui.GameOverButtons[1].OnClicked += ReturnToMainMenu;

		}

		private void StartGame()
		{
			// Make a new level
			TestLevel = new Level(5, 5, 10);

			CurrentLevel = TestLevel;

			Player.MoveToRoomCenter(CurrentLevel.StartRoom);
			TestLevel.LoadRoomUsingOffset(new Point(0, 0));

			State = GameState.Play;
			_ui.LoadMinimap();
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

			CSManager.StartCutscene(Cutscene.Tutorial);

			CurrentLevel = TutorialLevel;

			Player.MoveToRoomCenter(TutorialLevel.StartRoom);
		}

		#endregion

		/// <summary>
		/// Allows devs to toggling different game values 
		/// via key press.
		/// FYI: Can only toggle values if in debug mode (DebugOn is true)
		/// </summary>
		private void HandleDevToggle()
		{
			// Toggle Debug Drawing
			if (SingleKeyPress(Keys.D4)) DebugOn = !DebugOn;

			if (!DebugOn) return;

			// Toggle Infinite Player Health
			if (SingleKeyPress(Keys.D5)) Player.InfiniteHealth = !Player.InfiniteHealth;

			// Toggle Infinite Enemy Health
			if (SingleKeyPress(Keys.D6))
				EManager.EnemiesInvincible = !EManager.EnemiesInvincible;
		}

		private void DrawDebug()
		{
			// Debug Drawing
			Player.DrawGizmos();

			EManager.DrawGizmos();

			PManager.DrawGizmos();

			_ui.DisplayRedirectsInCursor();
		}
	}
}
