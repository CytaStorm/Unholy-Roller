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
        Wall_U,
        Wall_UL,
        Wall_UR,
        Wall_R,
        Wall_BR,
        Wall_B,
        Wall_BL,
        Wall_L,
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

                    result.TileSprite = new Sprite(_tileTextures[1], 0, 0, Game1.TILESIZE, Game1.TILESIZE);
                    break;

                case (int)TileType.Wall_U:
                    result.CollisionOn = false;

                    result.TileSprite = new Sprite(_tileTextures[2], Game1.TILESIZE, 0, Game1.TILESIZE, Game1.TILESIZE);
                    break;

                default:
                    result.CollisionOn = false;

                    result.TileSprite = new Sprite(_tileTextures[0], 0, 0, Game1.TILESIZE, Game1.TILESIZE);
                    break;
            }
            
            return result;
        }

        /*
        private static Rectangle GetOrientationSource(string orientation)
        {
            if (!string.IsNullOrEmpty(orientation))
            {
                return new Rectangle(0, 0, Game1.TILESIZE, Game1.TILESIZE);
            }

            switch (orientation)
            {
                case "topleft":
                    return new Rectangle(0, 0, Game1.TILESIZE, Game1.TILESIZE);
                case "top":
                    return new Rectangle(Game1.TILESIZE, 0, Game1.TILESIZE, Game1.TILESIZE);
                case "topright":
                    return new Rectangle(Game1.TILESIZE*2, 0, Game1.TILESIZE, Game1.TILESIZE);
                case "left":
                    return new Rectangle(0, Game1.TILESIZE, Game1.TILESIZE, Game1.TILESIZE);
                case "right":
                    return new Rectangle(Game1.TILESIZE*2, Game1.TILESIZE, Game1.TILESIZE, Game1.TILESIZE);
                case "botLeft":
                    return new Rectangle(0, Game1.TILESIZE*2, Game1.TILESIZE, Game1.TILESIZE);
                case "botRight":
                    return new Rectangle(Game1.TILESIZE*2, Game1.TILESIZE*2, Game1.TILESIZE, Game1.TILESIZE);
                case "bot":
                    return new Rectangle(Game1.TILESIZE, Game1.TILESIZE*2, Game1.TILESIZE, Game1.TILESIZE);
            }

            return new Rectangle(0, 0, Game1.TILESIZE, Game1.TILESIZE);
        }
        */

    }
}
