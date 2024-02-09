using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.GameEntity
{
    public class Dummy : IGameObject
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
        public Sprite Image { get; set; }

        /// <summary>
        /// The player's position
        /// </summary>
        public Vector2 WorldPosition { get; set; }

        /// <summary>
        /// The player's current velocity
        /// </summary>
        public Vector2 Velocity { get; private set; }

        public Rectangle Hitbox { get; private set; }

        public bool Alive { get; private set; }

        public Dummy(Texture2D spriteSheet, Vector2 position, GraphicsDeviceManager gdManager)
        {
            // Set Player Image
            Image = new Sprite(spriteSheet, DEFAULT_SPRITE_X, DEFAULT_SPRITE_Y,
                DEFAULT_SPRITE_WIDTH, DEFAULT_SPRITE_HEIGHT, new Vector2(50, 50));

            // Hitbox
            Hitbox = new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, DEFAULT_SPRITE_WIDTH, DEFAULT_SPRITE_HEIGHT);

            // Position
            WorldPosition = position;

            // Graphics Manager -> Screen Collision
            _gdManager = gdManager;

            // Default Speed
            _speed = 0f;

            // Default Velocity
            Velocity = new Vector2(_speed, _speed);

            // Set Vitality
            Alive = true;
        }


        // Methods
        public void Update(GameTime gameTime)
        {
            bool sideScreenCollision = WorldPosition.X < 0f ||
                WorldPosition.X + DEFAULT_SPRITE_WIDTH >= _gdManager.PreferredBackBufferWidth;

            bool topBottomScreenCollision = WorldPosition.Y < 0f ||
                WorldPosition.Y + DEFAULT_SPRITE_HEIGHT >= _gdManager.PreferredBackBufferHeight;

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
            WorldPosition += Velocity;

        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Image.TintColor = Color.Red;
            Image.Draw(spriteBatch, WorldPosition);
        }
    }
}
