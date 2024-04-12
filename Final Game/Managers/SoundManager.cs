using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
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

        private static Random _random = new Random();
        #endregion

        #region Methods
        public static void LoadSoundFiles(ContentManager cm)
        {
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
        #endregion
    }
}
