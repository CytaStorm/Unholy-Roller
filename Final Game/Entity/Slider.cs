using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Entity
{
	public class Slider
	{
		#region Fields
		private Rectangle _knobBounds;
		private Texture2D _knobImage;

		private Texture2D _barImage;

		private Point _position;

		private bool _focused; // = Held & not released
		#endregion

		#region Properties

		public float CurValue { get; set; }
		public float MaxValue { get; set; }

		#endregion

		// Constructors
		public Slider(Point position, Texture2D barImage, Texture2D knobImage)
		{
			_position = position;

			_barImage = barImage;

			_knobImage = knobImage;
			_knobBounds = new Rectangle(
				position.X - _knobImage.Width / 2,
				position.Y,
				knobImage.Width,
				knobImage.Height);

			MaxValue = 1f; 
		}

		// Methods
		public void Update(GameTime gameTime)
		{
			// Check if user is holding knob
			if (Game1.IsMouseButtonPressed(1) &&
			  _knobBounds.Contains(Game1.CurMouse.Position)){
				_focused = true;
			}
			else if (Game1.IsMouseButtonReleased(1))
			{
				_focused = false;
			} 

			// Move knob if holding it
			if (_focused)
			{
				// Move knob with mouse
				_knobBounds.X = Game1.CurMouse.Position.X - _knobBounds.Width / 2;

				// Set value based on center of knob
				CurValue = 
					MaxValue * (float)(_knobBounds.Center.X - _position.X) / _barImage.Width;

				// User has not released knob
				_focused = true;
			}

			// Clamp knob to slider
			if (_knobBounds.Center.X < _position.X)
				_knobBounds.X = _position.X - _knobBounds.Width / 2;

			if (_knobBounds.Center.X > _position.X + _barImage.Width)
				_knobBounds.X = _position.X + _barImage.Width - _knobBounds.Width / 2;

			// Clamp slider value
			CurValue = MathHelper.Clamp(CurValue, 0, MaxValue);
		}

		public void Draw(SpriteBatch sb)
		{
			// Draw Bar
			sb.Draw(
				_barImage,
				_position.ToVector2(),
				Color.White);

			// Draw Knob
			sb.Draw(_knobImage,
				_knobBounds,
				Color.White);
		}
	}
}
