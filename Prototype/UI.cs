using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prototype.GameEntity;
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
        private SpriteBatch _sb;

        private Sprite _heart;
        private Sprite _halfHeart;
        private Sprite _emptyHeart;

        // Font fields
        private SpriteFont _titleCaseArial;

        // Constructors
        public UI(Game1 gm, SpriteBatch sb)
        {
            _gm = gm;
            _sb = sb;

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

            // Load Fonts
            _titleCaseArial = _gm.Content.Load<SpriteFont>("TitleCaseArial");
        }

        public void Draw(GameTime gameTime)
        {
            // Display current health
            //spriteBatch.DrawString(Game1.ARIAL32, $"Hp: {Game1.Player1.CurHealth}", ScreenPosition, Color.White);

            switch (Game1.GAMESTATE)
            {
                case Gamestate.Play:
                    // Draw whole hearts
                    int temp = Game1.Player1.CurHealth;
                    for (int i = 0; i < (int)Math.Round(Game1.Player1.MaxHealth / 2.0); i++)
                    {
                        if (temp / 2 >= 1)
                        {
                            _heart.Draw(_sb, new Vector2(i * 105f, 10f));
                            temp -= 2;
                        }
                        else if (temp > 0)
                        {
                            _halfHeart.Draw(_sb, new Vector2(i * 105f, 10f));
                            temp -= 1;
                        }
                        else
                        {
                            _emptyHeart.Draw(_sb, new Vector2(i * 105f, 10f));
                        }
                    }

                    // Display Bullet Time multiplier
                    _sb.DrawString(
                        Game1.ARIAL32,
                        $"Time Multiplier: {Player.BulletTimeMultiplier:0.00}",
                        new Vector2(0f, 200f),
                        Color.White);
                    break;

                case Gamestate.Pause:
                    DrawPauseMenu();
                    break;
            }

        }

        public void DrawPauseMenu()
        {
            // Draw Heading
            string pauseHeadingText = "Paused";
            Vector2 pauseHeadingDimensions = _titleCaseArial.MeasureString(pauseHeadingText);

            _sb.DrawString(
                _titleCaseArial,
                pauseHeadingText,
                new Vector2(
                    _gm.Graphics.PreferredBackBufferWidth / 2 - pauseHeadingDimensions.X / 2,
                    Game1.TILESIZE * 2),
                Color.White);

            // Draw resume instructions
            string resumeText = "Press 'P' to Resume";
            Vector2 resumeTextDimensions = Game1.ARIAL32.MeasureString(resumeText);

            _sb.DrawString(
                Game1.ARIAL32,
                resumeText,
                new Vector2(
                    _gm.Graphics.PreferredBackBufferWidth / 2 - resumeTextDimensions.X / 2,
                    _gm.Graphics.PreferredBackBufferHeight / 2 - resumeTextDimensions.Y / 2),
                Color.White);
        }
    }
}
