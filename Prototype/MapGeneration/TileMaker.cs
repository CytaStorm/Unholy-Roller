using Microsoft.Xna.Framework;
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
        Grass,
        Placeholder
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
            return SetTile(tileType, new Vector2(0f, 0f));
        }

        /// <summary>
        /// Creates a tile of the specified type
        /// </summary>
        /// <param name="tileType"> type of tile to create </param>
        /// <param name="position"> position of the tile </param>
        /// <returns> created tile </returns>
        public static Tile SetTile(int tileType, Vector2 position)
        {
            Tile result = new Tile();
            result.Type = (TileType)tileType;

            result.WorldPosition = position;

            switch (tileType)
            {               
                case (int)TileType.Grass:

                    result.CollisionOn = false;

                    result.TileSprite = new Sprite(_tileTextures[1], 0, 0, Game1.TILESIZE, Game1.TILESIZE);
                    break;

                default:
                    result.CollisionOn = false;

                    result.TileSprite = new Sprite(_tileTextures[0], 0, 0, Game1.TILESIZE, Game1.TILESIZE);
                    break;
            }
            
            return result;
        }
    }
}
