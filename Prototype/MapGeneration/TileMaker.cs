using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.MapGeneration
{
    public enum TileType
    {
        Grass,
        Wall,
        Placeholder
    }

    public class TileMaker
    {
        // Fields 
        private static Texture2D[] _tileTextures;

        private const int sourceTilesize = 60;

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
            return SetTile(tileType, new Vector2(0f, 0f), "");
        }

        /// <summary>
        /// Creates a tile of the specified type
        /// </summary>
        /// <param name="tileType"> type of tile to create </param>
        /// <param name="position"> position of the tile </param>
        /// <returns> created tile </returns>
        public static Tile SetTile(int tileType, Vector2 position)
        {
            return SetTile(tileType, position, "");
        }
        
        /// <summary>
        /// Creates a tile of the specified type
        /// </summary>
        /// <param name="tileType"> type of tile to create </param>
        /// <param name="position"> position of the tile </param>
        /// /// <param name="orientation"> rotation of the tile </param>
        /// <returns> created tile </returns>
        public static Tile SetTile(int tileType, Vector2 position, string orientation)
        {
            Tile result = new Tile();
            result.Type = (TileType)tileType;

            result.WorldPosition = position;


            switch (tileType)
            {
                case (int)TileType.Grass:

                    result.CollisionOn = false;

                    result.TileSprite = new Sprite(_tileTextures[1], GetOrientationSource(""));
                    break;

                case (int)TileType.Wall:

                    result.CollisionOn = true;

                    result.TileSprite = new Sprite(_tileTextures[2], GetOrientationSource(orientation));
                    break;

                default:

                    result.CollisionOn = false;

                    result.TileSprite = new Sprite(_tileTextures[0], GetOrientationSource(""));
                    break;
            }
            
            return result;
        }

        
        private static Rectangle GetOrientationSource(string orientation)
        {
            if (string.IsNullOrEmpty(orientation))
            {
                return new Rectangle(0, 0, sourceTilesize, sourceTilesize);
            }

            switch (orientation)
            {
                case "UR":
                    return new Rectangle(sourceTilesize*2, 0, sourceTilesize, sourceTilesize);
                case "U":
                    return new Rectangle(sourceTilesize, 0, sourceTilesize, sourceTilesize);
                case "UL":
                    return new Rectangle(0, 0, sourceTilesize, sourceTilesize);
                case "L":
                    return new Rectangle(0, sourceTilesize, sourceTilesize, sourceTilesize);
                case "R":
                    return new Rectangle(sourceTilesize*2, sourceTilesize, sourceTilesize, sourceTilesize);
                case "BL":
                    return new Rectangle(0, sourceTilesize*2, sourceTilesize, sourceTilesize);
                case "B":
                    return new Rectangle(sourceTilesize, sourceTilesize*2, sourceTilesize, sourceTilesize);
                case "BR":
                    return new Rectangle(sourceTilesize*2, sourceTilesize*2, sourceTilesize, sourceTilesize);
            }

            return new Rectangle(0, 0, sourceTilesize, sourceTilesize);
        }
    }
}
