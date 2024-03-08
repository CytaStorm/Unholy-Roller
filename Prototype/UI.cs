using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prototype.GameEntity;
using ShapeUtils;
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
        private SpriteBatch _spriteBatch;

        private Sprite _heart;
        private Sprite _halfHeart;
        private Sprite _emptyHeart;

        // Font fields
        private SpriteFont _titleCaseArial;

        // Buttons
        public Button[] MenuButtons { get; private set; }
        public Button[] PauseButtons { get; private set; }

        public bool DontUpdatePlay { get; private set; }

        private static MouseState _prevMouse;

        // Constructors
        public UI(Game1 gm, SpriteBatch sb)
        {
            _gm = gm;
            _spriteBatch = sb;

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

            Texture2D emptyButton = _gm.Content.Load<Texture2D>("EmptyButton");

            // Load Fonts
            _titleCaseArial = _gm.Content.Load<SpriteFont>("TitleCaseArial");

            // Make menu buttons
            Rectangle buttonBounds = new Rectangle(
                Game1.WINDOW_WIDTH / 2 - emptyButton.Bounds.Width / 2,
                400,
                emptyButton.Bounds.Width,
                emptyButton.Bounds.Height);

            MenuButtons = new Button[3];
            MenuButtons[0] = new Button(buttonBounds, emptyButton, emptyButton);
            MenuButtons[0].TintColor = Color.Blue;
            MenuButtons[0].SetText("Play", _titleCaseArial);
            buttonBounds.Y += emptyButton.Height;

            MenuButtons[1] = new Button(buttonBounds, emptyButton, emptyButton);
            MenuButtons[1].TextColor = Color.Coral;
            MenuButtons[1].SetText("Tutorial", _titleCaseArial);

            buttonBounds.Y += emptyButton.Height;
            MenuButtons[2] = new Button(buttonBounds, emptyButton, emptyButton);
            MenuButtons[2].TintColor = Color.Orange;
            MenuButtons[2].TextColor = Color.Purple;
            MenuButtons[2].SetText("Quit", _titleCaseArial);

            // Make pause buttons
            buttonBounds.Y = 400;

            PauseButtons = new Button[2];
            PauseButtons[0] = new Button(buttonBounds, emptyButton, emptyButton);
            PauseButtons[0].TintColor = Color.Blue;
            PauseButtons[0].SetText("Resume", _titleCaseArial);
            buttonBounds.Y += emptyButton.Height;

            PauseButtons[1] = new Button(buttonBounds, emptyButton, emptyButton);
            PauseButtons[1].TextColor = Color.Coral;
            PauseButtons[1].SetText("Main Menu", _titleCaseArial);
        }

        public void Update(GameTime gameTime)
        {
            MouseState mState = Mouse.GetState();
            switch (Game1.GAMESTATE)
            {
                case Gamestate.Menu:
                    // Update menu buttons
                    foreach (Button b in MenuButtons)
                    {
                        b.Update(gameTime, mState);
                    }
                    break;

                case Gamestate.Pause:
                    foreach (Button b in PauseButtons)
                    {
                        b.Update(gameTime, mState);
                    }
                    break;
            }
            _prevMouse = mState;

        }

        public void Draw(GameTime gameTime)
        {
            // Display current health
            //spriteBatch.DrawString(Game1.ARIAL32, $"Hp: {Game1.Player1.CurHealth}", ScreenPosition, Color.White);

            switch (Game1.GAMESTATE)
            {
                case Gamestate.Menu:
                    //string controls =
                    //    "WASD - Move (if walking)\n" +
                    //    "Left Mouse - Slow time/Prime launch\n" +
                    //    "Left Mouse Release - Launch\n" +
                    //    "Right Mouse - Brake";

                    //_spriteBatch.DrawString(
                    //    Game1.ARIAL32,
                    //    controls,
                    //    new Vector2(200f, 50f),
                    //    Color.White);


                    //string titleText = "Press 'Enter' to begin testing";
                    //Vector2 titleMeasure = Game1.ARIAL32.MeasureString(titleText);

                    //_spriteBatch.DrawString(
                    //    Game1.ARIAL32,
                    //    titleText,
                    //    new Vector2(
                    //        Game1.WINDOW_WIDTH / 2f - titleMeasure.X / 2f,
                    //        Game1.WINDOW_HEIGHT / 2f - titleMeasure.Y / 2f),
                    //    Color.White);


                    // Draw Title

                    string titleText = "UnHoly Roller";
                    Vector2 titleMeasure = _titleCaseArial.MeasureString(titleText);

                    _spriteBatch.DrawString(
                        _titleCaseArial,
                        titleText,
                        new Vector2(
                            Game1.WINDOW_WIDTH / 2f - titleMeasure.X / 2f,
                            150f),
                        Color.White);

                    foreach (Button b in MenuButtons)
                    {
                        b.Draw(_spriteBatch);
                    }
                    break;

                case Gamestate.Play:
                    // Draw whole hearts
                    int temp = Game1.Player1.CurHealth;
                    for (int i = 0; i < (int)Math.Round(Game1.Player1.MaxHealth / 2.0); i++)
                    {
                        if (temp / 2 >= 1)
                        {
                            _heart.Draw(_spriteBatch, new Vector2(i * 105f, 10f));
                            temp -= 2;
                        }
                        else if (temp > 0)
                        {
                            _halfHeart.Draw(_spriteBatch, new Vector2(i * 105f, 10f));
                            temp -= 1;
                        }
                        else
                        {
                            _emptyHeart.Draw(_spriteBatch, new Vector2(i * 105f, 10f));
                        }
                    }

                    // Display Bullet Time multiplier
                    _spriteBatch.DrawString(
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

            _spriteBatch.DrawString(
                _titleCaseArial,
                pauseHeadingText,
                new Vector2(
                    _gm.Graphics.PreferredBackBufferWidth / 2 - pauseHeadingDimensions.X / 2,
                    Game1.TILESIZE * 2),
                Color.White);

            // Draw resume instructions
            //string resumeText = "Press 'Escape' to Resume";
            //Vector2 resumeTextDimensions = Game1.ARIAL32.MeasureString(resumeText);

            //_spriteBatch.DrawString(
            //    Game1.ARIAL32,
            //    resumeText,
            //    new Vector2(
            //        _gm.Graphics.PreferredBackBufferWidth / 2 - resumeTextDimensions.X / 2,
            //        _gm.Graphics.PreferredBackBufferHeight / 2 - resumeTextDimensions.Y / 2),
            //    Color.White);

            // Draw pause buttons
            foreach (Button b in PauseButtons)
            {
                b.Draw(_spriteBatch);
            }
        }

        public void DrawTutorialTooltipBar(GraphicsDevice gd, SpriteBatch sb)
        {
            // Draw a box over the left side of the screen

            Rectangle subScreen = new Rectangle(
                0, 0, Game1.TILESIZE * 2, Game1.WINDOW_HEIGHT);

            ShapeBatch.Begin(gd);

            ShapeBatch.Box(subScreen, Color.DarkGray);

            ShapeBatch.End();

            // Draw tutorial text
            _spriteBatch.Begin();


            _spriteBatch.End();
        }

        public static bool MouseClick(string button)
        {
            MouseState m = Mouse.GetState();
            if (button == "left")
            {
                return
                    m.LeftButton == ButtonState.Released &&
                    _prevMouse.LeftButton == ButtonState.Pressed;
            }
            else if (button == "right")
            {
                return
                    m.RightButton == ButtonState.Released &&
                    _prevMouse.RightButton == ButtonState.Pressed;
            }

            throw new Exception("Invalid mouse key");
        }
    }
}
