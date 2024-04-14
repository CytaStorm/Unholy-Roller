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
		private Color _curImageTint;

		private string _text;
		private SpriteFont _font;
		#endregion

		#region Properties
		public Point Position => Bounds.Location;
		public Rectangle Bounds { get; set; }

		public Color TextColor { get; set; } = Color.White;
		public Color StaticTint { get; set; } = Color.White;
		public Color HoverTint { get; set; } = Color.White;
		public Color PressTint { get; set; } = Color.White;

		public bool IsBeingPressed =>
			ContainsCursor &&
			Game1.IsMouseButtonPressed(1);

		public bool IsHoveredOver =>
			ContainsCursor &&
			Game1.IsMouseButtonReleased(1);

		public bool ContainsCursor => 
			Bounds.Contains(Game1.CurMouse.Position);
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

			this.Bounds = bounds;

			_curImage = _staticImage;
		}

		// Methods
		public void Update(GameTime gameTime)
		{
			_curImage = _staticImage;

			_curImageTint = StaticTint;

			// Check if under Mouse
			if (Bounds.Contains(Game1.CurMouse.Position))
			{   
				if (Game1.IsMouseButtonClicked(Game1.Player.LaunchButton))
				{
					// Do something
					if (OnClicked != null) OnClicked();
				}

				if (Game1.IsMouseButtonPressed(Game1.Player.LaunchButton))
				{
					_curImage = _pressedImage;
					_curImageTint = PressTint;
				}
				else if (Game1.IsMouseButtonReleased(Game1.Player.LaunchButton))
				{
					_curImage = _hoverImage;
					_curImageTint = HoverTint;
				}
			}
		}

		public void Draw(SpriteBatch sb)
		{
			// Draw current button image
			if (_curImage != null)
			{
                sb.Draw(
                    _curImage,
                    Bounds,
                    _curImageTint);
            }
				

			// Draw text if it exists
			if (!string.IsNullOrEmpty(_text) && _font != null)
			{
				Vector2 textDimensions = _font.MeasureString(_text);

				// Center text in button
				Vector2 textPos = new Vector2(
					Bounds.X + Bounds.Width / 2 - textDimensions.X / 2,
					Bounds.Y + Bounds.Height / 2 - textDimensions.Y / 2);

				// Multiply text color by image tint
                Color combination = 
					new Color(TextColor.ToVector3() * _curImageTint.ToVector3());

                // Draw text
                sb.DrawString(
					_font,
					_text,
					textPos,
					combination);

				
			}
		}

		public void SetText(string text, SpriteFont font)
		{
			this._text = text;
			this._font = font;
		}

	}
}
