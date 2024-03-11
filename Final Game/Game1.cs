using Final_Game.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Final_Game
{
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		private Player _player;
		private Texture2D _cursorTexture;

        public static int WindowWidth = 1920;
        public static int WindowHeight = 1080;

        #region Mouse Properties
        public static MouseState CurMouse { get; private set; }
		public static MouseState PrevMouse { get; private set; }
		public static bool MouseIsOnScreen => ScreenBounds.Contains(CurMouse.Position);
        #endregion

        public static Rectangle ScreenBounds 
		{
			get => new Rectangle(0, 0, WindowWidth, WindowHeight);
		}

		public static int TileSize { get; private set; } = 100;

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

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_player = new Player(this, new Vector2(300, 300));

			_cursorTexture = Content.Load<Texture2D>("CursorSprite");

			// Create custom cursor
			Mouse.SetCursor(MouseCursor.FromTexture2D(
				_cursorTexture, _cursorTexture.Width / 2, _cursorTexture.Height / 2));
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			CurMouse = Mouse.GetState();

			if (this.IsActive)
				_player.Update(gameTime);

			PrevMouse = CurMouse;

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			_spriteBatch.Begin();

			_player.Draw(_spriteBatch);

			_spriteBatch.End();


			base.Draw(gameTime);
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
    }
}