using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Final_Game.Entity;
using Microsoft.Xna.Framework;

namespace Final_Game
{
    public class IndicatorManager
    {
        private Game1 gm;

        private List<Indicator> Indicators { get; set; }

        public IndicatorManager(Game1 gm)
        {
            this.gm = gm;
            Indicators = new List<Indicator>();
            
        }
        public void Update(GameTime gameTime)
        {
            for(int i = 0; i< Indicators.Count; i++)
            {
                if (Indicators[i].Update(gameTime))
                {
                    Indicators.RemoveAt(i);
                }
            }
        }
        public void Clear()
        {
            Indicators.Clear();
        }
        public void Add(Indicator indicator)
        {
            Indicators.Add(indicator);
        }
        public void Draw()
        {
            for (int i = 0; i < Indicators.Count; i++)
            {
                Indicators[i].Draw();
            }

        }
    }
}
