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

		// Cheats
		public Point MapDims { get; private set; } = new Point(3, 3);
		public int NumRoomsInMap { get; private set; } = 5;

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
		/// The previous active game state
		/// </summary>
		public GameState PrevState { get; set; }

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

		// Particle Manager
		public static ParticleTrailManager FXManager { get; private set; }

        /// <summary>
        /// UI controller object.
        /// </summary>
        public UI UIManager { get; private set; }

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
			UIManager = new UI(this, _spriteBatch);

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

					UpdatePlayCursor();

					MainCamera.Update(gameTime);

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

			UIManager.Update(gameTime);

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

					if (!Player.CurCore.UsesCurve &&
						Player.Controllable &&
						Player.LaunchPrimed)
					{
						Player.CurCore.DrawTrajectoryHint(_spriteBatch);
					}

					break;

				case GameState.Cutscene:
					CSManager.Draw(_spriteBatch);
					break;
			}

			UIManager.Draw();
            
            _spriteBatch.End();

			// Draw simplified shapes

			ShapeBatch.Begin(GraphicsDevice);

			switch (State)
			{
				case GameState.Play:

					if (DebugOn) DrawDebug();
                    IManager.Draw();
                    UIManager.DrawMinimap();

					// Draw player launch arrow
					if ((Player.Controllable &&
						Player.LaunchPrimed))
					{
						Player.CurCore.DrawTrajectoryHint();
					}
					break;

				case GameState.Cutscene:
					CSManager.DrawSimpleShapes();

                    // Draw player launch arrow
                    if ((Player.Controllable &&
                        Player.LaunchPrimed))
                    {
                        Player.CurCore.DrawTrajectoryHint();
                    }
                    break;
			}

			ShapeBatch.End();

			base.Draw(gameTime);
		}

		public void PauseGame(bool paused)
		{
			if (paused)
			{
				PrevState = State;
				State = GameState.Pause;
				Mouse.SetCursor(_menuCursor);
			}
			else
			{
				State = PrevState;
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
			UIManager.MenuButtons[0].OnClicked += StartGame;
			UIManager.MenuButtons[1].OnClicked += StartTutorial;
			UIManager.MenuButtons[2].OnClicked += ExitGame;

			UIManager.PauseButtons[0].OnClicked += ResumeGame;
			UIManager.PauseButtons[1].OnClicked += ReturnToMainMenu;
			UIManager.PauseButtons[1].OnClicked += CSManager.EndCurrentScene;

			UIManager.GameOverButtons[0].OnClicked += ResetGame;
			UIManager.GameOverButtons[0].OnClicked += StartGame;
			UIManager.GameOverButtons[1].OnClicked += ReturnToMainMenu;

		}

		private void StartGame()
		{
			// Make a new level
			TestLevel = new Level(MapDims.X, MapDims.Y, NumRoomsInMap);

			CurrentLevel = TestLevel;

			Player.MoveToRoomCenter(CurrentLevel.StartRoom);
			TestLevel.LoadRoomUsingOffset(new Point(0, 0));

			State = GameState.Play;
			UIManager.LoadMinimap();
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


			if (State == GameState.Menu)
			{
				// Change Map Dimensions
				if (SingleKeyPress(Keys.Up))
				{
					MapDims = new Point(
						MapDims.X + 1,
						MapDims.Y + 1);
				}
				if (SingleKeyPress(Keys.Down) && MapDims.X > 1)
				{
					MapDims = new Point(
						MapDims.X - 1,
						MapDims.Y - 1);
				}

				if (SingleKeyPress(Keys.Right))
				{
					NumRoomsInMap++;
				}
				if (SingleKeyPress(Keys.Left) && NumRoomsInMap > 1)
				{
					NumRoomsInMap--;
				}
			}
		}

		private void DrawDebug()
		{
			// Debug Drawing
			Player.DrawGizmos();

			EManager.DrawGizmos();

			PManager.DrawGizmos();

			UIManager.DisplayRedirectsInCursor();
		}

		public void CreateTutorialLevel()
		{
            bool[,] tutorialRooms = new bool[,]
            {
                { true, true, true },
                { false, false, true }
            };

            string[,] obsData = new string[tutorialRooms.GetLength(0), tutorialRooms.GetLength(1)];
			obsData[0, 1] = "s,4,5|s,4,6|s,4,7|s,4,8|s,4,9|s,4,10|s,5,5|s,5,6|s,5,7|s,5,8|s,5,9|s,5,10|" +
				"s,6,5|s,6,6|w,6,7|w,6,8|s,6,9|s,6,10|s,7,5|s,7,6|w,7,7|w,7,8|s,7,9|s,7,10|s,8,5|s,8,6|" +
				"w,8,7|w,8,8|s,8,9|s,8,10|s,9,5|s,9,6|w,9,7|w,9,8|s,9,9|s,9,10|s,10,5|s,10,6|s,10,7|" +
				"s,10,8|s,10,9|s,10,10|s,11,5|s,11,6|s,11,7|s,11,8|s,11,9|s,11,10"; // Hella spikes


            string[,] enemySpawnData = new string[tutorialRooms.GetLength(0), tutorialRooms.GetLength(1)];
            enemySpawnData[0, 2] = "2,5|6,3|9,7|3,9";

            TutorialLevel = new Level(
                tutorialRooms,
                new Point(0, 0),
                obsData,
                enemySpawnData);
        }

		/// <summary>
		/// Updates cursor appearance based on how many
		/// redirects the player has
		/// </summary>
		public void UpdatePlayCursor()
		{
            if (Player.NumRedirects > 0)
                Mouse.SetCursor(_gameplayCursor);
            else
                Mouse.SetCursor(_noRedirectCursor);
        }
	}
}
