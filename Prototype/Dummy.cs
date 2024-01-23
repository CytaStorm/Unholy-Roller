using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype
{
    internal class Dummy
    {
        // Fields
        public const int DEFAULT_SPRITE_X = 0;
        public const int DEFAULT_SPRITE_Y = 0;
        public const int DEFAULT_SPRITE_WIDTH = 120;
        public const int DEFAULT_SPRITE_HEIGHT = 120;

        private GraphicsDeviceManager _gdManager;

        private float _speed;

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

        public Dummy(Texture2D spriteSheet, Vector2 position, GraphicsDeviceManager gdManager)
        {
            // Set Player Image
            Sprite = new Sprite(spriteSheet, DEFAULT_SPRITE_X, DEFAULT_SPRITE_Y,
                DEFAULT_SPRITE_WIDTH, DEFAULT_SPRITE_HEIGHT, new Vector2(50, 50));

            // Position
            Position = position;

            // Graphics Manager -> Screen Collision
            _gdManager = gdManager;

            // Default Speed
            _speed = 0f;

            // Default Velocity
            Velocity = new Vector2(_speed, _speed);
        }


        // Methods
        public void Update(GameTime gameTime)
        {
            bool sideScreenCollision = Position.X < 0f ||
                Position.X + DEFAULT_SPRITE_WIDTH >= _gdManager.PreferredBackBufferWidth;

            bool topBottomScreenCollision = Position.Y < 0f ||
                Position.Y + DEFAULT_SPRITE_HEIGHT >= _gdManager.PreferredBackBufferHeight;

            // Check collisions with wall
            if (sideScreenCollision)
            {
                Velocity = new Vector2(Velocity.X * -1, Velocity.Y);
            }
            if (topBottomScreenCollision)
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y * -1);
            }

            // Move player based on velocity
            Position += Velocity;

        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Sprite.TintColor = Color.Red;
            Sprite.Draw(spriteBatch, Position);
        }
    }
}
