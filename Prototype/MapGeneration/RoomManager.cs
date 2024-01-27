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
        private Room[] _rooms;

        // Constructors

        /// <summary>
        /// Creates connected set of rooms
        /// </summary>
        /// <param name="numRooms"> number of rooms to create </param>
        public RoomManager(int numRooms)
        {
            _rooms = new Room[numRooms];

            // Create original room
            _rooms[0] = new Room(new Point(Game1.WINDOW_WIDTH/3, Game1.WINDOW_HEIGHT/3));
        }

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Draw all rooms
            for (int i = 0; i < _rooms.Length; i++)
            {
                _rooms[i].Draw(spriteBatch, gameTime);
            }
        }
    }
}
