using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Entity
{
	public class BasicPuncher : Enemy
	{
		private Sprite _walkSprites;

		// Constructors
		public BasicPuncher(Game1 gm, Vector2 position) 
			: base(gm, position)
		{
			// Set Enemy Image
			Texture2D puncherSpritesheet = 
				gm.Content.Load<Texture2D>("Sprites/BasicEnemySpritesheet");

			Image = new Sprite(
				puncherSpritesheet,
				new Rectangle(
					0, 0,
					120, 140),
				new Rectangle(
					(int)position.X,
					(int)position.Y,
					(int)(Game1.TileSize * 1.5f),
					(int)(Game1.TileSize * 1.5f *
					140 / 120)));

			// Position
			WorldPosition = position;

			// Hitbox
			Hitbox = new Rectangle(
				(int)WorldPosition.X + Image.DestinationRect.Width / 2 - 50,
				(int)WorldPosition.Y + Image.DestinationRect.Height - 100,
				100,
				100);

			// Default Speed
			Speed = 5f;

			// Set Vitality
			MaxHealth = 3;
			CurHealth = MaxHealth;
			InvDuration = 0.5;
			InvTimer = InvDuration;

			// Attacking
			_attackForce = 15f;
			_attackDuration = 0.2;
			_attackDurationTimer = 0.0;
			_attackRadius = Game1.TileSize;
			_attackRange = Game1.TileSize;
			_attackWindupDuration = 0.5;
			_attackWindupTimer = _attackWindupDuration;

			_attackCooldown = 0.8;
			_attackCooldownTimer = 0.0;

			Texture2D gloveSpritesheet = 
				gm.Content.Load<Texture2D>("Sprites/PinPunch2");

			_gloveFrameWidth = gloveSpritesheet.Width / 5;
			_gloveImages = new Sprite(
				gloveSpritesheet,
				new Rectangle(
					0, 0,
					_gloveFrameWidth, gloveSpritesheet.Height),
				new Rectangle(
					0, 0, 
					(int)_attackRadius, (int)_attackRadius));

			// Set type
			Type = EntityType.Enemy;

			// Set state
			_chaseRange = Game1.TileSize * 5;
			_aggroRange = Game1.TileSize * 3;

			ActionState = EnemyState.Chase;

			// Animation
			_walkAnimSecondsPerFrame = 0.16;

			Texture2D walkTexture = gm.Content.Load<Texture2D>("Sprites/PuncherWalkSheet");
			_walkSprites = new Sprite(
				walkTexture,
				new Rectangle(0, 0, walkTexture.Width / 4, walkTexture.Height),
				new Rectangle(0, 0, Game1.TileSize * 3/2, Image.DestinationRect.Width * 3/2 * 140/120));
			_walkSprites.FrameBounds = _walkSprites.SourceRect;
			_walkSprites.Columns = 4;

			// Extra aesthetics
			Texture2D _koStarSpritesheet = 
				gm.Content.Load<Texture2D>("Sprites/KO_StarsSpritesheet");
            _knockoutStars = new Sprite(
				_koStarSpritesheet,
				new Rectangle(
					0, 0,
					_koStarSpritesheet.Bounds.Width / 4,
					_koStarSpritesheet.Bounds.Height),
				new Rectangle(
					0, 0,
					(int)(Game1.TileSize * 1.5), 
					(int)(Game1.TileSize * 1.5)));

			_koStarAnimDuration = 0.5;

			return;
		}

		// Methods

		public override void Update(GameTime gameTime)
		{
			TickInvincibility(gameTime);

			TickKnockout(gameTime);


			//CollisionChecker.CheckTilemapCollision(this, Game1.TEST_ROOM.Floor);

			Vector2 distanceFromPlayer = Game1.Player.CenterPosition - CenterPosition;
			float playerDist = distanceFromPlayer.Length();

			DetermineState(playerDist);

			switch (ActionState)
			{
				case EnemyState.KO:
                    UpdateKOAnimation(gameTime);
                    break;

				case EnemyState.Idle:
					Velocity = Vector2.Zero;
					break;

				case EnemyState.Chase:
					TargetPlayer();
					break;

				case EnemyState.Attack:
					Velocity = Vector2.Zero;

					// Apply attack cooldown
					if (_attackCooldownTimer > 0)
					{
						_attackCooldownTimer -=
							gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
					}
					else
					{
						// Choose a direction to attack in
						if (!_attackDirChosen)
						{
							// Get direction from self to player
							_attackDirection = Game1.Player.CenterPosition - CenterPosition;

							// Apply attack range
							_attackDirection.Normalize();
							_attackDirection *= _attackRange;

							_attackDirChosen = true;
						}

						// Charge attack
						_attackWindupTimer -=
							gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;

						// If not currently attacking or winding up
						// Use attack 
						if (_attackDurationTimer <= 0 && _attackWindupTimer <= 0)
							_attackDurationTimer = _attackDuration;

						// Currently Attacking
						if (_attackDurationTimer > 0)
						{
							Attack();
							_attackDurationTimer -=
								gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;

							if (_attackDurationTimer <= 0)
							{
								EndAttack(false);
							}
						}
					}
					break;
			}

			CollisionOn = !IsKO;

			CheckEnemyCollisions();

			CollisionChecker.CheckTilemapCollision(this, Game1.CurrentLevel.CurrentRoom.Tileset);

			Move(Velocity * Player.BulletTimeMultiplier);

			// Update animations
			
			UpdateEnemySprite();

			if (Velocity.LengthSquared() > 0)
				UpdateWalkAnimation(gameTime);

			return;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			// Draw Enemy relative to the player
			Vector2 screenPos = WorldPosition + Game1.MainCamera.WorldToScreenOffset;

			switch (ActionState)
			{
				case EnemyState.KO:

					Vector2 drawPos =
						screenPos +
						new Vector2(Image.DestinationRect.Width, 0f);

                    Image.Draw(
                        spriteBatch,
                        drawPos,
                        MathHelper.PiOver2,
                        Vector2.Zero);

					Vector2 starDrawPos =
						screenPos -
						new Vector2(0f, _knockoutStars.DestinationRect.Height / 2);

                    _knockoutStars.Draw(
						spriteBatch,
						starDrawPos,
						0f,
						Vector2.Zero);
                    break;

				case EnemyState.Idle:

					Image.Draw(
						spriteBatch, 
						screenPos, 
						0f, 
						Vector2.Zero);
					break;

				case EnemyState.Chase:
					if (Velocity.LengthSquared() > 0)
						DrawWalking(spriteBatch, screenPos);
					else
						Image.Draw(
							spriteBatch,
							screenPos,
							0f,
							Vector2.Zero);
					break;

				case EnemyState.Attack:

					Image.Draw(spriteBatch, screenPos, 0f, Vector2.Zero);

					DrawAttacking(spriteBatch, screenPos);
					break;
			}

			// Display current health
			//spriteBatch.DrawString(Game1.ARIAL32, $"Hp: {CurHealth}", screenPos, Color.White);

			// Display attack delay
			//spriteBatch.DrawString(
			//    Game1.ARIAL32,
			//    $"ATK_D: {_attackDelayTimer:0.00}",
			//    screenPos,
			//    Color.White);
			return;
		}

        #region Attack Methods

     

        protected override void TargetPlayer()
		{
			// Get direction from self to player
			Point eMinusP = Game1.Player.Hitbox.Center - Hitbox.Center;
			Vector2 directionToPlayer = new Vector2(eMinusP.X, eMinusP.Y);

			// Aim enemy toward player at their speed
			directionToPlayer.Normalize();
			directionToPlayer *= Speed;

			Vector2 positionAfterMoving = WorldPosition + directionToPlayer;

            if (CheckTilemapCollisionAhead( this , directionToPlayer, Game1.CurrentLevel.CurrentRoom.Tileset))
            {

                Vector2 newDirection = new Vector2(directionToPlayer.Y, -directionToPlayer.X);
                newDirection.Normalize();
                newDirection *= Speed;
                positionAfterMoving = WorldPosition + newDirection;
            }
           
            // Stop if get too close to another enemy
            bool shouldStop = false;
			float minDistanceFromEnemies = Game1.TileSize * 3;
			foreach (Enemy e in Game1.EManager.Enemies)
			{
				Vector2 distFromEnemyAfterMoving = (e.WorldPosition - positionAfterMoving);
				if (e != this &&
					distFromEnemyAfterMoving.LengthSquared() <= minDistanceFromEnemies * minDistanceFromEnemies)
				{
					shouldStop = true;
					break;
				}
			}

			if (shouldStop)
			{
				Velocity = Vector2.Zero;
				return;
			}

			Velocity = directionToPlayer;
			return;
		}

        public static bool CheckTilemapCollisionAhead( Entity e, Vector2 direction, Tileset tileset)
        {
            
            Vector2 nextPosition = e.WorldPosition + direction * e.Speed;
            
            Rectangle predictedHitbox = new Rectangle( (int)nextPosition.X, (int)nextPosition.Y,e.Hitbox.Width, e.Hitbox.Height);         
			
            for (int row = 0; row < tileset.Layout.GetLength(0); row++)
            {
                for (int col = 0; col < tileset.Layout.GetLength(1); col++)
                {
                    Tile tile = tileset.Layout[row, col];
                    if (tile.CollisionOn && predictedHitbox.Intersects(tile.Hitbox))
                    {                       
                        return true;
                    }
                }
            }
            return false;
        }
      
        protected override void Attack()
		{
			Vector2 directionToPlayer = _attackDirection;

			// Cast the damage box
			Rectangle damageBox = new Rectangle(
				(int)(CenterPosition.X + directionToPlayer.X - _attackRadius / 2),
				(int)(CenterPosition.Y + directionToPlayer.Y - _attackRadius / 2),
				(int)_attackRadius,
				(int)_attackRadius);

			// Store damage box
			_attackHitbox = damageBox;

			// Check if player is in damage box
			bool hitPlayerDir = damageBox.Intersects(Game1.Player.Hitbox);
			//CollisionChecker.CheckEntityCollision(damageBox, Game1.Player);

			// Once attack is landed, will not do damage
			// until next punch 
			if (hitPlayerDir && !_attackLandedOnce)
			{
				directionToPlayer.Normalize();
				directionToPlayer *= _attackForce;

				Game1.Player.TakeDamage(1);

				Game1.Player.CurCore.StopCurving();
				Game1.Player.CurCore.Ricochet(directionToPlayer);

				_attackLandedOnce = true;
			}

			return;
		}

		protected override void EndAttack(bool stoppedEarly)
		{
			// Reset windup
			_attackWindupTimer = _attackWindupDuration;

			if (stoppedEarly)
				_attackCooldown = 0;
			else
				_attackCooldownTimer = _attackCooldown;

			// Enemy is not attacking
			_attackDurationTimer = 0;

			// No new attack has happened
			_attackLandedOnce = false;
			_attackDirChosen = false;

			return;
		}

		#endregion

		#region Drawing Helper Methods

		private void UpdateWalkAnimation(GameTime gameTime)
		{
			// Add to the time counter (need TOTALSECONDS here)
			_walkAnimTimeCounter += gameTime.ElapsedGameTime.TotalSeconds 
				* Player.BulletTimeMultiplier;

			// Has enough time gone by to actually flip frames?
			if (_walkAnimTimeCounter >= _walkAnimSecondsPerFrame)
			{
				// Update the frame and wrap
				_walkAnimCurrentFrame++;
				if (_walkAnimCurrentFrame > _walkSprites.Columns) _walkAnimCurrentFrame = 1;

				_walkSprites.SetSourceToFrame(_walkAnimCurrentFrame);

				// Remove one "frame" worth of time
				_walkAnimTimeCounter -= _walkAnimSecondsPerFrame;
			}

			return;
		}
		private void UpdateEnemySprite()
		{
			// Reset Image Color
			Image.TintColor = Color.White;

            if (IsKO)
			{
				Image.SourceRect = new Rectangle(
					Image.SourceRect.Width * 4,
					Image.SourceRect.Y,
					Image.SourceRect.Width,
					Image.SourceRect.Height);

                Image.TintColor = Color.CornflowerBlue;
				return;
			}

            if (IsInvincible)
			{
				Image.TintColor *= 0.8f;

				Image.SourceRect = new Rectangle(
                    Image.SourceRect.Width * 4,
					Image.SourceRect.Y,
					Image.SourceRect.Width,
					Image.SourceRect.Height);
				return;
			}

			if (_attackDurationTimer <= 0)
			{
				int numChargeSprites = 3;
				int spriteNum = (int)
					((_attackWindupDuration - _attackWindupTimer) /
					(_attackWindupDuration / numChargeSprites));

				if (_attackWindupTimer < _attackWindupDuration)
					Image.TintColor = Color.Orange;

				Image.SourceRect = new Rectangle(
					Image.SourceRect.Width * spriteNum,
					Image.SourceRect.Y,
					Image.SourceRect.Width,
					Image.SourceRect.Height);
			}
			else
			{
				Image.SourceRect = new Rectangle(
					Image.SourceRect.Width * 3,
					Image.SourceRect.Y,
					Image.SourceRect.Width,
					Image.SourceRect.Height);
			}
		}
		private void UpdateKOAnimation(GameTime gameTime)
		{
			int numKoStars = 4;

			// Current frame is (passedTime / (seconds per frame))
			int curFrame =
				(int)(_koStarAnimTimeCounter /
				(_koStarAnimDuration / (numKoStars - 1)));

			// Get corresponding image
			_knockoutStars.SourceRect = new Rectangle(
				_knockoutStars.SourceRect.Width * curFrame,
				0,
				_knockoutStars.SourceRect.Width,
				_knockoutStars.SourceRect.Height);

			// Move animation forward
			_koStarAnimTimeCounter += 
				gameTime.ElapsedGameTime.TotalSeconds *
				Player.BulletTimeMultiplier;

			// Reset animation once it reaches or exceeds its duration
			if (_koStarAnimTimeCounter >= _koStarAnimDuration)
				_koStarAnimTimeCounter -= _koStarAnimDuration;
		}

		private void DrawWalking(SpriteBatch sb, Vector2 screenPos)
		{
			//switch (_walkAnimCurrentFrame)
			//{
			//	case 1:
			//		Vector2 origin = new Vector2(
			//					Image.SourceRect.X,
			//					Image.SourceRect.Center.Y - 15);


			//		Image.Draw(sb, screenPos, MathF.PI / 6, origin);
			//		break;

			//	case 2:
			//		Image.Draw(sb, screenPos, 0f, Vector2.Zero);
			//		break;

			//	case 3:

			//                 Image.Draw(
			//			sb, 
			//			screenPos, 
			//			-MathF.PI / 6,
			//                     new Vector2(
			//                         Image.SourceRect.Center.X,
			//                         Image.SourceRect.Y));
			//		break;

			//	case 4:
			//		Image.Draw(sb, screenPos, 0f, Vector2.Zero);
			//		break;
			//}

			Vector2 offset = Vector2.Zero;
			if (_walkAnimCurrentFrame % 2 == 1)
			{
				offset = new Vector2(0f, -30f);
			}
			_walkSprites.Draw(sb, screenPos + offset, 0f, Vector2.Zero);

			return;
		}

		private void DrawAttacking(SpriteBatch sb, Vector2 screenPos)
		{
            
            // Draw Attack Windup
            if (_attackWindupTimer < _attackWindupDuration && _attackDurationTimer <= 0d)
			{
				_gloveImages.TintColor = Color.White;

				// Get the center position of the pulled back fist
				// in screen space

				Vector2 correction = new Vector2(
					Image.DestinationRect.Width / 2.5f,
					Image.DestinationRect.Height / 2.5f);

				Vector2 windupScreenPos =
                    CenterPosition + correction - _attackDirection
                    + Game1.MainCamera.WorldToScreenOffset;

				// Shift position to top-left of glove image (for drawing)
				windupScreenPos -= new Vector2(_gloveFrameWidth / 2, _gloveFrameWidth / 2);

                // Select the correct glove image
                _gloveImages.SourceRect = 
					new Rectangle(
						0, 0, 
						_gloveFrameWidth, _gloveImages.SourceRect.Height);

                // Rotate fist so knuckles face away from player
                float dirAngle = MathF.Atan2(_attackDirection.Y, _attackDirection.X);

				// Glove changes colors right before attacking
				if (_attackWindupTimer < _attackWindupDuration * .3)
					_gloveImages.TintColor = Color.Red;

                // Draw glove
                _gloveImages.Draw(
					sb, 
					windupScreenPos, 
					dirAngle,
                    _gloveImages.SourceRect.Center.ToVector2());
			}

			// Draw actively attacking
			if (_attackDurationTimer > 0)
			{
				// Get position of the extended fist
				// in screen space

				Vector2 attackScreenPos = CenterPosition + _attackDirection * 5f
                    + Game1.MainCamera.WorldToScreenOffset;

				// Rotate fist so knuckles face player
				float dirAngle = MathF.Atan2(_attackDirection.Y, _attackDirection.X);

				// Get proper glove image
				_gloveImages.SourceRect = new Rectangle(
                    _gloveFrameWidth * 4,
                    0,
                    _gloveFrameWidth,
                    _gloveFrameWidth);

				// Draw glove
				_gloveImages.Draw(
					sb,
					attackScreenPos,
					dirAngle,
					_gloveImages.SourceRect.Center.ToVector2());
			}
			return;
		}

        #endregion

       
    }
}
