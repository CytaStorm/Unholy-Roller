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
        public List<SoundEffect> PinDamageSoundEffects = new List<SoundEffect>();
        public List<SoundEffect> PinKnockSoundEffects = new List<SoundEffect>();

        private Random _random = new Random();
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
            PinDamageSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pool_break-105353"));
            PinDamageSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/punchthump"));


            //Pin knock sound effects
            PinKnockSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinKnock1"));
            PinKnockSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinKnock2"));
            PinKnockSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinKnock3"));
            PinKnockSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinKnock4"));
            PinKnockSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinKnock5"));
            PinKnockSoundEffects.Add(cm.Load<SoundEffect>("Sound Effects/pinKnock6"));
        }

        public void PlayHitSound()
        {
            PinDamageSoundEffects[0].Play();
            PinDamageSoundEffects[1].Play();
        }

        public void PlayKnockSound()
        {
            PinKnockSoundEffects[_random.Next(PinKnockSoundEffects.Count)].Play(0.1f, 0, 0);
        }
        #endregion
    }
}
