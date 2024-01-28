using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.MapGeneration
{
    internal class Room : IGameEntity
    {
        // Fields
        private static string[] _floors = new string[] { "TestMap.txt","TestMap2.txt" ,"TestMap3.txt", "TestMap4.txt" };

        // Properties
        public Point Origin { get; private set; }

        public TileSet Floor { get; private set; }
        
        public List<MapOBJ> Interactables { get; set; }

        public List<Dummy> Enemies { get; set; }

        // Constructors

        public Room() : this(new Point(0, 0), _floors[new Random().Next(_floors.Length)]) { }

        public Room(string floorName) : this(new Point(0, 0), floorName) { }
        
        public Room(Point origin) : this(origin, _floors[new Random().Next(_floors.Length)]) { }

        public Room(Point origin, string floorName)
        {
            Random rng = new Random();
            Floor = new TileSet("../../../TileMaps/" + floorName, origin);

            Origin = origin;
        }

        // Methods

        public void Move(Vector2 distance)
        {
            for (int y = 0; y < Floor.Layout.GetLength(1); y++)
            {
                for (int x = 0; x < Floor.Layout.GetLength(0); x++)
                {
                    Tile curTile = Floor.Layout[x, y];

                    curTile.WorldPosition += distance;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Floor.Draw(spriteBatch, gameTime);
        }
    }
}
