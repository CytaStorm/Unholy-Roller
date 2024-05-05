using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Managers
{
	public enum SoundFX
	{
		PinDamaged,
		PinKnockdown,
		PlayerDamaged,
		PlayerDeath
	}

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

		#region Properties

		public static bool SoundEffectsOn { get; set; } = true;

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
			AllSongs.Add(cm.Load<Song>("Music/Heaven_in_Hell"));
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
			float playerHealthPercent = (float)Game1.Player.CurHealth / Game1.Player.MaxHealth;

            PlayerDamageSoundEffects[0].Play(1f, playerHealthPercent, 0f);
		}
		public static void PlayDeathSound()
		{
			PlayerDamageSoundEffects[1].Play();
		}

		public static void PlaySFX(SoundFX soundToPlay)
		{
			if (!SoundEffectsOn) return;

            switch (soundToPlay)
            {
                case SoundFX.PinDamaged:
					PlayHitSound();
                    break;
                case SoundFX.PinKnockdown:
					PlayKnockSound();
                    break;
                case SoundFX.PlayerDamaged:
					PlayTakeDamageSound();
                    break;
                case SoundFX.PlayerDeath:
					PlayDeathSound();
                    break;
            }
        }

        public static void PlayBGM()
		{
			MediaPlayer.Play(curSong);
		}

		/// <summary>
		/// Changes song
		/// </summary>
		/// <param name="song">Index of song to play. 0 = battle, 1 = ambience, 2 = death </param>
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

		public static void ToggleSFX()
		{
			SoundEffectsOn = !SoundEffectsOn;
		}
        #endregion
    }
}
