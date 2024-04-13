using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mime;

namespace Final_Game.LevelGen
{
	public enum Directions
	{
		North,
		South,
		East,
		West
	}

	public class Level
	{
		#region Fields
		/// <summary>
		/// All rooms that make up the level.
		/// </summary>
		public Room[,] Map { get; private set; }

		/// <summary>
		/// List of all rooms.
		/// </summary>
		private List<Room> _rooms = new List<Room>();

		/// <summary>
		/// Random object to use in class.
		/// </summary>
		private static Random _random = new Random();

		/// <summary>
		/// Position on map of the starting room.
		/// </summary>
		public Point StartPoint;

		/// <summary>
		/// Position on map of the room player is currently in.
		/// </summary>
		public Point CurrentPoint;

		/// <summary>
		/// Position on map of the boss room.
		/// </summary>
		public Point BossPoint;

		/// <summary>
		/// Room player is in.
		/// </summary>
		public Room CurrentRoom { get { return Map[CurrentPoint.X, CurrentPoint.Y]; } }

		/// <summary>
		/// Room player started in.
		/// </summary>
		public Room StartRoom { get { return Map[StartPoint.X, StartPoint.Y]; } }

		/// <summary>
		/// Boss Room.
		/// </summary>
		public Room BossRoom { get { return Map[BossPoint.X, BossPoint.Y]; } }
		#endregion

		#region Constructor(s)
		/// <summary>
		/// Constructor for level.
		/// </summary>
		/// <param name="height">Places to generate rooms on the Y axis.</param>
		/// <param name="width">Places to generate rooms on the X axis.</param>
		/// <param name="size">How many rooms to generate.</param>
		public Level(int height, int width, int size)
		{
			Map = new Room[height, width];

			//Create first room (randomly placed)
			StartPoint = 
				new Point(
					_random.Next(Map.GetLength(0)),
					_random.Next(Map.GetLength(1)));
			Map[StartPoint.X, StartPoint.Y] = new Room(
				new Point(StartPoint.X, StartPoint.Y), 
				this);

			_rooms.Add(Map[StartPoint.X, StartPoint.Y]);
			CurrentPoint = StartPoint;

			//Expand rooms.
			for (int i = 1; i < size; i++)
			{
				//PrintLevel();
				ExpandLevel(_rooms);
			}

			foreach (Room room in _rooms)
			{
				//Create connections between rooms.
				room.CreateConnections();
			}
			StartRoom.Discovered = true;
			StartRoom.Entered = true;

			//Determine Boss Room
			DetermineBossRoom();


			//PrintLevel();
		}
		#endregion

		#region Method(s)
		/// <summary>
		/// Chooses a room to expand upon.
		/// 6x more likely to choose the room that was most
		/// recently added.
		/// </summary>
		/// <param name="roomsOriginal">List of all rooms.</param>
		private void ExpandLevel(List<Room> roomsOriginal)
		{
			//Make a copy of the passed in list of rooms.
			List<Room> roomsCopy = new List<Room>(roomsOriginal);

			List<Point> possibleExpansions = new List<Point>();

			//Makes it more likely to select previously added room to
			//expand upon.
			int branchMultiplier = 75;
			//Pick random room. Add branch multiplier and clamp to increase
			//chance of expanding on most recently expanded room.
			int roomToExpandIndex = _random.Next(roomsCopy.Count + branchMultiplier);
			roomToExpandIndex = Math.Clamp(roomToExpandIndex, 0, roomsCopy.Count - 1);

			//Debug.WriteLine("Possible rooms to expand: " + roomsCopy.Count);
			//Debug.WriteLine("Index of room to expand: " + roomToExpandIndex);

			Room roomToExpand = roomsCopy[roomToExpandIndex];
			roomToExpand.UpdateAdjacencyPossibilities();

			//Check room openings
			if (roomToExpand.PossibleConnections[(int)Directions.North])
			{
				//Debug.WriteLine("North");
				possibleExpansions.Add(new Point(-1, 0));
			}
			if (roomToExpand.PossibleConnections[(int)Directions.South])
			{
				//Debug.WriteLine("South");
				possibleExpansions.Add(new Point(1, 0));
			}
			if (roomToExpand.PossibleConnections[(int)Directions.East])
			{
				//Debug.WriteLine("East");
				possibleExpansions.Add(new Point(0, 1));
			}
			if (roomToExpand.PossibleConnections[(int)Directions.West])
			{
				//Debug.WriteLine("West");
				possibleExpansions.Add(new Point(0, -1));
			}

			//If able to expand, expand.
			if (possibleExpansions.Count != 0)
			{
				//Pick side to expand
				Point newRoomPos = possibleExpansions[
					_random.Next(possibleExpansions.Count)] + roomToExpand.MapPosition;
				//Debug.WriteLine("Old room " + roomToExpand.MapPosition);
				//Debug.WriteLine("New room " + newRoomPos);

				//Expand
				Map[newRoomPos.X, newRoomPos.Y] = new Room(newRoomPos, this);
				_rooms.Add(Map[newRoomPos.X, newRoomPos.Y]);
				roomToExpand.UpdateAdjacencyPossibilities();
				return;
			}

			//Hit dead end.
			//Remove dead end from resursion.
			roomsCopy.RemoveAt(roomToExpandIndex);

			ExpandLevel(roomsCopy);
			return;
		}

		/// <summary>
		/// Sets the furthest room with 1 connection from start
		/// room as boss room.
		/// </summary>
		private void DetermineBossRoom()
		{
			Room farthestRoom = StartRoom;
			float furthestDistance = 0;
			//Filter out rooms with only 1 connection.
			List<Room> roomsWithOneConnection = new List<Room>();
			foreach(Room room in _rooms)
			{
				if (room.NumberOfConnections == 1 && room.MapPosition != StartPoint)
				{
					roomsWithOneConnection.Add(room);
				}
			}
			
			//If no rooms have only 1 connection, then loop through all rooms.
			if (roomsWithOneConnection.Count == 0) 
			{
				//Debug.WriteLine("no 1 connection rooms");
				roomsWithOneConnection = _rooms;	
			}

			//Select boss room.
			Vector2 startPointVector2 = StartPoint.ToVector2();
			foreach (Room room in roomsWithOneConnection) 
			{
				Vector2 distanceVector = room.MapPosition.ToVector2() - startPointVector2;
                if (Math.Abs(distanceVector.Length()) > furthestDistance &&
					room != StartRoom)
				{
					farthestRoom = room;
					furthestDistance = Math.Abs(distanceVector.Length());
				}
			}
			farthestRoom.IsBossRoom = true;
			for (int i = 1; i < farthestRoom.Tileset.Layout.GetLength(0) - 1; i++)
			{
                for (int j = 1; j < farthestRoom.Tileset.Layout.GetLength(1)-1; j++)
				{
					farthestRoom.Tileset.Layout[i, j] = TileMaker.SetTile(TileType.Grass, farthestRoom.Tileset.Layout[i, j].WorldPosition);
				}

            }

			BossPoint = farthestRoom.MapPosition;
		}

		/// <summary>
		/// Loads new room.
		/// </summary>
		/// <param name="newRoomOffset">Offset that
		/// determines which room is loaded.</param>
		public void LoadRoomUsingOffset(Point newRoomOffset)
		{
			Game1.PManager.ClearPickups();
			CurrentPoint += newRoomOffset;

			//Remove enemies near player.
			CurrentRoom.RemoveEnemiesNearDoor(newRoomOffset);
			
			//Update minimap
			CurrentRoom.Entered = true;
			CurrentRoom.Discovered = true;

			for (int i = 0; i < 4; i++)
			{
				if (CurrentRoom.ActualConnections[i] != null)
				{
					CurrentRoom.ActualConnections[i].Discovered = true;
				}
			}

			if (!CurrentRoom.Cleared)
			{
				if (CurrentRoom.IsBossRoom)
					Game1.EManager.SpawnBoss(CurrentRoom);
				else
					Game1.EManager.CreateRoomEnemies(CurrentRoom);
			}
			
		}

		#region Debug methods
		/// <summary>
		/// Prints out Level Map to Debug.
		/// </summary>
		public void PrintLevel()
		{
			for (int row = 0; row < Map.GetLength(0); row++)
			{
				for (int col = 0;  col < Map.GetLength(1); col++)
				{
					PrintRoom(row, col);
				}
				Debug.WriteLine("");
			}

			Debug.WriteLine($"Start room {StartPoint}");
		}

		/// <summary>
		/// Prints type of room.
		/// </summary>
		/// <param name="row">Room row in map.</param>
		/// <param name="col">Romm col in map.</param>
		private void PrintRoom(int row, int col)
		{
			if (Map[row, col] == null)
			{
				Debug.Write("____ | ");
				return;
			}

			if (Map[row, col].IsBossRoom)
			{
				Debug.Write("BOSS | ");
				return;
			}

			Debug.Write("ROOM | ");
			return;
		}
		#endregion
		#endregion

	}
}
