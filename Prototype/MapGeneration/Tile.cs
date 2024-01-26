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
    internal class Tile
    {
        // Properties
        public Texture2D Image { get; set; }

        public bool CollisionOn { get; set; }

        public bool IsDoor { get; set; }

        public Vector2 Position { get; set; } = Vector2.Zero;

        public TileType type { get; set; }

        // Constructors

    }

}
