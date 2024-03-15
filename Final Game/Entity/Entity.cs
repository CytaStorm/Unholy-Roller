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
    public abstract class Entity : IMovable, IDamageable, ICollidable
    {
        #region Move Properties
        public Vector2 WorldPosition { get; protected set; }
        public Vector2 Velocity { get; protected set; } = Vector2.Zero;
        public Vector2 Acceleration { get; protected set; } = Vector2.Zero;
        public float Speed { get; protected set; }
        
        #endregion

        #region Damage Properties
        public int MaxHealth { get; protected set; }
        public int CurHealth { get; protected set; }
        public double InvDuration { get; protected set; }
        public double InvTimer { get; protected set; }
        #endregion

        #region Collision Properties
        public Rectangle Hitbox { get; protected set; }
        public bool CollisionOn { get; protected set; }
        #endregion



        public virtual void Update(GameTime gameTime)
        {

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

        public virtual void OnHitSomething(ICollidable other, CollisionDirection collision) { }

        public virtual void OnHitTile(Tile tile, CollisionDirection collision) { }

        /// <summary>
        /// Moves entity to the edge of the tile that they hit
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="colDir"></param>
        public void PlaceOnHitSurface(Tile tile, CollisionDirection colDir)
        {
            if (colDir == CollisionDirection.Horizontal)
            {
                if (Velocity.X > 0)
                {
                    Vector2 whereItShouldBe =
                        new Vector2(tile.WorldPosition.X - Hitbox.Width, WorldPosition.Y);
                    Move(whereItShouldBe - WorldPosition);
                }
                else
                {
                    Vector2 whereItShouldBe =
                        new Vector2(tile.WorldPosition.X + Game1.TileSize + 1, WorldPosition.Y);
                    Move(whereItShouldBe - WorldPosition);
                }
            }
            else if (colDir == CollisionDirection.Vertical)
            {
                if (Velocity.Y > 0)
                {
                    Vector2 whereItShouldBe =
                        new Vector2(WorldPosition.X, tile.WorldPosition.Y - Hitbox.Height);
                    Move(whereItShouldBe - WorldPosition);
                }
                else
                {
                    Vector2 whereItShouldBe =
                        new Vector2(WorldPosition.X, tile.WorldPosition.Y + Game1.TileSize + 1);
                    Move(whereItShouldBe - WorldPosition);
                }
            }
        }

        public virtual void TakeDamage(int amount)
        {

        }

        public virtual void Die()
        {

        }

        public virtual void Draw(SpriteBatch sb)
        {

        }

    }
}
