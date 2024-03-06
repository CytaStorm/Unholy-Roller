using Final_Game.LevelGen;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
	internal class Map
	{
		#region Fields
		/// <summary>
		/// All rooms that make up the map.
		/// </summary>
		Room[,] allRooms = new Room[5, 5];
		/// <summary>
		/// Which room the player is in.
		/// </summary>
		public Room playerCurrentRoom;
		private Random _random = new Random();
		#endregion

		#region Constructor(s)
		public Map(int size)
		{
			//Create first room
			allRooms[_random.Next(allRooms.GetLength(0)),
				_random.Next(allRooms.GetLength(1))] = 
				new Room();

			for (int i = 1; i < size; i++)
			{

			}


		}
		#endregion

		#region Method(s)
		#endregion

	}
}
