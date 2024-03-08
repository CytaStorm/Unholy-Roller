using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prototype.GameEntity;
using Prototype.MapGeneration;
using ShapeUtils;
using System;
using System.ComponentModel.DataAnnotations;

namespace Prototype
{
    public delegate void EnemiesKilled();

    public enum Gamestate
    {
        Menu,
        Play,
        Pause,
        Win,
        Death
    }

    public enum TutorialState
    {
        HasPrimedLaunch,
        HasLaunched,
        HasRedirected,
        HasUsedBrake,
        HasWalked,
        Finished
    }

    public class Game1 : Game
    {
        private SpriteBatch _spriteBatch;

        private Texture2D _spriteSheetTexture;

        // Map Generation
        private TileMaker _tileMaker;
        private Texture2D[] _tileTextures;
        public static RoomManager _roomManager;
        public static Room TUTORIAL_ROOM, TEST_ROOM, TEST_ROOM_TWO, TEST_ROOM_THREE,SPIKE_ROOM;

        // UI
        public static SpriteFont ARIAL32;
        private UI _ui;

        // Entities
        public static Player Player1 { get; private set; }
        public static EnemyManager EManager { get; private set; }

        // Screen
        public GraphicsDeviceManager Graphics { get; private set; }
        public const int WINDOW_WIDTH = 1920;
        public const int WINDOW_HEIGHT = 1080;

        public const int TILESIZE = 100;

        // Game State
        public static Gamestate GAMESTATE;

        private KeyboardState _curKB;
        private KeyboardState _prevKB;

        // Cutscenes
        private CutsceneManager _csManager;

        public TutorialState TUTORIALSTATE { get; private set; }

        public Game1()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Set window size
            Graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            Graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            Graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _spriteSheetTexture = Content.Load<Texture2D>("BasicBlueClean");

            ARIAL32 = Content.Load<SpriteFont>("arial32");

            // Create Tile Manager
            _tileMaker = new TileMaker(this);

            EManager = new EnemyManager(this);

            TUTORIAL_ROOM = new Room(
                this,
                "../../../TutorialArena.txt",
                "../../../Enemyless.txt",
                "../../../Enemyless.txt",
                new Point(100000, 0));

            TEST_ROOM = new Room(this, "../../../TestArena1.txt",
                "../../../PossibleEnemyPosition.txt",
                "../../../Enemyless.txt",
                new Point(0, 0));
            TEST_ROOM_TWO = new Room(this, "../../../TestArena5.txt",
                "../../../EnemyPositionsTA2.txt",
                "../../../Enemyless.txt",
                new Point(0, 0));

            SPIKE_ROOM = new Room(this, "../../../TestArena7.txt",
                "../../../Enemyless.txt",
                "../../../IntroduceObstacles.txt",
                new Point(0, 0));

            TEST_ROOM_THREE = new Room(this, "../../../TestArena6.txt",
                "../../../EnemyPositionsTA6.txt",
                "../../../ObstaclePositionsTA6.txt",
                new Point(TEST_ROOM.Floor.Width + TEST_ROOM_TWO.Floor.Width, 0));


            /* Connect rooms manually */

            // Room one
            TEST_ROOM.Connections.Add(SPIKE_ROOM);

            // Spike introduction room
            SPIKE_ROOM.Connections.Add(TEST_ROOM);
            SPIKE_ROOM.Connections.Add(TEST_ROOM_TWO);

            // Room two
            TEST_ROOM_TWO.Connections.Add(SPIKE_ROOM);
            TEST_ROOM_TWO.Connections.Add(TEST_ROOM_THREE);

            // Room three
            TEST_ROOM_THREE.Connections.Add(TEST_ROOM_TWO);


            // Create Dungeon
            //_roomManager = new RoomManager(10);

            // Add a door to each doorway in test room
            //TEST_ROOM.PlayerEntered();

            // Create Player
            Player1 = new Player(_spriteSheetTexture, new Vector2(WINDOW_WIDTH / 3, WINDOW_HEIGHT / 2),
                Graphics, _roomManager, this);

            _csManager = new CutsceneManager(this);
            
            // Hook up buttons
            _ui = new UI(this, _spriteBatch);
            _ui.MenuButtons[0].OnClicked += StartFirstLevel;
            _ui.MenuButtons[1].OnClicked += StartTutorial;
            _ui.MenuButtons[2].OnClicked += ExitGame;

            _ui.PauseButtons[0].OnClicked += Resume;
            _ui.PauseButtons[1].OnClicked += ReturnToMainMenu;
            _ui.PauseButtons[1].OnClicked += ResetGame;
            _ui.PauseButtons[1].OnClicked += _csManager.EndCurrentScene;

            GAMESTATE = Gamestate.Menu;
        }

        protected override void Update(GameTime gameTime)
        {
            _curKB = Keyboard.GetState();

            _csManager.Update(gameTime);

            switch (GAMESTATE)
            {
                case Gamestate.Menu:
                    //if (SingleKeyPress(Keys.Enter))
                    //{
                    //    GAMESTATE = Gamestate.Play;

                    //    _csManager.StartCutscene(Cutscene.Tutorial);
                    //}
                    break;

                case Gamestate.Play:

                    Player1.CurrentRoom.Update(gameTime);

                    EManager.Update(gameTime);

                    Player1.Update(gameTime);

                    if (SingleKeyPress(Keys.Escape))
                    {
                        GAMESTATE = Gamestate.Pause;
                    }
                    break;

                case Gamestate.Pause:
                    if (SingleKeyPress(Keys.Escape))
                    {
                        GAMESTATE = Gamestate.Play;
                    }
                    break;

                case Gamestate.Death:

                    // Reset stage with 'R'
                    if (SingleKeyPress(Keys.R))
                    {
                        ResetGame();

                        StartFirstLevel();
                    }
                    break;

            }
            _ui.Update(gameTime);

            _prevKB = _curKB;

            base.Update(gameTime);
        }

        public bool SingleKeyPress(Keys key)
        {
            return _curKB.IsKeyDown(key) && _prevKB.IsKeyUp(key);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            //_roomManager.Draw(_spriteBatch, gameTime);

            //_spriteBatch.DrawString(ARIAL32, Player1.NumRedirects.ToString(), new Vector2(_player.Position.X + Player.DEFAULT_SPRITE_WIDTH / 2, _player.Position.Y - 40f), Color.White);

            switch (GAMESTATE)
            {

                case Gamestate.Play:
                    Player1.CurrentRoom.Draw(_spriteBatch, gameTime);

                    foreach (MapOBJ obj in Player1.CurrentRoom.Interactables)
                    {
                        obj.Draw(_spriteBatch, gameTime);
                    }

                    EManager.Draw(_spriteBatch, gameTime);

                    Player1.Draw(_spriteBatch, gameTime);
                    break;

                case Gamestate.Death:
                    string deathText = "Press 'R' to restart";
                    Vector2 deathTextMeasure = ARIAL32.MeasureString(deathText);

                    _spriteBatch.DrawString(
                        ARIAL32,
                        deathText,
                        new Vector2(
                            WINDOW_WIDTH / 2f - deathTextMeasure.X / 2f,
                            WINDOW_HEIGHT / 2f - deathTextMeasure.Y / 2f),
                        Color.White);
                    break;
            }

            _ui.Draw(gameTime);

            _spriteBatch.DrawString(
                ARIAL32,
                _csManager.PhaseNum.ToString(),
                new Vector2(0f, 300f),
                Color.White);

            _csManager.Draw(_spriteBatch);

            _spriteBatch.End();

            //ShapeBatch.Begin(GraphicsDevice);

            //// Draw Gizmos
            //if (GAMESTATE == Gamestate.Play)
            //{
            //    EManager.DrawGizmos(gameTime);

            //    Player1.DrawGizmos();
            //}

            //ShapeBatch.End();

            base.Draw(gameTime);
        }

        public void StartFirstLevel()
        {
            GAMESTATE = Gamestate.Play;

            // Set default room player is in
            Player1.CurrentRoom = TEST_ROOM;
            Player1.MoveToCenterOfRoom(TEST_ROOM);

            TEST_ROOM.PlayerEntered();
        }

        public void StartTutorial()
        {
            GAMESTATE = Gamestate.Play;

            _csManager.StartCutscene(Cutscene.Tutorial);

            // Set default room player is in
            Player1.CurrentRoom = TUTORIAL_ROOM;
            Player1.MoveToCenterOfRoom(TUTORIAL_ROOM);
        }

        public void Resume()
        {
            GAMESTATE = Gamestate.Play;
        }

        public void ReturnToMainMenu()
        {
            GAMESTATE = Gamestate.Menu;
        }

        public void ResetGame()
        {
            TEST_ROOM.Reset();
            TEST_ROOM_TWO.Reset();
            TEST_ROOM_THREE.Reset();

            Player1.Reset();

            EManager.Clear();
        }

        public void ExitGame()
        {
            Exit();
        }
    }
}