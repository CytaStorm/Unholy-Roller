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
    public class Enemy : Entity
    {
        // Fields
        public const int DEFAULT_SPRITE_X = 0;
        public const int DEFAULT_SPRITE_Y = 0;
        public const int DEFAULT_SPRITE_WIDTH = 120;
        public const int DEFAULT_SPRITE_HEIGHT = 120;

        private GraphicsDeviceManager _gdManager;

        private float _speed;
        
        // Constructors

        public Enemy(Texture2D spriteSheet, Vector2 position, GraphicsDeviceManager gdManager)
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
            MaxHealth = 5;
            CurHealth = MaxHealth;
            _iFrames = 30;
            _iTimer = _iFrames;

            // Set type
            type = EntityType.Enemy;
        }


        // Methods
        public override void Update(GameTime gameTime)
        {
            TickInvincibility();

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

            Move();

        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Draw Enemy relative to the player
            Vector2 distFromPlayer = WorldPosition - Game1.Player1.WorldPosition;
            Vector2 screenPos = Game1.Player1.ScreenPosition + distFromPlayer;

            // Only draw enemy if they are on screen
            Rectangle screenBounds = new Rectangle(
                0, 0,
                _gdManager.PreferredBackBufferWidth,
                _gdManager.PreferredBackBufferHeight);

            Rectangle screenHit = new Rectangle(
                (int)screenPos.X,
                (int)screenPos.Y,
                Hitbox.Width,
                Hitbox.Height);

            bool onScreen = screenBounds.Right >= screenHit.X &&
               screenBounds.X <= screenHit.Right &&
               screenBounds.Bottom >= screenHit.Y &&
               screenBounds.Y <= screenHit.Bottom;

            if (onScreen)
            {
                Image.TintColor = Color.Red;
                Image.Draw(spriteBatch, screenPos);
            }

            // Display current health
            //spriteBatch.DrawString(Game1.ARIAL32, $"Hp: {CurHealth}", screenPos, Color.White);

        }
    }
}
