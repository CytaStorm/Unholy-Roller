using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.MapGeneration
{
    internal class RoomManager : IGameEntity
    {
        // Fields
        private List<Room> _rooms;

        // Constructors

        /// <summary>
        /// The number of rooms that have been generated so far
        /// </summary>
        public int CreatedRooms { get; private set; }

        /// <summary>
        /// Creates connected set of rooms
        /// </summary>
        /// <param name="numRooms"> number of rooms to create </param>
        public RoomManager(int roomsToMake)
        {
            if (roomsToMake < 2)
            {
                throw new Exception("Cannot make less than two rooms");
            }

            _rooms = new List<Room>();

            Random rng = new Random();

            int doorMax = Math.Min(roomsToMake, Room.GREATEST_POSSIBLE_NUM_DOORS);

            // Create original room
            Room oRoom = new Room(new Point(Game1.WINDOW_WIDTH / 3, Game1.WINDOW_HEIGHT / 3), rng.Next(1, doorMax + 1));
            _rooms.Add(oRoom);
            CreatedRooms++;

            MakeBranch(oRoom, roomsToMake);

        }

        private void MakeBranch(Room room, int roomsToMake)
        {
            // Find a connecting room for each door
            for (int i = 0; i < room.Floor.Doors.Count; i++)
            {
                Tile door = room.Floor.Doors[i];

                // Don't try to connect to doors that already have
                // a connection
                if (door.Bridged)
                {
                    continue;
                }

                // Find where the door is
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

                int numLoops = 0; // Debugging

                bool wouldConnect = false;
                Room r = null;
                while (!wouldConnect)
                {
                    if (numLoops > 10000)
                    {
                        Console.WriteLine("Error");
                    }

                    int totalDoors = GetTotalDoors();
                    int maxDoors = Math.Min(Room.GREATEST_POSSIBLE_NUM_DOORS, roomsToMake - totalDoors + 1);
                    if (totalDoors >= roomsToMake)
                    {
                        maxDoors = 1;
                    }

                    // Make a new random room
                    r = new Room(new Point(room.Origin.X, room.Origin.Y), maxDoors);

                    // Shift over new room off of its origin
                    Vector2 shift = Vector2.Zero;

                    // Create the room where it should be relative to the current room
                    if (dorientation == "right")
                    {
                        //r = new Room(new Point(room.Origin.X + room.Floor.Width, room.Origin.Y), maxDoors);
                        shift = new Vector2(room.Floor.Width, 0);
                    }
                    else if (dorientation == "left")
                    {
                        //r = new Room(new Point(room.Origin.X - room.Floor.Width, room.Origin.Y), maxDoors);
                        shift = new Vector2(-r.Floor.Width, 0);
                    }
                    else if (dorientation == "top")
                    {
                        //r = new Room(new Point(room.Origin.X, room.Origin.Y - room.Floor.Height), maxDoors);
                        shift = new Vector2(0, -r.Floor.Height);
                    }
                    else if (dorientation == "bottom")
                    {
                        //r = new Room(new Point(room.Origin.X, room.Origin.Y + room.Floor.Height), maxDoors);
                        shift = new Vector2(0, room.Floor.Height);
                    }

                    if (shift == Vector2.Zero)
                    {
                        Console.WriteLine("No shift applied");
                    }

                    // Apply shift to new room
                    r.Move(shift);

                    // Find a connecting room
                    for (int j = 0; j < r.Floor.Doors.Count; j++)
                    {
                        Tile d = r.Floor.Doors[j];


                        wouldConnect =
                            (dorientation == "left" && d.WorldPosition.X == r.Origin.X + r.Floor.Width - Game1.TILESIZE) ||
                            (dorientation == "top" && d.WorldPosition.Y == r.Origin.Y + r.Floor.Height - Game1.TILESIZE) ||
                            (dorientation == "right" && d.WorldPosition.X == r.Origin.X) ||
                            (dorientation == "bottom" && d.WorldPosition.Y == r.Origin.Y);

                        if (wouldConnect)
                        {
                            // Shift to match up with door
                            if (dorientation == "top" || dorientation == "bottom")
                            {
                                r.Move(new Vector2(door.WorldPosition.X - d.WorldPosition.X, 0));
                            }
                            else if (dorientation == "right" || dorientation == "left")
                            {
                                r.Move(new Vector2(0, door.WorldPosition.Y - d.WorldPosition.Y));
                            }

                            d.Bridged = true;
                           
                            break;
                        }
                    }

                    numLoops++;
                }
                
                _rooms.Add(r);
                door.Bridged = true;
                CreatedRooms++;

                MakeBranch(r, roomsToMake);
            }

        }

        private int GetTotalDoors()
        {
            int total = 0;

            for (int i = 0; i < _rooms.Count; i++)
            {
                total += _rooms[i].Floor.Doors.Count;
            }

            return total;
        }
        
        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Draw all rooms
            for (int i = 0; i < _rooms.Count; i++)
            {
                _rooms[i].Draw(spriteBatch, gameTime);
            }
        }
    }
}
