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
        #region Fields
        private int _healingPower;
		#endregion

		#region Constructor(s)

		/// <summary>
		/// Creates a health pickup at the specified world position
		/// </summary>
		/// <param name="content"> image data source </param>
		/// <param name="worldPosition"> where to spawn the pickup </param>
		public Pickup_Health(ContentManager content, Vector2 worldPosition)
		{
			// Set Image
			Texture2D texture = content.Load<Texture2D>("Sprites/HealthPin");
			Image = new Sprite(
				texture,
				new Rectangle(25, 0, texture.Width - 50, texture.Height),
				new Rectangle(0, 0, Game1.TileSize, Game1.TileSize));

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
				texture.Width,
				texture.Height);

			// Set Effect
			_healingPower = 2;

			// Set Type
			Type = EntityType.Pickup;
		}
		#endregion

		#region Method(s)

		public override void Update(GameTime gameTime) { }

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
				entity.Heal(_healingPower);
			}
        }

        #endregion
    }
}
