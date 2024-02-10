using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prototype.GameEntity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.MapGeneration
{
    public class Room : IGameObject
    {
        // Fields
        public const int GREATEST_POSSIBLE_NUM_DOORS = 3;

        // Properties

        /// <summary>
        /// The top left corner of the room
        /// </summary>
        public Point Origin { get; private set; }

        /// <summary>
        /// The room's tileset
        /// </summary>
        public Tileset Floor { get; private set; }

        /// <summary>
        /// The filepath of the room's tileset
        /// </summary>
        public string FloorFilepath { get; private set; }
        
        /// <summary>
        /// The interactable objects in the room
        /// </summary>
        public List<MapOBJ> Interactables { get; set; }

        /// <summary>
        /// The rooms enemies
        /// </summary>
        public List<Enemy> Enemies { get; set; }

        // Constructors

        public Room(Point origin, int numDoors)
        {
            // Determine folder name
            string folder = numDoors + "Door";

            // Create a random tileset within the folder

            FloorFilepath = GetRandomMap(folder);
            Floor = new Tileset(FloorFilepath, origin);

            // Save top left coordinate of room
            Origin = origin;
        }

        public Room(string filename, Point origin)
        {
            FloorFilepath = filename;
            Floor = new Tileset(filename, origin);

            Origin = origin;
        }

        // Methods

        /// <summary>
        /// Moves the room and all of its components by a distance
        /// </summary>
        /// <param name="distance"> the distance to move </param>
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
