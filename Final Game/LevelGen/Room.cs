using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.LevelGen
{
	internal class Room
	{
		//Map made of rooms, like linked list.
		#region Fields
		public Room NorthRoom;
		public Room SouthRoom;
		public Room EastRoom;
		public Room WestRoom;

		/// <summary>
		/// Does the room have valid connections?
		/// </summary>
		public bool Closed { get
			{
				return NorthRoom == null &&
					SouthRoom == null &&
					EastRoom == null &&
					WestRoom == null;
			}
		}
		#endregion

		#region Constructor(s)
		public Room(Room northRoom, Room southRoom, Room eastRoom, Room westRoom)
		{
			NorthRoom = northRoom;
			SouthRoom = southRoom;
			EastRoom = eastRoom;
			WestRoom = westRoom;
		}
		#endregion
	}
}
