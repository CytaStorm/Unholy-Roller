using Final_Game.LevelGen;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace Final_Game
{
	internal class Map
	{
		#region Fields
		/// <summary>
		/// All rooms that make up the map.
		/// </summary>
		public static Room[,] Level = new Room[5, 5];
		public static bool[,] availableRooms = new bool[Level.GetLength(0), Level.GetLength(1)];
		public static Random _random = new Random();
		/// <summary>
		/// List of all rooms.
		/// </summary>
		List<Point> rooms = new List<Point>();
		#endregion

		#region Constructor(s)
		public Map(int size)
		{
			//Create first room (randomly placed)
			Point firstRoom = 
				new Point(
					_random.Next(availableRooms.GetLength(0)),
					_random.Next(availableRooms.GetLength(1)));
			availableRooms[firstRoom.X, firstRoom.Y] = true;
			

		}
		#endregion

		#region Method(s)
		private void ExpandLevel()
		{
			Point selectedRoom = rooms[_random.Next(rooms.Count)];
			bool TopRow = selectedRoom.X == 0;
			bool BotRow = selectedRoom.X == availableRooms.GetLength(0);
			bool LeftCol = selectedRoom.Y == 0;
			bool RightCol = selectedRoom.Y == availableRooms.GetLength(0);

			if (!TopRow) { }

		}
		#endregion

	}
}
