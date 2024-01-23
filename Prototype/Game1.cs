using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Prototype
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _spriteSheetTexture;

        private SpriteFont _arial32;

        private Player _player;

        //private Dummy _dummy;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _spriteSheetTexture = Content.Load<Texture2D>("Cthulu_Muggles");

            _arial32 = Content.Load<SpriteFont>("arial32");

            _player = new Player(_spriteSheetTexture, new Vector2(20, 20), _graphics);

            //_dummy = new Dummy(_spriteSheetTexture, new Vector2(100, 100), _graphics);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _player.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _spriteBatch.DrawString(_arial32, _player.NumRedirects.ToString(), new Vector2(_player.Position.X + Player.DEFAULT_SPRITE_WIDTH/2, _player.Position.Y - 40f), Color.White);
            _player.Draw(_spriteBatch, gameTime);
            //_dummy.Draw(_spriteBatch, gameTime);

            _spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}