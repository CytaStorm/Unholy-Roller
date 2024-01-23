using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    internal class Player : IGameEntity
    {
        // Fields
        public const int DEFAULT_SPRITE_X = 0;
        public const int DEFAULT_SPRITE_Y = 0;
        public const int DEFAULT_SPRITE_WIDTH = 120;
        public const int DEFAULT_SPRITE_HEIGHT = 120;

        private GraphicsDeviceManager _gdManager;

        private float _speed;

        private bool _canRedirect = true;

        private int _maxRedirects = 3;
        private int _numRedirects;

        // Properties

        /// <summary>
        /// The player's image
        /// </summary>
        public Sprite Sprite { get; private set; }

        /// <summary>
        /// The player's position
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        /// The player's current velocity
        /// </summary>
        public Vector2 Velocity { get; private set; }

        public int NumRedirects { get => _numRedirects; }

        public Player(Texture2D spriteSheet, Vector2 position, GraphicsDeviceManager gdManager)
        {
            // Set Player Image
            Sprite = new Sprite(spriteSheet, DEFAULT_SPRITE_X, DEFAULT_SPRITE_Y, 
                DEFAULT_SPRITE_WIDTH, DEFAULT_SPRITE_HEIGHT, new Vector2(50, 50));

            // Position
            Position = position;

            // Graphics Manager -> Screen Collision
            _gdManager = gdManager;

            // Default Speed
            _speed = 5f;

            // Default Velocity
            Velocity = new Vector2(_speed, _speed);

            // Redirecting
            _numRedirects = _maxRedirects;
        }


        // Methods
        public void Update(GameTime gameTime)
        {
            bool sideScreenCollision = Position.X < 0f || 
                Position.X + DEFAULT_SPRITE_WIDTH >= _gdManager.PreferredBackBufferWidth;

            bool topBottomScreenCollision = Position.Y < 0f ||
                Position.Y + DEFAULT_SPRITE_HEIGHT >= _gdManager.PreferredBackBufferHeight;

            // Player can Redirect if not Colliding with Screen Border
            if (!sideScreenCollision && !topBottomScreenCollision && _numRedirects > 0)
            {
                HandleLaunch();
            }

            // Check collisions with wall
            if (sideScreenCollision)
            {
                Velocity = new Vector2(Velocity.X * -1, Velocity.Y);

                // Reset Redirects
                _numRedirects = _maxRedirects;
            }
            if (topBottomScreenCollision)
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y * -1);

                // Reset Redirects
                _numRedirects = _maxRedirects;
            }

            // Move player based on velocity
            Position += Velocity;

        }

        private void HandleLaunch()
        {
            // Press mouse, Launch is primed
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                // Todo: Slow time
                if (!_canRedirect)
                    Velocity /= 2;

                // Player can launch
                _canRedirect = true;
            }

            // Let go of Mouse, Launch Player in direction of Mouse
            else if (Mouse.GetState().LeftButton == ButtonState.Released && _canRedirect)
            {
                // Get mouse Position
                Vector2 mousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

                // Aim from center of the Player
                Vector2 centerPos = new Vector2(Position.X + DEFAULT_SPRITE_WIDTH / 2, 
                    Position.Y + DEFAULT_SPRITE_HEIGHT / 2);

                // Aim toward mouse at player speed
                Vector2 distance = mousePos - centerPos;
                distance.Normalize();
                distance *= _speed * 1.5f;
                Velocity = distance;

                // Stop player from launching again
                _canRedirect = false;

                _numRedirects--;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Sprite.Draw(spriteBatch, Position);
        }
    }
}
