using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prototype.MapGeneration;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.GameEntity
{
    public class Player : Entity
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

        private RoomManager rm;

        // Properties

        public Vector2 ScreenPosition { get; private set; }

        /// <summary>
        /// The remaining number of times the player 
        /// can shoot in a different direction
        /// </summary>
        public int NumRedirects { get => _numRedirects; }

        // Constructors

        public Player(Texture2D spriteSheet, Vector2 worldPosition, GraphicsDeviceManager gdManager, RoomManager rm)
        {
            // Set Player Image
            Image = new Sprite(spriteSheet, DEFAULT_SPRITE_X, DEFAULT_SPRITE_Y,
                DEFAULT_SPRITE_WIDTH, DEFAULT_SPRITE_HEIGHT, new Vector2(50, 50));

            // Hitbox
            Hitbox = new Rectangle((int)WorldPosition.X, (int)WorldPosition.Y, DEFAULT_SPRITE_WIDTH, DEFAULT_SPRITE_HEIGHT);

            // Position
            WorldPosition = worldPosition;

            ScreenPosition = new Vector2(
                Game1.WINDOW_WIDTH / 2 - DEFAULT_SPRITE_WIDTH / 2,
                Game1.WINDOW_HEIGHT / 2 - DEFAULT_SPRITE_HEIGHT / 2);

            // Graphics Manager -> Screen Collision
            _gdManager = gdManager;

            // Default Speed
            _speed = 5f;

            // Default Velocity
            Velocity = new Vector2(_speed, _speed);

            // Redirecting
            _numRedirects = _maxRedirects;

            this.rm = rm;
        }


        // Methods
        public override void Update(GameTime gameTime)
        {
            bool sideScreenCollision = WorldPosition.X < 0f ||
                WorldPosition.X + DEFAULT_SPRITE_WIDTH >= _gdManager.PreferredBackBufferWidth;

            bool topBottomScreenCollision = WorldPosition.Y < 0f ||
                WorldPosition.Y + DEFAULT_SPRITE_HEIGHT >= _gdManager.PreferredBackBufferHeight;

            // Player can Redirect if not Colliding with Screen Border
            if (/*!sideScreenCollision && !topBottomScreenCollision && */_numRedirects > 0)
            {
                HandleLaunch();
            }

            /*
            // Check collisions with screenBounds
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
            */

            // Todo: Fix Clipping issue
            // Could try tracking time where collision is on and
            // if it's on too long, let the player move in a direction until it turns off

            CollisionType hitTile = CollisionChecker.CheckTileCollision(this);

            if (hitTile == CollisionType.Vertical)
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y * -1);
            }
            else if (hitTile == CollisionType.Horizontal)
            {
                Velocity = new Vector2(Velocity.X * -1, Velocity.Y);
            }

            // Adjust player's position based on velocity
            WorldPosition += Velocity;

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
                Vector2 centerPos = new Vector2(ScreenPosition.X + DEFAULT_SPRITE_WIDTH / 2,
                    ScreenPosition.Y + DEFAULT_SPRITE_HEIGHT / 2);

                // Aim toward mouse at player speed
                Vector2 distance = mousePos - centerPos;
                distance.Normalize();
                distance *= _speed * 1.5f;
                Velocity = distance;

                // Stop player from launching again
                _canRedirect = false;

                //_numRedirects--;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

            Image.Draw(spriteBatch, ScreenPosition);
        }
    }
}
