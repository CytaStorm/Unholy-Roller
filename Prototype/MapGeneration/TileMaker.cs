using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.MapGeneration
{
    public enum TileType
    {
        Grass,
        Wall,
        Spike,
        Placeholder
    }

    public class TileMaker
    {
        // Fields 
        private static Texture2D[] _tileTextures;

        // The uniform dimensions of all map tiles
        private const int sourceTilesize = 60;

        Game1 gm;

        // Constructors
        public TileMaker(Game1 gm)
        {
            this.gm = gm;

            // Load all tile spritesheets
            _tileTextures = new Texture2D[5];
            _tileTextures[0] = gm.Content.Load<Texture2D>("PlaceholderTile");
            _tileTextures[1] = gm.Content.Load<Texture2D>("GrassTile");
            _tileTextures[2] = gm.Content.Load<Texture2D>("WallSheet");
            _tileTextures[3] = gm.Content.Load<Texture2D>("SpikeTile");
        }

        // Methods

        /// <summary>
        /// Creates a tile of the specified type
        /// </summary>
        /// <param name="tileType"> type of tile to create </param>
        /// <returns> created tile </returns>
        public static Tile SetTile(TileType tileType)
        {
            return SetTile(tileType, new Vector2(0f, 0f), "");
        }

        /// <summary>
        /// Creates a tile of the specified type
        /// </summary>
        /// <param name="tileType"> type of tile to create </param>
        /// <param name="position"> position of the tile </param>
        /// <returns> created tile </returns>
        public static Tile SetTile(TileType tileType, Vector2 position)
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
        public static Tile SetTile(TileType tileType, Vector2 position, string orientation)
        {
            Tile result = new Tile();
            result.Type = (TileType)tileType;

            result.WorldPosition = position;


            switch (tileType)
            {
                case TileType.Grass:

                    result.Collidable = false;

                    result.TileSprite = new Sprite(_tileTextures[1], GetOrientationSource(""), 
                        new Rectangle(
                            (int)position.X, 
                            (int)position.Y, 
                            Game1.TILESIZE, 
                            Game1.TILESIZE));
                    break;

                case TileType.Wall:

                    result.Collidable = true;

                    result.TileSprite = new Sprite(_tileTextures[2], GetOrientationSource(orientation),
                        new Rectangle(
                            (int)position.X,
                            (int)position.Y,
                            Game1.TILESIZE,
                            Game1.TILESIZE));
                    break;

                case TileType.Spike:

                    result.Collidable = true;

                    result.TileSprite = new Sprite(_tileTextures[3], GetOrientationSource(""),
                        new Rectangle(
                            (int)position.X,
                            (int)position.Y,
                            Game1.TILESIZE,
                            Game1.TILESIZE));
                    break;

                default:

                    result.Collidable = false;

                    result.TileSprite = new Sprite(_tileTextures[0], GetOrientationSource(""),
                        new Rectangle(
                            (int)position.X,
                            (int)position.Y,
                            Game1.TILESIZE,
                            Game1.TILESIZE));
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
