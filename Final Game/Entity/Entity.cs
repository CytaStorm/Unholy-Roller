using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Entity
{
    public enum EntityType
    {
        Player,
        Enemy,
        Pickup
    }

    public delegate void EntityDamaged(int amount);

    public delegate void EntityDying();

    public abstract class Entity : IMovable, IDamageable, ICollidable
    {

        #region Move Properties
        public Vector2 WorldPosition { get; set; }
        public virtual Vector2 Velocity { get; protected set; } = Vector2.Zero;
        public Vector2 Acceleration { get; protected set; } = Vector2.Zero;
        public float Speed { get; protected set; }
        
        #endregion

        #region Health Properties

        /// <summary>
        /// Gets whether entity's health is greater than zero
        /// </summary>
        public virtual bool Alive => CurHealth > 0;
        public int MaxHealth { get; protected set; }
        public int CurHealth { get; protected set; }
        public double InvDuration { get; protected set; }
        public double InvTimer { get; protected set; }
        public bool IsInvincible => InvTimer > 0;

        public bool InfiniteHealth { get; set; }
        #endregion

        #region Collision Properties
        public Rectangle Hitbox { get; protected set; }
        public virtual bool CollisionOn { get; set; } = true;
        #endregion

        #region Misc Properties
        /// <summary>
        /// Gets the center position of the entity's image
        /// in world space
        /// </summary>
        public Vector2 CenterPosition => new Vector2(
            WorldPosition.X + Image.DestinationRect.Width / 2f * Game1.MainCamera.Zoom,
            WorldPosition.Y + Image.DestinationRect.Height / 2f * Game1.MainCamera.Zoom);

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
            Rectangle overlap = Rectangle.Intersect(Hitbox, tile.Hitbox);

            bool rightCollision = overlap.X > Hitbox.X;
            bool topCollision = overlap.Y > Hitbox.Y;

            if (colDir == CollisionDirection.Horizontal)
            {
                if (rightCollision)
                {
                    
                    Move(new Vector2(-overlap.Width, 0f));
                }
                else
                {
                    Move(new Vector2(overlap.Width, 0f));
                }
            }
            else
            {
                if (topCollision)
                {
                    Move(new Vector2(0f, -overlap.Height));
                }
                else {
                    Move(new Vector2(0f, overlap.Height));
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
            if (InvTimer <= 0 && !InfiniteHealth)
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

        /// <summary>
        /// Restores entity's health by the specified amount, without
        /// exceeding their max health.
        /// </summary>
        /// <param name="amount"> amount to heal entity by </param>
        public virtual void Heal(int amount)
        {
            // Restore health
            CurHealth += amount;

            // Clamp to max health
            if (CurHealth > MaxHealth)
            {
                CurHealth = MaxHealth;
            }

            return;
        }


        public virtual void Die() { }

        #endregion

        #region Drawing Helper Methods

        /// <summary>
        /// Draws entity's debug information (e.g. Hitbox)
        /// </summary>
        public virtual void DrawGizmos()
        {
            // Draw Hitbox
            Vector2 hitboxScreenPos =
                Game1.MainCamera.GetPerspectivePosition(
                Hitbox.Location.ToVector2() +
                Game1.MainCamera.WorldToScreenOffset);

            Rectangle hitboxInScreenSpace = new Rectangle(
                (int)hitboxScreenPos.X,
                (int)hitboxScreenPos.Y,
                (int)(Hitbox.Width * Game1.MainCamera.Zoom),
                (int)(Hitbox.Height * Game1.MainCamera.Zoom));

            ShapeBatch.Box(hitboxInScreenSpace, Color.Red * 0.4f);
        }

        #endregion


        /// <summary>
        /// Moves entity to the center of the specified room
        /// </summary>
        /// <param name="r"> room to move to </param>
        public void MoveToRoomCenter(Room r)
        {
            Vector2 distFromRoomCenter = r.Center.ToVector2() - WorldPosition;

            Move(distFromRoomCenter);
        }

    }
}