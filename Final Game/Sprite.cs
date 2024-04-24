using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
	public class Sprite
	{

		#region Properties
		public Texture2D Texture { get; private set; }
		public Rectangle SourceRect { get; set; }
		public Rectangle DestinationRect { get; set; }
		public Color TintColor { get; set; }

		/// <summary>
		/// Transparency of sprite (0-1 inclusive)
		/// </summary>
		public float AlphaMultiplier { get; set; } = 1f;

		/// <summary>
		/// The uniform bounding rectangle of a frame
		/// of this sprite's animation
		/// </summary>
		public Rectangle FrameBounds { get; set; }

		/// <summary>
		/// The number of frames of animation contained in each row of this sprite's texture
		/// </summary>
		public int Columns { get; set; }

		/// <summary>
		/// The number of rows of frames contained in this sprite's texture
		/// </summary>
		public int Rows { get; set; }

		/// <summary>
		/// Number of animation frames represented by this sprite
		/// </summary>
		public int NumFrames => Columns * Rows;

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
			Draw(spriteBatch, position, rotation, origin, SpriteEffects.None);
		}

		public void Draw(SpriteBatch spriteBatch, Vector2 position, float rotation, Vector2 origin, SpriteEffects spFx)
		{
			float scale = (float)DestinationRect.Width / SourceRect.Width;

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
				TintColor * AlphaMultiplier,
				rotation,
				origin,
				scale,
				spFx,
				0);
				return;
			}

			//Doesn't obey camera.
			spriteBatch.Draw(Texture,
				position,
				SourceRect,
				TintColor * AlphaMultiplier,
				0f,
				Vector2.Zero,
				scale,
				spFx,
				0);
			return;
		}

		/// <summary>
		/// Adjusts the source rectangle to represent
		/// the specified frame of animation
		/// </summary>
		/// <param name="frameNum"> frame number (one-based) </param>
		public void SetSourceToFrame(int frameNum)
		{
			if (frameNum < 1) 
				throw new Exception("Frame number cannot be less than 1");

			SourceRect = new Rectangle(
				(frameNum - 1) % Columns * FrameBounds.Width,
				(frameNum - 1) / (Columns + 1) * FrameBounds.Height,
				FrameBounds.Width,
				FrameBounds.Height);
		}


    }
}
