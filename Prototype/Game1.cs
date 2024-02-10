using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prototype.GameEntity;
using Prototype.MapGeneration;

namespace Prototype
{
    public enum Gamestate
    {
        Play,
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

        // Entities
        public static Player Player1 { get; private set; }
        public static EnemyManager EManager { get; private set; }
        
        // Screen
        public GraphicsDeviceManager Graphics { get; private set; }
        public const int WINDOW_WIDTH = 1920;
        public const int WINDOW_HEIGHT = 1080;

        public const int TILESIZE = 80;

        // Game State
        public static Gamestate GAMESTATE = Gamestate.Play;

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
            _spriteSheetTexture = Content.Load<Texture2D>("Cthulu_Muggles");

            _tileTextures = new Texture2D[5];
            _tileTextures[0] = Content.Load<Texture2D>("PlaceholderTile");
            _tileTextures[1] = Content.Load<Texture2D>("GrassTile");
            _tileTextures[2] = Content.Load<Texture2D>("WallSheet");
            _tileTextures[3] = Content.Load<Texture2D>("SpikeTile");

            ARIAL32 = Content.Load<SpriteFont>("arial32");

            // Create Tile Manager
            _tileMaker = new TileMaker(_tileTextures);

            TEST_ROOM = new Room("../../../TestArena18x18.txt", new Point(0, 0));

            // Create Dungeon
            //_roomManager = new RoomManager(10);
            
            // Create Player
            Player1 = new Player(_spriteSheetTexture, new Vector2(WINDOW_WIDTH/3 + Game1.TILESIZE, WINDOW_HEIGHT/3 + Game1.TILESIZE), 
                Graphics, _roomManager);

            EManager = new EnemyManager(this);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            if (GAMESTATE == Gamestate.Play)
            {
                Player1.Update(gameTime);

                EManager.Update(gameTime);
            }


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            //_roomManager.Draw(_spriteBatch, gameTime);

            //_spriteBatch.DrawString(_arial32, _player.NumRedirects.ToString(), new Vector2(_player.Position.X + Player.DEFAULT_SPRITE_WIDTH/2, _player.Position.Y - 40f), Color.White);

            if (GAMESTATE == Gamestate.Play)
            {
                TEST_ROOM.Draw(_spriteBatch, gameTime);

                EManager.Draw(_spriteBatch, gameTime);

                Player1.Draw(_spriteBatch, gameTime);
            }
            else if (GAMESTATE == Gamestate.Death)
            {
                _spriteBatch.DrawString(
                    ARIAL32,
                    "Game Over",
                    new Vector2(WINDOW_WIDTH / 2f, WINDOW_HEIGHT / 2f),
                    Color.White);
            }


            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}