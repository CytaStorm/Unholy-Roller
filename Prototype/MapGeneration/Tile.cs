using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.MapGeneration
{
    internal class Tile : IGameEntity
    {
        // Properties
        public Sprite TileSprite { get; set; }

        public bool CollisionOn { get; set; }
        public Vector2 WorldPosition { get; set; } = Vector2.Zero;

        public TileType Type { get; set; }

        // Door Attributes
        public bool IsDoor { get; set; }

        public bool Bridged { get; set; }

        public string Dorientation { get; set; }


        // Methods
        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
        
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            TileSprite.Draw(spriteBatch, WorldPosition);
        }

    }

}
