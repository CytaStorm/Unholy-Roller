using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Final_Game.LevelGen
{
	public class Room : IGameObject
	{

		#region Fields
		/// <summary>
		/// First clear of room, used to make empty rooms automatically clear.
		/// </summary>
		private bool _firstClear = true;
		#endregion

		#region Properties
		/// <summary>
		/// Top left corner of the room.
		/// </summary>
		public Point Origin { get; private set; } = new Point(0, 0);

		/// <summary>
		/// Center of the room.
		/// </summary>
		public Point Center
		{
			get => new Point(
				Origin.X + Tileset.Width / 2,
				Origin.Y + Tileset.Height / 2);
		}

		/// <summary>
		/// Has the room been cleared of enemies?
		/// </summary>
		public bool Cleared { get; private set; }

		/// <summary>
		/// Where on the map the room is.
		/// </summary>
		public Point MapPosition { get; private set; }

		/// <summary>
		/// X position on map where the room is.
		/// </summary>
		public int X { get { return MapPosition.X; } }
		/// <summary>
		/// Y position on map where the room is.
		/// </summary>
		public int Y { get { return MapPosition.Y; } }

		/// <summary>
		/// Tileset of the room containing layout, enemies,\
		/// and obstacles.
		/// </summary>
		public Tileset Tileset { get; set; }

		/// <summary>
		/// Which layout of the room floor the room uses.
		/// </summary>
		public int RoomFloorLayout { get; set; }
		/// <summary>
		/// Which layout of enemies the room uses.
		/// </summary>
		public int EnemyPositionLayout { get; set; }
		/// <summary>
		/// Which layout of obstacles the room uses.
		/// </summary>
		public int ObstaclePositionLayout { get; set; }

		/// <summary>
		/// Does the room have a possible connection 
		/// on its North/South/East/West side?
		/// </summary>
		public Dictionary<string, bool> PossibleConnections { get; private set; }
		/// <summary>
		/// To which rooms the room is actually connected to.
		/// </summary>
		public Dictionary<string, bool> ActualConnections { get; private set; }

		/// <summary>
		/// Is this room the boss room?
		/// </summary>
		public bool IsBossRoom { get; set; }
		#endregion

		#region Constructor(s)

		/// <summary>
		/// Constructs a room.
		/// </summary>
		/// <param name="mapPosition">Position of room in the level map.</param>
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
			ActualConnections = new Dictionary<string, bool>()
			{
				{ "North", false },
				{ "South", false },
				{ "East", false },
				{ "West", false }
			};

			//Create the tileset.
			Tileset = new Tileset();
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
				ActualConnections["North"] = Level.Map[X - 1, Y] != null;
			}
			if (X != Level.Map.GetLength(0) - 1)
			{
				ActualConnections["South"] = Level.Map[X + 1, Y] != null;
			}
			if (Y != Level.Map.GetLength (1) - 1)
			{
				ActualConnections["East"] = Level.Map[X, Y + 1] != null;
			}
			if (Y != 0) 
			{
				ActualConnections["West"] = Level.Map[X, Y - 1] != null;
			}
			Tileset.CreateClosedDoors(ActualConnections);
		}

		/// <summary>
		/// Checks if the room has been cleared.
		/// </summary>
		public void CheckCleared()
		{
			Cleared = Tileset.EnemyCount < 1;
			//Firstclear added so room only adds open doors once.
			if (Cleared && _firstClear)
			{
				_firstClear = false;
				//Method to create open doors
				Tileset.CreateOpenDoors(ActualConnections);
			}
		}

		public void Update(GameTime gameTime)
		{
			CheckCleared();
		}

		public void Draw(SpriteBatch sb)
		{
			Tileset.Draw(sb);
		}
		#endregion
	}
}
