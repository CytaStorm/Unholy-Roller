using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prototype.MapGeneration;

namespace Prototype
{
    public class Game1 : Game
    {
        private SpriteBatch _spriteBatch;

        private Texture2D _spriteSheetTexture;

        // Map Generation
        private TileMaker _tileMaker;
        private Texture2D[] _tileTextures;
        private RoomManager _roomManager;

        // UI
        private SpriteFont _arial32;

        // Entities
        private Player _player;
        private DummyManager _dManager;
        
        // Screen
        public GraphicsDeviceManager Graphics { get; private set; }
        public const int WINDOW_WIDTH = 1920;
        public const int WINDOW_HEIGHT = 1080;

        public const int TILESIZE = 60;

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

            _arial32 = Content.Load<SpriteFont>("arial32");

            // Create Tile Manager
            _tileMaker = new TileMaker(_tileTextures);

            // Test Tileset
            _roomManager = new RoomManager(10);
            
            _player = new Player(_spriteSheetTexture, new Vector2(Game1.WINDOW_WIDTH/2, Game1.WINDOW_HEIGHT/2), 
                Graphics, _roomManager);

            //_dManager = new DummyManager(this);

            //_dummy = new Dummy(_spriteSheetTexture, new Vector2(100, 100), _graphics);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _player.Update(gameTime);

            //_dManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _roomManager.Draw(_spriteBatch, gameTime);
            
            //_spriteBatch.DrawString(_arial32, _player.NumRedirects.ToString(), new Vector2(_player.Position.X + Player.DEFAULT_SPRITE_WIDTH/2, _player.Position.Y - 40f), Color.White);


            //_dManager.Draw(_spriteBatch, gameTime);

            _player.Draw(_spriteBatch, gameTime);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}