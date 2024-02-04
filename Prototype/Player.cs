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

namespace Prototype
{
    public class Player : IGameEntity
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

        /// <summary>
        /// The player's image
        /// </summary>
        public Sprite Sprite { get; private set; }

        /// <summary>
        /// The player's position
        /// </summary>
        public Vector2 WorldPosition { get; private set; }

        public Vector2 ScreenPosition { get; private set; }

        /// <summary>
        /// The player's current velocity
        /// </summary>
        public Vector2 Velocity { get; private set; }

        /// <summary>
        /// The remaining number of times the player 
        /// can shoot in a different direction
        /// </summary>
        public int NumRedirects { get => _numRedirects; }

        public Rectangle Hitbox { get; private set; }

        // Constructors

        public Player(Texture2D spriteSheet, Vector2 worldPosition, GraphicsDeviceManager gdManager, RoomManager rm)
        {
            // Set Player Image
            Sprite = new Sprite(spriteSheet, DEFAULT_SPRITE_X, DEFAULT_SPRITE_Y, 
                DEFAULT_SPRITE_WIDTH, DEFAULT_SPRITE_HEIGHT, new Vector2(50, 50));

            // Hitbox
            Hitbox = new Rectangle((int)this.WorldPosition.X, (int)this.WorldPosition.Y, DEFAULT_SPRITE_WIDTH, DEFAULT_SPRITE_HEIGHT);

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
        public void Update(GameTime gameTime)
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
          
            // Adjust player's position based on velocity
            WorldPosition += Velocity;
            
        }

        private bool CheckTileCollision()
        {
            // Get where hitbox will be
            Vector2 colPos = WorldPosition + Velocity;
            Rectangle colHit = new Rectangle(
                (int)colPos.X,
                (int)colPos.Y,
                DEFAULT_SPRITE_WIDTH,
                DEFAULT_SPRITE_HEIGHT);

            foreach (Room r in rm.Rooms)
            {
                foreach (Tile t in r.Floor.Layout)
                {
                    // Get tile hitbox
                    Rectangle tileHit = new Rectangle(
                        (int)t.WorldPosition.X,
                        (int)t.WorldPosition.Y,
                        Game1.TILESIZE,
                        Game1.TILESIZE
                        );

                    // Check if player will intersect any tiles
                    if (tileHit.Contains(colHit))
                    {
                        return true;
                    }
                }
            }

            return false;
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

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

            Sprite.Draw(spriteBatch, ScreenPosition);
        }
    }
}
