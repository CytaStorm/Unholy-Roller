using Final_Game.LevelGen;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;

namespace Final_Game
{
	internal class Map
	{
		#region Fields
		/// <summary>
		/// All rooms that make up the map.
		/// </summary>
		public static Room[,] Level;
		/// <summary>
		/// List of all rooms.
		/// </summary>
		private static List<Room> _rooms = new List<Room>();
		private static Random _random = new Random();
		#endregion

		#region Constructor(s)
		public Map(int height, int width, int size)
		{
			Level = new Room[height, width];

			//Create first room (randomly placed)
			Point firstRoom = 
				new Point(
					_random.Next(Level.GetLength(0)),
					_random.Next(Level.GetLength(1)));
			Level[firstRoom.X, firstRoom.Y] = new Room(
				new Point(firstRoom.X, firstRoom.Y));

			_rooms.Add(Level[firstRoom.X, firstRoom.Y]);

			//Expand rooms.
			for (int i = 1; i < size; i++)
			{
				PrintLevel();
				//Debug.WriteLine("");
				ExpandLevel(_rooms);
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

			//Makes it more likely to select previously added room to
			//expand upon.
			roomsCopy.Add(Room.Copy(roomsCopy.Last()));
			roomsCopy.Add(Room.Copy(roomsCopy.Last()));
			roomsCopy.Add(Room.Copy(roomsCopy.Last()));
			roomsCopy.Add(Room.Copy(roomsCopy.Last()));
			roomsCopy.Add(Room.Copy(roomsCopy.Last()));
			roomsCopy.Add(Room.Copy(roomsCopy.Last()));

			List<Point> possibleExpansions = new List<Point>();

			//Pick random room
			int roomToExpandIndex = _random.Next(roomsCopy.Count);

			//If picked the duplicate room added in line 59, decrement
			//to ensure that room can be selected from 
			//_rooms. This is because we need the to access
			//the original room's data, not the copy of it
			//in roomsCopy.
			if (roomToExpandIndex == roomsCopy.Count - 1) 
			{
				roomToExpandIndex--;
				roomToExpandIndex--;
				roomToExpandIndex--;
				roomToExpandIndex--;
				roomToExpandIndex--;
				roomToExpandIndex--;
			}
			//Debug.WriteLine("Possible rooms to expand: " + roomsCopy.Count);
			//Debug.WriteLine("Index of room to expand: " + roomToExpandIndex);

			Room roomToExpand = roomsCopy[roomToExpandIndex];
			roomToExpand.UpdateAdjacencies();
			
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

			//If hit dead end, recurse to find another room that
			//is not a dead end.
			if (possibleExpansions.Count == 0)
			{
				//Debug.WriteLine("Hit dead end.");
				//If the dead end room was the one that was just expanded,
				//which has duplicate(s).
				if(roomToExpandIndex == _rooms.Count)
				{

					//Remove dead end duplicates
					roomsCopy.RemoveAt(roomToExpandIndex);
					roomsCopy.RemoveAt(roomToExpandIndex);
					roomsCopy.RemoveAt(roomToExpandIndex);
					roomsCopy.RemoveAt(roomToExpandIndex);
					roomsCopy.RemoveAt(roomToExpandIndex);
					roomsCopy.RemoveAt(roomToExpandIndex);
				}
				//Remove dead end from resursion.
				roomsCopy.RemoveAt(roomToExpandIndex);

				ExpandLevel(roomsCopy);
				return;
			}

			//Pick side to expand
			Point newRoomPos = possibleExpansions[
				_random.Next(possibleExpansions.Count)] + roomToExpand.MapPosition;
			//Debug.WriteLine("Old room " + roomToExpand.MapPosition);
			//Debug.WriteLine("New room " + newRoomPos);

			//Expand
			Level[newRoomPos.X, newRoomPos.Y] = new Room(newRoomPos);
			_rooms.Add(Level[newRoomPos.X, newRoomPos.Y]);
			roomToExpand.UpdateAdjacencies();
		}

		//Prints out level to Debug.
		public void PrintLevel()
		{
			for (int row = 0; row < Level.GetLength(0);  row++)
			{
				for (int col = 0;  col < Level.GetLength(1); col++)
				{
					if (Level[row, col] != null)
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
