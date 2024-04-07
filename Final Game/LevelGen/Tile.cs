using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.LevelGen
{
	public class Tile : ICollidable
	{
		public Sprite TileSprite { get; set; }
		public bool CollisionOn { get; set; }
		public Vector2 WorldPosition { get; set; }
		public TileType Type { get; set; }
		public bool IsDoor { get; set; }
		public string DoorOrientation { get; set; }
		public bool IsEnemySpawner { get; set; }
		public bool HasHealthPickup { get; set; } = false;
		public Rectangle Hitbox =>
			new Rectangle(
				(int)(WorldPosition.X),
				(int)(WorldPosition.Y),
				Game1.TileSize,
				Game1.TileSize);

		public void Draw(SpriteBatch sb, Vector2 screenPos)
		{
			//Debug.WriteLine(TileSprite.Texture);
			TileSprite.Draw(sb, screenPos, 0, Vector2.Zero);
		}

		public void OnHitSomething(ICollidable other, CollisionDirection collision)
		{
			throw new NotImplementedException();
		}
	}
}
