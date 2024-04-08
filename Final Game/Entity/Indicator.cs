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
    public class Indicator
    {
        
        private Vector2 WorldPos;
        private Vector2 screenPos;
        private Vector2 playerScreenPos;
        private BossState attackType;
        private double timeVisible;
        private int direction;
     

        public Indicator(Vector2 position, BossState attackType, int direction) {
            WorldPos = position;
            this.attackType = attackType;
            timeVisible = 1;
            this.direction = direction;
            
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
        public void Draw()
        {
            Vector2 distFromPlayer = WorldPos - Game1.Player.WorldPosition;
            screenPos = Game1.Player.ScreenPosition + distFromPlayer;

            switch (attackType)
            {
               
                
                case BossState.PinBombs:
                    ShapeBatch.CircleOutline(screenPos + new Vector2(50,50), 50f, Color.Red);
                    ShapeBatch.Circle(screenPos + new Vector2(50, 50), (50.0f * (float)(1.1 - timeVisible)), Color.DarkRed);
                    break;
                case BossState.PinThrow:
                    ShapeBatch.Line(screenPos + new Vector2(350 + Game1.TileSize * 1.5f, 50), 750f, (90 * direction), 10,Color.Red);
                    ShapeBatch.Line(screenPos - new Vector2(275, -50), 750f, (90 * direction), 10,Color.Red);
                    break;



            }
           
        }
    }
}
