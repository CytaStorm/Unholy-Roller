using Final_Game.Entity;
using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Final_Game
{
    public class Core
    {
        #region Fields
        private float _walkSpeed = 10f;

        protected Sprite _launchArrow;
        #endregion

        #region Properties

        public Sprite Icon { get; protected set; }
        public Vector2 Velocity { get; set; }

        /// <summary>
        /// Magnitude of the friction slowing the player down each frame
        /// </summary>
        public float FrictionMagnitude { get; protected set; } = 0.01f;

        /// <summary>
        /// Gets whether or not player is moving along core's
        /// calculated trajectory
        /// </summary>
        public virtual bool IsCurving => true;

        /// <summary>
        /// Gets whether or not this core moves the player in an arc path
        /// </summary>
        public bool UsesCurve { get; protected set; }

        /// <summary>
        /// Name of this core
        /// </summary>
        public string Name { get; protected set; }


        #endregion

        #region Constructors

        public Core(ContentManager content)
        {
            Texture2D launchArrowTexture = 
                content.Load<Texture2D>("Sprites/LaunchArrowSpritesheet");

            _launchArrow = new Sprite(
                launchArrowTexture,
                new Rectangle(
                    0, 0,
                    launchArrowTexture.Width / 4,
                    launchArrowTexture.Height),
                new Rectangle(
                    0, 0,
                    launchArrowTexture.Width / 4,
                    launchArrowTexture.Height));
            _launchArrow.FrameBounds = _launchArrow.SourceRect;
            _launchArrow.Columns = 4;

            Texture2D iconTexture = content.Load<Texture2D>("Sprites/StraightCoreIcon");
            Icon = new Sprite(
                iconTexture,
                iconTexture.Bounds,
                new Rectangle(
                    0, 0,
                    Game1.TileSize * 3 / 2, Game1.TileSize * 3 / 2));
            Icon.ObeyCamera = false;

            Name = "Straight";
        }

        #endregion

        #region Methods
        public virtual void Update(GameTime gameTime) 
        {
            ApplyFriction();
        }

        public virtual void DrawTrajectoryHint(SpriteBatch sb)
        {
            if (!Game1.Player.Controllable || !Game1.Player.LaunchPrimed) return;

            // Get angle between arrow and mouse
            Vector2 mousePos = new Vector2(Game1.CurMouse.X, Game1.CurMouse.Y);

            Vector2 centerScreenPos = new Vector2(
                Game1.Player.ScreenPosition.X + Game1.Player.Image.DestinationRect.Width / 2,
                Game1.Player.ScreenPosition.Y + Game1.Player.Image.DestinationRect.Height / 2);

            Vector2 playerToMouseDistance = mousePos - centerScreenPos;

            float angleBetweenArrowAndMouse = MathF.Atan2(
                playerToMouseDistance.X,
                playerToMouseDistance.Y);

            // Scale distance from player to mouse for drawing
            Vector2 directionFromPlayerToMouse = playerToMouseDistance;
            directionFromPlayerToMouse.Normalize();
            directionFromPlayerToMouse *= 120; // Radius

            // Get correct launch arrow from sprite sheet
            _launchArrow.SetSourceToFrame(Game1.Player.NumRedirects);

            // Draw aiming arrow
            _launchArrow.Draw(
                sb,
                centerScreenPos + directionFromPlayerToMouse,
                -angleBetweenArrowAndMouse,
                new Vector2(
                    _launchArrow.SourceRect.Width / 2,
                    _launchArrow.SourceRect.Height / 2));
        }

        public virtual void DrawTrajectoryHint()
        {
            return;
        }

        #region Movement

        /// <summary>
        /// Calculates the next velocity the player should use to move
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void CalculateVelocity(GameTime gameTime)
        {
            // Get mouse Position
            Vector2 mousePos = new Vector2(Game1.CurMouse.X, Game1.CurMouse.Y);

            // Aim from center of the Player
            Vector2 centerPos = new Vector2(Game1.Player.ScreenPosition.X + 
                Game1.Player.Image.DestinationRect.Width / 2,
                Game1.Player.ScreenPosition.Y + Game1.Player.Image.DestinationRect.Height / 2);

            // Aim toward mouse at player speed
            Vector2 distance = mousePos - centerPos;
            distance.Normalize();

            // Speed is less than max
            if (Velocity.LengthSquared() < MathF.Pow(Game1.Player.Speed, 2))
            {
                // Launch player at max speed
                distance *= Game1.Player.Speed;
                Velocity = distance;
            }
            else
            {
                // Launch player at current speed
                distance *= Velocity.Length();
                Velocity = distance;
            }
        }

        /// <summary>
        /// Calculates the player's movement path
        /// </summary>
        public virtual void CalculateTrajectory() { }

        /// <summary>
        /// Makes necessary calculations to begin to move player
        /// along their trajectory
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Launch(GameTime gameTime) 
        {
            CalculateVelocity(gameTime);
        }

        /// <summary>
        /// Reverses the X or Y velocity of the player
        /// if they have a horizontal or vertical collision respectively
        /// </summary>
        /// <param name="hitDirection"> direction of collision </param>
        public void Ricochet(CollisionDirection hitDirection)
        {
            if (hitDirection == CollisionDirection.Vertical)
            {
                Velocity = new Vector2(Velocity.X, -Velocity.Y);
            }
            else if (hitDirection == CollisionDirection.Horizontal)
            {
                Velocity = new Vector2(-Velocity.X, Velocity.Y);
            }

            Game1.Player.State = PlayerState.Rolling;
        }

        /// <summary>
        /// Overrides the player's velocity with the specified vector
        /// </summary>
        /// <param name="newDirection"> new velocity </param>
        public void Ricochet(Vector2 newDirection)
        {
            Velocity = newDirection;

            Game1.Player.State = PlayerState.Rolling;
        }

        /// <summary>
        /// Adds a force to the player's current velocity
        /// </summary>
        /// <param name="force"> force to add </param>
        public void Accelerate(Vector2 force)
        {
            Velocity += force;
        }

        /// <summary>
        /// Slows the player down by the core's friction
        /// </summary>
        protected virtual void ApplyFriction()
        {
            if (Velocity.LengthSquared() <= MathF.Pow(Game1.Player.MinRollSpeed, 2)) 
                return;

            // Naturally decelerate over time
            Vector2 natDeceleration = -Velocity;
            natDeceleration.Normalize();
            Velocity += natDeceleration * FrictionMagnitude * Player.BulletTimeMultiplier;
        }

        /// <summary>
        /// Stops the player from moving along a predetermined trajectory
        /// (e.g. a curve)
        /// </summary>
        public virtual void StopCurving() { }

        /// <summary>
        /// Gets a walking velocity for the player in one of 
        /// 8 directions
        /// </summary>
        /// <param name="kb"></param>
        public void MoveWithKeyboard(KeyboardState kb)
        {
            Velocity = Vector2.Zero;

            // Move up
            if (kb.IsKeyDown(Keys.W) ||
                kb.IsKeyDown(Keys.Up))
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y - _walkSpeed);
            }
            // Move down
            if (kb.IsKeyDown(Keys.S) ||
                kb.IsKeyDown(Keys.Down))
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y + _walkSpeed);
            }
            // Move left
            if (kb.IsKeyDown(Keys.A) ||
                kb.IsKeyDown(Keys.Left))
            {
                Velocity = new Vector2(Velocity.X - _walkSpeed, Velocity.Y);
            }
            // Move right
            if (kb.IsKeyDown(Keys.D) ||
                kb.IsKeyDown(Keys.Right))
            {
                Velocity = new Vector2(Velocity.X + _walkSpeed, Velocity.Y);
            }

            // Max Velocity is _walkSpeed
            if (Velocity.LengthSquared() > _walkSpeed * _walkSpeed)
            {
                Velocity = Velocity * _walkSpeed / Velocity.Length();
            }
        }

        #endregion

        #region Collision Reaction
        public virtual void OnHitEntity(CollisionDirection colDir, Entity.Entity e) 
        {
            switch (e.Type)
            {
                case EntityType.Enemy:
                    if (Game1.Player.State == PlayerState.Rolling)
                    {
                        // Speed up
                        Vector2 acc = Velocity;
                        acc.Normalize();
                        acc *= 0.25f;
                        Accelerate(acc);

                        return;
                    }

                    // Player gets knocked back if standing on top of enemy
                    Vector2 distToEnemy = e.CenterPosition - Game1.Player.CenterPosition;
                    distToEnemy.Normalize();
                    distToEnemy *= -5;

                    Velocity = distToEnemy;
                    Game1.Player.State = PlayerState.Rolling;
                    break;
            }
        }

        public virtual void OnHitTile(CollisionDirection colDir, Tile tile) 
        {
            switch (tile.Type)
            {
                case TileType.OpenDoor:

                    // Stop Curving early
                    StopCurving();
                    return;
            }

            if (Game1.Player.State == PlayerState.Rolling)
            {
                if (IsCurving) StopCurving();

                Ricochet(colDir);
            }
        }
        #endregion

        public override string ToString()
        {
            return Name;
        }

        #endregion

    }
}
