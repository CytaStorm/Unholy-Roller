using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
	public interface IMovable
	{
		// Properties
		Vector2 WorldPosition { get; }
		Vector2 Velocity { get; }
		Vector2 Acceleration { get; }
		float Speed { get; }

		// Methods
		void Move(Vector2 distance);

	}
}
