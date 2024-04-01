using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game
{
	public class SoundManager
	{
		#region Fields
		public List<SoundEffect> SoundEffects = new List<SoundEffect>();
		#endregion

		#region Constructor(s)
		public SoundManager(ContentManager cm) 
		{
			LoadSoundFiles(cm);
		}
		#endregion

		#region Methods
		private void LoadSoundFiles(ContentManager cm)
		{
			SoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pool_break-105353"));
		}
		#endregion
	}
}
