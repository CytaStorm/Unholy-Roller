using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Final_Game.Entity
{
	public class TrailParticle : Entity
	{
        #region Fields
        private double _lifeTime;
		private double _lifeTimeCounter;

		private float _startAlpha;
        #endregion

        #region Properties
        public override bool Alive => _lifeTimeCounter < _lifeTime;
        #endregion

        #region Constructors
        public TrailParticle(
			Sprite spriteSample, Color tint, Vector2 worldPos, double lifeTime)
		{
			// Copy specified spriteSample
			Image = new Sprite(
				spriteSample.Texture,
				spriteSample.SourceRect,
				spriteSample.DestinationRect);
			Image.TintColor = tint;

			// Set position
			WorldPosition = worldPos;

			// Set existence duration
			this._lifeTime = lifeTime;

			// Set initial alpha value
			_startAlpha = tint.A / 255f;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Update every frame.
		/// </summary>
		/// <param name="gameTime">Gametime.</param>
		public override void Update(GameTime gameTime)
		{
			_lifeTimeCounter += gameTime.ElapsedGameTime.TotalSeconds;

			FadeOverLifespan();
		}

		private void FadeOverLifespan()
		{
            float curAlpha =
                _startAlpha *
                (float)((_lifeTime - _lifeTimeCounter) / _lifeTime);

            Vector3 curTintWithoutTransparecy = Image.TintColor.ToVector3();

            Image.TintColor = new Color(
                curTintWithoutTransparecy.X,
                curTintWithoutTransparecy.Y,
                curTintWithoutTransparecy.Z,
                curAlpha);
        }

		/// <summary>
		/// Draws the effect.
		/// </summary>
		/// <param name="sb"></param>
		public override void Draw(SpriteBatch sb)
		{
			// Draw spriteSample fx
			Vector2 screenPos = WorldPosition + Game1.MainCamera.WorldToScreenOffset;
			Image.Draw(sb, screenPos, 0f, Vector2.Zero);
		}
		#endregion
	}
}
