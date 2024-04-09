using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
	internal class BulletTimeFX
	{
		#region Properties
		public Sprite Sprite { get; set; }
		public float EffectLife { get; set; }
		#endregion

		#region Constructors
		public BulletTimeFX(Sprite sprite, float effectLife, SpriteBatch sb)
		{
			Sprite = sprite;
			EffectLife = effectLife;
		}
		#endregion

		#region Methods
		public void Update()
		{

		}

		public void Draw()
		{
			//Sprite.Draw();
		}
		#endregion
	}
}
