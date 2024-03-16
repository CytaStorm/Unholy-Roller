using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Final_Game.Entity
{
    public enum EntityType
    {
        Player,
        Enemy
    }

    public abstract class Entity : IMovable, IDamageable, ICollidable
    {
        #region Move Properties
        public Vector2 WorldPosition { get; protected set; }
        public Vector2 Velocity { get; protected set; } = Vector2.Zero;
        public Vector2 Acceleration { get; protected set; } = Vector2.Zero;
        public float Speed { get; protected set; }
        
        #endregion

        #region Health Properties

        /// <summary>
        /// Gets whether entity's health is greater than zero
        /// </summary>
        public bool Alive => CurHealth > 0;
        public int MaxHealth { get; protected set; }
        public int CurHealth { get; protected set; }
        public double InvDuration { get; protected set; }
        public double InvTimer { get; protected set; }
        public bool IsInvincible => InvTimer > 0;
        #endregion

        #region Collision Properties
        public Rectangle Hitbox { get; protected set; }
        public bool CollisionOn { get; protected set; }
        #endregion

        #region Misc Properties
        /// <summary>
        /// Gets the center position of the entity's image
        /// in world space
        /// </summary>
        public Vector2 CenterPosition => new Vector2(
            WorldPosition.X + Image.DestinationRect.Width / 2f,
            WorldPosition.Y + Image.DestinationRect.Height / 2f);

        public Sprite Image { get; protected set; }

        public EntityType Type { get; protected set; }

        public int Damage { get; protected set; }

        #endregion

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(SpriteBatch sb);

        #region Movement Methods

        /// <summary>
        /// Moves the entity in worldspace by their velocity
        /// and moves their hitbox by velocity.
        /// ALWAYS CALL THIS TO GET PROPER COLLISIONS
        /// </summary>
        public virtual void Move()
        {
            // Preserve hitbox distance from entity origin
            int hitXDist = (int)WorldPosition.X - Hitbox.X;
            int hitYDist = (int)WorldPosition.Y - Hitbox.Y;

            // Update position
            WorldPosition += Velocity;

            // Update hitbox position
            Hitbox = new Rectangle(
                (int)(WorldPosition.X - hitXDist),
                (int)(WorldPosition.Y - hitYDist),
                Hitbox.Width,
                Hitbox.Height);

        }

        /// <summary>
        /// Moves the entity in worldspace by the specified velocity
        /// and moves their hitbox by that velocity.
        /// ALWAYS CALL THIS TO GET PROPER COLLISIONS
        /// </summary>
        public virtual void Move(Vector2 velocity)
        {
            // Preserve hitbox distance from entity origin
            int hitXDist = (int)WorldPosition.X - Hitbox.X;
            int hitYDist = (int)WorldPosition.Y - Hitbox.Y;

            // Update position
            WorldPosition += velocity;

            // Update hitbox position
            Hitbox = new Rectangle(
                (int)(WorldPosition.X - hitXDist),
                (int)(WorldPosition.Y - hitYDist),
                Hitbox.Width,
                Hitbox.Height);

        }
        #endregion

        #region Collision Response Methods
        public virtual void OnHitSomething(ICollidable other, CollisionDirection collision) { }
        
        /// <summary>
        /// Handles reactions to a tile collision
        /// </summary>
        /// <param name="tile"> the tile that was hit </param>
        /// <param name="collision"> the direction of the collision </param>
        public virtual void OnHitTile(Tile tile, CollisionDirection collision) { }
        
        /// <summary>
        /// Handles reactions to an entity collision
        /// </summary>
        /// <param name="entity"> the entity that was hit </param>
        /// <param name="collision"> the direction of the collision </param>
        public virtual void OnHitEntity(Entity entity, CollisionDirection collision) { }

        /// <summary>
        /// Moves entity to the specific edge of the tile that they hit
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="colDir"></param>
        public void PlaceOnHitEdge(Tile tile, CollisionDirection colDir)
        {
            if (colDir == CollisionDirection.Horizontal)
            {
                // Moving Right = Left edge collision
                if (Velocity.X > 0)
                {
                    Vector2 whereItShouldBe =
                        new Vector2(tile.WorldPosition.X - Hitbox.Width, WorldPosition.Y);
                    Move(whereItShouldBe - WorldPosition);
                }

                // Moving Left = Right edge collision
                else
                {
                    Vector2 whereItShouldBe =
                        new Vector2(tile.WorldPosition.X + Game1.TileSize + 1, WorldPosition.Y);
                    Move(whereItShouldBe - WorldPosition);
                }
            }
            else if (colDir == CollisionDirection.Vertical)
            {
                // Moving Down = Top edge collision
                if (Velocity.Y > 0)
                {
                    Vector2 whereItShouldBe =
                        new Vector2(WorldPosition.X, tile.WorldPosition.Y - Hitbox.Height);
                    Move(whereItShouldBe - WorldPosition);
                }

                // Moving Up = Bottom edge collision
                else
                {
                    Vector2 whereItShouldBe =
                        new Vector2(WorldPosition.X, tile.WorldPosition.Y + Game1.TileSize + 1);
                    Move(whereItShouldBe - WorldPosition);
                }
            }
        }

        #endregion

        #region Health & Damage Methods

        /// <summary>
        /// Decreases invincibility time by seconds passed since last frame
        /// </summary>
        /// <param name="gameTime"></param>
        protected virtual void TickInvincibility(GameTime gameTime)
        {
            // Update invincibility time
            if (InvTimer > 0)
            {
                InvTimer -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
            }
        }

        /// <summary>
        /// Subtracts an amount of damage from the entity's health
        /// </summary>
        /// <param name="amount"> damage to deal </param>
        public virtual void TakeDamage(int amount)
        {
            // Take damage if not invincible
            if (InvTimer <= 0)
            {
                CurHealth -= amount;

                // Temporarily become invincible
                InvTimer = InvDuration;

                // Handle low health
                if (CurHealth <= 0)
                {
                    Die();
                }
            }
        }


        public virtual void Die() { }

        #endregion

        #region Drawing Helper Methods

        /// <summary>
        /// Draws entity's debug information (e.g. Hitbox)
        /// </summary>
        public virtual void DrawGizmos() { }

        #endregion

    }
}