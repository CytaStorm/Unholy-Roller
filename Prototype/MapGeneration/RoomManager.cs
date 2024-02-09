using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Prototype.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.MapGeneration
{
    public class RoomManager : IGameObject
    {
        // Fields
        private Random _rng;

        // Properties
        public List<Room> Rooms { get; private set; }

        // Constructors

        /// <summary>
        /// Creates a connected set of rooms
        /// </summary>
        /// <param name="roomsToMake"> number of rooms to make </param>
        /// <exception cref="Exception"> Cannot make less than two rooms </exception>
        public RoomManager(int roomsToMake)
        {
            if (roomsToMake < 2)
            {
                throw new Exception("Cannot have open doorway. At least 2 rooms must be created.");
            }

            // Setup room list
            Rooms = new List<Room>();

            // Calculate number of doors for origin room
            _rng = new Random();
            int doorMax = Math.Min(roomsToMake, Room.GREATEST_POSSIBLE_NUM_DOORS);
            //int numDoors = _rng.Next(1, doorMax + 1);

            // Create origin room
            Room oRoom = new Room(new Point(Game1.WINDOW_WIDTH / 3, Game1.WINDOW_HEIGHT / 3), doorMax);

            // Store origin
            Rooms.Add(oRoom);

            MakeBranch(oRoom, roomsToMake);

        }

        // Methods

        /// <summary>
        /// Creates a connecting room for each of the specified room's doors
        /// </summary>
        /// <param name="room"> room to branch off of </param>
        /// <param name="roomsToMake"> final number of rooms that must be created </param>
        private void MakeBranch(Room room, int roomsToMake)
        {
            // Find a connecting room for each door
            for (int i = 0; i < room.Floor.Doors.Count; i++)
            {
                Tile door = room.Floor.Doors[i];

                // Skip doors that are already connected
                if (door.Bridged)
                {
                    continue;
                }

                // Find which side door is on
                string dorientation = "";
                if (door.WorldPosition.X == room.Origin.X)
                {
                    dorientation = "left";
                }
                else if (door.WorldPosition.X == room.Origin.X + room.Floor.Width - Game1.TILESIZE)
                {
                    dorientation = "right";
                }
                else if (door.WorldPosition.Y == room.Origin.Y)
                {
                    dorientation = "top";
                }
                else if (door.WorldPosition.Y == room.Origin.Y + room.Floor.Height - Game1.TILESIZE)
                {
                    dorientation = "bottom";
                }


                // Find a room that will connect to the door

                bool wouldConnect = false;
                Room r = null;
                while (!wouldConnect)
                {
                    int totalDoors = GetTotalDoors();
                    int maxDoors = Math.Min(Room.GREATEST_POSSIBLE_NUM_DOORS, roomsToMake - (totalDoors - 1) + 1);
                    if (maxDoors <= 0)
                    {
                        maxDoors = 1;
                    }


                    // Make max doors unless all rooms can be made
                    // with remaining open doors
                    int numDoors = maxDoors;
                    /*
                    if (GetOpenDoors() - 1 > 0)
                    {
                        numDoors = _rng.Next(1, maxDoors + 1);
                    }
                    */

                    // Make a new random room
                    r = new Room(new Point(room.Origin.X, room.Origin.Y), numDoors);

                    // Position it properly relative to the current room
                    Vector2 shift = Vector2.Zero;
                    switch (dorientation)
                    {
                        case "right":
                            shift = new Vector2(room.Floor.Width, 0);
                            break;
                        case "left":
                            shift = new Vector2(-r.Floor.Width, 0);
                            break;
                        case "top":
                            shift = new Vector2(0, -r.Floor.Height);
                            break;
                        case "bottom":
                            shift = new Vector2(0, room.Floor.Height);
                            break;
                    }

                    r.Move(shift);

                    // Check if any of new room's doors would connect
                    for (int j = 0; j < r.Floor.Doors.Count; j++)
                    {
                        Tile d = r.Floor.Doors[j];

                        wouldConnect =
                            (dorientation == "left" && d.WorldPosition.X == r.Origin.X + r.Floor.Width - Game1.TILESIZE) ||
                            (dorientation == "top" && d.WorldPosition.Y == r.Origin.Y + r.Floor.Height - Game1.TILESIZE) ||
                            (dorientation == "right" && d.WorldPosition.X == r.Origin.X) ||
                            (dorientation == "bottom" && d.WorldPosition.Y == r.Origin.Y);

                        // Connection found
                        if (wouldConnect)
                        {
                            // Shift new room to match current room's door
                            if (dorientation == "top" || dorientation == "bottom")
                            {
                                r.Move(new Vector2(door.WorldPosition.X - d.WorldPosition.X, 0));
                            }
                            else if (dorientation == "right" || dorientation == "left")
                            {
                                r.Move(new Vector2(0, door.WorldPosition.Y - d.WorldPosition.Y));
                            }

                            // Door has connected
                            d.Bridged = true;
                            
                            break;
                        }
                    }
                }
                
                // Store the new room
                Rooms.Add(r);

                // Door has connected
                door.Bridged = true;
                    
                // Make branches off of new room
                MakeBranch(r, roomsToMake);
            }

        }

        /// <summary>
        /// Gets the total number of doors in this set of rooms
        /// </summary>
        /// <returns> the sum of all doors in every room </returns>
        private int GetTotalDoors()
        {
            int total = 0;

            for (int i = 0; i < Rooms.Count; i++)
            {
                total += Rooms[i].Floor.Doors.Count;
            }

            return total;
        }

        private int GetOpenDoors()
        {
            int total = 0;

            foreach (Room r in Rooms)
            {
                foreach (Tile d in r.Floor.Doors){
                    if (d.Bridged == false)
                    {
                        total++;
                    }
                }
            }

            return total;
        }

        /// <summary>
        /// Moves every room and its components by a distance
        /// </summary>
        /// <param name="distance"> distance to move </param>
        public void Move(Vector2 distance)
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                Rooms[i].Move(distance);
            }
        }
        
        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

            // Draw all rooms
            for (int i = 0; i < Rooms.Count; i++)
            {
                Rooms[i].Draw(spriteBatch, gameTime);
            }
        }
    }
}
