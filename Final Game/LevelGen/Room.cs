using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Final_Game.LevelGen
{
	public class Room : IGameObject
	{

		#region Fields
		/// <summary>
		/// First clear of room, used to make empty rooms automatically clear.
		/// </summary>
		private bool _firstClear = true;

		private Level _parent;

		/// <summary>
		/// Random number generator.
		/// </summary>
		private Random _random = new Random();
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
		public bool[] PossibleConnections { get; private set; } = new bool[4];
		/// <summary>
		/// To which rooms the room is actually connected to.
		/// </summary>
		public Room[] ActualConnections { get; private set; } = new Room[4];
		/// <summary>
		/// How many connections the room has.
		/// </summary>
		public int NumberOfConnections {
			get
			{
				int numberOfConnections = 0;
				foreach (Room room in ActualConnections)
				{
					numberOfConnections++;
				}
				return numberOfConnections;
			}
		}

		/// <summary>
		/// Has the player discovered this room yet?
		/// </summary>
		public bool Discovered { get; set; } = false;
		/// <summary>
		/// Has the player entered this room yet?
		/// </summary>
		public bool Entered { get; set; } = false;
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
		public Room(Point mapPosition, Level parent)
		{
			this._parent = parent;

			//Set up room data.
			MapPosition = mapPosition;

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
			PossibleConnections[(int)Directions.North] = 
				X != 0 && _parent.Map[X - 1, Y] == null;
			PossibleConnections[(int)Directions.South] =
				X != _parent.Map.GetLength(0) - 1 && _parent.Map[X + 1, Y] == null;
			PossibleConnections[(int)Directions.East] =
				Y != _parent.Map.GetLength(1) - 1 && _parent.Map[X, Y + 1] == null;
			PossibleConnections[(int)Directions.West] =
				Y != 0 && _parent.Map[X, Y - 1] == null;
		}

		/// <summary>
		/// Checks to see if there are adjacent rooms.
		/// If there are, add them as connections.
		/// </summary>
		public void CreateConnections()
		{
			//These if statements are here to protect against checking
			//out of bounds rooms on the map.
			if (X != 0 && _parent.Map[X - 1, Y] != null)
			{
				ActualConnections[(int)Directions.North] = _parent.Map[X - 1, Y];
			}
			if (X != _parent.Map.GetLength(0) - 1 && _parent.Map[X + 1, Y] != null)
			{
				ActualConnections[(int)Directions.South] = _parent.Map[X + 1, Y];
			}
			if (Y != _parent.Map.GetLength (1) - 1 && _parent.Map[X, Y + 1] != null)
			{
				ActualConnections[(int)Directions.East] = _parent.Map[X, Y + 1];
			}
			if (Y != 0 && _parent.Map[X, Y - 1] != null) 
			{
				ActualConnections[(int)Directions.West] = _parent.Map[X, Y - 1];
			}
			Tileset.CreateClosedDoors(ActualConnections);
		}

		/// <summary>
		/// Checks if the room has been cleared.
		/// </summary>
		public void CheckCleared()
		{
			Cleared = Game1.EManager.Enemies.Count < 1
				&& _parent != Game1.TutorialLevel;

			//Early return if not cleared.
			if (!Cleared)
			{
				return;
			}

			//What to do on first clear of room.
			if (_firstClear)
			{
				_firstClear = false;
				//Method to create open doors
				Tileset.CreateOpenDoors(ActualConnections);
				//30% chance to create health pickup.
				if (_random.Next() <= 0.3f) CreateHealthPickup();
			}
			return;
		}

		/// <summary>
		/// Creates a health pickup at the center of room.
		/// Otherwise creates a health pickup at random point in room.
		/// </summary>
		private void CreateHealthPickup()
		{
			//Selects middle tile
			Tile selectedTile = Tileset.Layout[
				Tileset.Rows / 2,
				Tileset.Columns / 2];

			if (selectedTile.Type != LevelGen.TileType.Grass)
			{
				//Select random tile.
				selectedTile = Game1.TestLevel.CurrentRoom.Tileset.FindRandomFloorTile();
			}

			selectedTile.HasHealthPickup = true;
		}

		/// <summary>
		/// Removes enemy spawners within 5 tiles of doors.
		/// </summary>
		/// <param name="roomOffset">Offset of this room relative to 
		/// the previous room.</param>
		public void RemoveEnemiesNearDoor(Point roomOffset)
		{
			//player at bottom
			switch (roomOffset)
			{
				//Player at bottom
				case (-1, 0):
					Tileset.RemoveEnemiesNearDoor(Tileset.Doors["South"]);
					break;
				//player at top
				case (1, 0):
					Tileset.RemoveEnemiesNearDoor(Tileset.Doors["North"]);
					break;
				//player at left
				case (0, 1):
					Tileset.RemoveEnemiesNearDoor(Tileset.Doors["West"]);
					break;
				//player at right
				case (0, -1):
					Tileset.RemoveEnemiesNearDoor(Tileset.Doors["East"]);
					break;
				case (0, 0):
					Tileset.ClearEnemies();
					break;
				default:
					throw new Exception("you shouldn't be here!");
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
