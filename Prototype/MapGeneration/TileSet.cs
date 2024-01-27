using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace Prototype.MapGeneration
{
    internal class TileSet : IGameEntity
    {
        // Properties
        public Tile[,] Layout { get; private set; }

        public TileSet(int[,] guide)
        {
            // Match size of guide
            Layout = new Tile[guide.GetLength(0),guide.GetLength(1)];

            // Set tiles using guide
            for (int y = 0; y < guide.GetLength(0); y++)
            {
                for (int x = 0; x < guide.GetLength(1); x++)
                {
                    Layout[y, x] = TileMaker.SetTile(
                        guide[y,x], 
                        new Vector2(x*Game1.TILESIZE, y*Game1.TILESIZE));
                }
            }
        }

        public TileSet(string filename, int rows, int columns)
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
                    Layout[y, x] = TileMaker.SetTile(
                        int.Parse(tileValues[x]), 
                        new Vector2(x*Game1.TILESIZE, y*Game1.TILESIZE));
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
                    Layout[x,y].Draw(spriteBatch, gameTime);
                }
            }
        }
    }
}
