using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.GameEntity
{
    public abstract class Entity
    {
        // Properties
        public Rectangle Hitbox { get; protected set; }
        public Vector2 WorldPosition { get; protected set; }
        public Vector2 Velocity { get; protected set; }
        public int MaxHealth { get; protected set; }
        public int CurHealth { get; protected set; }
        public Sprite Image { get; protected set; }

        // Methods

        public virtual void Update(GameTime gameTime) { }

        public virtual void Move() { }

        public virtual void OnHitEntity() { }

        public virtual void OnHitTile() { }

        public virtual void TakeDamage(int damage)
        {
            CurHealth -= damage;
        }

        public virtual void Die() { }

        public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Find screen position relative to player
            Vector2 distanceFromPlayer = WorldPosition - Game1.Player1.WorldPosition;
            Vector2 screenPos = distanceFromPlayer + Game1.Player1.ScreenPosition;

            Image.Draw(spriteBatch, screenPos);
        }
    }
}
