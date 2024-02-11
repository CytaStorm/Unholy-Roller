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
        protected int _iFrames;
        protected int _iTimer;

        /* ---- Properties ---- */

        // Transform
        public Vector2 WorldPosition { get; protected set; }
        
        // Collision
        public Rectangle Hitbox { get; protected set; }
        
        // Physics
        public Vector2 Velocity { get; protected set; }

        // Health
        public int MaxHealth { get; protected set; } = 5; // Default so entity isn't auto-removed
        public int CurHealth { get; protected set; } = 5; // Default so entity isn't auto-removed
        public bool Alive { get; protected set; } = true;

        // Image Properties
        public Sprite Image { get; protected set; }

        // Identifiers
        public EntityType type { get; protected set; }

        // Attacking
        public int Damage { get; protected set; }


        // Methods

        public virtual void Update(GameTime gameTime)
        {
            // Update invincibility time
            TickInvincibility();
        }

        /// <summary>
        /// Moves the entity in worldspace by their velocity
        /// and moves their hitbox by velocity
        /// </summary>
        public virtual void Move()
        {
            // Update position
            WorldPosition += Velocity;

            // Update hitbox position
            Hitbox = new Rectangle(
                (int)WorldPosition.X,
                (int)WorldPosition.Y,
                Hitbox.Width,
                Hitbox.Height);
        }

        public virtual void OnHitEntity(Entity entityThatWasHit, CollisionType colType) { }

        public virtual void OnHitTile(Tile tile, CollisionType colType) { }

        public virtual void TakeDamage(int damage)
        {
            if (_iTimer <= 0)
            {
                CurHealth -= damage;

                // Entity becomes temporarily invincible
                _iTimer = _iFrames;

                if (CurHealth <= 0)
                {
                    Die();
                }
            }
        }

        public virtual void Die() { }

        protected virtual void TickInvincibility()
        {
            // Update invincibility time
            if (_iTimer > 0)
            {
                _iTimer--;
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
