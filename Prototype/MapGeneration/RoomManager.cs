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
        public RoomManager(int numRooms)
        {
            _rooms = new List<Room>();

            // Create original room
            _rooms.Add(new Room(new Point(Game1.WINDOW_WIDTH/3, Game1.WINDOW_HEIGHT/3)));
            CreatedRooms++;

            // Find a connecting room for each door
            for (int i = 0; i < _rooms[0].Floor.Doors.Count; i++)
            {
                Tile door = _rooms[0].Floor.Doors[i];

                // Find where the door is
                string dorientation = "";

                if (door.WorldPosition.X == _rooms[0].Origin.X)
                {
                    dorientation = "left";
                }
                else if (door.WorldPosition.X == _rooms[0].Origin.X + _rooms[0].Floor.Width - Game1.TILESIZE)
                {
                    dorientation = "right";
                }
                else if (door.WorldPosition.Y == _rooms[0].Origin.Y)
                {
                    dorientation = "top";
                }
                else if (door.WorldPosition.Y == _rooms[0].Origin.Y + _rooms[0].Floor.Height - Game1.TILESIZE)
                {
                    dorientation = "bottom";
                }

                // Find a room that will connect to the door

                bool wouldConnect = false;
                Room r = null;
                while (!wouldConnect)
                {
                    // Create the room where it should be relative to the current room
                    if (dorientation == "right")
                    {
                        r = new Room(new Point(_rooms[0].Origin.X + _rooms[0].Floor.Width, _rooms[0].Origin.Y));
                    }
                    else if (dorientation == "left")
                    {
                        r = new Room(new Point(_rooms[0].Origin.X - _rooms[0].Floor.Width, _rooms[0].Origin.Y));
                    }
                    else if (dorientation == "top")
                    {
                        r = new Room(new Point(_rooms[0].Origin.X, _rooms[0].Origin.Y - _rooms[0].Floor.Height));
                    }
                    else if (dorientation == "bottom")
                    {
                        r = new Room(new Point(_rooms[0].Origin.X, _rooms[0].Origin.Y + _rooms[0].Floor.Height));
                    }

                    // Find a connecting room
                    foreach (Tile d in r.Floor.Doors)
                    {
                        wouldConnect =
                            (dorientation == "left" && d.WorldPosition.X == r.Origin.X + r.Floor.Width - Game1.TILESIZE) ||
                            (dorientation == "top" && d.WorldPosition.Y == r.Origin.Y + r.Floor.Height - Game1.TILESIZE) ||
                            (dorientation == "right" && d.WorldPosition.X == r.Origin.X) ||
                            (dorientation == "bottom" && d.WorldPosition.Y == r.Origin.Y);

                        if (wouldConnect)
                        {
                            if (dorientation == "top" || dorientation == "bottom")
                            {
                                r.Move(new Vector2(door.WorldPosition.X - d.WorldPosition.X, 0));
                            }
                            else if (dorientation == "right" || dorientation == "left")
                            {
                                r.Move(new Vector2(0, door.WorldPosition.Y - d.WorldPosition.Y));
                            }
                           
                            break;
                        }
                    }
                }

                _rooms.Add(r);
                CreatedRooms++;
            }
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
