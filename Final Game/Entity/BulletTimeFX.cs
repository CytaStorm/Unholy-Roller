using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Entity
{
	internal class BulletTimeFX : Entity
	{
		#region Constructors
		public BulletTimeFX(Sprite sprite, SpriteBatch sb, Color tint, Vector2 worldPos)
		{
			Image = sprite;
			Image.TintColor = tint;
			WorldPosition = worldPos;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Update every frame.
		/// </summary>
		/// <param name="gt">Gametime.</param>
		public override void Update(GameTime gt)
		{
		}

		/// <summary>
		/// Draws the effect.
		/// </summary>
		/// <param name="sb"></param>
		public override void Draw(SpriteBatch sb)
		{
			// Draw sprite fx
			Vector2 screenPos = WorldPosition + Game1.MainCamera.WorldToScreenOffset;
			Image.Draw(sb, screenPos, , Vector2.Zero);
		}
		#endregion
	}
}
