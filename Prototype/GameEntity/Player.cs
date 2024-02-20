using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Prototype.MapGeneration;
using ShapeUtils;
using System;


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

        private bool _canRedirect = true;

        private int _maxRedirects = 3;
        private int _numRedirects;

        private RoomManager rm;

        private Game1 gm;

        // Properties

        public Vector2 ScreenPosition { get; private set; }

        /// <summary>
        /// The remaining number of times the player 
        /// can shoot in a different direction
        /// </summary>
        public int NumRedirects { get => _numRedirects; }

        // Constructors

        public Player(Texture2D spriteSheet, Vector2 position, 
            GraphicsDeviceManager gdManager, RoomManager rm, Game1 gm)
        {
            // Set Player Image
            Image = new Sprite(spriteSheet,
               new Rectangle(
                   DEFAULT_SPRITE_X,
                   DEFAULT_SPRITE_Y,
                   DEFAULT_SPRITE_WIDTH,
                   DEFAULT_SPRITE_HEIGHT),
               new Rectangle(
                   (int)position.X,
                   (int)position.Y,
                   Game1.TILESIZE,
                   Game1.TILESIZE));

            // Position
            WorldPosition = position;
            
            // Hitbox
            Hitbox = new Rectangle(
                (int)WorldPosition.X, 
                (int)WorldPosition.Y, 
                Image.DestinationRect.Width, 
                Image.DestinationRect.Height);

            ScreenPosition = new Vector2(
                Game1.WINDOW_WIDTH / 2 - DEFAULT_SPRITE_WIDTH / 2,
                Game1.WINDOW_HEIGHT / 2 - DEFAULT_SPRITE_HEIGHT / 2);

            // Graphics Manager -> Screen Collision
            _gdManager = gdManager;

            // Default Speed
            _speed = 10f;

            // Default Velocity
            Velocity = new Vector2(0f, 0f);

            // Redirecting
            _numRedirects = _maxRedirects;

            // Vitality
            MaxHealth = 6;
            CurHealth = MaxHealth;
            _iFrames = 30;

            // Attacking
            Damage = 2;

            // Game Manager
            this.gm = gm;

            this.rm = rm;

            Type = EntityType.Player;
        }


        // Methods
        public override void Update(GameTime gameTime)
        {
            TickInvincibility();

            if (_iTimer > 0) Image.TintColor = Color.Purple;

            bool sideScreenCollision = WorldPosition.X < 0f ||
                WorldPosition.X + DEFAULT_SPRITE_WIDTH >= _gdManager.PreferredBackBufferWidth;

            bool topBottomScreenCollision = WorldPosition.Y < 0f ||
                WorldPosition.Y + DEFAULT_SPRITE_HEIGHT >= _gdManager.PreferredBackBufferHeight;

            // Player can Redirect if not Colliding with Screen Border
            if (_numRedirects > 0)
            {
                HandleLaunch();
            }

            // Todo: Fix Clipping issue
            // Could try tracking time where collision is on and
            // if it's on too long, let the player move in a direction until it turns off
            
            bool hitTile = CollisionChecker.CheckTilemapCollision(this, Game1.TEST_ROOM.Floor);            

            HandleEnemyCollisions();

            Move();
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

                _numRedirects--;
            }
        }

        private void HandleEnemyCollisions()
        {
            for (int i = 0; i < Game1.EManager.Dummies.Count; i++)
            {
                Enemy curEnemy = Game1.EManager.Dummies[i];

                if (CollisionChecker.CheckEntityCollision(this, curEnemy))
                {
                    //Image.TintColor = Color.LightGoldenrodYellow;
                }
            }
        }

        public override void OnHitEntity(Entity entityThatWasHit, CollisionType colType,
            bool causedCollision)
        {
            switch (entityThatWasHit.Type)
            {
                case EntityType.Enemy:
                    //Ricochet(colType);

                    entityThatWasHit.TakeDamage(Damage);
                    break;
            }
        }

        public override void OnHitTile(Tile tile, CollisionType colType)
        {
            switch (tile.Type)
            {
                case TileType.Spike:

                    TakeDamage(2);

                    Image.TintColor = Color.LightGoldenrodYellow;
                    break;
            }

            Ricochet(colType);

            // Restore redirects
            _numRedirects = _maxRedirects;

            base.OnHitTile(tile, colType);
        }

        public void Ricochet(CollisionType hitDirection)
        {
            if (hitDirection == CollisionType.Vertical)
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y * -1);
            }
            else if (hitDirection == CollisionType.Horizontal)
            {
                Velocity = new Vector2(Velocity.X * -1, Velocity.Y);
            }
        }

        public void Ricochet(Vector2 newDirection)
        {
            Velocity = newDirection;
        }

        public override void Die()
        {
            // Go into gameover state
            Game1.GAMESTATE = Gamestate.Death;

            base.Die();
        }

        public void Reset()
        {
            CurHealth = MaxHealth;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Image.Draw(spriteBatch, ScreenPosition);

            // Display remaining redirects
            Vector2 textPos =
                new Vector2(
                    ScreenPosition.X + DEFAULT_SPRITE_WIDTH / 3 ,
                    ScreenPosition.Y + DEFAULT_SPRITE_HEIGHT / 3);
            spriteBatch.DrawString(Game1.ARIAL32, $"{_numRedirects}", textPos, Color.White);

            // Reset player color to default
            Image.TintColor = Color.White;
        }

        public void DrawGizmos()
        {
            // Draw Hitbox
            int hitDistX = (int)WorldPosition.X - Hitbox.X;
            int hitDistY = (int)WorldPosition.Y - Hitbox.Y;

            Rectangle drawnHit = new Rectangle(
                (int)(ScreenPosition.X - hitDistX),
                (int)(ScreenPosition.Y - hitDistY),
                Hitbox.Width,
                Hitbox.Height);

            Color fadedRed = new Color(1f, 0f, 0f, 0.4f);

            ShapeBatch.Box(drawnHit, fadedRed);
        }
    }
}
