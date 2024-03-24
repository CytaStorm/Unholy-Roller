using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Final_Game.Entity
{
    internal class Indicator
    {
        private Vector2 position;
        private BossState attackType;
        private double timeVisible;
        public Indicator(Vector2 position, BossState attackType) { 
            this.position = position;
            this.attackType = attackType;
            timeVisible = 1;
        }

    }
}
