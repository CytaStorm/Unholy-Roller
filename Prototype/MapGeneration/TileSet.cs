using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


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
                    Layout[y, x] = TileMaker.SetTile(guide[y,x]);
                    Layout[y, x].Position = new Vector2(x*Game1.TILESIZE, y*Game1.TILESIZE);
                }
            }
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
                    spriteBatch.Draw(Layout[x,y].Image, Layout[x,y].Position, 
                        new Rectangle(0,0,Game1.TILESIZE,Game1.TILESIZE), Color.White);
                }
            }
        }
    }
}
