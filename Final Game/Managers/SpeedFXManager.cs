using Final_Game.Entity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Managers
{
	public class SpeedFXManager
	{
		#region Fields
		private Queue<BulletTimeFX> FXinstances = new Queue<BulletTimeFX>();
		private double _curFXTimer;
		private double _curDestructionTimer;
		#endregion             	
                         	
		#region Properties     	
		public double FXTimer {	 get; private set; } = 1f;
		#endregion             	

		#region Constructor(s)
		public SpeedFXManager(float fxTimer)
		{
			FXTimer = fxTimer;
			_curFXTimer = FXTimer;
		}
		#endregion

		#region Methods
		public void Update(SpriteBatch sb, GameTime gameTime)
		{
			//In bullet time,
			//create bullet fx based on timer
			if (Player.BulletTimeMultiplier < 1)
			{
				TickTimer(gameTime, _curFXTimer);
				if (_curFXTimer < 0)
				{
					_curFXTimer = FXTimer;
					FXinstances.Enqueue(new BulletTimeFX(Game1.Player.Image, sb, Color.White));
				}
				return;
			}
			
			//exit bullet time,
			//start destroying the fx trail every 0.5s
			if (FXinstances.Count > 0)
			{
				TickTimer(gameTime, _curDestructionTimer);	
				if (_curDestructionTimer < 0)
				{
					FXinstances.Dequeue();
					_curDestructionTimer = 0.5f;
				}
			}
		}

			

		private void TickTimer(GameTime gameTime, double timer)
		{
            if (_curFXTimer > 0)
            {
				_curFXTimer -= gameTime.ElapsedGameTime.TotalSeconds;
            }
		}
		#endregion
	}
}
