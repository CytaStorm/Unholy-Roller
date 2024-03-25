using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Final_Game.Entity
{
    internal class Indicator
    {
        private Vector2 position;
        private BossState attackType;
        private double timeVisible;

        public Indicator(Vector2 position, BossState attackType) { 
            this.position = position;
            this.attackType = attackType;
            timeVisible = 1;
        }
        public bool Update(GameTime gameTime)
        {
            if (timeVisible > 0)
            {
                timeVisible -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
                return false;
            }
                return true;
            
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            switch(attackType)
            {
                case BossState.PinBombs:
                    ShapeBatch.Circle(position, 5.0f, Color.Red);
                    ShapeBatch.CircleOutline(position, (5.0f * (float)(1.1 - timeVisible)), Color.DarkRed);
                    break;
                    
            }
        }
    }
}
