using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.LevelGen
{
    internal class Tile
    {
        public Sprite TileSprite { get; set; }
        public bool Collidable { get; set; }
        public Vector2 WorldPosition { get; set; }
        public TileType Type { get; set; }
        public bool IsDoor { get; set; }
        public string DoorOrientation { get; set; }
        public bool IsEnemySpawner { get; set; }

        public void Draw(SpriteBatch sb, Vector2 screenPos)
        {
            TileSprite.Draw(sb, screenPos);
        }
    }
}
