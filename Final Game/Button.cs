using Final_Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
	public delegate void ButtonClicked();

	public class Button
	{
		public event ButtonClicked OnClicked;

		#region Fields
		private Texture2D _staticImage;
		private Texture2D _hoverImage;
		private Texture2D _pressedImage;

		private Texture2D _curImage;

		private Rectangle _bounds;

		private string _text;
		private SpriteFont _font;
		#endregion

		#region Properties
		public Color TextColor { get; set; } = Color.White;
		public Color TintColor { get; set; } = Color.White;
		#endregion

		// Constructors
		public Button(
			Rectangle bounds, 
			Texture2D staticImage, 
			Texture2D hoverImage,
			Texture2D pressedImage)
		{
			this._staticImage = staticImage;
			this._hoverImage = hoverImage;
			this._pressedImage = pressedImage;

			this._bounds = bounds;

			_curImage = _staticImage;
		}

		// Methods
		public void Update(GameTime gameTime)
		{
			_curImage = _staticImage;

			// Check if underMouse
			if (_bounds.Contains(Game1.CurMouse.Position))
			{   
				if (Game1.IsMouseLeftClicked())
				{
					// Do something
					if (OnClicked != null) OnClicked();
				}

				if (Game1.IsMouseButtonPressed(1))
				{
					_curImage = _pressedImage;
				}
				else if (Game1.IsMouseButtonReleased(1))
				{
					_curImage = _hoverImage;
				}
			}
		}

		public void Draw(SpriteBatch sb)
		{
			// Draw current button image
			sb.Draw(
				_curImage,
				_bounds,
				TintColor);

			// Draw text if it exists
			if (!string.IsNullOrEmpty(_text) && _font != null)
			{
				Vector2 textDimensions = _font.MeasureString(_text);

				// Center text in button
				Vector2 textPos = new Vector2(
					_bounds.X + _bounds.Width / 2 - textDimensions.X / 2,
					_bounds.Y + _bounds.Height / 2 - textDimensions.Y / 2);

				// Draw text
				sb.DrawString(
					_font,
					_text,
					textPos,
					TextColor);
			}
		}

		public void SetText(string text, SpriteFont font)
		{
			this._text = text;
			this._font = font;
		}

	}
}
