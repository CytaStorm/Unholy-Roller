using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
	public class Sprite
	{

		#region Fields
		public Texture2D Texture { get; private set; }
		public Rectangle SourceRect { get; set; }
		public Rectangle DestinationRect { get; set; }
		public Color TintColor { get; set; }

		/// <summary>
		/// Determines whether or not the sprite can be scaled by the camera
		/// </summary>
		public bool ObeyCamera { get; set; } = true;

		#endregion

		public Sprite(Texture2D texture, Rectangle sourceRect, Rectangle destinationRect)
		{
			Texture = texture;
			SourceRect = sourceRect;
			DestinationRect = destinationRect;
			TintColor = Color.White;
		}

		public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 origin)
		{
			float scale = (float) DestinationRect.Width / SourceRect.Width;

			if (ObeyCamera)
			{
				scale *= Game1.MainCamera.Zoom;

                spriteBatch.Draw(Texture,
                Game1.MainCamera.GetPerspectivePosition(position),
                SourceRect,
                TintColor,
                rotation,
                origin,
                scale,
                SpriteEffects.None,
                0);

            }
			else
			{
				spriteBatch.Draw(Texture,
					position,
					SourceRect,
					TintColor,
					0f,
					Vector2.Zero,
					scale,
					SpriteEffects.None,
					0);
			}
		}
	}
}
