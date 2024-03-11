using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Entity
{
    public enum PlayerState
    {
        Rolling,
        Walking
    }

    public class Player : Entity
    {
        // Fields
        private Texture2D _launchArrowsTexture;
        private int _launchArrowSpriteWidth;

        private int _numRedirects;
        private int _maxRedirects;

        private float _brakeForce;

        // Properties
        public Texture2D Image { get; private set; }
        public PlayerState State { get; private set; }

        // Constructors

        public Player(Game1 gm, Vector2 worldPosition)
        {
            Image = gm.Content.Load<Texture2D>("BasicBlueClean");
            _launchArrowsTexture = gm.Content.Load<Texture2D>("LaunchArrowSpritesheet");

            int numLaunchArrows = 2;
            _launchArrowSpriteWidth = _launchArrowsTexture.Width / numLaunchArrows;

            WorldPosition = worldPosition;

            Speed = 10f;
            _brakeForce = 0.2f;

            State = PlayerState.Walking;

            _maxRedirects = 3;
            _numRedirects = _maxRedirects + 1;
        }

        // Methods

        public override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();

            switch (State)
            {
                case PlayerState.Walking:
                    MoveWithKeyboard(kb);   
                    break;

                case PlayerState.Rolling:
                    HandleBraking();

                    // Transition to walking
                    if (Velocity.Length() < 1f)
                    {
                        State = PlayerState.Walking;

                        _numRedirects = _maxRedirects + 1;
                    }
                    break;
            }

            HandleLaunch();

            Move(Velocity);
        }

        public void MoveWithKeyboard(KeyboardState kb)
        {
            Velocity = Vector2.Zero;

            // Move up
            if (kb.IsKeyDown(Keys.W))
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y - Speed);
            }
            // Move down
            if (kb.IsKeyDown(Keys.S))
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y + Speed);
            }
            // Move left
            if (kb.IsKeyDown(Keys.A))
            {
                Velocity = new Vector2(Velocity.X - Speed, Velocity.Y);
            }
            // Move right
            if (kb.IsKeyDown(Keys.D))
            {
                Velocity = new Vector2(Velocity.X + Speed, Velocity.Y);
            }

            // Maximize Velocity at speed
            if (Velocity.LengthSquared() > Speed * Speed)
            {
                Velocity *= Speed / Velocity.Length();
            }
        }

        private void HandleLaunch()
        {
            if (Game1.IsMouseButtonPressed(1))
            {
                // Todo: Slow time
            }

            // Launch Player in direction of Mouse
            if (_numRedirects > 0 && Game1.IsMouseLeftClicked())
            {
                // Get mouse Position
                Vector2 mousePos = new Vector2(Game1.CurMouse.X, Game1.CurMouse.Y);

                // Aim from center of the Player
                Vector2 centerPos = new Vector2(WorldPosition.X + Image.Width / 2,
                    WorldPosition.Y + Image.Height / 2);

                // Aim toward mouse at player speed
                Vector2 distance = mousePos - centerPos;
                distance.Normalize();

                // Speed is less than max
                if (Velocity.LengthSquared() <= Speed * Speed)
                {
                    // Launch player at max speed
                    distance *= Speed;
                    Velocity = distance;
                }
                else
                {
                    // Launch player at current speed
                    distance *= Velocity.Length();
                    Velocity = distance;
                }

                // Launch Player at max speed
                // Redirect Player at cur speed
                //if (_numRedirects > _maxRedirects)
                //{
                //    // Launch player at default speed
                //    distance *= _speed;
                //    Velocity = distance;
                //}
                //else
                //{
                //    // Launch player at current speed
                //    distance *= Velocity.Length();
                //    Velocity = distance;
                //}

                _numRedirects--;

                // Player is now rolling
                State = PlayerState.Rolling;
            }
        }

        private void HandleBraking()
        {
            float lowestBrakableSpeed = 0.1f * 0.1f;

            if (Game1.IsMouseButtonPressed(2) && 
                Velocity.LengthSquared() >= lowestBrakableSpeed)
            {
                Vector2 deceleration = -Velocity;
                deceleration.Normalize();
                deceleration *= _brakeForce;

                Velocity += deceleration;
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            // Draw player image
            sb.Draw(
                Image,
                WorldPosition,
                Color.White);

            if (_numRedirects > 0 && 
                Game1.IsMouseButtonPressed(1))
            {
                DrawLaunchArrow(sb);
            }
        }

        private void DrawLaunchArrow(SpriteBatch sb)
        {
            
            // Get angle between arrow and mouse
            Vector2 mousePos = new Vector2(Game1.CurMouse.X, Game1.CurMouse.Y);

            Vector2 centerPlayerPos = new Vector2(
                WorldPosition.X + Image.Width / 2,
                WorldPosition.Y + Image.Height / 2);

            Vector2 playerToMouseDistance = mousePos - centerPlayerPos;

            float angleBetweenArrowAndMouse = MathF.Atan2(
                playerToMouseDistance.X,
                playerToMouseDistance.Y);

            // Scale distance from player to mouse for drawing
            Vector2 directionFromPlayerToMouse = playerToMouseDistance;
            directionFromPlayerToMouse.Normalize();
            directionFromPlayerToMouse *= 120; // Radius


            Rectangle arrowSourceRect = new Rectangle();
            if (_numRedirects > _maxRedirects)
            {
                // Launch Arrow
                arrowSourceRect = new Rectangle(
                    _launchArrowSpriteWidth, 0,
                    _launchArrowSpriteWidth, _launchArrowSpriteWidth);
            }
            else
            {
                // Redirect Arrow
                arrowSourceRect = new Rectangle(
                    0, 0,
                    _launchArrowSpriteWidth, _launchArrowSpriteWidth);
            }

            // Draw aiming arrow
            sb.Draw(
                _launchArrowsTexture,
                centerPlayerPos + directionFromPlayerToMouse,
                arrowSourceRect,
                Color.White,
                -angleBetweenArrowAndMouse,
                new Vector2(
                    _launchArrowSpriteWidth / 2,
                    _launchArrowSpriteWidth / 2),
                1f,
                SpriteEffects.None,
                0f
                );

            // Todo: Display remaining redirects
            //Vector2 redirectStringDimensions =
            //    Game1.ARIAL32.MeasureString(_numRedirects.ToString());

            //Vector2 textPos = centerPlayerPos + directionFromPlayerToMouse;
            //textPos = new Vector2(
            //    textPos.X - redirectStringDimensions.X / 2,
            //    textPos.Y - redirectStringDimensions.Y / 2);

            //sb.DrawString(
            //    Game1.ARIAL32,
            //    _numRedirects.ToString(),
            //    textPos,
            //    Color.White);
        }
    }
}
