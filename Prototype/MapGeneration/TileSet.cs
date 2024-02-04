using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System.IO;

namespace Prototype.MapGeneration
{
    public class TileSet : IGameEntity
    {
        // Fields
        public bool devRendering = true;

        // Properties

        /// <summary>
        /// Set of individual tiles comprising this tileset
        /// </summary>
        public Tile[,] Layout { get; private set; }

        /// <summary>
        /// The number of vertical columns in this tileset
        /// </summary>
        public int Columns { get => Layout.GetLength(1); }

        /// <summary>
        /// The number of horizontal rows in this tileset
        /// </summary>
        public int Rows { get => Layout.GetLength(0); }

        /// <summary>
        /// Width of the tileset
        /// </summary>
        public int Width { get => Columns * Game1.TILESIZE; }

        /// <summary>
        /// Height of the tileset
        /// </summary>
        public int Height { get => Rows * Game1.TILESIZE; }

        /// <summary>
        /// The doors or bridge-points of this tileset
        /// </summary>
        public List<Tile> Doors { get; private set; } = new List<Tile>();

        // Constructors

        /*
        public TileSet(string filename, int rows, int columns, Point origin)
        {
            // Open the file
            StreamReader input = new StreamReader(filename);

            // Create tile grid
            Layout = new Tile[rows, columns];

            // Convert file values to tiles
            int y = 0;
            string line = "";
            while ((line = input.ReadLine()!) != null)
            {
                // Split line into tile ids
                string[] tileValues = line.Split(" ");
                
                // Add tiles to tile grid
                for (int x = 0; x < columns; x++)
                {
                    int tileId = -1;

                    // Unparsable tile values are probably doors (denoted by '*')
                    bool isDoor = !int.TryParse(tileValues[x], out tileId) && tileValues[x].Length > 1;

                    Layout[y, x] = TileMaker.SetTile(
                        tileId,
                        new Vector2(x * Game1.TILESIZE, y * Game1.TILESIZE));

                    Layout[y, x].IsDoor = isDoor;
                }

                // Move to next row
                y++;
            }

            input.Close();
        }
        */

        /// <summary>
        /// Creates a square tileset from the integer values in a text file
        /// </summary>
        /// <param name="filename"> the file to read </param>
        public TileSet(string filename, Point origin)
        {
            // Open the file
            StreamReader input = new StreamReader(filename);

            // Convert file values to tiles
            int y = 0;
            string line = "";
            while ((line = input.ReadLine()!) != null)
            {
                // Split line into tile ids
                string[] tileValues = line.Split(" ");

                // Set layout size based off of line width
                if (Layout == null)
                {
                    Layout = new Tile[tileValues.Length, tileValues.Length];
                }
                
                // Add tiles to tile grid
                for (int x = 0; x < Layout.GetLength(0); x++)
                {
                    int tileId = -1;

                    bool isDoor = false;
                    string orientation = "";

                    // Unparsable tile values are probably special tiles
                    if (!int.TryParse(tileValues[x], out tileId))
                    {
                        string[] sValues = tileValues[x].Split('.');

                        // Read tile type 
                        tileId = int.Parse(sValues[0]);

                        if (sValues.Length == 2)
                        {
                            // Read tile specification
                            if (sValues[1] == "*")
                            {
                                isDoor = true;
                            }
                            else
                            {
                                orientation = sValues[1];
                            }
                        }

                    }

                    // Create and add tile
                    Layout[y, x] = TileMaker.SetTile(
                        tileId,
                        new Vector2(origin.X + x*Game1.TILESIZE, origin.Y + y*Game1.TILESIZE), 
                        orientation);

                    // Set door status
                    Layout[y, x].IsDoor = isDoor;
                    if (isDoor)
                    {
                        // Store door
                        Doors.Add(Layout[y, x]);
                    }
                    
                }

                // Move to next row
                y++;
            }

            input.Close();
        }

        // Methods

        public void Update(GameTime gameTime)
        {
            // TODO: Check for events
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {


            // Draw all tiles
            for (int y = 0; y < Layout.GetLength(1); y++)
            {
                for (int x = 0; x < Layout.GetLength(0); x++)
                {
                    Tile curTile = Layout[x, y];

                    // Find screen position
                    Vector2 distanceFromPlayer = curTile.WorldPosition - Game1.Player1.WorldPosition;
                    Vector2 screenPos = distanceFromPlayer + Game1.Player1.ScreenPosition;

                    if (devRendering && curTile.IsDoor)
                    {
                        curTile.TileSprite.TintColor = Color.Pink;

                        Layout[x, y].Draw(spriteBatch, gameTime, screenPos);

                        curTile.TileSprite.TintColor = Color.White;
                    }
                    else
                    {
                        Layout[x, y].Draw(spriteBatch, gameTime, screenPos);
                    }
                }
            }
        }
    }
}
