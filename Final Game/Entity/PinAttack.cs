using Final_Game.LevelGen;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;

namespace Final_Game.Entity
{
	public class PinAttack : Enemy
	{
		
		private Vector2 ScreenPos;
		private int Xdirection;
		private int Ydirection;
		private Texture2D texture;
		private float speed;
		private double timeLeft;
		private bool attacked;
	   
		
		public PinAttack(int Xdirection, int Ydirection, Vector2 position, Game1 gm)
			: base(gm, position)
		{
			texture = gm.Content.Load<Texture2D>("Sprites/BasicEnemy");
			this.Xdirection = Xdirection;
			this.Ydirection = Ydirection;
			speed = -7.5f;
			Vector2 distFromPlayer = position - Game1.Player.WorldPosition;
			ScreenPos = Game1.Player.ScreenPosition + distFromPlayer;
			timeLeft = 1.5;
			CurHealth = 100;
			InvTimer = 100;
			attacked = false;
			Image = new Sprite(
				texture,
				new Rectangle(
					0, 0,
					120, 140),
				new Rectangle(
					(int)position.X,
					(int)position.Y,
					(int)(Game1.TileSize * 1.5f),
					(int)(Game1.TileSize * 1.5f * 140/120)));
			_attackForce = 5f;
			_attackRange = Game1.TileSize;
			Type = EntityType.Enemy;
			Hitbox = new Rectangle(
			(int)WorldPosition.X + Image.DestinationRect.Width / 2 - 50 - (350 * Math.Abs(Ydirection)),
			(int)WorldPosition.Y + Image.DestinationRect.Height - 100 - (350 * Math.Abs(Xdirection)),
			100 + (700 * Math.Abs(Ydirection)),
			100 + (700 * Math.Abs(Xdirection)));
		}
		public override void Update(GameTime gameTime)
		{
			Vector2 velocity = new Vector2((speed * Xdirection * Player.BulletTimeMultiplier),(speed * Ydirection * Player.BulletTimeMultiplier));
			Move(velocity);
			Vector2 distFromPlayer = WorldPosition - Game1.Player.WorldPosition;
			ScreenPos = Game1.Player.ScreenPosition + distFromPlayer;
			timeLeft -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
			if (timeLeft < 0)
			{
				Hitbox = new Rectangle(0, 0, 0, 0);
				attacked = true;
				CurHealth= 0;
			}
			_attackDirection = Game1.Player.CenterPosition - CenterPosition;

			// Apply attack range
			_attackDirection.Normalize();
			_attackDirection *= _attackRange;

			Attack();
		}
		public override void Draw(SpriteBatch sb)
		{
			Vector2 distFromPlayer = WorldPosition - Game1.Player.WorldPosition;
			ScreenPos = Game1.Player.ScreenPosition + distFromPlayer;
			if (!attacked)
			{
				//Horizontal
				if (Ydirection != 0) {
					Image.Draw(sb, ScreenPos, (float)Math.PI / 2, Vector2.Zero );
					Image.Draw(sb, ScreenPos + new Vector2((Game1.TileSize * 1.5f * 140/120), 0), (float)Math.PI / 2, Vector2.Zero );
					Image.Draw(sb, ScreenPos + new Vector2((Game1.TileSize * 3f * 140/120), 0), (float)Math.PI / 2, Vector2.Zero );
                    Image.Draw(sb, ScreenPos + new Vector2((Game1.TileSize * 4.5f * 140/120), 0), (float)Math.PI / 2, Vector2.Zero );
					Image.Draw(sb, ScreenPos - new Vector2((Game1.TileSize * 1.5f * 140/120), 0), (float)Math.PI / 2, Vector2.Zero );
				}
				//Vertical
				else
				{
					Image.Draw(sb, ScreenPos, 0f, Vector2.Zero);
					Image.Draw(sb, ScreenPos + new Vector2(0, (Game1.TileSize * 1.5f * 140/120)), 0f, Vector2.Zero );
					Image.Draw(sb, ScreenPos + new Vector2(0, (Game1.TileSize * 3f * 140/120)), 0f, Vector2.Zero );
					Image.Draw(sb, ScreenPos - new Vector2(0, (Game1.TileSize * 1.5f * 140/120)), 0f, Vector2.Zero );
					Image.Draw(sb, ScreenPos - new Vector2(0, (Game1.TileSize * 3f * 140/120)), 0f, Vector2.Zero );
				}
			}
		}
		protected override void EndAttack(bool w) { }
		protected override void Attack() {
			Vector2 directionToPlayer = _attackDirection;

			// Cast the damage box
			

			// Store damage box
			

			// Check if player is in damage box
			bool hitPlayerDir = Hitbox.Intersects(Game1.Player.Hitbox);
			//CollisionChecker.CheckEntityCollision(damageBox, Game1.Player);

			if (hitPlayerDir)
			{
				directionToPlayer.Normalize();
				directionToPlayer *= _attackForce;

				Game1.Player.TakeDamage(1);
				Game1.Player.CurCore.Ricochet(directionToPlayer);
				// Keep player from ricocheting endlessly
				Hitbox = new Rectangle(0, 0, 0, 0);
				attacked = true;
			}

			return;
		}

	}
}
