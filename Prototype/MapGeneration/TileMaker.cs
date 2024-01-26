using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.MapGeneration
{
    enum TileType
    {
        Placeholder,
        Grass
    }

    internal class TileMaker
    {
        // Fields 
        private static Texture2D[] _tileTextures;

        // Constructors
        public TileMaker(Texture2D[] tileTextures)
        {
            _tileTextures = tileTextures;
        }

        // Methods

        /// <summary>
        /// Creates a tile of the specified type
        /// </summary>
        /// <param name="tileType"> type of tile to create </param>
        /// <returns> created tile </returns>
        public static Tile SetTile(int tileType)
        {
            Tile result = new Tile();
            result.type = (TileType)tileType;

            switch (tileType)
            {
                case (int)TileType.Placeholder:

                    result.CollisionOn = false;

                    result.Image = _tileTextures[0];
                    break;
                
                case (int)TileType.Grass:

                    result.CollisionOn = false;

                    result.Image = _tileTextures[1];
                    break;
            }
            
            return result;
        }
    }
}
