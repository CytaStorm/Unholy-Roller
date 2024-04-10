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
        #region Properties
        public double EffectLife { get; set; }
        public Vector2 Position { get; set; }
        #endregion

        #region Constructors
        public BulletTimeFX(Sprite sprite, float effectLife, SpriteBatch sb)
        {
            Image = sprite;
            EffectLife = effectLife;
        }
        #endregion

        #region Methods
        public override void Update(GameTime gt)
        {
            if (EffectLife > 0)
            {
                EffectLife -= gt.ElapsedGameTime.TotalSeconds;
            }

        }

        public override void Draw(SpriteBatch sb)
        {
            // Draw player image
			Vector2 screenPos = WorldPosition + Game1.MainCamera.WorldToScreenOffset;
			Image.Draw(sb, screenPos, 0f, Vector2.Zero);
        }

        public void EndEffect()
        {
            EffectLife = 0;
        }
        #endregion
    }
}
