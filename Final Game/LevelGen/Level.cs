using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;

namespace Final_Game.LevelGen
{
	internal class Level
	{
		#region Fields
		// <summary>
		/// All rooms that make up the level.
		/// </summary>
		public static Room[,] Map;
		/// <summary>
		/// List of all rooms.
		/// </summary>
		private static List<Room> _rooms = new List<Room>();
		/// <summary>s
		/// Random to use in class.
		/// </summary>
		private static Random _random = new Random();

		/// <summary>
		/// Room to start in.
		/// </summary>
		public Room StartRoom;

		public Room CurrentRoom;
		#endregion

		#region Constructor(s)
		public Level(int height, int width, int size)
		{
			Map = new Room[height, width];

			//Create first room (randomly placed)
			Point startPoint = 
				new Point(
					_random.Next(Map.GetLength(0)),
					_random.Next(Map.GetLength(1)));
			Map[startPoint.X, startPoint.Y] = new Room(
				new Point(startPoint.X, startPoint.Y));

			_rooms.Add(Map[startPoint.X, startPoint.Y]);
			StartRoom = Map[startPoint.X, startPoint.X];
			CurrentRoom = StartRoom;

			//Expand rooms.
			for (int i = 1; i < size; i++)
			{
				PrintLevel();
				ExpandLevel(_rooms);
			}

			//Create connections.
			foreach (Room room in _rooms)
			{
				room.CreateConnections();
			}


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
			Math.Clamp(roomToExpandIndex, 0, roomsCopy.Count - 1);

			//If picked the duplicate room added in line 59, decrement
			//to ensure that room can be selected from 
			//_rooms. This is because we need the to access
			//the original room's data, not the copy of it
			//in roomsCopy.
			if (roomToExpandIndex == roomsCopy.Count - 1)
			{
				for (int i = 0; i < branchMultiplier; i++) roomToExpandIndex--;
			}
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

		//Prints out level to Debug.
		public void PrintLevel()
		{
			for (int row = 0; row < Map.GetLength(0);  row++)
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

			Debug.WriteLine("--------------------------------------------------" +
				"------------------------------------------------------");
		}

		#endregion

	}
}
