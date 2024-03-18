using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;

namespace Final_Game.LevelGen
{
	public class Level
	{
		#region Fields
		/// <summary>
		/// All rooms that make up the level.
		/// </summary>
		public static Room[,] Map;

		/// <summary>
		/// List of all rooms.
		/// </summary>
		private static List<Room> _rooms = new List<Room>();

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
		/// Room player is in.
		/// </summary>
		public Room CurrentRoom { get { return Map[CurrentPoint.X, CurrentPoint.Y]; } }

		/// <summary>
		/// Room player started in.
		/// </summary>
		public Room StartRoom { get { return Map[StartPoint.X, StartPoint.Y]; } }
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
			Point StartPoint = 
				new Point(
					_random.Next(Map.GetLength(0)),
					_random.Next(Map.GetLength(1)));
			Map[StartPoint.X, StartPoint.Y] = new Room(
				new Point(StartPoint.X, StartPoint.Y));

			_rooms.Add(Map[StartPoint.X, StartPoint.Y]);
			CurrentPoint = StartPoint;

			//Expand rooms.
			for (int i = 1; i < size; i++)
			{
				//PrintLevel();
				ExpandLevel(_rooms);
			}

			//Create connections between rooms.
			foreach (Room room in _rooms)
			{
				room.CreateConnections();
			}

			Debug.WriteLine($"Start room [{StartPoint.Y}, {StartPoint.X}]");
			PrintLevel();
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
			if (roomToExpand.PossibleConnections["North"])
			{
				//Debug.WriteLine("North");
				possibleExpansions.Add(new Point(-1, 0));
			}
			if (roomToExpand.PossibleConnections["South"])
			{
				//Debug.WriteLine("South");
				possibleExpansions.Add(new Point(1, 0));
			}
			if (roomToExpand.PossibleConnections["East"])
			{
				//Debug.WriteLine("East");
				possibleExpansions.Add(new Point(0, 1));
			}
			if (roomToExpand.PossibleConnections["West"])
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
				Map[newRoomPos.X, newRoomPos.Y] = new Room(newRoomPos);
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
		/// Prints out Level Map to Debug.
		/// </summary>
		public void PrintLevel()
		{
			for (int row = 0; row < Map.GetLength(0); row++)
			{
				for (int col = 0;  col < Map.GetLength(1); col++)
				{
					if (Map[row, col] != null)
					{
						Debug.Write("ROOM | ");
						continue;
					}
					Debug.Write("____ | ");
				}
				Debug.WriteLine("");
			}

			//Debug.WriteLine("--------------------------------------------------" +
			//	"------------------------------------------------------");
		}
		#endregion

	}
}
