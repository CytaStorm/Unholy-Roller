using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prototype.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Prototype.MapGeneration
{
    internal class HealthOBJ : MapOBJ
    {
        // Fields
        private int _healingPower;

        public HealthOBJ(Game1 gm, Vector2 worldPos, Room parent)
        {
            WorldPosition = worldPos;

            Texture2D doorTexture = gm.Content.Load<Texture2D>("BowlingHeart");
            Image = new Sprite(
                doorTexture,
                doorTexture.Bounds,
                new Rectangle(0, 0, Game1.TILESIZE, Game1.TILESIZE));

            Hitbox = new Rectangle(
                (int)worldPos.X,
                (int)worldPos.Y,
                doorTexture.Width,
                doorTexture.Height);

            Parent = parent;

            Type = MapObJType.Health;

            _healingPower = 2;
        }

        public override void OnHitEntity(Entity e, CollisionType colType)
        {
            switch (e.Type)
            {
                case EntityType.Player:

                    // Heal player
                    e.Heal(_healingPower);

                    // Cease to exist
                    Alive = false;
                    break;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            Vector2 distFromPlayer = WorldPosition - Game1.Player1.WorldPosition;
            Vector2 screenPos = Game1.Player1.ScreenPosition + distFromPlayer;
            Image.Draw(sb, screenPos);
        }
    }
}
