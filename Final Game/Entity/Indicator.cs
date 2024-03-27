using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Final_Game.Entity
{
    internal class Indicator
    {
        
        private Vector2 WorldPos;
        private Vector2 screenPos;
        private BossState attackType;
        private double timeVisible;
        private Texture2D redCircle;

        public Indicator(Vector2 position, BossState attackType, Texture2D circle) {
            WorldPos = position;
            this.attackType = attackType;
            timeVisible = 1;
            redCircle = circle;
            
        }
        public bool Update(GameTime gameTime)
        {
            Vector2 distFromPlayer = WorldPos - Game1.Player.WorldPosition;
            screenPos = Game1.Player.ScreenPosition + distFromPlayer;
            if (timeVisible > 0)
            {
                timeVisible -= gameTime.ElapsedGameTime.TotalSeconds * Player.BulletTimeMultiplier;
                return false;
            }
                return true;
            
        }
        public void Draw(SpriteBatch spriteBatch)
        {

            switch (attackType)
            {
               
                
                case BossState.PinBombs:
                    spriteBatch.Draw(redCircle, new Rectangle((int)screenPos.X,(int)screenPos.Y, 125, 125), Color.Red);

                    break;
                    
            }
           
        }
    }
}
