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

        /// <summary>
        /// The tile's image
        /// </summary>
        public Sprite TileSprite { get; set; }

        /// <summary>
        /// Whether or not entites can collide with this tile
        /// </summary>
        public bool CollisionOn { get; set; }

        /// <summary>
        /// The position of this tile in worldspace
        /// </summary>
        public Vector2 WorldPosition { get; set; } = Vector2.Zero;

        /// <summary>
        /// The tile's aesthetic
        /// </summary>
        public TileType Type { get; set; }

        // Door Attributes

        /// <summary>
        /// Gets and sets whether or not this tile is a bridge-point
        /// </summary>
        public bool IsDoor { get; set; }

        /// <summary>
        /// Whether or not this tile is connected to another door tile
        /// </summary>
        public bool Bridged { get; set; }


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
