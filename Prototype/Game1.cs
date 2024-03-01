using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prototype.GameEntity;
using Prototype.MapGeneration;
using ShapeUtils;

namespace Prototype
{
    public enum Gamestate
    {
        Menu,
        Play,
        Win,
        Death
    }

    public class Game1 : Game
    {
        private SpriteBatch _spriteBatch;

        private Texture2D _spriteSheetTexture;

        // Map Generation
        private TileMaker _tileMaker;
        private Texture2D[] _tileTextures;
        public static RoomManager _roomManager;
        public static Room TEST_ROOM;

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

        public const int TILESIZE = 80;

        // Game State
        public static Gamestate GAMESTATE;

        public Game1()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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

            TEST_ROOM = new Room("../../../TestArena2.txt", new Point(0, 0));

            // Add a door to each doorway in test room
            foreach (Tile d in TEST_ROOM.Floor.Doors)
            {
                TEST_ROOM.Interactables.Add(
                    new MapOBJ(
                        d.WorldPosition,
                        Content.Load<Texture2D>("PlaceholderDoor"),
                        MapObJType.Door));
            }

            // Create Dungeon
            //_roomManager = new RoomManager(10);

            // Create Player
            Player1 = new Player(_spriteSheetTexture, new Vector2(WINDOW_WIDTH / 3, WINDOW_HEIGHT / 2),
                Graphics, _roomManager, this); ;

            EManager = new EnemyManager(this);

            _ui = new UI(this);

            GAMESTATE = Gamestate.Menu;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            

            KeyboardState kb = Keyboard.GetState();
            switch (GAMESTATE)
            {
                case Gamestate.Menu:
                    if (kb.IsKeyDown(Keys.Enter))
                    {
                        GAMESTATE = Gamestate.Play;
                    }
                    break;

                case Gamestate.Play:
                    Player1.Update(gameTime);

                    EManager.Update(gameTime);

                    if (EManager.Dummies.Count == 0)
                    {
                        TEST_ROOM.Interactables.Clear();
                    }
                    break;

                case Gamestate.Death:

                    // Reset stage with 'R'
                    if (kb.IsKeyDown(Keys.R))
                    {
                        EManager.ResetRoomEnemies(TEST_ROOM);

                        Player1.Reset();

                        GAMESTATE = Gamestate.Play;
                    }
                    break;

            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            //_roomManager.Draw(_spriteBatch, gameTime);

            //_spriteBatch.DrawString(ARIAL32, Player1.NumRedirects.ToString(), new Vector2(_player.Position.X + Player.DEFAULT_SPRITE_WIDTH / 2, _player.Position.Y - 40f), Color.White);

            switch (GAMESTATE)
            {
                case Gamestate.Menu:

                    string titleText = "Press 'Enter' to begin testing";
                    Vector2 titleMeasure = ARIAL32.MeasureString(titleText);

                    _spriteBatch.DrawString(
                        ARIAL32,
                        titleText,
                        new Vector2(
                            WINDOW_WIDTH / 2f - titleMeasure.X / 2f,
                            WINDOW_HEIGHT / 2f - titleMeasure.Y / 2f),
                        Color.White);
                    break;

                case Gamestate.Play:
                    TEST_ROOM.Draw(_spriteBatch, gameTime);
                    foreach (MapOBJ obj in TEST_ROOM.Interactables)
                    {
                        obj.Draw(_spriteBatch, gameTime);
                    }

                    EManager.Draw(_spriteBatch, gameTime);

                    Player1.Draw(_spriteBatch, gameTime);

                    _ui.Draw(_spriteBatch, gameTime);
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

            _spriteBatch.End();

            ShapeBatch.Begin(GraphicsDevice);

            // Draw Gizmos
            if (GAMESTATE == Gamestate.Play)
            {
                EManager.DrawGizmos(gameTime);

                Player1.DrawGizmos();
            }

            ShapeBatch.End();

            base.Draw(gameTime);
        }
    }
}