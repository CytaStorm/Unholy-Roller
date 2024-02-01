using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.MapGeneration
{
    internal class Room : IGameEntity
    {
        // Fields
        public const int GREATEST_POSSIBLE_NUM_DOORS = 3;

        // Properties
        public Point Origin { get; private set; }

        public TileSet Floor { get; private set; }

        public string FloorFilepath { get; private set; }
        
        public List<MapOBJ> Interactables { get; set; }

        public List<Dummy> Enemies { get; set; }

        // Constructors

        public Room(Point origin, int numDoors)
        {
            // Determine folder name
            string folder = numDoors + "Door";

            // Create a random tileset within the folder

            FloorFilepath = GetRandomMap(folder);
            Floor = new TileSet(FloorFilepath, origin);

            // Save top left coordinate of room
            Origin = origin;
        }

        // Methods

        public void Move(Vector2 distance)
        {
            // Move tileset by distance
            for (int y = 0; y < Floor.Layout.GetLength(0); y++)
            {
                for (int x = 0; x < Floor.Layout.GetLength(1); x++)
                {
                    Tile curTile = Floor.Layout[y, x];

                    curTile.WorldPosition += distance;
                }
            }

            // Change origin to match tileset starting point
            Origin = new Point(Origin.X + (int)distance.X, Origin.Y + (int)distance.Y);
        }

        private string GetRandomMap(string folder)
        {
            Random rng = new Random();

            // Choose a random file out of the specified folder
            string[] fileNames = Directory.GetFiles($"../../../TileMaps/{folder}");

            // Return name of that file
            return fileNames[new Random().Next(fileNames.Length)];
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
