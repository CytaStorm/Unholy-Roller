using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Final_Game.LevelGen
{
	internal class Room : IGameObject
	{

		#region Fields
		/// <summary>
		/// Where on the map the room is.
		/// </summary>
		public Point MapPosition;
		/// <summary>
		/// X position on map where the room is.
		/// </summary>
		public int X { get { return MapPosition.X; } }
		/// <summary>
		/// Y position on map where the room is.
		/// </summary>
		public int Y { get { return MapPosition.Y; } }
		/// <summary>
		/// Does the room have a possible connection 
		/// on its North/South/East/West side?
		/// </summary>
		public Dictionary<string, bool> PossibleConnections;
		/// <summary>
		/// To which rooms the room is connected to.
		/// </summary>
		public Dictionary<string, Room> Connections;
		#endregion

		#region Constructor(s)
		public Room(Point mapPosition)
		{
			//Set up room data.
			MapPosition = mapPosition;
			PossibleConnections = new Dictionary<string, bool>()
			{
				{ "North", X != 0 && Level.Map[X - 1, Y] == null},
				{ "South", X != Level.Map.GetLength(0) - 1 && Level.Map[X + 1, Y] == null},
				{ "East", Y != Level.Map.GetLength(1) - 1 && Level.Map[X, Y + 1] == null},
				{ "West",  Y != 0 && Level.Map[X, Y - 1] == null}
			};

			Connections = new Dictionary<string, Room>()
			{
				{ "North", null },
				{ "South", null },
				{ "East", null },
				{ "West", null }
			};
		}
		#endregion

		#region Method(s)
		/// <summary>
		/// Updates possibilities of North/South/East/West connections.
		/// </summary>
		public void UpdateAdjacencyPossibilities()
		{
			PossibleConnections["North"] = X != 0 && Level.Map[X - 1, Y] == null;
			PossibleConnections["South"] = X != Level.Map.GetLength(0) - 1 && Level.Map[X + 1, Y] == null;
			PossibleConnections["East"] = Y != Level.Map.GetLength(1) - 1 && Level.Map[X, Y + 1] == null;
			PossibleConnections["West"] = Y != 0 && Level.Map[X, Y - 1] == null;
		}

		/// <summary>
		/// Checks to see if there are adjacent rooms.
		/// If there are, add them as connections.
		/// </summary>
		public void CreateConnections()
		{
			//These if statements are here to protect against checking
			//out of bounds rooms on the map.
			if (X != 0)
			{
				Connections["North"] = Level.Map[X - 1, Y];
			}
			if (X != Level.Map.GetLength(0) - 1)
			{
				Connections["South"] = Level.Map[X + 1, Y];
			}
			if (Y != Level.Map.GetLength (1) - 1)
			{
				Connections["East"] = Level.Map[X, Y + 1];
			}
			if (Y != 0) 
			{
				Connections["West"] = Level.Map[X, Y - 1];
			}
		}

		/// <summary>
		/// Copies provided room.
		/// </summary>
		/// <param name="roomToBeCopied">Room to copy.</param>
		/// <returns>A copy of the provided room.</returns>
		public static Room Copy(Room roomToBeCopied)
		{
			return new Room(roomToBeCopied.MapPosition);
		}

		public void Update(GameTime gameTime){

		}

		public void Draw(SpriteBatch sb, GameTime gt)
		{

		}
		#endregion
	}
}
