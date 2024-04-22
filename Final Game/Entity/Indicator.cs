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
        private Rectangle HitBox;
     
        public Indicator(Vector2 position, BossState attackType, int direction) {
            WorldPos = position;
            this.attackType = attackType;
            timeVisible = 1;
            this.direction = direction;
            HitBox = new Rectangle((int)position.X, (int)position.Y, 50,50);
            
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
            if (HitBox.Intersects(Game1.Player.Hitbox))
            {
                Game1.Player.TakeDamage(1);
            }
            return true;
            
        }
        public void Draw()
        {
            screenPos = Game1.MainCamera.GetPerspectivePosition(
                    WorldPos + Game1.MainCamera.WorldToScreenOffset);

            switch (attackType)
            {
               
                
                case BossState.PinBombs:
                    float baseRadius = 50f * Game1.MainCamera.Zoom;
                    ShapeBatch.CircleOutline(
                        screenPos + new Vector2(50,50), 
                        baseRadius, 
                        Color.Red);
                    ShapeBatch.Circle(
                        screenPos + new Vector2(50, 50), 
                        (baseRadius * (float)(1.1 - timeVisible)), 
                        Color.DarkRed);
                    break;
                case BossState.PinThrow:
                    if (direction % 2 == 1)
                    {
                        float baseLength = 750f * Game1.MainCamera.Zoom;
                        ShapeBatch.Line(
                            screenPos + new Vector2(475, 50) * Game1.MainCamera.Zoom,
                            (baseLength * (float)(1.1 - timeVisible)),
                            (float)(Math.PI / 2) * direction, 
                            10, 
                            Color.Red);
                        ShapeBatch.Line(
                            screenPos - new Vector2(325, -50) * Game1.MainCamera.Zoom,
                             baseLength * (float)(1.1 - timeVisible), 
                            (float)(Math.PI / 2) * direction, 
                            10, 
                            Color.Red);
                    }
                    else
                    {
                        float baseLength = 750f * Game1.MainCamera.Zoom;
                        ShapeBatch.Line(
                            screenPos + new Vector2(50 , 350 + Game1.TileSize * 1.5f)
                            * Game1.MainCamera.Zoom, 
                            baseLength *(float)(1.1 - timeVisible), 
                            (float)(Math.PI / 2) * direction, 
                            10, 
                            Color.Red);
                        ShapeBatch.Line(
                            screenPos - new Vector2(-50 , 275)
                            * Game1.MainCamera.Zoom, 
                            baseLength * (float)(1.1 - timeVisible), 
                            (float)(Math.PI / 2) * direction, 
                            10, 
                            Color.Red);
                    }
                    break;



            }
           
        }
    }
}
