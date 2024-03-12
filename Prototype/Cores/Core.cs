using Microsoft.Xna.Framework;
using Prototype.GameEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prototype.Cores
{
    internal class Core
    {
        // Properties
        public Vector2 Velocity { get; protected set; }
        public Vector2 Acceleration { get; protected set; }

        // Methods
        public virtual void Use(Player p, Vector2 pVelocity)
        {
            
        }

    }
}
