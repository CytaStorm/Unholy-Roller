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
		public Rectangle SourceRect { get; private set; }
		public Rectangle DestinationRect { get; private set; }
		public Color TintColor { get; set; }
		#endregion

		public Sprite(Texture2D texture, Rectangle sourceRect, Rectangle destinationRect)
		{
			Texture = texture;
			SourceRect = sourceRect;
			DestinationRect = destinationRect;
			TintColor = Color.White;
		}

		public void Draw(SpriteBatch spriteBatch, Vector2 position)
		{
			float scale = (float)DestinationRect.Width / SourceRect.Width;
			
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

		public void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Draw(Texture, 
				DestinationRect, 
				SourceRect, 
				TintColor, 
				0f, 
				Vector2.Zero, 
				SpriteEffects.None, 
				0);
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
