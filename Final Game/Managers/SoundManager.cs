using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Managers
{
	public class SoundManager
	{
		#region Fields
		public static List<SoundEffect> PinDamageSoundEffects = new List<SoundEffect>();
		public static List<SoundEffect> PinKnockSoundEffects = new List<SoundEffect>();
		public static List<SoundEffect> PlayerDamageSoundEffects = new List<SoundEffect>();
		public static List<SoundEffect> MiscSoundEffects = new List<SoundEffect>();

		private static Random _random = new Random();

		public static List<Song> AllSongs = new List<Song>();
		public static Song curSong;
        #endregion

        #region Methods
        public static void LoadSoundFiles(ContentManager cm)
		{
			//Player deal damage sound effects
			PinDamageSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pool_break-105353"));
			PinDamageSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/punchthump"));

			//Pin knock sound effects
			PinKnockSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinKnock1"));
			PinKnockSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinKnock2"));
			PinKnockSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinKnock3"));
			PinKnockSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinKnock4"));
			PinKnockSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinKnock5"));
			PinKnockSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinKnock6"));

			//Take damage sound
			PlayerDamageSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinPunch"));
			PlayerDamageSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/playerdeath"));

			//Misc sound effects
			MiscSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/healthpickupsound"));

			//Setup music
			AllSongs.Add(cm.Load<Song>("Music/Battle_Music"));
			AllSongs.Add(cm.Load<Song>("Music/Unholy_Ambience"));
			curSong = AllSongs[1];



			MediaPlayer.IsRepeating = true;
		}

		public static void PlayHitSound()
		{
			PinDamageSoundEffects[0].Play();
			PinDamageSoundEffects[1].Play();
		}

		public static void PlayKnockSound()
		{
			PinKnockSoundEffects[_random.Next(PinKnockSoundEffects.Count)].Play(0.1f, 0, 0);
		}

		public static void PlayTakeDamageSound()
		{
			PlayerDamageSoundEffects[0].Play();
		}
		public static void PlayDeathSound()
		{
			PlayerDamageSoundEffects[1].Play();
		}

		public static void PlayBGM()
		{
			MediaPlayer.Volume = 0.6f;
			MediaPlayer.Play(curSong);
		}

		/// <summary>
		/// Changes song
		/// </summary>
		/// <param name="song">Index of song to play. 0 = battle, 1 = ambience</param>
		public static void ChangeBGM(int song)
		{
			//Don't change is song is same
			if (curSong == AllSongs[song]) return;

			curSong = AllSongs[song];
			PlayBGM();
		}

		public static void PlayOutOfCombatSong()
		{
			//Debug.WriteLine("Play out of combat song");
			if (curSong == AllSongs[1]) return;
			curSong = AllSongs[1];
			PlayBGM();
		}

		public static void PlayHealthPickupSound()
		{
			MiscSoundEffects[0].Play(1f, 0f, 0f);
		}
		#endregion
	}
}
