using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    internal class Sprite
    {

        // Properties
        public Texture2D Texture { get; private set; }

        public Rectangle SourceRect { get; private set; }

        //public Vector2 Scale { get; set; }

        public Color TintColor { get; set; }

        /// <summary>
        /// Creates a sprite that contains a 'cut-out' of the given texture
        /// from x to x + width and y to y + height
        /// </summary>
        /// <param name="texture"> the texture to cut from </param>
        /// <param name="x"> the x position of the image on the texture </param>
        /// <param name="y"> the y position of the image on the texture </param>
        /// <param name="width"> the width of the image on the texture </param>
        /// <param name="height"> the height of the image on the texture </param>
        public Sprite(Texture2D texture, int x, int y, int width, int height) :
            this(texture, x, y, width, height, new Vector2(width, height)) { }


        public Sprite(Texture2D texture, int x, int y, int width, int height, Vector2 scale)
        {
            Texture = texture;

            SourceRect = new Rectangle(x, y, width, height);

            //Scale = scale;

            TintColor = Color.White;
        }

        public Sprite(Texture2D texture, Rectangle sourceRect)
        {
            Texture = texture;
            SourceRect = sourceRect;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(Texture, position, SourceRect, TintColor);
        }
        
    }
}
