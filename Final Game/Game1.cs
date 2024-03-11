using Final_Game.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.ComponentModel.DataAnnotations;

namespace Final_Game
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		private Player _player;
		private Texture2D _cursorTexture;

		public static MouseState CurMouse { get; private set; }
		public static MouseState PrevMouse { get; private set; }

		public static int TileSize { get; private set; } = 100;

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = false;

			_graphics.PreferredBackBufferWidth = 1920;
			_graphics.PreferredBackBufferHeight = 1080;
			_graphics.ApplyChanges();
		}

		protected override void Initialize()
		{
			// TODO: Add your initialization logic here

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_player = new Player(this, new Vector2(300, 300));

			_cursorTexture = Content.Load<Texture2D>("CursorSprite");
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			CurMouse = Mouse.GetState();

			_player.Update(gameTime);

			PrevMouse = CurMouse;

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			_spriteBatch.Begin();

			_player.Draw(_spriteBatch);

			// Draw custom cursor
			_spriteBatch.Draw(
				_cursorTexture,
				new Vector2(
					CurMouse.X - _cursorTexture.Width / 2,
					CurMouse.Y - _cursorTexture.Height / 2),
				Color.White);

			_spriteBatch.End();

			base.Draw(gameTime);
		}

		public static bool MouseLeftClicked()
		{
			return
				CurMouse.LeftButton == ButtonState.Released &&
				PrevMouse.LeftButton == ButtonState.Pressed;
		}
	}
}