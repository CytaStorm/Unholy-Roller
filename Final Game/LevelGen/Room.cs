using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Final_Game.LevelGen
{
	public class Room : IGameObject
	{

		#region Fields
		public bool Cleared { get; private set; } = true;
		/// <summary>
		/// Top left corner of the room.
		/// </summary>
		public Point Origin = new Point(0, 0);
		/// <summary>
		/// Center of the room.
		/// </summary>
		public Point Center
		{
			get => new Point(
				Origin.X + RoomFloor.Width / 2,
				Origin.Y + RoomFloor.Height / 2);
		}
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
		public Dictionary<string, bool> Connections;
		public Tileset RoomFloor { get; set; }

		public int RoomFloorLayout { get; set; }
		public int EnemyPositionLayout { get; set; }
		public int ObstaclePositionLayout { get; set; }
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

			Connections = new Dictionary<string, bool>()
			{
				{ "North", false },
				{ "South", false },
				{ "East", false },
				{ "West", false }
			};
			RoomFloor = new Tileset();
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
				Connections["North"] = Level.Map[X - 1, Y] != null;
			}
			if (X != Level.Map.GetLength(0) - 1)
			{
				Connections["South"] = Level.Map[X + 1, Y] != null;
			}
			if (Y != Level.Map.GetLength (1) - 1)
			{
				Connections["East"] = Level.Map[X, Y + 1] != null;
			}
			if (Y != 0) 
			{
				Connections["West"] = Level.Map[X, Y - 1] != null;
			}
			RoomFloor.CreateClosedDoors(Connections);
		}

		public void CheckCleared()
		{
			Cleared = RoomFloor.EnemyCount < 1;
			if (Cleared)
			{
				//Method to create open doors
				RoomFloor.CreateOpenDoors(Connections);
			}
		}

		public void Update(GameTime gameTime)
		{
			CheckCleared();
		}

		public void Draw(SpriteBatch sb)
		{
			RoomFloor.Draw(sb);
		}
		#endregion
	}
}
