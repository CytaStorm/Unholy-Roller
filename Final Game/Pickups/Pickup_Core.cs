using Final_Game.Entity;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Pickups
{
    public class Pickup_Core : Entity.Entity
    {
        #region Fields
        private ContentManager _content;

        private string _coreName;

        private double _collisionDelay;
        private double _collisionDelayTimer;
        #endregion

        #region Properties

        public override bool CollisionOn => _collisionDelayTimer <= 0;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Creates a core of the specified type at the specified world position
        /// </summary>
        /// <param name="content"> image data source </param>
        /// <param name="worldPosition"> where to spawn the pickup </param>
        public Pickup_Core(ContentManager content, Vector2 worldPosition, string coreName)
        {
            // Set Image
            Texture2D texture = content.Load<Texture2D>("Sprites/CurveCoreIcon");
            Image = new Sprite(
                texture,
                texture.Bounds,
                new Rectangle(0, 0, Game1.TileSize * 4/3, Game1.TileSize * 4/3));

            // Set Health
            MaxHealth = 1;
            CurHealth = MaxHealth;

            // Set Position
            WorldPosition = worldPosition;

            // Set Hitbox
            Point worldPoint = worldPosition.ToPoint();
            Hitbox = new Rectangle(
                worldPoint.X,
                worldPoint.Y,
                Image.DestinationRect.Width,
                Image.DestinationRect.Height);

            // Set Type
            Type = EntityType.Pickup;

            _content = content;

            // Name of core to give
            _coreName = coreName;

            // Wait a bit before it can be picked up
            _collisionDelay = 1;
            _collisionDelayTimer = _collisionDelay;
        }
        #endregion

        #region Method(s)

        public override void Update(GameTime gameTime) 
        {
            if (_collisionDelayTimer > 0)
            {
                Image.AlphaMultiplier = 
                    (float)((_collisionDelay - _collisionDelayTimer) / _collisionDelay);

                _collisionDelayTimer -= gameTime.ElapsedGameTime.TotalSeconds;

                if (_collisionDelayTimer <= 0) Image.AlphaMultiplier = 1f;
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            // Draw Pickup relative to the player
            Vector2 screenPos = WorldPosition + Game1.MainCamera.WorldToScreenOffset;

            Image.Draw(sb, screenPos, 0f, Vector2.Zero);
        }

        public override void OnHitEntity(Entity.Entity entity, CollisionDirection collision)
        {
            if (entity.Type == EntityType.Player)
            {
                if (!Game1.Player.AddCore(GetCoreByName(_coreName)))
                {
                    CurHealth = 1;
                }
            }
        }

        /// <summary>
        /// Returns a new instance of the specified type of core
        /// </summary>
        /// <param name="coreName"> type of core </param>
        /// <returns> new core of specified type </returns>
        private Core GetCoreByName(string coreName)
        {
            switch (coreName)
            {
                case "Straight":
                    return new Core(_content);

                case "Curve":
                    return new Core_ThreePointCurve(_content);

                default:
                    return null;
            }
        }



        #endregion
    }
}
