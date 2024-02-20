using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    public class UI
    {
        // Fields
        private Game1 _gm;

        private Sprite _heart;
        private Sprite _halfHeart;
        private Sprite _emptyHeart;

        // Constructors
        public UI(Game1 gm)
        {
            _gm = gm;

            // Heart image
            Texture2D heart = _gm.Content.Load<Texture2D>("BowlingHeart");
            _heart = new Sprite(
                heart,
                new Rectangle(0, 0, 100, 100),
                new Rectangle(0, 0, 100, 100));

            Texture2D halfHeart = _gm.Content.Load<Texture2D>("BowlingHalfHeart");
            _halfHeart = new Sprite(
                halfHeart,
                new Rectangle(0, 0, 100, 100),
                new Rectangle(0, 0, 100, 100));

            Texture2D emptyHeart = _gm.Content.Load<Texture2D>("EmptyHeart");
            _emptyHeart = new Sprite(
                emptyHeart,
                new Rectangle(0, 0, 100, 100),
                new Rectangle(0, 0, 100, 100));
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Display current health
            //spriteBatch.DrawString(Game1.ARIAL32, $"Hp: {Game1.Player1.CurHealth}", ScreenPosition, Color.White);

            int temp = Game1.Player1.CurHealth;
            // Draw whole hearts
            for (int i = 0; i < (int)Math.Round(Game1.Player1.MaxHealth / 2.0); i++)
            {
                if (temp / 2 >= 1)
                {
                    _heart.Draw(spriteBatch, new Vector2(i * 105f, 10f));
                    temp -= 2;
                }
                else if (temp > 0)
                {
                    _halfHeart.Draw(spriteBatch, new Vector2(i * 105f, 10f));
                    temp -= 1;
                }
                else
                {
                    _emptyHeart.Draw(spriteBatch, new Vector2(i * 105f, 10f));
                }
            }
        }
    }
}
