﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
	public interface IGameObject
	{
		void Update(GameTime gameTime);

		void Draw(SpriteBatch spriteBatch, GameTime gameTime);
	}
}
