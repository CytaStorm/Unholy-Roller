using Final_Game.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Pickups
{
	public class Pickup_Health : Entity.Entity
	{
		// Fields
		private int _healingPower;

		public Pickup_Health(ContentManager content, Vector2 worldPosition)
		{
			Texture2D texture = content.Load<Texture2D>("Sprites/HealthPin");
			Image = new Sprite(
				texture,
				new Rectangle(25, 0, texture.Width - 50, texture.Height),
				new Rectangle(0, 0, Game1.TileSize, Game1.TileSize));

			MaxHealth = 1;
			CurHealth = MaxHealth;

			WorldPosition = worldPosition;

			Point worldPoint = worldPosition.ToPoint();
			Hitbox = new Rectangle(
				worldPoint.X,
				worldPoint.Y,
				texture.Width,
				texture.Height);

			_healingPower = 2;

			Type = EntityType.Pickup;
		}

		public override void Update(GameTime gameTime) { }

		public override void Draw(SpriteBatch sb)
		{
			// Draw Pickup relative to the player
			Vector2 distFromPlayer = WorldPosition - Game1.Player.WorldPosition;
			Vector2 screenPos = Game1.Player.ScreenPosition + distFromPlayer;

			// Only draw pickup if it's on screen
			Rectangle screenHit = new Rectangle(
				(int)screenPos.X,
				(int)screenPos.Y,
				Image.DestinationRect.Width,
				Image.DestinationRect.Height);

			bool onScreen = screenHit.Intersects(Game1.ScreenBounds);
			if (!onScreen) return;

			Image.Draw(sb, screenPos);
		}

        public override void OnHitEntity(Entity.Entity entity, CollisionDirection collision)
        {
            if (entity.Type == EntityType.Player)
			{
				entity.Heal(_healingPower);
			}
        }
    }
}
