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

        protected Game1 gameManager;

        // Properties

        /// <summary>
        /// The top left corner of the room
        /// </summary>
        public Point Origin { get; private set; }

        public Point Center { 
            get => new Point(
                Origin.X + Floor.Width / 2,
                Origin.Y + Floor.Height / 2);
        }

        /// <summary>
        /// The room's tileset
        /// </summary>
        public Tileset Floor { get; private set; }

        /// <summary>
        /// The filepath of the room's tileset
        /// </summary>
        public string FloorFilepath { get; private set; }
        /// <summary>
        /// The filepath of the room's possible enemy
        /// positions.
        /// </summary>
        public string EnemyPositionFilepath { get; private set; }
        /// <summary>
        /// The filepath of the room's possible obstacles
        /// (walls, hazards, etc.)
        /// </summary>
        public string ObstaclePositionFilepath { get; private set; }
        
        /// <summary>
        /// The interactable objects in the room
        /// </summary>
        public List<MapOBJ> Interactables { get; set; }

        /// <summary>
        /// The rooms enemies
        /// </summary>
        public List<Enemy> Enemies { get; set; }

        public bool Cleared { get; private set; }

        public List<Room> Connections { get; set; }

        // Constructors

        public Room(Game1 gm, Point origin, int numDoors)
        {
            // Determine folder name
            string folder = numDoors + "Door";

            // Create a random tileset within the folder

            FloorFilepath = GetRandomMap(folder);
            Floor = new Tileset(FloorFilepath, EnemyPositionFilepath, ObstaclePositionFilepath, origin);

            // Save top left coordinate of room
            Origin = origin;

            Connections = new List<Room>();

            gameManager = gm;
        }

        public Room(Game1 gm, string floorFilename, string enemyPosFilename,
            string obstaclePosFilename, Point origin)
        {
            FloorFilepath = floorFilename;
            
            Floor = new Tileset(floorFilename, enemyPosFilename, obstaclePosFilename, origin);
            Origin = origin;

            Interactables = new List<MapOBJ>();
            Enemies = new List<Enemy>();

            Connections = new List<Room>();

            gameManager = gm;
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

        public void PlayerEntered()
        {
            if (!Cleared && this != Game1.SPIKE_ROOM)
            {
                // Close doors
                foreach(Tile d in Floor.Doors)
                {
                    Interactables.Add(new DoorOBJ(
                        gameManager,
                        d.WorldPosition,
                        this));
                }

                Game1.EManager.CreateRoomEnemies(this);
                Game1.EManager.OnLastEnemyKilled += ReactToEnemyClear;
            }
            else
            {
                // Add transfer tiles
                for (int i = 0; i < Floor.Doors.Count; i++)
                {
                    Interactables.Add(new TransferTileOBJ(
                        gameManager,
                        Floor.Doors[i].WorldPosition,
                        this,
                        Connections[i],
                        FindCorrespondingDoor(Floor.Doors[i], Connections[i])));
                }
            }
        }

        public void PlayerLeft()
        {
            Interactables.Clear();
        }

        public void ReactToEnemyClear()
        {
            Interactables.Clear();

            Cleared = true;

            // Add transfer tiles
            for (int i = 0; i < Floor.Doors.Count; i++)
            {
                Interactables.Add(new TransferTileOBJ(
                    gameManager,
                    Floor.Doors[i].WorldPosition,
                    this,
                    Connections[i],
                    FindCorrespondingDoor(Floor.Doors[i], Connections[i])));
            }

            // Drop a heart in the middle of the room
            Interactables.Add(new HealthOBJ(
                gameManager,
                new Vector2(Origin.X + Floor.Width / 2, Origin.Y + Floor.Height / 2),
                this));
        }

        public void Reset()
        {
            Game1.EManager.OnLastEnemyKilled -= ReactToEnemyClear;

            Interactables.Clear();
            Cleared = false;
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < Interactables.Count; i++)
            {
                if (!Interactables[i].Alive)
                {
                    Interactables.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

            Floor.Draw(spriteBatch, gameTime);
        }

        public Tile FindCorrespondingDoor(Tile d1, Room r)
        {
            string targetOrientation = "";

            switch (d1.Doorientaiton)
            {
                case "top":
                    targetOrientation = "bottom";
                    break;

                case "bottom":
                    targetOrientation = "top";
                    break;

                case "left":
                    targetOrientation = "right";
                    break;

                case "right":
                    targetOrientation = "left";
                    break;
            }

            // Find target door
            foreach(Tile d in r.Floor.Doors)
            {
                if (d.Doorientaiton == targetOrientation)
                    return d;
            }


            throw new Exception("No corresponding door found");
        }
    }
}
