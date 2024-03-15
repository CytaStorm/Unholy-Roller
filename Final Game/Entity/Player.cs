using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Entity
{
	public enum PlayerState
	{
		Rolling,
		Walking
	}
	public class Player : Entity
	{
		#region Fields
		private Texture2D _launchArrowsTexture;
		private int _launchArrowSpriteWidth;

		private int _numRedirects;
		private int _maxRedirects;

		private float _brakeSpeed;
		private float _frictionMagnitude;

		private float _transitionToWalkingSpeed;
		#endregion

		#region Properties
		public Sprite Image { get; private set; }
		public PlayerState State { get; private set; }
		public Vector2 ScreenPosition { get; private set; }
		#endregion

		// Constructors
		public Player(Game1 gm, Vector2 worldPosition)
		{
			// Set images
			Texture2D playerSprite = gm.Content.Load<Texture2D>("Sprites/BasicBlueClean");
			Image = new Sprite(playerSprite,
				new Rectangle(0, 0, 120, 120),
				new Rectangle(0, 0, Game1.TileSize, Game1.TileSize));
			_launchArrowsTexture = gm.Content.Load<Texture2D>("Sprites/LaunchArrowSpritesheet");

			//Set screenPosition
			ScreenPosition = new Vector2(
				Game1.ScreenCenter.X - Image.DestinationRect.Width / 2,
				Game1.ScreenCenter.Y - Image.DestinationRect.Height / 2);

			int numLaunchArrows = 2;
			_launchArrowSpriteWidth = _launchArrowsTexture.Width / numLaunchArrows;

			// Set position (world space)
			WorldPosition = worldPosition;

			// Set movement vars
			Speed = 20f;
			_brakeSpeed = 0.2f;
			_frictionMagnitude = 0.01f;
			_transitionToWalkingSpeed = 1f;

			// Set default state
			State = PlayerState.Walking;

			// Give launches
			_maxRedirects = 3;
			_numRedirects = _maxRedirects + 1;
		}

		// Methods
		public override void Update(GameTime gameTime)
		{
			switch (State)
			{
				case PlayerState.Walking:
					MoveWithKeyboard(Game1.CurKB);   
					break;

				case PlayerState.Rolling:
					ApplyFriction();

					HandleBraking();

					// Transition to walking
					if (Velocity.Length() < 1f)
					{
						State = PlayerState.Walking;

						_numRedirects = _maxRedirects + 1;
					}
					break;
			}

			HandleLaunch();

			//ApplyScreenBoundRicochet();

		   Move(Velocity);
		}
		public override void Draw(SpriteBatch sb)
		{
			// Draw player image

			Image.Draw(sb, ScreenPosition);

            if (_numRedirects > 0 && 
                Game1.IsMouseButtonPressed(1))
            {
                DrawLaunchArrow(sb);
            }
        }

        #region Movement Helper Methods

		public void MoveWithKeyboard(KeyboardState kb)
		{
			Velocity = Vector2.Zero;

			// Move up
			if (kb.IsKeyDown(Keys.W))
			{
				Velocity = new Vector2(Velocity.X, Velocity.Y - Speed);
			}
			// Move down
			if (kb.IsKeyDown(Keys.S))
			{
				Velocity = new Vector2(Velocity.X, Velocity.Y + Speed);
			}
			// Move left
			if (kb.IsKeyDown(Keys.A))
			{
				Velocity = new Vector2(Velocity.X - Speed, Velocity.Y);
			}
			// Move right
			if (kb.IsKeyDown(Keys.D))
			{
				Velocity = new Vector2(Velocity.X + Speed, Velocity.Y);
			}

			// Maximize Velocity at speed
			if (Velocity.LengthSquared() > Speed * Speed)
			{
				Velocity *= Speed / Velocity.Length();
			}
		}

		private void HandleLaunch()
		{
			if (Game1.IsMouseButtonPressed(1))
			{
				// Todo: Slow time
			}

			// Launch Player in direction of Mouse
			if (_numRedirects > 0 && Game1.IsMouseLeftClicked())
			{
				// Get mouse Position
				Vector2 mousePos = new Vector2(Game1.CurMouse.X, Game1.CurMouse.Y);

				// Aim from center of the Player
				Vector2 centerPos = new Vector2(ScreenPosition.X + Image.DestinationRect.Width / 2,
					ScreenPosition.Y + Image.DestinationRect.Height / 2);

				// Aim toward mouse at player speed
				Vector2 distance = mousePos - centerPos;
				distance.Normalize();

				// Speed is less than max
				if (Velocity.LengthSquared() <= Speed * Speed)
				{
					// Launch player at max speed
					distance *= Speed;
					Velocity = distance;
				}
				else
				{
					// Launch player at current speed
					distance *= Velocity.Length();
					Velocity = distance;
				}

				// Launch Player at max speed
				// Redirect Player at cur speed
				//if (_numRedirects > _maxRedirects)
				//{
				//    // Launch player at default speed
				//    distance *= _speed;
				//    Velocity = distance;
				//}
				//else
				//{
				//    // Launch player at current speed
				//    distance *= Velocity.Length();
				//    Velocity = distance;
				//}

				//_numRedirects--;

				// Player is now rolling
				State = PlayerState.Rolling;
			}
		}

		private void HandleBraking()
		{
			float lowestBrakableSpeed = 0.1f * 0.1f;

			if (Game1.IsMouseButtonPressed(2) && 
				Velocity.LengthSquared() >= lowestBrakableSpeed)
			{
				Vector2 deceleration = -Velocity;
				deceleration.Normalize();
				deceleration *= _brakeSpeed;

				Velocity += deceleration;
			}
		}

		public void Accelerate(Vector2 force)
		{
			Velocity += force;
		}

		private void ApplyFriction()
		{
			if (Velocity.LengthSquared() > MathF.Pow(_transitionToWalkingSpeed, 2))
			{
				// Naturally decelerate over time
				Vector2 natDeceleration = -Velocity;
				natDeceleration.Normalize();
				Velocity += natDeceleration * _frictionMagnitude;
			}
		}
        private void ApplyScreenBoundRicochet()
        {
            if (WorldPosition.X + Image.DestinationRect.Width > Game1.WindowWidth ||
                WorldPosition.X <= 0)
            {
                Velocity = new Vector2(-Velocity.X, Velocity.Y);
            }
            if (WorldPosition.Y + Image.DestinationRect.Height > Game1.WindowHeight ||
                WorldPosition.Y <= 0)
            {
                Velocity = new Vector2(Velocity.X, -Velocity.Y);
            }
        }
        #endregion

        #region Drawing Helper Methods
        private void DrawLaunchArrow(SpriteBatch sb)
        {
            
            // Get angle between arrow and mouse
            Vector2 mousePos = new Vector2(Game1.CurMouse.X, Game1.CurMouse.Y);

			Vector2 centerScreenPos = new Vector2(
				ScreenPosition.X + Image.DestinationRect.Width / 2,
				ScreenPosition.Y + Image.DestinationRect.Height / 2);

			Vector2 playerToMouseDistance = mousePos - centerScreenPos;

			float angleBetweenArrowAndMouse = MathF.Atan2(
				playerToMouseDistance.X,
				playerToMouseDistance.Y);

			// Scale distance from player to mouse for drawing
			Vector2 directionFromPlayerToMouse = playerToMouseDistance;
			directionFromPlayerToMouse.Normalize();
			directionFromPlayerToMouse *= 120; // Radius


			Rectangle arrowSourceRect = new Rectangle();
			if (_numRedirects > _maxRedirects)
			{
				// Launch Arrow
				arrowSourceRect = new Rectangle(
					_launchArrowSpriteWidth, 0,
					_launchArrowSpriteWidth, _launchArrowSpriteWidth);
			}
			else
			{
				// Redirect Arrow
				arrowSourceRect = new Rectangle(
					0, 0,
					_launchArrowSpriteWidth, _launchArrowSpriteWidth);
			}

			// Draw aiming arrow
			sb.Draw(
				_launchArrowsTexture,
				centerScreenPos + directionFromPlayerToMouse,
				arrowSourceRect,
				Color.White,
				-angleBetweenArrowAndMouse,
				new Vector2(
					_launchArrowSpriteWidth / 2,
					_launchArrowSpriteWidth / 2),
				1f,
				SpriteEffects.None,
				0f
				);

			// Todo: Display remaining redirects
			//Vector2 redirectStringDimensions =
			//    Game1.ARIAL32.MeasureString(_numRedirects.ToString());

			//Vector2 textPos = centerPlayerPos + directionFromPlayerToMouse;
			//textPos = new Vector2(
			//    textPos.X - redirectStringDimensions.X / 2,
			//    textPos.Y - redirectStringDimensions.Y / 2);

            //sb.DrawString(
            //    Game1.ARIAL32,
            //    _numRedirects.ToString(),
            //    textPos,
            //    Color.White);
        }
        #endregion

        public void Reset()
        {
            CurHealth = MaxHealth;

            _numRedirects = _maxRedirects + 1;

            Velocity = Vector2.Zero;

            State = PlayerState.Walking;

            WorldPosition = new Vector2(
                Game1.ScreenCenter.X - Image.DestinationRect.Width / 2,
                Game1.ScreenCenter.Y - Image.DestinationRect.Height / 2);
        }
    }
}
