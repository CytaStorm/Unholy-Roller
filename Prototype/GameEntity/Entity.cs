using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prototype.MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.GameEntity
{
    public enum EntityType
    {
        Player,
        Enemy
    }

    public abstract class Entity
    {
        /* ---- Fields ---- */

        // Health
        protected double _iDuration;
        protected double _iTimer;
        protected float _speed;

        /* ---- Properties ---- */

        // Transform
        public Vector2 WorldPosition { get; protected set; }
        public Vector2 CenterPosition { get => new Vector2(
            WorldPosition.X + Image.DestinationRect.Width / 2,
            WorldPosition.Y + Image.DestinationRect.Height / 2);
        }
        
        // Collision
        public Rectangle Hitbox { get; protected set; }
        
        // Physics
        public Vector2 Velocity { get; protected set; }

        // Health
        public int MaxHealth { get; protected set; } = 5; // Default so entity isn't auto-removed
        public int CurHealth { get; protected set; } = 5; // Default so entity isn't auto-removed
        public bool Alive { get; protected set; } = true;
        public bool IsInvincible { get => _iTimer > 0; }

        // Image Properties
        public Sprite Image { get; protected set; }

        // Identifiers
        public EntityType Type { get; protected set; }

        // Attacking
        public int Damage { get; protected set; }


        // Methods

        public virtual void Update(GameTime gameTime)
        {
            // Update invincibility time
            TickInvincibility(gameTime);
        }

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
        /// Moves the entity in worldspace by their velocity
        /// and moves their hitbox by velocity.
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

        public virtual void OnHitEntity(Entity entityThatWasHit, CollisionType colType, 
            bool causedCollision) { }

        public virtual void OnHitTile(Tile tile, CollisionType colType) { }

        public virtual void OnHitObject(MapOBJ obj, CollisionType colType) { }

        public virtual void TakeDamage(int damage)
        {
            if (_iTimer <= 0)
            {
                CurHealth -= damage;

                // Entity becomes temporarily invincible
                _iTimer = _iDuration;

                if (CurHealth <= 0)
                {
                    Die();
                }
            }
        }

        public virtual void Die() { }

        public virtual void Heal(int amount)
        {
            CurHealth += amount;

            if (CurHealth > MaxHealth)
                CurHealth = MaxHealth;
        }

        protected virtual void TickInvincibility(GameTime gameTime)
        {
            // Update invincibility time
            if (_iTimer > 0)
            {
                _iTimer -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Find screen position relative to player
            Vector2 distanceFromPlayer = WorldPosition - Game1.Player1.WorldPosition;
            Vector2 screenPos = distanceFromPlayer + Game1.Player1.ScreenPosition;

            Image.Draw(spriteBatch, screenPos);
        }
    }
}
