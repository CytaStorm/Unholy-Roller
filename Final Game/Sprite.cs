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

				Vector2 perspectivePos = Game1.MainCamera.GetPerspectivePosition(position);

				// Only draw if on screen
				Rectangle screenHit = new Rectangle(
					(int)perspectivePos.X,
					(int)perspectivePos.Y,
					(int)(DestinationRect.Width * Game1.MainCamera.Zoom),
					(int)(DestinationRect.Height * Game1.MainCamera.Zoom));

				bool onScreen = screenHit.Intersects(Game1.ScreenBounds);
				if (!onScreen) return;

				spriteBatch.Draw(Texture,
                perspectivePos,
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
        public void Draw(SpriteBatch spriteBatch, Vector2 position, Color tint)
        {
            float scale = (float)DestinationRect.Width / SourceRect.Width;

            spriteBatch.Draw(Texture,
                position,
                SourceRect,
                tint,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                0);
        }

    }
}
