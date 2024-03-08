using Microsoft.Xna.Framework;

namespace Final_Game.LevelGen
{
	internal class Room
	{

		#region Fields
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
		#endregion

		#region Constructor(s)
		public Room(Point mapPosition)
		{
			MapPosition = mapPosition;
		}
		#endregion
	}
}
