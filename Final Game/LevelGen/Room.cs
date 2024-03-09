using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Final_Game.LevelGen
{
	internal class Room
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
		#endregion

		#region Constructor(s)
		public Room(Point mapPosition)
		{
			//Set up room data.
			MapPosition = mapPosition;
			PossibleConnections = new Dictionary<string, bool>()
			{
				{ "North", X != 0 && Map.Level[X - 1, Y] == null},
				{ "South", X != Map.Level.GetLength(0) - 1 && Map.Level[X + 1, Y] == null},
				{ "East", Y != Map.Level.GetLength(1) - 1 && Map.Level[X, Y + 1] == null},
				{ "West",  Y != 0 && Map.Level[X, Y - 1] == null}
			};

		}
		#endregion

		#region Method(s)
		/// <summary>
		/// Updates status of North/South/East/West connections.
		/// </summary>
		public void UpdateAdjacencies()
		{
			PossibleConnections["North"] = X != 0 && Map.Level[X - 1, Y] == null;
			PossibleConnections["South"] = X != Map.Level.GetLength(0) - 1 && Map.Level[X + 1, Y] == null;
			PossibleConnections["East"] = Y != Map.Level.GetLength(1) - 1 && Map.Level[X, Y + 1] == null;
			PossibleConnections["West"] = Y != 0 && Map.Level[X, Y - 1] == null;

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
		#endregion
	}
}
