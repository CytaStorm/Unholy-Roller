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

        private MouseState _prevMouse;
        private MouseState _curMouse;

        private Sprite _default;
        private Sprite _spedUpSprite;

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
            _default = new Sprite(gm.Content.Load<Texture2D>("BasicBlueClean"),
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

            _spedUpSprite = new Sprite(gm.Content.Load<Texture2D>("SpookyBlueTeeth"),
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

            Image = _default;

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
            _speed = 20f;

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
            _curMouse = Mouse.GetState();

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

            HandleObjCollisions();

            // Press mouse, Launch is primed
            if (_numRedirects > 0 && _curMouse.LeftButton == ButtonState.Pressed)
            {
                // Todo: Slow time

                Move(Velocity / 2);

                // Player can launch
                _canRedirect = true;
            }
            else
            {
                Move(Velocity);
            }

            // Friction: Deccelerate a bit over time
            // Todo: Tile-based Friction?
            Vector2 friction = Velocity * -1;
            friction.Normalize();
            friction *= 0.01f;
            Accelerate(friction);

            _prevMouse = _curMouse;
        }

        private void HandleLaunch()
        {
            // Let go of Mouse, Launch Player in direction of Mouse
            if (_curMouse.LeftButton == ButtonState.Released && _canRedirect)
            {
                // Get mouse Position
                Vector2 mousePos = new Vector2(_curMouse.X, _curMouse.Y);

                // Aim from center of the Player
                Vector2 centerPos = new Vector2(ScreenPosition.X + DEFAULT_SPRITE_WIDTH / 2,
                    ScreenPosition.Y + DEFAULT_SPRITE_HEIGHT / 2);

                // Aim toward mouse at player speed
                Vector2 distance = mousePos - centerPos;
                distance.Normalize();

                // Speed is less than default
                if (Velocity.LengthSquared() <= _speed * _speed)
                {
                    // Launch player at default speed
                    distance *= _speed;
                    Velocity = distance;
                }
                else
                {
                    // Launch player at current speed
                    distance *= Velocity.Length();
                    Velocity = distance;
                }

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

        private void HandleObjCollisions()
        {
            foreach(MapOBJ obj in Game1.TEST_ROOM.Interactables)
            {
                CollisionChecker.CheckMapObjectCollision(this, obj);
            }
        }

        public override void OnHitEntity(Entity entityThatWasHit, CollisionType colType,
            bool causedCollision)
        {
            switch (entityThatWasHit.Type)
            {
                case EntityType.Enemy:
                    entityThatWasHit.TakeDamage(Damage);

                    // Speed up
                    Vector2 acc = Velocity;
                    acc.Normalize();
                    acc *= 0.05f;
                    Accelerate(acc);
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

                case TileType.Wall:
                    // Place self on part of wall that was hit
                    //if (colType == CollisionType.Horizontal)
                    //{
                    //    if (Velocity.X > 0)
                    //    {
                    //        Vector2 whereItShouldBe = new Vector2(tile.WorldPosition.X - Hitbox.Width, WorldPosition.Y);
                    //        Move(whereItShouldBe - WorldPosition);
                    //    }
                    //    else
                    //    {
                    //        Vector2 whereItShouldBe = new Vector2(tile.WorldPosition.X + Game1.TILESIZE + 1, WorldPosition.Y);
                    //        Move(whereItShouldBe - WorldPosition);
                    //    }
                    //}
                    //else if (colType == CollisionType.Vertical)
                    //{
                    //    if (Velocity.Y > 0)
                    //    {
                    //        Vector2 whereItShouldBe = new Vector2(WorldPosition.X, tile.WorldPosition.Y - Hitbox.Height);
                    //        Move(whereItShouldBe - WorldPosition);
                    //    }
                    //    else
                    //    {
                    //        Vector2 whereItShouldBe = new Vector2(WorldPosition.X, tile.WorldPosition.Y + Game1.TILESIZE + 1);
                    //        Move(whereItShouldBe - WorldPosition);
                    //    }
                    //}
                    break;
            }


            Ricochet(colType);

            // Restore redirects
            if (_numRedirects < _maxRedirects)
                _numRedirects++;

            base.OnHitTile(tile, colType);
        }

        public override void OnHitObject(MapOBJ obj, CollisionType colType)
        {
            switch (obj.Type)
            {
                case MapObJType.Door:
                    Ricochet(colType);
                    break;
            }
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

        public void Accelerate(Vector2 force)
        {
            float forceMag = force.LengthSquared();
            if (forceMag < 0 || forceMag >= 0)
                Velocity += force;
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
            float veloMag = Velocity.Length();
            if (veloMag < 20f)
                Image = _default;
            else if (veloMag > 20f)
                Image = _spedUpSprite;

            Image.Draw(spriteBatch, ScreenPosition);
                

            // Display remaining redirects
            //Vector2 textPos =
            //    new Vector2(
            //        ScreenPosition.X + DEFAULT_SPRITE_WIDTH / 3 ,
            //        ScreenPosition.Y + DEFAULT_SPRITE_HEIGHT / 3);
            //spriteBatch.DrawString(Game1.ARIAL32, $"{_numRedirects}", textPos, Color.White);

            // Reset player color to default
            Image.TintColor = Color.White;

            // Display player velocity
            spriteBatch.DrawString(
                Game1.ARIAL32,
                $"Speed: {Velocity.Length():0.00}",
                new Vector2(0f, 150f),
                Color.White);
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
